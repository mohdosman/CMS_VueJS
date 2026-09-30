import { ref, reactive, onMounted } from 'vue';
import { publicFilesAdminApi } from '../api/publicFilesAdminApi.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { announce } from '../../../services/liveAnnouncer.js';
import { apiErrorMessage } from '../../../utils/apiError.js';

const MAX_BYTES = 5 * 1024 * 1024;   // the server enforces the same limit

// Newest first, as in the Blazor CMS.
const criteria = reactive({ orderBy: 'createdOn', reverse: true });
const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 20 });
const files = ref([]);
const totalRecords = ref(0);
const isSearching = ref(false);

export function usePublicFiles() {
    const { can } = useCapabilities();
    const { logApiError, logSuccess } = useLogger();

    const canEdit = can('publicfiles.edit');
    const uploadError = ref('');
    const isUploading = ref(false);
    const confirming = ref(null);   // the file awaiting delete confirmation

    async function getFiles() {
        isSearching.value = true;
        try {
            const result = await publicFilesAdminApi.search(paging.currentPage, paging.pageSize, criteria);
            files.value = result.items;
            totalRecords.value = result.totalCount;
            announce(`${result.totalCount} public files found`);
        } catch (e) {
            logApiError(e);
        } finally {
            isSearching.value = false;
        }
    }

    const setOrder = createSetOrder(criteria, paging, getFiles);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getFiles);

    // The file uploads as soon as it is chosen, like the Blazor CMS screen.
    async function onPick(e) {
        const file = e.target.files[0];
        e.target.value = '';
        uploadError.value = '';
        if (!file) return;
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
            uploadError.value = fieldErrors?.join(' ') ?? apiErrorMessage(err);
        } finally {
            isUploading.value = false;
        }
    }

    async function remove() {
        const f = confirming.value;
        try {
            await publicFilesAdminApi.remove(f.id);
            logSuccess(`${f.fileName} deleted.`);
            confirming.value = null;
            await getFiles();
        } catch (e) {
            confirming.value = null;
            logApiError(e);
        }
    }

    onMounted(getFiles);

    return {
        paging, files, totalRecords, isSearching, canEdit, uploadError, isUploading, confirming,
        setOrder, sortIcon, onPageChanged, onPageSizeChanged, onPick, remove
    };
}
