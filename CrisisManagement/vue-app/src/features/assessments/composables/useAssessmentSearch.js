import { ref, reactive, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { assessmentsApi } from '../api/assessmentsApi.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { fieldMessages } from '../../../utils/formErrors.js';
import { announce } from '../../../services/liveAnnouncer.js';

const DEFAULT_CRITERIA = () => ({
    providerId: null, providerPatientNo: '', ssn: '', assessmentDateFrom: '', assessmentDateTo: '',
    lastName: '', firstName: '', completedByLastName: '', completedByFirstName: '',
    f2FAssessmentId: null, phoneAssessmentId: null, providerF2FAssessmentId: '', providerPhoneAssessmentId: '',
    incompleteOnly: false, orderBy: 'assessmentDate', reverse: true
});

// Module scope on purpose: filters and results survive search -> detail -> back within the SPA.
const criteria = reactive(DEFAULT_CRITERIA());
const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 10 });
const assessments = ref([]);
const totalRecords = ref(0);
const providers = ref([]);
const hasSearched = ref(false);
const isSearching = ref(false);
const errors = ref({});

// Empty text boxes are sent as absent, not as "".
const clean = () => Object.fromEntries(Object.entries(criteria).map(([k, v]) => [k, v === '' ? null : v]));

export function useAssessmentSearch() {
    const router = useRouter();
    const { can } = useCapabilities();
    const { logApiError } = useLogger();

    async function getAssessments() {
        errors.value = {};
        isSearching.value = true;
        try {
            const result = await assessmentsApi.search(paging.currentPage, paging.pageSize, clean());
            assessments.value = result.items;
            totalRecords.value = result.totalCount;
            hasSearched.value = true;
            announce(`${result.totalCount} assessments found`);
        } catch (e) {
            // Field problems (400) show next to their inputs; anything else is a toast.
            const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
            if (fieldErrors) errors.value = fieldErrors;
            else logApiError(e);
        } finally {
            isSearching.value = false;
        }
    }

    const setOrder = createSetOrder(criteria, paging, getAssessments);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getAssessments);

    function search() {
        paging.currentPage = 1;
        return getAssessments();
    }

    // Nothing to choose from: preselect the only provider instead of making the user open the list.
    function defaultProvider() {
        if (providers.value.length === 1 && !criteria.providerId) criteria.providerId = providers.value[0].id;
    }

    function showAll() {
        criteria.incompleteOnly = false;
        return search();
    }

    function clear() {
        Object.assign(criteria, DEFAULT_CRITERIA());
        criteria.incompleteOnly = true;   // like WebForms, Clear returns to the incomplete work list
        defaultProvider();
        return search();
    }

    // ref=pa opens the phone assessment, ref=f2f the face-to-face one (a phone row with no F2F has F2F id 0).
    const keyOf = (a) => (a.f2FAssessmentId > 0 ? `f2f-${a.f2FAssessmentId}` : a.phoneAssessmentId > 0 ? `pa-${a.phoneAssessmentId}` : '');
    const gotoAssessment = (a) => keyOf(a) && router.push(`/assessments/${keyOf(a)}`);
    const canAdd = can('assessments.edit');
    // The provider picked in the search carries over to the new assessment, as in WebForms.
    const add = () => router.push({ path: '/assessments/0', query: criteria.providerId ? { providerId: criteria.providerId } : {} });

    onMounted(async () => {
        if (!providers.value.length) {
            try {
                providers.value = await assessmentsApi.providers();
            } catch (e) {
                logApiError(e);
            }
        }
        defaultProvider();
        // The first visit lists the incomplete assessments (the follow-up work list), as the Blazor CMS did.
        if (!hasSearched.value) criteria.incompleteOnly = true;
        await (hasSearched.value ? getAssessments() : search());
    });

    const msg = fieldMessages(errors);

    return {
        msg, criteria, paging, assessments, totalRecords, providers, isSearching, errors,
        search, showAll, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, keyOf, gotoAssessment, canAdd, add
    };
}
