import { test, expect } from '@playwright/test';
import { expectNoErrorToast, expectTitle, openPage } from './helpers/e2eHelpers.js';

test.describe('Reports workflow', () => {
    test('lists, filters, sorts and clears', async ({ page }) => {
        await openPage(page, '/reports');
        await expectTitle(page, 'Report Listing');
        const rows = page.locator('tbody tr');
        await expect(rows.first()).toBeVisible();
        expect(await rows.count()).toBeGreaterThan(1);

        await page.getByLabel('Report Name').fill('zzz-no-such-report');
        await page.getByRole('button', { name: /^Search$/ }).click();
        await expect(page.getByText('No reports found.')).toBeVisible();

        await page.getByRole('button', { name: /^Clear$/ }).click();
        await expect(page.getByText('No reports found.')).toHaveCount(0);

        // Sorting by a header changes its aria-sort state.
        const header = page.getByRole('columnheader', { name: /Report Name/ });
        await header.getByRole('button').click();
        await expect(header).toHaveAttribute('aria-sort', /ascending|descending/);
        await expectNoErrorToast(page);
    });

    test('running a report signs a token and opens the report window', async ({ page, context }) => {
        await openPage(page, '/reports');
        const run = page.locator('tbody tr').first().getByRole('button', { name: /^Run/ });
        await expect(run).toBeVisible();

        const answer = page.waitForResponse((r) => /\/api\/reports\/[0-9a-f-]+\/run$/.test(r.url()) && r.request().method() === 'POST');
        const popup = context.waitForEvent('page');
        await run.click();
        const response = await answer;
        expect(response.status(), await response.text()).toBe(200);
        expect((await response.json()).redirectUrl).toContain('/api/reports/open/');
        await (await popup).close();   // the window goes on to the external report server
    });

    test('the add screen offers the report files that have no report yet', async ({ page }) => {
        await openPage(page, '/reports/0');
        await expectTitle(page, 'Add Report');
        await expect(page.getByLabel(/^File Name/)).toBeDisabled();
        await page.getByRole('button', { name: /^Save$/ }).click();
        await expect(page.getByText(/report name/i).first()).toBeVisible();
    });
});
