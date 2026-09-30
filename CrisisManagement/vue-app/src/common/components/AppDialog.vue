<script setup>
import { ref, onMounted, onBeforeUnmount, useId } from 'vue';

// Modal built on the native <dialog>: focus is trapped and Esc closes it without extra code.
// Render it with v-if; it opens on mount and hands focus back to whatever opened it on unmount.
defineProps({ title: { type: String, required: true }, wide: Boolean });
const emit = defineEmits(['close']);

const dialog = ref(null);
const titleId = useId();
const opener = document.activeElement;

onMounted(() => dialog.value.showModal());
// Close first: while the modal is open the rest of the page is inert and cannot take focus.
onBeforeUnmount(() => {
    dialog.value?.close();
    opener?.focus?.();
});
</script>

<template>
    <dialog ref="dialog" class="app-dialog" :class="{ 'app-dialog-wide': wide }" :aria-labelledby="titleId"
            @cancel.prevent="emit('close')" @click.self="emit('close')">
        <div class="card border-0">
            <div class="card-header"><h2 :id="titleId" class="h6 mb-0">{{ title }}</h2></div>
            <div class="card-body"><slot /></div>
        </div>
    </dialog>
</template>

<style scoped>
.app-dialog {
    width: 32rem;
    max-width: 95vw;
    padding: 0;
    border: 1px solid var(--bs-border-color, #ccc);
    border-radius: 6px;
}
.app-dialog-wide { width: 60rem; }
.app-dialog::backdrop { background: rgba(0, 0, 0, .45); }
</style>
