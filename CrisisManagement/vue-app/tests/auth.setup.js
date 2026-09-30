import { test as setup, expect } from '@playwright/test';
import { loadTestEnv } from './testEnv.js';

const authFile = 'playwright/.auth/cms.json';

setup('authenticate', async ({ page, baseURL }) => {
    loadTestEnv();
    const username = process.env.CMS_USERNAME;
    const password = process.env.CMS_PASSWORD;
    if (!username || !password) {
        throw new Error('Set CMS_USERNAME and CMS_PASSWORD in .env.test.local or the environment before running the suites.');
    }

    await page.goto('/Account/Login');
    await page.getByLabel(/user id\/email/i).fill(username);
    await page.getByLabel(/^password$/i).fill(password);
    await page.getByRole('button', { name: /^sign in$/i }).click();

    await expect(page).not.toHaveURL(/\/Account\/(Login|Change|SetupMfa|VerifyMfa)/i);
    await page.context().storageState({ path: authFile });
    console.log(`Saved authenticated state for ${baseURL} to ${authFile}`);
});
