import { expect } from '@playwright/test';

export const splitList = (value) => (value ?? '').split(',').map((r) => r.trim()).filter(Boolean);

// Full load per route: a hash-only goto keeps the previous route on screen until Vue swaps it, so a scan or a Tab could hit the old page.
export async function openRoute(page, route) {
    await page.goto('about:blank');
    await page.goto(`/#${route}`, { waitUntil: 'networkidle' });
    await expect(page).not.toHaveURL(/\/Account\/Login/i);
    await expect(page.locator('#vue-app')).toBeAttached();
    // Unknown routes redirect home; the caller decides whether that is a skip.
    return page.evaluate(() => location.hash.replace(/^#/, '') || '/');
}

// The routes the signed-in user may open: the menu urls in the server-rendered boot data, plus the ones that are not menu items.
export async function getBaseRoutes(page) {
    await page.goto('/', { waitUntil: 'networkidle' });
    await expect(page).not.toHaveURL(/\/Account\/Login/i);
    const menu = await page.evaluate(() => {
        const el = document.getElementById('__cms_boot__');
        return (el ? JSON.parse(el.textContent).routes ?? [] : []).map((r) => r.url);
    });
    return [...new Set(['/', '/profile', ...menu])].sort();
}

// New-record pages: /x/0 for every list route (a route without a detail view redirects home and is skipped), and the extras from A11Y_EXTRA_ROUTES.
export async function getDetailRoutes(page) {
    const base = (await getBaseRoutes(page)).filter((r) => r !== '/' && r !== '/profile');
    return [...new Set([...base.map((r) => `${r}/0`), ...splitList(process.env.A11Y_EXTRA_ROUTES)])].sort();
}
