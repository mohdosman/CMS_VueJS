<script setup>
import { useLoadingStore } from '../../stores/useLoadingStore.js';

const loadingStore = useLoadingStore();
</script>

<template>
    <!-- Global "please wait" overlay, driven by the loading store (same as SafetyNet). -->
    <Transition name="dissolve">
        <div v-if="loadingStore.isLoading" class="page-splash" role="status" aria-live="polite">
            <div class="cms-spinner" aria-hidden="true"></div>
            <div class="page-splash-message page-splash-message-subtle">Please wait ...</div>
        </div>
    </Transition>
</template>

<style scoped>
.cms-spinner {
    width: 80px;
    height: 80px;
    border: 12px solid #C7D7F5;
    border-top-color: #F58A00;
    border-radius: 50%;
    animation: cms-spin 0.9s linear infinite;
    margin: 28% auto 20px;
}

@keyframes cms-spin {
    to { transform: rotate(360deg); }
}

.dissolve-enter-active,
.dissolve-leave-active {
    transition: opacity 0.8s linear;
}

/* While fading out the overlay must not swallow clicks meant for the page underneath. */
.dissolve-leave-active {
    pointer-events: none;
}

.dissolve-enter-from,
.dissolve-leave-to {
    opacity: 0;
}
</style>
