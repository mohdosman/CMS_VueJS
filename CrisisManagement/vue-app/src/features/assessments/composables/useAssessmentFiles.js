import { ref, reactive, onMounted } from 'vue';
import { assessmentsApi } from '../api/assessmentsApi.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { fieldMessages } from '../../../utils/formErrors.js';
import { announce } from '../../../services/liveAnnouncer.js';

const DEFAULT_CRITERIA = () => ({ providerId: null, dateFrom: '', dateTo: '', orderBy: 'createdOn', reverse: true });

// Module scope on purpose: the filters and results survive opening a file and coming back.
const criteria = reactive(DEFAULT_CRITERIA());
const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 10 });
const files = ref([]);
const totalRecords = ref(0);
const providers = ref([]);
const hasSearched = ref(false);
const isSearching = ref(false);
const errors = ref({});

const clean = () => Object.fromEntries(Object.entries(criteria).map(([k, v]) => [k, v === '' ? null : v]));

export function useAssessmentFiles() {
    const { logApiError } = useLogger();

    // Raw file text and import errors open in dialogs.
    const dialog = ref('');          // '', 'raw' or 'errors'
    const current = ref(null);       // the file the dialog is about
    const rawXml = ref('');
    const fileErrors = ref([]);
    const isLoadingDialog = ref(false);

    async function getFiles() {
        errors.value = {};
        isSearching.value = true;
        try {
            const result = await assessmentsApi.searchFiles(paging.currentPage, paging.pageSize, clean());
            files.value = result.items;
            totalRecords.value = result.totalCount;
            hasSearched.value = true;
            announce(`${result.totalCount} files found`);
        } catch (e) {
            const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
            if (fieldErrors) errors.value = fieldErrors;
            else logApiError(e);
        } finally {
            isSearching.value = false;
        }
    }

    const setOrder = createSetOrder(criteria, paging, getFiles);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getFiles);

    function search() {
        paging.currentPage = 1;
        return getFiles();
    }

    function defaultProvider() {
        if (providers.value.length === 1 && !criteria.providerId) criteria.providerId = providers.value[0].id;
    }

    function clear() {
        Object.assign(criteria, DEFAULT_CRITERIA());
        defaultProvider();
        files.value = [];
        totalRecords.value = 0;
        hasSearched.value = false;
        errors.value = {};
    }

    async function open(kind, file) {
        current.value = file;
        dialog.value = kind;
        isLoadingDialog.value = true;
        try {
            if (kind === 'raw') rawXml.value = (await assessmentsApi.fileRaw(file.id)).xml;
            else fileErrors.value = await assessmentsApi.fileErrors(file.id);
        } catch (e) {
            dialog.value = '';
            logApiError(e);
        } finally {
            isLoadingDialog.value = false;
        }
    }

    const closeDialog = () => { dialog.value = ''; current.value = null; };

    onMounted(async () => {
        if (!providers.value.length) {
            try {
                providers.value = await assessmentsApi.fileProviders();
            } catch (e) {
                logApiError(e);
            }
        }
        defaultProvider();
        if (hasSearched.value) await getFiles();
    });

    const msg = fieldMessages(errors);

    return {
        msg, criteria, paging, files, totalRecords, providers, hasSearched, isSearching, errors, dialog, current, rawXml, fileErrors, isLoadingDialog,
        search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, open, closeDialog
    };
}
