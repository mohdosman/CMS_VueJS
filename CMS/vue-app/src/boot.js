// Server-injected bootstrap blob (HomeViewModel.ToBootstrapJson).
export const boot = JSON.parse(document.getElementById('__cms_boot__')?.textContent ?? '{}');

const permissions = new Set((boot.currentUser?.permissions ?? []).map((p) => p.toLowerCase()));

// UI gate only - the server enforces the same rules on every API call.
// Mirrors PermissionHandler: admins pass, and x.edit implies x.view.
export function can(permission) {
    if (boot.currentUser?.isAdmin) return true;
    const p = permission.toLowerCase();
    return permissions.has(p) || (p.endsWith('.view') && permissions.has(p.slice(0, -5) + '.edit'));
}
