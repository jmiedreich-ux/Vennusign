import { request as playwrightRequest } from "@playwright/test";
import { readFileSync } from "node:fs";
import { resolve } from "node:path";
import { apiBaseUrl, tokens, type VennuRole } from "./fixtures";

const testApiBaseUrl = process.env.VENNU_TEST_API_URL ?? "https://localhost:7140";

function testApiKey(): string {
  if (process.env.VENNU_TEST_API_KEY) return process.env.VENNU_TEST_API_KEY;

  try {
    return readFileSync(resolve(import.meta.dirname, "..", "..", "artifacts", "ui-test-env", "test-api.key"), "utf8").trim();
  } catch {
    throw new Error("The Test API key is unavailable. Start the environment with scripts/start-ui-test-env.ps1.");
  }
}

/** Response from the separately deployed Test API. */
export type SeedResult = {
  organizationId: string;
  venueId: string;
  menuId: string;
  sectionId: string;
  itemId: string;
  menuName: string;
  sectionName: string;
  itemName: string;
  itemDescription: string;
  itemPrice: number;
  screenId?: string;
  screenKey?: string;
  pages: Array<{ pageId: string; name: string; sortOrder: number }>;
  sections: Array<{ sectionId: string; pageId: string; name: string; sortOrder: number }>;
  items: Array<{ itemId: string; sectionId: string; name: string; price: string }>;
};

/**
 * Creates a private menu/section/item (and optionally a screen) for one test.
 *
 * This is what lets specs run fully in parallel: no two tests touch the same rows,
 * so nothing has to be serialised behind a shared fixture the way the hosted-agent
 * lanes were.
 */
export async function seed(
  options: { role?: VennuRole; includeScreen?: boolean; label?: string; pageCount?: number; sectionCount?: number; itemsPerSection?: number; screenState?: string; screenWidthPixels?: number; screenHeightPixels?: number } = {}
): Promise<SeedResult> {
  const context = await playwrightRequest.newContext({ ignoreHTTPSErrors: true });
  try {
    const response = await context.post(`${testApiBaseUrl}/api/test/seed`, {
      headers: { "X-Vennusign-Test-Api-Key": testApiKey() },
      data: {
        accessToken: tokens[options.role ?? "owner"],
        includeScreen: options.includeScreen ?? Boolean(options.screenState),
        label: options.label ?? "ui",
        pageCount: options.pageCount ?? 1,
        sectionCount: options.sectionCount ?? 1,
        itemsPerSection: options.itemsPerSection ?? 1,
        screenState: options.screenState ?? "offline",
        screenWidthPixels: options.screenWidthPixels ?? 1920,
        screenHeightPixels: options.screenHeightPixels ?? 1080
      }
    });
    if (!response.ok()) {
      throw new Error(`Seed failed (${response.status()}): ${await response.text()}`);
    }
    const result = (await response.json()) as SeedResult;
    created.push({ menuId: result.menuId, role: options.role ?? "owner" });
    return result;
  } finally {
    await context.dispose();
  }
}

/*
 * What this worker has made and not yet put away.
 *
 * A run makes 98 seed calls, every one creating a menu in the same shared venue, and nothing ever
 * removed them. The venue filled and the rest of the suite failed itself with "That would be 51
 * menus" - 138 of those in one run. The venue's ceiling is now raised, which stops that being
 * fatal; this stops it accumulating in the first place.
 *
 * A plain module-level array is safe because Playwright runs one test at a time inside a worker,
 * and each worker is its own process. A registry shared across parallel tests would be a race;
 * this is neither shared nor parallel.
 */
const created: Array<{ menuId: string; role: VennuRole }> = [];

/** Hands over everything seeded since the last call, and forgets it. */
export function takeSeeded(): Array<{ menuId: string; role: VennuRole }> {
  return created.splice(0, created.length);
}

/**
 * Puts away the menus a finished test made.
 *
 * Never throws. This runs after the test has already reached its verdict, and a cleanup that turns
 * a passing test red tells nobody anything about the product.
 */
export async function cleanupSeeded(): Promise<void> {
  const mine = takeSeeded();
  if (mine.length === 0) return;

  const context = await playwrightRequest.newContext({ ignoreHTTPSErrors: true });
  try {
    // Grouped by role because put-away goes through the real back-office route, with the token of
    // whoever owns that venue.
    for (const role of new Set(mine.map(entry => entry.role))) {
      await context.post(`${testApiBaseUrl}/api/test/cleanup`, {
        headers: { "X-Vennusign-Test-Api-Key": testApiKey() },
        data: { accessToken: tokens[role], menuIds: mine.filter(entry => entry.role === role).map(entry => entry.menuId) }
      });
    }
  } catch {
    // Deliberately silent. See above.
  } finally {
    await context.dispose();
  }
}

/** Scale response from the separately deployed Test API. */
export type ScaleSeedResult = {
  venueId: string;
  seededMenus: Array<{
    menuId: string;
    name: string;
    state: "on-screens" | "pending-changes" | "put-away" | "never-published";
    screenIds: string[];
  }>;
  screenIds: string[];
};

/**
 * Fills the scale venue with a shelf big enough to change shape, and leaves it in
 * a known state (Q176: twenty screens, thirteen menus).
 *
 * It clears the venue first, so it is deterministic however many times it runs —
 * which is why it has a venue of its own and refuses the shared one.
 */
export async function scaleSeed(options: { menus?: number; screens?: number } = {}): Promise<ScaleSeedResult> {
  const context = await playwrightRequest.newContext({ ignoreHTTPSErrors: true });
  try {
    const response = await context.post(`${testApiBaseUrl}/api/test/seed/scale`, {
      headers: { "X-Vennusign-Test-Api-Key": testApiKey() },
      data: {
        accessToken: tokens.scale,
        menus: options.menus ?? 13,
        screens: options.screens ?? 20
      },
      timeout: 120_000
    });
    if (!response.ok()) {
      throw new Error(`Scale seed failed (${response.status()}): ${await response.text()}`);
    }
    return (await response.json()) as ScaleSeedResult;
  } finally {
    await context.dispose();
  }
}

export async function backdateAvailability(itemId: string, minutesAgo: number): Promise<void> {
  const context = await playwrightRequest.newContext({ ignoreHTTPSErrors: true });
  try {
    const response = await context.post(`${testApiBaseUrl}/api/test/seed/backdate-availability`, {
      headers: { "X-Vennusign-Test-Api-Key": testApiKey() },
      data: { accessToken: tokens.owner, itemId, minutesAgo }
    });
    if (!response.ok()) {
      throw new Error(`Availability backdate failed (${response.status()}): ${await response.text()}`);
    }
  } finally {
    await context.dispose();
  }
}
