<script setup>
import { useAssessmentSearch } from '../composables/useAssessmentSearch.js';

const {
    criteria, paging, assessments, totalRecords, providers, isSearching, errors,
    search, showAll, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, keyOf, gotoAssessment, canAdd, add
} = useAssessmentSearch();

const msg = (f) => errors.value[f]?.join(' ');
const date = (v) => (v ? new Date(v).toLocaleDateString('en-US') : '-');
const fields = [
    { f: 'lastName', label: 'Last Name', col: 'col-md-3' },
    { f: 'firstName', label: 'First Name', col: 'col-md-3' },
    { f: 'providerPatientNo', label: 'Provider Patient ID', col: 'col-md-3' },
    { f: 'ssn', label: 'SSN', col: 'col-md-3' },
    { f: 'completedByLastName', label: 'Assessment Completed By Last Name', col: 'col-md-3' },
    { f: 'completedByFirstName', label: 'Assessment Completed By First Name', col: 'col-md-3' },
    { f: 'f2FAssessmentId', label: 'Face to Face Assessment ID', col: 'col-md-3', type: 'number' },
    { f: 'phoneAssessmentId', label: 'Phone Assessment ID', col: 'col-md-3', type: 'number' },
    { f: 'providerF2FAssessmentId', label: 'Provider Face to Face Assessment ID', col: 'col-md-3' },
    { f: 'providerPhoneAssessmentId', label: 'Provider Phone Assessment ID', col: 'col-md-3' }
];
</script>

<template>
    <div class="main_content" role="main" aria-labelledby="main-title">
        <h1 id="main-title" class="visually-hidden">Search Assessments</h1>

        <SearchPanel title="Search Assessments" icon="fa fa-search" form-name="assessmentForm" @submit="search" @reset="clear">
            <template #fields>
                <div class="row">
                    <div class="col-md-4 mb-3">
                        <label class="form-label" for="providerId">Provider</label>
                        <select id="providerId" v-model="criteria.providerId" class="form-select form-select-sm">
                            <option :value="null">- - ALL - -</option>
                            <option v-for="p in providers" :key="p.id" :value="p.id">{{ p.label }}</option>
                        </select>
                    </div>
                    <div class="col-md-2 mb-3">
                        <label class="form-label" for="assessmentDateFrom">Assessment Date (From)</label>
                        <input id="assessmentDateFrom" v-model="criteria.assessmentDateFrom" type="date" class="form-control form-control-sm" />
                    </div>
                    <div class="col-md-2 mb-3">
                        <label class="form-label" for="assessmentDateTo">Assessment Date (To)</label>
                        <input id="assessmentDateTo" v-model="criteria.assessmentDateTo" type="date" class="form-control form-control-sm"
                               :aria-invalid="!!msg('assessmentDateTo')" aria-describedby="assessmentDateTo-err" />
                        <div id="assessmentDateTo-err" class="form-text has-error" role="alert">{{ msg('assessmentDateTo') }}</div>
                    </div>
                    <div class="col-md-4 mb-3">
                        <div class="form-label">Show</div>
                        <label class="checkbox-inline"><input v-model="criteria.incompleteOnly" type="checkbox" /> Incomplete assessments only</label>
                    </div>
                </div>
                <div class="row">
                    <div v-for="x in fields" :key="x.f" class="mb-3" :class="x.col">
                        <label class="form-label" :for="x.f">{{ x.label }}</label>
                        <input :id="x.f" v-model="criteria[x.f]" :type="x.type || 'text'" :min="x.type === 'number' ? 1 : undefined"
                               class="form-control form-control-sm" :aria-invalid="!!msg(x.f)" :aria-describedby="`${x.f}-err`" />
                        <div :id="`${x.f}-err`" class="form-text has-error" role="alert">{{ msg(x.f) }}</div>
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
            <h2 id="results-heading" class="visually-hidden">Assessment Results Grid</h2>
            <div class="col-md-12">
                <div v-if="criteria.incompleteOnly" class="alert alert-warning d-flex align-items-center justify-content-between py-2 mb-2" role="status">
                    <span><i class="fa fa-exclamation-triangle" aria-hidden="true"></i> Showing incomplete assessments only (follow-up still needed).</span>
                    <AppButton action="cancel" size="xs" @click="showAll">Show all assessments</AppButton>
                </div>
                <table class="table table-hover table-striped table-sm table-bordered">
                    <thead>
                        <tr>
                            <SortHeader col="lastName" :sort-icon="sortIcon" @sort="setOrder">Patient Name</SortHeader>
                            <SortHeader col="assessmentType" :sort-icon="sortIcon" @sort="setOrder">Assmnt Type</SortHeader>
                            <th scope="col">F2F Id</th>
                            <th scope="col">PA Id</th>
                            <SortHeader col="assessmentDate" :sort-icon="sortIcon" @sort="setOrder">Assmnt Date</SortHeader>
                            <SortHeader col="ssn" :sort-icon="sortIcon" @sort="setOrder">SSN</SortHeader>
                            <SortHeader col="dob" :sort-icon="sortIcon" @sort="setOrder">DOB</SortHeader>
                            <SortHeader col="providerName" :sort-icon="sortIcon" @sort="setOrder">Provider Abbrev.</SortHeader>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="!assessments.length" class="msg-error">
                            <td colspan="8"><div class="text-center"><strong>No Assessments Found</strong></div></td>
                        </tr>
                        <tr v-for="a in assessments" :key="`${a.f2FAssessmentId}-${a.phoneAssessmentId}`" :style="keyOf(a) ? 'cursor:pointer' : ''"
                            @click="gotoAssessment(a)">
                            <td>
                                <router-link v-if="keyOf(a)" :to="`/assessments/${keyOf(a)}`" @click.stop>{{ a.firstName }} {{ a.lastName }}</router-link>
                                <span v-else>{{ a.firstName }} {{ a.lastName }}</span>
                            </td>
                            <td>{{ a.assessmentType }}</td>
                            <td>{{ a.f2FAssessmentId || '' }}</td>
                            <td>{{ a.phoneAssessmentId || '' }}</td>
                            <td>{{ date(a.assessmentDate) }}</td>
                            <td>{{ a.ssn }}</td>
                            <td>{{ date(a.dob) }}</td>
                            <td>{{ a.abbreviation }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

        <div role="region" aria-labelledby="paging-heading">
            <h2 id="paging-heading" class="visually-hidden">Assessment Results Paging</h2>
            <SearchPaging :total-items="totalRecords"
                          :page-size="paging.pageSize"
                          :current-page="paging.currentPage"
                          :max-pages="paging.maxPagesToShow"
                          @page-changed="onPageChanged"
                          @page-size-changed="onPageSizeChanged" />
        </div>
    </div>
</template>
