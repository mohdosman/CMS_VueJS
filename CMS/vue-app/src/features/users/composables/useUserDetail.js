import { ref, reactive, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { usersApi } from '../api/usersApi.js';
import { can } from '../../../boot.js';
import { announce } from '../../../services/liveAnnouncer.js';

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

    const isNew = route.params.key === '0';
    const canEdit = can('users.edit');

    const form = reactive(blankForm());
    const info = ref(null);          // read-only facts about an existing user
    const roles = ref([]);
    const providers = ref([]);
    const policy = ref({ passwordRules: [], adUserNameRule: '' });
    const errors = ref({});          // { field: [messages] }
    const formError = ref('');
    const notice = ref('');
    const dialog = ref('');          // '', 'password' or 'delete'
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

    function fail(e) {
        errors.value = e.fieldErrors ?? {};
        // Anything that is not a field problem (403, 409 conflict, form-level) is shown as a banner.
        formError.value = e.fieldErrors ? (e.fieldErrors.form?.join(' ') ?? '') : e.message;
    }

    async function save() {
        errors.value = {};
        formError.value = '';
        notice.value = '';
        isSaving.value = true;
        try {
            const body = { ...form, providerIds: hasAdminRole.value ? [] : form.providerIds };
            const detail = isNew ? await usersApi.create(body) : await usersApi.update(route.params.key, body);
            if (isNew) {
                router.replace(`/admin/users/${detail.userKey}`);
                return;
            }
            fill(detail);
            notice.value = 'User saved.';
            announce('User saved');
        } catch (e) {
            fail(e);
            announce('The user could not be saved. Check the highlighted fields.');
        } finally {
            isSaving.value = false;
        }
    }

    async function remove() {
        try {
            await usersApi.remove(route.params.key);
            router.push('/admin/users');
        } catch (e) {
            // 403 / 409 explain themselves ("has documents", "your own account"); show them on the page.
            dialog.value = '';
            formError.value = e.message;
        }
    }

    // A password reset bumps the security stamp but not the row version, so the form stays valid.
    function passwordSet() {
        dialog.value = '';
        notice.value = 'Password set. The user must change it at next sign-in.';
        announce(notice.value);
    }

    const cancel = () => router.push('/admin/users');

    onMounted(async () => {
        try {
            [roles.value, providers.value, policy.value] = await Promise.all([usersApi.roles(), usersApi.providers(), usersApi.policy()]);
            if (!isNew) fill(await usersApi.get(route.params.key));
        } catch (e) {
            formError.value = e.status === 404 ? 'User not found.' : e.message;
        } finally {
            isLoading.value = false;
        }
    });

    return {
        isNew, canEdit, form, info, roles, providers, policy, errors, formError, notice, dialog,
        isLoading, isSaving, hasAdminRole, save, remove, passwordSet, cancel
    };
}
