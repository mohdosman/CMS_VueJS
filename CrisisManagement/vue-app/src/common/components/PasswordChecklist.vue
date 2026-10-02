<script setup>
import { usePasswordChecklist } from '../composables/usePasswordChecklist.js';

const props = defineProps({
    password: { type: String, default: '' },
    confirm: { type: String, default: null },
    policy: { type: Object, default: null }   // { minLength, requireLowercase, requireUppercase, requireDigit, requireSpecial, uniqueChars }
});

const { items } = usePasswordChecklist(props);
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
