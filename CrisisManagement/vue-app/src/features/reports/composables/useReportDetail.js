import { ref, reactive, computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { reportsApi } from '../api/reportsApi.js';
import { useCapabilities } from '../../../common/composables/useCapabilities.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { restoreSearchOnReturn } from '../../../common/composables/useSearchState.js';
import { createShowError, createTouch } from '../../../utils/validationUtils.js';

// One form for a new report (/reports/0) and for an existing one (/reports/:key, the report id).
export function useReportDetail() {
    const route = useRoute();
    const router = useRouter();
    const { logSuccess, logError, logApiError } = useLogger();

    // ================================================================
    // User permissions
    // ================================================================
    const { can } = useCapabilities();
    const canEdit = can('reports.edit');

    // ================================================================
    // State
    // ================================================================
    const isNew = route.params.key === '0';
    const title = isNew ? 'Add Report' : 'Edit Report';

    const form = reactive({ id: null, rowVersion: null, reportName: '', fileName: '', description: '', exportOption: 'PDF' });
    const available = ref([]);       // report files not defined yet (new report)
    const serverErrors = ref({});    // { field: [messages] } from a 400 response
    const dialog = ref('');          // '' or 'delete'
    const isLoading = ref(true);
    const isSaving = ref(false);

    // Fixed by the report server: PDF, CSV, Excel and text.
    const exportOptions = [
        { id: 'PDF', label: 'PDF' }, { id: 'CSV', label: 'CSV' }, { id: 'MSExcel', label: 'Excel' }, { id: 'TXT', label: 'TXT' }
    ];
    const nameOptions = computed(() => available.value.map((name) => ({ id: name, label: name })));
    const fileName = computed(() => (isNew ? (form.reportName ? `${form.reportName}.rpt` : '') : form.fileName));

    // ================================================================
    // Validation
    // ================================================================
    const submitted = ref(false);
    // Which fields the user has visited. A field only shows its message once it was left, or after a submit.
    const touched = reactive({});
    const touch = createTouch(touched);

    // The error message for each field; a field that is fine has no entry. The server also checks the name against the report files.
    const clientErrors = computed(() => {
        const e = {};
        if (isNew && !form.reportName.trim()) {
            e.reportName = 'Report name is required.';
        }
        if (!form.exportOption) {
            e.exportOption = 'Export option is required';
        }
        if (!form.description.trim()) {
            e.description = 'Description is required.';
        }
        return e;
    });

    const isValid = computed(() => Object.keys(clientErrors.value).length === 0);
    // A field shows its message only after it was touched, or after a submit was attempted.
    const showError = createShowError(touched, submitted, clientErrors);

    // What the screen shows for a field: the server's message when it sent one, else the form's own once it is due.
    const msg = (field) => serverErrors.value[field]?.join(' ') || (showError(field) ? clientErrors.value[field] : '');

    // ================================================================
    // Loading
    // ================================================================
    async function getPageData() {
        try {
            if (isNew) {
                available.value = await reportsApi.available();
            } else {
                Object.assign(form, await reportsApi.get(route.params.key));
            }
        } catch (e) {
            logApiError(e, { fallback: 'Report not found.' });
        }
    }

    // ================================================================
    // Navigation
    // ================================================================
    function backToList() {
        restoreSearchOnReturn();
        router.push('/reports');
    }

    function cancel() {
        backToList();
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

    // Checks permission, then saves.
    async function save() {
        if (isSaving.value) {
            return;
        }
        if (!canEdit) {
            logError('You do not have permission to manage reports.');
            return;
        }
        isSaving.value = true;
        try {
            if (isNew) {
                await reportsApi.create({ ...form, fileName: fileName.value });
                logSuccess('Report created.');
                backToList();
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
            await reportsApi.remove(form.id);
            logSuccess('Report deleted.');
            backToList();
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
        form, fileName, available, nameOptions, exportOptions, dialog, title, isNew,

        // Busy and validation state
        isLoading, isSaving, submitted, touched, touch, isValid, showError, msg,

        // User and permissions
        canEdit,

        // Actions
        save, onSubmit, remove, cancel
    };
}
