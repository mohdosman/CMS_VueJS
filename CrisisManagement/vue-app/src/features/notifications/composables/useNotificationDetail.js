import { ref, reactive, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { notificationsApi } from '../api/notificationsApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { restoreSearchOnReturn } from '../../../common/composables/useSearchState.js';
import { createShowError, createTouch } from '../../../utils/validationUtils.js';

// Port of ManageNotification.aspx: one form for a new notification (/notifications/0) and an existing one (/notifications/:key).
export function useNotificationDetail() {
    const route = useRoute();
    const router = useRouter();
    const { logSuccess, logError, logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const isNew = computed(() => route.params.key === '0');
    const title = 'Manage Notification';
    const maxLength = 1000;   // the server enforces the same limit

    const form = reactive({ rowVersion: null, notification: '' });
    const notificationId = ref(0);
    const serverErrors = ref({});    // { field: [messages] } from a 400 response
    const isLoading = ref(true);
    const isSaving = ref(false);

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canEdit = can('notifications.edit');

    // ================================================================
    // Validation
    // ================================================================
    const submitted = ref(false);
    // Which fields the user has visited. A field only shows its message once it was left, or after a submit.
    const touched = reactive({});
    const touch = createTouch(touched);

    // The error message for each field; a field that is fine has no entry.
    const clientErrors = computed(() => {
        const e = {};
        if (!form.notification.trim()) {
            e.notification = 'Notification is required';
        }
        return e;
    });

    const isValid = computed(() => Object.keys(clientErrors.value).length === 0);
    // A field shows its message only after it was touched, or after a submit was attempted.
    const showError = createShowError(touched, submitted, clientErrors);

    // What the screen shows for a field: the server's message when it sent one, else the form's own once it is due.
    const msg = (field) => serverErrors.value[field]?.join(' ') || (showError(field) ? clientErrors.value[field] : '');

    // ================================================================
    // Loading
    // ================================================================
    async function getNotification() {
        try {
            const detail = await notificationsApi.get(route.params.key);
            notificationId.value = detail.id;
            Object.assign(form, { rowVersion: detail.rowVersion, notification: detail.notification });
        } catch (e) {
            logApiError(e, { fallback: 'Notification not found.' });
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
    // Checks permission, then saves. On success it returns to the list.
    async function save() {
        if (isSaving.value) {
            return;
        }
        if (!canEdit) {
            logError('You do not have permission to manage notifications.');
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
                serverErrors.value = fieldErrors;
                logError('Please correct the validation errors first.');
            } else {
                logApiError(e);
            }
        } finally {
            isSaving.value = false;
        }
    }

    // The form's submit: validates first, and only saves when everything is valid.
    function onSubmit() {
        submitted.value = true;
        serverErrors.value = {};
        if (!isValid.value) {
            logError('Please correct the validation errors first.');
            return;
        }
        save();
    }

    // ================================================================
    // Page load
    // ================================================================
    useActivate(async () => {
        isLoading.value = true;
        if (!isNew.value) {
            await getNotification();
        }
        isLoading.value = false;
    });

    return {
        // Form
        form, title, maxLength,

        // Busy and validation state
        isLoading, isSaving, submitted, touched, touch, isValid, showError, msg,

        // User and permissions
        canEdit,

        // Actions
        save, onSubmit, cancel
    };
}
