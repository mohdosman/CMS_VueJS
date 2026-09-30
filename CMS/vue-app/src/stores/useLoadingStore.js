import { defineStore } from 'pinia';
import { ref } from 'vue';

// Global spinner counter (same as SafetyNet): every tracked request shows it, and it stays up
// until the last one finishes.
export const useLoadingStore = defineStore('loading', () => {
    const count = ref(0);
    const isLoading = ref(false);

    function show() {
        count.value++;
        isLoading.value = true;
    }

    function hide() {
        count.value = Math.max(0, count.value - 1);
        isLoading.value = count.value > 0;
    }

    return { isLoading, show, hide };
});
