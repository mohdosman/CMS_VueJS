import { ref, reactive, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { servicesApi } from '../api/servicesApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { apiErrorMessage } from '../../../utils/apiError.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';

const blankForm = (providerId = null) => ({
    id: null, rowVersion: null, providerId, patientId: null,
    providerPatientNo: '', ssn: '', firstName: '', lastName: '', dob: '', genderId: null, countyId: null,
    payorSourceId: null, primaryInsurerId: null, serviceCodeId: null, dosAdmitDate: '', dischargeDate: '', durationHours: null, serviceCountyId: null
});

const day = (v) => (v ? v.slice(0, 10) : '');

// One form for a new service (/services/new) and for an existing one (/services/:key, the service id).
export function useServiceDetail() {
    const route = useRoute();
    const router = useRouter();
    const { can } = useCapabilities();
    const { logSuccess, logApiError } = useLogger();

    const key = route.params.key ?? '0';
    const isNew = key === '0';
    const canSave = isNew ? can('services.enter') : can('services.edit');
    const canDelete = can('services.delete');

    const form = reactive(blankForm());
    const lookups = ref({});
    const providers = ref([]);
    const errors = ref({});          // { field: [messages] } from a 400 validation response
    const formError = ref('');       // page-level message: a load failure
    const dialog = ref('');          // '' or 'delete'
    const isLoading = ref(true);
    const isSaving = ref(false);
    const sessionServices = ref([]); // what was entered in this session (new mode)
    const sessionTotal = ref(0);
    const sessionPaging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 15 });
    const sessionCriteria = reactive({ orderBy: 'serviceId', reverse: true });
    const noProvider = ref(false);   // a user who is assigned no provider cannot enter a service

    const title = isNew ? 'Enter Service' : `Manage Service - ID : ${key}`;

    // Whether discharge date and duration are required depends on the service code; no code (or an unknown one) requires both.
    const rule = computed(() => (lookups.value.serviceCodeRules ?? []).find((r) => r.serviceCodeId === form.serviceCodeId));
    const dischargeRequired = computed(() => rule.value?.isDischargeDateRequired ?? true);
    const durationRequired = computed(() => rule.value?.isDurationHoursRequired ?? true);

    // The patient block is fixed once a patient was found (or the service exists): only the first name and gender may change, and only when entering.
    const patientLocked = computed(() => !isNew || !!form.patientId);

    const msg = (f) => errors.value[f]?.join(' ') ?? '';

    function fill(detail) {
        Object.assign(form, blankForm(), Object.fromEntries(Object.entries(detail).filter(([, v]) => v !== null)));
        form.dob = day(detail.dob);
        form.dosAdmitDate = day(detail.dosAdmitDate);
        form.dischargeDate = day(detail.dischargeDate);
    }

    const clearPatient = () => Object.assign(form, { patientId: null, ssn: '', firstName: '', lastName: '', dob: '', genderId: null });

    // Typing a provider patient number looks for a patient the provider already has under it.
    let lastLookup = '';
    async function findExistingPatient() {
        const number = form.providerPatientNo.trim();
        if (!isNew || !form.providerId || !number || lastLookup === `${form.providerId}|${number}`) return;
        lastLookup = `${form.providerId}|${number}`;
        clearPatient();
        try {
            const found = await servicesApi.existingPatients(form.providerId, form.providerPatientNo.trim());
            if (found.length === 1) {
                const p = found[0];
                Object.assign(form, { patientId: p.patientId, ssn: p.ssn ?? '', firstName: p.firstName ?? '', lastName: p.lastName, dob: day(p.dob), genderId: p.genderId ?? null });
            }
        } catch (e) {
            logApiError(e);
        }
    }

    async function loadSessionServices() {
        try {
            const r = await servicesApi.searchCurrentSession(sessionPaging.currentPage, sessionPaging.pageSize, sessionCriteria);
            sessionServices.value = r.items;
            sessionTotal.value = r.totalCount;
        } catch (e) {
            logApiError(e);
        }
    }

    const setSessionOrder = createSetOrder(sessionCriteria, sessionPaging, loadSessionServices);
    const sessionSortIcon = (col) => getSortIcon(col, sessionCriteria);
    const { onPageChanged: onSessionPageChanged, onPageSizeChanged: onSessionPageSizeChanged } = createPagingHandlers(sessionPaging, loadSessionServices);

    // Field problems (400) show next to their inputs; anything else (403, 409 conflict, ...) is a toast.
    function fail(e) {
        const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
        if (fieldErrors) errors.value = fieldErrors;
        else logApiError(e);
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
                router.push('/services');
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
            router.push('/services');
        } catch (e) {
            dialog.value = '';
            logApiError(e);
        }
    }

    const cancel = () => router.push('/services');

    onMounted(async () => {
        try {
            [lookups.value, providers.value] = await Promise.all([servicesApi.lookups(), servicesApi.entryProviders()]);
            if (isNew) {
                if (providers.value.length === 1) form.providerId = providers.value[0].id;
                noProvider.value = providers.value.length === 0;
                await loadSessionServices();
            } else fill(await servicesApi.get(key));
        } catch (e) {
            formError.value = e.response?.status === 404 ? 'Service not found.' : apiErrorMessage(e);
        } finally {
            isLoading.value = false;
        }
    });

    return {
        isNew, canSave, canDelete, title, form, lookups, providers, errors, formError, dialog, isLoading, isSaving,
        sessionServices, sessionTotal, sessionPaging, setSessionOrder, sessionSortIcon, onSessionPageChanged, onSessionPageSizeChanged, noProvider, dischargeRequired, durationRequired, patientLocked, msg,
        findExistingPatient, save, remove, cancel
    };
}
