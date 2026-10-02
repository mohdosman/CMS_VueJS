import { ref, computed, onMounted, onBeforeUnmount } from 'vue';

// A closed dropdown with a checkbox per option; the field shows the short names of what is selected
// (the Blazor MudSelect multi-selection). Built on <details> so it opens and closes natively.
export function useMultiSelectDropdown(props, emit) {
    // ================================================================
    // State
    // ================================================================
    const root = ref(null);
    const isOn = (id) => props.modelValue.includes(id);
    const text = computed(() =>
        props.options.filter((o) => isOn(o.id)).map((o) => o.short || o.label).join(', ') || '- - NONE - -');

    function toggle(id, on) {
        emit('update:modelValue', on ? [...props.modelValue, id] : props.modelValue.filter((x) => x !== id));
    }

    function clear() {
        emit('update:modelValue', []);
    }

    // ================================================================
    // Closing: native <details> closes on neither an outside click nor Escape
    // ================================================================
    function close() {
        if (root.value) {
            root.value.open = false;
        }
    }

    function onDocClick(e) {
        if (root.value && !root.value.contains(e.target)) {
            close();
        }
    }

    function onKey(e) {
        if (e.key === 'Escape' && root.value?.open) {
            close();
            root.value.querySelector('summary').focus();
        }
    }

    onMounted(() => document.addEventListener('click', onDocClick));
    onBeforeUnmount(() => document.removeEventListener('click', onDocClick));

    return {
        // State
        root, text,

        // Actions
        isOn, toggle, clear, close, onKey
    };
}
