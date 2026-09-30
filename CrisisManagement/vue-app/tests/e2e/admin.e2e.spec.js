import { test, expect } from '@playwright/test';
import { expectNoErrorToast, expectTitle, openPage } from './helpers/e2eHelpers.js';

// The administration screens load their data and open a record; their create/edit flows are covered by the API tests.
test.describe('Administration screens', () => {
    test('users: search, open a user', async ({ page }) => {
        await openPage(page, '/admin/users');
        await expectTitle(page, 'Search Users');
        await page.getByRole('button', { name: /^Search$/ }).click();
        const first = page.locator('tbody tr').first().getByRole('link').first();
        await expect(first).toBeVisible();
        await first.click();
        await expect(page).toHaveURL(/#\/admin\/users\/.+/);
        await expect(page.getByRole('button', { name: /^Save$/ })).toBeVisible();
        await expectNoErrorToast(page);
    });

    test('roles: search, open a role and see its permissions', async ({ page }) => {
        await openPage(page, '/admin/roles');
        await page.getByRole('button', { name: /^Search$/ }).click();
        const first = page.locator('tbody tr').first().getByRole('link').first();
        await expect(first).toBeVisible();
        await first.click();
        await expect(page).toHaveURL(/#\/admin\/roles\/.+/);
        await expect(page.getByRole('checkbox').first()).toBeVisible();
        await expectNoErrorToast(page);
    });

    test('menus: the tree loads and a menu opens', async ({ page }) => {
        await openPage(page, '/admin/menus');
        const link = page.getByRole('link', { name: /Report Listing/ }).first();
        await expect(link).toBeVisible();
        await link.click();
        await expect(page).toHaveURL(/#\/admin\/menus\/\d+/);
        await expectNoErrorToast(page);
    });

    test('public files: the list loads', async ({ page }) => {
        await openPage(page, '/publicfiles');
        await expect(page.getByLabel(/Choose PDF file/)).toBeAttached();
        await expectNoErrorToast(page);
    });

    test('profile: shows the account and its two-factor status', async ({ page }) => {
        await openPage(page, '/profile');
        await expect(page.getByRole('term').filter({ hasText: 'User ID' })).toBeVisible();
        await expect(page.getByText(/Two-factor authentication/).first()).toBeVisible();
        await expect(page.getByRole('link', { name: /Set up|Re-enroll device/ })).toHaveAttribute('href', /\/Account\/SetupMfa$/);
    });

    test('the user menu opens with the keyboard and My Profile navigates', async ({ page }) => {
        await openPage(page, '/');
        const toggle = page.locator('.user_menu .dropdown-toggle');
        await toggle.focus();
        await page.keyboard.press('Enter');
        await expect(toggle).toHaveAttribute('aria-expanded', 'true');
        await page.getByRole('link', { name: /My Profile/ }).click();
        await expect(page).toHaveURL(/#\/profile$/);
    });
});
