<script setup>
import { ref, watch } from 'vue';

// A date typed as MM/DD/YYYY, the way SafetyNet's v-cc-valid-date fields work: on blur "T" is today, MMDDYY and MMDDYYYY
// are spread out, "-", "." and spaces become "/", a two-digit year gets the current century, a missing year the current year, and
// a date that does not exist is cleared. The model is the ISO date (yyyy-MM-dd) or "", so the API and the forms keep their shape.
// Attributes (id, class, aria-*, disabled) go to the input.
defineOptions({ inheritAttrs: false });
const props = defineProps({ modelValue: { type: String, default: '' } });
const emit = defineEmits(['update:modelValue']);

const pad = (n) => String(n).padStart(2, '0');
const toText = (iso) => {
    const m = /^(\d{4})-(\d{2})-(\d{2})/.exec(iso ?? '');
    return m ? `${+m[2]}/${+m[3]}/${m[1]}` : '';
};

const text = ref(toText(props.modelValue));
const focused = ref(false);

// A change from outside (a reset, a loaded record) shows immediately; typing is left alone.
watch(() => props.modelValue, (v) => { if (!focused.value || toText(v) !== text.value) text.value = toText(v); });

// The typed text as an ISO date, or null when it is not a real date.
function parse(raw) {
    let value = raw.trim();
    if (!value) return '';
    const now = new Date();
    if (value.toUpperCase() === 'T') value = `${now.getMonth() + 1}/${now.getDate()}/${now.getFullYear()}`;
    if (/^(\d{6}|\d{8})$/.test(value)) value = `${value.slice(0, 2)}/${value.slice(2, 4)}/${value.slice(4)}`;
    let [month = '', day = '', year = ''] = value.replace(/[-.\s_]/g, '/').split('/');
    if (year.length === 2) year = String(now.getFullYear()).slice(0, 2) + year;
    if (year.length === 0) year = String(now.getFullYear());
    if (!/^\d{1,2}$/.test(month) || !/^\d{1,2}$/.test(day) || !/^\d{4}$/.test(year)) return null;
    const d = new Date(+year, +month - 1, +day);
    if (d.getFullYear() !== +year || d.getMonth() !== +month - 1 || d.getDate() !== +day) return null;
    return `${year}-${pad(month)}-${pad(day)}`;
}

function onBlur() {
    focused.value = false;
    const iso = parse(text.value);
    text.value = iso ? toText(iso) : '';
    emit('update:modelValue', iso ?? '');
}

// Typing a complete date updates the model at once, so a Save right after typing (Enter) sends it.
function onInput(e) {
    text.value = e.target.value;
    const iso = parse(text.value);
    if (iso !== null && /^\d{1,2}\/\d{1,2}\/\d{4}$/.test(text.value.trim())) emit('update:modelValue', iso);
}
</script>

<template>
    <!-- The label is the parent's: its for= points at the id passed through $attrs. -->
    <!-- eslint-disable-next-line vuejs-accessibility/form-control-has-label -->
    <input v-bind="$attrs" type="text" inputmode="numeric" autocomplete="off" placeholder="MM/DD/YYYY" :value="text"
           @focus="focused = true" @input="onInput" @blur="onBlur" />
</template>
