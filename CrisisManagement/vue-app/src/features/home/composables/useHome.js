import { computed } from 'vue';
import { useAppStore } from '../../../stores/useAppStore.js';

// The welcome page: the signed-in user's name and roles.
export function useHome() {
    const appStore = useAppStore();

    const currentUser = computed(() => appStore.currentUser);

    return {
        currentUser
    };
}
