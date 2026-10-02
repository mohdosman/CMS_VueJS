<script setup>
import { useAssessmentDetail } from '../composables/useAssessmentDetail.js';

const {
    isNew, canEdit, canDelete, title, form, dt, lookups, providers, dialog, isLoading, isSaving,
    panelTitle, isDispatched, isOther, referralAccepted, msg,
    save, remove, cancel, isEmptyRow, removeRow, dispositionsFor
} = useAssessmentDetail();

</script>

<template>
    <DetailPanel :title="title" icon="fa fa-file-text-o" form-name="assessmentForm" main-labelledby="main-title"
 :can-save="canEdit && !isSaving && !isLoading" :show-buttons="canEdit" @save="save" @cancel="cancel">
        <template #fields>
            <h1 id="main-title" class="visually-hidden">{{ title }}</h1>

            <!-- A read-only viewer (assessments.view only) gets the same form with every control disabled. -->
            <fieldset :disabled="!canEdit || isLoading" class="border-0 p-0 m-0">
                <ExpandPanel :title="panelTitle('CONSUMER', form.patientId)">
                    <div class="row">
                        <FieldSelect v-model="form.providerId" label="Provider" required :options="providers" :disabled="!isNew"
                                     :error="msg('providerId')" col="col-md-6 col-lg-4" />
                        <FieldInput v-model="form.firstName" label="First Name" required :maxlength="150" :error="msg('firstName')" col="col-md-6 col-lg-4" />
                        <FieldInput v-model="form.lastName" label="Last Name" required :maxlength="150" :error="msg('lastName')" col="col-md-6 col-lg-4" />
                        <FieldInput v-model="form.ssn" label="SSN" :maxlength="11" hint="9 digits, or 000-00-0000." :error="msg('ssn')" col="col-md-4 col-lg-3" />
                        <FieldInput v-model="form.providerPatientNo" label="Provider Patient ID" :maxlength="50" :error="msg('providerPatientNo')" col="col-md-4 col-lg-3" />
                        <FieldInput v-model="form.dob" label="Date of Birth" type="date" :error="msg('dob')" col="col-md-4 col-lg-3" />
                        <FieldSelect v-model="form.genderId" label="Gender" :options="lookups.genders" :error="msg('genderId')" col="col-md-4 col-lg-3" />
                        <FieldSelect v-model="form.raceId" label="Race" :options="lookups.races" :error="msg('raceId')" col="col-md-4 col-lg-3" />
                        <FieldSelect v-model="form.ethnicityId" label="Ethnicity" :options="lookups.ethnicities" :error="msg('ethnicityId')" col="col-md-4 col-lg-3" />
                    </div>
                </ExpandPanel>

                <ExpandPanel :title="panelTitle('CRISIS TELEPHONE ASSESSMENT', form.phoneAssessmentId, form.providerPhoneAssessmentId)">
                    <div class="row">
                        <FieldDateTime v-model="dt.callEnded" label="Call End" :error="msg('callEnded')" col="col-md-6 col-lg-4" />
                        <FieldDateTime v-model="dt.dispatchDateTime" label="Dispatch" :disabled="!isDispatched" :error="msg('dispatchDateTime')" col="col-md-6 col-lg-4" />
                        <FieldSelect v-model="form.dispositionId" label="Disposition" :options="lookups.dispositions" :error="msg('dispositionId')" col="col-md-6 col-lg-4" />
                        <FieldInput v-model="form.dispositionOther" label="Disposition, Other" :maxlength="150" :disabled="!isOther" :error="msg('dispositionOther')" col="col-12" />
                        <div class="col-12 mb-3">
                            <label class="form-label" for="notes">Notes</label>
                            <textarea id="notes" v-model="form.notes" class="form-control form-control-sm" rows="3"></textarea>
                        </div>
                    </div>
                </ExpandPanel>

                <ExpandPanel :title="panelTitle('CRISIS FACE TO FACE ASSESSMENT', form.f2FAssessmentId, form.providerF2FAssessmentId)">
                    <p v-if="!form.f2FAssessmentId" class="form-text">Enter an Assessment Date below to include a face-to-face assessment.</p>
                    <div class="row">
                        <FieldSelect v-model="form.assessmentTypeId" label="Assessment Type" :options="lookups.assessmentTypes" :error="msg('assessmentTypeId')" col="col-md-6 col-lg-4" />
                        <FieldDateTime v-model="dt.f2FAssessmentDateTime" label="Assessment" :error="msg('f2FAssessmentDateTime')" col="col-md-6 col-lg-4" />
                        <FieldYesNo v-model="form.transportedByLE" label="Transported by Law Enforcement" :error="msg('transportedByLE')" col="col-md-6 col-lg-4" />
                        <FieldSelect v-model="form.payorSourceId" label="Primary Insurer" :options="lookups.payorSources" :error="msg('payorSourceId')" col="col-md-3" />
                        <FieldSelect v-model="form.secondaryPayorSourceId" label="Payor Billed for Service" :options="lookups.payorSources" :error="msg('secondaryPayorSourceId')" col="col-md-3" />
                        <FieldInput v-model="form.annualHouseholdIncome" label="Annual Gross Household Income" type="number" :min="0" max="9999999.99" step="0.01" :error="msg('annualHouseholdIncome')" col="col-md-3" />
                        <FieldInput v-model="form.numberInHousehold" label="Number of People in Household" type="number" :min="0" :max="255" :error="msg('numberInHousehold')" col="col-md-3" />
                        <FieldSelect v-model="form.assessmentLocationId" label="Consumer Location at Assessment" :options="lookups.assessmentLocations" :error="msg('assessmentLocationId')" col="col-md-3" />
                        <FieldYesNo v-model="form.televideoAssessment" label="Crisis Assessment via Televideo" :error="msg('televideoAssessment')" col="col-md-3" />
                        <FieldSelect v-model="form.currentServicesId" label="Current Services Being Received" :options="lookups.currentServices" :error="msg('currentServicesId')" col="col-md-3" />
                        <FieldSelect v-model="form.mhTreatmentDeclarationId" label="Declaration of MH Treatment" :options="lookups.yesNoUnknown" :error="msg('mhTreatmentDeclarationId')" col="col-md-3" />
                        <FieldSelect v-model="form.motStatusId" label="MOT Status" :options="lookups.yesNoUnknown" :error="msg('motStatusId')" col="col-md-3" />
                        <FieldSelect v-model="form.durablePOAId" label="Durable POA / Conservator / Guardian" :options="lookups.yesNoUnknown" :error="msg('durablePOAId')" col="col-md-3" />
                        <FieldSelect v-model="form.residentialStatusId" label="Residential Status" :options="lookups.residentialStatuses" :error="msg('residentialStatusId')" col="col-md-3" />
                        <FieldSelect v-model="form.countyId" label="County of Residence" :options="lookups.counties" :error="msg('countyId')" col="col-md-3" />
                        <FieldSelect v-model="form.employmentStatusId" label="Employment Status" :options="lookups.employmentStatuses" :error="msg('employmentStatusId')" col="col-md-3" />
                        <FieldInput v-model="form.arrests30Days" label="Number of arrests in last 30 days" type="number" :min="0" :max="255" :error="msg('arrests30Days')" col="col-md-3" />
                        <FieldSelect v-model="form.maritalStatusId" label="Marital Status" :options="lookups.maritalStatuses" :error="msg('maritalStatusId')" col="col-md-3" />
                        <FieldSelect v-model="form.militaryStatusId" label="Military Status" :options="lookups.militaryStatuses" :error="msg('militaryStatusId')" col="col-md-3" />
                        <FieldSelect v-model="form.school3MonthsId" label="Attended School in Last 3 Months" :options="lookups.yesNoUnknown" :error="msg('school3MonthsId')" col="col-md-3" />
                        <FieldSelect v-model="form.educationLevelId" label="Current or Highest Grade Completed" :options="lookups.educationLevels" :error="msg('educationLevelId')" col="col-md-3" />
                    </div>

                    <ExpandPanel title="PRIMARY PROBLEM" :level="3">
                        <div class="row">
                            <FieldSelect v-model="form.primaryProblemId" label="Primary Problem that Led to Recommended Treatment" :options="lookups.primaryProblems"
                                         :error="msg('primaryProblemId')" col="col-md-8" />
                        </div>
                    </ExpandPanel>

                    <ExpandPanel title="MENTAL HEALTH NEEDS" :level="3">
                        <div class="row">
                            <FieldSelect v-model="form.intellectualDisabilityId" label="Intellectual / Developmental Disability" :options="lookups.yesNoUnknown" :error="msg('intellectualDisabilityId')" col="col-md-3" />
                            <FieldSelect v-model="form.medicalInstabilityId" label="Medical / Physical Instability" :options="lookups.yesNoUnknown" :error="msg('medicalInstabilityId')" col="col-md-3" />
                            <FieldSelect v-model="form.medicationIssuesId" label="Medication Compliance Issues" :options="lookups.yesNoUnknown" :error="msg('medicationIssuesId')" col="col-md-3" />
                            <FieldSelect v-model="form.pastTraumaId" label="Past Trauma" :options="lookups.yesNoUnknown" :error="msg('pastTraumaId')" col="col-md-3" />
                        </div>
                    </ExpandPanel>

                    <ExpandPanel title="SUBSTANCE ABUSE" :level="3">
                        <div class="row">
                            <FieldSelect v-model="form.substanceAbuseId" label="Substance Abuse" :options="lookups.yesNoUnknown" :error="msg('substanceAbuseId')" col="col-md-4" />
                            <div class="col-md-8 mb-3 pt-md-4">
                                <label class="checkbox-inline me-3"><input v-model="form.currentDetoxWithdrawal" type="checkbox" /> Current Detox / Withdrawal Symptoms</label>
                                <label class="checkbox-inline"><input v-model="form.historyDetoxWithdrawal" type="checkbox" /> History of Detox / Withdrawal Symptoms</label>
                            </div>
                        </div>
                        <h4 class="h6">Drugs</h4>
                        <table class="table table-sm table-bordered">
                            <thead><tr><th scope="col">Drug</th><th scope="col">Route</th><th scope="col">Frequency</th><th scope="col"><span class="visually-hidden">Remove</span></th></tr></thead>
                            <tbody>
                                <tr v-for="(d, i) in form.drugs" :key="i">
                                    <td>
                                        <select v-model="d.drugId" class="form-select form-select-sm" :aria-label="`Drug ${i + 1}`">
                                            <option :value="null">- - SELECT - -</option>
                                            <option v-for="o in lookups.drugs" :key="o.id" :value="o.id">{{ o.label }}</option>
                                        </select>
                                    </td>
                                    <td>
                                        <select v-model="d.drugRouteId" class="form-select form-select-sm" :aria-label="`Route ${i + 1}`">
                                            <option :value="null">- - SELECT - -</option>
                                            <option v-for="o in lookups.drugRoutes" :key="o.id" :value="o.id">{{ o.label }}</option>
                                        </select>
                                    </td>
                                    <td>
                                        <select v-model="d.drugFrequencyId" class="form-select form-select-sm" :aria-label="`Frequency ${i + 1}`">
                                            <option :value="null">- - SELECT - -</option>
                                            <option v-for="o in lookups.drugFrequencies" :key="o.id" :value="o.id">{{ o.label }}</option>
                                        </select>
                                    </td>
                                    <td><AppButton v-if="!isEmptyRow(d)" action="cancel" size="xs" @click="removeRow(form.drugs, i)">Remove<span class="visually-hidden"> drug {{ i + 1 }}</span></AppButton></td>
                                </tr>
                            </tbody>
                        </table>
                        <div class="form-text has-error" role="alert">{{ msg('drugs') }}</div>
                    </ExpandPanel>

                    <ExpandPanel title="ALTERNATIVES TO HOSPITALIZATION" :level="3">
                        <table class="table table-sm table-bordered">
                            <thead><tr><th scope="col">Alternative</th><th scope="col">Disposition</th><th scope="col"><span class="visually-hidden">Remove</span></th></tr></thead>
                            <tbody>
                                <tr v-for="(a, i) in form.hospAlternatives" :key="i">
                                    <td>
                                        <select v-model="a.hospitalizationAlternativeId" class="form-select form-select-sm" :aria-label="`Alternative ${i + 1}`"
                                                @change="a.hospAltDispositionListId = null">
                                            <option :value="null">- - SELECT - -</option>
                                            <option v-for="o in lookups.hospitalizationAlternatives" :key="o.id" :value="o.id">{{ o.label }}</option>
                                        </select>
                                    </td>
                                    <td>
                                        <select v-model="a.hospAltDispositionListId" class="form-select form-select-sm" :aria-label="`Disposition of alternative ${i + 1}`"
                                                :disabled="!a.hospitalizationAlternativeId">
                                            <option :value="null">- - SELECT - -</option>
                                            <option v-for="o in dispositionsFor(a.hospitalizationAlternativeId)" :key="o.id" :value="o.id">{{ o.label }}</option>
                                        </select>
                                    </td>
                                    <td><AppButton v-if="!isEmptyRow(a)" action="cancel" size="xs" @click="removeRow(form.hospAlternatives, i)">Remove<span class="visually-hidden"> alternative {{ i + 1 }}</span></AppButton></td>
                                </tr>
                            </tbody>
                        </table>
                        <div class="form-text has-error" role="alert">{{ msg('hospAlternatives') }}</div>
                    </ExpandPanel>

                    <ExpandPanel title="HOSPITALIZATION" :level="3">
                        <div class="row">
                            <FieldYesNo v-model="form.voluntaryAdmissionRecommended" label="Voluntary Admission Recommended?" :error="msg('voluntaryAdmissionRecommended')" col="col-md-4" />
                            <FieldYesNo v-model="form.telehealthAdmissionAssessment" label="Admission Assessment via Telehealth?" :error="msg('telehealthAdmissionAssessment')" col="col-md-4" />
                            <FieldSelect v-model="form.firstHospitalizationId" label="1st Hospitalization" :options="lookups.yesNoUnknown" :error="msg('firstHospitalizationId')" col="col-md-4" />
                        </div>
                        <table class="table table-sm table-bordered">
                            <thead><tr><th scope="col">Referred To</th><th scope="col">Disposition</th><th scope="col"><span class="visually-hidden">Remove</span></th></tr></thead>
                            <tbody>
                                <tr v-for="(h, i) in form.hospitalizations" :key="i">
                                    <td>
                                        <select v-model="h.hospitalizationId" class="form-select form-select-sm" :aria-label="`Referred to ${i + 1}`">
                                            <option :value="null">- - SELECT - -</option>
                                            <option v-for="o in lookups.hospitalizations" :key="o.id" :value="o.id">{{ o.label }}</option>
                                        </select>
                                    </td>
                                    <td>
                                        <select v-model="h.hospitalizationDispositionId" class="form-select form-select-sm" :aria-label="`Disposition of referral ${i + 1}`">
                                            <option :value="null">- - SELECT - -</option>
                                            <option v-for="o in lookups.hospitalizationDispositions" :key="o.id" :value="o.id">{{ o.label }}</option>
                                        </select>
                                    </td>
                                    <td><AppButton v-if="!isEmptyRow(h)" action="cancel" size="xs" @click="removeRow(form.hospitalizations, i)">Remove<span class="visually-hidden"> referral {{ i + 1 }}</span></AppButton></td>
                                </tr>
                            </tbody>
                        </table>
                        <div class="form-text has-error" role="alert">{{ msg('hospitalizations') }}</div>
                    </ExpandPanel>

                    <ExpandPanel title="ASSESSMENT COMPLETION / FOLLOW-UP" :level="3">
                        <p v-if="referralAccepted" class="form-text">A referral was accepted: the transport and admission details are required.</p>
                        <div class="row">
                            <FieldSelect v-model="form.recommendedTransportModeId" label="Recommended Mode of Transport" :options="lookups.transportModes" :error="msg('recommendedTransportModeId')" col="col-md-4" />
                            <FieldDateTime v-model="dt.timeDispositionCompleted" label="Disposition Completed" :error="msg('timeDispositionCompleted')" col="col-md-4" />
                            <FieldDateTime v-model="dt.timeTransported" label="Transported to receiving facility" :error="msg('timeTransported')" col="col-md-4" />
                        </div>
                        <h4 class="h6">Assessment Completed By</h4>
                        <div class="row">
                            <FieldInput v-model="form.completedByFirstName" label="First Name" :maxlength="250" :error="msg('completedByFirstName')" col="col-md-6" />
                            <FieldInput v-model="form.completedByLastName" label="Last Name" :maxlength="250" :error="msg('completedByLastName')" col="col-md-6" />
                        </div>
                        <h4 class="h6">Follow-Up</h4>
                        <div class="row">
                            <FieldYesNo v-model="form.followupContact" label="Contacted?" col="col-md-3" />
                            <FieldYesNo v-model="form.isAdmitted" label="Was the Patient Admitted?" :error="msg('isAdmitted')" col="col-md-3" />
                            <FieldYesNo v-model="form.followupReportedServiceHelpful" label="Report Service Helpful" :error="msg('followupReportedServiceHelpful')" col="col-md-3" />
                            <FieldInput v-model="form.contactAttempts" label="No. of Attempts to Contact" type="number" :min="0" :max="255" :error="msg('contactAttempts')" col="col-md-3" />
                        </div>
                    </ExpandPanel>
                </ExpandPanel>
            </fieldset>
        </template>

        <template #actions>
            <AppButton v-if="!isNew && canDelete" action="delete" @click="dialog = 'delete'">Delete</AppButton>
        </template>

        <template v-if="!canEdit" #button-row>
            <AppButton action="close" @click="cancel">Back to search</AppButton>
        </template>

        <template #below>
            <AppDialog v-if="dialog === 'delete'" title="Delete assessment" @close="dialog = ''">
                <p v-if="form.f2FAssessmentId">Delete this face to face assessment? The telephone assessment and the consumer stay.</p>
                <p v-else>Delete this telephone assessment? The consumer stays. This cannot be undone.</p>
                <AppButton action="delete" @click="remove">Delete assessment</AppButton>
                <AppButton action="cancel" @click="dialog = ''" />
            </AppDialog>
        </template>
    </DetailPanel>
</template>
