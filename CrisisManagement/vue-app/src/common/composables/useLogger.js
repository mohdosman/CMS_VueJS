import { useToast } from 'vue-toastification';
import { apiErrorMessage } from '../../utils/apiError.js';
import { announce } from '../../services/liveAnnouncer.js';

// User feedback: a toast, also spoken through the shared live region (same as SafetyNet).
// vue-toastification's own role="alert" is turned off in main.js (accessibility.toastRole), because a
// node inserted with its text already in place is announced unreliably - and with both on, screen
// readers read each save/delete twice.
export function useLogger() {
    const toast = useToast();

    function show(level, msg) {
        announce(msg);
        return toast[level](msg);
    }

    return {
        logSuccess: (msg) => show('success', msg),
        logError: (msg) => show('error', msg),
        // Same toast, but reads the message out of an axios error first.
        logApiError: (error, { fallback } = {}) => show('error', apiErrorMessage(error, fallback)),
        logWarning: (msg) => show('warning', msg),
        log: (msg) => show('info', msg)
    };
}
