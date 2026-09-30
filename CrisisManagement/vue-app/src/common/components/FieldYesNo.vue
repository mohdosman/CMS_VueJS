<script setup>
import { useId } from 'vue';

// A Yes / No choice (true / false; null until chosen) as a radio group.
defineProps({
    modelValue: { type: [Boolean, null], default: null },
    label: { type: String, required: true },
    error: { type: String, default: '' },
    disabled: Boolean,
    col: { type: String, default: 'col-md-4' }
});
defineEmits(['update:modelValue']);
const id = useId();
</script>

<template>
    <fieldset class="mb-3 border-0 p-0" :class="col" :disabled="disabled" :aria-describedby="`${id}-err`">
        <legend class="form-label mb-1" style="font-size: inherit; float: none; width: auto">{{ label }}</legend>
        <label class="checkbox-inline me-3"><input type="radio" :name="id" :checked="modelValue === true" @change="$emit('update:modelValue', true)" /> Yes</label>
        <label class="checkbox-inline"><input type="radio" :name="id" :checked="modelValue === false" @change="$emit('update:modelValue', false)" /> No</label>
        <div :id="`${id}-err`" class="form-text has-error" role="alert">{{ error }}</div>
    </fieldset>
</template>
