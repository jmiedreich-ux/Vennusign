import { test, expect, findShelfCard, openAs, openMenuBuilderAs, apiBaseUrl, tokens } from "../fixtures";

/**
 * Workbook cases 0-0, 0-1 and 0-2 - environment, deterministic fixture, owner sign-in.
 *
 * These gate every other case: if the environment or the fixture is wrong, later
 * failures are meaningless. Asserting them explicitly means a broken environment
 * reports as itself instead of as a dozen unrelated UI failures.
 */

test("0-0 the acceptance environment is up and answering", async ({ request }) => {
  const health = await request.get(`${apiBaseUrl}/health/version`, { ignoreHTTPSErrors: true });
  expect(health.ok(), `health returned ${health.status()}`).toBeTruthy();

  const session = await request.get(`${apiBaseUrl}/api/back-office/session`, {
    headers: { "X-Vennusign-Back-Office-Token": tokens.owner },
    ignoreHTTPSErrors: true
  });
  expect(session.ok(), `session returned ${session.status()}`).toBeTruthy();

  const payload = await session.json();
  expect(payload.venueId, "session must resolve a venue").toBeTruthy();
  expect(Array.isArray(payload.capabilityDecisions)).toBeTruthy();
});

test("0-1 the deterministic fixture is loaded", async ({ page }) => {
  // The shelf is what this route shows from milestone 2 on, so the fixture's
  // menu is named on a card before anything is opened.
  await openAs(page, "owner", "menu");
  await expect(page.getByTestId("menus-home")).toContainText("Acceptance Menu");

  // Named, not "the first card": every spec that seeds adds a menu to this venue,
  // so which card comes first is whatever the shelf happened to order that run.
  (await findShelfCard(page, "Acceptance Menu")).getByTestId("open-menu").click();
  await page.getByTestId("menu-builder").waitFor();

  // The fixture's named records are what later cases assert against by name.
  await expect(page.getByTestId("builder-menu-name")).toContainText("Acceptance Menu");
  await expect(page.getByTestId("board-item").first()).toBeVisible();
  await expect(page.getByTestId("canvas")).toContainText("Harbor Lemonade");

  await page.goto("/#screens");
  await expect(page.getByTestId("screen-card").filter({ hasText: "Acceptance Screen" })).toHaveCount(1);
});

test("0-2 the owner signs in with the configured venue access", async ({ page }) => {
  await openAs(page, "owner", "home");

  // Signed in, identified, and not refused anywhere core.
  await expect(page.locator("body")).toContainText("Track 1 Owner Review");
  await expect(page.getByTestId("locked-panel")).toHaveCount(0);

  // Core work is reachable and editable: the shelf loads, and the editor behind
  // a card is not refused.
  await openAs(page, "owner", "menu");
  (await findShelfCard(page, "Acceptance Menu")).getByTestId("open-menu").click();
  await page.getByTestId("menu-builder").waitFor();
  await page.getByTestId("board-item").first().locator(".board-item-name").click();
  await expect(page.getByTestId("item-name")).toBeEnabled();
});
