<script setup>
import { useId } from 'vue';

// A labelled drop-down over lookup items ({ id, label }) with its error text; v-model holds the chosen id (null = none).
defineProps({
    modelValue: { type: [Number, String, null], default: null },
    label: { type: String, required: true },
    options: { type: Array, default: () => [] },
    error: { type: String, default: '' },
    required: Boolean,
    disabled: Boolean,
    placeholder: { type: String, default: '- - SELECT - -' },
    col: { type: String, default: 'col-md-4' }
});
defineEmits(['update:modelValue']);
const id = useId();
</script>

<template>
    <div class="mb-3" :class="[col, { 'has-error': !!error }]">
        <label class="form-label" :for="id">{{ label }} <span v-if="required" class="f_req" aria-hidden="true">*</span></label>
        <select :id="id" class="form-select form-select-sm" :value="modelValue" :disabled="disabled" :aria-required="required || undefined"
                :aria-invalid="!!error" :aria-describedby="`${id}-err`"
                @change="$emit('update:modelValue', $event.target.value === '' ? null : Number($event.target.value))">
            <option value="">{{ placeholder }}</option>
            <option v-for="o in options" :key="o.id" :value="o.id">{{ o.label }}</option>
        </select>
        <div :id="`${id}-err`" class="form-text has-error" role="alert">{{ error }}</div>
    </div>
</template>
