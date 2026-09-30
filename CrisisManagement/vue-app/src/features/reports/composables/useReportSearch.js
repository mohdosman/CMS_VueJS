import { ref, reactive, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { reportsApi } from '../api/reportsApi.js';
import { createSetOrder, getSortIcon, createPagingHandlers } from '../../../utils/searchUtils.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { apiErrorMessage } from '../../../utils/apiError.js';
import { fieldMessages } from '../../../utils/formErrors.js';
import { announce } from '../../../services/liveAnnouncer.js';

const DEFAULT_CRITERIA = () => ({ reportName: '', description: '', orderBy: 'reportName', reverse: false });
const REPORT_WINDOW = 'resizable=yes,height=600,width=1000,location=0,toolbar=0,menubar=0,scrollbars=1';

// Module scope on purpose: filters and results survive list -> detail -> back within the SPA.
const criteria = reactive(DEFAULT_CRITERIA());
const paging = reactive({ currentPage: 1, maxPagesToShow: 10, pageSize: 10 });
const reports = ref([]);
const totalRecords = ref(0);
const isSearching = ref(false);
const errors = ref({});

// Empty text boxes are sent as absent, not as "".
const clean = () => Object.fromEntries(Object.entries(criteria).map(([k, v]) => [k, v === '' ? null : v]));

export function useReportSearch() {
    const router = useRouter();
    const { can } = useCapabilities();
    const { logSuccess, logError, logApiError } = useLogger();
    const runningKey = ref('');   // the report being run (its Run button shows a spinner)

    async function getReports() {
        errors.value = {};
        isSearching.value = true;
        try {
            const result = await reportsApi.search(paging.currentPage, paging.pageSize, clean());
            reports.value = result.items;
            totalRecords.value = result.totalCount;
            announce(`${result.totalCount} reports found`);
        } catch (e) {
            const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
            if (fieldErrors) errors.value = fieldErrors;
            else logApiError(e);
        } finally {
            isSearching.value = false;
        }
    }

    const setOrder = createSetOrder(criteria, paging, getReports);
    const sortIcon = (col) => getSortIcon(col, criteria);
    const { onPageChanged, onPageSizeChanged } = createPagingHandlers(paging, getReports);

    function search() {
        paging.currentPage = 1;
        return getReports();
    }

    function clear() {
        Object.assign(criteria, DEFAULT_CRITERIA());
        return search();
    }

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
            if ([401, 403].includes(e.response?.status)) logError(apiErrorMessage(e, 'You do not have permission to run this report'));
            else logError(`Failed to run report '${report.reportName}'`);
        } finally {
            runningKey.value = '';
        }
    }

    const exportLabel = (option) => (option === 'MSExcel' ? 'Excel' : option);
    const canAdd = can('reports.edit');
    const add = () => router.push('/reports/0');

    onMounted(getReports);

    const msg = fieldMessages(errors);

    return {
        exportLabel, msg, criteria, paging, reports, totalRecords, isSearching, errors, runningKey,
        search, clear, setOrder, sortIcon, onPageChanged, onPageSizeChanged, run, canAdd, add
    };
}
