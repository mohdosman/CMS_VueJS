import { expect } from '@playwright/test';

// Milliseconds plus two random digits: records made by different specs never collide, and every one is recognisable as test data.
export function uniqueId(prefix) {
    const stamp = new Date().toISOString().replace(/[-:.TZ]/g, '').slice(0, 17);
    return `${prefix}${stamp}${String(Math.floor(Math.random() * 100)).padStart(2, '0')}`;
}

// Full page load of a route. A hash-only goto leaves the old screen up, and a search screen restores its saved criteria on activation.
export async function openPage(page, route) {
    await page.goto('about:blank');
    await page.goto(`/#${route}`, { waitUntil: 'networkidle' });
    await expect(page).not.toHaveURL(/\/Account\/Login/i);
}

export const toast = (page, kind) => page.locator(`.Vue-Toastification__toast--${kind}`);
export const expectSuccessToast = (page, pattern) => expect(toast(page, 'success').filter({ hasText: pattern }).first()).toBeVisible();
export const expectNoErrorToast = (page) => expect(toast(page, 'error')).toHaveCount(0);

// Clicks Save and asserts on the API answer itself, so a backend failure reports its status and body instead of timing out on a missing toast.
export async function saveAndExpectOk(page, urlPart, workflow, buttonName = /^Save$/) {
    const answer = page.waitForResponse((r) => r.url().includes(urlPart) && ['POST', 'PUT'].includes(r.request().method()), { timeout: 15_000 }).catch(() => null);
    await page.getByRole('button', { name: buttonName }).click();
    const response = await answer;
    expect(response, `${workflow}: Save sent no ${urlPart} request (client validation or a disabled Save?)`).not.toBeNull();
    const body = await response.text().catch(() => '');
    expect(response.ok(), `${workflow}: ${response.request().method()} ${response.url()} returned ${response.status()} ${body.slice(0, 500)}`).toBe(true);
    try { return body ? JSON.parse(body) : null; } catch { return null; }
}

// Calls the app's own API from the signed-in page (antiforgery header included): for set-up, clean-up and server-side checks.
export async function callApi(page, method, path, body) {
    return page.evaluate(async ({ method, path, body }) => {
        const boot = JSON.parse(document.getElementById('__cms_boot__').textContent);
        const response = await fetch(`${boot.globals.webApiBaseUrl}/${path}`, {
            method,
            headers: { 'Content-Type': 'application/json', 'X-XSRF-TOKEN': boot.globals.antiforgeryToken },
            body: body === undefined ? undefined : JSON.stringify(body),
            credentials: 'include'
        });
        const text = await response.text();
        let json = null;
        try { json = text ? JSON.parse(text) : null; } catch { /* not JSON */ }
        return { status: response.status, text, json };
    }, { method, path, body });
}

// A table row, found by the text of any cell.
export const tableRow = (page, text) => page.locator('tbody tr').filter({ hasText: text });

// The option list fills in after the page renders, so poll for a real option instead of reading the <select> once.
export async function selectFirstOption(select) {
    await expect.poll(() => select.locator('option').evaluateAll((o) => o.filter((x) => x.value && x.value !== '').length), { message: `the select ${select} never got options` }).toBeGreaterThan(0);
    const value = await select.locator('option').evaluateAll((o) => o.find((x) => x.value && x.value !== '').value);
    await select.selectOption(value);
    return value;
}

// Every problem shown to the user after a save that the server refused.
export async function expectFieldErrors(page, messages) {
    for (const message of messages) await expect(page.getByText(message, { exact: false }).first()).toBeVisible();
}

export const ymd = (daysAgo = 0) => {
    const d = new Date(Date.now() - daysAgo * 86_400_000);
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
};

// Every screen has one visually hidden h1 (#main-title); the visible card heading repeats it, so roles by name would match twice.
export const expectTitle = (page, text) => expect(page.locator('#main-title')).toHaveText(text);

// getByLabel matches the label text including the aria-hidden "*" of required fields, so an exact label is "Name" or "Name *".
export const label = (text) => new RegExp('^' + [...text].map((c) => (/[a-z0-9 ]/i.test(c) ? c : '[' + c + ']')).join('') + '( [*])? *$');
