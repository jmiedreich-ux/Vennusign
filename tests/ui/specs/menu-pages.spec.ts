import { test, expect, openMenuBuilderAs, apiBaseUrl, tokens } from "../fixtures";
import { seed } from "../seed";

test.describe("menu pages", () => {
  test.skip(({ browserName }) => browserName !== "chromium", "M3-A is desktop-only");

  test("tabs switch the page content and blank add is abandoned", async ({ page }) => {
    const data = await seed({ role: "owner", label: "pages", pageCount: 2, sectionCount: 2, itemsPerSection: 1 });
    await openMenuBuilderAs(page, "owner", data.menuId);

    const tabs = page.getByTestId("page-tab");
    await expect(tabs).toHaveCount(2);
    await expect(page.getByTestId("canvas")).toContainText(data.items[0].name);
    await tabs.nth(1).click();
    await expect(page.getByTestId("canvas")).toContainText(data.items[1].name);
    await expect(page.getByTestId("canvas")).not.toContainText(data.items[0].name);
    await page.getByTestId("view-whole-board").click();
    await expect(page.getByTestId("canvas")).toContainText(data.items[1].name);
    await expect(page.getByTestId("canvas")).not.toContainText(data.items[0].name);

    await page.getByTestId("add-page").click();
    await page.getByTestId("page-name-input").blur();
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
    await page.getByRole("button", { name: "Rename" }).click();
    const rename = page.getByTestId("page-rename-input");
    await rename.fill("Abandoned name");
    await rename.press("Escape");
    await expect(page.getByTestId("page-tab").first()).not.toHaveText("Abandoned name");

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
    await expect(page.getByTestId("section-chips").getByRole("button")).toHaveCount(6);
    await page.getByTestId("section-chips").getByText("More").click();
    await page.getByTestId("section-chips").getByRole("button", { name: data.sections[5].name }).click();
    await expect(page.getByTestId("canvas")).toContainText(data.items[5].name);
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
    await page.getByTestId("view-whole-board").click();
    await expect(page.getByTestId("canvas")).toContainText(data.items[0].name);
  });

  test("capacity exposes its computed limit and every dropped item", async ({ page }) => {
    const data = await seed({ role: "owner", label: "overflow", itemsPerSection: 25, screenState: "has-not-taken-this-yet" });
    await openMenuBuilderAs(page, "owner", data.menuId);
    const banner = page.getByTestId("capacity-banner");
    await expect(banner).toHaveAttribute("data-capacity", "overflow");
    await expect(banner).toHaveAttribute("data-capacity-limit", "16");
    const dropped = await banner.getAttribute("data-dropped-items");
    expect(dropped?.split("|")).toEqual(data.items.slice(16).map(item => item.name));
  });

  test("capacity evaluates the whole page while one section is being viewed", async ({ page }) => {
    await seed({ role: "owner", label: "whole-page-fit", sectionCount: 2, itemsPerSection: 10, screenState: "has-not-taken-this-yet" }).then(async data => {
      await openMenuBuilderAs(page, "owner", data.menuId);
      await expect(page.getByTestId("view-one-section")).toHaveAttribute("aria-pressed", "true");
      await expect(page.getByTestId("capacity-banner")).toHaveAttribute("data-capacity", "overflow");
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
    await page.getByTestId("manage-page-screens").click();
    await page.getByTestId("screen-assignments-view").getByText("page-assignment screen").locator("..").getByRole("button", { name: "Assign" }).click();
    await page.getByTestId("screen-assignments-view").getByRole("button", { name: "Save" }).click();
    await expect(page.getByTestId("page-assignment-count")).toHaveText("1 screen");

    const assignments = await page.request.get(`${apiBaseUrl}/api/back-office/content/assignments`, {
      headers: { "X-Vennusign-Back-Office-Token": tokens.owner }
    });
    expect(assignments.ok()).toBeTruthy();
    expect((await assignments.json()).find((assignment: { screenId: string }) => assignment.screenId === data.screenId)?.pageId).toBe(data.pages[1].pageId);
  });

  test("an occupied screen offers rotation or named replacement and saves only on Save", async ({ page }) => {
    const data = await seed({ role: "owner", label: "rotation-choice", pageCount: 3, sectionCount: 3, screenState: "has-not-taken-this-yet" });
    await openMenuBuilderAs(page, "owner", data.menuId);
    await page.getByTestId("page-tab").nth(1).click();
    await page.getByTestId("manage-page-screens").click();
    const row = page.getByTestId("screen-assignments-view").getByText("rotation-choice screen").locator("..");
    await row.getByRole("button", { name: "Choose…" }).click();
    await expect(page.getByTestId("assignment-choice")).toContainText(data.pages[0].name);
    await page.getByRole("button", { name: "Rotate both" }).click();
    let response = await page.request.get(`${apiBaseUrl}/api/back-office/content/assignments`, { headers: { "X-Vennusign-Back-Office-Token": tokens.owner } });
    expect((await response.json()).filter((assignment: { screenId: string }) => assignment.screenId === data.screenId)).toHaveLength(1);
    await page.getByTestId("screen-assignments-view").getByRole("button", { name: "Cancel" }).click();
    response = await page.request.get(`${apiBaseUrl}/api/back-office/content/assignments`, { headers: { "X-Vennusign-Back-Office-Token": tokens.owner } });
    expect((await response.json()).filter((assignment: { screenId: string }) => assignment.screenId === data.screenId)).toHaveLength(1);
    await page.getByTestId("manage-page-screens").click();
    await page.getByTestId("screen-assignments-view").getByText("rotation-choice screen").locator("..").getByRole("button", { name: /^Choose/ }).click();
    await page.getByRole("button", { name: "Rotate both" }).click();
    await page.getByTestId("screen-assignments-view").getByRole("button", { name: "Save" }).click();
    response = await page.request.get(`${apiBaseUrl}/api/back-office/content/assignments`, { headers: { "X-Vennusign-Back-Office-Token": tokens.owner } });
    expect((await response.json()).filter((assignment: { screenId: string }) => assignment.screenId === data.screenId)).toHaveLength(2);

    await page.getByTestId("manage-page-screens").click();
    await page.getByTestId("screen-assignments-view").getByTestId("remove-page-screen").click();
    response = await page.request.get(`${apiBaseUrl}/api/back-office/content/assignments`, { headers: { "X-Vennusign-Back-Office-Token": tokens.owner } });
    expect((await response.json()).filter((assignment: { screenId: string }) => assignment.screenId === data.screenId)).toHaveLength(2);
    const [removalResponse] = await Promise.all([
      page.waitForResponse(candidate => candidate.request().method() === "DELETE" && candidate.url().includes(`/screens/${data.screenId}/menus/`)),
      page.getByTestId("screen-assignments-view").getByRole("button", { name: "Save" }).click()
    ]);
    expect(removalResponse.status()).toBe(204);
    response = await page.request.get(`${apiBaseUrl}/api/back-office/content/assignments`, { headers: { "X-Vennusign-Back-Office-Token": tokens.owner } });
    const afterRemoval = (await response.json()).filter((assignment: { screenId: string }) => assignment.screenId === data.screenId);
    expect(afterRemoval).toHaveLength(1);
    expect(afterRemoval[0].pageId).toBe(data.pages[0].pageId);

    await page.getByTestId("manage-page-screens").click();
    await page.getByTestId("screen-assignments-view").getByText("rotation-choice screen").locator("..").getByRole("button", { name: /^Choose/ }).click();
    await page.getByRole("button", { name: "Rotate both" }).click();
    await page.getByTestId("screen-assignments-view").getByRole("button", { name: "Save" }).click();

    await page.getByTestId("page-tab").nth(2).click();
    await page.getByTestId("manage-page-screens").click();
    await page.getByTestId("screen-assignments-view").getByText("rotation-choice screen").locator("..").getByRole("button", { name: "Choose…" }).click();
    await page.getByRole("button", { name: "Replace" }).click();
    await page.getByTestId("screen-assignments-view").getByRole("button", { name: "Save" }).click();
    response = await page.request.get(`${apiBaseUrl}/api/back-office/content/assignments`, { headers: { "X-Vennusign-Back-Office-Token": tokens.owner } });
    const final = (await response.json()).filter((assignment: { screenId: string }) => assignment.screenId === data.screenId);
    expect(final).toHaveLength(1);
    expect(final[0].pageId).toBe(data.pages[2].pageId);
  });

  test("cross-menu assignment names the page already occupying the screen", async ({ page }) => {
    const occupied = await seed({ role: "owner", label: "cross-menu-owner", screenState: "has-not-taken-this-yet" });
    const candidate = await seed({ role: "owner", label: "cross-menu-candidate" });
    await openMenuBuilderAs(page, "owner", candidate.menuId);
    await page.getByTestId("manage-page-screens").click();
    const row = page.getByTestId("screen-assignments-view").getByText("cross-menu-owner screen").locator("..");
    await row.getByRole("button", { name: /^Choose/ }).click();
    await expect(page.getByTestId("assignment-choice")).toContainText(occupied.pages[0].name);
  });
});
