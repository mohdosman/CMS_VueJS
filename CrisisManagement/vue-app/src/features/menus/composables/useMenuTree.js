import { ref, reactive, computed } from 'vue';
import { useRouter } from 'vue-router';
import { menusApi } from '../api/menusApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useSearchState } from '../../../common/composables/useSearchState.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { announce } from '../../../services/liveAnnouncer.js';

// Menu list: the whole menu tree, filtered by name or url.
export function useMenuTree() {
    const router = useRouter();
    const { logApiError, logSuccess } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const items = ref([]);
    const isLoading = ref(false);
    const confirming = ref(null);   // the menu item awaiting delete confirmation

    const DEFAULT_CRITERIA = { filter: '' };
    const criteria = reactive({ ...DEFAULT_CRITERIA });
    const filter = computed({
        get: () => criteria.filter,
        set: (value) => { criteria.filter = value; }
    });

    const { load, save, clear: clearState } = useSearchState('menuSearchJSON', criteria, DEFAULT_CRITERIA);

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canEdit = can('menus.edit');

    // ================================================================
    // Loading and filtering
    // ================================================================
    async function getMenus() {
        isLoading.value = true;
        try {
            items.value = await menusApi.list();
        } catch (e) {
            logApiError(e);
        } finally {
            isLoading.value = false;
        }
    }

    const matches = (menu, term) => !term || menu.name.toLowerCase().includes(term) || (menu.url ?? '').toLowerCase().includes(term);

    // The tree, roots first. While filtering, an item stays when it matches by name or url, or when a descendant does,
    // so a match is never cut off from its parents.
    const tree = computed(() => {
        const term = filter.value.trim().toLowerCase();
        const byParent = new Map();
        for (const menu of items.value) {
            const key = menu.parentId ?? 0;
            if (!byParent.has(key)) {
                byParent.set(key, []);
            }
            byParent.get(key).push(menu);
        }
        const build = (parent) => (byParent.get(parent) ?? [])
            .map((menu) => ({ ...menu, children: build(menu.id) }))
            .filter((node) => matches(node, term) || node.children.length);
        return build(0);
    });

    function search() {
        const term = filter.value.trim().toLowerCase();
        save();
        announce(`${items.value.filter((menu) => matches(menu, term)).length} menu items match`);
    }

    // Runs each time the screen is shown: restore the saved filter and load the menus.
    useActivate(async () => {
        load();
        await getMenus();
    });

    // ================================================================
    // Navigation and clear
    // ================================================================
    function edit(menu) {
        router.push(`/admin/menus/${menu.id}`);
    }

    function addRoot() {
        router.push('/admin/menus/0');
    }

    function addChild(menu) {
        router.push({ path: '/admin/menus/0', query: { parentId: menu.id } });
    }

    function clear() {
        clearState();
        search();
    }

    // ================================================================
    // Delete
    // ================================================================
    async function remove() {
        const menu = confirming.value;
        try {
            await menusApi.remove(menu.id);
            logSuccess('Menu item deleted.');
            confirming.value = null;
            await getMenus();
        } catch (e) {
            // 409 explains itself ("still has sub-menus").
            confirming.value = null;
            logApiError(e);
        }
    }

    return {
        // Results
        tree, filter,

        // Busy state
        isLoading, confirming,

        // User and permissions
        canEdit,

        // Actions
        search, clear, edit, addRoot, addChild, remove
    };
}
