/**
 * Singleton screen reader live region — announces messages that have no
 * visible focus change (modal dismissals, async results, save confirmations).
 *
 * Prefers the #global-aria-status-message region rendered by _Navigation.cshtml
 * (the one the AngularJS shell wrote to) and creates its own only if absent.
 *
 * Usage: import { announce } from '@/services/liveAnnouncer.js'; announce('Saved');
 */
let region = null;

function ensureRegion() {
    if (region?.isConnected) return region;

    region = document.getElementById('global-aria-status-message');
    if (region) return region;

    region = document.createElement('div');
    region.setAttribute('aria-live', 'polite');
    region.setAttribute('aria-atomic', 'true');
    region.className = 'visually-hidden';
    document.body.appendChild(region);
    return region;
}

export function announce(message) {
    const el = ensureRegion();
    // Clear first so repeating the same message is re-announced.
    el.textContent = '';
    window.setTimeout(() => { el.textContent = message; }, 50);
}
