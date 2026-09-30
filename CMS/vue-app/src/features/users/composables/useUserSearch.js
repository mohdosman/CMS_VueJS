import { ref, reactive, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { usersApi } from '../api/usersApi.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { can } from '../../../boot.js';
import { announce } from '../../../services/liveAnnouncer.js';

// YesNoFilter on the server: 0 = All, 1 = Yes, 2 = No.
const DEFAULT_CRITERIA = () => ({
    userName: '', firstName: '', lastName: '', email: '',
    roleIds: [], providerIds: [],
    isEnabled: 1, isADAccount: 0, isLockedOut: 0,
    orderBy: 'userName', reverse: false
});

// Module scope on purpose: filters and results survive search -> detail -> back
// within the SPA. A page reload starts fresh.
const criteria = reactive(DEFAULT_CRITERIA());
const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 20 });
const users = ref([]);
const totalRecords = ref(0);
const roles = ref([]);
const providers = ref([]);
const hasSearched = ref(false);
const isSearching = ref(false);
const error = ref('');

async function getUsers() {
    isSearching.value = true;
    error.value = '';
    try {
        const result = await usersApi.search(paging.currentPage, paging.pageSize, criteria);
        users.value = result.items;
        totalRecords.value = result.totalCount;
        hasSearched.value = true;
        announce(`${result.totalCount} users found`);
    } catch (e) {
        error.value = e.message;
    } finally {
        isSearching.value = false;
    }
}

export function useUserSearch() {
    const router = useRouter();

    const setOrder = createSetOrder(criteria, paging, getUsers);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getUsers);

    function search() {
        paging.currentPage = 1;
        return getUsers();
    }

    function clear() {
        Object.assign(criteria, DEFAULT_CRITERIA());
        return search();
    }

    const gotoUser = (u) => router.push(`/admin/users/${u.userKey}`);
    const canAdd = can('users.edit');
    const add = () => router.push('/admin/users/0');

    onMounted(async () => {
        // Lookups are scoped server-side to what this user may see.
        if (!roles.value.length) {
            try {
                [roles.value, providers.value] = await Promise.all([usersApi.roles(), usersApi.providers()]);
            } catch (e) {
                error.value = e.message;
            }
        }
        // Coming back from a detail screen: refresh in place so edits and deletes show, keeping the page.
        await (hasSearched.value ? getUsers() : search());
    });

    return {
        criteria, paging, users, totalRecords, roles, providers, isSearching, error,
        search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, gotoUser, canAdd, add
    };
}
