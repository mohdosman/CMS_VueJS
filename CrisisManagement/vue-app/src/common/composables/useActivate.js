import { ref, onMounted, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useLogger } from './useLogger.js';
import { useLoadingStore } from '../../stores/useLoadingStore.js';

/**
 * Runs a screen's page-load work once it mounts, and again when the route params change on a
 * reused component instance.
 *
 *   const { isActivating } = useActivate(async () => { ...load the page... });
 *
 * Shows the global spinner only if the work takes longer than 200ms, and reports a failure as a toast.
 */
export function useActivate(activateFn) {
    const isActivating = ref(false);
    const { logApiError } = useLogger();
    const loadingStore = useLoadingStore();
    const route = useRoute();

    async function run() {
        isActivating.value = true;
        let spinnerShown = false;
        const timer = setTimeout(() => {
            loadingStore.show();
            spinnerShown = true;
        }, 200);
        try {
            await activateFn();
        } catch (e) {
            logApiError(e);
        } finally {
            clearTimeout(timer);
            isActivating.value = false;
            if (spinnerShown) loadingStore.hide();
        }
    }

    onMounted(run);
    watch(() => route.params, run, { deep: true });

    return { isActivating };
}
