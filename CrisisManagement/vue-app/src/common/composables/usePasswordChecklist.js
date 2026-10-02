import { computed } from 'vue';

// "Password must include:" list that ticks off as the user types (Blazor PasswordRequirementsChecklist).
// policy is the server's structured rules; confirm is optional and adds a "Passwords match" line.
export function usePasswordChecklist(props) {
    const items = computed(() => {
        const policy = props.policy;
        if (!policy) {
            return [];
        }
        const value = props.password ?? '';
        const list = [{ label: `At least ${policy.minLength} characters long`, met: value.length >= policy.minLength }];
        if (policy.requireLowercase) {
            list.push({ label: 'At least one lower case letter', met: /[a-z]/.test(value) });
        }
        if (policy.requireUppercase) {
            list.push({ label: 'At least one upper case letter', met: /[A-Z]/.test(value) });
        }
        if (policy.requireDigit) {
            list.push({ label: 'At least one number', met: /\d/.test(value) });
        }
        if (policy.requireSpecial) {
            list.push({ label: 'At least one special character', met: /[^A-Za-z0-9]/.test(value) });
        }
        if (policy.uniqueChars > 1) {
            list.push({ label: `At least ${policy.uniqueChars} different characters`, met: new Set(value).size >= policy.uniqueChars });
        }
        if (props.confirm !== null) {
            list.push({ label: 'Passwords match', met: value.length > 0 && value === props.confirm });
        }
        return list;
    });

    return {
        items
    };
}
