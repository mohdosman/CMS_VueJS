import { ref, reactive, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { menusApi } from '../api/menusApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { restoreSearchOnReturn } from '../../../common/composables/useSearchState.js';
import { createShowError, createTouch } from '../../../utils/validationUtils.js';
import { iconOptions } from '../icons.js';

// One form for a new menu item (/admin/menus/0, optional ?parentId=) and an existing one (/admin/menus/:key, the menu item id).
export function useMenuDetail() {
    const route = useRoute();
    const router = useRouter();
    const { logSuccess, logError, logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const blankForm = () => ({
        rowVersion: null, name: '', icon: '', description: '', url: '', detailUrl: '', templateUrl: '', detailTemplateUrl: '',
        apiUrl: '', comment: '', parentId: null, displaySequence: 0, isAlwaysEnabled: false, isEnabled: true
    });

    const isNew = route.params.key === '0';

    const form = reactive(blankForm());
    const menuId = ref(0);
    const parents = ref([]);
    const serverErrors = ref({});    // { field: [messages] } from a 400 validation response
    const tab = ref('details');      // 'details' or 'permissions'
    const isLoading = ref(true);
    const isSaving = ref(false);

    const title = computed(() => (isNew ? 'New Menu Item' : `Edit Menu Item #${menuId.value}`));

    // The address text boxes under the name and icon.
    const urlFields = [
        { f: 'url', label: 'Url' },
        { f: 'detailUrl', label: 'Detail url' },
        { f: 'templateUrl', label: 'Template url' },
        { f: 'detailTemplateUrl', label: 'Detail template url' },
        { f: 'apiUrl', label: 'Api url' }
    ];

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canEdit = can('menus.edit');

    // ================================================================
    // Validation
    // ================================================================
    const submitted = ref(false);
    // Which fields the user has visited. A field only shows its message once it was left, or after a submit.
    const touched = reactive({});
    const touch = createTouch(touched);

    // The error message for each field; a field that is fine has no entry. The server also checks the name is unique and the parent.
    const clientErrors = computed(() => {
        const e = {};
        if (!String(form.name ?? '').trim()) {
            e.name = 'Menu item name is required.';
        }
        // A blank display order saves as 0.
        const sequence = Number(form.displaySequence);
        if (form.displaySequence !== '' && form.displaySequence !== null && (Number.isNaN(sequence) || sequence < 0 || sequence > 255)) {
            e.displaySequence = 'Display order must be between 0 and 255.';
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
        menuId.value = detail.id;
        Object.assign(form, blankForm(), detail);
        form.parentId = detail.parentId ?? null;
        resetValidation();
    }

    async function getPageData() {
        try {
            parents.value = await menusApi.parents(isNew ? null : route.params.key);
            if (isNew) {
                const parentId = Number(route.query.parentId);
                if (parentId) {
                    form.parentId = parentId;
                }
            } else {
                fill(await menusApi.get(route.params.key));
            }
        } catch (e) {
            logApiError(e, { fallback: 'Menu item not found.' });
        }
    }

    // ================================================================
    // Navigation
    // ================================================================
    function cancel() {
        restoreSearchOnReturn();
        router.push('/admin/menus');
    }

    // ================================================================
    // Save
    // ================================================================
    // Checks permission, then saves.
    async function save() {
        if (isSaving.value) {
            return;
        }
        if (!canEdit) {
            logError('You do not have permission to manage menus.');
            return;
        }
        isSaving.value = true;
        try {
            const body = { ...form, displaySequence: Number(form.displaySequence) || 0 };
            const detail = isNew ? await menusApi.create(body) : await menusApi.update(menuId.value, body);
            logSuccess('Menu item saved.');
            if (isNew) {
                // Permissions can only be added once the item exists, so land on the saved item.
                router.replace(`/admin/menus/${detail.id}`);
                return;
            }
            fill(detail);
            parents.value = await menusApi.parents(menuId.value);
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
        await getPageData();
        isLoading.value = false;
    });

    return {
        // Form
        form, menuId, parents, tab, title, isNew, urlFields,

        // Busy and validation state
        isLoading, isSaving, submitted, touched, touch, isValid, showError, msg, err,

        // User and permissions
        canEdit,

        // Actions
        save, onSubmit, cancel,

        // Helpers for the template
        iconOptions
    };
}
