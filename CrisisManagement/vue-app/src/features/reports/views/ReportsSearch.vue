<script setup>
import { useReportSearch } from '../composables/useReportSearch.js';
import { truncate } from '../../../utils/formatters.js';

const {
    exportLabel, msg, criteria, paging, reports, totalRecords, isSearching, errors, runningKey,
    search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, run, canAdd, add
} = useReportSearch();

</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Report Listing</h1>

        <SearchPanel title="Report Listing" icon="fa fa-search" form-name="reportForm" @submit="search" @reset="clear">
            <template #fields>
                <div class="row">
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="reportName">Report Name</label>
                        <input id="reportName" v-model="criteria.reportName" type="text" maxlength="50" class="form-control form-control-sm"
                               :aria-invalid="!!msg('reportName')" aria-describedby="reportName-err" />
                        <div id="reportName-err" class="form-text has-error" role="alert">{{ msg('reportName') }}</div>
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="description">Description</label>
                        <input id="description" v-model="criteria.description" type="text" maxlength="255" class="form-control form-control-sm"
                               :aria-invalid="!!msg('description')" aria-describedby="description-err" />
                        <div id="description-err" class="form-text has-error" role="alert">{{ msg('description') }}</div>
                    </div>
                </div>
            </template>
            <template #buttons>
                <AppButton action="search" :disabled="isSearching" />
                <AppButton v-if="canAdd" action="add" @click="add">Add Report</AppButton>
                <AppButton action="clear" @click="clear" />
            </template>
        </SearchPanel>

        <div class="row" role="region" aria-labelledby="results-heading">
            <h2 id="results-heading" class="visually-hidden">Report Results Grid</h2>
            <div class="col-md-12">
                <table class="table table-hover table-striped table-sm table-bordered">
                    <thead>
                        <tr>
                            <th scope="col">Run</th>
                            <SortHeader col="reportName" :sort-icon="sortIcon" @sort="setOrder">Report Name</SortHeader>
                            <SortHeader col="description" :sort-icon="sortIcon" @sort="setOrder">Description</SortHeader>
                            <SortHeader col="exportOption" :sort-icon="sortIcon" @sort="setOrder">Export Option</SortHeader>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="!reports.length" class="msg-error">
                            <td colspan="4"><div class="text-center"><strong>No reports found.</strong></div></td>
                        </tr>
                        <tr v-for="r in reports" :key="r.id">
                            <td>
                                <button type="button" class="btn btn-success btn-xs" :disabled="runningKey === r.reportKey" @click="run(r)">
                                    <i :class="runningKey === r.reportKey ? 'fa fa-spinner fa-spin' : 'fa fa-play'" aria-hidden="true"></i>
                                    <span class="visually-hidden">Run {{ r.reportName }}</span>
                                </button>
                            </td>
                            <td><router-link :to="`/reports/${r.id}`">{{ r.reportName }}</router-link></td>
                            <td :title="r.description">{{ truncate(r.description) }}</td>
                            <td><span class="badge text-bg-secondary">{{ exportLabel(r.exportOption) }}</span></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <div role="region" aria-labelledby="paging-heading">
            <h2 id="paging-heading" class="visually-hidden">Report Results Paging</h2>
            <SearchPaging :total-items="totalRecords"
                          :page-size="paging.pageSize"
                          :current-page="paging.currentPage"
                          :max-pages="paging.maxPagesToShow"
                          @page-changed="onPageChanged"
                          @page-size-changed="onPageSizeChanged" />
        </div>
    </div>
</template>
