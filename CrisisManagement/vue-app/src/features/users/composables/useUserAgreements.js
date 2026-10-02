import { ref, onMounted } from 'vue';
import { usersApi } from '../api/usersApi.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { formatDate, formatFileSize } from '../../../utils/formatters.js';

// Lists a user's agreements with download and (for users.edit) delete.
export function useUserAgreements(props) {
    const { logSuccess, logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const docs = ref([]);
    const isLoading = ref(true);
    const confirming = ref(0);   // id of the document awaiting delete confirmation

    // ================================================================
    // Loading
    // ================================================================
    async function load() {
        try {
            docs.value = await usersApi.documents(props.userKey);
        } catch (e) {
            logApiError(e);
        } finally {
            isLoading.value = false;
        }
    }

    onMounted(load);

    // ================================================================
    // Delete
    // ================================================================
    async function remove(document) {
        try {
            await usersApi.removeDocument(props.userKey, document.documentId);
            logSuccess('User agreement deleted.');
            confirming.value = 0;
            await load();
        } catch (e) {
            confirming.value = 0;
            logApiError(e);
        }
    }

    return {
        // Agreements
        docs,

        // Busy and validation state
        isLoading, confirming,

        // Actions
        remove,

        // Helpers for the template
        formatDate, formatFileSize, documentUrl: usersApi.documentUrl
    };
}
