<script setup>
import { ref, computed } from 'vue';
import { announce } from '../../../services/liveAnnouncer.js';

// The permission catalog as collapsible groups. "View: x" / "Manage: x" permissions are shown as one
// row with two boxes, because manage implies view, so a row holds at most one of them.
const props = defineProps({
    groups: { type: Array, required: true },      // [{ groupName, items: [{ name, value, description }] }]
    modelValue: { type: Array, required: true },  // selected permission values
    disabled: { type: Boolean, default: false }
});
const emit = defineEmits(['update:modelValue']);

const search = ref('');
const selectedOnly = ref(false);
const expanded = ref(new Set());

const selected = computed(() => new Set(props.modelValue));
const has = (v) => selected.value.has(v);

function set(values, on) {
    const next = new Set(props.modelValue);
    for (const v of values) (on ? next.add(v) : next.delete(v));
    emit('update:modelValue', [...next]);
}

// Checking one of a View/Manage pair unchecks the other.
function setPair(item, other, on) {
    const next = new Set(props.modelValue);
    if (on) { next.add(item.value); if (other) next.delete(other.value); } else next.delete(item.value);
    emit('update:modelValue', [...next]);
}

function matches(i) {
    const t = search.value.trim().toLowerCase();
    return !t || [i.name, i.value, i.description].some((s) => (s ?? '').toLowerCase().includes(t));
}
const visibleItems = (g) => g.items.filter((i) => (!selectedOnly.value || has(i.value)) && matches(i));

function rows(items) {
    const byLabel = new Map();
    const unpaired = [];
    for (const item of items) {
        const m = /^(view|manage):\s*(.+)$/i.exec(item.name);
        if (!m) { unpaired.push(item); continue; }
        const label = m[2].trim();
        if (!byLabel.has(label)) byLabel.set(label, { label, view: null, manage: null });
        byLabel.get(label)[m[1].toLowerCase()] = item;
    }
    return { pairs: [...byLabel.values()], unpaired };
}

const visibleGroups = computed(() =>
    props.groups
        .map((g) => ({ g, items: visibleItems(g) }))
        .filter((x) => x.items.length)
        .map((x) => ({ ...x, ...rows(x.items), selected: x.g.items.filter((i) => has(i.value)).length })));

// A group is open when expanded, or while searching / filtering (so matches are visible).
const filtering = computed(() => !!search.value.trim() || selectedOnly.value);
const isOpen = (name) => filtering.value || expanded.value.has(name);
function onToggle(name, e) {
    if (filtering.value) return;
    const next = new Set(expanded.value);
    if (e.target.open) next.add(name); else next.delete(name);
    expanded.value = next;
}
const expandAll = () => { expanded.value = new Set(props.groups.map((g) => g.groupName)); announce('All groups expanded'); };
const collapseAll = () => { expanded.value = new Set(); announce('All groups collapsed'); };

// Select all prefers View on a pair, keeping the pair mutually exclusive.
function selectGroup(vg) {
    const values = [...vg.unpaired.map((i) => i.value), ...vg.pairs.map((p) => (p.view ?? p.manage).value)];
    const others = vg.pairs.filter((p) => p.view && p.manage).map((p) => p.manage.value);
    const next = new Set(props.modelValue);
    values.forEach((v) => next.add(v));
    others.forEach((v) => next.delete(v));
    emit('update:modelValue', [...next]);
}
const clearGroup = (vg) => set(vg.items.map((i) => i.value), false);
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
            <div>
                <AppButton action="cancel" @click="expandAll">Expand all</AppButton>
                <AppButton action="cancel" @click="collapseAll">Collapse all</AppButton>
            </div>
        </div>

        <p v-if="!visibleGroups.length" class="text-muted">No permission groups match the current filters.</p>

        <details v-for="vg in visibleGroups" :key="vg.g.groupName" class="perm-group mb-2"
                 :open="isOpen(vg.g.groupName)" @toggle="onToggle(vg.g.groupName, $event)">
            <summary>
                <strong>{{ vg.g.groupName }}</strong>
                <span class="text-muted small"> {{ vg.selected }}/{{ vg.g.items.length }} selected</span>
            </summary>

            <div class="ps-3 pt-2">
                <div v-if="!disabled" class="mb-2">
                    <AppButton action="cancel" @click="selectGroup(vg)">Select all<span class="visually-hidden"> in {{ vg.g.groupName }}</span></AppButton>
                    <AppButton action="cancel" @click="clearGroup(vg)">Clear<span class="visually-hidden"> {{ vg.g.groupName }}</span></AppButton>
                </div>

                <div v-for="i in vg.unpaired" :key="i.value" class="mb-1">
                    <label class="checkbox-inline" :title="i.description">
                        <input type="checkbox" :checked="has(i.value)" :disabled="disabled" @change="set([i.value], $event.target.checked)" />
                        {{ i.name }}
                    </label>
                    <div v-if="i.description" class="form-text ms-4 mt-0">{{ i.description }}</div>
                </div>

                <table v-if="vg.pairs.length" class="table table-sm table-borderless w-auto mb-1">
                    <thead>
                        <tr><th scope="col">Permission</th><th scope="col" class="text-center">View</th><th scope="col" class="text-center">Manage</th></tr>
                    </thead>
                    <tbody>
                        <tr v-for="p in vg.pairs" :key="p.label">
                            <th scope="row" class="fw-normal">{{ p.label }}</th>
                            <td class="text-center">
                                <input v-if="p.view" type="checkbox" :checked="has(p.view.value)" :disabled="disabled"
                                       :aria-label="`View ${p.label}`" @change="setPair(p.view, p.manage, $event.target.checked)" />
                                <span v-else aria-hidden="true">&mdash;</span>
                            </td>
                            <td class="text-center">
                                <input v-if="p.manage" type="checkbox" :checked="has(p.manage.value)" :disabled="disabled"
                                       :aria-label="`Manage ${p.label}`" @change="setPair(p.manage, p.view, $event.target.checked)" />
                                <span v-else aria-hidden="true">&mdash;</span>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </details>
    </div>
</template>
