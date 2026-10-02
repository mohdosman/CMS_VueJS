import { computed } from 'vue';
import { useRoute } from 'vue-router';

// A menu entry whose screen has not been migrated yet: it shows the entry's title.
export function useComingSoon() {
    const route = useRoute();

    const title = computed(() => route.meta.title);

    return {
        title
    };
}
