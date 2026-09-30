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
        <component :is="`h${level}`" class="card-header h6 mb-0 p-0">
            <button type="button" class="btn btn-link text-decoration-none w-100 text-start fw-bold" :aria-expanded="expanded" :aria-controls="id"
                    @click="expanded = !expanded">
                <i :class="expanded ? 'fa fa-chevron-down' : 'fa fa-chevron-right'" aria-hidden="true"></i> {{ title }}
            </button>
        </component>
        <div v-show="expanded" :id="id" class="card-body"><slot /></div>
    </section>
</template>
