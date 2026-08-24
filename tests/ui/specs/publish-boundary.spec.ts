import { test, expect } from "@playwright/test";
import { openAddItem } from "../fixtures";
// @ts-expect-error - plain .mjs helpers, shared with non-Playwright QA tooling.
import { qaCredentials, qaCredentialSources, signInAsCustomer } from "../lib/customerAccount.mjs";
// @ts-expect-error - see above.
import { loadSnapshot } from "../lib/customerOnboarding.mjs";

/**
 * What "publish" is supposed to mean, asserted from the screen's side.
 *
 * Two claims are under test here, both raised against dev:
 *   1. Items reach the display before they are published.
 *   2. Reordering sections does not reach the display, even after publishing.
 *
 * Both matter for the same reason: a venue edits its board in front of customers.
 * If a half-typed item is on the wall the moment it is created, publish is not a
 * gate, it is decoration - and if a deliberate publish does NOT ship a change the
 * owner just made, they have no way to tell what the screen is actually showing.
 *
 * Read the display through /api/display/{id}/content here rather than the rendered
 * player. customer-menu-journey.spec.ts already covers "is it drawn"; what these
 * cases need to separate is what the SERVER decides to serve from what the player
 * happens to have cached, and only the raw contract can tell those apart.
 *
 *   VENNU_BACK_OFFICE_URL=https://dev.back-office.vennusign.com \
 *   VENNU_API_URL=https://dev.api.vennusign.com \
 *   node node_modules/@playwright/test/cli.js test specs/publish-boundary.spec.ts --project=desktop
 */
const apiBaseUrl = process.env.VENNU_API_URL ?? "https://dev.api.vennusign.com";
const displayBaseUrl = process.env.VENNU_DISPLAY_URL ?? "https://dev.display.vennusign.com";
const credentials = qaCredentials();

/** The board as the screen is served it, section order preserved. */
const servedToScreen = async (page: any, screenId: string) => {
  const response = await page.request.get(`${apiBaseUrl}/api/display/${screenId}/content`);
  expect(response.ok(), `display content request failed: ${response.status()}`).toBe(true);
  const body = await response.json();
  const sections = body?.menu?.sections ?? body?.sections ?? [];
  return {
    raw: JSON.stringify(body),
    sectionNames: sections.map((section: any) => section.name ?? section.Name),
    itemNames: sections.flatMap((section: any) =>
      (section.items ?? section.Items ?? []).map((item: any) => item.name ?? item.Name))
  };
};

const addItem = async (page: any, name: string, price: string) => {
  await openAddItem(page);
  await page.getByTestId("add-item-input").fill(name);
  await page.getByTestId("add-item-price").fill(price);
  await page.getByTestId("add-item-create").click();
  await expect(page.getByTestId("board")).toContainText(name, { timeout: 60_000 });
};

const publish = async (page: any) => {
  await page.getByTestId("publish").click();
  await expect(page.getByTestId("publish-bar")).toBeVisible({ timeout: 60_000 });
};

/** A menu of this run's own, assigned to the QA account's paired screen. */
const buildAssignedMenu = async (page: any, menuName: string) => {
  await page.goto("/#/menu");
  await page.locator('[data-testid="nav-item"]').first().waitFor({ state: "attached" });

  await page.getByTestId("add-a-menu").first().click();
  await page.getByTestId("new-menu-name").fill(menuName);
  await page.getByTestId("create-menu").click();
  await expect(page.getByTestId("menu-builder")).toBeVisible({ timeout: 60_000 });

  await page.getByTestId("assignment-pill").click();
  const assignments = page.getByTestId("screen-assignments");
  await expect(assignments).toBeVisible();
  const row = assignments.getByTestId("screen-row").first();
  await expect(row, "the QA account needs a paired screen").toBeVisible();
  await row.getByTestId("add-screen-page").click();
  await assignments.getByTestId("add-screen-page-menu").getByRole("button").first().click();

  // A screen that already carries an assignment - which it does as soon as any
  // earlier run assigned it - asks whether to rotate or replace instead of just
  // taking the page. Nothing is drafted until that is answered, so skipping it
  // leaves "save" disabled and the whole setup silently does nothing.
  const choice = assignments.getByTestId("assignment-choice");
  if (await choice.isVisible().catch(() => false)) {
    await choice.getByRole("button", { name: /^replace$/i }).click();
  }

  await page.getByRole("button", { name: /save changes and return/i }).click();
  await expect(page.getByTestId("menu-builder")).toBeVisible({ timeout: 60_000 });
};

test.describe("publish is the boundary between the builder and the screen", () => {
  // Serial, and it matters. Every case here signs in as the one QA customer and
  // publishes to its one paired screen, so concurrent workers fight over the screen
  // assignment and over Entra sign-in itself - a parallel run failed all of these
  // with "the customer entry page at /signin never finished loading" while the same
  // cases passed one at a time. Playwright's own "consider running tests from slow
  // files in parallel" hint is wrong for this file.
  test.describe.configure({ mode: "serial", timeout: 300_000 });

  test.skip(!credentials, `No QA customer credentials. Looked in ${qaCredentialSources()}.`);
  test.beforeEach(({}, testInfo) =>
    test.skip(testInfo.project.name === "mobile", "Menus mobile interactions are out of scope (Q158)."));

  test("an unpublished item must not reach the screen", async ({ page }) => {
    test.setTimeout(300_000);

    await signInAsCustomer(page, credentials);
    const snapshot = await loadSnapshot(page, apiBaseUrl);
    expect(snapshot?.firstScreenId, "the QA account needs a paired screen").toBeTruthy();
    const screenId = snapshot.firstScreenId;

    const stamp = Math.random().toString(16).slice(2, 8);
    const published = `Published Pint ${stamp}`;
    const draft = `Draft Secret ${stamp}`;

    await buildAssignedMenu(page, `QA publish-gate ${stamp}`);

    // A published baseline first, so the screen is definitely serving THIS menu and
    // a later absence means "withheld", not "the screen was never looking here".
    await addItem(page, published, "6.00");
    await publish(page);
    await expect(async () => {
      const served = await servedToScreen(page, screenId);
      expect(served.itemNames, "the published item must reach the screen").toContain(published);
    }).toPass({ timeout: 90_000, intervals: [2_000, 5_000, 10_000] });

    // Now the actual case: created, never published.
    await addItem(page, draft, "9.99");

    // Give it the same grace the published item got. If it is going to appear it
    // will appear in this window, and waiting makes a pass mean something.
    await page.waitForTimeout(20_000);
    const served = await servedToScreen(page, screenId);
    expect(
      served.itemNames,
      `"${draft}" was never published, so the screen must not be serving it. Served: ${served.itemNames.join(", ")}`
    ).not.toContain(draft);
  });

  test("reordering sections reaches the screen when published", async ({ page }) => {
    test.setTimeout(300_000);

    await signInAsCustomer(page, credentials);
    const snapshot = await loadSnapshot(page, apiBaseUrl);
    expect(snapshot?.firstScreenId, "the QA account needs a paired screen").toBeTruthy();
    const screenId = snapshot.firstScreenId;

    const stamp = Math.random().toString(16).slice(2, 8);
    await buildAssignedMenu(page, `QA reorder ${stamp}`);

    // Two sections, each with an item, so order is observable from the screen's side.
    const firstSection = `Alpha ${stamp}`;
    const secondSection = `Beta ${stamp}`;

    const sections = page.getByTestId("section-row");

    // Rename the section the menu starts with. The pencil is what puts the rail row
    // into edit mode; the rename input does not exist until it is clicked.
    await sections.first().getByRole("button", { name: /^Rename / }).click();
    await page.getByTestId("section-rename-input").fill(firstSection);
    await page.keyboard.press("Enter");
    await expect(sections.first()).toContainText(firstSection, { timeout: 30_000 });

    // Adding a section uses its own input, not the rename one.
    await page.getByTestId("add-section").click();
    await page.getByTestId("new-section-name").fill(secondSection);
    await page.keyboard.press("Enter");
    await expect(sections).toHaveCount(2, { timeout: 30_000 });

    await page.getByTestId("rail-section").filter({ hasText: firstSection }).click();
    await addItem(page, `Alpha Item ${stamp}`, "1.00");
    await page.getByTestId("rail-section").filter({ hasText: secondSection }).click();
    await addItem(page, `Beta Item ${stamp}`, "2.00");
    await publish(page);

    await expect(async () => {
      const served = await servedToScreen(page, screenId);
      expect(served.sectionNames).toEqual([firstSection, secondSection]);
    }).toPass({ timeout: 90_000, intervals: [2_000, 5_000, 10_000] });

    // Move the second section above the first, then publish that deliberately.
    //
    // Dispatched drag events, because dragging is the only reorder this UI actually
    // offers: the rail's move buttons are styled `.builder__rail-move { display:none }`
    // with nothing anywhere turning them back on, so they are unreachable to a mouse,
    // a keyboard and a screen reader alike. The row's own React handlers keep the
    // dragged section in component state and never read dataTransfer, so dispatching
    // the three events drives the real handler rather than faking the outcome.
    const movingRow = sections.filter({ hasText: secondSection });
    const landingRow = sections.filter({ hasText: firstSection });
    await movingRow.dispatchEvent("dragstart");
    await landingRow.dispatchEvent("dragover");
    await landingRow.dispatchEvent("drop");
    await expect(sections.first()).toContainText(secondSection, { timeout: 30_000 });
    await publish(page);

    await expect(async () => {
      const served = await servedToScreen(page, screenId);
      expect(
        served.sectionNames,
        `the published order is ${secondSection} then ${firstSection}; the screen is being served ${served.sectionNames.join(", ")}`
      ).toEqual([secondSection, firstSection]);
    }).toPass({ timeout: 90_000, intervals: [2_000, 5_000, 10_000] });
  });

  test("a screen already showing a menu picks up a publish without being reloaded", async ({ page, context }) => {
    test.setTimeout(300_000);

    // The case the two above cannot see. They ask the API fresh every time, so they
    // prove what the server would serve to a player that asks. A real screen asks
    // once and then waits to be told; if nothing tells it, it keeps drawing what it
    // drew hours ago while Back Office insists the change is live. That is the shape
    // of "reordering does not update the display after publish" - the server is
    // right and the wall is stale.
    await signInAsCustomer(page, credentials);
    const snapshot = await loadSnapshot(page, apiBaseUrl);
    expect(snapshot?.firstScreenId, "the QA account needs a paired screen").toBeTruthy();
    const screenId = snapshot.firstScreenId;

    const stamp = Math.random().toString(16).slice(2, 8);
    const before = `Before ${stamp}`;
    const after = `After ${stamp}`;

    await buildAssignedMenu(page, `QA live-refresh ${stamp}`);
    await addItem(page, before, "3.00");
    await publish(page);

    // Open the player and leave it open for the rest of the case. Never reloaded
    // from here on: a reload would answer a question nobody asked.
    const player = await context.newPage();
    // The real player, not ?preview=observer: observer mode is the read-only preview
    // Back Office embeds, and this case is specifically about what a live screen does.
    await player.goto(`${displayBaseUrl}/display/${screenId}`);
    await expect(player.locator("body")).toContainText(before, { timeout: 90_000 });

    // Publish a second item while that player sits there.
    await addItem(page, after, "4.00");
    await publish(page);

    await expect(
      player.locator("body"),
      "a screen that was already showing this menu must pick up the publish on its own"
    ).toContainText(after, { timeout: 120_000 });

    // And now the claim exactly as it was reported: reorder the sections on a menu a
    // screen is already showing, publish, and see whether the wall follows. A second
    // section first, so there is an order to change.
    const second = `Second ${stamp}`;
    await page.getByTestId("add-section").click();
    await page.getByTestId("new-section-name").fill(second);
    await page.keyboard.press("Enter");
    await expect(page.getByTestId("section-row")).toHaveCount(2, { timeout: 30_000 });
    await page.getByTestId("rail-section").filter({ hasText: second }).click();
    await addItem(page, `Second Item ${stamp}`, "5.00");
    await publish(page);
    await expect(player.locator("body")).toContainText(`Second Item ${stamp}`, { timeout: 120_000 });

    const rows = page.getByTestId("section-row");
    const moving = rows.filter({ hasText: second });
    const landing = rows.first();
    await moving.dispatchEvent("dragstart");
    await landing.dispatchEvent("dragover");
    await landing.dispatchEvent("drop");
    await expect(rows.first()).toContainText(second, { timeout: 30_000 });
    await publish(page);

    // Order as the live player draws it, not as the API would answer a fresh ask.
    await expect(async () => {
      const drawn = await player.locator("body").innerText();
      const secondAt = drawn.indexOf(second);
      const firstAt = drawn.indexOf(`Second Item ${stamp}`);
      expect(secondAt, `the reordered section is not drawn at all: ${drawn.slice(0, 300)}`).toBeGreaterThanOrEqual(0);
      expect(firstAt).toBeGreaterThanOrEqual(0);
      const beforeText = drawn.indexOf(before);
      expect(
        secondAt,
        `after publishing the reorder, "${second}" must be drawn above the original section`
      ).toBeLessThan(beforeText >= 0 ? beforeText : Number.MAX_SAFE_INTEGER);
    }).toPass({ timeout: 120_000, intervals: [5_000, 10_000, 15_000] });

    await player.close();
  });
});
