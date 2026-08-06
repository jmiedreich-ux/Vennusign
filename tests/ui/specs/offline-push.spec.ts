import { test, expect, openAs } from "../fixtures";
import { seed } from "../seed";

/**
 * Workbook case 1-2 - queue a push while the screen is Offline.
 *
 * The invariant is honesty about state: an offline player cannot have applied
 * anything, so the UI must report the revision as requested/pending and must never
 * claim it was applied.
 */
test("1-2 pushing to an offline screen queues rather than claiming delivery", async ({ page }) => {
  const data = await seed({ role: "owner", includeScreen: true, label: "offline" });

  // Each screen card embeds a live /display/{id} iframe, and that player heartbeats
  // the screen Online. Without blocking it, simply opening this page makes every
  // screen online and the offline path becomes untestable through the UI.
  await page.route("**/display/**", route => route.abort());

  await openAs(page, "owner", "screens");
  // Scope to the fleet card: data-screen-id also appears on the delivery target list.
  const card = page.locator(`[data-testid="screen-card"][data-screen-id="${data.screenId}"]`);
  await expect(card).toBeVisible();

  // A freshly seeded screen has never heartbeat, so it must not read as online.
  await expect(card).not.toHaveAttribute("data-status", "online");

  await card.getByTestId("screen-push").click();

  const banner = page.getByTestId("delivery-state");
  await expect(banner).toBeVisible();
  await expect
    .poll(async () => ((await banner.getAttribute("data-state")) ?? "").toLowerCase(), { timeout: 10_000 })
    .toMatch(/^(requested|failed)$/);

  // The offline path must explain recovery rather than report success.
  const state = ((await banner.getAttribute("data-state")) ?? "").toLowerCase();
  if (state === "requested") {
    await expect(banner).toContainText(/offline|stale|reconnect/i);
    await expect(card).not.toHaveAttribute("data-delivery-state", "applied");
    // Nothing can be applied by a player that has never connected.
    expect(await card.getAttribute("data-applied-revision")).toBeFalsy();
  }
});
