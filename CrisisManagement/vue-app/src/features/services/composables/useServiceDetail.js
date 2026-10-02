import { ref, reactive, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { servicesApi } from '../api/servicesApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { restoreSearchOnReturn } from '../../../common/composables/useSearchState.js';
import { apiErrorMessage } from '../../../utils/apiError.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';

// Port of ManageService.aspx: one form for a new service (/services/new) and an existing one (/services/:key, the service id).
export function useServiceDetail() {
    const route = useRoute();
    const router = useRouter();
    const { logSuccess, logApiError } = useLogger();

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
    const errors = ref({});          // { field: [messages] } from a 400 validation response
    const formError = ref('');       // page-level message: a load failure
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

    const msg = (field) => errors.value[field]?.join(' ') ?? '';

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
    // Loading
    // ================================================================
    function fill(detail) {
        Object.assign(form, blankForm(), Object.fromEntries(Object.entries(detail).filter(([, v]) => v !== null)));
        form.dob = day(detail.dob);
        form.dosAdmitDate = day(detail.dosAdmitDate);
        form.dischargeDate = day(detail.dischargeDate);
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
            formError.value = e.response?.status === 404 ? 'Service not found.' : apiErrorMessage(e);
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
            errors.value = fieldErrors;
        } else {
            logApiError(e);
        }
    }

    async function save() {
        errors.value = {};
        isSaving.value = true;
        try {
            const payload = Object.fromEntries(Object.entries(form).map(([k, v]) => [k, v === '' ? null : v]));
            if (isNew) {
                await servicesApi.create(payload);
                logSuccess('Service created.');
                Object.assign(form, blankForm(form.providerId));
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
        formError.value = '';
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
        isLoading, isSaving, errors, formError, msg, noProvider,

        // User and permissions
        canSave, canDelete,

        // Actions
        findExistingPatient, save, remove, cancel,
        setSessionOrder, sessionSortIcon, onSessionPageChanged, onSessionPageSizeChanged
    };
}
