import { test as base, expect, type Page } from "@playwright/test";

/**
 * The Back Office reads its session token from sessionStorage on first render,
 * so seeding it before any script runs signs the page in without driving a form.
 * Tokens come from the API's BackOffice__Sessions__* configuration.
 */
const tokenStorageKey = "vennusign.back-office.token";

export type VennuRole = "owner" | "editor" | "publisher";

/** Isolation tag of the seeded dataset this run targets. See run-track1-qa.ps1. */
const tag = process.env.VENNU_ISOLATION_TAG ?? "0000";

export const tokens: Record<VennuRole, string> = {
  owner: process.env.VENNU_OWNER_TOKEN ?? `track1-owner-${tag}`,
  editor: process.env.VENNU_EDITOR_TOKEN ?? `track1-editor-${tag}`,
  publisher: process.env.VENNU_PUBLISHER_TOKEN ?? `track1-publisher-${tag}`
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

export const test = base.extend<{ asOwner: Page }>({
  asOwner: async ({ page }, use) => {
    await signIn(page, "owner");
    await use(page);
  }
});

export { expect };
