import { ref, reactive, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { servicesApi } from '../api/servicesApi.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { fieldMessages } from '../../../utils/formErrors.js';
import { announce } from '../../../services/liveAnnouncer.js';

const DEFAULT_CRITERIA = () => ({
    providerId: null, providerPatientNo: '', ssn: '', lastName: '', firstName: '', serviceCodeId: null,
    dosAdmitDateFrom: '', dosAdmitDateTo: '', serviceFileId: null, orderBy: 'dosAdmitDate', reverse: true
});

// Module scope on purpose: filters and results survive search -> detail -> back within the SPA.
const criteria = reactive(DEFAULT_CRITERIA());
const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 10 });
const services = ref([]);
const totalRecords = ref(0);
const providers = ref([]);
const serviceCodes = ref([]);
const hasSearched = ref(false);
const isSearching = ref(false);
const errors = ref({});

// Empty text boxes are sent as absent, not as "".
const clean = () => Object.fromEntries(Object.entries(criteria).map(([k, v]) => [k, v === '' ? null : v]));

export function useServiceSearch() {
    const router = useRouter();
    const { can } = useCapabilities();
    const { logApiError } = useLogger();

    async function getServices() {
        errors.value = {};
        isSearching.value = true;
        try {
            const result = await servicesApi.search(paging.currentPage, paging.pageSize, clean());
            services.value = result.items;
            totalRecords.value = result.totalCount;
            hasSearched.value = true;
            announce(`${result.totalCount} services found`);
        } catch (e) {
            // Field problems (400) show next to their inputs; anything else is a toast.
            const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
            if (fieldErrors) errors.value = fieldErrors;
            else logApiError(e);
        } finally {
            isSearching.value = false;
        }
    }

    const setOrder = createSetOrder(criteria, paging, getServices);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getServices);

    function search() {
        paging.currentPage = 1;
        return getServices();
    }

    // Nothing to choose from: preselect the only provider instead of making the user open the list.
    function defaultProvider() {
        if (providers.value.length === 1 && !criteria.providerId) criteria.providerId = providers.value[0].id;
    }

    function clear() {
        Object.assign(criteria, DEFAULT_CRITERIA());
        defaultProvider();
        services.value = [];
        totalRecords.value = 0;
        hasSearched.value = false;
        errors.value = {};
    }

    const canAdd = can('services.enter');
    const add = () => router.push('/services/new');

    onMounted(async () => {
        if (!providers.value.length) {
            try {
                [providers.value, serviceCodes.value] = await Promise.all([servicesApi.providers(), servicesApi.serviceCodes()]);
            } catch (e) {
                logApiError(e);
            }
        }
        defaultProvider();
        if (hasSearched.value) await getServices();
    });

    const msg = fieldMessages(errors);

    return {
        msg, criteria, paging, services, totalRecords, providers, serviceCodes, hasSearched, isSearching, errors,
        search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, canAdd, add
    };
}
