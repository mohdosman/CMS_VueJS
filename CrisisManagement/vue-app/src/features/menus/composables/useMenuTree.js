import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { menusApi } from '../api/menusApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { announce } from '../../../services/liveAnnouncer.js';

// Module scope on purpose: the filter survives list -> edit -> back within the SPA.
const items = ref([]);
const filter = ref('');
const isLoading = ref(false);

export function useMenuTree() {
    const router = useRouter();
    const { can } = useCapabilities();
    const { logApiError, logSuccess } = useLogger();

    const canEdit = can('menus.edit');
    const confirming = ref(null);   // the menu item awaiting delete confirmation

    async function load() {
        isLoading.value = true;
        try {
            items.value = await menusApi.list();
        } catch (e) {
            logApiError(e);
        } finally {
            isLoading.value = false;
        }
    }

    // The tree, roots first. While filtering, an item stays when it matches by name or url, or when a descendant does,
    // so a match is never cut off from its parents.
    const tree = computed(() => {
        const term = filter.value.trim().toLowerCase();
        const byParent = new Map();
        for (const m of items.value) {
            const key = m.parentId ?? 0;
            if (!byParent.has(key)) byParent.set(key, []);
            byParent.get(key).push(m);
        }
        const matches = (m) => !term || m.name.toLowerCase().includes(term) || (m.url ?? '').toLowerCase().includes(term);
        const build = (parent) => (byParent.get(parent) ?? [])
            .map((m) => ({ ...m, children: build(m.id) }))
            .filter((n) => matches(n) || n.children.length);
        return build(0);
    });

    function search() {
        const count = items.value.filter((m) => !filter.value.trim()
            || m.name.toLowerCase().includes(filter.value.trim().toLowerCase())
            || (m.url ?? '').toLowerCase().includes(filter.value.trim().toLowerCase())).length;
        announce(`${count} menu items match`);
    }

    const clear = () => { filter.value = ''; search(); };
    const edit = (m) => router.push(`/admin/menus/${m.id}`);
    const addRoot = () => router.push('/admin/menus/0');
    const addChild = (m) => router.push({ path: '/admin/menus/0', query: { parentId: m.id } });

    async function remove() {
        const m = confirming.value;
        try {
            await menusApi.remove(m.id);
            logSuccess('Menu item deleted.');
            confirming.value = null;
            await load();
        } catch (e) {
            // 409 explains itself ("still has sub-menus").
            confirming.value = null;
            logApiError(e);
        }
    }

    onMounted(load);

    return { tree, filter, isLoading, canEdit, confirming, search, clear, edit, addRoot, addChild, remove };
}
