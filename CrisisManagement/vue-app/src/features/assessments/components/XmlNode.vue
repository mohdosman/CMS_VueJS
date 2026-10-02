<script setup>
import { computed } from 'vue';

// Recursive collapsible XML element: native <details>, so no library and the browser's Ctrl+F opens collapsed matches.
const props = defineProps({ node: { type: Object, required: true }, depth: { type: Number, default: 0 } });

const name = computed(() => props.node.tagName);
const attrs = computed(() => Array.from(props.node.attributes).map((a) => ` ${a.name}="${a.value}"`).join(''));
const kids = computed(() => Array.from(props.node.children));
</script>

<template>
    <div v-if="!kids.length" class="xml-line">&lt;{{ name }}{{ attrs }}&gt;<span class="xml-text">{{ node.textContent }}</span>&lt;/{{ name }}&gt;</div>
    <details v-else :open="depth < 2">
        <summary class="xml-line">&lt;{{ name }}{{ attrs }}&gt;</summary>
        <div class="ms-4">
            <XmlNode v-for="(k, i) in kids" :key="i" :node="k" :depth="depth + 1" />
        </div>
        <div class="xml-line">&lt;/{{ name }}&gt;</div>
    </details>
</template>

<style scoped>
.xml-line { font-family: monospace; white-space: pre-wrap; overflow-wrap: anywhere; }
.xml-text { color: var(--bs-info, #0dcaf0); }
summary { cursor: pointer; }
</style>
