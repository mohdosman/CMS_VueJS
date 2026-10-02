import { ref } from 'vue';
import { iconClass } from '../icons.js';

// One menu item with its sub-menus (recursive). Roots and the first level start open.
export function useMenuTreeNode(props) {
    // ================================================================
    // State
    // ================================================================
    const open = ref(props.depth < 2);

    // While filtering, every item is open so the matches are visible.
    const isOpen = () => props.forceOpen || open.value;

    return {
        // State
        open,

        // Helpers for the template
        isOpen, iconClass
    };
}
