<script setup>
import { ref } from 'vue';
import { iconClass } from '../icons.js';

// One menu item with its sub-menus (recursive). Roots and the first level start open.
const props = defineProps({
    node: { type: Object, required: true },
    depth: { type: Number, default: 0 },
    canEdit: { type: Boolean, default: false },
    forceOpen: { type: Boolean, default: false }   // while filtering, so matches are visible
});
const emit = defineEmits(['edit', 'add-child', 'remove']);

const open = ref(props.depth < 2);
const isOpen = () => props.forceOpen || open.value;
</script>

<template>
    <li class="menu-node">
        <div class="menu-row d-flex align-items-center gap-2 py-1">
            <button v-if="node.children.length" type="button" class="btn btn-link btn-sm p-0 menu-toggle" :aria-expanded="isOpen()"
                    :aria-label="`${isOpen() ? 'Collapse' : 'Expand'} ${node.name}`" @click="open = !open">
                <i :class="isOpen() ? 'fa fa-chevron-down' : 'fa fa-chevron-right'" aria-hidden="true"></i>
            </button>
            <span v-else class="menu-toggle" aria-hidden="true"></span>

            <i v-if="iconClass(node.icon)" :class="['fa', iconClass(node.icon)]" aria-hidden="true"></i>
            <router-link :to="`/admin/menus/${node.id}`" class="fw-semibold">{{ node.name }}</router-link>
            <span class="badge text-bg-light border" :title="`Display order ${node.displaySequence}`">{{ node.displaySequence }}</span>
            <span v-if="node.isAlwaysEnabled" class="badge text-bg-success">Always</span>
            <span v-if="!node.isEnabled" class="badge text-bg-secondary">Disabled</span>
            <span v-if="node.url" class="text-muted small">{{ node.url }}</span>

            <span class="ms-auto text-nowrap">
                <AppButton v-if="canEdit" action="cancel" size="xs" @click="emit('add-child', node)">Add<span class="visually-hidden"> sub-menu under {{ node.name }}</span></AppButton>
                <AppButton action="cancel" size="xs" @click="emit('edit', node)">{{ canEdit ? 'Edit' : 'View' }}<span class="visually-hidden"> {{ node.name }}</span></AppButton>
                <AppButton v-if="canEdit" action="cancel" size="xs" @click="emit('remove', node)">Delete<span class="visually-hidden"> {{ node.name }}</span></AppButton>
            </span>
        </div>

        <ul v-if="node.children.length" v-show="isOpen()" class="list-unstyled ms-4 mb-0">
            <MenuTreeNode v-for="c in node.children" :key="c.id" :node="c" :depth="depth + 1" :can-edit="canEdit" :force-open="forceOpen"
                          @edit="emit('edit', $event)" @add-child="emit('add-child', $event)" @remove="emit('remove', $event)" />
        </ul>
    </li>
</template>

<style scoped>
.menu-toggle { width: 1rem; display: inline-block; text-align: center; }
.menu-row { border-bottom: 1px solid var(--bs-border-color, #dee2e6); }
</style>
