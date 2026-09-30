<script setup>
    import { computed } from 'vue';

    // Sortable results-grid column header. Replaces the hand-written
    // <th>/<button>/<i> block that every search and detail grid repeated.
    const props = defineProps({
        col: { type: String, required: true },
        sortIcon: { type: Function, required: true },
        disabled: { type: Boolean, default: false }
    });
    defineEmits(['sort']);

    const icon = computed(() => props.sortIcon(props.col));
    const direction = computed(() =>
        icon.value ? (icon.value === 'fa fa-sort-down' ? 'descending' : 'ascending') : ''
    );
</script>

<template>
    <th scope="col" :aria-sort="direction || undefined">
        <button type="button" class="sort-link" :disabled="disabled" @click="$emit('sort', col)">
            <slot /> <i :class="icon" aria-hidden="true"></i>
            <span v-if="direction" class="visually-hidden">(sorted {{ direction }})</span>
        </button>
    </th>
</template>
