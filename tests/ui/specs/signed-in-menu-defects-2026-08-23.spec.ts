import { test, expect } from "@playwright/test";
// @ts-expect-error - plain .mjs helpers, shared with non-Playwright QA tooling.
import { qaCredentials, qaCredentialSources, signInAsCustomer } from "../lib/customerAccount.mjs";

const apiBaseUrl = process.env.VENNU_API_URL ?? "https://dev.api.vennusign.com";

/**
 * Deployed-environment, real-Entra-sign-in coverage for the 2026-08-23 deploy
 * (#775, #797/#809, #799/#812, #806/#807, #823). Unlike menu-builder.spec.ts
 * (which seeds through bypass tokens configured only for the local Track 1
 * environment), these cases sign in as the real QA customer account and run
 * against whatever VENNU_BACK_OFFICE_URL/VENNU_API_URL point at - built
 * specifically so Murphy (and anyone else) can point this at a deployed
 * environment and get real coverage of the menu builder without a
 * locally-seeded back door.
 *
 *   VENNU_BACK_OFFICE_URL=https://dev.back-office.vennusign.com \
 *   VENNU_API_URL=https://dev.api.vennusign.com \
 *   node node_modules/@playwright/test/cli.js test specs/signed-in-menu-defects-2026-08-23.spec.ts --project=desktop
 *
 * Each case creates a throwaway menu. The QA venue has a real, working ceiling
 * (confirmed live 2026-08-23: creating a 51st menu on a 50-menu venue correctly
 * refuses with "That would be 51 menus, and this venue is set up for 50...",
 * shown in the create-menu dialog's own error text) - and this run's own testing
 * pushed the venue to exactly that ceiling. Rather than fail confusingly against
 * an exhausted venue, each case skips itself with a clear reason via
 * skipIfMenuShelfFull below; the fix is cleanup (see #752 - report drift, don't
 * remove it from a test), not a bigger timeout.
 */
const credentials = qaCredentials();

const openAddItemRow = async (page: any) => {
  const opener = page.getByTestId("open-add-item");
  if (await opener.isVisible({ timeout: 2_000 }).catch(() => false)) await opener.click();
  await expect(page.getByTestId("add-item-input")).toBeVisible({ timeout: 15_000 });
};

/** Skips the calling test with a clear reason if the venue has no room left to create a menu. */
const skipIfMenuShelfFull = async (page: any) => {
  const count = await page.evaluate(async (base: string) => {
    const r = await fetch(`${base}/api/back-office/content/menus`, { credentials: "include" });
    return r.ok ? (await r.json()).length : null;
  }, apiBaseUrl);
  test.skip(count !== null && count >= 50, `QA venue is at its menu ceiling (${count}/50) - see #752, cleanup needed before this case can create another throwaway menu.`);
};

test.describe("signed-in menu builder regression, 2026-08-23 deploy", () => {
  // Serial for the same reason publish-boundary.spec.ts is: this is the one QA
  // customer account, and concurrent workers racing Entra sign-in and menu
  // creation on the same account produced flaky failures unrelated to the product.
  test.describe.configure({ mode: "serial", timeout: 400_000 });
  test.skip(!credentials, `No QA customer credentials. Looked in ${qaCredentialSources()}.`);
  test.beforeEach(({}, testInfo) => test.skip(testInfo.project.name === "mobile", "Menus mobile interactions are out of scope (Q158)."));

  test("#775: Enter and the create button racing each other still produces exactly one item", async ({ page }) => {
    test.setTimeout(240_000);
    await signInAsCustomer(page, credentials);
    await page.goto("/#/menu");
    await page.locator('[data-testid="nav-item"]').first().waitFor({ state: "attached" });
    await skipIfMenuShelfFull(page);

    const stamp = Math.random().toString(16).slice(2, 8);
    await page.getByTestId("add-a-menu").first().click();
    await page.getByTestId("new-menu-name").fill(`QA Murphy race ${stamp}`);
    await page.getByTestId("create-menu").click();
    await expect(page.getByTestId("menu-builder")).toBeVisible({ timeout: 90_000 });

    await openAddItemRow(page);
    const name = `Race Item ${stamp}`;
    await page.getByTestId("add-item-input").fill(name);
    await Promise.all([
      page.getByTestId("add-item-input").press("Enter"),
      page.getByTestId("add-item-create").click({ force: true })
    ]);

    await expect(page.getByTestId("board-item").filter({ hasText: name })).toHaveCount(1, { timeout: 60_000 });
    // Give any second write a moment to land before asserting it never did.
    await page.waitForTimeout(2_000);
    await expect(page.getByTestId("board-item").filter({ hasText: name })).toHaveCount(1);
  });

  test("#797/#809: both delete-section options say \"delete this section\"", async ({ page }) => {
    test.setTimeout(240_000);
    await signInAsCustomer(page, credentials);
    await page.goto("/#/menu");
    await page.locator('[data-testid="nav-item"]').first().waitFor({ state: "attached" });
    await skipIfMenuShelfFull(page);

    const stamp = Math.random().toString(16).slice(2, 8);
    await page.getByTestId("add-a-menu").first().click();
    await page.getByTestId("new-menu-name").fill(`QA Murphy delsec ${stamp}`);
    await page.getByTestId("create-menu").click();
    await expect(page.getByTestId("menu-builder")).toBeVisible({ timeout: 90_000 });

    await openAddItemRow(page);
    await page.getByTestId("add-item-input").fill(`Keeper ${stamp}`);
    await page.getByTestId("add-item-create").click();
    await expect(page.getByTestId("board-item").filter({ hasText: `Keeper ${stamp}` })).toBeVisible({ timeout: 90_000 });

    await page.getByTestId("add-section").click();
    await page.getByTestId("new-section-name").fill(`Second ${stamp}`);
    await page.getByTestId("new-section-name").press("Enter");
    await expect(page.getByTestId("section-row")).toHaveCount(2, { timeout: 30_000 });

    const firstRow = page.getByTestId("section-row").first();
    await firstRow.getByTestId("rail-section").click();
    await firstRow.getByTestId("delete-section").click();
    const dialog = page.getByTestId("delete-section-dialog");
    await expect(dialog).toBeVisible({ timeout: 15_000 });
    await expect(dialog.getByLabel(/^Delete this section, moving its items to/)).toBeVisible();
    await expect(dialog.getByLabel(/^Delete this section, returning its items/)).toBeVisible();
    await page.keyboard.press("Escape");
  });

  test("#806/#807: publishing a price-less item shows a centered confirmation naming it", async ({ page }) => {
    test.setTimeout(240_000);
    await signInAsCustomer(page, credentials);
    await page.goto("/#/menu");
    await page.locator('[data-testid="nav-item"]').first().waitFor({ state: "attached" });
    await skipIfMenuShelfFull(page);

    const stamp = Math.random().toString(16).slice(2, 8);
    await page.getByTestId("add-a-menu").first().click();
    await page.getByTestId("new-menu-name").fill(`QA Murphy priceless ${stamp}`);
    await page.getByTestId("create-menu").click();
    await expect(page.getByTestId("menu-builder")).toBeVisible({ timeout: 90_000 });

    await openAddItemRow(page);
    const name = `Priceless Item ${stamp}`;
    await page.getByTestId("add-item-input").fill(name);
    await page.getByTestId("add-item-create").click();
    await expect(page.getByTestId("board-item").filter({ hasText: name })).toBeVisible({ timeout: 90_000 });
    await expect(page.getByTestId("missing-price-flag")).toBeVisible();

    await page.getByTestId("publish").click();
    const dialog = page.getByTestId("publish-missing-price-dialog");
    await expect(dialog).toBeVisible({ timeout: 20_000 });
    await expect(dialog).toContainText("1 item has no price");
    await expect(dialog).toContainText(name);

    // Placement: a small, centered modal identical in mechanism to every other
    // builder confirmation dialog (review, discard, delete-section) - not a
    // banner, not anchored to the item, not full-screen. Pins today's layout so
    // a future change to it is a deliberate, visible diff here.
    const viewport = page.viewportSize()!;
    const box = (await dialog.boundingBox())!;
    const centerX = box.x + box.width / 2;
    const centerY = box.y + box.height / 2;
    expect(Math.abs(centerX - viewport.width / 2)).toBeLessThan(5);
    expect(Math.abs(centerY - viewport.height / 2)).toBeLessThan(5);

    await dialog.getByRole("button", { name: "Go back" }).click();
    await expect(dialog).toHaveCount(0);
  });

  /**
   * #823: "Go back to..." has no loading state, unlike its sibling "View all"
   * dialog (which correctly shows "Loading history..."). It renders `history ??
   * []` uniformly, so while its own history fetch is still in flight it shows
   * "Nothing to go back to yet - this menu has not been published" even on a
   * menu whose "go back to..." link only appears because it HAS a published
   * version. This is expected to FAIL until #823 is fixed - it documents the
   * gap rather than a passing guarantee.
   * See https://github.com/jmiedreich-ux/Vennusign/issues/823
   */
  test("#823 (known gap): \"go back to...\" never claims a just-published menu has no history", async ({ page }) => {
    test.setTimeout(240_000);
    await signInAsCustomer(page, credentials);
    await page.goto("/#/menu");
    await page.locator('[data-testid="nav-item"]').first().waitFor({ state: "attached" });
    await skipIfMenuShelfFull(page);

    const stamp = Math.random().toString(16).slice(2, 8);
    await page.getByTestId("add-a-menu").first().click();
    await page.getByTestId("new-menu-name").fill(`QA Murphy goback ${stamp}`);
    await page.getByTestId("create-menu").click();
    await expect(page.getByTestId("menu-builder")).toBeVisible({ timeout: 90_000 });

    await openAddItemRow(page);
    await page.getByTestId("add-item-input").fill(`Priced ${stamp}`);
    await page.getByTestId("add-item-price").fill("5.00");
    await page.getByTestId("add-item-create").click();
    await expect(page.getByTestId("board-item").filter({ hasText: `Priced ${stamp}` })).toBeVisible({ timeout: 90_000 });

    await page.getByTestId("publish").click();
    await expect(page.getByTestId("publish-bar")).toBeVisible({ timeout: 90_000 });
    await expect(page.getByTestId("go-back-to")).toBeVisible({ timeout: 30_000 });

    await page.getByTestId("go-back-to").click();
    const dialog = page.getByTestId("history-dialog");
    await expect(dialog).toBeVisible({ timeout: 15_000 });

    // The defect: this text appears immediately, before the history fetch has
    // had any chance to resolve, on a menu that was JUST published above.
    await expect(dialog.getByTestId("history-empty")).toHaveCount(0);
  });
});
