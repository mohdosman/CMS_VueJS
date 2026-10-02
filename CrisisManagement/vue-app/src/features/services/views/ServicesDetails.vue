<script setup>
import { useServiceDetail } from '../composables/useServiceDetail.js';

const {
    isNew, canSave, canDelete, title, form, lookups, providers, dialog, isLoading, isSaving,
    sessionServices, sessionTotal, sessionPaging, setSessionOrder, sessionSortIcon, onSessionPageChanged, onSessionPageSizeChanged, noProvider, dischargeRequired, durationRequired, patientLocked, msg, touch,
    findExistingPatient, onSubmit, remove, cancel
} = useServiceDetail();

</script>

<template>
    <DetailPanel :title="title" icon="fa fa-medkit" form-name="serviceEntryForm" main-labelledby="main-title"
 :can-save="canSave && !isSaving && !isLoading" :show-buttons="canSave && !noProvider" @save="onSubmit" @cancel="cancel">
        <template #fields>
            <h1 id="main-title" class="visually-hidden">{{ title }}</h1>

            <div v-if="msg('form')" class="alert alert-danger" role="alert">{{ msg('form') }}</div>
            <div v-if="noProvider" class="alert alert-warning" role="alert">Current user is not assigned to the Facility/Provider</div>

            <!-- A read-only viewer (services.view only) gets the same form with every control disabled. -->
            <fieldset :disabled="!canSave || isLoading" class="border-0 p-0 m-0">
                <div class="row">
                    <FieldSelect v-model="form.providerId" label="Provider" required :options="providers" :error="msg('providerId')" @touch="touch('providerId')" col="col-md-6 col-lg-4"
                                 @update:model-value="findExistingPatient" />
                    <FieldInput v-model="form.providerPatientNo" label="Provider Patient No" required :maxlength="50" :disabled="!isNew"
                                :error="msg('providerPatientNo')" @touch="touch('providerPatientNo')" col="col-md-6 col-lg-4" @change="findExistingPatient" @keydown.enter.prevent />
                    <FieldInput v-model="form.ssn" label="SSN" :maxlength="11" :disabled="patientLocked" :error="msg('ssn')" @touch="touch('ssn')" col="col-md-6 col-lg-4" />
                    <FieldInput v-model="form.firstName" label="First Name" required :maxlength="150" :disabled="!isNew" :error="msg('firstName')" @touch="touch('firstName')" col="col-md-6 col-lg-4" />
                    <FieldInput v-model="form.lastName" label="Last Name" required :maxlength="150" :disabled="patientLocked" :error="msg('lastName')" @touch="touch('lastName')" col="col-md-6 col-lg-4" />
                    <FieldInput v-model="form.dob" label="DOB" type="date" required :disabled="patientLocked" :error="msg('dob')" @touch="touch('dob')" col="col-md-6 col-lg-4" />
                    <FieldSelect v-model="form.genderId" label="Gender" required :options="lookups.genders" :disabled="!isNew" :error="msg('genderId')" @touch="touch('genderId')" col="col-md-6 col-lg-4" />
                    <FieldSelect v-model="form.countyId" label="County of Residence" required :options="lookups.counties" :error="msg('countyId')" @touch="touch('countyId')" col="col-md-6 col-lg-4" />
                    <FieldSelect v-model="form.payorSourceId" label="Payor Billed for Service" required :options="lookups.payorSources" :error="msg('payorSourceId')" @touch="touch('payorSourceId')" col="col-md-6 col-lg-4" />
                    <FieldSelect v-model="form.primaryInsurerId" label="Primary Insurer" :options="lookups.payorSources" :error="msg('primaryInsurerId')" @touch="touch('primaryInsurerId')" col="col-md-6 col-lg-4" />
                    <FieldSelect v-model="form.serviceCodeId" label="Service" required :options="lookups.serviceCodes" :error="msg('serviceCodeId')" @touch="touch('serviceCodeId')" col="col-md-6 col-lg-4" />
                    <FieldInput v-model="form.dosAdmitDate" label="DOS or Admit Date" type="date" required :error="msg('dosAdmitDate')" @touch="touch('dosAdmitDate')" col="col-md-6 col-lg-4" />
                    <FieldInput v-model="form.dischargeDate" label="Discharge Date" type="date" :required="dischargeRequired" :error="msg('dischargeDate')" @touch="touch('dischargeDate')" col="col-md-6 col-lg-4" />
                    <FieldInput v-model="form.durationHours" label="Duration (Hours)" type="number" :min="1" :max="999" :required="durationRequired"
                                :error="msg('durationHours')" @touch="touch('durationHours')" col="col-md-6 col-lg-4" />
                    <FieldSelect v-model="form.serviceCountyId" label="County of Service" required :options="lookups.counties" :error="msg('serviceCountyId')" @touch="touch('serviceCountyId')" col="col-md-6 col-lg-4" />
                </div>
            </fieldset>
        </template>

        <template #actions>
            <AppButton v-if="!isNew && canDelete" action="delete" @click="dialog = 'delete'">Delete</AppButton>
        </template>

        <template v-if="!canSave" #button-row>
            <AppButton action="close" @click="cancel">Back to search</AppButton>
        </template>

        <template #below>
            <div v-if="isNew && sessionServices.length" class="row mt-2" role="region" aria-label="Services entered in this session">
                <div class="col-md-12">
                    <table class="table table-sm table-striped table-bordered">
                        <thead>
                            <tr>
                                <SortHeader col="serviceId" :sort-icon="sessionSortIcon" @sort="setSessionOrder">Id</SortHeader>
                                <SortHeader col="providerPatientNo" :sort-icon="sessionSortIcon" @sort="setSessionOrder">Provider Patient No</SortHeader>
                                <SortHeader col="providerAbbrev" :sort-icon="sessionSortIcon" @sort="setSessionOrder">Provider</SortHeader>
                                <SortHeader col="ssn" :sort-icon="sessionSortIcon" @sort="setSessionOrder">SSN</SortHeader>
                                <SortHeader col="dosAdmitDate" :sort-icon="sessionSortIcon" @sort="setSessionOrder">DOS/Admit Date</SortHeader>
                                <SortHeader col="dischargeDate" :sort-icon="sessionSortIcon" @sort="setSessionOrder">Discharge Date</SortHeader>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="s in sessionServices" :key="s.serviceId">
                                <td><router-link :to="`/services/${s.serviceId}`">{{ s.serviceId }}</router-link></td>
                                <td>{{ s.providerPatientNo }}</td><td>{{ s.providerAbbrev }}</td><td>{{ s.ssn }}</td>
                                <td>{{ s.dosAdmitDate }}</td><td>{{ s.dischargeDate }}</td>
                            </tr>
                        </tbody>
                    </table>
                    <SearchPaging :total-items="sessionTotal"
                                  :page-size="sessionPaging.pageSize"
                                  :current-page="sessionPaging.currentPage"
                                  :max-pages="sessionPaging.maxPagesToShow"
                                  @page-changed="onSessionPageChanged"
                                  @page-size-changed="onSessionPageSizeChanged" />
                </div>
            </div>

            <AppDialog v-if="dialog === 'delete'" title="Delete service" @close="dialog = ''">
                <p>Delete service <strong>{{ form.id }}</strong>? This action cannot be undone.</p>
                <AppButton action="delete" @click="remove">Delete service</AppButton>
                <AppButton action="cancel" @click="dialog = ''" />
            </AppDialog>
        </template>
    </DetailPanel>
</template>
