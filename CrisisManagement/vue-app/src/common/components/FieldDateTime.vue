<script setup>
import { useId } from 'vue';

// A date and a time input side by side for one date-time value; v-model is { date: 'yyyy-mm-dd', time: 'hh:mm' }.
const props = defineProps({
    modelValue: { type: Object, required: true },
    label: { type: String, required: true },
    error: { type: String, default: '' },
    required: Boolean,
    disabled: Boolean,
    col: { type: String, default: 'col-md-6' }
});
const emit = defineEmits(['update:modelValue']);
const id = useId();
const set = (part, e) => emit('update:modelValue', { ...props.modelValue, [part]: e.target.value });
</script>

<template>
    <div class="mb-3" :class="col">
        <div class="row g-1">
            <div class="col-7">
                <label class="form-label" :for="`${id}-date`">{{ label }} date <span v-if="required" class="f_req" aria-hidden="true">*</span></label>
                <DateInput :id="`${id}-date`" class="form-control form-control-sm" :model-value="modelValue.date" :disabled="disabled"
                       :aria-invalid="!!error" :aria-describedby="`${id}-err`" @update:model-value="(v) => emit('update:modelValue', { ...modelValue, date: v })" />
            </div>
            <div class="col-5">
                <label class="form-label" :for="`${id}-time`">{{ label }} time</label>
                <input :id="`${id}-time`" type="time" class="form-control form-control-sm" :value="modelValue.time" :disabled="disabled"
                       :aria-invalid="!!error" :aria-describedby="`${id}-err`" @input="set('time', $event)" />
            </div>
        </div>
        <div :id="`${id}-err`" class="form-text has-error" role="alert">{{ error }}</div>
    </div>
</template>
