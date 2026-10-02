import { ref, reactive, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { notificationsApi } from '../api/notificationsApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { restoreSearchOnReturn } from '../../../common/composables/useSearchState.js';
import { apiErrorMessage } from '../../../utils/apiError.js';

// Port of ManageNotification.aspx: one form for a new notification (/notifications/0) and an existing one (/notifications/:key).
export function useNotificationDetail() {
    const route = useRoute();
    const router = useRouter();
    const { logSuccess, logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const isNew = computed(() => route.params.key === '0');
    const title = 'Manage Notification';
    const maxLength = 1000;   // the server enforces the same limit

    const form = reactive({ rowVersion: null, notification: '' });
    const notificationId = ref(0);
    const errors = ref({});          // { field: [messages] } from the checks below or a 400 response
    const formError = ref('');       // page-level message: a load failure
    const isLoading = ref(true);
    const isSaving = ref(false);

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canEdit = can('notifications.edit');

    // ================================================================
    // Loading
    // ================================================================
    async function getNotification() {
        try {
            const detail = await notificationsApi.get(route.params.key);
            notificationId.value = detail.id;
            Object.assign(form, { rowVersion: detail.rowVersion, notification: detail.notification });
        } catch (e) {
            // Nothing to edit: keep the reason on the page.
            formError.value = e.response?.status === 404 ? 'Notification not found.' : apiErrorMessage(e);
        }
    }

    // ================================================================
    // Navigation
    // ================================================================
    function backToList() {
        restoreSearchOnReturn();
        router.push('/notifications');
    }

    function cancel() {
        backToList();
    }

    // ================================================================
    // Save
    // ================================================================
    function validate() {
        errors.value = {};
        if (!form.notification.trim()) {
            errors.value = { notification: ['Notification is required'] };
        }
        return Object.keys(errors.value).length === 0;
    }

    async function save() {
        if (!validate()) {
            return;
        }
        isSaving.value = true;
        try {
            if (isNew.value) {
                await notificationsApi.create(form);
            } else {
                await notificationsApi.update(notificationId.value, form);
            }
            logSuccess(isNew.value ? 'Notification created.' : 'Notification updated.');
            backToList();
        } catch (e) {
            // Field problems (400) show next to their inputs; anything else (409 conflict, ...) is a toast.
            const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
            if (fieldErrors) {
                errors.value = fieldErrors;
            } else {
                logApiError(e);
            }
        } finally {
            isSaving.value = false;
        }
    }

    // ================================================================
    // Page load
    // ================================================================
    useActivate(async () => {
        isLoading.value = true;
        formError.value = '';
        if (!isNew.value) {
            await getNotification();
        }
        isLoading.value = false;
    });

    return {
        // Form
        form, errors, formError, title, maxLength,

        // Busy state
        isLoading, isSaving,

        // User and permissions
        canEdit,

        // Actions
        save, cancel
    };
}
