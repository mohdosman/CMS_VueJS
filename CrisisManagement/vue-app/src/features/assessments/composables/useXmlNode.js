import { computed } from 'vue';

// Recursive collapsible XML element: native <details>, so no library and the browser's Ctrl+F opens collapsed matches.
export function useXmlNode(props) {
    const name = computed(() => props.node.tagName);
    const attrs = computed(() => Array.from(props.node.attributes).map((a) => ` ${a.name}="${a.value}"`).join(''));
    const kids = computed(() => Array.from(props.node.children));

    return {
        name, attrs, kids
    };
}
