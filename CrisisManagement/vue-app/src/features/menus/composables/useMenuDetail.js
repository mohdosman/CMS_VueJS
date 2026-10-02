import { ref, reactive, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { menusApi } from '../api/menusApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { restoreSearchOnReturn } from '../../../common/composables/useSearchState.js';
import { fieldMessages } from '../../../utils/formErrors.js';
import { apiErrorMessage } from '../../../utils/apiError.js';
import { iconOptions } from '../icons.js';

// One form for a new menu item (/admin/menus/0, optional ?parentId=) and an existing one (/admin/menus/:key, the menu item id).
export function useMenuDetail() {
    const route = useRoute();
    const router = useRouter();
    const { logSuccess, logApiError } = useLogger();

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
    const errors = ref({});          // { field: [messages] } from a 400 validation response
    const formError = ref('');       // page-level message: a load failure
    const tab = ref('details');      // 'details' or 'permissions'
    const isLoading = ref(true);
    const isSaving = ref(false);

    const title = computed(() => (isNew ? 'New Menu Item' : `Edit Menu Item #${menuId.value}`));

    const msg = fieldMessages(errors);
    const err = (field) => errors.value[field]?.length ?? 0;

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
    // Loading
    // ================================================================
    function fill(detail) {
        menuId.value = detail.id;
        Object.assign(form, blankForm(), detail);
        form.parentId = detail.parentId ?? null;
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
            // Nothing to edit: keep the reason on the page.
            formError.value = e.response?.status === 404 ? 'Menu item not found.' : apiErrorMessage(e);
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
    async function save() {
        errors.value = {};
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
        await getPageData();
        isLoading.value = false;
    });

    return {
        // Form
        form, menuId, parents, tab, title, isNew, urlFields,

        // Busy and validation state
        isLoading, isSaving, errors, formError, msg, err,

        // User and permissions
        canEdit,

        // Actions
        save, cancel,

        // Helpers for the template
        iconOptions
    };
}
