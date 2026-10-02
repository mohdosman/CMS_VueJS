import { ref, reactive, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { notificationsApi } from '../api/notificationsApi.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { announce } from '../../../services/liveAnnouncer.js';

// Newest first by default: the newest one is what the sign-in page shows.
const DEFAULT_CRITERIA = () => ({ orderBy: 'createdOn', reverse: true });

// Module scope on purpose: the sort and page survive list -> detail -> back within the SPA.
const criteria = reactive(DEFAULT_CRITERIA());
const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 20 });
const notifications = ref([]);
const totalRecords = ref(0);
const hasSearched = ref(false);
const isSearching = ref(false);
const confirming = ref(null);   // the notification awaiting delete confirmation

export function useNotificationSearch() {
    const router = useRouter();
    const { can } = useCapabilities();
    const { logApiError, logSuccess } = useLogger();

    async function getNotifications() {
        isSearching.value = true;
        try {
            const result = await notificationsApi.search(paging.currentPage, paging.pageSize, criteria);
            notifications.value = result.items;
            totalRecords.value = result.totalCount;
            hasSearched.value = true;
            announce(`${result.totalCount} notifications found`);
        } catch (e) {
            logApiError(e);
        } finally {
            isSearching.value = false;
        }
    }

    const setOrder = createSetOrder(criteria, paging, getNotifications);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getNotifications);

    function search() {
        paging.currentPage = 1;
        return getNotifications();
    }

    async function remove() {
        const n = confirming.value;
        try {
            await notificationsApi.remove(n.id);
            logSuccess(`Notification '${n.id}' deleted.`);
            confirming.value = null;
            await getNotifications();
        } catch (e) {
            confirming.value = null;
            logApiError(e);
        }
    }

    const gotoNotification = (n) => router.push(`/notifications/${n.id}`);
    const canEdit = can('notifications.edit');
    const add = () => router.push('/notifications/0');

    // Coming back from a detail screen: refresh in place so edits and deletes show, keeping the page.
    onMounted(() => (hasSearched.value ? getNotifications() : search()));

    return {
        criteria, paging, notifications, totalRecords, isSearching, confirming,
        search, remove, setOrder, sortIcon, onPageChanged, onPageSizeChanged, gotoNotification, canEdit, add
    };
}
