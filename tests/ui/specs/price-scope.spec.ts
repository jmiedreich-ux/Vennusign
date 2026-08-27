import { test, expect, openAddItem, openMenuBuilderAs } from "../fixtures";
import { seed } from "../seed";

/**
 * A20 — a price change that could mean two things asks which (#913).
 *
 * The owner chose this over both silent answers on 2026-08-27. Changing every menu is the
 * behaviour A19 withdrew; changing one quietly leaves the others wrong with nothing said.
 *
 * Driven through the screen, because every part of this that went wrong in development was in the
 * screen: the question asked about the wrong dish, the question asked when nothing was ambiguous,
 * and Cancel leaving the typed price in the field so the NEXT edit asked again about a price
 * nobody kept.
 *
 * The first version of this file was written without seeding and clicked whatever card happened to
 * be first on a shared venue. It could not have passed. This one makes its own two menus and puts
 * one dish on both, which is the only state in which the question is supposed to appear at all.
 */
test.describe("changing a price that could mean two things", () => {
  test.beforeEach(({ }, testInfo) => {
    test.skip(testInfo.project.name !== "desktop", "The builder is a desktop surface");
  });

  test("a dish on one menu is not ambiguous, and nothing is asked", async ({ page }) => {
    // Decision 18 — confirm only what we were unsure of. One placement is not a decision, and a
    // dialog on every price edit is what Q5's follow-up was told to avoid.
    const only = await seed({ role: "owner", label: "scope-one" });
    await openMenuBuilderAs(page, "owner", only.menuId);

    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    await page.getByTestId("item-price").fill("7.77");
    await page.getByTestId("item-price").blur();

    await expect(page.getByTestId("price-scope-dialog")).toHaveCount(0);
  });

  test("a dish on two menus asks, names both, and Cancel puts the field back", async ({ page }) => {
    const first = await seed({ role: "owner", label: "scope-a" });
    const second = await seed({ role: "owner", label: "scope-b" });

    // Put the FIRST menu's dish onto the second menu, through the add row a person uses. Q112:
    // picking an existing item places that item rather than a copy, which is what makes one dish
    // sit on two menus and is the whole precondition for the question.
    await openMenuBuilderAs(page, "owner", second.menuId);
    await openAddItem(page);
    await page.getByTestId("add-item-input").fill(first.itemName);
    await page.getByTestId("add-item-result").filter({ hasText: first.itemName }).first().click();

    const price = page.getByTestId("item-price");
    const before = await price.inputValue();

    await price.fill("41.41");
    await price.blur();

    const dialog = page.getByTestId("price-scope-dialog");
    await expect(dialog).toBeVisible();
    await expect(dialog).toContainText("2 menus");
    // Both answers name what happens. "Yes" would not say which menus are about to change.
    await expect(page.getByTestId("price-scope-here")).toContainText(/here only/i);
    await expect(page.getByTestId("price-scope-everywhere")).toContainText(/all 2/);

    // The defect this half exists for: cancelling skipped the write and left the typed price on
    // screen, so the next edit of any field saw a changed price and asked all over again.
    await page.getByTestId("price-scope-cancel").click();
    await expect(dialog).toHaveCount(0);
    await expect(price).toHaveValue(before);
  });
});
