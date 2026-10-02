import { ref } from 'vue';
import http from '../../../common/api/http.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { useActivate } from '../../../common/composables/useActivate.js';
import { getBaseUrl } from '../../../utils/urlUtils.js';

// My Profile: read-only, as in the Blazor CMS (names are administered on the Users screen). Authenticator setup is the
// server-rendered Account/SetupMfa page.
export function useProfile() {
    const { logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const profile = ref(null);
    const setupUrl = `${getBaseUrl()}/Account/SetupMfa`;

    // ================================================================
    // Page load
    // ================================================================
    useActivate(async () => {
        try {
            profile.value = (await http.get('profile')).data;
        } catch (e) {
            logApiError(e);
        }
    });

    return {
        // Profile
        profile, setupUrl
    };
}
