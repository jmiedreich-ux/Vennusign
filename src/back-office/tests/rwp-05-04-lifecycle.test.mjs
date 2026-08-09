import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const source = name => readFile(new URL(`../src/${name}`, import.meta.url), "utf8");
const [app, navigation, pos, sections, items, quick, api] = await Promise.all([
  source("App.tsx"), source("navigation.mjs"), source("PosIntegrationAdministration.tsx"),
  source("MenuSectionsEditor.tsx"), source("MenuItemsEditor.tsx"), source("QuickUpdateMode.tsx"), source("api.ts")
]);

test("navigation exposes only implemented home, account, and POS administration", () => {
  assert.match(navigation, /path: "home"/);
  assert.doesNotMatch(navigation, /path: "settings"/);
  assert.match(app, /DaypartHome/);
  assert.match(navigation, /path: "pos"/);
  assert.match(navigation, /capabilityId: "content\.source\.synchronize"/);
  assert.match(app, /PosIntegrationAdministration/);
  assert.match(pos, /Refresh status/);
  assert.match(pos, /Import catalog/);
  assert.match(pos, /role="alert"/);
});

test("menu lifecycle is selectable recoverable ordered and explicitly saved", () => {
  assert.match(sections, /Select menu/);
  assert.match(sections, /Create menu/);
  assert.match(sections, /Confirm archive/);
  assert.match(sections, /Retry last change/);
  assert.match(items, /Unsaved draft/);
  assert.match(items, /Save failed/);
  assert.match(items, /reorderMenuItems/);
  assert.match(api, /items\/order/);
  // Per-item archive left with the consolidation: removing an item from a board
  // is a placement change, and that arrives with the milestone 3 builder.
  assert.doesNotMatch(items, /Confirm archive/);
  assert.doesNotMatch(api, /items\/\$\{itemId\}\/lifecycle/);
});

test("Quick Update bounds selection and retains recovery feedback", () => {
  assert.match(quick, /bulkLimit = 25/);
  assert.match(quick, /Search quick-update items/);
  assert.match(quick, /Undo last change/);
  assert.match(quick, /Refresh to verify current item state/);
});
