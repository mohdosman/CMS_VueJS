import { ref, reactive, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { usersApi } from '../api/usersApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { restoreSearchOnReturn } from '../../../common/composables/useSearchState.js';
import { createShowError, createTouch } from '../../../utils/validationUtils.js';

// One form for a new user (/admin/users/0) and an existing one (/admin/users/:key).
export function useUserDetail() {
    const route = useRoute();
    const router = useRouter();
    const { logSuccess, logError, logApiError } = useLogger();

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
    const policy = ref({ passwordRules: [], adUserNameRule: '', password: null, userName: null });
    const serverErrors = ref({});    // { field: [messages] } from a 400 validation response
    const formError = ref('');       // page-level message: form-level validation
    const dialog = ref('');          // '', 'password', 'mfa', 'agreements' or 'upload'
    const documentCount = ref(0);
    const isLoading = ref(true);
    const isSaving = ref(false);

    // A local account signs in with its email, so its user ID is the email (fixed once created).
    const userIdShown = computed(() => (form.isADAccount || !isNew ? form.userName : form.email));
    const idCaption = computed(() => (form.isADAccount
        ? 'The user ID cannot be changed after the account is created.'
        : 'The email and user ID cannot be changed after the account is created.'));

    // The numeric id Blazor shows is not exposed here, so the user ID stands in for it.
    const title = computed(() => (isNew ? 'Add User' : `Edit user ${info.value?.userName ?? ''}`.trim()));

    // Providers do not apply to administrators, whose access is unrestricted.
    const hasAdminRole = computed(() =>
        roles.value.some((r) => form.roleIds.includes(r.id) && r.label.toLowerCase() === 'administrator'));

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canEdit = can('users.edit');

    // ================================================================
    // Validation
    // ================================================================
    const submitted = ref(false);
    // Which fields the user has visited. A field only shows its message once it was left, or after a submit.
    const touched = reactive({});
    const touch = createTouch(touched);

    const blank = (value) => !String(value ?? '').trim();

    // Column limits; the same numbers as UserService on the server.
    const MAX_NAME = 256, MAX_AD_EMAIL = 100, MAX_LOCAL_EMAIL = 50, MAX_PHONE = 32;

    // Whether a password meets the server's structured rules (the ones the checklist shows).
    function passwordMeetsPolicy(password) {
        const rules = policy.value.password;
        if (!rules) {
            return true;
        }
        return password.length >= rules.minLength
            && (!rules.requireLowercase || /[a-z]/.test(password))
            && (!rules.requireUppercase || /[A-Z]/.test(password))
            && (!rules.requireDigit || /\d/.test(password))
            && (!rules.requireSpecial || /[^A-Za-z0-9]/.test(password))
            && (!(rules.uniqueChars > 1) || new Set(password).size >= rules.uniqueChars);
    }

    // The AD user ID rules come from the server policy, so they follow its configuration.
    function userNameError(value) {
        const rule = policy.value.userName;
        const v = value.trim();
        if (!v) {
            return 'User ID is required.';
        }
        if (!rule) {
            return '';
        }
        if (v.length < rule.minLength) {
            return `User ID must be at least ${rule.minLength} characters in length.`;
        }
        if (v.length > rule.maxLength) {
            return `User ID cannot exceed ${rule.maxLength} characters in length.`;
        }
        if (v.includes('@')) {
            return 'User ID cannot contain \'@\'.';
        }
        if (!rule.prefixes.some((p) => v.toLowerCase().startsWith(p.toLowerCase()))) {
            return `User ID must start with ${rule.prefixes.map((p) => p.toUpperCase()).join(' or ')}.`;
        }
        return '';
    }

    // The error message for each field; a field that is fine has no entry. The server repeats these checks and also checks the account type.
    const clientErrors = computed(() => {
        const e = {};
        if (isNew && form.isADAccount && userNameError(form.userName)) {
            e.userName = userNameError(form.userName);
        }
        if (blank(form.firstName)) {
            e.firstName = 'First name is required';
        } else if (form.firstName.trim().length > MAX_NAME) {
            e.firstName = `First name cannot exceed ${MAX_NAME} characters`;
        }
        if (blank(form.lastName)) {
            e.lastName = 'Last name is required';
        } else if (form.lastName.trim().length > MAX_NAME) {
            e.lastName = `Last name cannot exceed ${MAX_NAME} characters`;
        }
        const email = form.email.trim();
        const maxEmail = form.isADAccount ? MAX_AD_EMAIL : MAX_LOCAL_EMAIL;
        if (!email) {
            e.email = 'Email is required';
        } else if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email)) {
            e.email = 'Invalid email format';
        } else if (email.length > maxEmail) {
            e.email = `Email cannot exceed ${maxEmail} characters`;
        }
        const phone = form.phoneNumber.trim();
        if (phone.length > MAX_PHONE) {
            e.phoneNumber = `Phone number cannot exceed ${MAX_PHONE} characters in length.`;
        } else if (phone) {
            const body = phone.startsWith('+') ? phone.slice(1) : phone;
            const digits = body.replace(/\D/g, '').length;
            if (!/^[\d ().-]+$/.test(body) || digits < 10 || digits > 15) {
                e.phoneNumber = 'Enter a valid phone number, for example (555) 123-4567.';
            }
        }
        if (form.isADAccount) {
            if (form.password) {
                e.password = 'AD accounts should not have passwords set here.';
            }
        } else if (isNew) {
            if (!form.password) {
                e.password = 'Password is required';
            } else if (!passwordMeetsPolicy(form.password)) {
                e.password = 'Password does not meet the requirements listed.';
            }
        }
        if (isNew && !form.isADAccount && form.password && !form.confirmPassword) {
            e.confirmPassword = 'Confirm password is required';
        } else if (isNew && form.password && form.password !== form.confirmPassword) {
            e.confirmPassword = 'Passwords must match';
        }
        if (!form.roleIds.length) {
            e.roleIds = 'At least one role must be selected';
        }
        return e;
    });

    const isValid = computed(() => Object.keys(clientErrors.value).length === 0);
    // A field shows its message only after it was touched, or after a submit was attempted.
    const showError = createShowError(touched, submitted, clientErrors);

    // What the screen shows for a field: the server's message when it sent one, else the form's own once it is due.
    const msg = (field) => serverErrors.value[field]?.join(' ') || (showError(field) ? clientErrors.value[field] : '');
    const err = (field) => (msg(field) ? 1 : 0);

    function resetValidation() {
        submitted.value = false;
        serverErrors.value = {};
        Object.keys(touched).forEach((field) => delete touched[field]);
    }

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
        resetValidation();
    }

    async function getPageData() {
        try {
            [roles.value, providers.value, policy.value] = await Promise.all([usersApi.roles(), usersApi.providers(), usersApi.policy()]);
            if (!isNew) {
                fill(await usersApi.get(route.params.key));
                documentCount.value = (await usersApi.documents(route.params.key)).length;
            }
        } catch (e) {
            logApiError(e, { fallback: 'Load failed.' });
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
    // Save
    // ================================================================
    // Field problems (400) show next to their inputs; anything else (403, 409 conflict, ...) is a toast.
    function fail(e) {
        const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
        if (fieldErrors) {
            serverErrors.value = fieldErrors;
            formError.value = fieldErrors.form?.join(' ') ?? '';
            logError('Please correct the validation errors first.');
        } else {
            logApiError(e);
        }
    }

    // Checks permission, then saves.
    async function save() {
        if (isSaving.value) {
            return;
        }
        if (!canEdit) {
            logError('You do not have permission to manage users.');
            return;
        }
        formError.value = '';
        isSaving.value = true;
        try {
            const body = { ...form, providerIds: hasAdminRole.value ? [] : form.providerIds };
            await (isNew ? usersApi.create(body) : usersApi.update(route.params.key, body));
            logSuccess('User saved.');
            backToList();
        } catch (e) {
            fail(e);
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
        form, info, roles, providers, policy, dialog, documentCount, isNew, title, userIdShown, idCaption,

        // Busy and validation state
        isLoading, isSaving, formError, submitted, touched, touch, isValid, showError, msg, err, hasAdminRole,

        // User and permissions
        canEdit,

        // Actions
        save, onSubmit, cancel, passwordSet, agreementsClosed, uploadClosed, resetMfa
    };
}
