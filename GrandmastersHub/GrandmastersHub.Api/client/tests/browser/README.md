# Cart refresh browser tests

These tests run the actual React cart in Chromium. API routes return controlled test data, and a second tab sends real storage events while mutation responses are held. They do not require SQL Server or the .NET API.

From `GrandmastersHub/GrandmastersHub.Api/client`:

```sh
npm ci
cd tests/browser
npm install --ignore-scripts
npx playwright install chromium
npm test
```

The test runner has a separate development-only package so the application's dependencies and lockfile do not change. Playwright starts and stops Vite on port 4179. Do not run another server on that port. Screenshots and traces are saved in `test-results` after failures.
