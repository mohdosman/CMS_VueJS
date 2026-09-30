<script setup>
import { computed } from 'vue';

// "Password must include:" list that ticks off as the user types (Blazor PasswordRequirementsChecklist).
// policy is the server's structured rules; confirm is optional and adds a "Passwords match" line.
const props = defineProps({
    password: { type: String, default: '' },
    confirm: { type: String, default: null },
    policy: { type: Object, default: null }   // { minLength, requireLowercase, requireUppercase, requireDigit, requireSpecial, uniqueChars }
});

const items = computed(() => {
    const p = props.policy;
    if (!p) return [];
    const v = props.password ?? '';
    const list = [{ label: `At least ${p.minLength} characters long`, met: v.length >= p.minLength }];
    if (p.requireLowercase) list.push({ label: 'At least one lower case letter', met: /[a-z]/.test(v) });
    if (p.requireUppercase) list.push({ label: 'At least one upper case letter', met: /[A-Z]/.test(v) });
    if (p.requireDigit) list.push({ label: 'At least one number', met: /\d/.test(v) });
    if (p.requireSpecial) list.push({ label: 'At least one special character', met: /[^A-Za-z0-9]/.test(v) });
    if (p.uniqueChars > 1) list.push({ label: `At least ${p.uniqueChars} different characters`, met: new Set(v).size >= p.uniqueChars });
    if (props.confirm !== null) list.push({ label: 'Passwords match', met: v.length > 0 && v === props.confirm });
    return list;
});
</script>

<template>
    <div v-if="items.length" aria-live="polite">
        <div class="form-text mb-1">Password must include:</div>
        <ul class="list-unstyled mb-0">
            <li v-for="i in items" :key="i.label" class="form-text mt-0" :class="{ 'text-success': i.met }">
                <i :class="i.met ? 'fa fa-check-circle' : 'fa fa-circle-o'" aria-hidden="true"></i>
                <span class="visually-hidden">{{ i.met ? 'Met: ' : 'Not met: ' }}</span>
                {{ i.label }}
            </li>
        </ul>
    </div>
</template>
