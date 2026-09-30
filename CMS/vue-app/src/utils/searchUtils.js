/**
 * Shared search-grid helpers — replace the per-controller setOrder / selectedCl /
 * pageChanged / pageSizeChanged functions in the AngularJS components.
 */

/** Toggle/set sort column, reset to page 1, then search. */
export function createSetOrder(criteria, paging, search) {
    return function setOrder(col) {
        if (criteria.orderBy === col) {
            criteria.reverse = !criteria.reverse;
        } else {
            criteria.orderBy = col;
            criteria.reverse = false;
        }
        paging.currentPage = 1;
        search();
    };
}

/** Font Awesome sort icon for a column (matches the AngularJS selectedCl()). */
export function getSortIcon(col, criteria) {
    if (criteria.orderBy !== col) return '';
    return criteria.reverse ? 'fa fa-sort-down' : 'fa fa-sort-up';
}

/** Paging handlers bound to a reactive paging object + search function. */
export function createPagingHandlers(paging, search) {
    return {
        onPageChanged(page) {
            const next = Math.max(page, 1);
            if (paging.currentPage === next) return;
            paging.currentPage = next;
            search();
        },
        onPageSizeChanged(size) {
            paging.pageSize = size;
            paging.currentPage = 1;
            search();
        }
    };
}
