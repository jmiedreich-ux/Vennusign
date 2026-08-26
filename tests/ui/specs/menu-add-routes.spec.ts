import { test, expect, openAs } from "../fixtures";
import { scaleSeed } from "../seed";

/**
 * The Menus home's front door (M6.5, issue #867).
 *
 * The defect these exist for: M6.1 through M6.4 shipped the paste import end to
 * end and verified it against deployed dev, and **nothing in the product could
 * reach it**. All three "Add a menu" affordances — the empty shelf, the dashed
 * tile, the header button at scale — opened a "Start a blank menu" name prompt.
 * The only way in was to type `#/menu/import` into the address bar.
 *
 * Four milestones passed their own acceptance workbooks while that was true,
 * because every workbook started from inside the flow. So each affordance is
 * asserted separately here rather than through one shared helper: they are three
 * code paths, and it was exactly the untested third that nobody noticed.
 */
test.describe("the ways a menu gets in", () => {
  test.beforeEach(({ }, testInfo) => {
    test.skip(testInfo.project.name !== "desktop", "Menus is a desktop surface");
  });

  test("the dashed tile opens the chooser, and paste reaches the import", async ({ page }) => {
    await scaleSeed({ menus: 3, screens: 2 });
    await openAs(page, "scale", "menu");

    await page.getByTestId("shelf-grid").getByTestId("add-a-menu").click();
    const chooser = page.getByTestId("add-menu-dialog");
    await expect(chooser).toBeVisible();
    await expect(chooser).toContainText("Let's get your menu in.");

    await chooser.getByTestId("add-route-paste").click();
    await expect(page).toHaveURL(/#\/menu\/import$/);
    await expect(page.getByLabel("Menu text")).toBeVisible();
  });

  test("the header button at scale opens the same chooser", async ({ page }) => {
    // Q166: past the cutover the tile is gone and Add a menu is a plain button
    // beside search. A different element, a different branch - and the branch
    // that would have stayed broken if this were folded into the test above.
    await scaleSeed({ menus: 9, screens: 4 });
    await openAs(page, "scale", "menu");

    await expect(page.getByTestId("shelf-grid").getByTestId("add-a-menu")).toHaveCount(0);
    await page.getByTestId("add-a-menu").click();
    await page.getByTestId("add-menu-dialog").getByTestId("add-route-paste").click();
    await expect(page).toHaveURL(/#\/menu\/import$/);
    await expect(page.getByLabel("Menu text")).toBeVisible();
  });

  test("an empty shelf draws the routes at page size, with nothing to dismiss", async ({ page }) => {
    // Decision 17: onboarding is this screen's empty state, not a wizard - there
    // is nothing to fall out of and nothing to re-enter. So no dialog, no scrim.
    await page.route("**/api/back-office/menus**", async route => {
      if (route.request().method() !== "GET") return route.fallback();
      await route.fulfill({ status: 200, contentType: "application/json", body: "[]" });
    });
    await openAs(page, "owner", "menu");

    const routes = page.getByTestId("menu-add-routes");
    await expect(routes).toBeVisible();
    await expect(routes).toHaveClass(/add-routes--page/);
    await expect(page.getByTestId("add-menu-dialog")).toHaveCount(0);
    await expect(page.getByTestId("add-menu-scrim")).toHaveCount(0);

    await routes.getByTestId("add-route-paste").click();
    await expect(page).toHaveURL(/#\/menu\/import$/);
  });

  test("only routes that exist are offered", async ({ page }) => {
    // README.md's M1a settles this for POS: "when it is not, there is no trace of
    // it - decision 4". Photo and spreadsheet are not built, so they are absent
    // rather than disabled, and one card must read as deliberate rather than as a
    // three-slot row with two holes in it.
    await scaleSeed({ menus: 3, screens: 2 });
    await openAs(page, "scale", "menu");
    await page.getByTestId("shelf-grid").getByTestId("add-a-menu").click();

    const chooser = page.getByTestId("add-menu-dialog");
    await expect(chooser.getByTestId("add-route-paste")).toBeVisible();
    for (const absent of ["photo", "spreadsheet", "pos"]) {
      await expect(chooser.getByTestId(`add-route-${absent}`)).toHaveCount(0);
    }
    await expect(chooser).not.toContainText(/coming soon/i);

    // Blank is a link below the routes, not a peer card. Drawing it as a peer is
    // how it became the only route anybody could reach.
    await expect(chooser.getByTestId("add-route-blank")).toBeVisible();
    await expect(chooser.getByTestId("add-route-blank")).not.toHaveClass(/add-routes__card/);
  });

  test("blank creates a menu nobody has named, and the builder is where it gets one", async ({ page }) => {
    // The owner removed the name prompt on 2026-08-26: naming a menu before it
    // has anything on it is the wrong order. dbo.Menus.Name is NOT NULL and
    // rejects blank, so the menu is real and carries a placeholder the crumb
    // draws as unnamed rather than as a name somebody chose.
    await scaleSeed({ menus: 3, screens: 2 });
    await openAs(page, "scale", "menu");

    await page.getByTestId("shelf-grid").getByTestId("add-a-menu").click();
    await page.getByTestId("add-route-blank").click();

    await expect(page.getByTestId("menu-builder")).toBeVisible({ timeout: 30_000 });
    const crumb = page.getByTestId("builder-menu-name");
    await expect(crumb).toHaveAttribute("data-unnamed", "true");

    const named = `Named in the builder ${Date.now()}`;
    await page.getByTestId("edit-menu-name").click();
    await page.getByTestId("menu-name-input").fill(named);
    await page.getByTestId("menu-name-input").press("Enter");
    await expect(crumb).toHaveText(named);
    await expect(crumb).toHaveAttribute("data-unnamed", "false");
  });

  test("the chooser closes on Cancel, on Escape and on the scrim", async ({ page }) => {
    await scaleSeed({ menus: 3, screens: 2 });
    await openAs(page, "scale", "menu");
    const tile = page.getByTestId("shelf-grid").getByTestId("add-a-menu");
    const chooser = page.getByTestId("add-menu-dialog");

    await tile.click();
    await chooser.getByRole("button", { name: "Cancel" }).click();
    await expect(chooser).toHaveCount(0);

    await tile.click();
    await page.keyboard.press("Escape");
    await expect(chooser).toHaveCount(0);

    await tile.click();
    await page.getByTestId("add-menu-scrim").click();
    await expect(chooser).toHaveCount(0);
  });

  test("a double-clicked route creates one menu, not two", async ({ page }) => {
    await scaleSeed({ menus: 3, screens: 2 });
    await openAs(page, "scale", "menu");

    let creates = 0;
    await page.route("**/api/back-office/menus", async route => {
      if (route.request().method() === "POST") creates += 1;
      await route.fallback();
    });

    await page.getByTestId("shelf-grid").getByTestId("add-a-menu").click();
    const blank = page.getByTestId("add-route-blank");
    await blank.dblclick();

    await expect(page.getByTestId("menu-builder")).toBeVisible({ timeout: 30_000 });
    expect(creates, "the second click lands on a disabled control").toBe(1);
  });

  test("the import refuses below 900px rather than the chooser hiding the route", async ({ page }) => {
    // Deliberate: hiding paste at a narrow width would be the ghost UI decision 4
    // exists to prevent, and the import screen already refuses well, naming the
    // reason and offering a way back. Better one honest refusal than a route that
    // silently is not there.
    await scaleSeed({ menus: 3, screens: 2 });
    await openAs(page, "scale", "menu");
    await page.getByTestId("shelf-grid").getByTestId("add-a-menu").click();
    await expect(page.getByTestId("add-route-paste")).toBeVisible();

    await page.setViewportSize({ width: 820, height: 900 });
    await page.getByTestId("add-route-paste").click();
    await expect(page.getByTestId("menu-import-narrow")).toBeVisible();
    await expect(page.getByRole("heading", { name: "Importing a menu needs a wider window" })).toBeVisible();
  });
});
