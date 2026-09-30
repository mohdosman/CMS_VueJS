<script setup>
import { useServiceSearch } from '../composables/useServiceSearch.js';

const {
    criteria, paging, services, totalRecords, providers, serviceCodes, hasSearched, isSearching, errors,
    search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, canAdd, add
} = useServiceSearch();

const msg = (f) => errors.value[f]?.join(' ');
const fields = [
    { f: 'providerPatientNo', label: 'Provider Patient ID', col: 'col-md-3' },
    { f: 'ssn', label: 'SSN', col: 'col-md-3' },
    { f: 'lastName', label: 'Last Name', col: 'col-md-3' },
    { f: 'firstName', label: 'First Name', col: 'col-md-3' }
];
</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Manage Service</h1>

        <SearchPanel title="Manage Service" icon="fa fa-search" form-name="serviceForm" @submit="search" @reset="clear">
            <template #fields>
                <div class="row">
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="providerId">Provider</label>
                        <select id="providerId" v-model="criteria.providerId" class="form-select form-select-sm">
                            <option :value="null">- - ALL - -</option>
                            <option v-for="p in providers" :key="p.id" :value="p.id">{{ p.label }}</option>
                        </select>
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="serviceCodeId">Service</label>
                        <select id="serviceCodeId" v-model="criteria.serviceCodeId" class="form-select form-select-sm">
                            <option :value="null">- - ALL - -</option>
                            <option v-for="s in serviceCodes" :key="s.id" :value="s.id">{{ s.label }}</option>
                        </select>
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="serviceFileId">Service File ID</label>
                        <input id="serviceFileId" v-model.number="criteria.serviceFileId" type="number" min="1" class="form-control form-control-sm" />
                    </div>
                </div>
                <div class="row">
                    <div v-for="x in fields" :key="x.f" class="mb-3" :class="x.col">
                        <label class="form-label" :for="x.f">{{ x.label }}</label>
                        <input :id="x.f" v-model="criteria[x.f]" type="text" class="form-control form-control-sm"
                               :aria-invalid="!!msg(x.f)" :aria-describedby="`${x.f}-err`" />
                        <div :id="`${x.f}-err`" class="form-text has-error" role="alert">{{ msg(x.f) }}</div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="dosAdmitDateFrom">DOS/Admit Date (From)</label>
                        <input id="dosAdmitDateFrom" v-model="criteria.dosAdmitDateFrom" type="date" class="form-control form-control-sm" />
                    </div>
                    <div class="col-md-3 mb-3">
                        <label class="form-label" for="dosAdmitDateTo">DOS/Admit Date (To)</label>
                        <input id="dosAdmitDateTo" v-model="criteria.dosAdmitDateTo" type="date" class="form-control form-control-sm"
                               :aria-invalid="!!msg('dosAdmitDateTo')" aria-describedby="dosAdmitDateTo-err" />
                        <div id="dosAdmitDateTo-err" class="form-text has-error" role="alert">{{ msg('dosAdmitDateTo') }}</div>
                    </div>
                </div>
            </template>
            <template #buttons>
                <AppButton action="search" :disabled="isSearching" />
                <AppButton v-if="canAdd" action="add" @click="add" />
                <AppButton action="clear" @click="clear" />
            </template>
        </SearchPanel>

        <div class="row" role="region" aria-labelledby="results-heading">
            <h2 id="results-heading" class="visually-hidden">Service Results Grid</h2>
            <div class="col-md-12">
                <p v-if="!hasSearched" class="text-muted text-center">Choose your filters, then click Search.</p>
                <table v-else class="table table-hover table-striped table-sm table-bordered">
                    <thead>
                        <tr>
                            <SortHeader col="serviceId" :sort-icon="sortIcon" @sort="setOrder">Id</SortHeader>
                            <SortHeader col="firstName" :sort-icon="sortIcon" @sort="setOrder">First Name</SortHeader>
                            <SortHeader col="lastName" :sort-icon="sortIcon" @sort="setOrder">Last Name</SortHeader>
                            <SortHeader col="providerPatientNo" :sort-icon="sortIcon" @sort="setOrder">Provider Patient No</SortHeader>
                            <SortHeader col="providerAbbrev" :sort-icon="sortIcon" @sort="setOrder">Provider</SortHeader>
                            <SortHeader col="serviceCodeAbbrev" :sort-icon="sortIcon" @sort="setOrder">Service</SortHeader>
                            <SortHeader col="ssn" :sort-icon="sortIcon" @sort="setOrder">SSN</SortHeader>
                            <SortHeader col="dosAdmitDate" :sort-icon="sortIcon" @sort="setOrder">DOS/Admit Date</SortHeader>
                            <SortHeader col="dischargeDate" :sort-icon="sortIcon" @sort="setOrder">Discharge Date</SortHeader>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="!services.length" class="msg-error">
                            <td colspan="9"><div class="text-center"><strong>No services found.</strong></div></td>
                        </tr>
                        <tr v-for="s in services" :key="s.serviceId">
                            <td><router-link :to="`/services/${s.serviceId}`">{{ s.serviceId }}</router-link></td>
                            <td>{{ s.firstName }}</td>
                            <td>{{ s.lastName }}</td>
                            <td>{{ s.providerPatientNo }}</td>
                            <td>{{ s.providerAbbrev }}</td>
                            <td>{{ s.serviceCodeAbbrev }}</td>
                            <td>{{ s.ssn }}</td>
                            <td>{{ s.dosAdmitDate }}</td>
                            <td>{{ s.dischargeDate }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <div v-if="hasSearched" role="region" aria-labelledby="paging-heading">
            <h2 id="paging-heading" class="visually-hidden">Service Results Paging</h2>
            <SearchPaging :total-items="totalRecords"
                          :page-size="paging.pageSize"
                          :current-page="paging.currentPage"
                          :max-pages="paging.maxPagesToShow"
                          @page-changed="onPageChanged"
                          @page-size-changed="onPageSizeChanged" />
        </div>
    </div>
</template>
