import { test, expect } from '@playwright/test';
import { expectTitle, callApi, expectNoErrorToast, expectSuccessToast, openPage, saveAndExpectOk, tableRow, uniqueId } from './helpers/e2eHelpers.js';

test.describe('Providers workflow', () => {
    test('validates, creates, searches, updates and deletes a provider', async ({ page }) => {
        const name = uniqueId('E2E Provider ');
        const abbreviation = uniqueId('E2E').slice(0, 20);
        const edison = String(Math.floor(1e9 + Math.random() * 9e9));

        await openPage(page, '/admin/providers');
        await expectTitle(page, 'Search Providers');
        await page.getByRole('button', { name: /^Add$/ }).click();
        await expectTitle(page, 'Add Provider');

        // Empty and short values are refused by the server, and each message shows next to its field.
        await page.getByLabel(/^Provider name/).fill('Short');
        await page.getByRole('button', { name: /^Save$/ }).click();
        await expect(page.getByText('Provider name must be more than 10 characters.')).toBeVisible();
        await expect(page.getByText('Provider abbreviation is required.')).toBeVisible();
        await expect(page.getByText('Edison number is required.')).toBeVisible();

        await page.getByLabel(/^Provider name/).fill(name);
        await page.getByLabel(/^Abbreviation/).fill(abbreviation);
        await page.getByLabel(/^Edison number/).fill('12345');
        await page.getByRole('button', { name: /^Save$/ }).click();
        await expect(page.getByText('Edison number must be exactly 10 digits.')).toBeVisible();

        await page.getByLabel(/^Edison number/).fill(edison);
        await saveAndExpectOk(page, '/api/providers', 'Create provider');
        await expectSuccessToast(page, /provider saved/i);
        await expect(page).toHaveURL(/#\/admin\/providers\/\d+$/);
        const id = page.url().match(/(\d+)$/)[1];

        try {
            await openPage(page, '/admin/providers');
            await page.getByLabel('Name', { exact: true }).fill(name);
            await page.getByRole('button', { name: /^Search$/ }).click();
            await expect(tableRow(page, name)).toHaveCount(1);
            await page.getByRole('link', { name }).click();
            await expectTitle(page, 'Provider Details');
            await expect(page.getByLabel(/^Abbreviation/)).toHaveValue(abbreviation);

            // The same Edison number cannot be used twice.
            const duplicate = await callApi(page, 'POST', 'providers', { name: `${name} Two`, abbreviation: `${abbreviation}2`, edisonNumber: edison });
            expect(duplicate.status).toBe(400);
            expect(duplicate.text).toMatch(/edison/i);

            await page.getByLabel(/^Abbreviation/).fill(`${abbreviation}U`);
            await saveAndExpectOk(page, `/api/providers/${id}`, 'Update provider');
            await expectSuccessToast(page, /provider saved/i);
            await expect(page.getByLabel(/^Abbreviation/)).toHaveValue(`${abbreviation}U`);
            await expectNoErrorToast(page);

            await page.getByRole('button', { name: /^Delete$/ }).click();
            await page.getByRole('dialog').getByRole('button', { name: /^Delete provider$/ }).click();
            await expectSuccessToast(page, /deleted/i);
            await expect(page).toHaveURL(/#\/admin\/providers$/);
        } finally {
            await callApi(page, 'DELETE', `providers/${id}`);   // no-op when the test already deleted it
        }

        await openPage(page, '/admin/providers');
        await page.getByLabel('Name', { exact: true }).fill(name);
        await page.getByRole('button', { name: /^Search$/ }).click();
        await expect(page.getByText('No Providers Found')).toBeVisible();
    });
});
