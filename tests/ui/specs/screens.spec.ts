import { test, expect, openAs, apiBaseUrl } from "../fixtures";
import { seed } from "../seed";
import type { Locator } from "@playwright/test";

/**
 * <details> toggles, so clicking <summary> a second time closes it and hides the
 * actions. Only click when it is actually shut.
 */
async function openMoreActions(card: Locator) {
  const details = card.getByTestId("screen-more-actions");
  if (!(await details.evaluate((element) => (element as HTMLDetailsElement).open))) {
    await details.locator("summary").click();
  }
  await expect(details).toHaveJSProperty("open", true);
}

/**
 * Workbook cases 1-1 and 1-4 - preview without mutating, and reset safely.
 *
 * Each test seeds its own screen, so pushing or resetting one never disturbs another
 * spec's screen. The reset case is the one QA could not complete when the API fell
 * over mid-run; here it is a two-step assertion with no dependency on other cases.
 */

test("1-1 preview selects a screen without changing it", async ({ page }) => {
  const data = await seed({ role: "owner", includeScreen: true, label: "preview" });

  await openAs(page, "owner", "screens");
  const card = page.locator(`[data-testid="screen-card"][data-screen-id="${data.screenId}"]`);
  await expect(card).toBeVisible();

  // Status is deliberately NOT asserted: each card renders a live /display/{id}
  // iframe, so merely opening this page heartbeats screens to Online. The invariant
  // that matters for preview is that no content or delivery state is written.
  const revisionBefore = await card.getAttribute("data-authoritative-revision");

  const mutations: string[] = [];
  page.on("request", request => {
    if (["POST", "PUT", "PATCH", "DELETE"].includes(request.method()) && request.url().includes("/api/back-office/")) {
      mutations.push(`${request.method()} ${request.url()}`);
    }
  });

  await card.getByTestId("screen-preview").click();
  await expect(card).toHaveAttribute("data-selected", "true");

  // Preview is a read-only affordance: selecting must not write anything.
  expect(mutations, "preview must not mutate").toHaveLength(0);
  await expect(card).toHaveAttribute("data-authoritative-revision", revisionBefore ?? "");
});

test("1-4 reset connection requires deliberate confirmation", async ({ page }) => {
  const data = await seed({ role: "owner", includeScreen: true, label: "reset" });

  await openAs(page, "owner", "screens");
  const card = page.locator(`[data-testid="screen-card"][data-screen-id="${data.screenId}"]`);
  await openMoreActions(card);

  const resets: string[] = [];
  page.on("request", request => {
    if (request.url().includes("/reset")) resets.push(request.url());
  });

  await card.getByTestId("screen-reset").click();

  const dialog = page.getByTestId("destructive-review-dialog");
  await expect(dialog).toBeVisible();
  await expect(dialog).toHaveAttribute("data-tone", "caution");
  await expect(dialog).toContainText("must reconnect");

  // Cancelling must be genuinely inert.
  await dialog.getByTestId("destructive-cancel").click();
  await expect(dialog).toBeHidden();
  expect(resets, "cancelling must not issue the reset").toHaveLength(0);

  // Confirming must actually reach the API.
  await openMoreActions(card);
  await card.getByTestId("screen-reset").click();
  await page.getByTestId("destructive-review-dialog").getByTestId("destructive-confirm").click();
  await expect.poll(() => resets.length, { timeout: 7_000 }).toBeGreaterThan(0);
});

test("1-3 a heartbeat brings the screen online and delivery state is reported", async ({ page, request }) => {
  const data = await seed({ role: "owner", includeScreen: true, label: "delivery" });

  const heartbeat = await request.post(`${apiBaseUrl}/api/display/${data.screenId}/heartbeat`, {
    data: { status: "Online" },
    ignoreHTTPSErrors: true
  });
  expect(heartbeat.ok(), `heartbeat failed: ${heartbeat.status()}`).toBeTruthy();

  await openAs(page, "owner", "screens");
  const card = page.locator(`[data-testid="screen-card"][data-screen-id="${data.screenId}"]`);
  await expect(card).toHaveAttribute("data-status", "online");

  await card.getByTestId("screen-push").click();

  // Delivery must report a definite state; "applied" must never be claimed while the
  // authoritative and applied revisions still disagree.
  const banner = page.getByTestId("delivery-state");
  await expect(banner).toBeVisible();

  // Poll rather than read once: the banner renders before the delivery state settles,
  // so a single getAttribute can catch an empty or transitional value under load.
  await expect
    .poll(async () => ((await banner.getAttribute("data-state")) ?? "").toLowerCase(), { timeout: 10_000 })
    .toMatch(/^(requested|received|applied|failed)$/);

  const state = ((await banner.getAttribute("data-state")) ?? "").toLowerCase();
  if (state === "applied") {
    await expect(card).toHaveAttribute(
      "data-applied-revision",
      (await card.getAttribute("data-authoritative-revision")) ?? ""
    );
  }
});
