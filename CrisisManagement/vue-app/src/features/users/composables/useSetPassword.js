import { ref, reactive, computed } from 'vue';
import { usersApi } from '../api/usersApi.js';
import { useLogger } from '../../../common/composables/useLogger.js';
import { usePasswordChecklist } from '../../../common/composables/usePasswordChecklist.js';

// The Set Password dialog: an administrator sets a user's password, which the user must change at next sign-in.
export function useSetPassword(props, emit) {
    const { logApiError } = useLogger();

    // ================================================================
    // State
    // ================================================================
    const password = ref('');
    const confirmPassword = ref('');
    const errors = ref({});
    const isSaving = ref(false);

    const showPasswords = ref(false);

    // The password rules, ticked off as typed; the same list the dialog shows. "Passwords match" is not a strength rule.
    const { items } = usePasswordChecklist(reactive({ password, confirm: confirmPassword, get policy() { return props.policy; } }));
    const rules = computed(() => items.value.filter((i) => i.label !== 'Passwords match'));
    const strength = computed(() => {
        const met = rules.value.filter((i) => i.met).length;
        const total = Math.max(rules.value.length, 1);
        const level = !password.value ? 0 : met === total ? 3 : met >= total / 2 ? 2 : 1;
        return { met, total, level, label: ['', 'Weak', 'Fair', 'Strong'][level] };
    });

    // ================================================================
    // Save
    // ================================================================
    // The server repeats these checks; this just saves the round trip.
    function validate() {
        const e = {};
        if (!password.value) {
            e.password = ['Password is required.'];
        } else if (rules.value.some((i) => !i.met)) {
            e.password = ['Password does not meet the requirements listed.'];
        }
        if (!confirmPassword.value) {
            e.confirmPassword = ['Confirm password is required.'];
        } else if (confirmPassword.value !== password.value) {
            e.confirmPassword = ['Passwords must match.'];
        }
        errors.value = e;
        return Object.keys(e).length === 0;
    }

    async function save() {
        if (!validate()) {
            return;
        }
        errors.value = {};
        isSaving.value = true;
        try {
            await usersApi.setPassword(props.userKey, { password: password.value, confirmPassword: confirmPassword.value });
            emit('saved');
        } catch (e) {
            // Field problems (400) sit next to their inputs; anything else is a toast.
            const fieldErrors = e.response?.status === 400 ? e.response.data?.errors : null;
            errors.value = fieldErrors ?? {};
            if (!fieldErrors) {
                logApiError(e);
            }
        } finally {
            isSaving.value = false;
        }
    }

    return {
        // Form
        password, confirmPassword, showPasswords, strength,

        // Busy and validation state
        isSaving, errors,

        // Actions
        save
    };
}
