import { ref, reactive, computed } from 'vue';
import { useRouter } from 'vue-router';
import { usersApi } from '../api/usersApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useSearchState } from '../../../common/composables/useSearchState.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { announce } from '../../../services/liveAnnouncer.js';

// User search: the list of users, filtered by name, roles, providers and account flags.
export function useUserSearch() {
    const router = useRouter();
    const { logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const users = ref([]);
    const totalRecords = ref(0);
    const roles = ref([]);
    const providers = ref([]);
    const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 20 });

    // YesNoFilter on the server: 0 = All, 1 = Yes, 2 = No.
    const DEFAULT_CRITERIA = {
        userName: '', firstName: '', lastName: '', email: '',
        roleIds: [], providerIds: [],
        isEnabled: 1, isADAccount: 0, isLockedOut: 0,
        orderBy: 'userName', reverse: false
    };
    const criteria = reactive(structuredClone(DEFAULT_CRITERIA));

    const { load, save, clear: clearState } = useSearchState('userSearchJSON', criteria, DEFAULT_CRITERIA, paging);

    const isSearching = ref(false);

    // The choices of the Active, AD and Locked filters, and the filters themselves.
    const yesNo = [{ v: 0, t: 'All' }, { v: 1, t: 'Yes' }, { v: 2, t: 'No' }];
    const flags = [
        { key: 'isEnabled', label: 'Active' },
        { key: 'isADAccount', label: 'AD' },
        { key: 'isLockedOut', label: 'Locked' }
    ];

    // Each async load bumps its counter, so a slow earlier response can't overwrite a newer one.
    let requestSequence = 0;

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canEdit = can('users.edit');
    const searched = ref(false);
    // Add is offered only after a search that found nobody, as in the Blazor app.
    const canAdd = computed(() => canEdit && searched.value && !isSearching.value && totalRecords.value === 0);

    // ================================================================
    // Sorting, paging and search
    // ================================================================
    const setOrder = createSetOrder(criteria, paging, getUsers);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getUsers);

    async function getUsers() {
        const requestId = ++requestSequence;
        isSearching.value = true;
        try {
            const result = await usersApi.search(paging.currentPage, paging.pageSize, criteria);
            if (requestId !== requestSequence) {
                return;
            }
            users.value = result.items;
            totalRecords.value = result.totalCount;
            searched.value = true;
            announce(`${result.totalCount} users found`);
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
        await getUsers();
    }

    // Lookups are scoped server-side to what this user may see.
    async function getLookups() {
        try {
            [roles.value, providers.value] = await Promise.all([usersApi.roles(), usersApi.providers()]);
        } catch (e) {
            logApiError(e);
        }
    }

    // With a single provider to choose from, preselect it rather than make the user open the dropdown.
    function applyDefaultProvider() {
        if (providers.value.length === 1 && !criteria.providerIds.length) {
            criteria.providerIds = [providers.value[0].id];
        }
    }

    // Runs each time the screen is shown: restore the saved criteria and list again.
    useActivate(async () => {
        load();
        await getLookups();
        applyDefaultProvider();
        await getUsers();
    });

    // ================================================================
    // Navigation and clear
    // ================================================================
    function gotoUser(user) {
        router.push(`/admin/users/${user.userKey}`);
    }

    function add() {
        router.push('/admin/users/0');
    }

    async function clear() {
        clearState();
        applyDefaultProvider();
        await getUsers();
    }

    return {
        // Results
        users, totalRecords, paging, criteria, roles, providers, yesNo, flags,

        // Busy state
        isSearching,

        // User and permissions
        canAdd,

        // Actions
        search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged,
        gotoUser, add
    };
}
