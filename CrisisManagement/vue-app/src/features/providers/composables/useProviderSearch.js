import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { providersApi } from '../api/providersApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useSearchState } from '../../../common/composables/useSearchState.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { announce } from '../../../services/liveAnnouncer.js';

// Port of SearchProvider.aspx.
export function useProviderSearch() {
    const router = useRouter();
    const { logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const providers = ref([]);
    const totalRecords = ref(0);
    const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 20 });

    const DEFAULT_CRITERIA = { name: '', edisonNumber: '', npi: '', orderBy: 'name', reverse: false };
    const criteria = reactive({ ...DEFAULT_CRITERIA });

    const { load, save, clear: clearState } = useSearchState('providerSearchJSON', criteria, DEFAULT_CRITERIA, paging);

    const hasSearched = ref(false);
    const isSearching = ref(false);

    // Each async load bumps its counter, so a slow earlier response can't overwrite a newer one.
    let requestSequence = 0;

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canAdd = can('providers.edit');   // the server also refuses an add from a non-administrator

    // ================================================================
    // Sorting, paging and search
    // ================================================================
    const setOrder = createSetOrder(criteria, paging, getProviders);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getProviders);

    async function getProviders() {
        const requestId = ++requestSequence;
        isSearching.value = true;
        try {
            const result = await providersApi.search(paging.currentPage, paging.pageSize, criteria);
            if (requestId !== requestSequence) {
                return;
            }
            providers.value = result.items;
            totalRecords.value = result.totalCount;
            hasSearched.value = true;
            announce(`${result.totalCount} providers found`);
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

    async function search() {
        paging.currentPage = 1;
        save();
        await getProviders();
    }

    // Runs each time the screen is shown: restore the saved criteria and list again.
    useActivate(async () => {
        load();
        await getProviders();
    });

    // ================================================================
    // Navigation and clear
    // ================================================================
    function gotoProvider(provider) {
        router.push(`/admin/providers/${provider.providerId}`);
    }

    function add() {
        router.push('/admin/providers/0');
    }

    // Like WebForms, Clear goes back to the full list.
    async function clear() {
        clearState();
        await getProviders();
    }

    return {
        // Results
        providers, totalRecords, paging, criteria, hasSearched,

        // Busy state
        isSearching,

        // User and permissions
        canAdd,

        // Actions
        search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged,
        gotoProvider, add
    };
}
