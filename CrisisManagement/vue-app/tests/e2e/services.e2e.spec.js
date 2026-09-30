import { test, expect } from '@playwright/test';
import { label, callApi, expectSuccessToast, expectTitle, openPage, saveAndExpectOk, selectFirstOption, uniqueId, ymd, mdy } from './helpers/e2eHelpers.js';

const field = (page, text) => page.getByLabel(label(text));

test.describe('Services workflow', () => {
    test('validates, enters, finds, edits and deletes a service', async ({ page }) => {
        const last = uniqueId('E2ELast');
        const patientNo = uniqueId('E2EP').slice(0, 20);

        await openPage(page, '/services/new');
        await expectTitle(page, 'Enter Service');

        await page.getByRole('button', { name: /^Save$/ }).click();
        const summary = page.getByRole('alert').filter({ hasText: 'Please correct the following' });
        await expect(summary).toContainText('Provider is required');

        await selectFirstOption(field(page, 'Provider'));
        await field(page, 'Provider Patient ID').fill(patientNo);
        await field(page, 'Provider Patient ID').blur();
        await field(page, 'First Name').fill('E2EFirst');
        await field(page, 'Last Name').fill(last);
        await field(page, 'Date of Birth').fill('3/4/1975');
        await selectFirstOption(field(page, 'Gender'));
        await selectFirstOption(field(page, 'County of Residence'));
        await selectFirstOption(field(page, 'Payor Billed for Service'));
        await selectFirstOption(field(page, 'Service'));
        await field(page, 'DOS/Admit Date').fill(mdy(3));
        await field(page, 'Discharge Date').fill(mdy(2));
        await field(page, 'Duration Hours').fill('5');
        await selectFirstOption(field(page, 'County of Service'));

        const created = page.waitForResponse((r) => r.url().endsWith('/api/services') && r.request().method() === 'POST');
        await page.getByRole('button', { name: /^Save$/ }).click();
        const body = await (await created).json();
        await expectSuccessToast(page, /service created/i);
        const id = body.id;

        try {
            // The form resets and the new service is listed under it.
            await expect(field(page, 'Last Name')).toHaveValue('');
            await expect(page.getByRole('heading', { name: /entered in this session \(1\)/i })).toBeVisible();
            await expect(page.getByRole('link', { name: String(id), exact: true })).toBeVisible();

            // Entering the same service again is a duplicate.
            const dup = await callApi(page, 'POST', 'services', { ...body, id: null, rowVersion: null });
            expect(dup.status).toBe(400);
            expect(dup.text).toContain('Duplicate Service.');

            await openPage(page, '/services');
            await field(page, 'Last Name').fill(last);
            await page.getByRole('button', { name: /^Search$/ }).click();
            await expect(page.getByRole('link', { name: String(id), exact: true })).toBeVisible();
            await page.getByRole('link', { name: String(id), exact: true }).click();
            await expectTitle(page, 'Edit Service');

            // The patient part is fixed on an existing service.
            await expect(field(page, 'Last Name')).toBeDisabled();
            await expect(field(page, 'Last Name')).toHaveValue(last);
            await field(page, 'Duration Hours').fill('7');
            await saveAndExpectOk(page, `/api/services/${id}`, 'Update service');
            await expectSuccessToast(page, /service updated/i);
            await expect(page).toHaveURL(/#\/services$/);
            expect((await callApi(page, 'GET', `services/${id}`)).json.durationHours).toBe(7);

            await openPage(page, `/services/${id}`);
            await page.getByRole('button', { name: /^Delete$/ }).click();
            await page.getByRole('dialog').getByRole('button', { name: /^Delete service$/ }).click();
            await expectSuccessToast(page, /service deleted/i);
        } finally {
            await callApi(page, 'DELETE', `services/${id}`);
        }
        expect((await callApi(page, 'GET', `services/${id}`)).status).toBe(404);
    });

    test('a discharge date and duration follow the service code', async ({ page }) => {
        await openPage(page, '/services/new');
        // No service chosen: both are required, as for an unknown code.
        await expect(page.getByText('Discharge Date *')).toBeVisible();
        await expect(page.getByText('Duration Hours *')).toBeVisible();
    });

    test('date fields work like the SafetyNet ones: T is today, digits spread out, a date that does not exist is cleared', async ({ page }) => {
        await openPage(page, '/services');
        const from = page.getByLabel('DOS/Admit Date (From)');
        await expect(from).toHaveAttribute('placeholder', 'MM/DD/YYYY');

        await from.fill('T');
        await from.blur();
        await expect(from).toHaveValue(mdy(0));

        await from.fill('010224');
        await from.blur();
        await expect(from).toHaveValue('1/2/2024');

        await from.fill('01022024');
        await from.blur();
        await expect(from).toHaveValue('1/2/2024');

        await from.fill('13/45/2024');
        await from.blur();
        await expect(from).toHaveValue('');

        // A range that is the wrong way round is refused by the server and shown next to the field.
        await from.fill('2/1/2024');
        await page.getByLabel('DOS/Admit Date (To)').fill('1/1/2024');
        await page.getByRole('button', { name: /^Search$/ }).click();
        await expect(page.getByText(/should be less than equal to/)).toBeVisible();
    });
});
