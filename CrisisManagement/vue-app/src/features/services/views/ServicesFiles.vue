<script setup>
import { useServiceFiles } from '../composables/useServiceFiles.js';

const {
    msg, criteria, paging, files, totalRecords, providers, hasSearched, isSearching, errors, dialog, current, rawText, fileErrors,
    errorPaging, errorTotal, setErrorOrder, errorSortIcon, isLoadingDialog,
    search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, onErrorPageChanged, onErrorPageSizeChanged, open, closeDialog,
    formatDateTimeFull
} = useServiceFiles();

</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Search Files</h1>

        <SearchPanel title="Search Files" icon="fa fa-files-o" form-name="fileForm" @submit="search" @reset="clear">
            <template #fields>
                <div class="row">
                    <div class="col-md-4 mb-3" :class="{ 'has-error': !!(msg('providerId')) }">
                        <label class="form-label" for="providerId">Provider</label>
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
                    <div class="col-md-3 mb-3" :class="{ 'has-error': !!(msg('dateTo')) }">
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
            <h2 id="results-heading" class="visually-hidden">Service File Results Grid</h2>
            <div class="col-md-12">
                <table v-if="hasSearched" class="table table-hover table-striped table-sm table-bordered">
                    <thead>
                        <tr>
                            <SortHeader col="id" :sort-icon="sortIcon" @sort="setOrder">Id</SortHeader>
                            <th scope="col">File Name</th>
                            <th scope="col">Processed</th>
                            <th scope="col" class="text-end">Total</th>
                            <th scope="col" class="text-end">Posted</th>
                            <th scope="col" class="text-end">Errors</th>
                            <SortHeader col="createdOn" :sort-icon="sortIcon" @sort="setOrder">Date Uploaded</SortHeader>
                            <th scope="col">Raw File</th>
                            <th scope="col">Errors</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="!files.length" class="msg-error">
                            <td colspan="9"><div class="text-center"><strong>No Records Found.</strong></div></td>
                        </tr>
                        <tr v-for="f in files" :key="f.id">
                            <td>{{ f.id }}</td>
                            <td>{{ f.fileName }}</td>
                            <td><span class="badge" :class="f.isProcessed ? 'text-bg-success' : f.isInProcess ? 'text-bg-info' : 'text-bg-warning'">{{ f.isProcessed ? 'Processed' : f.isInProcess ? 'Processing' : 'Pending' }}</span></td>
                            <td class="text-end">{{ f.serviceCount }}</td>
                            <td class="text-end">{{ f.postedCount }}</td>
                            <td class="text-end">{{ f.errorCount }}</td>
                            <td>{{ formatDateTimeFull(f.createdOn) }}</td>
                            <td><AppButton action="cancel" size="xs" @click="open('raw', f)">View<span class="visually-hidden"> raw file {{ f.fileName }}</span></AppButton></td>
                            <td>
                                <AppButton v-if="f.errorCount > 0" action="cancel" size="xs" @click="open('errors', f)">View<span class="visually-hidden"> errors of {{ f.fileName }}</span></AppButton>
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

        <AppDialog wide v-if="dialog === 'raw'" :title="`Raw file: ${current?.fileName}`" @close="closeDialog">
            <p v-if="isLoadingDialog" role="status">Loading, please wait...</p>
            <pre v-else class="border p-2" style="max-height: 24rem; overflow: auto; background: var(--bs-tertiary-bg, #f8f9fa); color: var(--bs-body-color)" tabindex="0">{{ rawText }}</pre>
            <AppButton action="close" @click="closeDialog" />
        </AppDialog>

        <AppDialog wide v-if="dialog === 'errors'" :title="`Errors for ${current?.fileName}`" @close="closeDialog">
            <p v-if="isLoadingDialog" role="status">Loading, please wait...</p>
            <template v-else>
                <table class="table table-sm table-striped table-bordered">
                    <thead>
                        <tr>
                            <SortHeader col="id" :sort-icon="errorSortIcon" @sort="setErrorOrder">Id</SortHeader>
                            <SortHeader col="importId" :sort-icon="errorSortIcon" @sort="setErrorOrder">ImportId</SortHeader>
                            <SortHeader col="ssn" :sort-icon="errorSortIcon" @sort="setErrorOrder">SSN</SortHeader>
                            <SortHeader col="dob" :sort-icon="errorSortIcon" @sort="setErrorOrder">DOB</SortHeader>
                            <SortHeader col="firstName" :sort-icon="errorSortIcon" @sort="setErrorOrder">First Name</SortHeader>
                            <SortHeader col="lastName" :sort-icon="errorSortIcon" @sort="setErrorOrder">Last Name</SortHeader>
                            <SortHeader col="serviceCode" :sort-icon="errorSortIcon" @sort="setErrorOrder">Service Code</SortHeader>
                            <SortHeader col="dosAdmitDate" :sort-icon="errorSortIcon" @sort="setErrorOrder">DOSAdmit Date</SortHeader>
                            <SortHeader col="description" :sort-icon="errorSortIcon" @sort="setErrorOrder">Error Desc</SortHeader>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="e in fileErrors" :key="e.id">
                            <td>{{ e.id }}</td><td>{{ e.importId }}</td><td>{{ e.ssn }}</td><td>{{ e.dob }}</td><td>{{ e.firstName }}</td>
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
