<script setup>
import { useId } from 'vue';

// A labelled text/number/date input with its error text. Numbers are kept as numbers ('' -> null).
const props = defineProps({
    modelValue: { type: [String, Number, null], default: '' },
    label: { type: String, required: true },
    type: { type: String, default: 'text' },
    error: { type: String, default: '' },
    required: Boolean,
    disabled: Boolean,
    maxlength: { type: Number, default: undefined },
    min: { type: Number, default: undefined },
    max: { type: Number, default: undefined },
    step: { type: String, default: undefined },
    hint: { type: String, default: '' },
    col: { type: String, default: 'col-md-4' }
});
const emit = defineEmits(['update:modelValue', 'touch']);
const id = useId();
const onInput = (e) => {
    const v = e.target.value;
    emit('update:modelValue', props.type === 'number' ? (v === '' ? null : Number(v)) : v);
};
</script>

<template>
    <div class="mb-3" :class="[col, { 'has-error': !!error }]" @focusout="!$event.currentTarget.contains($event.relatedTarget) && emit('touch')">
        <label class="form-label" :for="id">{{ label }} <span v-if="required" class="f_req" aria-hidden="true">*</span></label>
        <DateInput v-if="type === 'date'" :id="id" class="form-control form-control-sm" :model-value="modelValue ?? ''" :disabled="disabled"
                   :aria-required="required || undefined" :aria-invalid="!!error" :aria-describedby="`${hint ? `${id}-hint ` : ''}${id}-err`"
                   @update:model-value="(v) => emit('update:modelValue', v)" />
        <input v-else :id="id" class="form-control form-control-sm" :type="type" :value="modelValue ?? ''" :disabled="disabled" :maxlength="maxlength"
               :min="min" :max="max" :step="step" :aria-required="required || undefined" :aria-invalid="!!error"
               :aria-describedby="`${hint ? `${id}-hint ` : ''}${id}-err`" @input="onInput" />
        <div v-if="hint" :id="`${id}-hint`" class="form-text">{{ hint }}</div>
        <div :id="`${id}-err`" class="form-text has-error" role="alert">{{ error }}</div>
    </div>
</template>
