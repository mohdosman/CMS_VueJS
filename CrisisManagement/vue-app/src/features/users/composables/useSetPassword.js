import { ref } from 'vue';
import { usersApi } from '../api/usersApi.js';
import { apiErrorMessage } from '../../../utils/apiError.js';

// The Set Password dialog: an administrator sets a user's password, which the user must change at next sign-in.
export function useSetPassword(props, emit) {
    // ================================================================
    // State
    // ================================================================
    const password = ref('');
    const confirmPassword = ref('');
    const errors = ref({});
    const formError = ref('');
    const isSaving = ref(false);

    // ================================================================
    // Save
    // ================================================================
    async function save() {
        errors.value = {};
        formError.value = '';
        isSaving.value = true;
        try {
            await usersApi.setPassword(props.userKey, { password: password.value, confirmPassword: confirmPassword.value });
            emit('saved');
        } catch (e) {
            // Field problems (400) sit next to their inputs; anything else is shown in the dialog.
            const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
            errors.value = fieldErrors ?? {};
            formError.value = fieldErrors ? '' : apiErrorMessage(e);
        } finally {
            isSaving.value = false;
        }
    }

    return {
        // Form
        password, confirmPassword,

        // Busy and validation state
        isSaving, errors, formError,

        // Actions
        save
    };
}
