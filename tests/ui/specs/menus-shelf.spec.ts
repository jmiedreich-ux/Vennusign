import { test, expect, findShelfCard, openAs } from "../fixtures";
import { scaleSeed, seed } from "../seed";

/**
 * The Menus home shelf, in a browser.
 *
 * Milestone 1's retrospective wrote the rule these exist under: acceptance
 * asserts what the customer sees, not that an API accepted a request. A workbook
 * can pass every check while the product is visibly broken, and has. So these
 * assert what a card actually draws.
 *
 * Serial, and against a venue of their own. The shelf's shape depends on how many
 * menus the venue has, and the default venue accumulates menus from every spec
 * that seeds — so "exactly six" is not a thing that can be true there while the
 * suite runs in parallel.
 */
test.describe.configure({ mode: "serial" });

/**
 * Desktop only, for two reasons that happen to agree.
 *
 * The design authority is explicit that the Menus hi-fis are 1440px desktop and
 * mobile is out of scope — Quick Update is the first mobile candidate, and it is
 * flagged rather than guessed. And running both projects would put two browsers
 * into the same scale venue at once, which is the very thing the venue exists to
 * prevent: the seed clears before it fills, so a second run mid-assertion would
 * empty the shelf under the first.
 */
test.describe("the shelf", () => {
  test.beforeEach(({ }, testInfo) => {
    test.skip(testInfo.project.name !== "desktop", "Menus is a desktop surface");
  });

  test("below the cutover it is the plain shelf: no search, no chips, and the dashed tile", async ({ page }) => {
    // Q163: at six or fewer the shelf is exactly the M1 design. Nothing collapses,
    // nothing is searchable, and Add a menu is the tile rather than a button.
    await scaleSeed({ menus: 4, screens: 4 });
    await openAs(page, "scale", "menu");

    const grid = page.getByTestId("shelf-grid");
    await expect(grid).toHaveAttribute("data-at-scale", "false");
    await expect(page.getByTestId("shelf-scale-controls")).toHaveCount(0);
    await expect(page.getByTestId("shelf-search")).toHaveCount(0);
    await expect(page.getByTestId("shelf-filter")).toHaveCount(0);
    await expect(page.getByTestId("shelf-more")).toHaveCount(0);

    // The dashed tile is in the grid, as its last cell.
    await expect(grid.getByTestId("add-a-menu")).toHaveCount(1);
  });

  test("at scale it compacts, gains search and chips, and keeps every on-screen menu visible", async ({ page }) => {
    // Q176's check: thirteen menus, twenty screens.
    const seeded = await scaleSeed({ menus: 13, screens: 20 });
    await openAs(page, "scale", "menu");

    const grid = page.getByTestId("shelf-grid");
    await expect(grid).toHaveAttribute("data-at-scale", "true");
    await expect(page.getByTestId("shelf-search")).toBeVisible();

    // Q164: none active on load.
    const chips = page.getByTestId("shelf-filter");
    expect(await chips.count()).toBeGreaterThan(0);
    for (const chip of await chips.all()) {
      await expect(chip).toHaveAttribute("aria-pressed", "false");
    }

    // Q166: once compacted, Add a menu is a plain button beside search rather than
    // a tile in the grid.
    await expect(grid.getByTestId("add-a-menu")).toHaveCount(0);
    await expect(page.getByTestId("add-a-menu")).toHaveCount(1);

    // Q165: every menu actually on a screen stays visible; the rest go behind
    // "N more", which is a fact about the shelf rather than a hidden state.
    const onScreens = seeded.seededMenus.filter(menu => menu.screenIds.length > 0 && menu.state !== "put-away");
    for (const menu of onScreens) {
      await expect(
        page.locator(`[data-testid="menu-card"][data-menu-id="${menu.menuId}"]`),
        `${menu.name} is on a screen and must stay on the shelf`
      ).toHaveCount(1);
    }

    await expect(page.getByTestId("shelf-more")).toBeVisible();
    await page.getByTestId("shelf-more").click();
    await expect(page.getByTestId("menu-card")).toHaveCount(
      seeded.seededMenus.filter(menu => menu.state !== "put-away").length
    );
  });

  test("search finds a put-away menu, and the filters narrow to what they name", async ({ page }) => {
    const seeded = await scaleSeed({ menus: 13, screens: 20 });
    await openAs(page, "scale", "menu");

    const away = seeded.seededMenus.find(menu => menu.state === "put-away");
    expect(away, "the scale seed should leave one menu put away").toBeTruthy();

    // Put away, so it is in the strip rather than the grid — and still findable.
    await expect(page.getByTestId("not-in-use")).toContainText(away!.name);

    await page.getByTestId("shelf-search").fill(away!.name);
    // Q163: search covers put-away menus, because a menu you cannot find is a
    // menu the shelf is lying about.
    await expect(page.getByTestId("not-in-use")).toContainText(away!.name);

    await page.getByTestId("shelf-search").fill("");
    await page.locator('[data-testid="shelf-filter"][data-filter="pending"]').click();

    const pending = seeded.seededMenus.filter(menu => menu.state === "pending-changes");
    await expect(page.getByTestId("menu-card")).toHaveCount(pending.length);
    await expect(page.getByTestId("pending-bar")).toHaveCount(pending.length);
  });

  test("a card draws the board its screens are showing, exactly as typed", async ({ page }) => {
    // The milestone's whole promise, asserted where a person would see it: the
    // card is a picture of the TV.
    await scaleSeed({ menus: 4, screens: 4 });
    await openAs(page, "scale", "menu");

    const published = page.locator('[data-testid="menu-card"]').filter({ has: page.getByTestId("board") }).first();
    const board = published.getByTestId("board");
    await expect(board).toBeVisible();

    // Q115/Q190: "MP" is a price, and it reaches the board as typed rather than
    // as a number. This is the assertion a decimal column would have failed.
    await expect(board).toContainText("MP");
    await expect(board).toContainText("8.5");

    // Q135: a card is a picture of the guest's screen, so it carries none of the
    // annotations a preview surface may show.
    await expect(board).toHaveAttribute("data-board-surface", "guest");
    await expect(board).not.toContainText("86'd");
    await expect(board).not.toContainText("OF ");

    // Q98: the engine draws no venue-name strip. If a TV carries one, the theme
    // editor owns it.
    await expect(board).not.toContainText("Scale Check Venue");

    // Every section it draws has something in it: an empty section never renders.
    for (const section of await board.getByTestId("board-section").all()) {
      expect(await section.getByTestId("board-item").count()).toBeGreaterThan(0);
    }
  });

  test("a menu that has never been published says so, rather than drawing a blank board", async ({ page }) => {
    const seeded = await scaleSeed({ menus: 13, screens: 20 });
    await openAs(page, "scale", "menu");

    const never = seeded.seededMenus.find(menu => menu.state === "never-published");
    expect(never).toBeTruthy();

    await page.getByTestId("shelf-search").fill(never!.name);
    const card = page.locator(`[data-testid="menu-card"][data-menu-id="${never!.menuId}"]`);
    await expect(card).toHaveCount(1);

    // No board, because no screen has ever shown it — and no pending bar either,
    // which would be true and useless: everything about it is a change.
    await expect(card.getByTestId("board")).toHaveCount(0);
    await expect(card.getByTestId("pending-bar")).toHaveCount(0);
    await expect(card.getByTestId("card-status")).toContainText("Never published");
  });

  test("criterion 6 — take off the screens shows what replaces the menu before confirming", async ({ page }) => {
    const seeded = await scaleSeed({ menus: 4, screens: 4 });
    await openAs(page, "scale", "menu");

    const onScreens = seeded.seededMenus.find(menu => menu.screenIds.length > 0);
    const card = page.locator(`[data-testid="menu-card"][data-menu-id="${onScreens!.menuId}"]`);

    await card.getByTestId("card-actions").click();

    // Q195 and build-decision 16: six items, Put away directly after Duplicate,
    // Take off the screens alone below the last divider.
    // Plain buttons in a disclosure, not role="menu": the roles were there without
    // the arrow-key navigation they promise, which tells a screen-reader user to
    // expect behaviour that does not exist.
    await expect(card.getByTestId("card-menu").locator("button")).toHaveText([
      "Open",
      "Quick update",
      "Go back to…",
      "Duplicate",
      "Put away",
      "Take off the screens"
    ]);

    await card.getByTestId("take-off-screens").click();

    const dialog = page.getByTestId("take-off-dialog");
    await expect(dialog).toBeVisible();
    // It is never a bare action: what people will see instead is shown, with a
    // picture of it, before anything is confirmed.
    await expect(dialog).toContainText("What people will see instead");
    await expect(dialog.getByTestId("venue-fallback")).toBeVisible();
    await expect(dialog).toContainText("It stays on your Menus home and keeps its history.");
  });

  /**
   * Independent review finding: every close path left focus on the document.
   *
   * The trigger lives inside the <details> menu, which is unmounted before the
   * dialog appears — so Escape, Cancel and the scrim all closed it with focus
   * nowhere, and the next Tab restarted from the top of the page instead of
   * continuing from the card. The previous take-off test asserted the dialog's
   * copy and presence, which is why it did not notice.
   */
  test("the take-off dialog can be worked by keyboard, and gives the card its focus back", async ({ page }) => {
    const seeded = await scaleSeed({ menus: 4, screens: 4 });
    await openAs(page, "scale", "menu");

    const onScreens = seeded.seededMenus.find(menu => menu.screenIds.length > 0);
    const card = page.locator(`[data-testid="menu-card"][data-menu-id="${onScreens!.menuId}"]`);
    const summary = card.getByTestId("card-actions");

    // Opened by keyboard alone, not by click.
    await summary.focus();
    await page.keyboard.press("Enter");
    await card.getByTestId("take-off-screens").focus();
    await page.keyboard.press("Enter");

    const dialog = page.getByTestId("take-off-dialog");
    await expect(dialog).toBeVisible();

    // Focus moves into the dialog rather than being left behind it.
    await expect(dialog.locator(":focus")).toHaveCount(1);

    await page.keyboard.press("Escape");
    await expect(dialog).toHaveCount(0);

    // Back on the card that opened it — so the next Tab continues from here
    // rather than restarting at the top of the page.
    await expect(summary).toBeFocused();

    // And the same for Cancel, which is the path a mouse user takes to the same
    // outcome.
    await page.keyboard.press("Enter");
    await card.getByTestId("take-off-screens").focus();
    await page.keyboard.press("Enter");
    await expect(dialog).toBeVisible();

    await dialog.getByRole("button", { name: "Cancel" }).click();
    await expect(dialog).toHaveCount(0);
    await expect(summary).toBeFocused();
  });

  test("criterion 5 — the banned words appear nowhere on the shelf", async ({ page }) => {
    await scaleSeed({ menus: 4, screens: 4 });
    await openAs(page, "scale", "menu");
    await page.getByTestId("menus-home").waitFor();

    // Decisions 9, 10 and 11, asserted against the rendered page rather than the
    // source: nobody ever sees these words.
    const shelf = page.getByTestId("menus-home");
    for (const banned of ["unpublish", "supersede", "restore", "archive"]) {
      await expect(shelf, `the shelf says "${banned}"`).not.toContainText(new RegExp(banned, "i"));
    }

    // And with the card menu open, where the destructive wording lives.
    await page.getByTestId("card-actions").first().click();
    for (const banned of ["unpublish", "supersede", "restore", "archive"]) {
      await expect(shelf, `the card menu says "${banned}"`).not.toContainText(new RegExp(banned, "i"));
    }
  });
});

test("a menu holding changes can still be opened by its board", async ({ page }) => {
  // The amber strip is drawn over the bottom of the card. It used to swallow the
  // click, so the one menu you most want to open - the one holding changes - was
  // the one you could not open by clicking its middle. On a narrow viewport the
  // centre of the board IS the strip, which is where this turned up.
  const data = await seed({ role: "owner", label: "pending" });
  await openAs(page, "owner", "menu");

  const card = await findShelfCard(page, data.menuName);
  await expect(card.getByTestId("open-menu")).toBeVisible();
  await card.getByTestId("open-menu").click();
  await expect(page.getByTestId("menu-builder")).toBeVisible();
});

