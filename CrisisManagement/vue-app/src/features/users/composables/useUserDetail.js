import { ref, reactive, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { usersApi } from '../api/usersApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { restoreSearchOnReturn } from '../../../common/composables/useSearchState.js';
import { apiErrorMessage } from '../../../utils/apiError.js';

// One form for a new user (/admin/users/0) and an existing one (/admin/users/:key).
export function useUserDetail() {
    const route = useRoute();
    const router = useRouter();
    const { logSuccess, logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const blankForm = () => ({
        rowVersion: null, userName: '', firstName: '', lastName: '', email: '', phoneNumber: '', notes: '',
        password: '', confirmPassword: '',
        isEnabled: true, isADAccount: false, twoFactorEnabled: false,
        roleIds: [], providerIds: []
    });

    const isNew = route.params.key === '0';

    const form = reactive(blankForm());
    const info = ref(null);          // read-only facts about an existing user
    const roles = ref([]);
    const providers = ref([]);
    const policy = ref({ passwordRules: [], adUserNameRule: '', password: null });
    const errors = ref({});          // { field: [messages] } from a 400 validation response
    const formError = ref('');       // page-level message: form-level validation, or a load failure
    const dialog = ref('');          // '', 'password', 'delete', 'agreements' or 'upload'
    const documentCount = ref(0);
    const isLoading = ref(true);
    const isSaving = ref(false);

    // Providers do not apply to administrators, whose access is unrestricted.
    const hasAdminRole = computed(() =>
        roles.value.some((r) => form.roleIds.includes(r.id) && r.label.toLowerCase() === 'administrator'));

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canEdit = can('users.edit');

    // ================================================================
    // Loading
    // ================================================================
    function fill(detail) {
        info.value = detail;
        Object.assign(form, blankForm(), {
            rowVersion: detail.rowVersion, userName: detail.userName, firstName: detail.firstName,
            lastName: detail.lastName, email: detail.email, phoneNumber: detail.phoneNumber ?? '',
            notes: detail.notes, isEnabled: detail.isEnabled, isADAccount: detail.isADAccount,
            twoFactorEnabled: detail.twoFactorEnabled, roleIds: [...detail.roleIds], providerIds: [...detail.providerIds]
        });
    }

    async function getPageData() {
        try {
            [roles.value, providers.value, policy.value] = await Promise.all([usersApi.roles(), usersApi.providers(), usersApi.policy()]);
            if (!isNew) {
                fill(await usersApi.get(route.params.key));
                documentCount.value = (await usersApi.documents(route.params.key)).length;
            }
        } catch (e) {
            // Nothing to edit: keep the reason on the page.
            formError.value = e.response?.status === 404 ? 'User not found.' : apiErrorMessage(e);
        }
    }

    // ================================================================
    // Navigation
    // ================================================================
    function backToList() {
        restoreSearchOnReturn();
        router.push('/admin/users');
    }

    function cancel() {
        backToList();
    }

    // ================================================================
    // Save and delete
    // ================================================================
    // Field problems (400) show next to their inputs; anything else (403, 409 conflict, ...) is a toast.
    function fail(e) {
        const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
        if (fieldErrors) {
            errors.value = fieldErrors;
            formError.value = fieldErrors.form?.join(' ') ?? '';
        } else {
            logApiError(e);
        }
    }

    async function save() {
        errors.value = {};
        formError.value = '';
        isSaving.value = true;
        try {
            const body = { ...form, providerIds: hasAdminRole.value ? [] : form.providerIds };
            const detail = isNew ? await usersApi.create(body) : await usersApi.update(route.params.key, body);
            logSuccess('User saved.');
            if (isNew) {
                router.replace(`/admin/users/${detail.userKey}`);
                return;
            }
            fill(detail);
        } catch (e) {
            fail(e);
        } finally {
            isSaving.value = false;
        }
    }

    async function remove() {
        try {
            await usersApi.remove(route.params.key);
            logSuccess('User deleted.');
            backToList();
        } catch (e) {
            // 403 / 409 explain themselves ("has documents", "your own account").
            dialog.value = '';
            logApiError(e);
        }
    }

    // ================================================================
    // Password, agreements and MFA
    // ================================================================
    // A password reset bumps the security stamp but not the row version, so the form stays valid.
    function passwordSet() {
        dialog.value = '';
        logSuccess('Password set. The user must change it at next sign-in.');
    }

    // The agreements dialog reports the count it ended on (deletes included); the upload dialog whether anything landed.
    function agreementsClosed(count) {
        dialog.value = '';
        documentCount.value = count;
    }

    async function uploadClosed(uploaded) {
        dialog.value = '';
        if (uploaded) {
            documentCount.value = (await usersApi.documents(route.params.key)).length;
        }
    }

    // Administrator lifts the two-factor requirement and forgets the user's device; reload so the form shows the result.
    async function resetMfa() {
        try {
            await usersApi.resetMfa(route.params.key);
            fill(await usersApi.get(route.params.key));
            dialog.value = '';
            logSuccess('MFA reset. The user can enroll a new device.');
        } catch (e) {
            dialog.value = '';
            logApiError(e);
        }
    }

    // ================================================================
    // Page load
    // ================================================================
    useActivate(async () => {
        isLoading.value = true;
        formError.value = '';
        await getPageData();
        isLoading.value = false;
    });

    return {
        // Form
        form, info, roles, providers, policy, dialog, documentCount, isNew,

        // Busy and validation state
        isLoading, isSaving, errors, formError, hasAdminRole,

        // User and permissions
        canEdit,

        // Actions
        save, remove, cancel, passwordSet, agreementsClosed, uploadClosed, resetMfa
    };
}
