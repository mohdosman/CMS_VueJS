import { test, expect } from '@playwright/test';
import { label, callApi, expectNoErrorToast, expectSuccessToast, expectTitle, openPage, saveAndExpectOk, selectFirstOption, tableRow, uniqueId, ymd, mdy } from './helpers/e2eHelpers.js';

const region = (page, name) => page.getByRole('region', { name });

test.describe('Assessments workflow', () => {
    test('validates, creates a telephone assessment, searches, edits and deletes it', async ({ page }) => {
        const last = uniqueId('E2ELast');
        const first = 'E2EFirst';

        await openPage(page, '/assessments');
        await expectTitle(page, 'Search Assessments');
        await page.getByRole('button', { name: /^Add$/ }).click();
        await expectTitle(page, 'Enter Assessment');

        // Nothing filled in: the server names every missing field, in a summary and next to the fields.
        await page.getByRole('button', { name: /^Save$/ }).click();
        const summary = page.getByRole('alert').filter({ hasText: 'Please correct the following' });
        await expect(summary).toBeVisible();
        await expect(summary).toContainText('First Name is required.');
        await expect(summary).toContainText('Call End Date is Required!');
        await expect(summary).toContainText('Disposition is required');

        const consumer = region(page, /^CONSUMER/);
        await selectFirstOption(consumer.getByLabel(label('Provider')));
        await consumer.getByLabel('First Name').fill(first);
        await consumer.getByLabel('Last Name').fill(last);
        await consumer.getByLabel('Date of Birth').fill('5/5/1980');

        const phone = region(page, /^CRISIS TELEPHONE/);
        await phone.getByLabel('Call End date').fill(mdy(2));
        await phone.getByLabel('Call End time').fill('10:30');
        // Any disposition except the two that need extra fields (mobile crisis dispatched, other).
        const disposition = phone.getByLabel(label('Disposition'));
        await expect.poll(() => disposition.locator('option').count()).toBeGreaterThan(1);
        const plain = await disposition.locator('option').evaluateAll((o) => o.find((x) => x.value && !['4', '6'].includes(x.value)).value);
        await disposition.selectOption(plain);
        await phone.getByLabel('Notes').fill('created by the e2e suite');

        await saveAndExpectOk(page, '/api/assessments', 'Create assessment');
        await expectSuccessToast(page, /assessment saved/i);
        await expect(page).toHaveURL(/#\/assessments\/pa-\d+$/);
        const key = page.url().match(/(pa-\d+)$/)[1];

        try {
            // The same call end, last name and provider is a duplicate.
            await openPage(page, `/assessments/${key}`);
            await expect(region(page, /^CONSUMER/).getByLabel('Last Name')).toHaveValue(last);
            const dup = await callApi(page, 'POST', 'assessments', {
                providerId: (await callApi(page, 'GET', `assessments/detail/${key}`)).json.providerId, firstName: first, lastName: last, dob: '1980-05-05',
                callEnded: `${ymd(2)}T10:30`, dispositionId: Number(plain)
            });
            expect(dup.status).toBe(400);
            expect(dup.text).toContain('Duplicate phone assessments');

            // Find it from the search screen.
            await openPage(page, '/assessments');
            await page.getByRole('button', { name: /show all assessments/i }).click();
            await page.getByLabel(label('Last Name')).fill(last);
            await page.getByRole('button', { name: /^Search$/ }).click();
            await expect(tableRow(page, last)).toHaveCount(1);
            await page.getByRole('link', { name: `${first} ${last}` }).click();
            await expectTitle(page, 'Edit Assessment');

            await region(page, /^CRISIS TELEPHONE/).getByLabel('Notes').fill('edited by the e2e suite');
            await saveAndExpectOk(page, `/api/assessments/${key}`, 'Update assessment');
            await expectNoErrorToast(page);
            await expect(region(page, /^CRISIS TELEPHONE/).getByLabel('Notes')).toHaveValue('edited by the e2e suite');

            await page.getByRole('button', { name: /^Delete$/ }).click();
            await page.getByRole('dialog').getByRole('button', { name: /^Delete assessment$/ }).click();
            await expectSuccessToast(page, /assessment deleted/i);
            await expect(page).toHaveURL(/#\/assessments$/);
        } finally {
            await callApi(page, 'DELETE', `assessments/${key}`);
        }
        expect((await callApi(page, 'GET', `assessments/detail/${key}`)).status).toBe(404);
    });

    test('the face to face sections open and close, and a phone-only save does not ask for face to face fields', async ({ page }) => {
        await openPage(page, '/assessments/0');
        const f2f = page.getByRole('button', { name: /^CRISIS FACE TO FACE/ });
        await expect(f2f).toHaveAttribute('aria-expanded', 'true');
        await f2f.click();
        await expect(f2f).toHaveAttribute('aria-expanded', 'false');
        await page.getByRole('button', { name: /^Save$/ }).click();
        await expect(page.getByRole('alert').filter({ hasText: 'Please correct the following' })).not.toContainText('Please select the Assessment Type!');
    });

    test('the incomplete-only banner can be turned off', async ({ page }) => {
        await openPage(page, '/assessments');
        const banner = page.getByRole('status').filter({ hasText: 'incomplete assessments only' });
        await expect(banner).toBeVisible();
        await banner.getByRole('button', { name: /show all/i }).click();
        await expect(banner).toHaveCount(0);
    });
});
