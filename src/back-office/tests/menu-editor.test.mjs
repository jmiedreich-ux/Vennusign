import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const source = name => readFile(new URL(`../src/${name}`, import.meta.url), "utf8");
const [app, sections, items, quick, api] = await Promise.all([
  source("App.tsx"),
  source("MenuSectionsEditor.tsx"),
  source("MenuItemsEditor.tsx"),
  source("QuickUpdateMode.tsx"),
  source("api.ts")
]);

test("Back Office composes the migrated menu and quick-update workflows", () => {
  assert.match(app, /<MenuSectionsEditor/);
  assert.match(sections, /MenuItemsEditor/);
  assert.match(sections, /QuickUpdateMode/);
  assert.match(sections, /reorderMenuSections/);
  assert.match(sections, /createMenu/);
  assert.match(sections, /Confirm archive/);
  assert.match(items, /updateMenuItemPresentation/);
  assert.match(items, /reorderMenuItems/);
  assert.match(items, /updateMenuItemLifecycle/);
  assert.match(items, /Save failed/);
  assert.match(quick, /updateQuickAvailability/);
});

test("menu requests use the venue token and never accept a browser venue route", () => {
  const menuApi = api.slice(api.indexOf("async function menuRequest"), api.indexOf("async function venueOperationRequest"));
  assert.match(menuApi, /api\/back-office\/menus/);
  assert.match(menuApi, /X-Vennusign-Back-Office-Token/);
  assert.doesNotMatch(menuApi, /api\/back-office\/venues\/\$\{venueId\}/);
  assert.doesNotMatch(menuApi, /X-Vennusign-Platform-Operations-Key/);
});

test("tier-aware menu affordances remain visible", () => {
  assert.match(items, /capabilities\.happyHour/);
  assert.match(items, /capabilities\.allergenBadges/);
  assert.match(items, /feature-preview/);
  assert.match(quick, /snapshot\.capabilities\.quickUpdate/);
});
