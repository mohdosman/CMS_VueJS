import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { servicesApi } from '../api/servicesApi.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useSearchState } from '../../../common/composables/useSearchState.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { fieldMessages } from '../../../utils/formErrors.js';
import { announce } from '../../../services/liveAnnouncer.js';

// Port of SearchService.aspx.
export function useServiceSearch() {
    const router = useRouter();
    const { logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const services = ref([]);
    const totalRecords = ref(0);
    const providers = ref([]);
    const serviceCodes = ref([]);
    const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 20 });

    const DEFAULT_CRITERIA = {
        providerId: null, providerPatientNo: '', ssn: '', lastName: '', firstName: '', serviceCodeId: null,
        dosAdmitDateFrom: '', dosAdmitDateTo: '', serviceFileId: null, orderBy: 'dosAdmitDate', reverse: true
    };
    const criteria = reactive({ ...DEFAULT_CRITERIA });

    const { load, save, clear: clearState } = useSearchState('serviceSearchJSON', criteria, DEFAULT_CRITERIA, paging);

    const hasSearched = ref(false);
    const isSearching = ref(false);
    const errors = ref({});

    // Each async load bumps its counter, so a slow earlier response can't overwrite a newer one.
    let requestSequence = 0;

    const msg = fieldMessages(errors);

    // The text boxes under the provider and service filters.
    const fields = [
        { f: 'providerPatientNo', label: 'Provider Patient ID', col: 'col-md-3' },
        { f: 'ssn', label: 'SSN', col: 'col-md-3' },
        { f: 'lastName', label: 'Last Name', col: 'col-md-3' },
        { f: 'firstName', label: 'First Name', col: 'col-md-3' }
    ];

    // Empty text boxes are sent as absent, not as "".
    const cleanCriteria = () => Object.fromEntries(Object.entries(criteria).map(([k, v]) => [k, v === '' ? null : v]));

    // ================================================================
    // Sorting, paging and search
    // ================================================================
    const setOrder = createSetOrder(criteria, paging, getServices);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getServices);

    async function getServices() {
        const requestId = ++requestSequence;
        errors.value = {};
        isSearching.value = true;
        try {
            const result = await servicesApi.search(paging.currentPage, paging.pageSize, cleanCriteria());
            if (requestId !== requestSequence) {
                return;
            }
            services.value = result.items;
            totalRecords.value = result.totalCount;
            hasSearched.value = true;
            announce(`${result.totalCount} services found`);
        } catch (e) {
            if (requestId === requestSequence) {
                // Field problems (400) show next to their inputs; anything else is a toast.
                const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
                if (fieldErrors) {
                    errors.value = fieldErrors;
                } else {
                    logApiError(e);
                }
            }
        } finally {
            if (requestId === requestSequence) {
                isSearching.value = false;
            }
        }
    }

    async function search() {
        paging.currentPage = 1;
        save();
        await getServices();
    }

    async function getLookups() {
        try {
            [providers.value, serviceCodes.value] = await Promise.all([servicesApi.providers(), servicesApi.serviceCodes()]);
        } catch (e) {
            logApiError(e);
        }
    }

    // Nothing to choose from: preselect the only provider instead of making the user open the list.
    function defaultProvider() {
        if (providers.value.length === 1 && !criteria.providerId) {
            criteria.providerId = providers.value[0].id;
        }
    }

    // Runs each time the screen is shown: restore the saved criteria and list again. The page opens on the full list, as WebForms does.
    useActivate(async () => {
        load();
        await getLookups();
        defaultProvider();
        await getServices();
    });

    // ================================================================
    // Navigation and clear
    // ================================================================
    function gotoService(service) {
        router.push(`/services/${service.serviceId}`);
    }

    // Like WebForms, Clear goes back to the full list.
    async function clear() {
        clearState();
        defaultProvider();
        await getServices();
    }

    return {
        // Results
        services, totalRecords, paging, criteria, providers, serviceCodes, hasSearched, fields,

        // Busy and validation state
        isSearching, errors, msg,

        // Actions
        search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged,
        gotoService
    };
}
