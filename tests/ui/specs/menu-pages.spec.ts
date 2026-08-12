import { test, expect, openMenuBuilderAs, apiBaseUrl, tokens } from "../fixtures";
import { seed } from "../seed";

test.describe("menu pages", () => {
  test.skip(({ browserName }) => browserName !== "chromium", "M3-A is desktop-only");

  test("capabilities independently remove only their guarded controls", async ({ page }) => {
    const data = await seed({ role: "owner", label: "capability-off", itemsPerSection: 25, screenState: "has-not-taken-this-yet" });
    await page.addInitScript(() => {
      const stored = window.sessionStorage.getItem("vennusign.test.menu-capabilities");
      window.__VENNUSIGN_BACK_OFFICE_CONFIGURATION__ = {
        menuCapabilityOverrides: stored ? JSON.parse(stored) : { "page-management": false }
      };
    });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await expect(page.getByTestId("add-page")).toHaveCount(0);
    await expect(page.getByTestId("page-actions")).toHaveCount(0);
    await expect(page.getByTestId("assignment-pill")).toBeVisible();
    await expect(page.getByTestId("capacity-banner")).toBeVisible();

    await page.evaluate(() => {
      window.sessionStorage.setItem("vennusign.test.menu-capabilities", JSON.stringify({ "screen-assignment": false }));
    });
    await page.reload();
    await expect(page.getByTestId("add-page")).toBeVisible();
    await expect(page.getByTestId("assignment-pill")).toHaveCount(0);
    await expect(page.getByTestId("capacity-banner")).toBeVisible();

    await page.evaluate(() => {
      window.sessionStorage.setItem("vennusign.test.menu-capabilities", JSON.stringify({ capacity: false }));
    });
    await page.reload();
    await expect(page.getByTestId("add-page")).toBeVisible();
    await expect(page.getByTestId("assignment-pill")).toBeVisible();
    await expect(page.getByTestId("capacity-banner")).toHaveCount(0);
  });

  test("page history follows the selected page, records section changes, and stays read-only", async ({ page }) => {
    const data = await seed({ role: "owner", label: "page-history", pageCount: 2, sectionCount: 2 });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await expect(page.getByTestId("page-history")).toContainText(data.pages[0].name);
    await expect(page.getByTestId("page-history-entry").first()).toContainText("Section added");

    const firstRow = page.getByTestId("section-row").first();
    await firstRow.getByRole("button", { name: /^Rename / }).click();
    await page.getByTestId("section-rename-input").fill("Lunch favourites");
    await page.getByTestId("section-rename-input").press("Enter");
    await expect(page.getByTestId("page-history-entry").first()).toContainText("renamed to Lunch favourites");
    await expect(page.getByTestId("page-history").getByRole("button")).toHaveCount(1);

    await page.getByTestId("page-tab").nth(1).click();
    await expect(page.getByTestId("page-history")).toContainText(data.pages[1].name);
    await expect(page.getByTestId("page-history-entry").first()).toContainText("Section added");
    await expect(page.getByTestId("page-history")).not.toContainText("Lunch favourites");

    await page.getByTestId("menu-history-link").click();
    await expect(page.getByTestId("history-dialog")).toBeVisible();
  });

  test("history capability removes only page history", async ({ page }) => {
    const data = await seed({ role: "owner", label: "history-off" });
    await page.addInitScript(() => {
      window.__VENNUSIGN_BACK_OFFICE_CONFIGURATION__ = { menuCapabilityOverrides: { history: false } };
    });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await expect(page.getByTestId("page-history")).toHaveCount(0);
    await expect(page.getByTestId("rail-section")).toBeVisible();
    await expect(page.getByTestId("assignment-pill")).toBeVisible();
  });

  test("page history failure is honest and retryable without blocking section work", async ({ page }) => {
    const data = await seed({ role: "owner", label: "history-retry" });
    let allowRetry = false;
    await page.route(`**/api/back-office/content/menus/${data.menuId}/pages/*/history`, async route => {
      if (!allowRetry) return route.fulfill({ status: 503, body: "temporarily unavailable" });
      return route.fallback();
    });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await expect(page.getByTestId("page-history")).toContainText("History couldn't load");
    await expect(page.getByTestId("rail-section")).toBeVisible();
    allowRetry = true;
    await page.getByTestId("page-history").getByRole("button", { name: "Try again" }).click();
    await expect(page.getByTestId("page-history-entry").first()).toContainText("Section added");
  });

  test("a late history response cannot replace the newly selected page's history", async ({ page }) => {
    const data = await seed({ role: "owner", label: "history-race", pageCount: 2, sectionCount: 2 });
    let releaseFirst: (() => void) | undefined;
    const firstReleased = new Promise<void>(resolve => { releaseFirst = resolve; });
    let delayed = false;
    await page.route(`**/api/back-office/content/menus/${data.menuId}/pages/${data.pages[0].pageId}/history`, async route => {
      if (!delayed) {
        delayed = true;
        await firstReleased;
      }
      await route.fallback();
    });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("page-tab").nth(1).click();
    await expect(page.getByTestId("page-history")).toContainText(data.pages[1].name);
    await expect(page.getByTestId("page-history-entry").first()).toContainText(data.sections[1].name);
    releaseFirst!();
    await page.waitForTimeout(300);
    await expect(page.getByTestId("page-history-entry").first()).toContainText(data.sections[1].name);
    await expect(page.getByTestId("page-history")).not.toContainText(data.sections[0].name);
  });

  test("tabs switch the page content and blank add is abandoned", async ({ page }) => {
    const data = await seed({ role: "owner", label: "pages", pageCount: 2, sectionCount: 2, itemsPerSection: 1 });
    await openMenuBuilderAs(page, "owner", data.menuId);

    const tabs = page.getByTestId("page-tab");
    await expect(tabs).toHaveCount(2);
    await expect(page.getByTestId("canvas")).toContainText(data.items[0].name);
    await tabs.nth(1).click();
    await expect(page.getByTestId("canvas")).toContainText(data.items[1].name);
    await expect(page.getByTestId("canvas")).not.toContainText(data.items[0].name);
    await page.getByTestId("viewing-chip").filter({ hasText: "Whole page" }).click();
    await expect(page.getByTestId("canvas")).toContainText(data.items[1].name);
    await expect(page.getByTestId("canvas")).not.toContainText(data.items[0].name);

    await page.getByTestId("add-page").click();
    const pageNameInput = page.getByTestId("page-name-input");
    await expect(pageNameInput).toHaveAttribute("placeholder", "Page name");
    await expect(pageNameInput).not.toHaveCSS("font-family", /Playfair Display/);
    const railBox = await page.getByTestId("page-rail").boundingBox();
    const inputBox = await pageNameInput.boundingBox();
    const workspaceBox = await page.locator(".builder__columns").boundingBox();
    const firstTabBox = await tabs.first().boundingBox();
    expect(railBox).not.toBeNull();
    expect(inputBox).not.toBeNull();
    expect(workspaceBox).not.toBeNull();
    expect(firstTabBox).not.toBeNull();
    expect(inputBox!.x + inputBox!.width).toBeLessThanOrEqual(railBox!.x + railBox!.width + 1);
    expect(firstTabBox!.x).toBeCloseTo(workspaceBox!.x, 0);
    expect(firstTabBox!.y + firstTabBox!.height).toBeCloseTo(workspaceBox!.y, 0);
    await pageNameInput.blur();
    await expect(tabs).toHaveCount(2);
  });

  test("duplicate creates an unassigned page and the only page cannot be deleted", async ({ page }) => {
    const data = await seed({ role: "owner", label: "duplicate-page" });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("page-actions").click();
    await expect(page.getByTestId("page-menu").getByRole("button", { name: "Delete" })).toBeDisabled();
    await page.getByTestId("page-menu").getByRole("button", { name: "Duplicate" }).click();
    await expect(page.getByTestId("page-tab")).toHaveCount(2);
    await page.getByTestId("page-tab").last().click();
    await expect(page.getByTestId("canvas")).toContainText(data.itemName);
    await expect(page.getByTestId("page-assignment-count")).toHaveCount(0);
  });

  test("page rename, escape, real-mouse reorder and empty delete preserve the visible lifecycle", async ({ page }) => {
    const data = await seed({ role: "owner", label: "page-lifecycle", pageCount: 2, sectionCount: 2 });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("page-actions").click();
    await page.getByTestId("page-menu").getByRole("button", { name: "Rename", exact: true }).click();
    const rename = page.getByTestId("page-rename-input");
    await rename.fill("Abandoned name");
    await rename.press("Escape");
    await expect(page.getByTestId("page-tab").first()).not.toHaveText("Abandoned name");

    await page.getByTestId("page-actions").click();
    await page.getByTestId("page-menu").getByRole("button", { name: "Rename", exact: true }).click();
    await page.getByTestId("page-rename-input").fill("Service page");
    await page.getByTestId("page-rename-input").press("Enter");
    await expect(page.getByTestId("page-tab").first()).toHaveText("Service page");
    await page.reload();
    await expect(page.getByTestId("page-tab").first()).toHaveText("Service page");

    const wraps = page.getByTestId("page-tab-wrap");
    const secondName = await page.getByTestId("page-tab").nth(1).textContent();
    const source = await wraps.nth(1).boundingBox();
    const target = await wraps.nth(0).boundingBox();
    expect(source).not.toBeNull();
    expect(target).not.toBeNull();
    await page.mouse.move(source!.x + source!.width / 2, source!.y + source!.height / 2);
    await page.mouse.down();
    await page.mouse.move(target!.x + target!.width / 2, target!.y + target!.height / 2, { steps: 24 });
    await page.mouse.up();
    await expect(page.getByTestId("page-tab").first()).toHaveText(secondName!);
    await page.reload();
    await expect(page.getByTestId("page-tab").first()).toHaveText(secondName!);

    await page.getByTestId("add-page").click();
    await page.getByTestId("page-name-input").fill("Empty page");
    await page.getByTestId("page-name-input").press("Enter");
    await page.getByTestId("page-tab").getByText("Empty page", { exact: true }).click();
    await expect(page.getByTestId("page-history-empty")).toBeVisible();
    await page.getByTestId("page-actions").click();
    await page.getByTestId("page-menu").getByRole("button", { name: "Delete" }).click();
    await expect(page.getByTestId("delete-page-dialog")).toContainText("This page is empty");
    await expect(page.getByTestId("delete-page-destination")).toHaveCount(0);
    await page.getByTestId("delete-page-dialog").getByRole("button", { name: "Delete page" }).click();
    await expect(page.getByTestId("page-tab")).toHaveCount(2);
  });

  test("six sections keep the sixth behind More and selecting it redraws the board", async ({ page }) => {
    const data = await seed({ role: "owner", label: "section-chips", sectionCount: 6 });
    await openMenuBuilderAs(page, "owner", data.menuId);
    const chipRow = page.getByTestId("section-chips");
    await expect(chipRow.getByRole("button")).toHaveCount(6);
    await expect(chipRow.getByText("More")).toBeVisible();
    await expect(chipRow).toHaveCSS("flex-wrap", "nowrap");
    await chipRow.getByText("More").click();
    await page.getByTestId("section-chips").getByRole("button", { name: data.sections[5].name }).click();
    await expect(page.getByTestId("canvas")).toContainText(data.items[5].name);
  });

  test("the section rail returns from Whole page to the selected section", async ({ page }) => {
    const data = await seed({ role: "owner", label: "rail-from-whole", sectionCount: 2, itemsPerSection: 1 });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("viewing-chip").filter({ hasText: "Whole page" }).click();
    await expect(page.getByTestId("canvas")).toContainText(data.items[0].name);
    await expect(page.getByTestId("canvas")).toContainText(data.items[1].name);
    await page.getByTestId("rail-section").nth(1).click();
    await expect(page.getByTestId("canvas")).toContainText(data.items[1].name);
    await expect(page.getByTestId("canvas")).not.toContainText(data.items[0].name);
    await expect(page.getByTestId("viewing-chip").nth(2)).toHaveAttribute("aria-pressed", "true");
  });

  test("populated page delete names the screen, offers a destination, and Cancel preserves the page", async ({ page }) => {
    const data = await seed({ role: "owner", label: "populated-page-delete", pageCount: 2, sectionCount: 2, screenState: "has-not-taken-this-yet" });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("page-actions").click();
    await page.getByTestId("page-menu").getByRole("button", { name: "Delete" }).click();
    const dialog = page.getByTestId("delete-page-dialog");
    await expect(dialog).toContainText("populated-page-delete screen");
    await expect(page.getByTestId("delete-page-destination")).toHaveValue(data.pages[1].pageId);
    await dialog.getByRole("button", { name: "Cancel" }).click();
    await expect(page.getByTestId("page-tab")).toHaveCount(2);

    await page.getByTestId("page-actions").click();
    await page.getByTestId("page-menu").getByRole("button", { name: "Delete" }).click();
    await dialog.getByRole("button", { name: "Delete page" }).click();
    await expect(page.getByTestId("page-tab")).toHaveCount(1);
    await page.getByTestId("viewing-chip").filter({ hasText: "Whole page" }).click();
    await expect(page.getByTestId("canvas")).toContainText(data.items[0].name);
  });

  test("populated page can be deleted with its sections instead of moving them", async ({ page }) => {
    const data = await seed({ role: "owner", label: "discard-page-sections", pageCount: 2, sectionCount: 2 });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("page-actions").click();
    await page.getByTestId("page-menu").getByRole("button", { name: "Delete" }).click();
    const dialog = page.getByTestId("delete-page-dialog");
    await expect(dialog).toContainText("Library items will be kept");
    await dialog.getByRole("radio", { name: /Delete the page and its sections/ }).check();
    await expect(page.getByTestId("delete-page-destination")).toHaveCount(0);
    await dialog.getByRole("button", { name: "Delete page" }).click();
    await expect(page.getByTestId("page-tab")).toHaveCount(1);
    await page.getByTestId("viewing-chip").filter({ hasText: "Whole page" }).click();
    await expect(page.getByTestId("canvas")).not.toContainText(data.items[0].name);
    await expect(page.getByTestId("canvas")).toContainText(data.items[1].name);
  });

  test("duplicate-to-move conflict keeps the delete choice open and offers deletion instead", async ({ page }) => {
    const data = await seed({ role: "owner", label: "duplicate-delete-conflict" });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("page-actions").click();
    await page.getByTestId("page-menu").getByRole("button", { name: "Duplicate" }).click();
    await page.getByTestId("page-tab").first().click();
    await page.getByTestId("page-actions").click();
    await page.getByTestId("page-menu").getByRole("button", { name: "Delete" }).click();
    const dialog = page.getByTestId("delete-page-dialog");
    await dialog.getByRole("button", { name: "Delete page" }).click();
    await expect(dialog).toBeVisible();
    await expect(page.getByTestId("builder-error")).toContainText("share an item");
    await dialog.getByRole("radio", { name: /Delete the page and its sections/ }).check();
    await dialog.getByRole("button", { name: "Delete page" }).click();
    await expect(dialog).toHaveCount(0);
    await expect(page.getByTestId("page-tab")).toHaveCount(1);
  });

  test("capacity exposes its computed limit and every dropped item", async ({ page }) => {
    const data = await seed({ role: "owner", label: "overflow", itemsPerSection: 25, screenState: "has-not-taken-this-yet" });
    await openMenuBuilderAs(page, "owner", data.menuId);
    const banner = page.getByTestId("capacity-banner");
    await expect(banner).toHaveAttribute("data-capacity", "overflowing");
    await expect(banner).toHaveAttribute("data-capacity-limit", "16");
    const dropped = await banner.getAttribute("data-dropped-items");
    expect(dropped?.split("|")).toEqual(data.items.slice(16).map(item => item.name));
    await page.getByTestId("check-fit").click();
    const results = page.getByTestId("fit-results");
    await expect(results).toContainText("overflow screen");
    await expect(results).toContainText(data.items[16].name);
    await results.getByRole("button", { name: "Done" }).click();
    await expect(results).toHaveCount(0);
  });

  test("capacity evaluates the whole page while one section is being viewed", async ({ page }) => {
    await seed({ role: "owner", label: "whole-page-fit", sectionCount: 2, itemsPerSection: 10, screenState: "has-not-taken-this-yet" }).then(async data => {
      await openMenuBuilderAs(page, "owner", data.menuId);
      await expect(page.getByTestId("viewing-chip").filter({ hasNotText: "Whole page" }).first()).toHaveAttribute("aria-pressed", "true");
      await expect(page.getByTestId("capacity-banner")).toHaveAttribute("data-capacity", "overflowing");
      await expect(page.getByTestId("capacity-banner")).toHaveAttribute("data-capacity-limit", "14");
    });
  });

  test("capacity follows the assigned page and the registered screen geometry", async ({ page }) => {
    const data = await seed({
      role: "owner",
      label: "portrait-capacity",
      itemsPerSection: 90,
      screenState: "has-not-taken-this-yet",
      screenWidthPixels: 2160,
      screenHeightPixels: 3840
    });
    await openMenuBuilderAs(page, "owner", data.menuId);

    await expect(page.getByTestId("capacity-banner")).toHaveAttribute("data-capacity-limit", "108");
  });

  test("capacity never reports against an unassigned page", async ({ page }) => {
    const data = await seed({ role: "owner", label: "unassigned-capacity", pageCount: 2, sectionCount: 2, itemsPerSection: 25, screenState: "has-not-taken-this-yet" });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await expect(page.getByTestId("capacity-banner")).toBeVisible();
    await page.getByTestId("page-tab").nth(1).click();
    await expect(page.getByTestId("capacity-banner")).toHaveCount(0);
  });

  test("a screen is assigned to the page the operator selected", async ({ page }) => {
    const data = await seed({ role: "owner", label: "page-assignment", pageCount: 2, sectionCount: 2, includeScreen: true });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("page-tab").nth(1).click();
    await page.getByTestId("assignment-pill").click();
    const row = page.getByTestId("screen-assignments").getByTestId("screen-row").filter({ hasText: "page-assignment screen" });
    await row.getByRole("button", { name: "Add a page" }).click();
    await row.getByTestId("add-screen-page-menu").getByRole("button", { name: data.pages[1].name }).click();
    await page.getByTestId("screen-assignments").getByRole("button", { name: "Save changes and return" }).click();
    await expect(page.getByTestId("page-assignment-count")).toHaveText("1 screen");

    const assignments = await page.request.get(`${apiBaseUrl}/api/back-office/content/assignments`, {
      headers: { "X-Vennusign-Back-Office-Token": tokens.owner }
    });
    expect(assignments.ok()).toBeTruthy();
    expect((await assignments.json()).find((assignment: { screenId: string }) => assignment.screenId === data.screenId)?.pageId).toBe(data.pages[1].pageId);
  });

  test("the connected Screen Assignments view stages all-page rotation, removal, replacement and Cancel", async ({ page }) => {
    const data = await seed({ role: "owner", label: "connected-assignments", pageCount: 3, sectionCount: 3, screenState: "has-not-taken-this-yet" });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("assignment-pill").click();
    const view = page.getByTestId("screen-assignments");
    await expect(view.getByRole("heading", { name: "Which screens show this menu" })).toBeVisible();
    await expect(view).toContainText("Pages showing, in order");
    await expect(view.getByTestId("assignments-back")).toBeVisible();
    await expect(view.getByTestId("assignment-save-state")).toHaveText("No unsaved changes");
    const row = view.getByTestId("screen-row").filter({ hasText: "connected-assignments screen" });
    await row.getByRole("button", { name: "Add a page" }).click();
    await row.getByTestId("add-screen-page-menu").getByRole("button", { name: data.pages[1].name }).click();
    await expect(view.getByTestId("assignment-choice")).toContainText(data.pages[0].name);
    await view.getByRole("button", { name: "Rotate together" }).click();
    await expect(row).toContainText(data.pages[1].name);
    await expect(view.getByTestId("assignment-save-state")).toHaveText("1 unsaved change");
    await expect(view.getByTestId("assignment-save-state")).toHaveText("1 unsaved change");
    await view.getByTestId("assignments-back").click();
    let response = await page.request.get(`${apiBaseUrl}/api/back-office/content/assignments`, { headers: { "X-Vennusign-Back-Office-Token": tokens.owner } });
    expect((await response.json()).filter((assignment: { screenId: string }) => assignment.screenId === data.screenId)).toHaveLength(1);

    await page.getByTestId("assignment-pill").click();
    const reopened = page.getByTestId("screen-assignments");
    const reopenedRow = reopened.getByTestId("screen-row").filter({ hasText: "connected-assignments screen" });
    await reopenedRow.getByRole("button", { name: "Add a page" }).click();
    await reopenedRow.getByTestId("add-screen-page-menu").getByRole("button", { name: data.pages[1].name }).click();
    await reopened.getByRole("button", { name: "Rotate together" }).click();
    await reopened.getByRole("button", { name: "Save changes and return" }).click();
    await expect(reopened).toHaveCount(0);
    response = await page.request.get(`${apiBaseUrl}/api/back-office/content/assignments`, { headers: { "X-Vennusign-Back-Office-Token": tokens.owner } });
    expect((await response.json()).filter((assignment: { screenId: string }) => assignment.screenId === data.screenId)).toHaveLength(2);

    await page.getByTestId("assignment-pill").click();
    const removeView = page.getByTestId("screen-assignments");
    await removeView.getByRole("button", { name: `Remove ${data.pages[1].name} from connected-assignments screen` }).click();
    await removeView.getByRole("button", { name: "Save changes and return" }).click();
    await expect(removeView).toHaveCount(0);
    response = await page.request.get(`${apiBaseUrl}/api/back-office/content/assignments`, { headers: { "X-Vennusign-Back-Office-Token": tokens.owner } });
    expect((await response.json()).filter((assignment: { screenId: string }) => assignment.screenId === data.screenId)).toHaveLength(1);

    await page.getByTestId("assignment-pill").click();
    const replaceView = page.getByTestId("screen-assignments");
    const replaceRow = replaceView.getByTestId("screen-row").filter({ hasText: "connected-assignments screen" });
    await replaceRow.getByRole("button", { name: "Add a page" }).click();
    await replaceRow.getByTestId("add-screen-page-menu").getByRole("button", { name: data.pages[2].name }).click();
    await replaceView.getByRole("button", { name: "Replace" }).click();
    await replaceView.getByRole("button", { name: "Save changes and return" }).click();
    await expect(replaceView).toHaveCount(0);
    response = await page.request.get(`${apiBaseUrl}/api/back-office/content/assignments`, { headers: { "X-Vennusign-Back-Office-Token": tokens.owner } });
    const final = (await response.json()).filter((assignment: { screenId: string }) => assignment.screenId === data.screenId);
    expect(final).toHaveLength(1);
    expect(final[0].pageId).toBe(data.pages[2].pageId);
  });

  test("a refused connected assignment Save changes no page and keeps the staged recovery", async ({ page }) => {
    const data = await seed({ role: "owner", label: "connected-atomic", pageCount: 2, sectionCount: 2, screenState: "has-not-taken-this-yet" });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("assignment-pill").click();
    const view = page.getByTestId("screen-assignments");
    const row = view.getByTestId("screen-row").filter({ hasText: "connected-atomic screen" });
    await row.getByRole("button", { name: "Add a page" }).click();
    await row.getByTestId("add-screen-page-menu").getByRole("button", { name: data.pages[1].name }).click();
    await view.getByRole("button", { name: "Rotate together" }).click();
    await page.route(`**/menus/${data.menuId}/screens`, async route => {
      const body = route.request().postDataJSON() as { changes: Array<{ screenId: string; pageId: string; mode: string }> };
      await route.continue({ postData: JSON.stringify({ changes: [...body.changes, { screenId: "00000000-0000-0000-0000-000000000099", pageId: data.pages[1].pageId, mode: "replace" }] }) });
    });
    await view.getByRole("button", { name: "Save changes and return" }).click();
    await expect(view).toBeVisible();
    await expect(row).toContainText(data.pages[1].name);
    await expect(page.getByTestId("builder-error")).toContainText("Nothing was changed");
    const response = await page.request.get(`${apiBaseUrl}/api/back-office/content/assignments`, { headers: { "X-Vennusign-Back-Office-Token": tokens.owner } });
    const assignments = await response.json() as Array<{ screenId: string; pageId: string }>;
    expect(assignments).toContainEqual(expect.objectContaining({ screenId: data.screenId, pageId: data.pages[0].pageId }));
    expect(assignments).not.toContainEqual(expect.objectContaining({ screenId: data.screenId, pageId: data.pages[1].pageId }));
  });

  test("the connected view names an occupied page from another menu before replace or rotation", async ({ page }) => {
    const occupied = await seed({ role: "owner", label: "cross-menu-owner", screenState: "has-not-taken-this-yet" });
    const candidate = await seed({ role: "owner", label: "cross-menu-candidate" });
    await openMenuBuilderAs(page, "owner", candidate.menuId);
    await page.getByTestId("assignment-pill").click();
    const view = page.getByTestId("screen-assignments");
    const row = view.locator(`[data-testid="screen-row"][data-screen-id="${occupied.screenId}"]`);
    await row.getByRole("button", { name: "Add a page" }).click();
    await row.getByTestId("add-screen-page-menu").getByRole("button", { name: candidate.pages[0].name }).click();
    await expect(view.getByTestId("assignment-choice")).toContainText(`${occupied.menuName} · ${occupied.pages[0].name}`);
    await view.getByTestId("assignment-choice").getByRole("button", { name: "Back", exact: true }).click();
    await view.getByTestId("assignments-back").click();
  });

  test("a put-away refusal preserves staged connected assignments until Cancel", async ({ page }) => {
    const occupied = await seed({ role: "owner", label: "assignment-refusal-screen", screenState: "has-not-taken-this-yet" });
    const candidate = await seed({ role: "owner", label: "assignment-refusal-menu" });
    await openMenuBuilderAs(page, "owner", candidate.menuId);
    await page.getByTestId("assignment-pill").click();
    const view = page.getByTestId("screen-assignments");
    const row = view.locator(`[data-testid="screen-row"][data-screen-id="${occupied.screenId}"]`);
    await row.getByRole("button", { name: "Add a page" }).click();
    await row.getByTestId("add-screen-page-menu").getByRole("button", { name: candidate.pages[0].name }).click();
    await view.getByRole("button", { name: "Replace" }).click();
    const putAway = await page.request.put(`${apiBaseUrl}/api/back-office/content/menus/${candidate.menuId}/put-away`, {
      headers: { "X-Vennusign-Back-Office-Token": tokens.owner },
      data: { isPutAway: true }
    });
    expect(putAway.ok()).toBeTruthy();
    await view.getByRole("button", { name: "Save changes and return" }).click();
    await expect(view).toBeVisible();
    await expect(row).toContainText(candidate.pages[0].name);
    await expect(page.getByTestId("builder-error")).toContainText("put away");
    await view.getByTestId("assignments-back").click();
    await expect(view).toHaveCount(0);
  });

});
