import { ref } from 'vue';
import { suicidesApi } from '../api/suicidesApi.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { apiErrorMessage } from '../../../utils/apiError.js';
import { announce } from '../../../services/liveAnnouncer.js';

// Port of ManageSuicideFiles.aspx: upload one death record spreadsheet (.xlsx). The server reads the first worksheet
// (row 1 = column names) and stores the file with its records; this page keeps a history of what was uploaded in this visit.
export function useSuicideFileUpload() {
    const { logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const MAX_BYTES = 5 * 1024 * 1024;

    const history = ref([]);          // [{ name, status, messages, id }]
    const isUploading = ref(false);
    const pickError = ref('');

    // ================================================================
    // Upload
    // ================================================================
    async function onPick(e) {
        const file = e.target.files[0];
        e.target.value = '';
        pickError.value = '';
        if (!file) {
            return;
        }
        if (!file.name.toLowerCase().endsWith('.xlsx')) {
            pickError.value = 'Upload failed: Only .xlsx Excel files are accepted';
            return;
        }
        if (file.size > MAX_BYTES) {
            pickError.value = `The file exceeds the ${MAX_BYTES / (1024 * 1024)} MB limit.`;
            return;
        }

        isUploading.value = true;
        try {
            const result = await suicidesApi.uploadFile(file);
            history.value.unshift({ name: file.name, status: 'Uploaded', messages: ['Data Imported successfully.'], id: result.id });
        } catch (err) {
            const fieldErrors = err.response?.status === 400 ? err.response.data?.errors?.file : null;
            if (!fieldErrors) {
                logApiError(err);
            }
            history.value.unshift({ name: file.name, status: 'Rejected', messages: fieldErrors ?? [apiErrorMessage(err)], id: '' });
            announce(`${file.name} was rejected`);
        } finally {
            isUploading.value = false;
        }
    }

    return {
        // History
        history,

        // Busy and validation state
        isUploading, pickError,

        // Actions
        onPick
    };
}
