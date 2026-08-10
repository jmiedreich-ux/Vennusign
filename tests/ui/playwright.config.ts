import { defineConfig, devices } from "@playwright/test";

/**
 * Runs against an already-running local environment. Start it with:
 *   scripts/run-track1-qa.ps1 -SkipBuild -PrepareOnly -KeepServices
 * or by running the API and `npm run dev` in src/back-office yourself.
 */
export default defineConfig({
  testDir: "./specs",
  // Clears accumulated seed data first; without it the screens page grows an extra
  // live display iframe per seeded screen and the suite slows until it times out.
  globalSetup: "./global-setup.ts",
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  reporter: [["list"], ["html", { open: "never" }]],
  // Cases are independent and seed their own state, so the suite is bounded by
  // the slowest single case rather than by any chain of cases.
  //
  // Bounded workers, because the whole suite runs against ONE dev server and one
  // LocalDB. Milestone 3 roughly doubled the case count, and past this point the
  // failures stop being about the product: the same cases pass alone and fail in
  // a full run, which is a queue, not a defect. An unbounded pool would keep
  // producing red runs that teach nothing.
  workers: process.env.CI ? 2 : 3,
  timeout: 30_000,
  expect: { timeout: 7_000 },
  use: {
    baseURL: process.env.VENNU_BACK_OFFICE_URL ?? "https://localhost:5174",
    // Vite basic-ssl and the Kestrel dev cert are both self-signed locally.
    ignoreHTTPSErrors: true,
    trace: "retain-on-failure",
    screenshot: "only-on-failure",
    video: "retain-on-failure"
  },
  projects: [
    { name: "desktop", use: { ...devices["Desktop Chrome"] } },
    { name: "mobile", use: { ...devices["Pixel 7"] } }
  ]
});
