import { ref, reactive } from 'vue';
import { publicFilesAdminApi } from '../api/publicFilesAdminApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { apiErrorMessage } from '../../../utils/apiError.js';
import { announce } from '../../../services/liveAnnouncer.js';
import { publicFilesApi } from '../../../common/api/publicFilesApi.js';
import { formatDateTimeFull, formatFileSize } from '../../../utils/formatters.js';

// Port of ManagePublicFiles.aspx: upload a public (help) PDF and list, download and delete the uploaded ones.
export function usePublicFiles() {
    const { logApiError, logSuccess } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const files = ref([]);
    const totalRecords = ref(0);
    const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 20 });

    // Newest first, as in the Blazor CMS.
    const criteria = reactive({ orderBy: 'createdOn', reverse: true });

    const isSearching = ref(false);
    const uploadError = ref('');
    const isUploading = ref(false);
    const confirming = ref(null);   // the file awaiting delete confirmation

    const MAX_BYTES = 5 * 1024 * 1024;   // the server enforces the same limit

    // Each async load bumps its counter, so a slow earlier response can't overwrite a newer one.
    let requestSequence = 0;

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canEdit = can('publicfiles.edit');

    // ================================================================
    // Sorting, paging and loading
    // ================================================================
    const setOrder = createSetOrder(criteria, paging, getFiles);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getFiles);

    async function getFiles() {
        const requestId = ++requestSequence;
        isSearching.value = true;
        try {
            const result = await publicFilesAdminApi.search(paging.currentPage, paging.pageSize, criteria);
            if (requestId !== requestSequence) {
                return;
            }
            files.value = result.items;
            totalRecords.value = result.totalCount;
            announce(`${result.totalCount} public files found`);
        } catch (e) {
            if (requestId === requestSequence) {
                logApiError(e);
            }
        } finally {
            if (requestId === requestSequence) {
                isSearching.value = false;
            }
        }
    }

    // Runs each time the screen is shown.
    useActivate(async () => {
        await getFiles();
    });

    // ================================================================
    // Upload and delete
    // ================================================================
    // The file uploads as soon as it is chosen, like the Blazor CMS screen.
    async function onPick(e) {
        const file = e.target.files[0];
        e.target.value = '';
        uploadError.value = '';
        if (!file) {
            return;
        }
        if (file.size > MAX_BYTES) {
            uploadError.value = `The file exceeds the ${MAX_BYTES / (1024 * 1024)} MB limit.`;
            return;
        }
        isUploading.value = true;
        try {
            await publicFilesAdminApi.upload(file);
            logSuccess(`${file.name} uploaded.`);
            paging.currentPage = 1;
            await getFiles();
        } catch (err) {
            const fieldErrors = err.response?.status === 400 ? err.response.data?.errors?.file : null;
            if (!fieldErrors) {
                logApiError(err);
            }
            uploadError.value = fieldErrors?.join(' ') ?? apiErrorMessage(err);
        } finally {
            isUploading.value = false;
        }
    }

    async function remove() {
        const file = confirming.value;
        try {
            await publicFilesAdminApi.remove(file.id);
            logSuccess(`${file.fileName} deleted.`);
            confirming.value = null;
            await getFiles();
        } catch (e) {
            confirming.value = null;
            logApiError(e);
        }
    }

    return {
        // Results
        files, totalRecords, paging,

        // Busy and validation state
        isSearching, isUploading, uploadError, confirming,

        // User and permissions
        canEdit,

        // Actions
        setOrder, sortIcon, onPageChanged, onPageSizeChanged, onPick, remove,

        // Helpers for the template
        formatDateTimeFull, formatFileSize, downloadUrl: publicFilesApi.downloadUrl
    };
}
