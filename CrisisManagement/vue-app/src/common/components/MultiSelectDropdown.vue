<script setup>
import { useMultiSelectDropdown } from '../composables/useMultiSelectDropdown.js';

const props = defineProps({
    id: { type: String, required: true },
    label: { type: String, required: true },
    options: { type: Array, required: true },       // [{ id, label, short? }]
    modelValue: { type: Array, required: true },    // selected ids
    disabled: { type: Boolean, default: false }
});
const emit = defineEmits(['update:modelValue']);

const { root, text, isOn, toggle, clear, close, onKey } = useMultiSelectDropdown(props, emit);
</script>

<template>
    <div class="msd">
        <span :id="`${id}-label`" class="form-label d-block">{{ label }}</span>
        <details ref="root" class="msd-box" :class="{ 'msd-disabled': disabled }" @keydown="onKey"
                 @click="disabled && $event.preventDefault()">
            <summary :id="id" class="form-select form-select-sm" :aria-labelledby="`${id}-label ${id}-value`"
                     :aria-disabled="disabled || undefined" :title="text">
                <span :id="`${id}-value`" class="msd-text">{{ text }}</span>
            </summary>
            <div class="msd-panel" role="group" :aria-labelledby="`${id}-label`">
                <label v-for="o in options" :key="o.id" class="msd-item">
                    <input type="checkbox" :checked="isOn(o.id)" :disabled="disabled" @change="toggle(o.id, $event.target.checked)" />
                    {{ o.label }}<span v-if="o.short" class="text-muted"> ({{ o.short }})</span>
                </label>
                <button v-if="modelValue.length && !disabled" type="button" class="btn btn-link btn-sm p-0 mt-1" @click="clear">Clear</button>
            </div>
        </details>
    </div>
</template>

<style scoped>
.msd-box { position: relative; }
.msd-box > summary { list-style: none; cursor: pointer; }
.msd-box > summary::-webkit-details-marker { display: none; }
.msd-disabled > summary { cursor: default; opacity: .6; }
.msd-text { display: block; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.msd-panel {
    position: absolute; z-index: 20; left: 0; right: 0; max-height: 16rem; overflow: auto;
    margin-top: 2px; padding: 6px 10px;
    background: var(--cms-surface, #fff); color: inherit;
    border: 1px solid #ced4da; border-radius: 4px; box-shadow: 0 4px 12px rgba(0, 0, 0, .15);
}
.msd-item { display: block; font-size: 13px; padding: 2px 0; cursor: pointer; }
</style>
