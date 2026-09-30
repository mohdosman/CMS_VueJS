import { ref, reactive, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { menusApi } from '../api/menusApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { apiErrorMessage } from '../../../utils/apiError.js';

const blankForm = () => ({
    rowVersion: null, name: '', icon: '', description: '', url: '', detailUrl: '', templateUrl: '', detailTemplateUrl: '',
    apiUrl: '', comment: '', parentId: null, displaySequence: 0, isAlwaysEnabled: false, isEnabled: true
});

// One form for both create (/admin/menus/0, optional ?parentId=) and edit (/admin/menus/:key, the menu item id).
export function useMenuDetail() {
    const route = useRoute();
    const router = useRouter();
    const { can } = useCapabilities();
    const { logSuccess, logApiError } = useLogger();

    const isNew = route.params.key === '0';
    const canEdit = can('menus.edit');

    const form = reactive(blankForm());
    const menuId = ref(0);
    const parents = ref([]);
    const errors = ref({});          // { field: [messages] } from a 400 validation response
    const formError = ref('');       // page-level message: a load failure
    const tab = ref('details');      // 'details' or 'permissions'
    const isLoading = ref(true);
    const isSaving = ref(false);

    const title = computed(() => (isNew ? 'New Menu Item' : `Edit Menu Item #${menuId.value}`));

    function fill(detail) {
        menuId.value = detail.id;
        Object.assign(form, blankForm(), detail);
        form.parentId = detail.parentId ?? null;
    }

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
            if (fieldErrors) errors.value = fieldErrors;
            else logApiError(e);
        } finally {
            isSaving.value = false;
        }
    }

    const cancel = () => router.push('/admin/menus');

    onMounted(async () => {
        try {
            parents.value = await menusApi.parents(isNew ? null : route.params.key);
            if (isNew) {
                const parentId = Number(route.query.parentId);
                if (parentId) form.parentId = parentId;
            } else {
                fill(await menusApi.get(route.params.key));
            }
        } catch (e) {
            // Nothing to edit: keep the reason on the page.
            formError.value = e.response?.status === 404 ? 'Menu item not found.' : apiErrorMessage(e);
        } finally {
            isLoading.value = false;
        }
    });

    return { isNew, canEdit, title, form, menuId, parents, errors, formError, tab, isLoading, isSaving, save, cancel };
}
