import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";
import {
  dismissUpgradeFeature,
  listUpgradeOpportunities,
  readDismissedUpgradeFeatures,
  tierPresentation
} from "../src/upgradeExperience.mjs";

const [app, api, sheet, sidebar, operations, platformOperationsApp, platformOperationsVenue] = await Promise.all([
  readFile(new URL("../src/App.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/UpgradeSheet.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/SidebarUpgradeNudge.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/VenueOperations.tsx", import.meta.url), "utf8"),
  readFile(new URL("../../platform-operations/src/App.tsx", import.meta.url), "utf8"),
  readFile(new URL("../../platform-operations/src/VenueDetail.tsx", import.meta.url), "utf8")
]);

test("billing presentation stays claim-bound and excludes Platform Operations credentials", () => {
  const billingApi = api.slice(api.indexOf("loadVenueBillingPresentation"), api.indexOf("async function menuRequest"));
  assert.match(billingApi, /api\/back-office\/billing\/presentation/);
  assert.match(billingApi, /X-Vennusign-Back-Office-Token/);
  assert.doesNotMatch(billingApi, /venueId|X-Vennusign-Platform-Operations-Key|stripe/i);
});

test("canonical opportunities and dismissals remain deterministic in Back Office", () => {
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

test("Back Office coordinates one prompt surface and one upgrade sheet", () => {
  assert.match(app, /allowed && !inlineOpportunity && !upgradeContext/);
  assert.match(app, /inlineOpportunity && !upgradeContext/);
  assert.match(app, /<LockedNavigationItem/);
  assert.match(app, /<LockedSectionPreview/);
  assert.match(app, /<UpgradeSheet/);
  assert.match(sidebar, /prefers-reduced-motion: reduce/);
  assert.equal((operations.match(/showUpgradePrompt=\{false\}/g) ?? []).length, 7);
});

test("tier value uses the established ten-month annual rule without changing entitlement", () => {
  assert.match(sheet, /targetTier\.monthlyPrice \* 10/);
  assert.match(sheet, /Annual · two months included/);
  assert.doesNotMatch(sheet, /fetch|effectiveFeatures|enabled\s*=/);
});

test("Platform Operations retains support controls but no customer upgrade orchestration", () => {
  assert.doesNotMatch(platformOperationsApp, /SidebarUpgradeNudge|UpgradeSheet|continueUpgrade/);
  assert.doesNotMatch(platformOperationsVenue, /InlineFeatureHint|LockedSectionPreview|upgradeOpportunity/);
  assert.match(platformOperationsVenue, /Switch tier/);
  assert.match(platformOperationsVenue, /Active overrides/);
  assert.match(platformOperationsVenue, /Open Back Office/);
});
