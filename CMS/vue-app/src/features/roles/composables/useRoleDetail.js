import { ref, reactive, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { rolesApi } from '../api/rolesApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { apiErrorMessage } from '../../../utils/apiError.js';

// One form for both create (/admin/roles/0) and edit (/admin/roles/:key, the role id).
export function useRoleDetail() {
    const route = useRoute();
    const router = useRouter();
    const { can } = useCapabilities();
    const { logSuccess, logApiError } = useLogger();

    const isNew = route.params.key === '0';
    const canEdit = can('roles.edit');

    const form = reactive({ rowVersion: null, name: '', permissions: [] });
    const saved = ref({ name: '', permissions: [] });   // what the server holds, for the unsaved-changes flag
    const roleId = ref(0);
    const groups = ref([]);
    const errors = ref({});          // { field: [messages] } from a 400 validation response
    const formError = ref('');       // page-level message: a load failure
    const dialog = ref('');          // '' or 'delete'
    const isLoading = ref(true);
    const isSaving = ref(false);

    const isAdminRole = computed(() => saved.value.name.toLowerCase() === 'administrator');
    const hasChanges = computed(() =>
        form.name.trim() !== saved.value.name
        || form.permissions.length !== saved.value.permissions.length
        || form.permissions.some((p) => !saved.value.permissions.includes(p)));

    function fill(detail) {
        roleId.value = detail.id;
        Object.assign(form, { rowVersion: detail.rowVersion, name: detail.name, permissions: [...detail.permissions] });
        saved.value = { name: detail.name, permissions: [...detail.permissions] };
    }

    // Field problems (400) show next to their inputs; anything else (403, 409 conflict, ...) is a toast.
    function fail(e) {
        const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
        if (fieldErrors) errors.value = fieldErrors;
        else logApiError(e);
    }

    async function save() {
        errors.value = {};
        isSaving.value = true;
        try {
            const body = { ...form, name: form.name.trim() };
            const detail = isNew ? await rolesApi.create(body) : await rolesApi.update(roleId.value, body);
            logSuccess('Role saved.');
            if (isNew) {
                router.replace(`/admin/roles/${detail.id}`);
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
            await rolesApi.remove(roleId.value);
            logSuccess('Role deleted.');
            router.push('/admin/roles');
        } catch (e) {
            // 409 explains itself ("users still hold this role", "built in").
            dialog.value = '';
            logApiError(e);
        }
    }

    const cancel = () => router.push('/admin/roles');

    onMounted(async () => {
        try {
            groups.value = await rolesApi.permissions();
            if (!isNew) fill(await rolesApi.get(route.params.key));
        } catch (e) {
            // Nothing to edit: keep the reason on the page.
            formError.value = e.response?.status === 404 ? 'Role not found.' : apiErrorMessage(e);
        } finally {
            isLoading.value = false;
        }
    });

    return {
        isNew, canEdit, form, groups, errors, formError, dialog, isLoading, isSaving,
        isAdminRole, hasChanges, save, remove, cancel
    };
}
