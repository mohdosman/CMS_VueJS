import { ref, reactive, computed, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { assessmentsApi } from '../api/assessmentsApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { restoreSearchOnReturn } from '../../../common/composables/useSearchState.js';
import { createShowError, createTouch, requireSelect } from '../../../utils/validationUtils.js';

// Port of ManageAssessment.aspx: one form for a new assessment (/assessments/0) and an existing one (/assessments/f2f-<id> or /assessments/pa-<id>).
export function useAssessmentDetail() {
    const route = useRoute();
    const router = useRouter();
    const { logSuccess, logError, logApiError } = useLogger();

    // ================================================================
    // Constants and blank shapes
    // ================================================================
    const MOBILE_CRISIS = 4;
    const OTHER = 6;
    const REFERRAL_ACCEPTED = 1;

    // The fields that hold a date and a time; the form edits each as { date, time } and the API takes "yyyy-MM-ddTHH:mm".
    const DATE_TIMES = ['callEnded', 'dispatchDateTime', 'f2FAssessmentDateTime', 'timeDispositionCompleted', 'timeTransported'];
    const DATE_TIME_LABELS = {
        callEnded: 'Call End', dispatchDateTime: 'Dispatch', f2FAssessmentDateTime: 'Assessment',
        timeDispositionCompleted: 'Disposition Completed', timeTransported: 'Transported'
    };

    const blankDateTime = () => ({ date: '', time: '' });
    const splitDateTime = (value) => (value ? { date: value.slice(0, 10), time: value.slice(11, 16) } : blankDateTime());

    const blankForm = () => ({
        key: '', rowVersion: null, f2FAssessmentId: null, phoneAssessmentId: null, providerF2FAssessmentId: null, providerPhoneAssessmentId: null,
        patientId: null, providerId: null,
        firstName: '', lastName: '', ssn: '', providerPatientNo: '', dob: '', genderId: null, raceId: null, ethnicityId: null,
        dispositionId: null, dispositionOther: '', notes: '',
        assessmentTypeId: null, transportedByLE: null, payorSourceId: null, secondaryPayorSourceId: null, annualHouseholdIncome: null,
        numberInHousehold: null, assessmentLocationId: null, televideoAssessment: null, currentServicesId: null, mhTreatmentDeclarationId: null,
        motStatusId: null, durablePOAId: null, residentialStatusId: null, countyId: null, employmentStatusId: null, arrests30Days: null,
        maritalStatusId: null, militaryStatusId: null, school3MonthsId: null, educationLevelId: null,
        primaryProblemId: null, intellectualDisabilityId: null, medicalInstabilityId: null, medicationIssuesId: null, pastTraumaId: null,
        substanceAbuseId: null, currentDetoxWithdrawal: false, historyDetoxWithdrawal: false,
        drugs: [], hospAlternatives: [], voluntaryAdmissionRecommended: null, telehealthAdmissionAssessment: null, firstHospitalizationId: null,
        hospitalizations: [], recommendedTransportModeId: null, completedByFirstName: '', completedByLastName: '',
        followupContact: null, isAdmitted: null, followupReportedServiceHelpful: null, contactAttempts: null
    });

    // ================================================================
    // State
    // ================================================================
    const key = ref(route.params.key);
    const isNew = computed(() => key.value === '0');
    const title = computed(() => (isNew.value ? 'Enter Assessment' : 'Edit Assessment'));

    const form = reactive(blankForm());
    const dt = reactive(Object.fromEntries(DATE_TIMES.map((field) => [field, blankDateTime()])));
    const lookups = ref({});
    const providers = ref([]);
    const serverErrors = ref({});     // { field: [messages] } from a 400 response
    const dialog = ref('');           // '' or 'delete'
    const isLoading = ref(true);
    const isSaving = ref(false);

    // The key the page was last loaded for, so a url change the page made itself does not load it twice.
    let loadedKey = null;

    const isDispatched = computed(() => form.dispositionId === MOBILE_CRISIS);
    const isOther = computed(() => form.dispositionId === OTHER);
    const referralAccepted = computed(() => form.hospitalizations.some((h) => h.hospitalizationDispositionId === REFERRAL_ACCEPTED));

    // A panel heading: the title, the record id when there is one, and the provider's own id for it.
    const panelTitle = (label, id, providerNumber) => `${label}${id ? ` #${id}` : ''}${providerNumber ? ` (Provider ID: ${providerNumber})` : ''}`;

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canEdit = can('assessments.edit');
    const canDelete = can('assessments.delete');

    // ================================================================
    // Field rules
    // ================================================================
    // Only these dispositions carry a dispatch time / detail; the fields empty when another one is chosen.
    watch(() => form.dispositionId, (id) => {
        if (id !== MOBILE_CRISIS) {
            dt.dispatchDateTime = blankDateTime();
        }
        if (id !== OTHER) {
            form.dispositionOther = '';
        }
    });

    // ================================================================
    // Grids: drugs, alternatives to hospitalization and referrals
    // ================================================================
    // Each grid always ends with one blank row (WebForms footer row); choosing its first select adds the next blank row. Blank rows are dropped on save.
    const GRIDS = [
        [() => form.drugs, () => ({ drugId: null, drugRouteId: null, drugFrequencyId: null })],
        [() => form.hospAlternatives, () => ({ hospitalizationAlternativeId: null, hospAltDispositionListId: null })],
        [() => form.hospitalizations, () => ({ hospitalizationId: null, hospitalizationDispositionId: null })]
    ];
    const isEmptyRow = (row) => Object.values(row)[0] == null; // like the WebForms footer row: the first select decides

    watch(() => GRIDS.map(([rows]) => rows()), () => {
        for (const [rows, blank] of GRIDS) {
            if (!rows().length || !isEmptyRow(rows().at(-1))) {
                rows().push(blank());
            }
        }
    }, { deep: true, immediate: true });

    const removeRow = (list, index) => list.splice(index, 1);

    const dispositionsFor = (alternativeId) => (lookups.value.hospAltDispositions ?? [])
        .filter((d) => d.hospitalizationAlternativeId === alternativeId)
        .map((d) => ({ id: d.dispositionListId, label: d.label }));

    // ================================================================
    // Validation
    // ================================================================
    // The checks that need only the form itself (required fields, and the date and time halves). The server repeats them and also
    // checks the rest (date order, duplicates, SSN, DOB), and its messages show the same way.
    const submitted = ref(false);
    // Which fields the user has visited. A field only shows its message once it was left, or after a submit.
    const touched = reactive({});
    const touch = createTouch(touched);

    const YES = 1;
    const blank = (value) => !String(value ?? '').trim();
    const hasDateTime = (field) => !!dt[field].date && !!dt[field].time;

    // The error message for each field; a field that is fine has no entry.
    const clientErrors = computed(() => {
        const e = {};
        const add = (field, message) => {
            if (!e[field]) {
                e[field] = message;
            }
        };
        const required = (field, label) => blank(form[field]) && add(field, `${label} is required.`);
        const select = (field, message) => !requireSelect(form[field]) && add(field, message);
        const yesNo = (field, message) => form[field] === null && add(field, message);
        const dateTime = (field, message) => {
            const { date, time } = dt[field];
            if (date && !time) {
                add(field, `Please enter the ${DATE_TIME_LABELS[field]} time.`);
            } else if (!date && time) {
                add(field, `Please enter the ${DATE_TIME_LABELS[field]} date.`);
            } else if (message && !date) {
                add(field, message);
            }
        };
        const entered = (rows) => rows.filter((row) => !isEmptyRow(row));

        // A date without a time (or the reverse) is a problem in any panel.
        for (const field of DATE_TIMES) {
            dateTime(field, '');
        }

        // The face to face panel counts when it has an assessment date; the phone panel when it has anything, or when there is no face to face.
        const isF2F = hasDateTime('f2FAssessmentDateTime');
        const wantsPhone = !isF2F || hasDateTime('callEnded') || requireSelect(form.dispositionId) || hasDateTime('dispatchDateTime');

        // Consumer
        required('firstName', 'First Name');
        required('lastName', 'Last Name');

        // Crisis telephone
        if (wantsPhone) {
            dateTime('callEnded', 'Call End Date is Required!');
            select('dispositionId', 'Disposition is required');
            if (form.dispositionId === OTHER && blank(form.dispositionOther)) {
                add('dispositionOther', "Detail must be provided when Disposition is 'Other'");
            }
            if (form.dispositionId === MOBILE_CRISIS && !hasDateTime('dispatchDateTime')) {
                add('dispatchDateTime', 'Dispatch Date and Time are required for Mobile Crisis Staff.');
            }
        }

        if (isF2F) {
            select('genderId', 'Please select the Gender!');
            select('raceId', 'Please select the Race!');
            select('ethnicityId', 'Please select the Ethnicity!');
            select('assessmentTypeId', 'Please select the Assessment Type!');
            yesNo('transportedByLE', 'Transported by Law Enforcement is required.');
            select('payorSourceId', 'Primary Insurer is required');
            dateTime('timeDispositionCompleted', 'Date Disposition Completed is Required!');
            required('completedByFirstName', 'Assessment Completed By First Name');
            required('completedByLastName', 'Assessment Completed By Last Name');

            select('assessmentLocationId', 'Please select the Consumer Location at Assessment!');
            yesNo('televideoAssessment', 'Assessment via Televideo is required.');
            select('currentServicesId', 'Please select the Current Services Being Received!');
            select('mhTreatmentDeclarationId', 'Please select the Declaration of MH Treatment!');
            select('motStatusId', 'Please select the MOT Status!');
            select('durablePOAId', 'Please select the Durable POA!');
            select('residentialStatusId', 'Please select the Residential Status!');
            select('countyId', 'Please select the County of Residence!');
            select('employmentStatusId', 'Please select the Employment Status!');
            select('maritalStatusId', 'Please select the Marital Status!');
            select('militaryStatusId', 'Please select the Military Status!');
            select('school3MonthsId', 'Please select the Attended school in last 3 months!');
            select('educationLevelId', 'Please select the Current or highest grade completed!');
            select('primaryProblemId', 'Please select the Primary Problem that lead to Recommended Treatment!');
            select('intellectualDisabilityId', 'Please select the Intellectual / Development Disability!');
            select('medicalInstabilityId', 'Please select the Medical / Physical Instability!');
            select('medicationIssuesId', 'Please select the Medication Compliance Issues!');
            select('pastTraumaId', 'Please select the Declaration of Past Trauma!');
            select('substanceAbuseId', 'Please select the Substance Abuse!');

            // Drugs count only when Substance Abuse is Yes.
            if (form.substanceAbuseId === YES) {
                const drugs = entered(form.drugs);
                if (!drugs.length) {
                    add('drugs', 'At least one drug entry is required when Substance Abuse is selected.');
                } else if (drugs.some((d) => !requireSelect(d.drugRouteId))) {
                    add('drugs', 'Drug Route is required.');
                } else if (drugs.some((d) => !requireSelect(d.drugFrequencyId))) {
                    add('drugs', 'Drug Frequency is required.');
                }
            }

            const alternatives = entered(form.hospAlternatives);
            if (!alternatives.length) {
                add('hospAlternatives', 'Alternative to Hospitalization is required.');
            } else if (alternatives.some((a) => !requireSelect(a.hospAltDispositionListId))) {
                add('hospAlternatives', 'Alt Disposition is required.');
            }

            if (entered(form.hospitalizations).some((h) => !requireSelect(h.hospitalizationDispositionId))) {
                add('hospitalizations', 'Referred To Disposition is required.');
            }

            // A referral that was accepted means the person was sent on: transport and admission details are then required.
            if (referralAccepted.value) {
                dateTime('timeTransported', 'Date transported to receiving facility is Required!');
                select('recommendedTransportModeId', 'Please select the Recommended mode of Transport!');
                select('firstHospitalizationId', 'Please select the 1st Hospitalization!');
                yesNo('voluntaryAdmissionRecommended', 'Voluntary Admission Recommended is required!');
                if (form.followupContact === true) {
                    yesNo('isAdmitted', 'Was the patient admitted is required!');
                }
                yesNo('telehealthAdmissionAssessment', 'Admission assessment via telehealth is required!');
            }

            if (form.followupContact === true && !referralAccepted.value) {
                yesNo('followupReportedServiceHelpful', 'Report Service Helpful is required!');
            }
            if (form.followupContact === false) {
                if (form.contactAttempts === null || form.contactAttempts === '') {
                    add('contactAttempts', 'No. of Attempts to contact is required!');
                } else if (form.contactAttempts <= 0) {
                    add('contactAttempts', 'Please provide the valid No. of Attempts to contact!');
                }
            }
        }
        return e;
    });

    const isValid = computed(() => Object.keys(clientErrors.value).length === 0);
    // A field shows its message only after it was touched, or after a submit was attempted.
    const showError = createShowError(touched, submitted, clientErrors);

    // What the screen shows for a field: the server's message when it sent one, else the form's own once it is due.
    const msg = (field) => serverErrors.value[field]?.join(' ') || (showError(field) ? clientErrors.value[field] : '');

    function resetValidation() {
        submitted.value = false;
        serverErrors.value = {};
        Object.keys(touched).forEach((field) => delete touched[field]);
    }

    // ================================================================
    // Loading
    // ================================================================
    function fill(detail) {
        Object.assign(form, blankForm(), Object.fromEntries(Object.entries(detail).filter(([, v]) => v !== null)));
        form.dob = detail.dob ? detail.dob.slice(0, 10) : '';
        for (const field of DATE_TIMES) {
            dt[field] = splitDateTime(detail[field]);
        }
    }

    async function load(assessmentKey) {
        isLoading.value = true;
        try {
            const detail = await assessmentsApi.get(assessmentKey);
            fill(detail);
            resetValidation();
            loadedKey = assessmentKey;
            // A phone call that already has a face to face assessment opens as that assessment.
            if (detail.key && detail.key !== assessmentKey) {
                key.value = detail.key;
                loadedKey = detail.key;
                router.replace(`/assessments/${detail.key}`);
            }
        } catch (e) {
            logApiError(e, { fallback: 'Assessment not found.' });
        } finally {
            isLoading.value = false;
        }
    }

    async function getPageData() {
        try {
            [lookups.value, providers.value] = await Promise.all([assessmentsApi.lookups(), assessmentsApi.providers()]);
            if (isNew.value) {
                // The provider picked in the search carries over; a user with only one provider gets it.
                const wanted = Number(route.query.providerId);
                if (providers.value.some((p) => p.id === wanted)) {
                    form.providerId = wanted;
                } else if (providers.value.length === 1) {
                    form.providerId = providers.value[0].id;
                }
                loadedKey = key.value;
                isLoading.value = false;
            } else {
                await load(key.value);
            }
        } catch (e) {
            logApiError(e);
            isLoading.value = false;
        }
    }

    // ================================================================
    // Navigation
    // ================================================================
    function backToSearch() {
        restoreSearchOnReturn();
        router.push('/assessments');
    }

    function cancel() {
        backToSearch();
    }

    // ================================================================
    // Save and delete
    // ================================================================
    // The API wants one value per date-time ("yyyy-MM-ddTHH:mm"); blank rows are dropped.
    function toPayload() {
        const payload = { ...form };
        for (const grid of ['drugs', 'hospAlternatives', 'hospitalizations']) {
            payload[grid] = form[grid].filter((row) => !isEmptyRow(row));
        }
        for (const field of DATE_TIMES) {
            const { date, time } = dt[field];
            payload[field] = date && time ? `${date}T${time}` : null;
        }
        for (const [name, value] of Object.entries(payload)) {
            if (value === '') {
                payload[name] = null;
            }
        }
        return payload;
    }

    // Field problems (400) show next to their inputs; anything else (403, 409 conflict, ...) is a toast.
    function fail(e) {
        const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
        if (fieldErrors) {
            serverErrors.value = fieldErrors;
            logError('Please correct the validation errors first.');
        } else {
            logApiError(e);
        }
    }

    // Checks permission, then saves. A new or changed record reloads, so its messages start clean.
    async function save() {
        if (isSaving.value) {
            return;
        }
        if (!canEdit) {
            logError('You do not have permission to manage assessments.');
            return;
        }
        const payload = toPayload();
        isSaving.value = true;
        try {
            const saved = isNew.value ? await assessmentsApi.create(payload) : await assessmentsApi.update(key.value, payload);
            logSuccess('Assessment saved.');
            key.value = saved.key;
            loadedKey = saved.key;
            if (route.params.key !== saved.key) {
                router.replace(`/assessments/${saved.key}`);
            }
            await load(saved.key);
        } catch (e) {
            fail(e);
        } finally {
            isSaving.value = false;
        }
    }

    // The form's submit: validates first, and only saves when everything is valid.
    function onSubmit() {
        submitted.value = true;
        serverErrors.value = {};
        if (!isValid.value) {
            logError('Please correct the validation errors first.');
            return;
        }
        save();
    }

    async function remove() {
        try {
            await assessmentsApi.remove(key.value);
            logSuccess('Assessment deleted.');
            backToSearch();
        } catch (e) {
            dialog.value = '';
            logApiError(e);
        }
    }

    // ================================================================
    // Page load
    // ================================================================
    useActivate(async () => {
        // The page already loaded this key (it rewrote the url itself after a save or a redirect).
        if (loadedKey === route.params.key) {
            return;
        }
        key.value = route.params.key;
        isLoading.value = true;
        await getPageData();
    });

    return {
        // Form
        form, dt, lookups, providers, dialog, title, isNew,
        isDispatched, isOther, referralAccepted, panelTitle,

        // Busy and validation state
        isLoading, isSaving, submitted, touched, touch, isValid, showError, msg,

        // User and permissions
        canEdit, canDelete,

        // Actions
        save, onSubmit, remove, cancel, isEmptyRow, removeRow, dispositionsFor
    };
}
