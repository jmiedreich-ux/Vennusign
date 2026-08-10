import { test, expect, findShelfCard, openAs, openMenuBuilderAs, apiBaseUrl, tokens } from "../fixtures";
import { seed } from "../seed";

/**
 * The menu builder, in a browser.
 *
 * Milestone 2's retrospective is the reason this file ships WITH the surface
 * rather than a step later: browser and screenshot checks caught four defects
 * there that unit tests could not see, and three were the most serious in the
 * milestone. The owner made it standing instruction.
 *
 * These also carry forward what the two retired editor specs were protecting —
 * that an edit survives a reload, and that an edit made while a save is in flight
 * is not overwritten by the older save completing late.
 */

test.describe("the builder", () => {
  test("a card opens the builder at its own address, and a refresh stays there", async ({ page }) => {
    const data = await seed({ role: "owner", label: "route" });

    await openAs(page, "owner", "menu");
    const card = await findShelfCard(page, data.menuName);
    await card.getByTestId("open-menu").click();

    await expect(page.getByTestId("menu-builder")).toBeVisible();
    expect(page.url()).toContain(`#/menu/${data.menuId}`);

    // The whole reason the builder has an address: a refresh mid-edit lands back
    // on the same menu rather than on the shelf, and so does the back button.
    await page.reload();
    await expect(page.getByTestId("menu-builder")).toBeVisible();
    await expect(page.getByTestId("builder-menu-name")).toContainText(data.menuName);

    await page.goBack();
    await expect(page.getByTestId("menus-home")).toBeVisible();
  });

  test("the canvas draws the real board, and the breadcrumb goes back", async ({ page }) => {
    const data = await seed({ role: "owner", label: "canvas" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    // Not a thumbnail and not a placeholder: the same engine, the same DOM.
    await expect(page.getByTestId("canvas").getByTestId("board")).toBeVisible();
    await expect(page.getByTestId("canvas")).toContainText(data.itemName);

    // There is no Preview button, because the canvas IS the preview.
    await expect(page.getByRole("button", { name: /^preview$/i })).toHaveCount(0);

    await page.getByTestId("back-to-menus").click();
    await expect(page.getByTestId("menus-home")).toBeVisible();
  });

  test("first open selects the top section and nothing in the inspector (Q116)", async ({ page }) => {
    const data = await seed({ role: "owner", label: "firstopen" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await expect(page.getByTestId("view-one-section")).toHaveAttribute("aria-pressed", "true");
    await expect(page.getByTestId("rail-section").first()).toHaveAttribute("aria-current", "true");
    await expect(page.getByTestId("inspector-empty")).toBeVisible();
  });

  test("clicking an item selects it; clicking the price edits in place (Q118)", async ({ page }) => {
    const data = await seed({ role: "owner", label: "select" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    const row = page.getByTestId("board-item").first();
    await row.locator(".board-item-name").click();

    await expect(page.getByTestId("item-name")).toHaveValue(data.itemName);
    await expect(page.getByTestId("inspector-empty")).toHaveCount(0);
    // The ring is drawn on the row itself, so what is selected is visible on the
    // board and not only in the panel.
    await expect(row).toHaveClass(/is-selected/);

    // In-place editing is the price ONLY.
    await row.locator(".board-item-price").click();
    await expect(page.getByTestId("price-edit")).toBeVisible();
    await page.getByTestId("price-edit").fill("11.5");
    await page.getByTestId("price-edit").press("Enter");

    await expect(page.getByTestId("canvas")).toContainText("11.5");
    await expect(page.getByTestId("item-price")).toHaveValue("11.5");
  });

  test("an edit survives a reload, and reaches no screen on its own", async ({ page }) => {
    // Carried from the retired menu-item-isolated spec: the persistence check,
    // now against the builder's save model.
    const data = await seed({ role: "owner", label: "persist" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    const value = `edited ${data.itemId.slice(0, 8)}`;
    await page.getByTestId("item-description").fill(value);
    await page.getByTestId("item-description").blur();

    await expect(page.getByTestId("canvas")).toContainText(value);
    await page.reload();
    await expect(page.getByTestId("canvas")).toContainText(value);

    // Nothing about typing reaches a screen. That is the whole save model.
    const showing = await page.request.get(`${apiBaseUrl}/api/back-office/content/screens/showing`, {
      headers: { "X-Vennusign-Back-Office-Token": tokens.owner }
    });
    const screens = (await showing.json()) as Array<{ menuId: string | null }>;
    expect(screens.filter(screen => screen.menuId === data.menuId)).toHaveLength(0);
  });

  test("an edit made while a save is in flight is not overwritten by the older save", async ({ page }) => {
    // Carried verbatim in intent from the retired menu-save-race spec. The old
    // editor cleared the pending marker when the OLDER request completed, and the
    // refresh that followed replaced the newer draft with the pre-edit value.
    const data = await seed({ role: "owner", label: "race" });

    let releaseFirst: (() => void) | undefined;
    const firstHeld = new Promise<void>(resolve => { releaseFirst = resolve; });
    let saves = 0;

    await page.route(`**/content/items/${data.itemId}`, async route => {
      if (route.request().method() !== "PUT") return route.fallback();
      saves += 1;
      if (saves === 1) await firstHeld;
      await route.continue();
    });

    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("board-item").first().locator(".board-item-name").click();

    await page.getByTestId("item-description").fill("first");
    await page.getByTestId("item-description").blur();
    await expect.poll(() => saves).toBe(1);

    await page.getByTestId("item-description").fill("second");
    await page.getByTestId("item-description").blur();

    releaseFirst?.();
    await expect.poll(() => saves, { timeout: 10_000 }).toBe(2);

    await page.reload();
    await expect(page.getByTestId("canvas")).toContainText("second");
  });

  test("adding an item: create as new, and picking an existing one jumps (Q112)", async ({ page }) => {
    const data = await seed({ role: "owner", label: "add" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await page.getByTestId("open-add-item").click();
    const name = `Spec Item ${data.itemId.slice(0, 6)}`;
    await page.getByTestId("add-item-input").fill(name);
    await page.getByTestId("add-item-create").click();

    // Born with the typed name, no price, and the name focused so it can be
    // corrected at once (Q113).
    await expect(page.getByTestId("item-name")).toHaveValue(name);
    await expect(page.getByTestId("item-price")).toHaveValue("");
    await expect(page.getByTestId("missing-price-flag")).toBeVisible();

    // Now offer the SAME item again. It must not place a second copy.
    const before = await page.getByTestId("board-item").count();
    await page.getByTestId("open-add-item").click();
    await page.getByTestId("add-item-input").fill(name);
    await page.getByTestId("add-item-result").filter({ hasText: name }).first().click();

    await expect(page.getByTestId("builder-notice")).toContainText("already on this board");
    await expect(page.getByTestId("board-item")).toHaveCount(before);
  });

  test("a section can be added, renamed, and deleted — and its items come back", async ({ page }) => {
    const data = await seed({ role: "owner", label: "sections" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await page.getByTestId("add-section").click();
    await page.getByTestId("new-section-name").fill("Puddings");
    await page.getByTestId("new-section-name").press("Enter");
    await expect(page.getByTestId("rail-section").filter({ hasText: "Puddings" })).toBeVisible();

    await page.getByTestId("section-name").fill("Afters");
    await page.getByTestId("section-name").blur();
    await expect(page.getByTestId("rail-section").filter({ hasText: "Afters" })).toBeVisible();

    // Put an item on it, then delete the section: Q96's "nothing is lost" is what
    // the message has to say, and it has to be true.
    await page.getByTestId("open-add-item").click();
    const name = `Released ${data.itemId.slice(0, 6)}`;
    await page.getByTestId("add-item-input").fill(name);
    await page.getByTestId("add-item-create").click();
    await expect(page.getByTestId("canvas")).toContainText(name);

    await page.getByTestId("delete-section").click();
    await expect(page.getByTestId("builder-notice")).toContainText("back to your library");

    // Still in the library: findable from the add row on another section.
    await page.getByTestId("rail-section").first().click();
    await page.getByTestId("open-add-item").click();
    await page.getByTestId("add-item-input").fill(name);
    await expect(page.getByTestId("add-item-result").filter({ hasText: name })).toBeVisible();
  });

  test("an 86'd item stays on the canvas but leaves the guest board at once", async ({ page }) => {
    const data = await seed({ role: "owner", includeScreen: true, label: "eightysix" });
    await page.request.put(`${apiBaseUrl}/api/back-office/content/screens/${data.screenId}/menu`, {
      headers: { "X-Vennusign-Back-Office-Token": tokens.owner, "Content-Type": "application/json" },
      data: { menuId: data.menuId }
    });
    await openMenuBuilderAs(page, "owner", data.menuId);

    // Published first, so the shelf card has a real board to draw — and so the
    // check that follows is about availability rather than about an empty card.
    await page.getByTestId("publish").click();
    await expect(page.getByTestId("draft-count")).toContainText("Everything is on your screens");

    await page.getByTestId("back-to-menus").click();
    let card = await findShelfCard(page, data.menuName);
    await expect(card.getByTestId("board")).toBeVisible();
    await expect(card).toContainText(data.itemName);

    await card.getByTestId("open-menu").click();
    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    await page.getByTestId("availability-switch").click();

    // On the canvas it stays, struck through: you cannot turn back on what the
    // surface has hidden from you (Q104).
    const row = page.getByTestId("board-item").filter({ hasText: data.itemName });
    await expect(row).toHaveAttribute("data-unavailable", "true");
    await expect(page.getByTestId("availability-panel")).toHaveAttribute("data-off", "true");
    await expect(page.getByTestId("availability-panel")).toContainText("Hidden on every screen");

    // On the guest board it is already gone — with no publish, and with the draft
    // still clean. That is the whole of the availability model, seen from outside.
    await expect(page.getByTestId("draft-count")).toContainText("Everything is on your screens");
    await page.getByTestId("back-to-menus").click();
    card = await findShelfCard(page, data.menuName);
    await expect(card.getByTestId("board")).toBeVisible();
    await expect(card).not.toContainText(data.itemName);
  });

  test("⌘K finds an item on this board and jumps to it (Q121)", async ({ page }) => {
    const data = await seed({ role: "owner", label: "find" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await page.keyboard.press("ControlOrMeta+k");
    await expect(page.getByTestId("find-dialog")).toBeVisible();
    await page.getByTestId("find-input").fill(data.itemName.slice(0, 5));
    await page.getByTestId("find-result").first().click();

    await expect(page.getByTestId("find-dialog")).toHaveCount(0);
    await expect(page.getByTestId("item-name")).toHaveValue(data.itemName);
  });

  test("the publish bar states the queue, publishes it, and then says it is clean", async ({ page }) => {
    const data = await seed({ role: "owner", includeScreen: true, label: "publish" });
    // A publish needs somewhere to go. Assigning the screen is its own deliberate
    // act in this model, and the seed deliberately does not do it for you.
    await page.request.put(`${apiBaseUrl}/api/back-office/content/screens/${data.screenId}/menu`, {
      headers: { "X-Vennusign-Back-Office-Token": tokens.owner, "Content-Type": "application/json" },
      data: { menuId: data.menuId }
    });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    await page.getByTestId("item-description").fill("Ready for the board.");
    await page.getByTestId("item-description").blur();

    // A menu on a screen but never published says exactly that, rather than
    // counting differences against a board that does not exist.
    await expect(page.getByTestId("draft-count")).toContainText("Nothing on your screens yet");
    // The button counts CHANGES in both bar forms — "Publish to N screens" was a
    // slip in the wireframe (Q161).
    await expect(page.getByTestId("publish")).toContainText(/Publish \d+ changes?/);
    await expect(page.getByTestId("publish")).not.toContainText("screens");

    await page.getByTestId("publish").click();

    // Clean state is the home of screen status: no Publish button, chips remain (Q111).
    await expect(page.getByTestId("draft-count")).toContainText("Everything is on your screens");
    await expect(page.getByTestId("publish")).toHaveCount(0);
    await expect(page.getByTestId("publish-bar")).toContainText("Published");
  });

  test("a menu nobody has published offers no discard, because it would do nothing", async ({ page }) => {
    const data = await seed({ role: "owner", label: "nodiscard" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    // Discard returns the menu to what its screens are showing. With nothing
    // published there is nowhere to go back to, so the link is absent rather than
    // present and inert (decision 5).
    await expect(page.getByTestId("draft-count")).toContainText("Nothing on your screens yet");
    await expect(page.getByTestId("discard-draft")).toHaveCount(0);
  });

  test("discarding the draft names the stakes and cannot be done by accident (Q110)", async ({ page }) => {
    const data = await seed({ role: "owner", includeScreen: true, label: "discard" });
    await page.request.put(`${apiBaseUrl}/api/back-office/content/screens/${data.screenId}/menu`, {
      headers: { "X-Vennusign-Back-Office-Token": tokens.owner, "Content-Type": "application/json" },
      data: { menuId: data.menuId }
    });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("publish").click();
    await expect(page.getByTestId("draft-count")).toContainText("Everything is on your screens");

    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    await page.getByTestId("item-description").fill("This goes away.");
    await page.getByTestId("item-description").blur();
    await expect(page.getByTestId("draft-count")).toContainText("not on your screens");

    await page.getByTestId("discard-draft").click();
    const dialog = page.getByTestId("discard-dialog");
    await expect(dialog).toContainText("can't be undone");

    // Escape leaves the queue alone — a destructive act needs a deliberate one.
    await page.keyboard.press("Escape");
    await expect(dialog).toHaveCount(0);
    await expect(page.getByTestId("draft-count")).toContainText("not on your screens");

    await page.getByTestId("discard-draft").click();
    await page.getByTestId("confirm-discard").click();
    await expect(page.getByTestId("draft-count")).toContainText("Everything is on your screens");
  });

  test("undo puts back what the last act changed", async ({ page }) => {
    const data = await seed({ role: "owner", label: "undo" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await page.getByTestId("add-section").click();
    await page.getByTestId("new-section-name").fill("Undo Me");
    await page.getByTestId("new-section-name").press("Enter");
    await expect(page.getByTestId("rail-section").filter({ hasText: "Undo Me" })).toBeVisible();

    await page.getByTestId("undo").click();
    await expect(page.getByTestId("rail-section").filter({ hasText: "Undo Me" })).toHaveCount(0);
  });

  test("the theme picker shows the empty state rather than a look nobody built (Q86)", async ({ page }) => {
    const data = await seed({ role: "owner", label: "theme" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    await page.getByTestId("open-theme-picker").click();

    await expect(page.getByTestId("theme-empty")).toBeVisible();
    await expect(page.getByTestId("theme-picker")).not.toContainText("Coastal");
  });

  test("Play stays visible and says plainly what it cannot do yet (Q102)", async ({ page }) => {
    const data = await seed({ role: "owner", label: "play" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    // Visible, never greyed and never absent — it says why instead.
    await expect(page.getByTestId("play")).toBeVisible();
    await expect(page.getByTestId("play")).toBeEnabled();
    await page.getByTestId("play").click();
    await expect(page.getByTestId("builder-notice")).toContainText(/play|screen/i);
  });

  test("redo puts back what undo took away", async ({ page }) => {
    const data = await seed({ role: "owner", label: "redo" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await page.getByTestId("add-section").click();
    await page.getByTestId("new-section-name").fill("Redo Me");
    await page.getByTestId("new-section-name").press("Enter");
    await expect(page.getByTestId("rail-section").filter({ hasText: "Redo Me" })).toBeVisible();

    await page.getByTestId("undo").click();
    await expect(page.getByTestId("rail-section").filter({ hasText: "Redo Me" })).toHaveCount(0);

    await page.getByTestId("redo").click();
    await expect(page.getByTestId("rail-section").filter({ hasText: "Redo Me" })).toBeVisible();
  });

  test("Viewing as lists the menu's screens, named without a resolution (Q101)", async ({ page }) => {
    const data = await seed({ role: "owner", includeScreen: true, label: "viewing" });
    await page.request.put(`${apiBaseUrl}/api/back-office/content/screens/${data.screenId}/menu`, {
      headers: { "X-Vennusign-Back-Office-Token": tokens.owner, "Content-Type": "application/json" },
      data: { menuId: data.menuId }
    });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await page.getByTestId("viewing-as").click();
    const options = page.getByTestId("viewing-as-option");
    await expect(options.first()).toBeVisible();
    // Screen geometry arrives in milestone 4; naming a resolution now would be a
    // guess dressed as a fact.
    await expect(page.getByTestId("viewing-as-list")).not.toContainText("1920");
  });

  test("an 86'd row on the canvas says when it went off", async ({ page }) => {
    const data = await seed({ role: "owner", label: "note" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    await page.getByTestId("availability-switch").click();

    // "hidden on all screens right now" without a time is half a sentence.
    await expect(page.getByTestId("board-item-note")).toContainText("86'd");
    await expect(page.getByTestId("board-item-note")).toContainText("hidden on all screens right now");
    await expect(page.getByTestId("board-item-note")).toHaveText(/86'd \w{3} \d{1,2}:\d{2}[ap]m/);
  });

  test("Review first lists exactly what will ship, in words (Q111)", async ({ page }) => {
    const data = await seed({ role: "owner", includeScreen: true, label: "review" });
    await page.request.put(`${apiBaseUrl}/api/back-office/content/screens/${data.screenId}/menu`, {
      headers: { "X-Vennusign-Back-Office-Token": tokens.owner, "Content-Type": "application/json" },
      data: { menuId: data.menuId }
    });
    await openMenuBuilderAs(page, "owner", data.menuId);

    // Published first, so the queue below is ONE change rather than the whole
    // menu measured against nothing.
    await page.getByTestId("publish").click();
    await expect(page.getByTestId("draft-count")).toContainText("Everything is on your screens");

    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    await page.getByTestId("item-price").fill("13.5");
    await page.getByTestId("item-price").blur();

    await page.getByTestId("review-first").click();
    await expect(page.getByTestId("review-dialog")).toBeVisible();
    // Named, not identified: a guid in a review list tells nobody anything.
    await expect(page.getByTestId("review-list")).toContainText(data.itemName);
    await expect(page.getByTestId("review-list")).toContainText("price");
    await expect(page.getByTestId("review-list")).not.toContainText(data.itemId);
  });

  test("go back to… offers versions and produces a draft, never a silent publish", async ({ page }) => {
    const data = await seed({ role: "owner", includeScreen: true, label: "goback" });
    await page.request.put(`${apiBaseUrl}/api/back-office/content/screens/${data.screenId}/menu`, {
      headers: { "X-Vennusign-Back-Office-Token": tokens.owner, "Content-Type": "application/json" },
      data: { menuId: data.menuId }
    });
    await openMenuBuilderAs(page, "owner", data.menuId);

    // Two published versions, so there is something to go back TO.
    await page.getByTestId("publish").click();
    await expect(page.getByTestId("draft-count")).toContainText("Everything is on your screens");
    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    await page.getByTestId("item-price").fill("21");
    await page.getByTestId("item-price").blur();
    await page.getByTestId("publish").click();
    await expect(page.getByTestId("draft-count")).toContainText("Everything is on your screens");

    await page.getByTestId("go-back-to").click();
    await expect(page.getByTestId("history-dialog")).toContainText("never publishes on its own");
    await page.getByTestId("go-back-to-version").last().click();

    // A draft against the current screens — the screens have not moved.
    await expect(page.getByTestId("draft-count")).toContainText("not on your screens");
    await expect(page.getByTestId("publish")).toBeVisible();
  });

  test("none of the four banned words appear anywhere in the builder (criterion 5)", async ({ page }) => {
    const data = await seed({ role: "owner", label: "words" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    const visible = (await page.getByTestId("menu-builder").innerText()).toLowerCase();

    for (const word of ["unpublish", "supersede", "restore", "archive"]) {
      expect(visible, `"${word}" is on the builder`).not.toContain(word);
    }
  });
});
