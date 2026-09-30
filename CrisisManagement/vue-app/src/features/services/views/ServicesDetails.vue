<script setup>
import { useServiceDetail } from '../composables/useServiceDetail.js';
import AppDialog from '../../../common/components/AppDialog.vue';
import FieldSelect from '../../../common/components/FieldSelect.vue';
import FieldInput from '../../../common/components/FieldInput.vue';

const {
    isNew, canSave, canDelete, title, form, lookups, providers, formError, dialog, isLoading, isSaving,
    sessionServices, sessionTotal, dischargeRequired, durationRequired, patientLocked, msg, allErrors,
    findExistingPatient, save, remove, cancel
} = useServiceDetail();

const opts = (list) => list ?? [];
</script>

<template>
    <DetailPanel :title="title" icon="fa fa-medkit" form-name="serviceEntryForm" main-labelledby="main-title"
 :can-save="canSave && !isSaving && !isLoading" :show-buttons="canSave" @save="save" @cancel="cancel">
        <template #fields>
            <h1 id="main-title" class="visually-hidden">{{ title }}</h1>

            <div v-if="formError" class="alert alert-danger" role="alert">{{ formError }}</div>
            <div v-if="allErrors.length" class="alert alert-danger" role="alert">
                <strong>Please correct the following:</strong>
                <ul class="mb-0"><li v-for="(m, i) in allErrors" :key="i">{{ m }}</li></ul>
            </div>

            <!-- A read-only viewer (services.view only) gets the same form with every control disabled. -->
            <fieldset :disabled="!canSave || isLoading" class="border-0 p-0 m-0">
                <div class="row">
                    <FieldSelect v-model="form.providerId" label="Provider" required :options="providers" :error="msg('providerId')" col="col-md-6 col-lg-4"
                                 @update:model-value="findExistingPatient" />
                    <FieldInput v-model="form.providerPatientNo" label="Provider Patient ID" required :maxlength="50" :disabled="!isNew"
                                :error="msg('providerPatientNo')" col="col-md-6 col-lg-4" @change="findExistingPatient" />
                    <FieldInput v-model="form.ssn" label="SSN" :maxlength="11" hint="9 digits, or 000-00-0000." :disabled="patientLocked" :error="msg('ssn')" col="col-md-6 col-lg-4" />
                    <FieldInput v-model="form.firstName" label="First Name" required :maxlength="150" :disabled="!isNew" :error="msg('firstName')" col="col-md-6 col-lg-4" />
                    <FieldInput v-model="form.lastName" label="Last Name" required :maxlength="150" :disabled="patientLocked" :error="msg('lastName')" col="col-md-6 col-lg-4" />
                    <FieldInput v-model="form.dob" label="Date of Birth" type="date" required :disabled="patientLocked" :error="msg('dob')" col="col-md-6 col-lg-4" />
                    <FieldSelect v-model="form.genderId" label="Gender" required :options="opts(lookups.genders)" :disabled="!isNew" :error="msg('genderId')" col="col-md-6 col-lg-4" />
                    <FieldSelect v-model="form.countyId" label="County of Residence" required :options="opts(lookups.counties)" :error="msg('countyId')" col="col-md-6 col-lg-4" />
                    <FieldSelect v-model="form.payorSourceId" label="Payor Billed for Service" required :options="opts(lookups.payorSources)" :error="msg('payorSourceId')" col="col-md-6 col-lg-4" />
                    <FieldSelect v-model="form.primaryInsurerId" label="Primary Insurer" :options="opts(lookups.payorSources)" :error="msg('primaryInsurerId')" col="col-md-6 col-lg-4" />
                    <FieldSelect v-model="form.serviceCodeId" label="Service" required :options="opts(lookups.serviceCodes)" :error="msg('serviceCodeId')" col="col-md-6 col-lg-4" />
                    <FieldInput v-model="form.dosAdmitDate" label="DOS/Admit Date" type="date" required :error="msg('dosAdmitDate')" col="col-md-6 col-lg-4" />
                    <FieldInput v-model="form.dischargeDate" label="Discharge Date" type="date" :required="dischargeRequired" :error="msg('dischargeDate')" col="col-md-6 col-lg-4" />
                    <FieldInput v-model="form.durationHours" label="Duration Hours" type="number" :min="1" :max="999" :required="durationRequired"
                                :error="msg('durationHours')" col="col-md-6 col-lg-4" />
                    <FieldSelect v-model="form.serviceCountyId" label="County of Service" required :options="opts(lookups.counties)" :error="msg('serviceCountyId')" col="col-md-6 col-lg-4" />
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
            <div v-if="isNew && sessionServices.length" class="row mt-2" role="region" aria-labelledby="session-heading">
                <div class="col-md-12">
                    <h2 id="session-heading" class="h6">Services entered in this session ({{ sessionTotal }})</h2>
                    <table class="table table-sm table-striped table-bordered">
                        <thead>
                            <tr>
                                <th scope="col">Id</th><th scope="col">Provider Patient No</th><th scope="col">Provider</th><th scope="col">SSN</th>
                                <th scope="col">DOS/Admit Date</th><th scope="col">Discharge Date</th>
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
