import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";
import {
  dismissUpgradeFeature,
  listUpgradeOpportunities,
  readDismissedUpgradeFeatures,
  tierPresentation
} from "../src/upgradeExperience.mjs";

const [app, api, modal, sidebar, operations, adminApp, adminVenue] = await Promise.all([
  readFile(new URL("../src/App.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/UpgradeModal.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/SidebarUpgradeNudge.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/VenueOperations.tsx", import.meta.url), "utf8"),
  readFile(new URL("../../admin/src/App.tsx", import.meta.url), "utf8"),
  readFile(new URL("../../admin/src/VenueDetail.tsx", import.meta.url), "utf8")
]);

test("billing presentation stays claim-bound and excludes Super Admin credentials", () => {
  const billingApi = api.slice(api.indexOf("loadVenueBillingPresentation"), api.indexOf("async function menuRequest"));
  assert.match(billingApi, /api\/venue-admin\/billing\/presentation/);
  assert.match(billingApi, /X-Vennu-Venue-Token/);
  assert.doesNotMatch(billingApi, /venueId|X-Vennu-Admin-Key|stripe/i);
});

test("canonical opportunities and dismissals remain deterministic in Venue Admin", () => {
  assert.equal(tierPresentation.pro.badgeLabel, "PRO");
  assert.deepEqual(
    listUpgradeOpportunities({
      happy_hour: { enabled: false },
      quick_update: { enabled: false },
      video_wall: { enabled: false }
    }).map(item => item.featureKey),
    ["quick_update", "happy_hour", "video_wall"]
  );
  const values = new Map();
  const storage = {
    getItem: key => values.get(key) ?? null,
    setItem: (key, value) => values.set(key, value)
  };
  dismissUpgradeFeature("happy_hour", storage);
  assert.deepEqual([...readDismissedUpgradeFeatures(storage)], ["happy_hour"]);
});

test("Venue Admin coordinates one prompt surface and one upgrade modal", () => {
  assert.match(app, /allowed && !inlineOpportunity && !upgradeContext/);
  assert.match(app, /inlineOpportunity && !upgradeContext/);
  assert.match(app, /<LockedNavigationItem/);
  assert.match(app, /<LockedSectionPreview/);
  assert.match(app, /<UpgradeModal/);
  assert.match(sidebar, /prefers-reduced-motion: reduce/);
  assert.equal((operations.match(/showUpgradePrompt=\{false\}/g) ?? []).length, 7);
});

test("tier value uses the established ten-month annual rule without changing entitlement", () => {
  assert.match(modal, /targetTier\.monthlyPrice \* 10/);
  assert.match(modal, /Annual · two months included/);
  assert.doesNotMatch(modal, /fetch|effectiveFeatures|enabled\s*=/);
});

test("Super Admin retains support controls but no customer upgrade orchestration", () => {
  assert.doesNotMatch(adminApp, /SidebarUpgradeNudge|UpgradeModal|continueUpgrade/);
  assert.doesNotMatch(adminVenue, /InlineFeatureHint|LockedSectionPreview|upgradeOpportunity/);
  assert.match(adminVenue, /Switch tier/);
  assert.match(adminVenue, /Active overrides/);
  assert.match(adminVenue, /Open Venue Admin/);
});
