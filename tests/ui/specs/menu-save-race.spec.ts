import { test, expect, openAs } from "../fixtures";
import { seed } from "../seed";

/**
 * Regression for the stale-overwrite race in the menu item editor.
 *
 * Editing again while a save is still in flight used to have its pending marker
 * cleared by the *older* request completing. The refresh that followed then replaced
 * the newer draft with the server's pre-edit value, silently discarding the edit.
 *
 * The save request is held open here so the second edit is guaranteed to land while
 * the first is unresolved, which is the window the defect lived in.
 */
test("an edit made while a save is in flight is not overwritten by the older save", async ({ page }) => {
  const data = await seed({ role: "owner", label: "race" });

  let releaseFirstSave: (() => void) | undefined;
  const firstSaveHeld = new Promise<void>(resolve => { releaseFirstSave = resolve; });
  let seenSaves = 0;

  await page.route(`**/items/${data.itemId}`, async route => {
    if (route.request().method() !== "PUT") return route.fallback();
    seenSaves += 1;
    // Hold only the first save open; later ones proceed immediately.
    if (seenSaves === 1) await firstSaveHeld;
    return route.fallback();
  });

  await openAs(page, "owner", "menu");
  await page.getByTestId("menu-picker").selectOption(data.menuId);

  const row = page.locator(`[data-item-id="${data.itemId}"]`);
  const description = row.getByTestId("item-description");

  // First edit, saved by blur. Its request is now held open.
  await description.fill("first edit");
  await description.blur();
  await expect(row).toHaveAttribute("data-save-state", "saving");

  // Second edit lands while the first request is still unresolved.
  await description.fill("second edit");
  await expect(row).toHaveAttribute("data-save-state", "draft");

  releaseFirstSave?.();

  // The newer edit must still be reported unsaved, never marked saved by the older
  // request completing, and must not be replaced by the refresh that follows it.
  await expect(row).toHaveAttribute("data-save-state", "draft");
  await expect(description).toHaveValue("second edit");

  // A save control must remain offered, because the newer edit is genuinely unsaved.
  await expect(row.getByTestId("item-save")).toBeEnabled();

  // The refresh triggered by the older save must not have replaced the newer draft
  // with the server's value. This is the regression: previously the field reverted to
  // "first edit" here and the edit was silently lost.
  await expect(description).toHaveValue("second edit");
  await expect(description).not.toHaveValue("first edit");

  // Exactly one save reached the server: the held first request. The second edit is
  // still unsaved, which is what "draft" above must mean.
  expect(seenSaves, "the newer edit must not have been auto-saved").toBe(1);
});
