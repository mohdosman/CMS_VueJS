import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { getBaseRoutes, getDetailRoutes, openRoute } from './routes.js';
import { loadTestEnv } from './testEnv.js';

const TAGS = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'];
// Documented temporary exceptions only.
const excludedRules = [];

async function scan(page, route) {
    const landed = await openRoute(page, route);
    if (route !== '/' && landed !== route) return null;   // not registered or not permitted for this user
    return new AxeBuilder({ page }).withTags(TAGS).disableRules(excludedRules).analyze();
}

// Each violation is reported with the route, the rule and the offending elements, so one run lists everything.
function describe(route, results) {
    return results.violations.map((v) => `${route}: ${v.id} (${v.impact}) ${v.help}\n    ${v.nodes.slice(0, 4).map((n) => n.target.join(' ')).join('\n    ')}`);
}

async function scanAll(page, routes, label) {
    test.setTimeout(30_000 + routes.length * 15_000);
    const problems = [];
    const scanned = [];
    for (const route of routes) {
        await test.step(route, async () => {
            const results = await scan(page, route);
            if (!results) return;
            scanned.push(route);
            problems.push(...describe(route, results));
        });
    }
    console.log(`${label}: scanned ${scanned.length} of ${routes.length} route(s)`);
    test.skip(scanned.length === 0, `No ${label} were available to the test user.`);
    expect(problems, problems.join('\n')).toEqual([]);
}

test('WCAG AA automated scan: menu routes', async ({ page }) => {
    loadTestEnv();
    await scanAll(page, await getBaseRoutes(page), 'menu routes');
});

test('WCAG AA automated scan: record pages', async ({ page }) => {
    loadTestEnv();
    await scanAll(page, await getDetailRoutes(page), 'record pages');
});

test('WCAG AA automated scan: sign-in page', async ({ browser }) => {
    const context = await browser.newContext({ ignoreHTTPSErrors: true });
    const page = await context.newPage();
    await page.goto('/Account/Login', { waitUntil: 'networkidle' });
    const results = await new AxeBuilder({ page }).withTags(TAGS).analyze();
    expect(describe('/Account/Login', results), 'sign-in page').toEqual([]);
    await context.close();
});
