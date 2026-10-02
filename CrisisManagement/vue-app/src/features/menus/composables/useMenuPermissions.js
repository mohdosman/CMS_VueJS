import { ref, reactive, computed, onMounted } from 'vue';
import { menusApi } from '../api/menusApi.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { announce } from '../../../services/liveAnnouncer.js';

// The Permissions tab of a saved menu item: its permissions grouped by permission group, plus the dialogs to add,
// edit and delete them and to create or rename permission groups. This lives inside the page form, so the dialogs
// use plain buttons and inputs (no nested <form>); Enter in a field submits the dialog.
export function useMenuPermissions(props) {
    const { logSuccess, logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const rows = ref([]);
    const groups = ref([]);
    const filter = ref('');
    const isLoading = ref(true);

    const dialog = ref('');          // '', 'permission', 'group', 'groups' or 'delete'
    const errors = ref({});          // field errors of the open dialog
    const editing = ref(null);       // the permission being edited, or null when adding
    const perm = reactive({ groupId: null, name: '', value: '', description: '' });
    const group = reactive({ id: 0, name: '', description: '' });   // 'group' dialog (id 0 = new) and the row being renamed in 'groups'
    const renaming = ref(0);         // group id being edited inline in the 'groups' dialog
    const confirming = ref(null);
    const isSaving = ref(false);

    const err = (field) => errors.value[field]?.join(' ');

    // The permissions that match the filter, grouped by permission group.
    const shown = computed(() => {
        const term = filter.value.trim().toLowerCase();
        const list = term ? rows.value.filter((r) => r.name.toLowerCase().includes(term) || r.value.toLowerCase().includes(term)) : rows.value;
        const byGroup = new Map();
        for (const row of list) {
            if (!byGroup.has(row.groupName)) {
                byGroup.set(row.groupName, []);
            }
            byGroup.get(row.groupName).push(row);
        }
        return [...byGroup].map(([name, items]) => ({ name, items }));
    });

    // ================================================================
    // Loading
    // ================================================================
    async function load() {
        try {
            [rows.value, groups.value] = await Promise.all([menusApi.permissions(props.menuId), menusApi.groups()]);
        } catch (e) {
            logApiError(e);
        } finally {
            isLoading.value = false;
        }
    }

    onMounted(load);

    // ================================================================
    // Dialogs
    // ================================================================
    function close() {
        dialog.value = '';
        errors.value = {};
        editing.value = null;
        renaming.value = 0;
        confirming.value = null;
    }

    // Runs a save; a 400 keeps the dialog open with the messages next to the fields, anything else is a toast.
    async function attempt(action, success) {
        errors.value = {};
        isSaving.value = true;
        try {
            await action();
            logSuccess(success);
            return true;
        } catch (e) {
            const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
            if (fieldErrors) {
                errors.value = fieldErrors;
            } else {
                logApiError(e);
            }
            return false;
        } finally {
            isSaving.value = false;
        }
    }

    // ================================================================
    // Permission add / edit / delete
    // ================================================================
    function openPermission(permission = null) {
        editing.value = permission;
        Object.assign(perm, permission
            ? { groupId: permission.groupId, name: permission.name, value: permission.value, description: permission.description }
            : { groupId: groups.value[0]?.id ?? null, name: '', value: '', description: '' });
        errors.value = {};
        dialog.value = 'permission';
    }

    async function savePermission() {
        const body = { menuId: props.menuId, ...perm };
        const saved = await attempt(
            () => (editing.value ? menusApi.updatePermission(editing.value.id, body) : menusApi.createPermission(body)),
            editing.value ? 'Permission updated.' : 'Permission created.'
        );
        if (saved) {
            close();
            await load();
        }
    }

    async function removePermission() {
        const permission = confirming.value;
        if (await attempt(() => menusApi.removePermission(permission.id), 'Permission deleted.')) {
            close();
            await load();
        }
    }

    // ================================================================
    // Permission groups
    // ================================================================
    function openNewGroup() {
        Object.assign(group, { id: 0, name: '', description: '' });
        errors.value = {};
        dialog.value = 'group';
    }

    async function saveNewGroup() {
        if (await attempt(() => menusApi.createGroup({ name: group.name, description: group.description }), 'Group created.')) {
            close();
            groups.value = await menusApi.groups();
        }
    }

    function startRename(existing) {
        Object.assign(group, { id: existing.id, name: existing.name, description: existing.description });
        renaming.value = existing.id;
        errors.value = {};
    }

    async function saveRename() {
        if (await attempt(() => menusApi.updateGroup(group.id, { name: group.name, description: group.description }), 'Group updated.')) {
            renaming.value = 0;
            await load();
        }
    }

    return {
        // Permissions
        groups, filter, shown,

        // Dialogs
        dialog, editing, perm, group, renaming, confirming,

        // Busy and validation state
        isLoading, isSaving, errors, err,

        // Actions
        openPermission, savePermission, removePermission,
        openNewGroup, saveNewGroup, startRename, saveRename, close,

        // Helpers for the template
        announce
    };
}
