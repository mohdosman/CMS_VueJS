// Reads server-injected bootstrap data from the DOM element written by Home/Index.cshtml
// (HomeViewModel.ToBootstrapJson). Called once in main.js -> useAppStore().hydrate(bootstrapData).
export function readServerBootstrap() {
    const el = document.getElementById('__cms_boot__');
    if (!el) {
        console.warn('[bootstrap] __cms_boot__ element not found');
        return {};
    }

    const data = JSON.parse(el.textContent);

    return {
        applicationName: data.applicationName ?? '',
        currentUser:     data.currentUser     ?? null,
        routes:          data.routes          ?? [],
        globals:         data.globals         ?? {}
    };
}
