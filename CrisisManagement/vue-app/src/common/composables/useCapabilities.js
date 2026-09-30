import { useAppStore } from '../../stores/useAppStore.js';

// UI gate only - the server enforces the same rules on every API call. Mirrors the server
// PermissionHandler: Administrators pass, and x.edit implies x.view.
//
// Usage:
//   const { can } = useCapabilities();
//   const canEdit = can('users.edit');
// Permissions that also grant another one, beyond "edit implies view" (same table as the server PermissionHandler).
const GRANTED_BY = { 'assessments.files.view': ['assessments.fileupload'] };

export function useCapabilities() {
    const appStore = useAppStore();

    function can(permission) {
        const user = appStore.currentUser;
        if (user?.isAdmin) return true;

        const held = new Set((user?.permissions ?? []).map(p => p.toLowerCase()));
        const wanted = permission.toLowerCase();
        return held.has(wanted) || (wanted.endsWith('.view') && held.has(`${wanted.slice(0, -5)}.edit`))
            || (GRANTED_BY[wanted] ?? []).some((p) => held.has(p));
    }

    return { can };
}
