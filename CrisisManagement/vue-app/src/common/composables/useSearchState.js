const MODE_KEY = 'mode';
const RESTORE_MODE = 'UPDATE';

/**
 * The other half of the protocol: a detail screen calls this before navigating
 * back, so the search screen restores the filters it saved.
 */
export function restoreSearchOnReturn() {
    sessionStorage.setItem(MODE_KEY, RESTORE_MODE);
}

/**
 * The criteria a search screen last saved, or null - for the detail screens
 * that seed a new record from whatever the user was searching for.
 */
export function readSearchCriteria(storageKey) {
    const stored = sessionStorage.getItem(storageKey);
    if (!stored) return null;
    try {
        return JSON.parse(stored);
    } catch {
        return null;
    }
}

/**
 * Persists search criteria across the search -> detail -> back navigation, honoring the
 * 'mode' = 'UPDATE' flag a detail screen sets when it returns to the list (the WebForms
 * Session["...SearchXML"] behaviour).
 *
 * On clear(), paging is reset to whatever shape it had when useSearchState() was first called.
 *
 *   const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 20 });
 *   const DEFAULT_CRITERIA = { name: '', orderBy: 'name', reverse: false };
 *   const criteria = reactive({ ...DEFAULT_CRITERIA });
 *   const { load, save, clear } = useSearchState('providerSearchJSON', criteria, DEFAULT_CRITERIA, paging);
 *
 * @param {string} storageKey - sessionStorage key the criteria are persisted under.
 * @param {object} criteria - reactive object holding the current search criteria.
 * @param {object} defaults - plain object criteria is reset to on load-with-no-stored-value / clear.
 * @param {object|null} paging - reactive paging object (currentPage, pageSize, ...), if this screen has paging.
 * @param {object} [options]
 * @param {string|null} [options.restoreMode] - load() only restores stored criteria when sessionStorage
 *        'mode' equals this value (set by restoreSearchOnReturn).
 * @param {boolean} [options.clearModeOnClear] - if true, clear() also removes the 'mode' key.
 * @param {object|null} [options.defaultPaging] - overrides the paging shape clear() resets to.
 */
export function useSearchState(storageKey, criteria, defaults, paging = null, options = {}) {
    const { restoreMode = RESTORE_MODE, clearModeOnClear = true, defaultPaging = null } = options;

    // Snapshot paging as it stands at setup time, so clear() can fall back to it.
    const initialPaging = paging ? { ...paging } : null;

    function applyDefaults() {
        Object.assign(criteria, structuredClone(defaults));   // a copy, so a list in the criteria is never shared with the defaults
    }

    function load() {
        const stored = sessionStorage.getItem(storageKey);
        const mode = sessionStorage.getItem(MODE_KEY);

        if (restoreMode !== null) {
            if (mode === restoreMode) {
                sessionStorage.removeItem(MODE_KEY);
                if (stored) Object.assign(criteria, JSON.parse(stored));
                else applyDefaults();
            } else {
                applyDefaults();
            }
            return;
        }

        if (stored) Object.assign(criteria, JSON.parse(stored));
        else applyDefaults();
    }

    function save() {
        sessionStorage.setItem(storageKey, JSON.stringify({ ...criteria }));
    }

    function clear() {
        sessionStorage.removeItem(storageKey);
        if (clearModeOnClear) sessionStorage.removeItem(MODE_KEY);
        applyDefaults();
        if (paging) {
            Object.assign(paging, initialPaging, defaultPaging);
            paging.currentPage = 1;
        }
    }

    return { load, save, clear };
}
