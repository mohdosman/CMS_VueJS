import { ref, reactive } from 'vue';
import { servicesApi } from '../api/servicesApi.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useSearchState } from '../../../common/composables/useSearchState.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { fieldMessages } from '../../../utils/formErrors.js';
import { announce } from '../../../services/liveAnnouncer.js';
import { formatDateTimeFull } from '../../../utils/formatters.js';

// Port of DisplayServiceFiles.aspx: the uploaded service files, their import counts, raw text and import errors.
export function useServiceFiles() {
    const { logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const files = ref([]);
    const totalRecords = ref(0);
    const providers = ref([]);
    const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 10 });

    // The screen opens blank: nothing is listed until the user searches (isDefault).
    const DEFAULT_CRITERIA = { providerId: null, dateFrom: '', dateTo: '', orderBy: 'createdOn', reverse: true, isDefault: true };
    const criteria = reactive({ ...DEFAULT_CRITERIA });

    const { load, save, clear: clearState } = useSearchState('serviceFileSearchJSON', criteria, DEFAULT_CRITERIA, paging);

    const hasSearched = ref(false);
    const isSearching = ref(false);
    const errors = ref({});

    // The raw text and the import errors open in dialogs.
    const dialog = ref('');          // '', 'raw' or 'errors'
    const current = ref(null);       // the file the dialog is about
    const rawText = ref('');
    const fileErrors = ref([]);
    const errorPaging = reactive({ currentPage: 1, maxPagesToShow: 5, pageSize: 20 });
    const errorCriteria = reactive({ orderBy: 'id', reverse: false });
    const errorTotal = ref(0);
    const isLoadingDialog = ref(false);

    // Each async load bumps its counter, so a slow earlier response can't overwrite a newer one.
    let requestSequence = 0;

    const msg = fieldMessages(errors);

    // Empty text boxes are sent as absent, and the screen's own flag is not a filter.
    const cleanCriteria = () => Object.fromEntries(
        Object.entries(criteria).filter(([k]) => k !== 'isDefault').map(([k, v]) => [k, v === '' ? null : v])
    );

    // ================================================================
    // Sorting, paging and search
    // ================================================================
    const setOrder = createSetOrder(criteria, paging, getFiles);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getFiles);

    async function getFiles() {
        // The default load shows no results until the user searches.
        if (criteria.isDefault) {
            return;
        }
        const requestId = ++requestSequence;
        errors.value = {};
        isSearching.value = true;
        try {
            const result = await servicesApi.searchFiles(paging.currentPage, paging.pageSize, cleanCriteria());
            if (requestId !== requestSequence) {
                return;
            }
            files.value = result.items;
            totalRecords.value = result.totalCount;
            hasSearched.value = true;
            announce(`${result.totalCount} files found`);
        } catch (e) {
            if (requestId === requestSequence) {
                const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
                if (fieldErrors) {
                    errors.value = fieldErrors;
                } else {
                    logApiError(e);
                }
            }
        } finally {
            if (requestId === requestSequence) {
                isSearching.value = false;
            }
        }
    }

    async function search() {
        if (isSearching.value) {
            return;
        }
        criteria.isDefault = false;
        paging.currentPage = 1;
        save();
        await getFiles();
    }

    // Nothing to choose from: preselect the only provider instead of making the user open the list.
    function defaultProvider() {
        if (providers.value.length === 1 && !criteria.providerId) {
            criteria.providerId = providers.value[0].id;
        }
    }

    // Runs each time the screen is shown: restore the saved criteria and, if the user had searched, list again.
    useActivate(async () => {
        load();
        try {
            providers.value = await servicesApi.fileProviders();
        } catch (e) {
            logApiError(e);
        }
        defaultProvider();
        await getFiles();
    });

    function clear() {
        requestSequence++;
        clearState();
        defaultProvider();
        isSearching.value = false;
        files.value = [];
        totalRecords.value = 0;
        hasSearched.value = false;
        errors.value = {};
    }

    // ================================================================
    // Dialogs: raw text and import errors
    // ================================================================
    async function loadErrors() {
        isLoadingDialog.value = true;
        try {
            const result = await servicesApi.fileErrors(current.value.id, errorPaging.currentPage, errorPaging.pageSize, errorCriteria);
            fileErrors.value = result.items;
            errorTotal.value = result.totalCount;
        } catch (e) {
            dialog.value = '';
            logApiError(e);
        } finally {
            isLoadingDialog.value = false;
        }
    }

    const setErrorOrder = createSetOrder(errorCriteria, errorPaging, loadErrors);
    const errorSortIcon = (col) => getSortIcon(col, errorCriteria);
    const { onPageChanged: onErrorPageChanged, onPageSizeChanged: onErrorPageSizeChanged } = createPagingHandlers(errorPaging, loadErrors);

    async function open(kind, file) {
        current.value = file;
        dialog.value = kind;
        if (kind === 'errors') {
            errorPaging.currentPage = 1;
            Object.assign(errorCriteria, { orderBy: 'id', reverse: false });
            await loadErrors();
            return;
        }
        isLoadingDialog.value = true;
        try {
            rawText.value = (await servicesApi.fileRaw(file.id)).content;
        } catch (e) {
            dialog.value = '';
            logApiError(e);
        } finally {
            isLoadingDialog.value = false;
        }
    }

    function closeDialog() {
        dialog.value = '';
        current.value = null;
    }

    return {
        // Results
        files, totalRecords, paging, criteria, providers, hasSearched,

        // Dialogs
        dialog, current, rawText, fileErrors, errorPaging, errorTotal, isLoadingDialog,

        // Busy and validation state
        isSearching, errors, msg,

        // Actions
        search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged,
        setErrorOrder, errorSortIcon, onErrorPageChanged, onErrorPageSizeChanged,
        open, closeDialog,

        // Helpers for the template
        formatDateTimeFull
    };
}
