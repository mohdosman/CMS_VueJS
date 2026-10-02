import { ref, watch } from 'vue';

// A date typed as MM/DD/YYYY, the way SafetyNet's v-cc-valid-date fields work: on blur "T" is today, MMDDYY and MMDDYYYY
// are spread out, "-", "." and spaces become "/", a two-digit year gets the current century, a missing year the current year, and
// a date that does not exist is cleared. The model is the ISO date (yyyy-MM-dd) or "", so the API and the forms keep their shape.
export function useDateInput(props, emit) {
    // ================================================================
    // Text and ISO conversion
    // ================================================================
    const pad = (n) => String(n).padStart(2, '0');

    const toText = (iso) => {
        const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(iso ?? '');
        return match ? `${+match[2]}/${+match[3]}/${match[1]}` : '';
    };

    // The typed text as an ISO date, '' when empty, or null when it is not a real date.
    function parse(raw) {
        let value = raw.trim();
        if (!value) {
            return '';
        }
        const now = new Date();
        if (value.toUpperCase() === 'T') {
            value = `${now.getMonth() + 1}/${now.getDate()}/${now.getFullYear()}`;
        }
        if (/^(\d{6}|\d{8})$/.test(value)) {
            value = `${value.slice(0, 2)}/${value.slice(2, 4)}/${value.slice(4)}`;
        }
        let [month = '', day = '', year = ''] = value.replace(/[-.\s_]/g, '/').split('/');
        if (year.length === 2) {
            year = String(now.getFullYear()).slice(0, 2) + year;
        }
        if (year.length === 0) {
            year = String(now.getFullYear());
        }
        if (!/^\d{1,2}$/.test(month) || !/^\d{1,2}$/.test(day) || !/^\d{4}$/.test(year)) {
            return null;
        }
        const date = new Date(+year, +month - 1, +day);
        if (date.getFullYear() !== +year || date.getMonth() !== +month - 1 || date.getDate() !== +day) {
            return null;
        }
        return `${year}-${pad(month)}-${pad(day)}`;
    }

    // ================================================================
    // State
    // ================================================================
    const text = ref(toText(props.modelValue));
    const focused = ref(false);

    // A change from outside (a reset, a loaded record) shows immediately; typing is left alone.
    watch(() => props.modelValue, (value) => {
        if (!focused.value || toText(value) !== text.value) {
            text.value = toText(value);
        }
    });

    // ================================================================
    // Events
    // ================================================================
    function onBlur() {
        focused.value = false;
        const iso = parse(text.value);
        text.value = iso ? toText(iso) : '';
        emit('update:modelValue', iso ?? '');
    }

    // Typing a complete date updates the model at once, so a Save right after typing (Enter) sends it.
    function onInput(e) {
        text.value = e.target.value;
        const iso = parse(text.value);
        if (iso !== null && /^\d{1,2}\/\d{1,2}\/\d{4}$/.test(text.value.trim())) {
            emit('update:modelValue', iso);
        }
    }

    return {
        // State
        text, focused,

        // Actions
        onBlur, onInput
    };
}
