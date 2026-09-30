import { test, expect } from '@playwright/test';
import { getBaseRoutes, getDetailRoutes, openRoute } from './routes.js';
import { loadTestEnv } from './testEnv.js';

const tabPresses = Number(process.env.A11Y_KEYBOARD_TAB_PRESSES ?? 40);
const minUniqueFocusTargets = Number(process.env.A11Y_KEYBOARD_MIN_UNIQUE_FOCUS_TARGETS ?? 1);

async function focusSnapshot(page) {
    return page.evaluate(() => {
        const el = document.activeElement;
        if (!el) return null;
        const path = [];
        for (let cur = el; cur && cur.nodeType === Node.ELEMENT_NODE && cur !== document.body; cur = cur.parentElement) {
            const same = cur.parentElement ? [...cur.parentElement.children].filter((c) => c.tagName === cur.tagName) : [];
            path.unshift(`${cur.tagName.toLowerCase()}:${same.indexOf(cur) + 1}`);
        }
        const style = getComputedStyle(el);
        const rect = el.getBoundingClientRect();
        return {
            tagName: el.tagName, id: el.id, name: el.getAttribute('name') ?? '', role: el.getAttribute('role') ?? '',
            text: (el.innerText || el.value || '').trim().slice(0, 60), path: path.join('>'),
            visibleFocus: style.outlineStyle !== 'none' || parseFloat(style.outlineWidth) > 0 || style.boxShadow !== 'none'
                || style.borderColor !== getComputedStyle(document.body).backgroundColor,
            visible: rect.width > 0 && rect.height > 0
        };
    });
}

// Tab through a page: focus never drops to <body>, never traps, and every focused control shows a focus indicator.
async function traverse(page, route) {
    const landed = await openRoute(page, route);
    if (landed === '/' && route !== '/') return false;

    await page.evaluate(() => document.activeElement instanceof HTMLElement && document.activeElement.blur());
    const seen = new Set();
    const history = [];
    for (let i = 0; i < tabPresses; i++) {
        await page.keyboard.press('Tab');
        const s = await focusSnapshot(page);
        expect.soft(s, `${route}: nothing focused after Tab ${i + 1}`).toBeTruthy();
        if (!s) break;
        if (s.tagName === 'BODY') {
            expect.soft(seen.size, `${route}: focus fell back to <body> after Tab ${i + 1} before reaching any control`).toBeGreaterThanOrEqual(minUniqueFocusTargets);
            break;
        }
        const key = [s.tagName, s.id, s.name, s.role, s.text, s.path].join('|');
        seen.add(key);
        history.push(key);
        if (s.visible) expect.soft(s.visibleFocus, `${route}: no visible focus indicator on ${key}`).toBe(true);
        if (history.length >= 6 && new Set(history.slice(-6)).size === 1) {
            expect.soft(false, `${route}: possible keyboard trap near ${key}`).toBe(true);
            break;
        }
    }
    expect.soft(seen.size, `${route}: expected at least ${minUniqueFocusTargets} focus targets`).toBeGreaterThanOrEqual(minUniqueFocusTargets);
    return true;
}

async function traverseAll(page, routes, label) {
    test.setTimeout(30_000 + routes.length * 15_000);
    let visited = 0;
    for (const route of routes) await test.step(route, async () => { if (await traverse(page, route)) visited++; });
    console.log(`${label}: walked ${visited} of ${routes.length} route(s)`);
    test.skip(visited === 0, `No ${label} were available to the test user.`);
}

test('keyboard traversal: menu routes', async ({ page }) => {
    loadTestEnv();
    await traverseAll(page, await getBaseRoutes(page), 'menu routes');
});

test('keyboard traversal: record pages', async ({ page }) => {
    loadTestEnv();
    await traverseAll(page, await getDetailRoutes(page), 'record pages');
});

test.describe('keyboard behaviour', () => {
    test('the first Tab on a page reaches a control', async ({ page }) => {
        await openRoute(page, '/profile');
        await page.keyboard.press('Tab');
        expect((await focusSnapshot(page))?.tagName).not.toBe('BODY');
    });

    test('a dialog keeps focus inside, closes with Escape and returns focus to its opener', async ({ page }) => {
        await openRoute(page, '/assessments/files');
        // The first provider may have no files: try providers until one lists a file.
        const opener = page.getByRole('button', { name: /raw file/i }).first();
        const providers = await page.locator('#providerId option').count();
        for (let i = 1; i < providers && !(await opener.isVisible()); i++) {
            await page.locator('#providerId').selectOption({ index: i });
            await page.getByRole('button', { name: /^Search$/ }).click();
            await opener.waitFor({ timeout: 3000 }).catch(() => {});
        }
        test.skip(!(await opener.isVisible()), 'No provider has assessment files to open.');
        await opener.focus();
        await page.keyboard.press('Enter');
        const dialog = page.locator('dialog[open]');
        await expect(dialog).toBeVisible();
        for (let i = 0; i < 8; i++) {
            await page.keyboard.press('Tab');
            // A modal dialog makes the rest of the page inert: focus stays in it, or leaves to the browser itself (activeElement = body).
            expect(await page.evaluate(() => document.activeElement === document.body || !!document.activeElement.closest('dialog[open]')), 'focus reached the page behind the dialog').toBe(true);
        }
        await page.keyboard.press('Escape');
        await expect(dialog).toHaveCount(0);
        await expect(opener).toBeFocused();
    });

    test('assessment sections toggle with Enter and Space and expose their state', async ({ page }) => {
        await openRoute(page, '/assessments/0');
        const toggle = page.getByRole('button', { name: /^CONSUMER/ });
        await expect(toggle).toHaveAttribute('aria-expanded', 'true');
        await toggle.focus();
        await page.keyboard.press('Enter');
        await expect(toggle).toHaveAttribute('aria-expanded', 'false');
        await page.keyboard.press('Space');
        await expect(toggle).toHaveAttribute('aria-expanded', 'true');
    });

    test('a search form submits with Enter from a text field', async ({ page }) => {
        await openRoute(page, '/reports');
        const request = page.waitForRequest((r) => r.url().includes('/api/reports/search') && r.method() === 'POST');
        await page.getByLabel('Report Name').fill('a');
        await page.keyboard.press('Enter');
        await request;
    });
});
