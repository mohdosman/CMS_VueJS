<script setup>
import { computed } from 'vue';

// Ported from BudgetContracts.Core SearchPaging.vue (slider page-size + a11y).
const props = defineProps({
    totalItems: { type: Number, default: 0 },
    pageSize: { type: Number, default: 20 },
    currentPage: { type: Number, default: 1 },
    maxPages: { type: Number, default: 10 },
    firstText: { type: String, default: 'First' },
    previousText: { type: String, default: 'Prev' },
    nextText: { type: String, default: 'Next' },
    lastText: { type: String, default: 'Last' },
    showRecordCount: { type: Boolean, default: true },
    pageSizeHeading: { type: String, default: 'Page Size' },
    pageSizeColumnClass: { type: String, default: 'col-md-3' },
    pagerColumnClass: { type: String, default: 'col-md-9' },
    wrapperClass: { type: String, default: 'row' },
    wrapperStyle: { type: String, default: '' },
    pageSizeMode: { type: String, default: 'slider' },
    pageSizeOptions: { type: Array, default: () => [20, 30, 40, 50] }
});

const emit = defineEmits(['page-changed', 'page-size-changed']);

const totalPages = computed(() => Math.ceil(props.totalItems / props.pageSize) || 1);

const pageNumbers = computed(() => {
    const half = Math.floor(props.maxPages / 2);
    let start = Math.max(1, props.currentPage - half);
    let end = Math.min(totalPages.value, start + props.maxPages - 1);
    if (end - start + 1 < props.maxPages) start = Math.max(1, end - props.maxPages + 1);
    const pages = [];
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
});

const sliderIndex = computed(() => {
    const idx = props.pageSizeOptions.indexOf(props.pageSize);
    return idx >= 0 ? idx : 0;
});

function emitPageChanged(page) {
    const clamped = Math.min(Math.max(page, 1), totalPages.value);
    if (clamped === props.currentPage) return;
    emit('page-changed', clamped);
}

function emitPageSizeChanged(value) {
    const numeric = Number(value);
    if (numeric === props.pageSize) return;
    emit('page-size-changed', numeric);
}

function onSliderInput(event) {
    const index = Number(event.target.value);
    const nextSize = props.pageSizeOptions[index];
    if (nextSize != null) emitPageSizeChanged(nextSize);
}
</script>

<template>
    <div :class="wrapperClass" :style="wrapperStyle">
        <div :class="[pageSizeColumnClass, { 'legacy-page-size-column': pageSizeMode === 'slider' }]">
            <h3 v-if="pageSizeHeading" class="legacy-page-size-heading">{{ pageSizeHeading }}</h3>
            <template v-if="pageSizeMode === 'slider'">
                <div class="legacy-page-size-slider">
                    <input type="range"
                           class="legacy-page-size-slider__input"
                           aria-label="Results per page"
                           :aria-valuetext="`${pageSizeOptions[sliderIndex]} per page`"
                           :min="0"
                           :max="Math.max(pageSizeOptions.length - 1, 0)"
                           :step="1"
                           :value="sliderIndex"
                           @input="onSliderInput" />
                    <div class="legacy-page-size-slider__ticks">
                        <span v-for="size in pageSizeOptions" :key="size">{{ size }}</span>
                    </div>
                </div>
            </template>
            <select v-else class="form-control form-control-sm" aria-label="Results per page"
                    :value="pageSize" @change="emitPageSizeChanged($event.target.value)">
                <option v-for="s in pageSizeOptions" :key="s" :value="s">{{ s }} per page</option>
            </select>
        </div>
        <div :class="[pagerColumnClass, { 'legacy-pager-column': pageSizeMode === 'slider' }]">
            <nav class="legacy-pagination-nav" aria-label="Search results pages">
            <ul class="pagination pagination-sm legacy-pagination" style="margin:0">
                <li class="page-item" :class="{ disabled: currentPage === 1 }">
                    <a class="page-link" href="#" :aria-disabled="currentPage === 1 || undefined" :tabindex="currentPage === 1 ? -1 : undefined" @click.prevent="emitPageChanged(1)">{{ firstText }}</a>
                </li>
                <li class="page-item" :class="{ disabled: currentPage === 1 }">
                    <a class="page-link" href="#" :aria-disabled="currentPage === 1 || undefined" :tabindex="currentPage === 1 ? -1 : undefined" @click.prevent="emitPageChanged(currentPage - 1)">{{ previousText }}</a>
                </li>
                <li v-for="n in pageNumbers" :key="n" class="page-item" :class="{ active: n === currentPage }">
                    <a class="page-link" href="#" :aria-current="n === currentPage ? 'page' : undefined" @click.prevent="emitPageChanged(n)">{{ n }}</a>
                </li>
                <li class="page-item" :class="{ disabled: currentPage === totalPages }">
                    <a class="page-link" href="#" :aria-disabled="currentPage === totalPages || undefined" :tabindex="currentPage === totalPages ? -1 : undefined" @click.prevent="emitPageChanged(currentPage + 1)">{{ nextText }}</a>
                </li>
                <li class="page-item" :class="{ disabled: currentPage === totalPages }">
                    <a class="page-link" href="#" :aria-disabled="currentPage === totalPages || undefined" :tabindex="currentPage === totalPages ? -1 : undefined" @click.prevent="emitPageChanged(totalPages)">{{ lastText }}</a>
                </li>
            </ul>
            </nav>
            <div v-if="showRecordCount" class="legacy-record-count text-muted" role="status">{{ totalItems }} records</div>
            <span v-else class="visually-hidden" role="status">{{ totalItems }} records</span>
        </div>
    </div>
</template>

<style scoped>
.legacy-page-size-column {
    min-height: 78px;
}

.legacy-page-size-heading {
    margin: 0 0 6px;
}

.legacy-page-size-slider {
    max-width: 240px;
}

.legacy-page-size-slider__input {
    width: 100%;
    margin: 0;
}

.legacy-page-size-slider__ticks {
    display: flex;
    justify-content: space-between;
    padding: 0 2px;
    margin-top: 4px;
}

.legacy-pager-column {
    min-height: 78px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
}

.legacy-pagination-nav {
    flex: 0 0 auto;
}

.legacy-pagination {
    margin-top: 10px;
}

.legacy-record-count {
    margin-left: auto;
    white-space: nowrap;
}
</style>
