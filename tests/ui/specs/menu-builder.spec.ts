import { test, expect, findShelfCard, openAs, openMenuBuilderAs, apiBaseUrl, tokens } from "../fixtures";
import { backdateAvailability, seed } from "../seed";

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

    // Put an item on it so the section has a canvas heading to rename over. An
    // empty section correctly shows only the add affordance (Q96).
    await page.getByTestId("open-add-item").click();
    const name = `Released ${data.itemId.slice(0, 6)}`;
    await page.getByTestId("add-item-input").fill(name);
    await page.getByTestId("add-item-create").click();
    await expect(page.getByTestId("canvas")).toContainText(name);

    await page.getByTestId("canvas").locator(".board-section-heading").click();
    await page.getByTestId("heading-edit").fill("Afters");
    await page.getByTestId("heading-edit").press("Enter");
    await expect(page.getByTestId("rail-section").filter({ hasText: "Afters" })).toBeVisible();

    // Delete the section: Q96's "nothing is lost" is what the message has to say,
    // and it has to be true.
    const sectionRow = page.getByTestId("rail-section").filter({ hasText: "Afters" }).locator("..");
    await sectionRow.getByTestId("delete-section").click();
    await expect(page.getByTestId("section-name")).toHaveCount(0);
    // Deleting a section asks first now — the irreversible act gets the guard the
    // reversible one always had.
    await page.getByTestId("confirm-delete-section").click();
    await expect(page.getByTestId("builder-notice")).toContainText("back to your library");

    // The deleted section was last, so the previous surviving section becomes the
    // canvas rather than leaving the builder pointed at an id that no longer exists.
    await expect(page.getByTestId("rail-section").first()).toHaveAttribute("aria-current", "true");
    await expect(page.getByTestId("canvas")).toContainText(data.itemName);

    // Still in the library: findable from the add row on another section.
    await page.getByTestId("open-add-item").click();
    await page.getByTestId("add-item-input").fill(name);
    await expect(page.getByTestId("add-item-result").filter({ hasText: name })).toBeVisible();
  });

  test("an 86'd item stays on the canvas but leaves the guest board at once", async ({ page }) => {
    const data = await seed({ role: "owner", includeScreen: true, label: "eightysix" });
    await page.request.put(`${apiBaseUrl}/api/back-office/content/screens/${data.screenId}/menu`, {
      headers: { "X-Vennusign-Back-Office-Token": tokens.owner, "Content-Type": "application/json" },
      data: { menuId: data.menuId, pageId: data.pages![0].pageId }
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
    // Available is the normal state: a plain inspector control, not a green
    // status panel that implies every edit here reaches screens immediately.
    await expect(page.getByTestId("availability-switch")).toBeVisible();
    await expect(page.getByTestId("availability-panel")).toHaveCount(0);
    await page.getByTestId("availability-switch").click();

    // On the canvas it stays, struck through: you cannot turn back on what the
    // surface has hidden from you (Q104).
    const row = page.getByTestId("board-item").filter({ hasText: data.itemName });
    await expect(row).toHaveAttribute("data-unavailable", "true");
    await expect(page.getByTestId("availability-panel")).toHaveAttribute("data-off", "true");
    await expect(page.getByTestId("availability-panel")).toContainText("Hidden on every screen");
    await expect(page.getByTestId("availability-panel")).toHaveCSS("background-color", "rgb(253, 234, 234)");

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
      data: { menuId: data.menuId, pageId: data.pages![0].pageId }
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
      data: { menuId: data.menuId, pageId: data.pages![0].pageId }
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
      data: { menuId: data.menuId, pageId: data.pages![0].pageId }
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
      data: { menuId: data.menuId, pageId: data.pages![0].pageId }
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
      data: { menuId: data.menuId, pageId: data.pages![0].pageId }
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

  test("a dialog takes focus, keeps it, and gives it back (impeccable critique)", async ({ page }) => {
    const data = await seed({ role: "owner", label: "focus" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    await page.getByTestId("item-price").fill("15");
    await page.getByTestId("item-price").blur();
    await page.getByTestId("review-first").click();

    // Focus lands inside, on the heading — a screen reader hears what this dialog
    // is before it hears the first thing it can do.
    const dialog = page.getByTestId("review-dialog");
    await expect(dialog).toBeVisible();
    expect(await dialog.evaluate(node => node.contains(document.activeElement))).toBe(true);

    // Tab cannot escape to the Publish button behind the scrim. That was the most
    // likely accidental keyboard action on the whole surface.
    for (let press = 0; press < 8; press += 1) await page.keyboard.press("Tab");
    expect(await dialog.evaluate(node => node.contains(document.activeElement))).toBe(true);

    // The regions behind the scrim are inert, so nothing back there is reachable.
    await expect(page.getByTestId("publish-bar")).toHaveAttribute("inert", "");

    await page.keyboard.press("Escape");
    await expect(dialog).toHaveCount(0);
    await expect(page.getByTestId("publish-bar")).not.toHaveAttribute("inert", "");
    await expect(page.getByTestId("review-first")).toBeFocused();
  });

  test("opening the add row does not push Publish off the screen", async ({ page }) => {
    const data = await seed({ role: "owner", label: "layout" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    await page.getByTestId("item-price").fill("16");
    await page.getByTestId("item-price").blur();
    await expect(page.getByTestId("publish")).toBeInViewport();

    await page.getByTestId("open-add-item").click();
    await page.getByTestId("add-item-input").fill("a");
    // The dropdown overlays; it does not reflow the page out from under the
    // primary action at exactly the moment somebody is adding items.
    await expect(page.getByTestId("publish")).toBeInViewport();
    await expect(page.getByTestId("draft-count")).toBeInViewport();
  });

  test("hitting the menu ceiling says so, in the server's own words", async ({ page }) => {
    await openAs(page, "owner", "menu");
    await page.getByTestId("shelf-headline").waitFor();

    // Refuse the create the way the server does at the ceiling, so the assertion is
    // about the surface rather than about how full the venue happens to be.
    await page.route("**/api/back-office/menus", async route => {
      if (route.request().method() !== "POST") return route.fallback();
      await route.fulfill({
        status: 409,
        contentType: "application/problem+json",
        body: JSON.stringify({
          detail: "That would be 62 menus, and this venue is set up for 50. Put one away first, or ask us to raise the limit."
        })
      });
    });

    await page.getByTestId("add-a-menu").first().click();
    await page.getByTestId("start-blank").click();
    await page.getByTestId("new-menu-name").fill("One too many");
    await page.getByTestId("create-menu").click();

    // Said where the person is looking, in the words the server chose, with the
    // dialog still open and the typed name still in it.
    const refusal = page.getByTestId("create-menu-error");
    await expect(refusal).toContainText("set up for 50");
    await expect(refusal).toContainText("Put one away first");
    await expect(page.getByTestId("name-menu-dialog")).toBeVisible();
    await expect(page.getByTestId("new-menu-name")).toHaveValue("One too many");
  });

  test("deleting a section asks first, and names what comes back", async ({ page }) => {
    const data = await seed({ role: "owner", label: "delconfirm" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await page.getByTestId("delete-section").click();
    const dialog = page.getByTestId("delete-section-dialog");
    await expect(dialog).toContainText("goes back to your library");
    await expect(dialog).toContainText("can't be undone");

    // Escape keeps it — the irreversible act now gets the guard the reversible
    // one always had.
    await page.keyboard.press("Escape");
    await expect(dialog).toHaveCount(0);
    await expect(page.getByTestId("rail-section")).toHaveCount(1);

    // If the first section goes, the next surviving section becomes the canvas.
    // It is empty, so Q96's add affordance is the honest board state.
    await page.getByTestId("add-section").click();
    await page.getByTestId("new-section-name").fill("Next section");
    await page.getByTestId("new-section-name").press("Enter");
    const firstSection = page.getByTestId("rail-section").first().locator("..");
    await firstSection.getByTestId("delete-section").click();
    await page.getByTestId("confirm-delete-section").click();
    await expect(page.getByTestId("builder-notice")).toContainText("back to your library");
    await expect(page.getByTestId("rail-section").filter({ hasText: "Next section" })).toHaveAttribute(
      "aria-current",
      "true"
    );
    await expect(page.getByTestId("open-add-item")).toBeVisible();
  });

  test("Review first shows the values, and leads into the publish", async ({ page }) => {
    const data = await seed({ role: "owner", includeScreen: true, label: "values" });
    await page.request.put(`${apiBaseUrl}/api/back-office/content/screens/${data.screenId}/menu`, {
      headers: { "X-Vennusign-Back-Office-Token": tokens.owner, "Content-Type": "application/json" },
      data: { menuId: data.menuId, pageId: data.pages![0].pageId }
    });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("publish").click();
    await expect(page.getByTestId("draft-count")).toContainText("Everything is on your screens");

    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    await page.getByTestId("item-price").fill("14.00");
    await page.getByTestId("item-price").blur();

    await page.getByTestId("review-first").click();
    // The values, not the word "changed".
    await expect(page.getByTestId("review-list")).toContainText("14.00");
    await expect(page.getByTestId("review-list")).toContainText("→");

    await page.getByTestId("publish-from-review").click();
    await expect(page.getByTestId("draft-count")).toContainText("Everything is on your screens");
  });

  test("the 86 note is legible, not a smudge", async ({ page }) => {
    const data = await seed({ role: "owner", label: "note" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    await page.getByTestId("availability-switch").click();

    const note = page.getByTestId("board-item-note");
    await expect(note).toContainText("hidden on all screens right now");

    // It is drawn outside the dimmed content, at a size the board's scale cannot
    // shrink away. Measured 1.51:1 at ~5px before this.
    const measured = await note.evaluate(node => {
      const box = node.getBoundingClientRect();
      let opacity = 1;
      for (let element: Element | null = node; element; element = element.parentElement) {
        opacity *= Number(getComputedStyle(element).opacity);
      }
      return { height: box.height, opacity };
    });
    expect(measured.opacity).toBeGreaterThan(0.95);
    expect(measured.height).toBeGreaterThan(9);
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

/**
 * The independent review of PR #691, answered in the browser.
 *
 * Every finding it raised was filed with an executed failure attached, so every
 * answer here drives the same sequence rather than describing it. Each was checked
 * against the code as it was before the fix, not only against the fix.
 */
test.describe("what the independent review found", () => {
  const owned = { "X-Vennusign-Back-Office-Token": tokens.owner };

  test("a save that fails is retried on its own, and Publish waits for it (Q197)", async ({ page }) => {
    const data = await seed({ role: "owner", label: "retry" });

    // The first attempt fails outright; anything after it goes through. The defect
    // was that there WAS nothing after it — one request, then silence, forever.
    let attempts = 0;
    await page.route(`**/content/items/${data.itemId}`, async route => {
      if (route.request().method() !== "PUT") return route.fallback();
      attempts += 1;
      if (attempts === 1) {
        return route.fulfill({ status: 500, contentType: "application/json", body: "{}" });
      }
      return route.continue();
    });

    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    await page.getByTestId("item-description").fill("retried into place");
    await page.getByTestId("item-description").blur();

    // Amber, and Publish shut while the queue is unconfirmed.
    await expect(page.getByTestId("save-failed")).toBeVisible();

    // Nobody touches anything. The retry happens by itself.
    await expect.poll(() => attempts, { timeout: 20_000 }).toBeGreaterThan(1);
    await expect(page.getByTestId("save-failed")).toHaveCount(0);
    await expect(page.getByTestId("canvas")).toContainText("retried into place");
    await expect(page.getByTestId("publish")).toBeEnabled();
  });

  test("an expired sign-in holds the change and sends it after signing back in (Q199)", async ({ page }) => {
    const data = await seed({ role: "owner", label: "signin" });

    // 401 until the moment the session is decided to be back.
    let expired = true;
    let saves = 0;
    await page.route(`**/content/items/${data.itemId}`, async route => {
      if (route.request().method() !== "PUT") return route.fallback();
      saves += 1;
      if (expired) {
        return route.fulfill({ status: 401, contentType: "application/json", body: "{}" });
      }
      return route.continue();
    });

    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    await page.getByTestId("item-description").fill("held through the expiry");
    await page.getByTestId("item-description").blur();

    // A prompt over the page — not a terminal error, and not a claim of retrying,
    // which is a promise an expired session would never let the screen keep.
    await expect(page.getByTestId("sign-back-in-dialog")).toBeVisible();
    await expect(page.getByTestId("save-failed")).toHaveCount(0);

    const before = saves;
    expired = false;
    await page.getByTestId("sign-back-in-token").fill(tokens.owner);
    await page.getByTestId("sign-back-in-submit").click();

    // It sends by itself. Nobody retypes the description.
    await expect(page.getByTestId("sign-back-in-dialog")).toHaveCount(0);
    await expect.poll(() => saves, { timeout: 20_000 }).toBeGreaterThan(before);
    await expect(page.getByTestId("canvas")).toContainText("held through the expiry");

    await page.reload();
    await expect(page.getByTestId("canvas")).toContainText("held through the expiry");
  });

  test("Undo refuses rather than erasing somebody else's later edit", async ({ page }) => {
    const data = await seed({ role: "owner", label: "staleundo" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    // Editor A, here, changes the description.
    await page.getByTestId("board-item").first().locator(".board-item-name").click();
    await page.getByTestId("item-description").fill("editor A");
    await page.getByTestId("item-description").blur();
    await expect(page.getByTestId("canvas")).toContainText("editor A");

    // Editor B, elsewhere, edits the same item afterwards — all three values.
    const theirs = await page.request.put(`${apiBaseUrl}/api/back-office/content/items/${data.itemId}`, {
      headers: owned,
      data: { name: "Editor B name", description: "editor B later", price: "99" }
    });
    expect(theirs.ok()).toBeTruthy();

    // Editor A presses Undo. Before the guard this restored A's whole snapshot and
    // all three of B's values vanished, with nothing said to either of them.
    await page.keyboard.press("Control+z");
    await expect(page.getByTestId("builder-error")).toContainText(/changed since/i);

    const after = await page.request.get(
      `${apiBaseUrl}/api/back-office/content/items?query=${encodeURIComponent("Editor B name")}&take=5`,
      { headers: owned }
    );
    const found = (await after.json()) as Array<{ itemId: string; name: string }>;
    expect(found.some(hit => hit.itemId === data.itemId)).toBeTruthy();
  });

  test("two 86'd items each carry their own time, not the first one's", async ({ page }) => {
    const data = await seed({ role: "owner", label: "two86" });

    const placed = await page.request.post(
      `${apiBaseUrl}/api/back-office/content/menus/${data.menuId}/sections/${data.sectionId}/items`,
      { headers: owned, data: { name: `Second ${data.itemName}` } }
    );
    expect(placed.ok()).toBeTruthy();
    const second = (await placed.json()) as { itemId: string };

    for (const itemId of [data.itemId, second.itemId]) {
      const off = await page.request.put(
        `${apiBaseUrl}/api/back-office/content/items/${itemId}/availability`,
        { headers: owned, data: { isAvailable: false } }
      );
      expect(off.ok()).toBeTruthy();
    }

    /*
     * The times have to actually differ, or this spec cannot tell the fix from the
     * defect: two items 86'd in the same second produce identical notes, and a
     * single board-level note handed to both rows would satisfy it. Ninety minutes
     * is enough to change the rendered hour whatever the venue's timezone.
     */
    await backdateAvailability(data.itemId, 90);

    await openMenuBuilderAs(page, "owner", data.menuId);
    await expect(page.getByTestId("board-item-note")).toHaveCount(2);

    const rows = await page.getByTestId("board-item").evaluateAll(nodes =>
      nodes.map(node => ({
        itemId: (node as HTMLElement).dataset.itemId,
        note: node.querySelector('[data-testid="board-item-note"]')?.textContent?.trim() ?? null
      }))
    );

    const noted = rows.filter(row => row.note);
    expect(noted).toHaveLength(2);
    expect(noted.map(row => row.itemId).sort()).toEqual([data.itemId, second.itemId].sort());

    // The point of the whole finding: each row states its OWN time.
    expect(noted[0].note).not.toBe(noted[1].note);
  });

  test("a section is renamed by clicking its heading on the canvas (Q96)", async ({ page }) => {
    const data = await seed({ role: "owner", label: "heading" });
    await openMenuBuilderAs(page, "owner", data.menuId);

    // The heading was inert: clicking it left focus on BODY and opened nothing.
    await page.getByTestId("canvas").locator(".board-section-heading").first().click();
    const editor = page.getByTestId("heading-edit");
    await expect(editor).toBeFocused();

    await editor.fill("Renamed On The Board");
    await editor.press("Enter");

    await expect(page.getByTestId("canvas")).toContainText("Renamed On The Board");
    await expect(page.getByTestId("rail-section").first()).toContainText("Renamed On The Board");

    // A draft change on the server, not a label on this screen.
    await page.reload();
    await expect(page.getByTestId("canvas")).toContainText("Renamed On The Board");
  });

  test("the bulk drawer places many, stays open, and retargets as sections change (Q124)", async ({ page }, testInfo) => {
    test.skip(testInfo.project.name === "mobile", "Menus mobile interactions are out of scope (Q158, owner reaffirmed).");
    const data = await seed({ role: "owner", label: "drawer" });

    // Two library items on no board, so the drawer has something to offer.
    for (const name of ["Alpha", "Beta"]) {
      const made = await page.request.post(
        `${apiBaseUrl}/api/back-office/content/menus/${data.menuId}/sections/${data.sectionId}/items`,
        { headers: owned, data: { name: `${name} ${data.itemName}` } }
      );
      expect(made.ok()).toBeTruthy();
      const item = (await made.json()) as { itemId: string };
      const removed = await page.request.delete(
        `${apiBaseUrl}/api/back-office/content/menus/${data.menuId}/items/${item.itemId}`,
        { headers: owned }
      );
      expect(removed.ok()).toBeTruthy();
    }

    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("open-add-item").click();
    await page.getByTestId("open-add-many").click();
    await expect(page.getByTestId("add-many-drawer")).toBeVisible();

    await page.getByTestId("add-many-search").fill(data.itemName);
    await expect(page.getByTestId("add-many-pick").first()).toBeVisible();

    // A second section, added while the drawer is open — which is only possible
    // because the drawer is not modal. Q124 requires exactly that.
    await page.getByTestId("add-section").click();
    await page.getByTestId("new-section-name").fill("Second Section");
    await page.getByTestId("new-section-name").press("Enter");
    await expect(page.getByTestId("rail-section")).toHaveCount(2);

    await expect(page.getByTestId("add-many-drawer")).toBeVisible();
    await expect(page.getByTestId("add-many-place")).toContainText("Second Section");

    // The button retargets as you move sections.
    await page.getByTestId("rail-section").first().click();
    await expect(page.getByTestId("add-many-place")).toContainText(data.sectionName);

    let picked = 0;
    for (const box of await page.getByTestId("add-many-pick").all()) {
      if (await box.isDisabled()) continue;
      await box.check();
      picked += 1;
      if (picked === 2) break;
    }
    expect(picked).toBe(2);

    await expect(page.getByTestId("add-many-place")).toContainText("Place 2 in");
    await page.getByTestId("add-many-place").click();

    // Stays open, selection cleared, and says how many landed (Q124).
    await expect(page.getByTestId("add-many-drawer")).toBeVisible();
    await expect(page.getByTestId("add-many-placed")).toContainText("2 placed");
    await expect(page.getByTestId("add-many-place")).toContainText("Place 0 in");

    // Escape closes it.
    await page.getByTestId("add-many-search").press("Escape");
    await expect(page.getByTestId("add-many-drawer")).toHaveCount(0);
  });

  test("an item is dragged to a new place on its own section (Q103)", async ({ page }, testInfo) => {
    test.skip(testInfo.project.name === "mobile", "Menus mobile interactions are out of scope (Q158, owner reaffirmed).");
    const data = await seed({ role: "owner", label: "dragitem" });

    for (const name of ["Beta", "Gamma"]) {
      const made = await page.request.post(
        `${apiBaseUrl}/api/back-office/content/menus/${data.menuId}/sections/${data.sectionId}/items`,
        { headers: owned, data: { name: `${name} ${data.itemName}` } }
      );
      expect(made.ok()).toBeTruthy();
    }

    await openMenuBuilderAs(page, "owner", data.menuId);
    const rows = page.getByTestId("board-item");
    await expect(rows).toHaveCount(3);

    const before = await rows.allInnerTexts();

    /*
     * The pill was drawn on hover and on selection all along, exactly as Q103 asks.
     * It simply dragged nothing: no handler, and `reorderMenuItems` imported and
     * never called anywhere in the builder.
     */
    const source = await rows.first().boundingBox();
    const target = await rows.nth(2).boundingBox();
    const handle = await rows.first().getByTestId("item-drag-handle").boundingBox();
    expect(source).not.toBeNull();
    expect(target).not.toBeNull();
    expect(handle).not.toBeNull();

    // A human does not complete a drag inside one synthetic event turn. Move the
    // actual browser pointer slowly enough for layout/observer work to happen
    // between positions; that is the path the former dragTo check missed.
    // Start on the visible handle itself. Starting in the row's text would not
    // reproduce what the owner actually grabbed.
    await page.mouse.move(handle!.x + handle!.width / 2, handle!.y + handle!.height / 2);
    await page.mouse.down();
    await page.mouse.move(target!.x + target!.width / 2, target!.y + target!.height / 2, { steps: 24 });
    const indicator = page.locator("[data-drop-edge]");
    await expect(indicator).toHaveCount(1);
    expect(
      await indicator.evaluate(element => Number.parseFloat(getComputedStyle(element, "::before").height))
    ).toBeGreaterThan(3);
    await page.waitForTimeout(300);
    await page.mouse.up();
    await expect.poll(async () => (await rows.allInnerTexts())[0], { timeout: 15_000 }).not.toBe(before[0]);

    // A draft change on the server, not a rearrangement of this screen.
    await page.reload();
    const after = await page.getByTestId("board-item").allInnerTexts();
    expect(after[0]).not.toBe(before[0]);
  });
});
