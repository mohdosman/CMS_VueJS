import { useAppStore } from '../../stores/useAppStore.js';

// UI gate only - the server enforces the same rules on every API call. Mirrors the server
// PermissionHandler: Administrators pass, and x.edit implies x.view.
//
// Usage:
//   const { can } = useCapabilities();
//   const canEdit = can('users.edit');
export function useCapabilities() {
    const appStore = useAppStore();

    function can(permission) {
        const user = appStore.currentUser;
        if (user?.isAdmin) return true;

        const held = new Set((user?.permissions ?? []).map(p => p.toLowerCase()));
        const wanted = permission.toLowerCase();
        return held.has(wanted) || (wanted.endsWith('.view') && held.has(`${wanted.slice(0, -5)}.edit`));
    }

    return { can };
}
