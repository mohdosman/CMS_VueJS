import { ref } from 'vue';
import { usersApi } from '../api/usersApi.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { apiErrorMessage } from '../../../utils/apiError.js';
import { announce } from '../../../services/liveAnnouncer.js';

// Uploads PDFs on selection, one at a time. The server checks type and size again and is the one that counts.
export function useUserAgreementUpload(props) {
    const { logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const MAX_BYTES = 10 * 1024 * 1024;
    const MAX_FILES = 10;

    const results = ref([]);   // [{ name, error }]
    const isBusy = ref(false);
    const stored = ref(0);

    // ================================================================
    // Upload
    // ================================================================
    async function uploadOne(file) {
        if (file.size > MAX_BYTES) {
            return { name: file.name, error: `Exceeds the ${MAX_BYTES / (1024 * 1024)} MB limit.` };
        }
        try {
            await usersApi.uploadDocument(props.userKey, file);
            stored.value++;
            return { name: file.name, error: null };
        } catch (e) {
            const fieldErrors = e.response?.status === 400 ? e.response.data?.errors?.file : null;
            if (!fieldErrors) {
                logApiError(e);
            }
            return { name: file.name, error: fieldErrors?.join(' ') ?? apiErrorMessage(e) };
        }
    }

    async function onPick(e) {
        const files = [...e.target.files].slice(0, MAX_FILES);
        e.target.value = '';
        isBusy.value = true;
        try {
            for (const file of files) {
                results.value.push(await uploadOne(file));
            }
        } finally {
            isBusy.value = false;
        }
        const uploaded = results.value.filter((r) => !r.error).length;
        announce(`${uploaded} of ${results.value.length} files uploaded`);
    }

    return {
        // Results
        results, stored, MAX_FILES,

        // Busy state
        isBusy,

        // Actions
        onPick
    };
}
