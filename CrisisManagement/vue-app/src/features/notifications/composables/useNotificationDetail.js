import { ref, reactive, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { notificationsApi } from '../api/notificationsApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { apiErrorMessage } from '../../../utils/apiError.js';

// One form for both create (/notifications/0) and edit (/notifications/:key, the notification id).
export function useNotificationDetail() {
    const route = useRoute();
    const router = useRouter();
    const { can } = useCapabilities();
    const { logSuccess, logApiError } = useLogger();

    const isNew = route.params.key === '0';
    const canEdit = can('notifications.edit');

    const form = reactive({ rowVersion: null, notification: '' });
    const notificationId = ref(0);
    const info = ref(null);
    const errors = ref({});          // { field: [messages] } from a 400 validation response
    const formError = ref('');       // page-level message: a load failure
    const isLoading = ref(true);
    const isSaving = ref(false);

    const title = computed(() => (isNew ? 'Add Notification' : 'Notification Details'));

    function fill(detail) {
        notificationId.value = detail.id;
        info.value = detail;
        Object.assign(form, { rowVersion: detail.rowVersion, notification: detail.notification });
    }

    async function save() {
        errors.value = {};
        isSaving.value = true;
        try {
            const detail = isNew ? await notificationsApi.create(form) : await notificationsApi.update(notificationId.value, form);
            logSuccess('Notification saved.');
            if (isNew) {
                router.replace(`/notifications/${detail.id}`);
                return;
            }
            fill(detail);
        } catch (e) {
            // Field problems (400) show next to their inputs; anything else (409 conflict, ...) is a toast.
            const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
            if (fieldErrors) errors.value = fieldErrors;
            else logApiError(e);
        } finally {
            isSaving.value = false;
        }
    }

    const cancel = () => router.push('/notifications');

    onMounted(async () => {
        try {
            if (!isNew) fill(await notificationsApi.get(route.params.key));
        } catch (e) {
            // Nothing to edit: keep the reason on the page.
            formError.value = e.response?.status === 404 ? 'Notification not found.' : apiErrorMessage(e);
        } finally {
            isLoading.value = false;
        }
    });

    return { isNew, canEdit, title, form, info, errors, formError, isLoading, isSaving, save, cancel };
}
