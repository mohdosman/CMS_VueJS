import { ref, reactive, computed, watch, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { assessmentsApi } from '../api/assessmentsApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { apiErrorMessage } from '../../../utils/apiError.js';
import { announce } from '../../../services/liveAnnouncer.js';

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
const splitDateTime = (v) => (v ? { date: v.slice(0, 10), time: v.slice(11, 16) } : blankDateTime());

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

// One form for a new assessment (/assessments/0) and for an existing one (/assessments/f2f-<id> or /assessments/pa-<id>).
export function useAssessmentDetail() {
    const route = useRoute();
    const router = useRouter();
    const { can } = useCapabilities();
    const { logSuccess, logApiError } = useLogger();

    const key = ref(route.params.key);
    const isNew = computed(() => key.value === '0');
    const canEdit = can('assessments.edit');
    const canDelete = can('assessments.delete');

    const form = reactive(blankForm());
    const dt = reactive(Object.fromEntries(DATE_TIMES.map((f) => [f, blankDateTime()])));
    const lookups = ref({});
    const providers = ref([]);
    const errors = ref({});           // { field: [messages] } from a 400 response or the date/time checks
    const formError = ref('');        // page-level message: a load failure
    const dialog = ref('');           // '' or 'delete'
    const isLoading = ref(true);
    const isSaving = ref(false);

    const title = computed(() => (isNew.value ? 'Enter Assessment' : 'Edit Assessment'));
    const isDispatched = computed(() => form.dispositionId === MOBILE_CRISIS);
    const isOther = computed(() => form.dispositionId === OTHER);
    const referralAccepted = computed(() => form.hospitalizations.some((h) => h.hospitalizationDispositionId === REFERRAL_ACCEPTED));

    // Only these dispositions carry a dispatch time / detail; the fields empty when another one is chosen.
    watch(() => form.dispositionId, (id) => {
        if (id !== MOBILE_CRISIS) dt.dispatchDateTime = blankDateTime();
        if (id !== OTHER) form.dispositionOther = '';
    });

    // Each grid always ends with one blank row (WebForms footer row); choosing its first select adds the next blank row. Blank rows are dropped on save.
    const GRIDS = [
        [() => form.drugs, () => ({ drugId: null, drugRouteId: null, drugFrequencyId: null })],
        [() => form.hospAlternatives, () => ({ hospitalizationAlternativeId: null, hospAltDispositionListId: null })],
        [() => form.hospitalizations, () => ({ hospitalizationId: null, hospitalizationDispositionId: null })]
    ];
    const isEmptyRow = (row) => Object.values(row)[0] == null; // like the WebForms footer row: the first select decides
    watch(() => GRIDS.map(([rows]) => rows()), () => {
        for (const [rows, blank] of GRIDS) if (!rows().length || !isEmptyRow(rows().at(-1))) rows().push(blank());
    }, { deep: true, immediate: true });

    const msg = (f) => errors.value[f]?.join(' ') ?? '';
    const allErrors = computed(() => Object.values(errors.value).flat());

    function fill(detail) {
        Object.assign(form, blankForm(), Object.fromEntries(Object.entries(detail).filter(([, v]) => v !== null)));
        form.dob = detail.dob ? detail.dob.slice(0, 10) : '';
        for (const f of DATE_TIMES) dt[f] = splitDateTime(detail[f]);
    }

    async function load(k) {
        isLoading.value = true;
        formError.value = '';
        try {
            const detail = await assessmentsApi.get(k);
            fill(detail);
            // A phone call that already has a face to face assessment opens as that assessment.
            if (detail.key && detail.key !== k) {
                key.value = detail.key;
                router.replace(`/assessments/${detail.key}`);
            }
        } catch (e) {
            formError.value = e.response?.status === 404 ? 'Assessment not found.' : apiErrorMessage(e);
        } finally {
            isLoading.value = false;
        }
    }

    // The API wants one value per date-time; a date without a time (or the reverse) is caught here.
    function toPayload() {
        const problems = {};
        const payload = { ...form };
        for (const k of ['drugs', 'hospAlternatives', 'hospitalizations']) payload[k] = form[k].filter((r) => !isEmptyRow(r));
        for (const f of DATE_TIMES) {
            const { date, time } = dt[f];
            if (date && !time) problems[f] = [`Please enter the ${DATE_TIME_LABELS[f]} time.`];
            else if (!date && time) problems[f] = [`Please enter the ${DATE_TIME_LABELS[f]} date.`];
            payload[f] = date && time ? `${date}T${time}` : null;
        }
        for (const [k, v] of Object.entries(payload)) if (v === '') payload[k] = null;
        return { payload, problems };
    }

    // Field problems (400) show next to their inputs and in the summary; anything else (403, 409 conflict, ...) is a toast.
    function fail(e) {
        const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
        if (fieldErrors) {
            errors.value = fieldErrors;
            announce(`${Object.values(fieldErrors).flat().length} problems were found`);
        } else logApiError(e);
    }

    async function save() {
        errors.value = {};
        const { payload, problems } = toPayload();
        if (Object.keys(problems).length) {
            errors.value = problems;
            announce('Please correct the date and time fields');
            return;
        }
        isSaving.value = true;
        try {
            const saved = isNew.value ? await assessmentsApi.create(payload) : await assessmentsApi.update(key.value, payload);
            logSuccess('Assessment saved.');
            key.value = saved.key;
            if (route.params.key !== saved.key) router.replace(`/assessments/${saved.key}`);
            await load(saved.key);
        } catch (e) {
            fail(e);
        } finally {
            isSaving.value = false;
        }
    }

    async function remove() {
        try {
            await assessmentsApi.remove(key.value);
            logSuccess('Assessment deleted.');
            router.push('/assessments');
        } catch (e) {
            dialog.value = '';
            logApiError(e);
        }
    }

    // A panel heading: the title, the record id when there is one, and the provider's own id for it.
    const panelTitle = (label, id, providerNumber) => `${label}${id ? ` #${id}` : ''}${providerNumber ? ` (Provider ID: ${providerNumber})` : ''}`;

    const cancel = () => router.push('/assessments');

    const removeRow = (list, i) => list.splice(i, 1);
    const dispositionsFor = (alternativeId) => (lookups.value.hospAltDispositions ?? [])
        .filter((d) => d.hospitalizationAlternativeId === alternativeId)
        .map((d) => ({ id: d.dispositionListId, label: d.label }));

    onMounted(async () => {
        try {
            [lookups.value, providers.value] = await Promise.all([assessmentsApi.lookups(), assessmentsApi.providers()]);
            if (isNew.value) {
                if (providers.value.length === 1) form.providerId = providers.value[0].id;
                isLoading.value = false;
            } else await load(key.value);
        } catch (e) {
            formError.value = apiErrorMessage(e);
            isLoading.value = false;
        }
    });

    return {
        isNew, canEdit, canDelete, title, form, dt, lookups, providers, errors, formError, dialog, isLoading, isSaving,
        panelTitle, isDispatched, isOther, referralAccepted, msg, allErrors,
        save, remove, cancel, isEmptyRow, removeRow, dispositionsFor
    };
}
