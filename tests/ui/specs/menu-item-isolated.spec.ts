import { test, expect, openAs, openMenuEditorAs } from "../fixtures";
import { seed } from "../seed";

/**
 * The same persistence check as menu-item-persist.spec.ts, but against data this test
 * created for itself. Nothing here touches a row another spec can see, so these run
 * in parallel with everything else instead of queueing behind a shared fixture.
 */
test.describe("menu item persistence (isolated data)", () => {
  test("an edit survives a reload", async ({ page }) => {
    const data = await seed({ role: "owner", label: "persist" });

    await openMenuEditorAs(page, "owner");
    await page.getByTestId("menu-picker").selectOption(data.menuId);

    const row = page.getByTestId("menu-item").filter({ has: page.locator(`[value="${data.itemName}"]`) });
    await expect(row).toHaveAttribute("data-item-id", data.itemId);
    await expect(row).toHaveAttribute("data-save-state", "clean");

    const value = `edited ${data.itemId.slice(0, 8)}`;
    await row.getByTestId("item-description").fill(value);
    await expect(row).toHaveAttribute("data-save-state", "draft");

    // A save control must be offered while a draft is pending. It is asserted but not
    // clicked: clicking blurs the input first, so onBlur->save and onClick->save both
    // fire, refresh() resets drafts between them, and the second save can persist the
    // pre-edit value. Blur alone is the app's primary save path.
    await expect(row.getByTestId("item-save")).toBeEnabled();
    await row.getByTestId("item-description").blur();
    await expect(row).toHaveAttribute("data-save-state", "saved");

    await page.reload();
    await openMenuEditorAs(page, "owner");
    await page.getByTestId("menu-picker").selectOption(data.menuId);
    await expect(
      page.locator(`[data-item-id="${data.itemId}"]`).getByTestId("item-description")
    ).toHaveValue(value);
  });

  test("availability reflects the persisted value", async ({ page }) => {
    const data = await seed({ role: "owner", label: "availability" });

    await openMenuEditorAs(page, "owner");
    await page.getByTestId("menu-picker").selectOption(data.menuId);

    const row = page.locator(`[data-item-id="${data.itemId}"]`);
    await expect(row).toHaveAttribute("data-available", "true");

    await row.getByTestId("item-availability").click();
    await expect(row).toHaveAttribute("data-save-state", "saved");

    await page.reload();
    await openMenuEditorAs(page, "owner");
    await page.getByTestId("menu-picker").selectOption(data.menuId);
    await expect(page.locator(`[data-item-id="${data.itemId}"]`)).toHaveAttribute("data-available", "false");
  });
});
