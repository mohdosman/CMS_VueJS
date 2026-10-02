import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { rolesApi } from '../api/rolesApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useSearchState } from '../../../common/composables/useSearchState.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { announce } from '../../../services/liveAnnouncer.js';

// Role search: the list of roles, filtered by name.
export function useRoleSearch() {
    const router = useRouter();
    const { logApiError, logError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const roles = ref([]);
    const totalRecords = ref(0);
    const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 20 });

    const DEFAULT_CRITERIA = { name: '', orderBy: 'name', reverse: false };
    const criteria = reactive({ ...DEFAULT_CRITERIA });

    const { load, save, clear: clearState } = useSearchState('roleSearchJSON', criteria, DEFAULT_CRITERIA, paging);

    const isSearching = ref(false);

    // Each async load bumps its counter, so a slow earlier response can't overwrite a newer one.
    let requestSequence = 0;

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canAdd = can('roles.edit');

    // ================================================================
    // Sorting, paging and search
    // ================================================================
    const setOrder = createSetOrder(criteria, paging, getRoles);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getRoles);

    async function getRoles() {
        const requestId = ++requestSequence;
        isSearching.value = true;
        try {
            const result = await rolesApi.search(paging.currentPage, paging.pageSize, criteria);
            if (requestId !== requestSequence) {
                return;
            }
            roles.value = result.items;
            totalRecords.value = result.totalCount;
            announce(`${result.totalCount} roles found`);
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
        await getRoles();
    }

    // Runs each time the screen is shown: restore the saved criteria and list again.
    useActivate(async () => {
        load();
        await getRoles();
    });

    // ================================================================
    // Navigation and clear
    // ================================================================
    // Set while a navigation is under way, so a double click opens the screen once and Add/Clear wait.
    const isNavigating = ref(false);

    async function open(path, failure) {
        if (isNavigating.value) {
            return;
        }
        isNavigating.value = true;
        try {
            await router.push(path);
        } catch {
            logError(failure);
        } finally {
            isNavigating.value = false;
        }
    }

    const gotoRole = (role) => open(`/admin/roles/${role.id}`, 'Failed to open role details.');
    const add = () => open('/admin/roles/0', 'Failed to open role form.');

    async function clear() {
        clearState();
        await getRoles();
    }

    return {
        // Results
        roles, totalRecords, paging, criteria,

        // Busy state
        isSearching, isNavigating,

        // User and permissions
        canAdd,

        // Actions
        search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged,
        gotoRole, add
    };
}
