import { ref } from 'vue';
import { servicesApi } from '../api/servicesApi.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { apiErrorMessage } from '../../../utils/apiError.js';
import { announce } from '../../../services/liveAnnouncer.js';

// Port of ManageServiceFiles.aspx: upload one service file (.txt). The server checks it (header, records, footer count, the
// provider NPI in it) and stores it for the nightly import; this page keeps a history of what was uploaded in this visit.
export function useServiceFileUpload() {
    const { logSuccess } = useLogger();

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
        if (!file.name.toLowerCase().endsWith('.txt')) {
            pickError.value = 'Only .txt files are accepted.';
            return;
        }
        if (file.size > MAX_BYTES) {
            pickError.value = `The file exceeds the ${MAX_BYTES / (1024 * 1024)} MB limit.`;
            return;
        }

        isUploading.value = true;
        try {
            const result = await servicesApi.uploadFile(file);
            history.value.unshift({ name: file.name, status: 'Pending', messages: [`Uploaded for ${result.providerName}. It is imported overnight.`], id: result.id });
            logSuccess(`${file.name} uploaded.`);
        } catch (err) {
            const fieldErrors = err.response?.status === 400 ? err.response.data?.errors?.file : null;
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
