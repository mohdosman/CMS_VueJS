// Thin fetch wrapper for the /api endpoints. Paths are relative to the page so the app
// works under a virtual directory. The auth cookie rides along automatically.
export class ApiError extends Error {
    // fieldErrors: { camelCaseField: [messages] } from a 400 validation response.
    constructor(status, message, fieldErrors = null) {
        super(message);
        this.status = status;
        this.fieldErrors = fieldErrors;
    }
}

export async function api(path, { method = 'GET', body } = {}) {
    const res = await fetch(`api/${path}`, {
        method,
        credentials: 'same-origin',
        headers: body === undefined ? undefined : { 'Content-Type': 'application/json' },
        body: body === undefined ? undefined : JSON.stringify(body)
    });

    // Session expired: back to the MVC login page.
    if (res.status === 401) {
        window.location.href = 'Account/Login';
        throw new ApiError(401, 'Your session has expired.');
    }
    if (!res.ok) {
        const problem = await res.json().catch(() => ({}));
        const fallback = res.status === 403 ? 'You do not have access to this record.' : res.statusText;
        throw new ApiError(res.status, problem.detail || fallback, problem.errors ?? null);
    }

    return res.status === 204 ? null : res.json();
}
