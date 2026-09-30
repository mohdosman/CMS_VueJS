import { ref, reactive, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { reportsApi } from '../api/reportsApi.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { apiErrorMessage } from '../../../utils/apiError.js';

// One form for a new report (/reports/0) and for an existing one (/reports/:key, the report id).
export function useReportDetail() {
    const route = useRoute();
    const router = useRouter();
    const { logSuccess, logApiError } = useLogger();

    const isNew = route.params.key === '0';
    const form = reactive({ id: null, rowVersion: null, reportName: '', fileName: '', description: '', exportOption: 'PDF' });
    const available = ref([]);       // report files not defined yet (new report)
    const errors = ref({});          // { field: [messages] } from a 400 response
    const formError = ref('');       // page-level message: a load failure
    const dialog = ref('');          // '' or 'delete'
    const isLoading = ref(true);
    const isSaving = ref(false);

    const title = isNew ? 'Add Report' : 'Edit Report';
    const msg = (f) => errors.value[f]?.join(' ') ?? '';
    const fileName = computed(() => (isNew ? (form.reportName ? `${form.reportName}.rpt` : '') : form.fileName));

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
            if (isNew) {
                await reportsApi.create({ ...form, fileName: fileName.value });
                logSuccess('Report created.');
                router.push('/reports');
            } else {
                Object.assign(form, await reportsApi.update(form.id, form));
                logSuccess('Report updated.');
            }
        } catch (e) {
            fail(e);
        } finally {
            isSaving.value = false;
        }
    }

    async function remove() {
        try {
            await reportsApi.remove(form.id);
            logSuccess('Report deleted.');
            router.push('/reports');
        } catch (e) {
            dialog.value = '';
            logApiError(e);
        }
    }

    const cancel = () => router.push('/reports');

    onMounted(async () => {
        try {
            if (isNew) available.value = await reportsApi.available();
            else Object.assign(form, await reportsApi.get(route.params.key));
        } catch (e) {
            formError.value = e.response?.status === 404 ? 'Report not found.' : apiErrorMessage(e);
        } finally {
            isLoading.value = false;
        }
    });

    return { isNew, title, form, fileName, available, errors, formError, dialog, isLoading, isSaving, msg, save, remove, cancel };
}
