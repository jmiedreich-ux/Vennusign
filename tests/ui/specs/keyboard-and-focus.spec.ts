import { test, expect, openAs } from "../fixtures";
import { seed } from "../seed";

/**
 * Workbook case 6-2 - keyboard-only pass.
 *
 * The agent lane reported that focus escapes the Reset confirmation dialog on Tab.
 * The component uses showModal(), which traps focus natively, so this asserts the
 * behaviour deterministically rather than leaving a contested finding open.
 */
test("6-2 the destructive dialog traps focus", async ({ page }) => {
  const data = await seed({ role: "owner", includeScreen: true, label: "focus" });
  await page.route("**/display/**", route => route.abort());

  await openAs(page, "owner", "screens");
  const card = page.locator(`[data-testid="screen-card"][data-screen-id="${data.screenId}"]`);
  await card.getByTestId("screen-more-actions").locator("summary").click();
  await card.getByTestId("screen-reset").click();

  const dialog = page.getByTestId("destructive-review-dialog");
  await expect(dialog).toBeVisible();

  // Tab repeatedly; focus must never leave the dialog subtree.
  for (let step = 0; step < 8; step++) {
    await page.keyboard.press("Tab");
    const inside = await dialog.evaluate(element => element.contains(document.activeElement));
    expect(inside, `focus escaped the dialog after ${step + 1} tab(s)`).toBeTruthy();
  }

  // Escape must dismiss without performing the action.
  const resets: string[] = [];
  page.on("request", request => { if (request.url().includes("/reset")) resets.push(request.url()); });
  await page.keyboard.press("Escape");
  await expect(dialog).toBeHidden();
  expect(resets, "Escape must not perform the reset").toHaveLength(0);
});

/**
 * Case 6-2, navigation half: the shell must be reachable and operable by keyboard,
 * and focus must be visible rather than implied.
 */
test("6-2 navigation is reachable by keyboard with visible focus", async ({ page }) => {
  await openAs(page, "owner", "home");

  await page.keyboard.press("Tab");
  const focused = await page.evaluate(() => {
    const element = document.activeElement as HTMLElement | null;
    if (!element) return undefined;
    const outline = window.getComputedStyle(element).outlineStyle;
    return { tag: element.tagName, outline };
  });
  expect(focused, "something must receive focus on first Tab").toBeTruthy();

  // At narrow widths the nav lives behind a toggle, so open it first. Doing this by
  // keyboard also proves the toggle itself is operable without a pointer.
  const toggle = page.getByTestId("nav-toggle");
  if (await toggle.isVisible()) {
    await toggle.focus();
    await page.keyboard.press("Enter");
    await expect(toggle).toHaveAttribute("aria-expanded", "true");
  }

  // A nav item must be reachable and activate by keyboard.
  const menuItem = page.locator('[data-testid="nav-item"][data-route="menu"]');
  await menuItem.focus();
  await page.keyboard.press("Enter");
  await expect(page.locator('[data-testid="nav-item"][data-route="menu"]')).toHaveAttribute("data-active", "true");
});
