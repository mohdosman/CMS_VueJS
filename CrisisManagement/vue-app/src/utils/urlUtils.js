// Reads base URLs from the server bootstrap JSON element. Reading from the DOM directly (rather
// than the Pinia store) is intentional: http.js evaluates getApiBaseUrl() at module-init time,
// before Pinia is hydrated.
function getBootstrapGlobals() {
    try {
        const el = document.getElementById('__cms_boot__');
        return el ? (JSON.parse(el.textContent).globals ?? {}) : {};
    } catch { return {}; }
}

const getBaseUrl = () => (getBootstrapGlobals().baseUrl ?? '').replace(/\/$/, '');
const getApiBaseUrl = () => getBootstrapGlobals().webApiBaseUrl ?? `${getBaseUrl()}/api`;

export { getBaseUrl, getApiBaseUrl };
