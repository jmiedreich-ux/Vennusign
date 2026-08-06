import { test, expect, openAs } from "../fixtures";

/**
 * Workbook case 6-1 - navigation at narrow mobile width.
 *
 * The agent found the sidebar stayed fully expanded at 390x844 (~532-680px tall) with
 * no toggle, so page content only began after scrolling past the whole nav. These
 * assertions pin the fix: content must be reachable without scrolling past the nav,
 * the nav must still be openable, and choosing a destination must dismiss it.
 */
test.describe("6-1 narrow width navigation", () => {
  test.skip(({ viewport }) => (viewport?.width ?? 1280) > 760, "mobile widths only");

  test("page content starts above the fold with the nav collapsed", async ({ page, viewport }) => {
    await openAs(page, "owner", "menu");

    const toggle = page.getByTestId("nav-toggle");
    await expect(toggle).toBeVisible();
    await expect(toggle).toHaveAttribute("aria-expanded", "false");

    // The primary content must begin within the first viewport, not below the nav.
    const main = page.locator("main");
    const box = await main.boundingBox();
    expect(box, "main content must be laid out").toBeTruthy();
    expect(box!.y, "content must start above the fold").toBeLessThan((viewport?.height ?? 844) / 2);

    // And the document must not scroll sideways.
    const overflow = await page.evaluate(() => document.documentElement.scrollWidth - window.innerWidth);
    expect(overflow, "no horizontal overflow").toBeLessThanOrEqual(0);
  });

  test("the nav opens, navigates, and dismisses itself", async ({ page }) => {
    await openAs(page, "owner", "home");

    const toggle = page.getByTestId("nav-toggle");
    await toggle.click();
    await expect(toggle).toHaveAttribute("aria-expanded", "true");

    const menuItem = page.locator('[data-testid="nav-item"][data-route="menu"]');
    await expect(menuItem).toBeVisible();
    await menuItem.click();

    // Selecting a destination must close the drawer, or the page stays hidden behind it.
    await expect(toggle).toHaveAttribute("aria-expanded", "false");
    await expect(page.getByTestId("menu-picker")).toBeVisible();
  });
});
