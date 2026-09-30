import { test, expect } from '@playwright/test';
import { expectNoErrorToast, expectTitle, openPage } from './helpers/e2eHelpers.js';

// Choose a file the way a user does; the screens upload as soon as one is chosen.
const choose = (page, input, name, content, mimeType = 'application/octet-stream') =>
    page.locator(input).setInputFiles({ name, mimeType, buffer: Buffer.from(content) });

const lastRow = (page) => page.locator('tbody tr').first();

test.describe('File uploads refuse bad files', () => {
    test('assessment upload: wrong extension and a file that is not the schema', async ({ page }) => {
        await openPage(page, '/assessments/upload');
        await expectTitle(page, 'Assessment File Upload');

        await choose(page, '#assessmentFile', 'notes.csv', 'a,b');
        await expect(page.getByText('Only .xml files are accepted.')).toBeVisible();

        await choose(page, '#assessmentFile', 'e2e-bad.xml', '<Provider><NPI>0000000000</NPI></Provider>', 'text/xml');
        await expect(lastRow(page)).toContainText('Rejected');
        await expect(lastRow(page)).toContainText('e2e-bad.xml');

        await choose(page, '#assessmentFile', 'e2e-malware.xml', 'MZ\u0090\u0000 This program cannot be run in DOS mode', 'text/xml');
        await expect(lastRow(page)).toContainText(/forbidden file type/i);
    });

    test('service upload: wrong extension, bad header and footer, an executable', async ({ page }) => {
        await openPage(page, '/services/servicefileupload');
        await expectTitle(page, 'Service File Upload');
        await expect(page.getByText(/processed only at night/i)).toBeVisible();

        await choose(page, '#serviceFile', 'services.csv', 'a,b');
        await expect(page.getByText('Only .txt files are accepted.')).toBeVisible();

        await choose(page, '#serviceFile', 'e2e-bad.txt', 'not,a,header\n1\n');
        await expect(lastRow(page)).toContainText('Rejected');
        await expect(lastRow(page)).toContainText('Invalid Header.');

        await choose(page, '#serviceFile', 'e2e-exe.txt', 'MZ\u0090\u0000 This program cannot be run in DOS mode');
        await expect(lastRow(page)).toContainText(/forbidden file type/i);
    });

    test('suicide upload: wrong extension and a file that is not a workbook', async ({ page }) => {
        await openPage(page, '/suicides/fileupload');
        await expectTitle(page, 'Suicide File Upload');

        await choose(page, '#suicideFile', 'deaths.xls', 'x');
        await expect(page.getByText('Only .xlsx files are accepted.')).toBeVisible();

        await choose(page, '#suicideFile', 'e2e-fake.xlsx', 'this is not a zip');
        await expect(lastRow(page)).toContainText('Rejected');
        await expect(lastRow(page)).toContainText(/xlsx/i);
    });
});

test.describe('File lists', () => {
    test('assessment files: search needs a provider, then the raw file opens and closes', async ({ page }) => {
        await openPage(page, '/assessments/files');
        await expectTitle(page, 'Search Assessment Files');
        await page.getByRole('button', { name: /^Search$/ }).click();
        await expect(page.getByText('Provider is required.')).toBeVisible();

        const opener = page.getByRole('button', { name: /raw file/i }).first();
        const providers = await page.locator('#providerId option').count();
        for (let i = 1; i < providers && !(await opener.isVisible()); i++) {
            await page.locator('#providerId').selectOption({ index: i });
            await page.getByRole('button', { name: /^Search$/ }).click();
            await opener.waitFor({ timeout: 3000 }).catch(() => {});
        }
        test.skip(!(await opener.isVisible()), 'No provider has assessment files.');
        await opener.click();
        const dialog = page.getByRole('dialog');
        await expect(dialog).toBeVisible();
        await expect(dialog.locator('pre')).not.toBeEmpty();
        await dialog.getByRole('button', { name: 'Close' }).first().click();   // the X in the header
        await expect(dialog).toHaveCount(0);
    });

    test('service files: raw file and import errors', async ({ page }) => {
        await openPage(page, '/services/files');
        await expectTitle(page, 'Display Service Files');
        const opener = page.getByRole('button', { name: /raw file/i }).first();
        const providers = await page.locator('#providerId option').count();
        for (let i = 1; i < providers && !(await opener.isVisible()); i++) {
            await page.locator('#providerId').selectOption({ index: i });
            await page.getByRole('button', { name: /^Search$/ }).click();
            await opener.waitFor({ timeout: 3000 }).catch(() => {});
        }
        test.skip(!(await opener.isVisible()), 'No provider has service files.');
        await opener.click();
        await expect(page.getByRole('dialog').locator('pre')).not.toBeEmpty();
        await page.keyboard.press('Escape');
        await expect(page.getByRole('dialog')).toHaveCount(0);

        const errors = page.getByRole('button', { name: /errors of/i }).first();
        if (await errors.isVisible()) {
            await errors.click();
            await expect(page.getByRole('dialog').locator('tbody tr').first()).toBeVisible();
            await page.getByRole('dialog').getByRole('button', { name: 'Close' }).first().click();
        }
        await expectNoErrorToast(page);
    });

    test('suicide files: records open in a dialog and a file downloads', async ({ page }) => {
        await openPage(page, '/suicides/files');
        await expectTitle(page, 'Display Suicide Files');
        const view = page.getByRole('button', { name: /records of/i }).first();
        test.skip(!(await view.isVisible().catch(() => false)), 'There are no suicide files.');

        await view.click();
        const dialog = page.getByRole('dialog');
        await expect(dialog.locator('tbody tr').first()).toBeVisible();
        await dialog.getByRole('button', { name: 'Last Name' }).click();   // sortable column
        await dialog.getByRole('button', { name: 'Close' }).first().click();
        await expect(dialog).toHaveCount(0);

        const download = page.waitForEvent('download');
        await page.getByRole('link', { name: /^Download/ }).first().click();
        expect((await download).suggestedFilename()).toMatch(/\.xlsx$/i);
    });
});
