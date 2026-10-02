import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { reportsApi } from '../api/reportsApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useSearchState } from '../../../common/composables/useSearchState.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { apiErrorMessage } from '../../../utils/apiError.js';
import { fieldMessages } from '../../../utils/formErrors.js';
import { announce } from '../../../services/liveAnnouncer.js';

// Report listing: the list of reports, filtered by name and description, with a Run button on each.
export function useReportSearch() {
    const router = useRouter();
    const { logSuccess, logError, logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const reports = ref([]);
    const totalRecords = ref(0);
    const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 10 });

    const DEFAULT_CRITERIA = { reportName: '', description: '', orderBy: 'reportName', reverse: false };
    const criteria = reactive({ ...DEFAULT_CRITERIA });

    const { load, save, clear: clearState } = useSearchState('reportSearchJSON', criteria, DEFAULT_CRITERIA, paging);

    const isSearching = ref(false);
    const errors = ref({});
    const runningKey = ref('');   // the report being run (its Run button shows a spinner)

    const REPORT_WINDOW = 'resizable=yes,height=600,width=1000,location=0,toolbar=0,menubar=0,scrollbars=1';

    // Each async load bumps its counter, so a slow earlier response can't overwrite a newer one.
    let requestSequence = 0;

    const msg = fieldMessages(errors);
    const exportLabel = (option) => (option === 'MSExcel' ? 'Excel' : option);

    // Empty text boxes are sent as absent, not as "".
    const cleanCriteria = () => Object.fromEntries(Object.entries(criteria).map(([k, v]) => [k, v === '' ? null : v]));

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canAdd = can('reports.edit');

    // ================================================================
    // Sorting, paging and search
    // ================================================================
    const setOrder = createSetOrder(criteria, paging, getReports);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getReports);

    async function getReports() {
        const requestId = ++requestSequence;
        errors.value = {};
        isSearching.value = true;
        try {
            const result = await reportsApi.search(paging.currentPage, paging.pageSize, cleanCriteria());
            if (requestId !== requestSequence) {
                return;
            }
            reports.value = result.items;
            totalRecords.value = result.totalCount;
            announce(`${result.totalCount} reports found`);
        } catch (e) {
            if (requestId === requestSequence) {
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
        await getReports();
    }

    // Runs each time the screen is shown: restore the saved criteria and list again.
    useActivate(async () => {
        load();
        await getReports();
    });

    // ================================================================
    // Run, navigation and clear
    // ================================================================
    // The window opens straight from the click (a window opened after the request would be blocked as a pop-up), then goes to
    // the link the server made for this run.
    async function run(report) {
        runningKey.value = report.reportKey;
        const win = window.open('', 'ReportWindow', REPORT_WINDOW);
        if (!win) {
            runningKey.value = '';
            logError('Your browser blocked the report window. Allow pop-ups for this site and try again.');
            return;
        }
        try {
            const { redirectUrl } = await reportsApi.run(report.reportKey);
            win.location.href = redirectUrl;
            logSuccess(`Report '${report.reportName}' opened successfully`);
        } catch (e) {
            win.close();
            if ([401, 403].includes(e.response?.status)) {
                logError(apiErrorMessage(e, 'You do not have permission to run this report'));
            } else {
                logError(`Failed to run report '${report.reportName}'`);
            }
        } finally {
            runningKey.value = '';
        }
    }

    function add() {
        router.push('/reports/0');
    }

    async function clear() {
        clearState();
        await getReports();
    }

    return {
        // Results
        reports, totalRecords, paging, criteria,

        // Busy and validation state
        isSearching, errors, msg, runningKey,

        // User and permissions
        canAdd,

        // Actions
        search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged,
        run, add,

        // Helpers for the template
        exportLabel
    };
}
