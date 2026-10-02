<script setup>
import { usePermissionPicker } from '../composables/usePermissionPicker.js';

const props = defineProps({
    groups: { type: Array, required: true },      // [{ groupName, items: [{ name, value, description }] }]
    modelValue: { type: Array, required: true },  // selected permission values
    disabled: { type: Boolean, default: false }
});
const emit = defineEmits(['update:modelValue']);

const {
    search, selectedOnly, visibleGroups, has, set, setPair, isOpen, toggleGroup, expandAll, collapseAll, selectGroup, clearGroup
} = usePermissionPicker(props, emit);
</script>

<template>
    <div>
        <div class="d-flex flex-wrap align-items-end gap-3 mb-2">
            <div class="flex-grow-1" style="max-width: 28rem">
                <label class="form-label" for="permSearch">Find a permission</label>
                <input id="permSearch" v-model="search" type="search" class="form-control form-control-sm"
                       aria-describedby="permSearchHelp" />
                <div id="permSearchHelp" class="form-text">Searches name, value and description.</div>
            </div>
            <label class="checkbox-inline"><input v-model="selectedOnly" type="checkbox" /> Show selected only</label>
            <div class="capability-tree-toolbar mb-0">
                <button type="button" class="btn btn-outline-secondary btn-xs" @click="expandAll">
                    <i class="fa fa-plus" aria-hidden="true"></i> Expand All
                </button>
                <button type="button" class="btn btn-outline-secondary btn-xs" @click="collapseAll">
                    <i class="fa fa-minus" aria-hidden="true"></i> Collapse All
                </button>
            </div>
        </div>

        <p v-if="!visibleGroups.length" class="text-muted">No permission groups match the current filters.</p>

        <div class="role-capability-tree">
            <div v-for="vg in visibleGroups" :key="vg.g.groupName" class="card capability-section">
                <button type="button" class="card-header capability-section-header" :title="vg.g.groupName"
                        :aria-expanded="isOpen(vg.g.groupName)" :aria-controls="`perm-group-${vg.key}`"
                        @click="toggleGroup(vg.g.groupName)">
                    <span class="capability-section-title">
                        <i :class="isOpen(vg.g.groupName) ? 'fa fa-chevron-down' : 'fa fa-chevron-right'" aria-hidden="true"></i>
                        {{ vg.g.groupName }}
                    </span>
                    <span class="badge capability-section-count">{{ vg.selected }}/{{ vg.g.items.length }} selected</span>
                </button>

                <div v-show="isOpen(vg.g.groupName)" :id="`perm-group-${vg.key}`" class="card-body capability-section-body">
                    <div v-if="!disabled" class="mb-2">
                        <AppButton action="cancel" size="xs" @click="selectGroup(vg)">Select all<span class="visually-hidden"> in {{ vg.g.groupName }}</span></AppButton>
                        <AppButton action="cancel" size="xs" @click="clearGroup(vg)">Clear<span class="visually-hidden"> {{ vg.g.groupName }}</span></AppButton>
                    </div>

                    <div v-if="vg.unpaired.length" class="capability-grid">
                        <label v-for="i in vg.unpaired" :key="i.value" class="permission-cell" :title="i.description">
                            <input type="checkbox" class="permission-switch-input" role="switch" :checked="has(i.value)"
                                   :disabled="disabled" @change="set([i.value], $event.target.checked)" />
                            <span class="permission-switch" aria-hidden="true"></span>
                            <span class="permission-name">{{ i.name }}</span>
                        </label>
                    </div>

                    <!-- Per-report permissions collapsed into one row with View/Manage switches -->
                    <div v-if="vg.pairs.length" class="permission-pair-table">
                        <div class="pair-row pair-head">
                            <span>Permission</span>
                            <span class="pair-col">View</span>
                            <span class="pair-col">Manage</span>
                        </div>
                        <div v-for="p in vg.pairs" :key="p.label" class="pair-row">
                            <span class="pair-label">{{ p.label }}</span>
                            <span v-for="action in ['view', 'manage']" :key="action" class="pair-cell">
                                <label v-if="p[action]" class="pair-toggle" :title="p[action].description">
                                    <input type="checkbox" class="permission-switch-input" role="switch"
                                           :checked="has(p[action].value)" :disabled="disabled"
                                           @change="setPair(p[action], p[action === 'view' ? 'manage' : 'view'], $event.target.checked)" />
                                    <span class="permission-switch" aria-hidden="true"></span>
                                    <span class="visually-hidden">{{ action }} {{ p.label }}</span>
                                </label>
                                <span v-else class="pair-none" aria-hidden="true">&mdash;</span>
                            </span>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped src="../assets/role-details.css"></style>
