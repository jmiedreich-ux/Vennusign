import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

const source = (name) => readFileSync(new URL(`../src/${name}`, import.meta.url), "utf8");

test("secure shell exposes every Phase 04 operator area", () => {
  const app = source("App.tsx");

  assert.match(app, /loadSession/);
  assert.match(app, /type="password"/);
  for (const route of ["dashboard", "venues", "tiers", "features"]) {
    assert.match(app, new RegExp(`path: "${route}"`));
  }
});

test("venue support retains context, override, and tier-switch journeys", () => {
  const detail = source("VenueDetail.tsx");

  for (const capability of [
    "loadVenueSupportDetail",
    "loadFeatureMatrix",
    "saveVenueFeatureOverride",
    "removeVenueFeatureOverride",
    "switchVenueTier"
  ]) {
    assert.match(detail, new RegExp(capability));
  }
});

test("dashboard retains health, live revenue, trend, and event journeys", () => {
  const dashboard = source("OperationalDashboard.tsx");

  for (const capability of [
    "loadOperationalDashboard",
    "loadRevenueSnapshot",
    "loadRevenueTrend",
    "loadOperationalEvents",
    "Screen health map",
    "Recent events"
  ]) {
    assert.match(dashboard, new RegExp(capability));
  }
});

test("tier and feature management remain wired to protected API calls", () => {
  const api = source("api.ts");
  const tierManagement = source("TierManagement.tsx");
  const featureMatrix = source("FeatureMatrix.tsx");

  for (const operation of [
    "saveTier",
    "cloneTier",
    "archiveTier",
    "saveFeatureMatrix"
  ]) {
    assert.match(api, new RegExp(`export async function ${operation}`));
  }
  assert.match(tierManagement, /cloneTier/);
  assert.match(tierManagement, /archiveTier/);
  assert.match(featureMatrix, /saveFeatureMatrix/);
  assert.ok((api.match(/X-Vennusign-Platform-Operations-Key/g) ?? []).length >= 10);
});
