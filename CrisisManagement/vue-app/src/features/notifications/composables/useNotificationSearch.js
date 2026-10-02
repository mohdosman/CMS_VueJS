import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { notificationsApi } from '../api/notificationsApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useSearchState } from '../../../common/composables/useSearchState.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { announce } from '../../../services/liveAnnouncer.js';
import { formatDateTimeFull } from '../../../utils/formatters.js';

// Port of SearchNotification.aspx.
export function useNotificationSearch() {
    const router = useRouter();
    const { logApiError, logSuccess } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const notifications = ref([]);
    const totalRecords = ref(0);
    const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 20 });

    // Newest first: the newest notification is the one the sign-in page shows.
    const DEFAULT_CRITERIA = { orderBy: 'createdOn', reverse: true };
    const criteria = reactive({ ...DEFAULT_CRITERIA });

    const { load, save } = useSearchState('notificationSearchJSON', criteria, DEFAULT_CRITERIA, paging);

    const isSearching = ref(false);
    const confirming = ref(null);   // the notification awaiting delete confirmation

    // Each async load bumps its counter, so a slow earlier response can't overwrite a newer one.
    let requestSequence = 0;

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canEdit = can('notifications.edit');

    // ================================================================
    // Sorting, paging and search
    // ================================================================
    const setOrder = createSetOrder(criteria, paging, getNotifications);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getNotifications);

    async function getNotifications() {
        const requestId = ++requestSequence;
        isSearching.value = true;
        try {
            const result = await notificationsApi.search(paging.currentPage, paging.pageSize, criteria);
            if (requestId !== requestSequence) {
                return;
            }
            notifications.value = result.items;
            totalRecords.value = result.totalCount;
            announce(`${result.totalCount} notifications found`);
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
        await getNotifications();
    }

    // Runs each time the screen is shown: restore the saved sort and list again.
    useActivate(async () => {
        load();
        await getNotifications();
    });

    // ================================================================
    // Navigation
    // ================================================================
    function gotoNotification(notification) {
        router.push(`/notifications/${notification.id}`);
    }

    function add() {
        router.push('/notifications/0');
    }

    // ================================================================
    // Delete
    // ================================================================
    async function remove() {
        const notification = confirming.value;
        try {
            await notificationsApi.remove(notification.id);
            logSuccess(`Notification '${notification.id}' deleted.`);
            confirming.value = null;
            await getNotifications();
        } catch (e) {
            confirming.value = null;
            logApiError(e);
        }
    }

    return {
        // Results
        notifications, totalRecords, paging, criteria,

        // Busy state
        isSearching, confirming,

        // User and permissions
        canEdit,

        // Actions
        search, setOrder, sortIcon, onPageChanged, onPageSizeChanged,
        gotoNotification, add, remove,

        // Helpers for the template
        formatDateTimeFull
    };
}
