import { test, expect } from "@playwright/test";
import { openAddItem, publishDraft } from "../fixtures";
// @ts-expect-error - plain .mjs helpers, shared with non-Playwright QA tooling.
import { qaCredentials, qaCredentialSources, signInAsCustomer } from "../lib/customerAccount.mjs";
// @ts-expect-error - see above.
import { loadSnapshot } from "../lib/customerOnboarding.mjs";

/**
 * What a customer does after onboarding: make a menu, put things on it, publish,
 * and have a screen show them.
 *
 * Signed in through the real Entra flow against a DEPLOYED environment, like
 * customer-onboarding.spec.ts and unlike the rest of this suite, which seeds a
 * Back Office session token. The token specs prove the builder works; this proves
 * the customer who just onboarded can reach it.
 *
 * The last assertion is the one that matters. #739 was a display that read
 * dbo.MenuItems while the builder wrote dbo.Items joined through dbo.Placements,
 * so every menu ever built rendered an empty board - and the unit coverage passed
 * throughout, because it seeded the table the product had stopped writing. So this
 * asserts against what the screen is actually served, not against the builder's
 * own view of itself.
 *
 *   VENNU_BACK_OFFICE_URL=https://dev.back-office.vennusign.com \
 *   VENNU_API_URL=https://dev.api.vennusign.com \
 *   node node_modules/@playwright/test/cli.js test specs/customer-menu-journey.spec.ts --project=desktop
 */
const apiBaseUrl = process.env.VENNU_API_URL ?? "https://dev.api.vennusign.com";
const credentials = qaCredentials();

test.describe("the customer builds a menu and a screen shows it", () => {
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

  test("a new menu, two items on it, published, and served to the screen", async ({ page }) => {
    // Not test.slow(): that is 3x the 30s default, and this one test signs in through
    // Entra, builds a menu, assigns a screen, publishes, and then waits for a real
    // player to draw - each of which is tens of seconds on B1. The per-step waits
    // below are meaningless if the whole test is capped under their sum.
    test.setTimeout(300_000);

    // Every non-OK API response, named. Without this a stuck screen is just a
    // timeout, and the interesting half - what the server actually said - is lost.
    const apiFailures: string[] = [];
    page.on("response", response => {
      const url = response.url();
      if (url.includes("/api/") && !response.ok()) {
        apiFailures.push(`${response.status()} ${response.request().method()} ${url}`);
      }
    });

    await signInAsCustomer(page, credentials);

    // A name this run owns, so repeated runs never collide and the board assertion
    // cannot accidentally pass on content some earlier run left behind.
    const stamp = Math.random().toString(16).slice(2, 8);
    const menuName = `QA journey ${stamp}`;
    const first = { name: `Tuna Melt ${stamp}`, price: "8.50" };
    const second = { name: `House Lager ${stamp}`, price: "5.00" };

    await page.goto("/#/menu");
    await page.locator('[data-testid="nav-item"]').first().waitFor({ state: "attached" });

    await test.step("create the menu", async () => {
      // M6.5: no name prompt. Add a menu opens the route chooser, blank creates
      // it immediately, and the builder's crumb is where it gets its name.
      await page.getByTestId("add-a-menu").first().click();
      await page.getByTestId("add-route-blank").click();
      // Generous, because a cold B1 worker is slow and "stuck" must not be
      // reported when the truth is "took 40 seconds".
      await expect(page.getByTestId("menu-builder"), `API failures so far:\n${apiFailures.join("\n") || "(none)"}`)
        .toBeVisible({ timeout: 60_000 });
      // The crumb opens as an edit field on a blank menu, so this is where the
      // name is given rather than confirmed.
      await page.getByTestId("menu-name-input").fill(menuName);
      await page.getByTestId("menu-name-input").press("Enter");
      await expect(page.getByTestId("builder-menu-name")).toContainText(menuName);
    });

    await test.step("put two items on it", async () => {
      for (const item of [first, second]) {
        // Creating an item selects it, which replaces the add panel with that
        // item's editor - so the add panel has to be reopened for each one. The
        // first run of this spec only ever reached item one, which is why the
        // loop assumed it stayed open.
        await openAddItem(page);
        await expect(page.getByTestId("add-item-input")).toBeVisible({ timeout: 30_000 });

        await page.getByTestId("add-item-input").fill(item.name);
        await page.getByTestId("add-item-price").fill(item.price);
        await page.getByTestId("add-item-create").click();
        // Explicit, and generous for the same reason the builder wait above is: the
        // create returns "placed" quickly, but the board refetch behind it is a
        // second round trip, and on a cold B1 worker that outruns the 7s default.
        await expect(page.getByTestId("board")).toContainText(item.name, { timeout: 60_000 });
      }
    });

    await test.step("assign the menu to the screen", async () => {
      // Without this the menu belongs to no screen, so the last step would be
      // asserting against whatever was already assigned rather than what was built.
      await page.getByTestId("assignment-pill").click();
      const assignments = page.getByTestId("screen-assignments");
      await expect(assignments).toBeVisible();

      const row = assignments.getByTestId("screen-row").first();
      await expect(row, "the QA account needs a paired screen to assign to").toBeVisible();
      await expect(row, "an unpaired screen cannot be assigned").not.toHaveAttribute("data-state", "unpaired");

      await row.getByTestId("add-screen-page").click();
      await assignments.getByTestId("add-screen-page-menu").getByRole("button").first().click();

      // Once the screen carries any assignment, adding a page asks rotate-or-replace
      // rather than just taking it, and nothing is drafted until that is answered.
      const choice = assignments.getByTestId("assignment-choice");
      if (await choice.isVisible().catch(() => false)) {
        await choice.getByRole("button", { name: /^replace$/i }).click();
      }

      await page.getByRole("button", { name: /save changes and return/i }).click();
      await expect(page.getByTestId("menu-builder")).toBeVisible({ timeout: 60_000 });
    });

    await test.step("publish it", async () => {
      await publishDraft(page);
      // The publish bar reports the outcome; a failure here is a failure to publish,
      // not a slow render, so it is worth waiting properly for.
      await expect(page.getByTestId("publish-bar")).toBeVisible({ timeout: 30_000 });
    });

    await test.step("the screen shows what was built", async () => {
      const snapshot = await loadSnapshot(page, apiBaseUrl);
      expect(snapshot?.firstScreenId, "the QA account needs a paired screen").toBeTruthy();

      // Through the display itself, not through /api/display/{id}/content. #739 was
      // a board that rendered empty while the API answered perfectly well, so a JSON
      // assertion is exactly the check that incident already walked past once. The
      // screen card embeds the real player in observer mode, which is the same code
      // a venue's screen runs.
      await page.goto("/#screens");
      const card = page.locator(`[data-testid="screen-card"][data-screen-id="${snapshot.firstScreenId}"]`);
      await expect(card, "the paired screen must be listed").toBeVisible({ timeout: 60_000 });
      await card.getByTestId("screen-preview").click();

      const player = card.locator("iframe").last();
      await expect(player).toHaveAttribute("src", /preview=observer/);

      // Poll the rendered board: publishing is asynchronous and a cold B1 worker adds
      // tens of seconds, so failing here has to mean "never drawn", not "not yet".
      const board = player.contentFrame().locator("body");
      await expect(board, "the published items must be drawn on the screen")
        .toContainText(first.name, { timeout: 90_000 });
      await expect(board).toContainText(second.name, { timeout: 30_000 });
    });
  });
});
