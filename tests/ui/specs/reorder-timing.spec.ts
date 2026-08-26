import { test, expect } from "@playwright/test";
import { openAddItem, openReview, publishDraft } from "../fixtures";
// @ts-expect-error - plain .mjs helpers, shared with non-Playwright QA tooling.
import { qaCredentials, qaCredentialSources, signInAsCustomer } from "../lib/customerAccount.mjs";
// @ts-expect-error - see above.
import { loadSnapshot } from "../lib/customerOnboarding.mjs";

/**
 * A measurement, not an assertion.
 *
 * "There was a lag in the interface, and it took a few seconds for the screen to
 * update" - this puts numbers on both halves of that, because dev keeps no request
 * telemetry at all (no Application Insights, HTTP logs disabled, container logs
 * carry lifecycle only), so the only way to time the sequence is to run it.
 *
 * Everything below is measured from the interface - the real rail, the real publish
 * button, and the real player. Nothing here asks the API a question the browser was
 * not already asking on its own; the two server numbers are read passively off the
 * page's own network traffic.
 *
 * It reports, in milliseconds:
 *   reorder.server   drop -> the reorder PUT came back
 *   reorder.ui       drop -> the rail actually shows the new order
 *   publish.server   click -> the publish POST came back
 *   publish.screen   click -> the live player has redrawn it
 *
 * Run it like the specs, with --grep "timing". It fails only if the sequence
 * cannot be completed; slow is data, not a failure.
 */
const apiBaseUrl = process.env.VENNU_API_URL ?? "https://dev.api.vennusign.com";
const displayBaseUrl = process.env.VENNU_DISPLAY_URL ?? "https://dev.display.vennusign.com";
const credentials = qaCredentials();

const addItem = async (page: any, name: string, price: string) => {
  await openAddItem(page);
  await page.getByTestId("add-item-input").fill(name);
  await page.getByTestId("add-item-price").fill(price);
  await page.getByTestId("add-item-create").click();
  await expect(page.getByTestId("board")).toContainText(name, { timeout: 60_000 });
};

test.describe("reorder timing", () => {
  test.describe.configure({ mode: "serial", timeout: 600_000 });
  test.skip(!credentials, `No QA customer credentials. Looked in ${qaCredentialSources()}.`);
  test.beforeEach(({}, testInfo) => test.skip(testInfo.project.name === "mobile", "Desktop only."));

  test("timing: how long a section reorder takes to reach the wall", async ({ page, context }) => {
    test.setTimeout(600_000);

    // Server time for the calls this sequence makes, keyed by a name we recognise.
    const serverMs: Record<string, number[]> = {};
    const started = new Map<string, number>();
    page.on("request", request => started.set(request.url() + request.method(), Date.now()));
    page.on("requestfinished", request => {
      const url = request.url();
      const key = url.includes("/sections/order") || url.includes("/sections/reorder") ? "reorderCall"
        : url.includes("/publish") ? "publishCall"
        : null;
      if (!key) return;
      const t0 = started.get(url + request.method());
      if (t0) (serverMs[key] ??= []).push(Date.now() - t0);
    });

    await signInAsCustomer(page, credentials);
    const snapshot = await loadSnapshot(page, apiBaseUrl);
    const screenId = snapshot.firstScreenId;
    expect(screenId, "the QA account needs a paired screen").toBeTruthy();

    const stamp = Math.random().toString(16).slice(2, 8);
    const alpha = `Alpha ${stamp}`;
    const beta = `Beta ${stamp}`;

    // Build a two-section menu on the screen, published, so the reorder is the only
    // thing being timed rather than the whole first-publish cost.
    await page.goto("/#/menu");
    await page.locator('[data-testid="nav-item"]').first().waitFor({ state: "attached" });
    // M6.5: no name prompt - the builder's crumb names it.
    await page.getByTestId("add-a-menu").first().click();
    await page.getByTestId("add-route-blank").click();
    await expect(page.getByTestId("menu-builder")).toBeVisible({ timeout: 60_000 });
    await page.getByTestId("menu-name-input").fill(`QA timing ${stamp}`);
    await page.getByTestId("menu-name-input").press("Enter");

    await page.getByTestId("assignment-pill").click();
    const assignments = page.getByTestId("screen-assignments");
    await expect(assignments).toBeVisible();
    await assignments.getByTestId("screen-row").first().getByTestId("add-screen-page").click();
    await assignments.getByTestId("add-screen-page-menu").getByRole("button").first().click();
    const choice = assignments.getByTestId("assignment-choice");
    if (await choice.isVisible().catch(() => false)) {
      await choice.getByRole("button", { name: /^replace$/i }).click();
    }
    await page.getByRole("button", { name: /save changes and return/i }).click();
    await expect(page.getByTestId("menu-builder")).toBeVisible({ timeout: 60_000 });

    const rows = page.getByTestId("section-row");
    await rows.first().getByRole("button", { name: /^Rename / }).click();
    await page.getByTestId("section-rename-input").fill(alpha);
    await page.keyboard.press("Enter");
    await expect(rows.first()).toContainText(alpha, { timeout: 30_000 });

    await page.getByTestId("add-section").click();
    await page.getByTestId("new-section-name").fill(beta);
    await page.keyboard.press("Enter");
    await expect(rows).toHaveCount(2, { timeout: 30_000 });

    await page.getByTestId("rail-section").filter({ hasText: alpha }).click();
    await addItem(page, `Alpha Item ${stamp}`, "1.00");
    await page.getByTestId("rail-section").filter({ hasText: beta }).click();
    await addItem(page, `Beta Item ${stamp}`, "2.00");

    await publishDraft(page);
    await expect(page.getByTestId("publish-bar")).toBeVisible({ timeout: 60_000 });

    const player = await context.newPage();
    await player.goto(`${displayBaseUrl}/display/${screenId}`);
    await expect(player.locator("body")).toContainText(`Alpha Item ${stamp}`, { timeout: 120_000 });

    // ---- the measured sequence starts here ----
    serverMs.reorderCall = [];
    const dropAt = Date.now();
    const moving = rows.filter({ hasText: beta });
    const landing = rows.filter({ hasText: alpha });
    await moving.dispatchEvent("dragstart");
    await landing.dispatchEvent("dragover");
    await landing.dispatchEvent("drop");
    await expect(rows.first()).toContainText(beta, { timeout: 60_000 });
    const reorderUiMs = Date.now() - dropAt;

    // Open the review dialog first, outside the measured window - the timing
    // here is about the publish call itself, not about how long the Actions
    // dropdown and review dialog take to open.
    await openReview(page);
    serverMs.publishCall = [];
    const publishAt = Date.now();
    await page.getByTestId("publish-from-review").click();
    await expect(page.getByTestId("publish-bar")).toBeVisible({ timeout: 60_000 });

    // When the live player has actually redrawn it.
    let screenMs = -1;
    await expect(async () => {
      const drawn = await player.locator("body").innerText();
      const betaAt = drawn.indexOf(`Beta Item ${stamp}`);
      const alphaAt = drawn.indexOf(`Alpha Item ${stamp}`);
      expect(betaAt, `drawn: ${drawn.slice(0, 200)}`).toBeGreaterThanOrEqual(0);
      expect(betaAt).toBeLessThan(alphaAt >= 0 ? alphaAt : Number.MAX_SAFE_INTEGER);
      if (screenMs < 0) screenMs = Date.now() - publishAt;
    }).toPass({ timeout: 240_000, intervals: [500, 1_000, 2_000, 5_000] });

    const report = [
      "",
      "==================== REORDER TIMING (dev, B1) ====================",
      `reorder.server   ${serverMs.reorderCall?.join(", ") || "not observed"} ms   the reorder call itself`,
      `reorder.ui       ${reorderUiMs} ms   drop -> the rail shows the new order`,
      `publish.server   ${serverMs.publishCall?.join(", ") || "not observed"} ms   the publish call itself`,
      `publish.screen   ${screenMs} ms   publish -> the wall has redrawn it`,
      "==================================================================",
      ""
    ].join("\n");
    console.log(report);
    test.info().annotations.push({ type: "timing", description: report });

    await player.close();
  });
});
