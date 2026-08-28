import { test, expect } from "@playwright/test";
// @ts-expect-error - plain .mjs helpers, shared with non-Playwright QA tooling.
import { qaCredentials, qaCredentialSources, signInAsCustomer } from "../lib/customerAccount.mjs";

/*
 * Review is a step now, not a screen the product jumps past (owner, 2026-08-28).
 *
 * A resolved session used to render the destination immediately, so these specs went straight
 * there. The operator now passes THROUGH the review - which is where the line inventory and
 * "Nothing left to answer" live - and moves on deliberately.
 */
async function onwardToDestination(page: import("@playwright/test").Page) {
  const onward = page.getByTestId("go-to-destination");
  if (await onward.count()) await onward.click();
}


/**
 * Regression for #862/#864 (M6.4): `MenuPasteParser.PriceAtEnd` used to require two
 * spaces (or a dot leader) between an item's name and its price, so an ordinary
 * single-space paste - or a tab-separated spreadsheet paste - read as zero items
 * and every line came back `unresolved` / `item_format_not_recognized`.
 *
 * Signed in through the real Entra flow against a DEPLOYED environment, like
 * customer-menu-journey.spec.ts - this proves the fix against the actual deployed
 * parser, not a unit-test double of it.
 *
 *   VENNU_BACK_OFFICE_URL=https://dev.back-office.vennusign.com \
 *   VENNU_API_URL=https://dev.api.vennusign.com \
 *   node node_modules/@playwright/test/cli.js test specs/menu-paste-import-parser.spec.ts --project=desktop
 */
const apiBaseUrl = process.env.VENNU_API_URL ?? "https://dev.api.vennusign.com";
const credentials = qaCredentials();

type ImportLine = {
  lineNumber: number; rawText: string; disposition: "blank" | "section" | "item" | "unresolved" | "fallback";
  parsedName: string | null; parsedPrice: string | null; parserReason: string | null;
};
type ImportSession = { session: { itemCount: number; lineCount: number }; lines: ImportLine[] };

test.describe("paste import reads an ordinary menu (#862/#864)", () => {
  test.skip(!credentials, `No QA customer credentials. Looked in ${qaCredentialSources()}.`);

  test("single-space, tab-separated, MP, and the SPECIALS-2 trade-off all parse as expected", async ({ page }) => {
    test.setTimeout(120_000);
    await signInAsCustomer(page, credentials);

    // The venueFetch client sends this literal token as the header value when the
    // page is authenticated through the customer session cookie rather than a
    // static access token (see src/back-office/src/App.tsx's `customerSessionAccess`).
    // page.request shares this browser context's cookies, so the __Host- session
    // cookie rides along automatically.
    async function startImport(rawPaste: string): Promise<ImportSession> {
      const response = await page.request.post(`${apiBaseUrl}/api/back-office/menu-imports`, {
        headers: { "Content-Type": "application/json", "X-Vennusign-Back-Office-Token": "customer-session" },
        data: { rawPaste }
      });
      expect(response.status(), `POST /api/back-office/menu-imports for:\n${rawPaste}`).toBe(201);
      return response.json();
    }

    await test.step("an ordinary menu: single spaces, two sections, four items", async () => {
      const rawPaste = "STARTERS\nGarlic Bread 6.50\nWings 12\n\nMAINS\nBurger  14\nSteak Frites 28.00";
      const body = await startImport(rawPaste);
      await test.info().attach("ordinary-menu-response.json", { body: JSON.stringify(body, null, 2), contentType: "application/json" });

      expect(body.session.itemCount, "itemCount").toBe(4);
      const sections = body.lines.filter(line => line.disposition === "section");
      const items = body.lines.filter(line => line.disposition === "item");
      const unresolved = body.lines.filter(line => line.disposition === "unresolved");
      expect(sections.map(line => line.rawText), "sections").toEqual(["STARTERS", "MAINS"]);
      expect(items.map(line => `${line.parsedName}|${line.parsedPrice}`), "items").toEqual([
        "Garlic Bread|6.50", "Wings|12", "Burger|14", "Steak Frites|28.00"
      ]);
      expect(unresolved, "no line should be unresolved").toEqual([]);
    });

    await test.step("a tab-separated line (spreadsheet paste)", async () => {
      const body = await startImport("Garlic Bread\t6.50");
      await test.info().attach("tab-separated-response.json", { body: JSON.stringify(body, null, 2), contentType: "application/json" });
      expect(body.session.itemCount).toBe(1);
      expect(body.lines[0].disposition).toBe("item");
      expect(body.lines[0].parsedName).toBe("Garlic Bread");
      expect(body.lines[0].parsedPrice).toBe("6.50");
    });

    await test.step("a market-price (MP) line", async () => {
      const body = await startImport("Market Fish MP");
      await test.info().attach("mp-line-response.json", { body: JSON.stringify(body, null, 2), contentType: "application/json" });
      expect(body.session.itemCount).toBe(1);
      expect(body.lines[0].disposition).toBe("item");
      expect(body.lines[0].parsedName).toBe("Market Fish");
      expect(body.lines[0].parsedPrice).toBe("MP");
    });

    await test.step("the accepted trade-off: a capitals line ending in a bare number reads as an item, not a section", async () => {
      const body = await startImport("SPECIALS 2");
      await test.info().attach("specials-2-response.json", { body: JSON.stringify(body, null, 2), contentType: "application/json" });
      // This is documented, deliberate behavior (milestone-plan.md, M6.4's
      // "trade-off accepted"), not a defect - recoverable through the review
      // screen's "Make it a section" action.
      expect(body.session.itemCount).toBe(1);
      expect(body.lines[0].disposition).toBe("item");
      expect(body.lines[0].parsedName).toBe("SPECIALS");
      expect(body.lines[0].parsedPrice).toBe("2");
    });
  });

  test("the review screen shows the parsed items to a person", async ({ page }) => {
    test.setTimeout(120_000);
    await signInAsCustomer(page, credentials);

    await page.goto("/#/menu/import");
    await expect(page.getByTestId("menu-import-start")).toBeVisible({ timeout: 30_000 });

    // The exact input from #864's repro. This QA venue's item library has no
    // "Garlic Bread"/"Wings"/"Burger"/"Steak Frites", so no identity match questions
    // are raised and the session goes straight to "resolved" - see the comment below.
    const rawPaste = "STARTERS\nGarlic Bread 6.50\nWings 12\n\nMAINS\nBurger  14\nSteak Frites 28.00";
    await page.getByLabel("Menu text").fill(rawPaste);
    await page.getByRole("button", { name: "Read menu" }).click();

    // With no library items to match, every line resolves with zero questions, so
    // the session goes straight to "resolved" (destination choice) rather than
    // pausing on the review screen - itself a sign every line parsed, since a
    // single item_format_not_recognized line raises an "unreadable" question that
    // would hold the session on the review screen instead.
    const review = page.getByTestId("menu-import-review");
    const destination = page.getByTestId("menu-import-create");
    await expect(review.or(destination)).toBeVisible({ timeout: 30_000 });

    // No "unreadable" question card - that card ("What should this line become?")
    // is exactly what every line rendered as before the fix.
    await expect(page.getByText("What should this line become?")).toHaveCount(0);

    if (await review.isVisible()) {
      // "4" items read - the decisive number this whole fix is about.
      await expect(page.locator(".import-summary")).toContainText("4");
      await expect(page.locator(".import-summary")).toContainText("items read");
    } else {
      await expect(destination).toContainText("Build a new unpublished menu from all 4 imported items.");
    }

    // Viewport only, not fullPage: this QA venue accumulates one "Replace an
    // existing menu" row per past automated run, so a full-page shot is mostly
    // unrelated list noise below the fold. The heading and item count are above it.
    await page.screenshot({ path: "artifacts/menu-paste-import-parser-review.png" });
  });
});
