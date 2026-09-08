import { defineConfig, devices } from '@playwright/test';

/**
 * Runs against a local `ng serve` (auto-started below) and expects the .NET API to already be
 * running at http://localhost:5120 (see README.md, "Testes end-to-end") — Playwright only owns
 * the frontend process, not the backend.
 */
export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  retries: 0,
  reporter: 'list',
  use: {
    baseURL: 'http://localhost:4200',
    trace: 'retain-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: {
    command: 'npm start',
    url: 'http://localhost:4200',
    reuseExistingServer: true,
    timeout: 120_000,
  },
});
