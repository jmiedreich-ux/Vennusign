import { test, expect, openAs } from "../fixtures";

/**
 * Workbook case 3-0 - screen capacity.
 *
 * The fixture grants a screen.device.pair allowance of 1 and consumes it, so pairing
 * must be refused with the allowance reason rather than silently failing on submit.
 */
test("3-0 an exhausted screen allowance is explained and blocks pairing up front", async ({ page }) => {
  await openAs(page, "owner", "screens");

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
 * Workbook case 3-1 - core recovery. Capacity pressure must not disable unrelated
 * core work; the menu editor has to stay fully usable.
 */
test("3-1 core work stays usable while capacity is exhausted", async ({ page }) => {
  await openAs(page, "owner", "screens");
  await expect(page.getByTestId("screen-quota")).toBeVisible();

  await page.goto("/#menu");
  await expect(page.getByTestId("menu-picker")).toBeVisible();
  await expect(page.getByTestId("menu-item").first().getByTestId("item-name")).toBeEnabled();
});
