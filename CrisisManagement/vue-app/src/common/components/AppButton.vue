<script setup>
import { computed, useAttrs } from 'vue';

// Colors follow the one-accent-per-screen convention: Save/Search are the
// primary commit, neutral actions are btn-outline-secondary, and only Delete is red.
const ACTION_MAP = {
    save: { color: 'btn-primary', type: 'submit' },
    search: { color: 'btn-primary', type: 'submit' },
    run: { color: 'btn-primary', type: 'button' },
    add: { color: 'btn-outline-secondary', type: 'button' },
    preview: { color: 'btn-outline-secondary', type: 'button' },
    cancel: { color: 'btn-outline-secondary', type: 'button' },
    clear: { color: 'btn-outline-secondary', type: 'button' },
    close: { color: 'btn-outline-secondary', type: 'button' },
    delete: { color: 'btn-danger', type: 'button' },
};

defineOptions({ inheritAttrs: false });

const props = defineProps({
    action: {
        type: String,
        required: true,
        // defineProps is hoisted and can't see ACTION_MAP — keep this list in sync
        validator: (value) => ['save', 'search', 'run', 'add', 'preview', 'cancel', 'clear', 'close', 'delete'].includes(value),
    },
    size: {
        type: String,
        default: 'sm',
        validator: (value) => ['sm', 'xs'].includes(value),
    },
    disabled: {
        type: Boolean,
        default: false,
    },
});

const attrs = useAttrs();

const buttonType = computed(() => ACTION_MAP[props.action].type);
const buttonClass = computed(() => ['btn', ACTION_MAP[props.action].color, `btn-${props.size}`, { disabled: props.disabled }]);
const defaultLabel = computed(() => props.action[0].toUpperCase() + props.action.slice(1));

// Everything except onClick falls through normally (aria-label, aria-describedby, etc.);
// onClick is pulled out so onClick below is the only listener Vue ever attaches.
const attrsWithoutClick = computed(() => {
    const { onClick, ...rest } = attrs;
    return rest;
});

// Native `disabled` pulls the button out of the tab order, so keyboard and
// screen-reader users can't reach it or learn why it's inert (WCAG 4.1.2).
// We signal the state with aria-disabled + a CSS class instead, and swallow
// the click/submit here rather than relying on the button being unreachable.
function onClick(event) {
    if (props.disabled) {
        event.preventDefault();
        return;
    }
    attrs.onClick?.(event);
}
</script>

<template>
    <!-- data-form-action: pressing Save/Cancel doesn't mark the blurred field touched (see createTouch). -->
    <button :type="buttonType" :class="buttonClass" :aria-disabled="disabled || undefined"
            :data-form-action="action === 'save' || action === 'cancel' ? '' : undefined"
            v-bind="attrsWithoutClick" @click="onClick">
        <slot>{{ defaultLabel }}</slot>
    </button>
</template>
