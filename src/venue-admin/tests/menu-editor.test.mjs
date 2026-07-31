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

test("venue admin composes the migrated menu and quick-update workflows", () => {
  assert.match(app, /<MenuSectionsEditor/);
  assert.match(sections, /MenuItemsEditor/);
  assert.match(sections, /QuickUpdateMode/);
  assert.match(sections, /reorderMenuSections/);
  assert.match(items, /updateMenuItemPresentation/);
  assert.match(quick, /updateQuickAvailability/);
});

test("menu requests use the venue token and never accept a browser venue route", () => {
  const menuApi = api.slice(api.indexOf("async function menuRequest"), api.indexOf("async function venueOperationRequest"));
  assert.match(menuApi, /api\/venue-admin\/menus/);
  assert.match(menuApi, /X-Vennu-Venue-Token/);
  assert.doesNotMatch(menuApi, /api\/venue-admin\/venues\/\$\{venueId\}/);
  assert.doesNotMatch(menuApi, /X-Vennu-Admin-Key/);
});

test("tier-aware menu affordances remain visible", () => {
  assert.match(items, /capabilities\.happyHour/);
  assert.match(items, /capabilities\.allergenBadges/);
  assert.match(items, /feature-preview/);
  assert.match(quick, /snapshot\.capabilities\.quickUpdate/);
});
