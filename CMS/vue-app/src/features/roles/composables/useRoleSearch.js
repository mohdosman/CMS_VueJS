import { ref, reactive, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { rolesApi } from '../api/rolesApi.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { announce } from '../../../services/liveAnnouncer.js';

const DEFAULT_CRITERIA = () => ({ name: '', orderBy: 'name', reverse: false });

// Module scope on purpose: filters and results survive search -> detail -> back
// within the SPA. A page reload starts fresh.
const criteria = reactive(DEFAULT_CRITERIA());
const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 20 });
const roles = ref([]);
const totalRecords = ref(0);
const hasSearched = ref(false);
const isSearching = ref(false);

export function useRoleSearch() {
    const router = useRouter();
    const { can } = useCapabilities();
    const { logApiError } = useLogger();

    async function getRoles() {
        isSearching.value = true;
        try {
            const result = await rolesApi.search(paging.currentPage, paging.pageSize, criteria);
            roles.value = result.items;
            totalRecords.value = result.totalCount;
            hasSearched.value = true;
            announce(`${result.totalCount} roles found`);
        } catch (e) {
            logApiError(e);
        } finally {
            isSearching.value = false;
        }
    }

    const setOrder = createSetOrder(criteria, paging, getRoles);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getRoles);

    function search() {
        paging.currentPage = 1;
        return getRoles();
    }

    function clear() {
        Object.assign(criteria, DEFAULT_CRITERIA());
        return search();
    }

    const gotoRole = (r) => router.push(`/admin/roles/${r.id}`);
    const canAdd = can('roles.edit');
    const add = () => router.push('/admin/roles/0');

    // Coming back from a detail screen: refresh in place so edits and deletes show, keeping the page.
    onMounted(() => (hasSearched.value ? getRoles() : search()));

    return {
        criteria, paging, roles, totalRecords, isSearching,
        search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, gotoRole, canAdd, add
    };
}
