import { ref } from 'vue';
import { assessmentsApi } from '../api/assessmentsApi.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { apiErrorMessage } from '../../../utils/apiError.js';
import { announce } from '../../../services/liveAnnouncer.js';

// Port of ManageDataFiles.aspx: upload one crisis assessment XML file. The server checks it (well-formed, the schema, the
// provider NPI inside it) and stores it for the nightly import; this page keeps a history of what was uploaded in this visit.
export function useAssessmentUpload() {
    const { logSuccess } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const MAX_BYTES = 5 * 1024 * 1024;

    const history = ref([]);          // [{ name, status, message, id }]
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
        if (!file.name.toLowerCase().endsWith('.xml')) {
            pickError.value = 'Only .xml files are accepted.';
            return;
        }
        if (file.size > MAX_BYTES) {
            pickError.value = `The file exceeds the ${MAX_BYTES / (1024 * 1024)} MB limit.`;
            return;
        }

        isUploading.value = true;
        try {
            const result = await assessmentsApi.uploadFile(file);
            history.value.unshift({ name: file.name, status: 'Pending', message: `Uploaded for ${result.providerName}. It is imported overnight.`, id: result.id });
            logSuccess(`${file.name} uploaded.`);
        } catch (err) {
            const fieldErrors = err.response?.status === 400 ? err.response.data?.errors?.file : null;
            const message = fieldErrors?.join(' ') ?? apiErrorMessage(err);
            history.value.unshift({ name: file.name, status: 'Rejected', message, id: '' });
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
