import { test, expect, openAs } from "../fixtures";
import { backOfficeRoutes } from "../../../src/back-office/src/navigation.mjs";

/**
 * Workbook cases 6-0 and 4-0.
 *
 * Importing the real route table means adding or renaming a route breaks these tests
 * rather than silently leaving it untested.
 */

test("6-0 every route renders a nav item and deep links resolve", async ({ page }) => {
  await openAs(page, "owner", "home");

  for (const route of backOfficeRoutes) {
    await expect(
      page.locator(`[data-testid="nav-item"][data-route="${route.path}"]`),
      `missing nav item for ${route.path}`
    ).toHaveCount(1);
  }

  // A deep link must select its own route, not fall back to home.
  for (const path of ["menu", "screens", "billing"]) {
    await page.goto(`/#${path}`);
    await expect(page.locator(`[data-testid="nav-item"][data-route="${path}"]`)).toHaveAttribute("data-active", "true");
  }
});

test("6-0 an unknown hash route falls back to home rather than blanking", async ({ page }) => {
  await openAs(page, "owner", "definitely-not-a-route");
  await expect(page.locator('[data-testid="nav-item"][data-route="home"]')).toHaveAttribute("data-active", "true");
});

test("4-0 an entitlement-blocked area explains itself and leaves core work usable", async ({ page }) => {
  await openAs(page, "owner", "pos");

  // A refusal renders either as the plain locked panel or, when an upgrade path
  // exists, as the richer locked preview. Both are legitimate; a blank area is not.
  const panel = page.getByTestId("locked-panel");
  const preview = page.getByTestId("locked-preview");
  const refusal = page.locator('[data-testid="locked-panel"], [data-testid="locked-preview"]');
  await expect(refusal).toHaveCount(1);

  if (await panel.count()) {
    await expect(panel).toHaveAttribute("data-route", "pos");
    expect(["entitlement", "rollout", "permission"]).toContain((await panel.getAttribute("data-category")) ?? "");
    expect(["denied", "unavailable", "temporarily-blocked"]).toContain((await panel.getAttribute("data-decision")) ?? "");
  } else {
    await expect(preview).toBeVisible();
    expect((await preview.getAttribute("data-feature")) ?? "").not.toHaveLength(0);
  }
  await expect(refusal).not.toContainText("could not verify access");

  // Core work stays reachable while that area is blocked.
  await page.goto("/#menu");
  await expect(page.getByTestId("menu-picker")).toBeVisible();
});
