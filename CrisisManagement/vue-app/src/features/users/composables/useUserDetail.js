import { ref, reactive, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { usersApi } from '../api/usersApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { apiErrorMessage } from '../../../utils/apiError.js';

const blankForm = () => ({
    rowVersion: null, userName: '', firstName: '', lastName: '', email: '', phoneNumber: '', notes: '',
    password: '', confirmPassword: '',
    isEnabled: true, isADAccount: false, twoFactorEnabled: false,
    roleIds: [], providerIds: []
});

// One form for both create (/admin/users/0) and edit (/admin/users/:key).
export function useUserDetail() {
    const route = useRoute();
    const router = useRouter();
    const { can } = useCapabilities();
    const { logSuccess, logApiError } = useLogger();

    const isNew = route.params.key === '0';
    const canEdit = can('users.edit');

    const form = reactive(blankForm());
    const info = ref(null);          // read-only facts about an existing user
    const roles = ref([]);
    const providers = ref([]);
    const policy = ref({ passwordRules: [], adUserNameRule: '' });
    const errors = ref({});          // { field: [messages] } from a 400 validation response
    const formError = ref('');       // page-level message: form-level validation, or a load failure
    const dialog = ref('');          // '', 'password', 'delete', 'agreements' or 'upload'
    const documentCount = ref(0);
    const isLoading = ref(true);
    const isSaving = ref(false);

    // Providers do not apply to administrators, whose access is unrestricted.
    const hasAdminRole = computed(() =>
        roles.value.some((r) => form.roleIds.includes(r.id) && r.label.toLowerCase() === 'administrator'));

    function fill(detail) {
        info.value = detail;
        Object.assign(form, blankForm(), {
            rowVersion: detail.rowVersion, userName: detail.userName, firstName: detail.firstName,
            lastName: detail.lastName, email: detail.email, phoneNumber: detail.phoneNumber ?? '',
            notes: detail.notes, isEnabled: detail.isEnabled, isADAccount: detail.isADAccount,
            twoFactorEnabled: detail.twoFactorEnabled, roleIds: [...detail.roleIds], providerIds: [...detail.providerIds]
        });
    }

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
            router.push('/admin/users');
        } catch (e) {
            // 403 / 409 explain themselves ("has documents", "your own account").
            dialog.value = '';
            logApiError(e);
        }
    }

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
        if (uploaded) documentCount.value = (await usersApi.documents(route.params.key)).length;
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

    const cancel = () => router.push('/admin/users');

    onMounted(async () => {
        try {
            [roles.value, providers.value, policy.value] = await Promise.all([usersApi.roles(), usersApi.providers(), usersApi.policy()]);
            if (!isNew) {
                fill(await usersApi.get(route.params.key));
                documentCount.value = (await usersApi.documents(route.params.key)).length;
            }
        } catch (e) {
            // Nothing to edit: keep the reason on the page.
            formError.value = e.response?.status === 404 ? 'User not found.' : apiErrorMessage(e);
        } finally {
            isLoading.value = false;
        }
    });

    return {
        isNew, canEdit, form, info, roles, providers, policy, errors, formError, dialog,
        isLoading, isSaving, hasAdminRole, documentCount, resetMfa, save, remove, passwordSet, agreementsClosed, uploadClosed, cancel
    };
}
