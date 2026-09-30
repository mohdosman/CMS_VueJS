<script setup>
import { ref, useId } from 'vue';

// A section that opens and closes (the Blazor form used expansion panels). The heading holds the toggle button.
const props = defineProps({
    title: { type: String, required: true },
    open: { type: Boolean, default: true },
    level: { type: Number, default: 2 }
});
const expanded = ref(props.open);
const id = useId();
</script>

<template>
    <section class="card mb-2">
        <component :is="`h${level}`" class="card-header expand-header h6 mb-0 p-0">
            <button type="button" class="btn btn-link expand-toggle text-decoration-none w-100 text-start fw-bold" :aria-expanded="expanded" :aria-controls="id"
                    @click="expanded = !expanded">
                <i :class="expanded ? 'fa fa-chevron-down' : 'fa fa-chevron-right'" aria-hidden="true"></i> {{ title }}
            </button>
        </component>
        <div v-show="expanded" :id="id" class="card-body"><slot /></div>
    </section>
</template>

<style scoped>
/* The theme paints .card-header and .btn-link light on hover; keep the panel heading on the theme surface. */
.expand-header, .expand-header:hover { background: var(--bs-tertiary-bg, #f8f9fa); }
.expand-toggle.btn-link, .expand-toggle.btn-link:hover, .expand-toggle.btn-link:focus-visible { color: var(--bs-emphasis-color); background: transparent; }
.expand-toggle:hover { background: var(--bs-secondary-bg, #e9ecef); }
</style>
