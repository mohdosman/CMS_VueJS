<script setup>
import { useSuicideFiles } from '../composables/useSuicideFiles.js';
import AppDialog from '../../../common/components/AppDialog.vue';

const {
    criteria, paging, files, totalRecords, isSearching, errors, current, records, recordTotal, recordPaging, isLoadingRecords,
    search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged,
    openRecords, closeRecords, setRecordOrder, recordSortIcon, onRecordPageChanged, onRecordPageSizeChanged, downloadUrl
} = useSuicideFiles();

const msg = (f) => errors.value[f]?.join(' ');
const dateTime = (v) => (v ? new Date(v).toLocaleString('en-US') : '');
const size = (bytes) => (bytes == null ? '' : bytes >= 1048576 ? `${(bytes / 1048576).toFixed(1)} MB` : `${Math.ceil(bytes / 1024)} KB`);
</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Display Suicide Files</h1>

        <SearchPanel title="Display Suicide Files" icon="fa fa-files-o" form-name="suicideFileForm" @submit="search" @reset="clear">
            <template #fields>
                <div class="row">
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="fileName">File Name</label>
                        <input id="fileName" v-model="criteria.fileName" type="text" class="form-control form-control-sm"
                               :aria-invalid="!!msg('fileName')" aria-describedby="fileName-err" />
                        <div id="fileName-err" class="form-text has-error" role="alert">{{ msg('fileName') }}</div>
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="dateFrom">Date Processed (From)</label>
                        <input id="dateFrom" v-model="criteria.dateFrom" type="date" class="form-control form-control-sm" />
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="dateTo">Date Processed (To)</label>
                        <input id="dateTo" v-model="criteria.dateTo" type="date" class="form-control form-control-sm"
                               :aria-invalid="!!msg('dateTo')" aria-describedby="dateTo-err" />
                        <div id="dateTo-err" class="form-text has-error" role="alert">{{ msg('dateTo') }}</div>
                    </div>
                </div>
            </template>
            <template #buttons>
                <AppButton action="search" :disabled="isSearching" />
                <AppButton action="clear" @click="clear" />
            </template>
        </SearchPanel>

        <div class="row" role="region" aria-labelledby="results-heading">
            <h2 id="results-heading" class="visually-hidden">Suicide File Results Grid</h2>
            <div class="col-md-12">
                <table class="table table-hover table-striped table-sm table-bordered">
                    <thead>
                        <tr>
                            <SortHeader col="id" :sort-icon="sortIcon" @sort="setOrder">Id</SortHeader>
                            <SortHeader col="fileName" :sort-icon="sortIcon" @sort="setOrder">File Name</SortHeader>
                            <th scope="col">Size</th>
                            <SortHeader col="recordCount" :sort-icon="sortIcon" @sort="setOrder">Records</SortHeader>
                            <SortHeader col="createdOn" :sort-icon="sortIcon" @sort="setOrder">Date Uploaded</SortHeader>
                            <th scope="col">File</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="!files.length" class="msg-error">
                            <td colspan="6"><div class="text-center"><strong>No files found.</strong></div></td>
                        </tr>
                        <tr v-for="f in files" :key="f.id">
                            <td>{{ f.id }}</td>
                            <td>{{ f.fileName }}</td>
                            <td>{{ size(f.fileSize) }}</td>
                            <td>{{ f.recordCount }}</td>
                            <td>{{ dateTime(f.createdOn) }}</td>
                            <td>
                                <AppButton action="cancel" size="xs" @click="openRecords(f)">View<span class="visually-hidden"> records of {{ f.fileName }}</span></AppButton>
                                <a class="btn btn-outline-secondary btn-xs" :href="downloadUrl(f.id)">Download<span class="visually-hidden"> {{ f.fileName }}</span></a>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <div role="region" aria-labelledby="paging-heading">
            <h2 id="paging-heading" class="visually-hidden">Suicide File Results Paging</h2>
            <SearchPaging :total-items="totalRecords"
                          :page-size="paging.pageSize"
                          :current-page="paging.currentPage"
                          :max-pages="paging.maxPagesToShow"
                          @page-changed="onPageChanged"
                          @page-size-changed="onPageSizeChanged" />
        </div>

        <AppDialog wide v-if="current" :title="`Records in ${current.fileName}`" @close="closeRecords">
            <p v-if="isLoadingRecords && !records.length" role="status">Loading...</p>
            <p v-else-if="!records.length" role="status">No records were found.</p>
            <template v-else>
                <div style="overflow: auto; max-height: 24rem">
                    <table class="table table-sm table-striped table-bordered">
                        <thead>
                            <tr>
                                <SortHeader col="lastName" :sort-icon="recordSortIcon" @sort="setRecordOrder">Last Name</SortHeader>
                                <SortHeader col="firstName" :sort-icon="recordSortIcon" @sort="setRecordOrder">First Name</SortHeader>
                                <th scope="col">Sex</th>
                                <SortHeader col="ssn" :sort-icon="recordSortIcon" @sort="setRecordOrder">SSN</SortHeader>
                                <th scope="col">DOB</th><th scope="col">DOD</th><th scope="col">Death State/Country</th>
                                <th scope="col">Res County</th><th scope="col">Res State/Country</th><th scope="col">Death Manner</th>
                                <th scope="col">US Armed Forces</th><th scope="col">Provider</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="r in records" :key="r.id">
                                <td>{{ r.lastName }}</td><td>{{ r.firstName }}</td><td>{{ r.sex }}</td><td>{{ r.ssn }}</td>
                                <td>{{ r.dateOfBirth }}</td><td>{{ r.dateOfDeath }}</td><td>{{ r.deathStateCountry }}</td>
                                <td>{{ r.resCounty }}</td><td>{{ r.resStateCountry }}</td><td>{{ r.deathManner }}</td>
                                <td>{{ r.usArmedForces }}</td><td>{{ r.provider }}</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <SearchPaging :total-items="recordTotal"
                              :page-size="recordPaging.pageSize"
                              :current-page="recordPaging.currentPage"
                              :max-pages="recordPaging.maxPagesToShow"
                              @page-changed="onRecordPageChanged"
                              @page-size-changed="onRecordPageSizeChanged" />
            </template>
            <AppButton action="close" @click="closeRecords" />
        </AppDialog>
    </div>
</template>
