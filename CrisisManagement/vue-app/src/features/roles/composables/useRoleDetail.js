import { ref, reactive, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { rolesApi } from '../api/rolesApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { restoreSearchOnReturn } from '../../../common/composables/useSearchState.js';
import { createShowError, createTouch } from '../../../utils/validationUtils.js';

// One form for a new role (/admin/roles/0) and an existing one (/admin/roles/:key, the role id).
export function useRoleDetail() {
    const route = useRoute();
    const router = useRouter();
    const { logSuccess, logError, logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const isNew = route.params.key === '0';
    const title = isNew ? 'Add Role' : 'Role Details';

    const form = reactive({ rowVersion: null, name: '', permissions: [] });
    const saved = ref({ name: '', permissions: [] });   // what the server holds, for the unsaved-changes flag
    const roleId = ref(0);
    const groups = ref([]);
    const serverErrors = ref({});    // { field: [messages] } from a 400 validation response
    const dialog = ref('');          // '' or 'delete'
    const isLoading = ref(true);
    const loadFailed = ref(false);    // the role (or the permission list) did not load, so there is nothing safe to edit
    const isSaving = ref(false);

    const isAdminRole = computed(() => saved.value.name.toLowerCase() === 'administrator');
    const hasChanges = computed(() =>
        form.name.trim() !== saved.value.name
        || form.permissions.length !== saved.value.permissions.length
        || form.permissions.some((p) => !saved.value.permissions.includes(p)));

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canEdit = can('roles.edit');

    // ================================================================
    // Validation
    // ================================================================
    const submitted = ref(false);
    // Which fields the user has visited. A field only shows its message once it was left, or after a submit.
    const touched = reactive({});
    const touch = createTouch(touched);

    // Same limit as RoleFieldLimits.MaxNameLength on the server.
    const MAX_NAME = 256;

    // The error message for each field; a field that is fine has no entry. The server also checks the name is unique.
    const clientErrors = computed(() => {
        const e = {};
        if (!form.name.trim()) {
            e.name = 'Role name is required.';
        } else if (form.name.trim().length > MAX_NAME) {
            e.name = `Role name cannot exceed ${MAX_NAME} characters.`;
        }
        return e;
    });

    const isValid = computed(() => Object.keys(clientErrors.value).length === 0);
    // A field shows its message only after it was touched, or after a submit was attempted.
    const showError = createShowError(touched, submitted, clientErrors);

    // What the screen shows for a field: the server's message when it sent one, else the form's own once it is due.
    const msg = (field) => serverErrors.value[field]?.join(' ') || (showError(field) ? clientErrors.value[field] : '');

    function resetValidation() {
        submitted.value = false;
        serverErrors.value = {};
        Object.keys(touched).forEach((field) => delete touched[field]);
    }

    // ================================================================
    // Loading
    // ================================================================
    function fill(detail) {
        roleId.value = detail.id;
        Object.assign(form, { rowVersion: detail.rowVersion, name: detail.name, permissions: [...detail.permissions] });
        saved.value = { name: detail.name, permissions: [...detail.permissions] };
        resetValidation();
    }

    async function getPageData() {
        loadFailed.value = false;
        try {
            groups.value = await rolesApi.permissions();
            if (!isNew) {
                fill(await rolesApi.get(route.params.key));
            }
        } catch (e) {
            loadFailed.value = true;
            logApiError(e, { fallback: 'Role not found.' });
        }
    }

    // ================================================================
    // Navigation
    // ================================================================
    function backToList() {
        restoreSearchOnReturn();
        router.push('/admin/roles');
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
            serverErrors.value = fieldErrors;
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
            logError('You do not have permission to manage roles.');
            return;
        }
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

    async function remove() {
        try {
            await rolesApi.remove(roleId.value);
            logSuccess('Role deleted.');
            backToList();
        } catch (e) {
            // 409 explains itself ("users still hold this role", "built in").
            dialog.value = '';
            logApiError(e);
        }
    }

    // ================================================================
    // Page load
    // ================================================================
    useActivate(async () => {
        isLoading.value = true;
        await getPageData();
        isLoading.value = false;
    });

    return {
        // Form
        form, groups, dialog, title, isNew,

        // Busy and validation state
        isLoading, loadFailed, isSaving, submitted, touched, touch, isValid, showError, msg, isAdminRole, hasChanges,

        // User and permissions
        canEdit,

        // Actions
        save, onSubmit, remove, cancel
    };
}
