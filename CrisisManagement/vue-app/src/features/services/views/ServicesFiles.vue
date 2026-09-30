<script setup>
import { useServiceFiles } from '../composables/useServiceFiles.js';
import AppDialog from '../../../common/components/AppDialog.vue';

const {
    criteria, paging, files, totalRecords, providers, hasSearched, isSearching, errors, dialog, current, rawText, fileErrors,
    errorPaging, errorTotal, isLoadingDialog,
    search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, onErrorPageChanged, onErrorPageSizeChanged, open, closeDialog
} = useServiceFiles();

const msg = (f) => errors.value[f]?.join(' ');
const dateTime = (v) => new Date(v).toLocaleString('en-US');
</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Display Service Files</h1>

        <SearchPanel title="Display Service Files" icon="fa fa-files-o" form-name="fileForm" @submit="search" @reset="clear">
            <template #fields>
                <div class="row">
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="providerId">Provider <span class="f_req" aria-hidden="true">*</span></label>
                        <select id="providerId" v-model="criteria.providerId" class="form-select form-select-sm" aria-required="true"
                                :aria-invalid="!!msg('providerId')" aria-describedby="providerId-err">
                            <option :value="null">- - SELECT - -</option>
                            <option v-for="p in providers" :key="p.id" :value="p.id">{{ p.label }}</option>
                        </select>
                        <div id="providerId-err" class="form-text has-error" role="alert">{{ msg('providerId') }}</div>
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
            <h2 id="results-heading" class="visually-hidden">Service File Results Grid</h2>
            <div class="col-md-12">
                <p v-if="!hasSearched" class="text-muted text-center">Select a provider, then click Search.</p>
                <table v-else class="table table-hover table-striped table-sm table-bordered">
                    <thead>
                        <tr>
                            <SortHeader col="id" :sort-icon="sortIcon" @sort="setOrder">Id</SortHeader>
                            <SortHeader col="fileName" :sort-icon="sortIcon" @sort="setOrder">File Name</SortHeader>
                            <SortHeader col="isProcessed" :sort-icon="sortIcon" @sort="setOrder">Processed</SortHeader>
                            <SortHeader col="serviceCount" :sort-icon="sortIcon" @sort="setOrder">Total</SortHeader>
                            <SortHeader col="postedCount" :sort-icon="sortIcon" @sort="setOrder">Posted</SortHeader>
                            <SortHeader col="errorCount" :sort-icon="sortIcon" @sort="setOrder">Errors</SortHeader>
                            <SortHeader col="createdOn" :sort-icon="sortIcon" @sort="setOrder">Date Uploaded</SortHeader>
                            <th scope="col">Raw File</th>
                            <th scope="col">Errors</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="!files.length" class="msg-error">
                            <td colspan="9"><div class="text-center"><strong>No service files found.</strong></div></td>
                        </tr>
                        <tr v-for="f in files" :key="f.id">
                            <td>{{ f.id }}</td>
                            <td>{{ f.fileName }}</td>
                            <td><span class="badge" :class="f.isProcessed ? 'text-bg-success' : f.isInProcess ? 'text-bg-info' : 'text-bg-warning'">{{ f.isProcessed ? 'Processed' : f.isInProcess ? 'Processing' : 'Pending' }}</span></td>
                            <td>{{ f.serviceCount }}</td>
                            <td>{{ f.postedCount }}</td>
                            <td>{{ f.errorCount }}</td>
                            <td>{{ dateTime(f.createdOn) }}</td>
                            <td><AppButton action="cancel" size="xs" @click="open('raw', f)">View<span class="visually-hidden"> raw file {{ f.fileName }}</span></AppButton></td>
                            <td>
                                <AppButton v-if="f.isProcessed && f.errorCount > 0" action="cancel" size="xs" @click="open('errors', f)">View<span class="visually-hidden"> errors of {{ f.fileName }}</span></AppButton>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <div v-if="hasSearched" role="region" aria-labelledby="paging-heading">
            <h2 id="paging-heading" class="visually-hidden">Service File Results Paging</h2>
            <SearchPaging :total-items="totalRecords"
                          :page-size="paging.pageSize"
                          :current-page="paging.currentPage"
                          :max-pages="paging.maxPagesToShow"
                          @page-changed="onPageChanged"
                          @page-size-changed="onPageSizeChanged" />
        </div>

        <AppDialog v-if="dialog === 'raw'" :title="`Raw file: ${current?.fileName}`" @close="closeDialog">
            <p v-if="isLoadingDialog" role="status">Loading...</p>
            <pre v-else class="border p-2 bg-body-tertiary text-body" style="max-height: 24rem; overflow: auto" tabindex="0">{{ rawText }}</pre>
            <AppButton action="close" @click="closeDialog" />
        </AppDialog>

        <AppDialog v-if="dialog === 'errors'" :title="`Errors for ${current?.fileName}`" @close="closeDialog">
            <p v-if="isLoadingDialog" role="status">Loading...</p>
            <p v-else-if="!fileErrors.length" role="status">No errors were found.</p>
            <template v-else>
                <table class="table table-sm table-striped table-bordered">
                    <thead>
                        <tr>
                            <th scope="col">Id</th><th scope="col">Import Id</th><th scope="col">SSN</th><th scope="col">First Name</th>
                            <th scope="col">Last Name</th><th scope="col">Service Code</th><th scope="col">DOS Admit Date</th><th scope="col">Error</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="e in fileErrors" :key="e.id">
                            <td>{{ e.id }}</td><td>{{ e.importId }}</td><td>{{ e.ssn }}</td><td>{{ e.firstName }}</td>
                            <td>{{ e.lastName }}</td><td>{{ e.serviceCode }}</td><td>{{ e.dosAdmitDate }}</td><td>{{ e.description }}</td>
                        </tr>
                    </tbody>
                </table>
                <SearchPaging :total-items="errorTotal"
                              :page-size="errorPaging.pageSize"
                              :current-page="errorPaging.currentPage"
                              :max-pages="errorPaging.maxPagesToShow"
                              @page-changed="onErrorPageChanged"
                              @page-size-changed="onErrorPageSizeChanged" />
            </template>
            <AppButton action="close" @click="closeDialog" />
        </AppDialog>
    </div>
</template>
