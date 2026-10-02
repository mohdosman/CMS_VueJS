import { ref, reactive } from 'vue';
import { suicidesApi } from '../api/suicidesApi.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useSearchState } from '../../../common/composables/useSearchState.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { fieldMessages } from '../../../utils/formErrors.js';
import { announce } from '../../../services/liveAnnouncer.js';

// Port of DisplaySuicideFiles.aspx: the uploaded suicide (death record) files, with a dialog for the records in each.
export function useSuicideFiles() {
    const { logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const files = ref([]);
    const totalRecords = ref(0);
    const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 10 });

    // The screen opens blank: nothing is listed until the user searches (isDefault).
    const DEFAULT_CRITERIA = { fileName: '', dateFrom: '', dateTo: '', orderBy: 'createdOn', reverse: true, isDefault: true };
    const criteria = reactive({ ...DEFAULT_CRITERIA });

    const { load, save, clear: clearState } = useSearchState('suicideFileSearchJSON', criteria, DEFAULT_CRITERIA, paging);

    const hasSearched = ref(false);
    const isSearching = ref(false);
    const errors = ref({});

    // The records of a file open in a dialog, paged and sortable.
    const current = ref(null);
    const records = ref([]);
    const recordTotal = ref(0);
    const recordPaging = reactive({ currentPage: 1, maxPagesToShow: 5, pageSize: 10 });
    const recordSort = reactive({ orderBy: '', reverse: false });
    const isLoadingRecords = ref(false);

    // Each async load bumps its counter, so a slow earlier response can't overwrite a newer one.
    let requestSequence = 0;

    const msg = fieldMessages(errors);
    const downloadUrl = suicidesApi.downloadUrl;

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
            const result = await suicidesApi.searchFiles(paging.currentPage, paging.pageSize, cleanCriteria());
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

    // Runs each time the screen is shown: restore the saved criteria and, if the user had searched, list again.
    useActivate(async () => {
        load();
        await getFiles();
    });

    function clear() {
        requestSequence++;
        clearState();
        isSearching.value = false;
        files.value = [];
        totalRecords.value = 0;
        hasSearched.value = false;
        errors.value = {};
    }

    // ================================================================
    // Records dialog
    // ================================================================
    async function loadRecords() {
        isLoadingRecords.value = true;
        try {
            const result = await suicidesApi.imports(
                current.value.id, recordPaging.currentPage, recordPaging.pageSize, recordSort.orderBy || null, recordSort.reverse
            );
            records.value = result.items;
            recordTotal.value = result.totalCount;
        } catch (e) {
            current.value = null;
            logApiError(e);
        } finally {
            isLoadingRecords.value = false;
        }
    }

    const setRecordOrder = createSetOrder(recordSort, recordPaging, loadRecords);
    const recordSortIcon = (col) => getSortIcon(col, recordSort);
    const { onPageChanged: onRecordPageChanged, onPageSizeChanged: onRecordPageSizeChanged } = createPagingHandlers(recordPaging, loadRecords);

    async function openRecords(file) {
        current.value = file;
        recordPaging.currentPage = 1;
        Object.assign(recordSort, { orderBy: '', reverse: false });
        await loadRecords();
    }

    function closeRecords() {
        current.value = null;
    }

    return {
        // Results
        files, totalRecords, paging, criteria, hasSearched,

        // Records dialog
        current, records, recordTotal, recordPaging, isLoadingRecords,

        // Busy and validation state
        isSearching, errors, msg,

        // Actions
        search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged,
        openRecords, closeRecords, setRecordOrder, recordSortIcon, onRecordPageChanged, onRecordPageSizeChanged,

        // Helpers for the template
        downloadUrl
    };
}
