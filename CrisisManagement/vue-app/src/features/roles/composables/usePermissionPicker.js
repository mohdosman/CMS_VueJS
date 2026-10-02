import { ref, reactive, computed } from 'vue';
import { announce } from '../../../services/liveAnnouncer.js';

// The permission catalog as collapsible group cards with a switch per permission (SafetyNet role screen).
// "View: x" / "Manage: x" permissions are shown as one row with two switches, because manage implies view,
// so a row holds at most one of them.
export function usePermissionPicker(props, emit) {
    // ================================================================
    // State
    // ================================================================
    const search = ref('');
    const selectedOnly = ref(false);
    const openGroups = reactive({});

    const selected = computed(() => new Set(props.modelValue));
    const has = (value) => selected.value.has(value);

    // ================================================================
    // Selecting
    // ================================================================
    function set(values, on) {
        const next = new Set(props.modelValue);
        for (const value of values) {
            if (on) {
                next.add(value);
            } else {
                next.delete(value);
            }
        }
        emit('update:modelValue', [...next]);
    }

    // Turning one of a View/Manage pair on turns the other off.
    function setPair(item, other, on) {
        const next = new Set(props.modelValue);
        if (on) {
            next.add(item.value);
            if (other) {
                next.delete(other.value);
            }
        } else {
            next.delete(item.value);
        }
        emit('update:modelValue', [...next]);
    }

    // ================================================================
    // Filtering and grouping
    // ================================================================
    function matches(item) {
        const term = search.value.trim().toLowerCase();
        return !term || [item.name, item.value, item.description].some((text) => (text ?? '').toLowerCase().includes(term));
    }

    const visibleItems = (group) => group.items.filter((item) => (!selectedOnly.value || has(item.value)) && matches(item));

    // Splits a group's items into View/Manage pairs and the rest.
    function rows(items) {
        const byLabel = new Map();
        const unpaired = [];
        for (const item of items) {
            const match = /^(view|manage):\s*(.+)$/i.exec(item.name);
            if (!match) {
                unpaired.push(item);
                continue;
            }
            const label = match[2].trim();
            if (!byLabel.has(label)) {
                byLabel.set(label, { label, view: null, manage: null });
            }
            byLabel.get(label)[match[1].toLowerCase()] = item;
        }
        return { pairs: [...byLabel.values()], unpaired };
    }

    const visibleGroups = computed(() =>
        props.groups
            .map((group) => ({ g: group, items: visibleItems(group) }))
            .filter((entry) => entry.items.length)
            .map((entry, index) => ({
                ...entry, ...rows(entry.items), key: index, selected: entry.g.items.filter((item) => has(item.value)).length
            })));

    // ================================================================
    // Opening and closing groups
    // ================================================================
    // A group is open when toggled open, or while searching / filtering (so matches are visible).
    const filtering = computed(() => !!search.value.trim() || selectedOnly.value);
    const isOpen = (name) => filtering.value || !!openGroups[name];

    function toggleGroup(name) {
        if (!filtering.value) {
            openGroups[name] = !openGroups[name];
        }
    }

    function expandAll() {
        props.groups.forEach((group) => { openGroups[group.groupName] = true; });
        announce('All groups expanded');
    }

    function collapseAll() {
        props.groups.forEach((group) => { openGroups[group.groupName] = false; });
        announce('All groups collapsed');
    }

    // Select all prefers View on a pair, keeping the pair mutually exclusive.
    function selectGroup(visibleGroup) {
        const next = new Set(props.modelValue);
        visibleGroup.unpaired.forEach((item) => next.add(item.value));
        visibleGroup.pairs.forEach((pair) => {
            const keep = pair.view ?? pair.manage;
            next.add(keep.value);
            if (pair.view && pair.manage) {
                next.delete(pair.manage.value);
            }
        });
        emit('update:modelValue', [...next]);
    }

    function clearGroup(visibleGroup) {
        set(visibleGroup.items.map((item) => item.value), false);
    }

    return {
        // Filters
        search, selectedOnly,

        // Groups
        visibleGroups,

        // Actions
        has, set, setPair, isOpen, toggleGroup, expandAll, collapseAll, selectGroup, clearGroup
    };
}
