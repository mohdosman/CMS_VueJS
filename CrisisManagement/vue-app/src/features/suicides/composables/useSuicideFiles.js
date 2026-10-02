import { ref, reactive, onMounted } from 'vue';
import { suicidesApi } from '../api/suicidesApi.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { fieldMessages } from '../../../utils/formErrors.js';
import { announce } from '../../../services/liveAnnouncer.js';

const DEFAULT_CRITERIA = () => ({ fileName: '', dateFrom: '', dateTo: '', orderBy: 'createdOn', reverse: true });

// Module scope on purpose: the filters and results survive opening a file and coming back.
const criteria = reactive(DEFAULT_CRITERIA());
const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 10 });
const files = ref([]);
const totalRecords = ref(0);
const hasSearched = ref(false);
const isSearching = ref(false);
const errors = ref({});

const clean = () => Object.fromEntries(Object.entries(criteria).map(([k, v]) => [k, v === '' ? null : v]));

export function useSuicideFiles() {
    const { logApiError } = useLogger();

    // The records of a file open in a dialog, paged and sortable.
    const current = ref(null);
    const records = ref([]);
    const recordTotal = ref(0);
    const recordPaging = reactive({ currentPage: 1, maxPagesToShow: 5, pageSize: 10 });
    const recordSort = reactive({ orderBy: '', reverse: false });
    const isLoadingRecords = ref(false);

    async function getFiles() {
        errors.value = {};
        isSearching.value = true;
        try {
            const result = await suicidesApi.searchFiles(paging.currentPage, paging.pageSize, clean());
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

    function clear() {
        Object.assign(criteria, DEFAULT_CRITERIA());
        files.value = [];
        totalRecords.value = 0;
        hasSearched.value = false;
        errors.value = {};
    }

    async function loadRecords() {
        isLoadingRecords.value = true;
        try {
            const r = await suicidesApi.imports(current.value.id, recordPaging.currentPage, recordPaging.pageSize, recordSort.orderBy || null, recordSort.reverse);
            records.value = r.items;
            recordTotal.value = r.totalCount;
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

    function openRecords(file) {
        current.value = file;
        recordPaging.currentPage = 1;
        Object.assign(recordSort, { orderBy: '', reverse: false });
        return loadRecords();
    }

    const closeRecords = () => { current.value = null; };

    onMounted(() => hasSearched.value && getFiles());

    const msg = fieldMessages(errors);

    return {
        msg, criteria, paging, files, totalRecords, hasSearched, isSearching, errors, current, records, recordTotal, recordPaging, isLoadingRecords,
        search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged,
        openRecords, closeRecords, setRecordOrder, recordSortIcon, onRecordPageChanged, onRecordPageSizeChanged,
        downloadUrl: suicidesApi.downloadUrl
    };
}
