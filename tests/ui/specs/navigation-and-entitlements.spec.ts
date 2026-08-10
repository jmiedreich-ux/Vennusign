import { test, expect, openAs, openMenuEditorAs } from "../fixtures";
import { backOfficeRoutes } from "../../../src/back-office/src/navigation.mjs";

/**
 * Workbook cases 6-0 and 4-0.
 *
 * Importing the real route table means adding or renaming a route breaks these tests
 * rather than silently leaving it untested.
 */

test("6-0 every route is either in the rail or absent from it, and deep links resolve", async ({ page }) => {
  await openAs(page, "owner", "home");

  // Rewritten by Menus milestone 2. This used to assert that every route in the
  // table rendered an item, which stopped being true when the rail started
  // honouring decision 4: an area the plan does not include renders nothing.
  //
  // What still has to hold is that there is no third state. A route is in the
  // rail, or it is not there at all - never a ghost, a tooltip or a placeholder.
  let inTheRail = 0;

  for (const route of backOfficeRoutes) {
    const count = await page.locator(`[data-route="${route.path}"]`).count();
    expect([0, 1], `${route.path} rendered ${count} times; it should be present once or not at all`).toContain(count);
    if (count === 1) inTheRail += 1;
  }

  expect(inTheRail, "the rail rendered nothing at all").toBeGreaterThan(0);

  // A deep link must select its own route, not fall back to home.
  for (const path of ["menu", "screens", "billing"]) {
    await page.goto(`/#${path}`);
    await expect(page.locator(`[data-testid="nav-item"][data-route="${path}"]`)).toHaveAttribute("data-active", "true");
  }
});

/**
 * Acceptance criterion 8, named: "A capability outside the account's plan renders
 * nothing — no disabled control, no tooltip, no placeholder."
 *
 * Decision 4 is the rule behind it, and decision 5 is its other half: an area
 * blocked for a real reason — a permission this role lacks — still renders and
 * still says what it is. Absent and blocked are different answers, and this
 * asserts both, because getting only one right is how the rule quietly inverts.
 */
test("criterion 8 — an area outside the plan renders nothing at all", async ({ page }) => {
  await openAs(page, "owner", "home");

  // The fixture's owner has no POS entitlement, so POS is outside the plan.
  await expect(
    page.locator('[data-route="pos"]'),
    "an area outside the plan must render nothing anywhere on the page"
  ).toHaveCount(0);

  // Not merely hidden or dimmed: nothing about it reaches the page at all.
  await expect(page.locator("text=POS integrations")).toHaveCount(0);
});

test("criterion 8's other half — an area this role cannot open is still there, and still says so", async ({ page }) => {
  // Decision 5: blocked is not absent. An editor who cannot open Screens needs to
  // know Screens exists. Getting only one of these two right is how the rule
  // quietly inverts into hiding real states.
  await openAs(page, "editor", "screens");

  const blocked = page.locator('[data-testid="nav-item"][data-route="screens"]');
  await expect(blocked).toHaveCount(1);
  await expect(blocked).toHaveAttribute("data-unlocked", "false");
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
  await expect(page.getByTestId("menus-home")).toBeVisible();
});
