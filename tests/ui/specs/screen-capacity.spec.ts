import { test, expect, openAs, openMenuBuilderAs } from "../fixtures";
import { seed } from "../seed";

/**
 * Workbook case 3-0 - screen capacity.
 *
 * The fixture grants a screen.device.pair allowance of 1 and consumes it, so pairing
 * must be refused with the allowance reason rather than silently failing on submit.
 */
test("3-0 an exhausted screen allowance is explained and blocks pairing up front", async ({ page }) => {
  await openAs(page, "capacity", "screens");

  const quota = page.getByTestId("screen-quota");
  await expect(quota).toBeVisible();
  await expect(quota).toHaveAttribute("data-limit-reached", "true");
  await expect(quota).toHaveAttribute("data-reason", "allowance.reached");

  // The refusal must state the numbers, not just that something went wrong.
  await expect(quota).toContainText(/\d+ of \d+ active screens/);

  // The control is disabled before submission, and describes itself for assistive tech.
  const pair = page.getByTestId("pair-screen");
  await expect(pair).toBeDisabled();
  await expect(pair).toHaveAttribute("aria-describedby", "screen-quota-status");
});

/**
 * Workbook case 3-1 - core recovery.
 *
 * The contract is specific: exhausting the pairing allowance must not disable work on
 * screens that already exist. Only adding or pairing a new screen may be refused, so
 * every per-screen action has to remain available.
 */
test("3-1 existing screens keep every action while capacity is exhausted", async ({ page }) => {
  await page.route("**/display/**", route => route.abort());

  await openAs(page, "capacity", "screens");
  await expect(page.getByTestId("screen-quota")).toHaveAttribute("data-limit-reached", "true");

  // Adding capacity is the only thing the allowance may block.
  await expect(page.getByTestId("pair-screen")).toBeDisabled();

  const card = page.locator('[data-testid="screen-card"][data-screen-id="74000000-0000-0000-0000-000000000003"]');
  await expect(card).toBeVisible();
  await expect(card.getByTestId("screen-preview"), "preview must remain available").toBeEnabled();
  await expect(card.getByTestId("screen-push"), "push must remain available").toBeEnabled();

  await card.getByTestId("screen-more-actions").locator("summary").click();
  for (const action of ["screen-reset", "screen-archive", "screen-unpair"] as const) {
    await expect(card.getByTestId(action), `${action} must remain available`).toBeEnabled();
  }
});

/**
 * Case 3-1, unrelated-area half: capacity pressure must not leak into menu work.
 */
test("3-1 menu work is unaffected by exhausted screen capacity", async ({ page }) => {
  const data = await seed({ role: "capacity", includeScreen: false, label: "capacity-menu" });
  await openAs(page, "capacity", "screens");
  await expect(page.getByTestId("screen-quota")).toBeVisible();

  // Through the shelf, the way a person reaches the editor from milestone 2 on.
  await openMenuBuilderAs(page, "capacity", data.menuId);
  await expect(page.getByTestId("canvas")).toBeVisible();
  await page.getByTestId("board-item").first().locator(".board-item-name").click();
  await expect(page.getByTestId("item-name")).toBeEnabled();
});
