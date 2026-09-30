import { ref, reactive, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { providersApi } from '../api/providersApi.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { announce } from '../../../services/liveAnnouncer.js';

const DEFAULT_CRITERIA = () => ({ name: '', abbreviation: '', edisonNumber: '', npi: '', orderBy: 'name', reverse: false });

// Module scope on purpose: filters and results survive search -> detail -> back within the SPA.
const criteria = reactive(DEFAULT_CRITERIA());
const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 20 });
const providers = ref([]);
const totalRecords = ref(0);
const hasSearched = ref(false);
const isSearching = ref(false);

export function useProviderSearch() {
    const router = useRouter();
    const { can } = useCapabilities();
    const { logApiError } = useLogger();

    async function getProviders() {
        isSearching.value = true;
        try {
            const result = await providersApi.search(paging.currentPage, paging.pageSize, criteria);
            providers.value = result.items;
            totalRecords.value = result.totalCount;
            hasSearched.value = true;
            announce(`${result.totalCount} providers found`);
        } catch (e) {
            logApiError(e);
        } finally {
            isSearching.value = false;
        }
    }

    const setOrder = createSetOrder(criteria, paging, getProviders);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getProviders);

    function search() {
        paging.currentPage = 1;
        return getProviders();
    }

    function clear() {
        Object.assign(criteria, DEFAULT_CRITERIA());
        return search();
    }

    const gotoProvider = (p) => router.push(`/admin/providers/${p.providerId}`);
    const canAdd = can('providers.edit');   // the server also refuses an add from a non-administrator
    const add = () => router.push('/admin/providers/0');

    // Coming back from a detail screen: refresh in place so edits and deletes show, keeping the page.
    onMounted(() => (hasSearched.value ? getProviders() : search()));

    return {
        criteria, paging, providers, totalRecords, isSearching,
        search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, gotoProvider, canAdd, add
    };
}
