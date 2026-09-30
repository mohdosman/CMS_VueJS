import { defineConfig, devices } from '@playwright/test';
import { loadTestEnv } from './tests/testEnv.js';

loadTestEnv();
const baseURL = process.env.CMS_BASE_URL ?? 'https://localhost:7031';
const authFile = 'playwright/.auth/cms.json';

// Accessibility (axe), keyboard and end-to-end suites. Start the app first (see tests/README.md): the Vue bundle needs the
// server-rendered boot data, the permitted routes, the antiforgery token and cookie sign-in, so the Vite dev server alone will not do.
export default defineConfig({
    testDir: './tests',
    timeout: 60_000,
    expect: { timeout: 10_000 },
    // One worker: the e2e specs create and remove records in a shared database.
    workers: 1,
    reporter: [['list'], ['html', { open: 'never' }]],
    use: { baseURL, trace: 'retain-on-failure', screenshot: 'only-on-failure', ignoreHTTPSErrors: true },
    projects: [
        { name: 'auth', testMatch: /.*\.setup\.js/ },
        {
            name: 'chromium-a11y',
            dependencies: ['auth'],
            testMatch: /.*(?:\.a11y|keyboard)\.spec\.js/,
            use: { ...devices['Desktop Chrome'], storageState: authFile }
        },
        {
            name: 'chromium-e2e',
            dependencies: ['auth'],
            testMatch: /.*\.e2e\.spec\.js/,
            use: { ...devices['Desktop Chrome'], storageState: authFile }
        }
    ]
});
