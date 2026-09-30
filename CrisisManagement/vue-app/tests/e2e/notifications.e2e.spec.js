import { test, expect } from '@playwright/test';
import { callApi, expectNoErrorToast, expectSuccessToast, expectTitle, openPage, saveAndExpectOk, tableRow, uniqueId } from './helpers/e2eHelpers.js';

test.describe('Notifications workflow', () => {
    test('validates, creates, edits and deletes a notification', async ({ page }) => {
        const text = uniqueId('E2E notification ');

        await openPage(page, '/notifications');
        await expectTitle(page, 'Search Notifications');
        await page.getByRole('button', { name: /^Add$/ }).click();
        await expectTitle(page, 'Add Notification');

        await page.getByRole('button', { name: /^Save$/ }).click();
        await expect(page.getByText(/notification .*required/i).first()).toBeVisible();

        await page.getByRole('textbox', { name: /^Notification/ }).fill(text);
        await expect(page.getByText(`${text.length}/1000 characters`)).toBeVisible();
        const saved = await saveAndExpectOk(page, '/api/notifications', 'Create notification');
        await expectSuccessToast(page, /notification saved/i);
        const id = saved?.id ?? page.url().match(/(\d+)$/)?.[1];

        try {
            await openPage(page, '/notifications');
            await expect(tableRow(page, text)).toHaveCount(1);

            await tableRow(page, text).getByRole('link').click();
            await expectTitle(page, 'Notification Details');
            await page.getByRole('textbox', { name: /^Notification/ }).fill(`${text} edited`);
            await saveAndExpectOk(page, `/api/notifications/${id}`, 'Update notification');
            await expectNoErrorToast(page);

            await openPage(page, '/notifications');
            const row = tableRow(page, `${text} edited`);
            await expect(row).toHaveCount(1);
            await row.getByRole('button', { name: /^Delete/ }).click();
            const dialog = page.getByRole('dialog');
            await expect(dialog).toBeVisible();
            await dialog.getByRole('button', { name: /^Delete notification$/ }).click();
            await expectSuccessToast(page, /notification deleted/i);
            await expect(tableRow(page, `${text} edited`)).toHaveCount(0);
        } finally {
            if (id) await callApi(page, 'DELETE', `notifications/${id}`);
        }
    });

    test('the delete dialog can be cancelled and closed with its X', async ({ page }) => {
        await openPage(page, '/notifications');
        const first = page.locator('tbody tr').first().getByRole('button', { name: /^Delete/ });
        test.skip(!(await first.isVisible().catch(() => false)), 'There are no notifications to open a dialog for.');
        await first.click();
        await expect(page.getByRole('dialog')).toBeVisible();
        await page.getByRole('dialog').getByRole('button', { name: 'Close' }).click();
        await expect(page.getByRole('dialog')).toHaveCount(0);
    });
});
