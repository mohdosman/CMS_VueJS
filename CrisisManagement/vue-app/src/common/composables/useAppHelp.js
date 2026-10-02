import { ref, onMounted } from 'vue';

// The navbar is server-rendered (outside #vue-app), so its Help button reaches the SPA through
// window.cms.openHelp, the same bridge SafetyNet uses.
export function useAppHelp() {
    const helpOpen = ref(false);

    onMounted(() => {
        window.cms = { openHelp: () => { helpOpen.value = true; } };
    });

    return {
        helpOpen
    };
}
