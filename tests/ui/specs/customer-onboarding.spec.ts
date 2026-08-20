import { test, expect } from "@playwright/test";
// @ts-expect-error - plain .mjs helpers, shared with non-Playwright QA tooling.
import { qaCredentials, qaCredentialSources, signInAsCustomer } from "../lib/customerAccount.mjs";
// @ts-expect-error - see above.
import { ensureOnboarded, loadSnapshot, newAccountDetails } from "../lib/customerOnboarding.mjs";
// @ts-expect-error - see above.
import { startQaDisplay } from "../lib/qaDisplay.mjs";

/**
 * Customer onboarding against a DEPLOYED environment, signed in through the real
 * Entra flow. Unlike the rest of this suite it does not use a Back Office session
 * token, because the thing under test is the customer journey itself.
 *
 * Point it at an environment with VENNU_BACK_OFFICE_URL and VENNU_API_URL, e.g.
 *   VENNU_BACK_OFFICE_URL=https://dev.back-office.vennusign.com \
 *   VENNU_API_URL=https://dev.api.vennusign.com \
 *   npx playwright test specs/customer-onboarding.spec.ts --project=desktop
 */
const apiBaseUrl = process.env.VENNU_API_URL ?? "https://dev.api.vennusign.com";
const credentials = qaCredentials();

// Only the signed-in cases need credentials. The display case does not, and must
// still run on a machine that has no QA account configured.
test.describe("signed in as the QA customer", () => {
  test.skip(!credentials, `No QA customer credentials. Looked in ${qaCredentialSources()}.`);
  test.describe.configure({ mode: "serial" });

test("the QA account reaches Back Office instead of the onboarding checklist", async ({ page }) => {
  test.slow();
  await signInAsCustomer(page, credentials);

  const snapshot = await ensureOnboarded(page, apiBaseUrl, newAccountDetails("murphy"));
  expect(snapshot.progress.goLive, "the QA account must finish onboarding").toBe(true);
  expect(snapshot.goLiveAchievedUtc, "go-live must be recorded, not inferred").toBeTruthy();

  // The point of the whole exercise: signing in again lands inside the product.
  await page.goto("/signin");
  await page.waitForURL(url => !url.pathname.startsWith("/onboarding"), { timeout: 30_000 });
  expect(new URL(page.url()).pathname).not.toBe("/onboarding");
});

test("a display that has gone live and is now offline does not re-open onboarding", async ({ page }) => {
  test.slow();
  await signInAsCustomer(page, credentials);
  const onboarded = await ensureOnboarded(page, apiBaseUrl, newAccountDetails("murphy"));
  expect(onboarded.progress.goLive).toBe(true);

  // Report the first display Offline, which is what HeartbeatMonitor does by itself
  // after 90 seconds of silence - a venue that powers its screens down overnight.
  // Doing it explicitly makes the case fast and deterministic rather than a sleep.
  const response = await page.request.post(`${apiBaseUrl}/api/display/${onboarded.firstScreenId}/heartbeat`, {
    data: { status: "Offline", platform: "browser", appVersion: "qa-display" }
  });
  expect(response.ok()).toBe(true);

  const afterOffline = await loadSnapshot(page, apiBaseUrl);
  expect(afterOffline.firstScreenStatus, "the device is genuinely offline").toBe("paired-offline");
  expect(afterOffline.progress.goLive, "but onboarding stays complete").toBe(true);
  expect(afterOffline.goLiveAchievedUtc).toBe(onboarded.goLiveAchievedUtc);

  // And the customer is still let into the product.
  await page.goto("/signin");
  await page.waitForURL(url => !url.pathname.startsWith("/onboarding"), { timeout: 30_000 });
});

});

test("a QA display registers, pairs and reports online through the shipped player code", async () => {
  const display = await startQaDisplay(apiBaseUrl);
  try {
    expect(display.screenId).toBeTruthy();
    expect(display.code, "a display shows a six-digit pairing code").toMatch(/^\d{6}$/);

    const status = await display.pairingStatus();
    expect(status.linked, "a freshly minted code is unclaimed until someone pairs it").toBe(false);
  } finally {
    display.stop();
  }
});
