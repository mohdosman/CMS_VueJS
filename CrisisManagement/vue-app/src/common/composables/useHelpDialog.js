import { ref, onMounted } from 'vue';
import { useLogger } from './useLogger.js';
import { publicFilesApi } from '../api/publicFilesApi.js';

// The Help dialog opened from the navbar: the public (help) files anyone signed in may download.
export function useHelpDialog() {
    const { logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const files = ref([]);
    const isLoading = ref(true);

    const size = (bytes) => {
        if (bytes >= 1048576) {
            return `${(bytes / 1048576).toFixed(2)} MB`;
        }
        return bytes >= 1024 ? `${Math.round(bytes / 1024)} KB` : `${bytes} Bytes`;
    };
    const date = (value) => new Date(value).toLocaleDateString();

    // ================================================================
    // Loading
    // ================================================================
    async function load() {
        try {
            files.value = await publicFilesApi.helpFiles();
        } catch (e) {
            logApiError(e);
        } finally {
            isLoading.value = false;
        }
    }

    onMounted(load);

    return {
        // Files
        files,

        // Busy and validation state
        isLoading,

        // Helpers for the template
        size, date, downloadUrl: publicFilesApi.downloadUrl
    };
}
