# Playwright suites

Three suites run against the running app, as in SafetyNet:

| Suite | Files | What it checks |
|---|---|---|
| Accessibility (axe) | `routes.a11y.spec.js`, `themes.a11y.spec.js` | WCAG 2.0/2.1/2.2 A and AA on every menu route the test user may open, the record pages (`/x/0`), the sign-in page, and the contrast-sensitive pages in every colour theme |
| Keyboard | `keyboard.spec.js` | Tab through every route: focus never falls to `<body>`, never traps, always shows an indicator. Dialogs keep focus and close with Escape; the assessment sections toggle with Enter and Space; Enter submits a search |
| End to end | `e2e/*.e2e.spec.js` | Real workflows in the browser: providers, notifications, assessments, services, reports, file uploads and lists, administration screens, profile |

The routes are not listed in the tests: they are read from the boot data the server renders for the signed-in user, so a route the user cannot open is skipped.

## Set up

```powershell
cd C:\temp\CMS_VueJS\CrisisManagement\vue-app
npm install
npx playwright install chromium
Copy-Item .env.test.example .env.test.local     # then edit it
```

`.env.test.local` (ignored by git) holds the app URL and the test user, an Administrator or a user with the screens under test:

```text
CMS_BASE_URL=https://localhost:7031
CMS_USERNAME=your-user-id
CMS_PASSWORD=your-password
A11Y_EXTRA_ROUTES=/assessments/f2f-6172,/services/446
```

`A11Y_EXTRA_ROUTES` adds record pages that need a real id. The test user must not need a temporary password or two-factor setup, or the sign-in step stops on that page.

Start the app first (`dotnet run --project CrisisManagement --launch-profile https`). Do not point the tests at the Vite dev server: the bundle needs the server-rendered boot data, antiforgery token and cookie sign-in.

## Run

```powershell
npm run test:a11y          # axe + themes + keyboard
npm run test:a11y:axe      # axe on the routes only
npm run test:a11y:keyboard
npm run test:e2e
npm run test:e2e:headed    # watch it
npm run test:all
```

## What the end-to-end tests leave behind

They create records with an `E2E` prefix and delete them again (providers, notifications, assessments, services), also when a step fails. Uploads are only ever refused files, so nothing is stored. Running a report writes one row to the report log per run.

## Adding a screen

1. Give it a visually hidden `<h1 id="main-title">`; `expectTitle(page, 'Text')` checks it (the visible card heading repeats the same words, so `getByRole('heading', { name })` matches twice).
2. Label every control. `label('Name')` in `helpers/e2eHelpers.js` matches a label with or without its `*`.
3. Write the spec in `e2e/`, with `uniqueId('E2E...')` data and a `finally` that removes it through `callApi`.
