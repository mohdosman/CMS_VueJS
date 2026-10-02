import { computed } from 'vue';

// The paging bar under a results grid: page numbers around the current page, plus a page-size slider.
export function useSearchPaging(props, emit) {
    // ================================================================
    // Pages
    // ================================================================
    const totalPages = computed(() => Math.ceil(props.totalItems / props.pageSize) || 1);

    // The page numbers shown, centred on the current page.
    const pageNumbers = computed(() => {
        const half = Math.floor(props.maxPages / 2);
        let start = Math.max(1, props.currentPage - half);
        const end = Math.min(totalPages.value, start + props.maxPages - 1);
        if (end - start + 1 < props.maxPages) {
            start = Math.max(1, end - props.maxPages + 1);
        }
        const pages = [];
        for (let page = start; page <= end; page++) {
            pages.push(page);
        }
        return pages;
    });

    function emitPageChanged(page) {
        const clamped = Math.min(Math.max(page, 1), totalPages.value);
        if (clamped === props.currentPage) {
            return;
        }
        emit('page-changed', clamped);
    }

    // ================================================================
    // Page size
    // ================================================================
    const sliderIndex = computed(() => {
        const index = props.pageSizeOptions.indexOf(props.pageSize);
        return index >= 0 ? index : 0;
    });

    function emitPageSizeChanged(value) {
        const numeric = Number(value);
        if (numeric === props.pageSize) {
            return;
        }
        emit('page-size-changed', numeric);
    }

    function onSliderInput(event) {
        const nextSize = props.pageSizeOptions[Number(event.target.value)];
        if (nextSize != null) {
            emitPageSizeChanged(nextSize);
        }
    }

    return {
        // Pages
        totalPages, pageNumbers, sliderIndex,

        // Actions
        emitPageChanged, emitPageSizeChanged, onSliderInput
    };
}
