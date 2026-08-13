import { expect, test, openAs } from "../fixtures";
import { seed } from "../seed";

test.describe("the 86 board", () => {
  test.beforeEach(({}, testInfo) => test.skip(testInfo.project.name !== "desktop", "Menus is a desktop surface"));

  test("searches published on-screen placements and confirms both venue-wide availability changes", async ({ page }) => {
    await seed({ role: "owner", includeScreen: true, label: "86-board", itemsPerSection: 2 });
    await openAs(page, "owner", "/menu/quick-update");

    const board = page.getByTestId("quick-update-board");
    await expect(board).toBeVisible();
    const firstTile = board.locator(".quick-update__grid button").first();
    await expect(firstTile).toBeVisible();
    const itemName = (await firstTile.locator("strong").innerText()).trim();
    await board.getByPlaceholder("Search every menu on the screens").fill(itemName);
    const tile = board.locator(".quick-update__grid button").filter({ hasText: itemName }).first();
    await expect(tile).toBeVisible();
    await tile.click();
    await expect(page.getByTestId("destructive-review-dialog")).toContainText(`86 ${itemName}?`);
    await page.getByTestId("destructive-confirm").click();

    const offCard = board.locator(".quick-update__off article").filter({ hasText: itemName });
    await expect(offCard).toBeVisible();
    await page.reload();
    await expect(board.locator(".quick-update__off article").filter({ hasText: itemName })).toBeVisible();

    await board.locator(".quick-update__off article").filter({ hasText: itemName }).getByRole("button", { name: "Back on sale" }).click();
    await expect(page.getByTestId("destructive-review-dialog")).toContainText(`Put ${itemName} back on sale?`);
    await page.getByTestId("destructive-confirm").click();
    await expect(board.locator(".quick-update__off article").filter({ hasText: itemName })).toHaveCount(0);
  });

  test("publish-only staff do not see the 86-board entry point", async ({ page }) => {
    await openAs(page, "publisher", "menu");
    await expect(page.getByTestId("locked-panel")).toHaveAttribute("data-category", "permission");
    await expect(page.getByTestId("quick-update-board")).toHaveCount(0);
  });

  test("blank menu creation stays a separate Menus-home path", async ({ page }) => {
    await openAs(page, "owner", "menu");
    await page.getByTestId("add-a-menu").click();
    const dialog = page.getByTestId("name-menu-dialog");
    await expect(dialog).toContainText("Start a blank menu");
    await expect(page.getByTestId("quick-update-board")).toHaveCount(0);
    await page.getByTestId("new-menu-name").fill(`Blank ${Date.now()}`);
    await dialog.getByRole("button", { name: "Start blank" }).click();
    await expect(page.getByTestId("menu-builder")).toBeVisible();
  });
});
