// Shared touched/blur validation helpers (the SafetyNet pattern).

/**
 * Returns a showError function that reveals an error only after the field has
 * been touched (blurred) or the form has been submitted.
 */
export function createShowError(touched, submitted, errors) {
    return function showError(field) {
        return (touched[field] || submitted.value) && !!errors.value[field];
    };
}

// Pressing a form's Save/Cancel blurs the focused field before the click completes.
// Marking that field touched there shows its message, the form grows, and the
// button slides out from under the pointer, so the click is lost. Skip the touch
// while such a press is down; Save's submit shows every message anyway.
// The buttons carry data-form-action (see AppButton).
let formActionPressed = false;
if (typeof document !== 'undefined') {
    document.addEventListener('pointerdown', (e) => {
        formActionPressed = !!e.target.closest?.('[data-form-action]');
    }, true);
    document.addEventListener('pointerup', () => { formActionPressed = false; }, true);
    document.addEventListener('pointercancel', () => { formActionPressed = false; }, true);
}

/** Returns a touch function that marks a field as interacted with. */
export function createTouch(touched) {
    return function touch(field) {
        if (formActionPressed) return;
        touched[field] = true;
    };
}

/**
 * Validates that a select/dropdown has a meaningful selection.
 * Empty = null, undefined, or 0 (the "--SELECT--" sentinel).
 */
export function requireSelect(value) {
    return value !== null && value !== undefined && value !== 0 && value !== '';
}
