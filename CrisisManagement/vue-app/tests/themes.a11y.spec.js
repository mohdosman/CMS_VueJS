import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { openRoute } from './routes.js';

// Contrast is the rule that differs between colour themes, so the same pages are scanned in each one.
const THEMES = ['navy', 'blue', 'dark', 'green', 'brown', 'eastern_blue', 'tamarillo'];
const ROUTES = ['/', '/assessments', '/assessments/0', '/services/new', '/reports'];

for (const theme of THEMES) {
    test(`WCAG AA scan in the ${theme} theme`, async ({ page }) => {
        test.setTimeout(30_000 + ROUTES.length * 15_000);
        await page.goto('/', { waitUntil: 'domcontentloaded' });
        await page.evaluate((t) => localStorage.setItem('cms_theme', t), theme);

        const problems = [];
        for (const route of ROUTES) {
            const landed = await openRoute(page, route);
            if (route !== '/' && landed !== route) continue;
            const results = await new AxeBuilder({ page }).withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa']).analyze();
            problems.push(...results.violations.map((v) => `${theme} ${route}: ${v.id} ${v.help}\n    ${v.nodes.slice(0, 3).map((n) => n.target.join(' ')).join('\n    ')}`));
        }
        expect(problems, problems.join('\n')).toEqual([]);
    });
}
