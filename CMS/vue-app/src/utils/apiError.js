/**
 * Turns an axios error into the message to show the user.
 *
 * The API answers a failure in one of these shapes:
 *   BadRequest("text")        -> data is the string itself
 *   BadRequest(ModelState)    -> data is { field: ["message"], ... }
 *   the same under [ApiController] -> ProblemDetails, messages nested in `errors`
 *   an unhandled exception    -> data is the /AppException HTML page
 * Anything else (no response at all, a timeout) falls back to axios' own message.
 */
// A model-binding failure appends the JSON coordinates of the offending value:
// "Could not convert string to integer: abc. Path 'agencyId', line 1, position 26."
// Useful in a log, noise in a toast.
const withoutJsonPath = (text) =>
    text.replace(/\. Path '[^']*', line \d+, position \d+\.?\s*$/, '.');

export function apiErrorMessage(error, fallback = 'An unexpected error occurred.') {
    const data = error?.response?.data;

    if (data && typeof data === 'object') {
        if (typeof data.message === 'string' && data.message.trim()) return data.message;

        // ProblemDetails keeps the field messages under `errors`. Never flatten the
        // envelope itself — that reads out type/title/status/traceId as the message.
        const isProblemDetails = typeof data.title === 'string' || typeof data.status === 'number';
        const fields = (data.errors && typeof data.errors === 'object') ? data.errors
            : (isProblemDetails ? null : data);
        if (fields) {
            const messages = Object.values(fields)
                .map(v => (Array.isArray(v) ? v[0] : v))
                .filter(v => typeof v === 'string' && v.trim())
                .map(withoutJsonPath);
            if (messages.length) return messages.join(' ');
        }

        // A ProblemDetails with no errors: `detail` is the message for this
        // occurrence (a 500 from BaseController.Failure), `title` names the status.
        if (typeof data.detail === 'string' && data.detail.trim()) return data.detail;
        if (typeof data.title === 'string' && data.title.trim()) return data.title;
    }

    // A string payload is the server's own message — unless it is an error page.
    if (typeof data === 'string') {
        const text = data.trim();
        if (text && text.length <= 300 && !text.startsWith('<')) return text;
    }

    return error?.message || fallback;
}
