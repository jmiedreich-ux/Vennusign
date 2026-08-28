import { test, expect, openAs } from "../fixtures";

/*
 * Review is a step now, not a screen the product jumps past (owner, 2026-08-28).
 *
 * A resolved session used to render the destination immediately, so these specs went straight
 * there. The operator now passes THROUGH the review - which is where the line inventory and
 * "Nothing left to answer" live - and moves on deliberately.
 */
async function onwardToDestination(page: import("@playwright/test").Page) {
  const onward = page.getByTestId("go-to-destination");
  if (await onward.count()) await onward.click();
}


/**
 * The way back into an unfinished import (#904).
 *
 * The defect: an import session was saved for 24 hours and `#/menu/import/{id}` resumed it
 * correctly — and nothing in the product could reach that URL a second time. The shelf only ever
 * linked to a NEW import, and no endpoint listed the open ones. Close the tab and the work sat in
 * the database, paid for, until it expired. The screen said "Saved until Friday 6:47 AM", which
 * stated a fact and withheld the action.
 *
 * This drives it the way an operator hits it — start an import, leave, come back — rather than by
 * seeding a session directly, because "leave and come back" is the step that was broken.
 */
test.describe("an import you did not finish", () => {
  test.beforeEach(({ }, testInfo) => {
    test.skip(testInfo.project.name !== "desktop", "Menus is a desktop surface");
  });

  test("the shelf offers the way back into it, and the way out of it", async ({ page }) => {
    await openAs(page, "owner", "/menu/import");
    await page.getByLabel("Menu text").fill("STARTERS\nGarlic Bread  6.50\nOlives  4.00");
    await page.getByRole("button", { name: /read menu/i }).click();
    await expect(page).toHaveURL(/#\/menu\/import\/[0-9a-f-]{36}$/);
    const sessionId = page.url().split("/").pop()!;

    // Leave, exactly as "Back to menus" does.
    await page.goto("/#/menu");

    const banner = page.getByTestId("open-imports");
    await expect(banner).toBeVisible();
    // Names what is left rather than what was done — that is what decides whether to go back in.
    await expect(page.getByTestId("open-import-detail")).toContainText("items");

    await banner.getByTestId("resume-import").first().click();
    await expect(page).toHaveURL(new RegExp(`#/menu/import/${sessionId}$`));

    // And back out again: an operator told "you have an import in progress" must be able to say
    // "no I do not", or the only way out of that sentence is to wait 24 hours (decision 10).
    await page.goto("/#/menu");
    await page.getByTestId("open-imports").getByTestId("discard-import").first().click();
    await expect(page.getByTestId("shelf-notice")).toContainText("thrown away");
    await expect(page.getByTestId("open-imports")).toHaveCount(0);
  });

  test("a shelf with no unfinished import says nothing about one", async ({ page }) => {
    // Decision 12: name the exception. An empty region that explains it has nothing to show is
    // the noise this rule exists to keep off the page.
    await openAs(page, "publisher", "/menu");
    await expect(page.getByTestId("open-imports")).toHaveCount(0);
  });
});
