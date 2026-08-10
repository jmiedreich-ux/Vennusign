import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const source = name => readFile(new URL(`../src/${name}`, import.meta.url), "utf8");
const [app, navigation, pos] = await Promise.all([
  source("App.tsx"), source("navigation.mjs"), source("PosIntegrationAdministration.tsx")
]);

/*
 * The two menu-lifecycle tests that used to live here asserted the pre-milestone-3
 * editor: a menu picker, a Create menu form, per-row Save buttons, "Confirm
 * archive" and the Quick Update bulk selector. Milestone 3 replaced that surface
 * with the builder, so they were retired here rather than left asserting files
 * that no longer exist. What they were protecting is asserted against the
 * replacement in menu-builder.test.mjs and tests/ui/specs/menu-builder.spec.ts —
 * including the one rule worth keeping verbatim, that an edit in flight is never
 * overwritten by an older save completing late.
 */

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
