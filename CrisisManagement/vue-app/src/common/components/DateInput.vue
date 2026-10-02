<script setup>
import { useDateInput } from '../composables/useDateInput.js';

// Attributes (id, class, aria-*, disabled) go to the input.
defineOptions({ inheritAttrs: false });
const props = defineProps({ modelValue: { type: String, default: '' } });
const emit = defineEmits(['update:modelValue']);

const { text, focused, onBlur, onInput } = useDateInput(props, emit);
</script>

<template>
    <!-- The label is the parent's: its for= points at the id passed through $attrs. -->
    <!-- eslint-disable-next-line vuejs-accessibility/form-control-has-label -->
    <input v-bind="$attrs" type="text" inputmode="numeric" autocomplete="off" placeholder="MM/DD/YYYY" :value="text"
           @focus="focused = true" @input="onInput" @blur="onBlur" />
</template>
