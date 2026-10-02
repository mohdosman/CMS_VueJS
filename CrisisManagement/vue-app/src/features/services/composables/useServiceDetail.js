import { ref, reactive, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { servicesApi } from '../api/servicesApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { restoreSearchOnReturn } from '../../../common/composables/useSearchState.js';
import { createShowError, createTouch, requireSelect } from '../../../utils/validationUtils.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';

// Port of ManageService.aspx: one form for a new service (/services/new) and an existing one (/services/:key, the service id).
export function useServiceDetail() {
    const route = useRoute();
    const router = useRouter();
    const { logSuccess, logError, logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const blankForm = (providerId = null) => ({
        id: null, rowVersion: null, providerId, patientId: null,
        providerPatientNo: '', ssn: '', firstName: '', lastName: '', dob: '', genderId: null, countyId: null,
        payorSourceId: null, primaryInsurerId: null, serviceCodeId: null, dosAdmitDate: '', dischargeDate: '', durationHours: null, serviceCountyId: null
    });
    const day = (value) => (value ? value.slice(0, 10) : '');

    const key = route.params.key ?? '0';
    const isNew = key === '0';
    const title = isNew ? 'Enter Service' : `Manage Service - ID : ${key}`;

    const form = reactive(blankForm());
    const lookups = ref({});
    const providers = ref([]);
    const serverErrors = ref({});    // { field: [messages] } from a 400 validation response
    const dialog = ref('');          // '' or 'delete'
    const isLoading = ref(true);
    const isSaving = ref(false);
    const noProvider = ref(false);   // a user who is assigned no provider cannot enter a service

    // What was entered in this session (new mode).
    const sessionServices = ref([]);
    const sessionTotal = ref(0);
    const sessionPaging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 15 });
    const sessionCriteria = reactive({ orderBy: 'serviceId', reverse: true });

    // The last provider patient number looked up, so the same one is not looked up twice.
    let lastLookup = '';

    // Whether discharge date and duration are required depends on the service code; no code (or an unknown one) requires both.
    const rule = computed(() => (lookups.value.serviceCodeRules ?? []).find((r) => r.serviceCodeId === form.serviceCodeId));
    const dischargeRequired = computed(() => rule.value?.isDischargeDateRequired ?? true);
    const durationRequired = computed(() => rule.value?.isDurationHoursRequired ?? true);

    // The patient block is fixed once a patient was found (or the service exists): only the first name and gender may change, and only when entering.
    const patientLocked = computed(() => !isNew || !!form.patientId);

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canSave = isNew ? can('services.enter') : can('services.edit');
    const canDelete = can('services.delete');

    // ================================================================
    // Validation
    // ================================================================
    const submitted = ref(false);
    // Which fields the user has visited. A field only shows its message once it was left, or after a submit.
    const touched = reactive({});
    const touch = createTouch(touched);

    const blank = (value) => !String(value ?? '').trim();

    // The error message for each field; a field that is fine has no entry. The patient fields only count when entering a service.
    // The server also checks the dates (not in the future, discharge not before admit), the SSN and duplicates.
    const clientErrors = computed(() => {
        const e = {};
        const select = (field, message) => !requireSelect(form[field]) && (e[field] = message);
        select('providerId', 'Please select the Provider!');
        if (isNew) {
            if (blank(form.providerPatientNo)) {
                e.providerPatientNo = 'Provider Patient No is Required.';
            }
            if (blank(form.firstName)) {
                e.firstName = 'First Name is Required.';
            }
            if (blank(form.lastName)) {
                e.lastName = 'Last Name is Required.';
            }
            if (blank(form.dob)) {
                e.dob = 'DOB is Required.';
            }
            select('genderId', 'Please select the Gender!');
        }
        select('countyId', 'Please select the County of Residence.');
        select('payorSourceId', 'Please select the Payor Billed for Service.');
        select('serviceCodeId', 'Please select the Service.');
        if (blank(form.dosAdmitDate)) {
            e.dosAdmitDate = 'DOS or Admit Date is Required.';
        }
        if (dischargeRequired.value && blank(form.dischargeDate)) {
            e.dischargeDate = 'Discharge Date is Required.';
        }
        if (blank(form.durationHours)) {
            if (durationRequired.value) {
                e.durationHours = 'Duration (Hours) is Required.';
            }
        } else if (!Number.isInteger(Number(form.durationHours)) || form.durationHours < 1 || form.durationHours > 999) {
            e.durationHours = 'Please Enter Valid Duration (Hours) greater than 0 upto 3 digits.';
        }
        select('serviceCountyId', 'Please select the County of Service.');
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
        form.dob = day(detail.dob);
        form.dosAdmitDate = day(detail.dosAdmitDate);
        form.dischargeDate = day(detail.dischargeDate);
        resetValidation();
    }

    async function loadSessionServices() {
        try {
            const result = await servicesApi.searchCurrentSession(sessionPaging.currentPage, sessionPaging.pageSize, sessionCriteria);
            sessionServices.value = result.items;
            sessionTotal.value = result.totalCount;
        } catch (e) {
            logApiError(e);
        }
    }

    const setSessionOrder = createSetOrder(sessionCriteria, sessionPaging, loadSessionServices);
    const sessionSortIcon = (col) => getSortIcon(col, sessionCriteria);
    const { onPageChanged: onSessionPageChanged, onPageSizeChanged: onSessionPageSizeChanged } = createPagingHandlers(sessionPaging, loadSessionServices);

    async function getPageData() {
        try {
            [lookups.value, providers.value] = await Promise.all([servicesApi.lookups(), servicesApi.entryProviders()]);
            if (isNew) {
                if (providers.value.length === 1) {
                    form.providerId = providers.value[0].id;
                }
                noProvider.value = providers.value.length === 0;
                await loadSessionServices();
            } else {
                fill(await servicesApi.get(key));
            }
        } catch (e) {
            logApiError(e, { fallback: 'Service not found.' });
        }
    }

    // ================================================================
    // Patient lookup
    // ================================================================
    function clearPatient() {
        Object.assign(form, { patientId: null, ssn: '', firstName: '', lastName: '', dob: '', genderId: null });
    }

    // Typing a provider patient number looks for a patient the provider already has under it.
    async function findExistingPatient() {
        const number = form.providerPatientNo.trim();
        if (!isNew || !form.providerId || !number || lastLookup === `${form.providerId}|${number}`) {
            return;
        }
        lastLookup = `${form.providerId}|${number}`;
        clearPatient();
        try {
            const found = await servicesApi.existingPatients(form.providerId, number);
            if (found.length === 1) {
                const patient = found[0];
                Object.assign(form, {
                    patientId: patient.patientId, ssn: patient.ssn ?? '', firstName: patient.firstName ?? '',
                    lastName: patient.lastName, dob: day(patient.dob), genderId: patient.genderId ?? null
                });
            }
        } catch (e) {
            logApiError(e);
        }
    }

    // ================================================================
    // Navigation
    // ================================================================
    function backToSearch() {
        restoreSearchOnReturn();
        router.push('/services');
    }

    function cancel() {
        backToSearch();
    }

    // ================================================================
    // Save and delete
    // ================================================================
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

    // Checks permission, then saves. Entering a service clears the form for the next one; editing returns to the search.
    async function save() {
        if (isSaving.value) {
            return;
        }
        if (!canSave) {
            logError('You do not have permission to manage services.');
            return;
        }
        isSaving.value = true;
        try {
            const payload = Object.fromEntries(Object.entries(form).map(([k, v]) => [k, v === '' ? null : v]));
            if (isNew) {
                await servicesApi.create(payload);
                logSuccess('Service created.');
                Object.assign(form, blankForm(form.providerId));
                resetValidation();
                lastLookup = '';
                sessionPaging.currentPage = 1;
                await loadSessionServices();
            } else {
                await servicesApi.update(form.id, payload);
                logSuccess('Service updated.');
                backToSearch();
            }
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
            await servicesApi.remove(form.id);
            logSuccess('Service deleted.');
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
        isLoading.value = true;
        await getPageData();
        isLoading.value = false;
    });

    return {
        // Form
        form, lookups, providers, dialog, title, isNew,
        dischargeRequired, durationRequired, patientLocked,

        // Services entered in this session
        sessionServices, sessionTotal, sessionPaging,

        // Busy and validation state
        isLoading, isSaving, submitted, touched, touch, isValid, showError, msg, noProvider,

        // User and permissions
        canSave, canDelete,

        // Actions
        findExistingPatient, save, onSubmit, remove, cancel,
        setSessionOrder, sessionSortIcon, onSessionPageChanged, onSessionPageSizeChanged
    };
}
