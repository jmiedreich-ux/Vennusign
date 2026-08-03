import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";
import { buildTierSwitchImpact, summarizeFeatureMatrixImpact } from "../src/operatorSafety.mjs";

const source = name => readFileSync(new URL(`../src/${name}`, import.meta.url), "utf8");

test("tier switch impact identifies screen-limit and entitlement losses", () => {
  const detail = { tier: { name: "Pro" }, screens: [{}, {}, {}], features: { analytics: { key: "analytics", enabled: true }, menu: { key: "menu", enabled: true } } };
  const snapshot = { tiers: [{ id: "starter", name: "Starter", maxScreens: 1 }], features: [{ id: "menu-id", key: "menu" }], enabledFeatures: [{ tierId: "starter", featureId: "menu-id" }] };
  const impact = buildTierSwitchImpact(detail, snapshot, "starter");
  assert.equal(impact.screenLimitExceeded, true);
  assert.deepEqual(impact.disabled, ["analytics"]);
});

test("bulk entitlement impact summarizes affected tiers and directions", () => {
  const changes = [{ tierId: "a", featureId: "1", enabled: true }, { tierId: "a", featureId: "2", enabled: false }, { tierId: "b", featureId: "2", enabled: false }];
  const impact = summarizeFeatureMatrixImpact(changes, { tiers: [{ id: "a", name: "A" }, { id: "b", name: "B" }] });
  assert.deepEqual(impact, { changedCount: 3, tierCount: 2, enabledCount: 1, disabledCount: 2, tierNames: ["A", "B"] });
});

test("operator UI exposes recovery, drill-down, screen support, status, and impact review", () => {
  const dashboard = source("OperationalDashboard.tsx");
  const directory = source("VenueDirectory.tsx");
  const detail = source("VenueDetail.tsx");
  const matrix = source("FeatureMatrix.tsx");
  const tiers = source("TierManagement.tsx");
  assert.match(dashboard, /Refresh dashboard/); assert.match(dashboard, /onOpenVenues/); assert.match(dashboard, /Retry events/);
  assert.match(directory, /Clear filters/); assert.match(directory, /Retry directory/); assert.match(directory, /role="status"/);
  assert.match(detail, /Confirm support impact/); assert.match(detail, /Screens \(/); assert.match(detail, /Review Stripe tier change/);
  assert.match(matrix, /Bulk entitlement review/); assert.match(matrix, /Confirm entitlement changes/);
  assert.match(tiers, /Tier catalog review/); assert.match(tiers, /Confirm tier action/);
});
