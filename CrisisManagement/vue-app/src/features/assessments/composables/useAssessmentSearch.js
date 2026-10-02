import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { assessmentsApi } from '../api/assessmentsApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useSearchState } from '../../../common/composables/useSearchState.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { fieldMessages } from '../../../utils/formErrors.js';
import { announce } from '../../../services/liveAnnouncer.js';

// Port of SearchAssessment.aspx.
export function useAssessmentSearch() {
    const router = useRouter();
    const { logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const assessments = ref([]);
    const totalRecords = ref(0);
    const providers = ref([]);
    const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 10 });

    // The page opens on the incomplete assessments (the follow-up work list), as WebForms does.
    const DEFAULT_CRITERIA = {
        providerId: null, providerPatientNo: '', ssn: '', assessmentDateFrom: '', assessmentDateTo: '',
        lastName: '', firstName: '', completedByLastName: '', completedByFirstName: '',
        f2FAssessmentId: null, phoneAssessmentId: null, providerF2FAssessmentId: '', providerPhoneAssessmentId: '',
        incompleteOnly: true, orderBy: 'assessmentDate', reverse: true
    };
    const criteria = reactive({ ...DEFAULT_CRITERIA });

    const { load, save, clear: clearState } = useSearchState('assessmentSearchJSON', criteria, DEFAULT_CRITERIA, paging);

    const isSearching = ref(false);
    const errors = ref({});

    // Each async load bumps its counter, so a slow earlier response can't overwrite a newer one.
    let requestSequence = 0;

    const msg = fieldMessages(errors);

    // Empty text boxes are sent as absent, not as "".
    const cleanCriteria = () => Object.fromEntries(Object.entries(criteria).map(([k, v]) => [k, v === '' ? null : v]));

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canAdd = can('assessments.edit');

    // ================================================================
    // Sorting, paging and search
    // ================================================================
    const setOrder = createSetOrder(criteria, paging, getAssessments);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getAssessments);

    async function getAssessments() {
        const requestId = ++requestSequence;
        errors.value = {};
        isSearching.value = true;
        try {
            const result = await assessmentsApi.search(paging.currentPage, paging.pageSize, cleanCriteria());
            if (requestId !== requestSequence) {
                return;
            }
            assessments.value = result.items;
            totalRecords.value = result.totalCount;
            announce(`${result.totalCount} assessments found`);
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
        await getAssessments();
    }

    // Nothing to choose from: preselect the only provider instead of making the user open the list.
    function defaultProvider() {
        if (providers.value.length === 1 && !criteria.providerId) {
            criteria.providerId = providers.value[0].id;
        }
    }

    async function getProviders() {
        try {
            providers.value = await assessmentsApi.providers();
        } catch (e) {
            logApiError(e);
        }
    }

    // Runs each time the screen is shown: restore the saved criteria and list again.
    useActivate(async () => {
        load();
        await getProviders();
        defaultProvider();
        await getAssessments();
    });

    async function showAll() {
        criteria.incompleteOnly = false;
        await search();
    }

    // Like WebForms, Clear returns to the incomplete work list.
    async function clear() {
        clearState();
        defaultProvider();
        await getAssessments();
    }

    // ================================================================
    // Navigation
    // ================================================================
    // ref=pa opens the phone assessment, ref=f2f the face-to-face one (a phone row with no F2F has F2F id 0).
    function keyOf(assessment) {
        if (assessment.f2FAssessmentId > 0) {
            return `f2f-${assessment.f2FAssessmentId}`;
        }
        return assessment.phoneAssessmentId > 0 ? `pa-${assessment.phoneAssessmentId}` : '';
    }

    function gotoAssessment(assessment) {
        if (keyOf(assessment)) {
            router.push(`/assessments/${keyOf(assessment)}`);
        }
    }

    // The provider picked in the search carries over to the new assessment, as in WebForms.
    function add() {
        router.push({ path: '/assessments/0', query: criteria.providerId ? { providerId: criteria.providerId } : {} });
    }

    return {
        // Results
        assessments, totalRecords, paging, criteria, providers,

        // Busy and validation state
        isSearching, errors, msg,

        // User and permissions
        canAdd,

        // Actions
        search, showAll, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged,
        keyOf, gotoAssessment, add
    };
}
