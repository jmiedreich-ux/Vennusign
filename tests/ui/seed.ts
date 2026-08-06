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
