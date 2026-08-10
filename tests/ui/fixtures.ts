import { test as base, expect, type Page } from "@playwright/test";

/**
 * The Back Office reads its session token from sessionStorage on first render,
 * so seeding it before any script runs signs the page in without driving a form.
 * Tokens come from the API's BackOffice__Sessions__* configuration.
 */
const tokenStorageKey = "vennusign.back-office.token";

/**
 * "scale" is an owner of a SECOND venue, used only by the shelf-at-scale checks.
 * The default venue accumulates menus from every spec that seeds, so nothing there
 * can assert "exactly this many menus" while the suite runs in parallel.
 */
export type VennuRole = "owner" | "editor" | "publisher" | "scale";

/** Isolation tag of the seeded dataset this run targets. See run-track1-qa.ps1. */
const tag = process.env.VENNU_ISOLATION_TAG ?? "0000";

/**
 * The default dataset keeps the token names the owner acceptance workbook prints, so a
 * reviewer can sign in with exactly what it says. Only extra isolated datasets carry a
 * tag suffix, which is all that is needed to stop lanes sharing an identity.
 */
const baselineTokens: Record<VennuRole, string> = {
  owner: "track1-owner-review",
  editor: "track1-content-editor",
  publisher: "track1-publisher",
  scale: "track1-scale-check"
};

const tokenFor = (role: VennuRole) =>
  tag === "0000" ? baselineTokens[role] : `track1-${role}-${tag}`;

export const tokens: Record<VennuRole, string> = {
  owner: process.env.VENNU_OWNER_TOKEN ?? tokenFor("owner"),
  editor: process.env.VENNU_EDITOR_TOKEN ?? tokenFor("editor"),
  publisher: process.env.VENNU_PUBLISHER_TOKEN ?? tokenFor("publisher"),
  scale: process.env.VENNU_SCALE_TOKEN ?? tokenFor("scale")
};

export const apiBaseUrl = process.env.VENNU_API_URL ?? "https://localhost:7138";

async function signIn(page: Page, role: VennuRole) {
  await page.addInitScript(
    ([key, value]) => window.sessionStorage.setItem(key, value),
    [tokenStorageKey, tokens[role]] as const
  );
}

/**
 * Opens a Back Office hash route already signed in as `role`.
 * Navigating straight to the route avoids paying for click-through navigation.
 */
export async function openAs(page: Page, role: VennuRole, route: string) {
  await signIn(page, role);
  await page.goto(`/#${route}`);
  // Not networkidle: the screens area polls for player status, so the network never
  // goes idle and the wait times out. Waiting for the shell is deterministic, and
  // Playwright's per-locator auto-waiting covers everything after it.
  await page.locator('[data-testid="nav-item"]').first().waitFor({ state: "attached" });
}

/**
 * Opens the menu editor, through the door a person uses.
 *
 * Menus milestone 2 put the shelf at `#/menu`, so the editor is no longer the
 * first thing that route shows — a card is opened to reach it. Every spec that
 * drives the editor goes through here rather than each learning the new path,
 * so when milestone 3 replaces the editor with the builder there is one place
 * to change.
 *
 * The board is the door: there is no Open button on a card (design README,
 * Navigation), which is why this clicks the board itself.
 */
export async function openMenuEditorAs(page: Page, role: VennuRole) {
  await openAs(page, role, "menu");
  await page.getByTestId("menus-home").waitFor();

  const card = page.getByTestId("menu-card").first();
  await card.waitFor();
  await card.getByTestId("open-menu").click();
  await page.getByTestId("menu-picker").waitFor();
}

export const test = base.extend<{ asOwner: Page }>({
  asOwner: async ({ page }, use) => {
    await signIn(page, "owner");
    await use(page);
  }
});

export { expect };
