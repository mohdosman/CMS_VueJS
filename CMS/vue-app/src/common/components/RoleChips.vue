<script setup>
// Multi-select as SafetyNet's role chips: a keyboard-operable checkbox per option.
const props = defineProps({
    options: { type: Array, required: true },       // [{ id, label }]
    modelValue: { type: Array, required: true },    // selected ids
    label: { type: String, required: true },
    hint: { type: String, default: '' },
    disabled: { type: Boolean, default: false }
});
const emit = defineEmits(['update:modelValue']);

const isOn = (id) => props.modelValue.includes(id);
function toggle(id) {
    if (props.disabled) return;
    emit('update:modelValue', isOn(id) ? props.modelValue.filter((x) => x !== id) : [...props.modelValue, id]);
}
</script>

<template>
    <div class="role-section" role="group" :aria-label="label">
        <span class="role-section-label" aria-hidden="true">{{ label }}</span>
        <div class="row role-chip-grid">
            <div v-for="o in options" :key="o.id" class="col-6 col-sm-4 col-md-3 col-lg-2 role-chip-cell">
                <span class="role-chip" :class="{ active: isOn(o.id) }" role="checkbox" tabindex="0" :aria-checked="isOn(o.id)"
                      :aria-disabled="disabled || undefined" :style="disabled ? 'opacity:.6;cursor:default' : ''"
                      @click="toggle(o.id)" @keydown.space.prevent="toggle(o.id)" @keydown.enter.prevent="toggle(o.id)">
                    <span class="role-chip-check">
                        <svg v-if="isOn(o.id)" width="8" height="8" viewBox="0 0 8 8" fill="none" aria-hidden="true">
                            <polyline points="1,4 3,6 7,2" stroke="white" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
                        </svg>
                    </span>
                    <span class="role-chip-label" :title="o.label">{{ o.label }}</span>
                </span>
            </div>
        </div>
        <div v-if="hint" class="role-hint">{{ hint }}</div>
    </div>
</template>

<style scoped src="../assets/role-chips.css"></style>
