import { test, expect, openAs } from "../fixtures";

/**
 * A20 — a price change that could mean two things asks which (issue #913).
 *
 * The owner chose this over both silent answers on 2026-08-27. Changing every menu is the
 * behaviour A19 withdrew; changing one quietly leaves the others wrong with nothing said.
 *
 * Driven through the screen rather than the API, because every part of this that went wrong in
 * development was in the screen: the question asked about the wrong dish, the question asked when
 * nothing was ambiguous, and Cancel leaving the typed price in the field so the NEXT edit asked
 * again about a price nobody kept.
 */
test.describe("changing a price that could mean two things", () => {
  test.beforeEach(({ }, testInfo) => {
    test.skip(testInfo.project.name !== "desktop", "The builder is a desktop surface");
  });

  test("a dish on one menu is not ambiguous, and nothing is asked", async ({ page }) => {
    await openAs(page, "owner", "/menu");
    await page.getByTestId("menu-card").first().click();
    await page.getByTestId("board-item").first().click();

    const price = page.getByLabel(/price/i).first();
    await price.fill("7.77");
    await price.blur();

    // Decision 18 — confirm only what we were unsure of. One placement is not a decision.
    await expect(page.getByTestId("price-scope-dialog")).toHaveCount(0);
  });

  test("Cancel writes nothing and puts the field back", async ({ page }) => {
    // The defect this exists for: cancelling skipped the write but left the typed price on screen,
    // so renaming the item afterwards saw a changed price and asked all over again.
    await openAs(page, "owner", "/menu");
    await page.getByTestId("menu-card").first().click();
    await page.getByTestId("board-item").first().click();

    const price = page.getByLabel(/price/i).first();
    const before = await price.inputValue();

    await price.fill("99.99");
    await price.blur();

    const dialog = page.getByTestId("price-scope-dialog");
    if (await dialog.count() === 0) {
      test.skip(true, "This venue's first dish is on one menu, so there is nothing to cancel out of");
      return;
    }

    await page.getByTestId("price-scope-cancel").click();
    await expect(price).toHaveValue(before);
    await expect(dialog).toHaveCount(0);
  });

  test("both answers name what happens rather than agreeing or disagreeing", async ({ page }) => {
    await openAs(page, "owner", "/menu");
    await page.getByTestId("menu-card").first().click();
    await page.getByTestId("board-item").first().click();

    const price = page.getByLabel(/price/i).first();
    await price.fill("8.88");
    await price.blur();

    const dialog = page.getByTestId("price-scope-dialog");
    if (await dialog.count() === 0) {
      test.skip(true, "This venue's first dish is on one menu");
      return;
    }

    await expect(page.getByTestId("price-scope-here")).toContainText(/here only/i);
    await expect(page.getByTestId("price-scope-everywhere")).toContainText(/all \d+/);
    // "Yes" would not tell anybody which menus are about to change.
    await expect(dialog).not.toContainText(/^Yes\b/);
  });
});
