<script setup>
import { useAssessmentFiles } from '../composables/useAssessmentFiles.js';
import XmlNode from '../components/XmlNode.vue';

const {
    msg, criteria, paging, files, totalRecords, providers, hasSearched, isSearching, errors, dialog, current, rawXml, xmlRoot, fileErrors, isLoadingDialog,
    search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, open, closeDialog,
    formatDateTimeFull, prettyXml
} = useAssessmentFiles();
</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Search Assessment Files</h1>

        <SearchPanel title="Search Assessment Files" icon="fa fa-files-o" form-name="fileForm" @submit="search" @reset="clear">
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
                        <DateInput id="dateFrom" v-model="criteria.dateFrom" class="form-control form-control-sm" />
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="dateTo">Date Processed (To)</label>
                        <DateInput id="dateTo" v-model="criteria.dateTo" class="form-control form-control-sm"
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
            <h2 id="results-heading" class="visually-hidden">File Results Grid</h2>
            <div class="col-md-12">
                <table v-if="hasSearched" class="table table-hover table-striped table-sm table-bordered">
                    <thead>
                        <tr>
                            <SortHeader col="fileName" :sort-icon="sortIcon" @sort="setOrder">File Name</SortHeader>
                            <SortHeader col="isProcessed" :sort-icon="sortIcon" @sort="setOrder">Status</SortHeader>
                            <SortHeader col="phoneTotal" :sort-icon="sortIcon" @sort="setOrder">PA Total</SortHeader>
                            <SortHeader col="phoneImported" :sort-icon="sortIcon" @sort="setOrder">PA Imported</SortHeader>
                            <SortHeader col="phoneErrors" :sort-icon="sortIcon" @sort="setOrder">PA Errors</SortHeader>
                            <SortHeader col="f2fTotal" :sort-icon="sortIcon" @sort="setOrder">F2F Total</SortHeader>
                            <SortHeader col="f2fImported" :sort-icon="sortIcon" @sort="setOrder">F2F Imported</SortHeader>
                            <SortHeader col="f2fErrors" :sort-icon="sortIcon" @sort="setOrder">F2F Errors</SortHeader>
                            <SortHeader col="createdOn" :sort-icon="sortIcon" @sort="setOrder">Date Uploaded</SortHeader>
                            <th scope="col">Raw File</th>
                            <th scope="col">Errors</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="!files.length" class="msg-error">
                            <td colspan="11"><div class="text-center"><strong>No Files Found</strong></div></td>
                        </tr>
                        <tr v-for="f in files" :key="f.id">
                            <td>{{ f.fileName }}</td>
                            <td><span class="badge" :class="f.isProcessed ? 'text-bg-success' : 'text-bg-warning'">{{ f.isProcessed ? 'Processed' : 'Pending' }}</span></td>
                            <td>{{ f.phoneTotal }}</td>
                            <td>{{ f.phoneImported }}</td>
                            <td>{{ f.phoneErrors }}</td>
                            <td>{{ f.f2FTotal }}</td>
                            <td>{{ f.f2FImported }}</td>
                            <td>{{ f.f2FErrors }}</td>
                            <td>{{ formatDateTimeFull(f.createdOn) }}</td>
                            <td><AppButton action="cancel" size="xs" @click="open('raw', f)">View<span class="visually-hidden"> raw file {{ f.fileName }}</span></AppButton></td>
                            <td><AppButton action="cancel" size="xs" :disabled="!f.isProcessed" @click="open('errors', f)">View<span class="visually-hidden"> errors of {{ f.fileName }}</span></AppButton></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <div v-if="hasSearched" role="region" aria-labelledby="paging-heading">
            <h2 id="paging-heading" class="visually-hidden">File Results Paging</h2>
            <SearchPaging :total-items="totalRecords"
                          :page-size="paging.pageSize"
                          :current-page="paging.currentPage"
                          :max-pages="paging.maxPagesToShow"
                          @page-changed="onPageChanged"
                          @page-size-changed="onPageSizeChanged" />
        </div>

        <AppDialog wide v-if="dialog === 'raw'" :title="`Raw file: ${current?.fileName}`" @close="closeDialog">
            <p v-if="isLoadingDialog" role="status">Loading...</p>
            <div v-else-if="xmlRoot" class="border p-2" style="max-height: 24rem; overflow: auto" tabindex="0"><XmlNode :node="xmlRoot" /></div>
            <pre v-else class="border p-2" style="max-height: 24rem; overflow: auto; background: var(--bs-tertiary-bg, #f8f9fa); color: var(--bs-body-color)" tabindex="0">{{ prettyXml(rawXml) }}</pre>
            <AppButton action="close" @click="closeDialog" />
        </AppDialog>

        <AppDialog wide v-if="dialog === 'errors'" :title="`Errors for ${current?.fileName}`" @close="closeDialog">
            <p v-if="isLoadingDialog" role="status">Loading...</p>
            <p v-else-if="!fileErrors.length" role="status">No file upload errors were found.</p>
            <table v-else class="table table-sm table-striped table-bordered">
                <thead><tr><th scope="col">Error</th><th scope="col">Prov Assess Id</th><th scope="col">Prov Pt#</th><th scope="col">Assessment Date</th><th scope="col">Description</th></tr></thead>
                <tbody>
                    <tr v-for="e in fileErrors" :key="e.id">
                        <td>{{ e.errorTable }}</td><td>{{ e.providerAssessmentId }}</td><td>{{ e.providerPatientNo }}</td><td>{{ formatDateTime(e.assessmentDate) }}</td><td>{{ e.description }}</td>
                    </tr>
                </tbody>
            </table>
            <AppButton action="close" @click="closeDialog" />
        </AppDialog>
    </div>
</template>
