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
            <div class="card-header d-flex justify-content-between align-items-center">
                <h2 :id="titleId" class="h6 mb-0">{{ title }}</h2>
                <button type="button" class="app-dialog-x" aria-label="Close" @click="emit('close')"><i class="fa fa-times" aria-hidden="true"></i></button>
            </div>
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
.app-dialog-x { background: none; border: 0; color: inherit; font-size: 1.1rem; line-height: 1; padding: .25rem .4rem; opacity: .7; }
.app-dialog-x:hover, .app-dialog-x:focus-visible { opacity: 1; }
.app-dialog::backdrop { background: rgba(0, 0, 0, .45); }
</style>
