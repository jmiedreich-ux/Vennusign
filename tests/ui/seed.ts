import { request as playwrightRequest } from "@playwright/test";
import { apiBaseUrl, tokens, type VennuRole } from "./fixtures";

/** Mirrors TestSeedController.SeedResponse. */
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
};

/**
 * Creates a private menu/section/item (and optionally a screen) for one test.
 *
 * This is what lets specs run fully in parallel: no two tests touch the same rows,
 * so nothing has to be serialised behind a shared fixture the way the hosted-agent
 * lanes were.
 */
export async function seed(
  options: { role?: VennuRole; includeScreen?: boolean; label?: string } = {}
): Promise<SeedResult> {
  const context = await playwrightRequest.newContext({ ignoreHTTPSErrors: true });
  try {
    const response = await context.post(`${apiBaseUrl}/api/test/seed`, {
      data: {
        accessToken: tokens[options.role ?? "owner"],
        includeScreen: options.includeScreen ?? false,
        label: options.label ?? "ui"
      }
    });
    if (!response.ok()) {
      throw new Error(`Seed failed (${response.status()}): ${await response.text()}`);
    }
    return (await response.json()) as SeedResult;
  } finally {
    await context.dispose();
  }
}

/** Mirrors TestSeedController.ScaleSeedResponse. */
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
    const response = await context.post(`${apiBaseUrl}/api/test/seed/scale`, {
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
