import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const component = await readFile(new URL("../src/MenuSectionsEditor.tsx", import.meta.url), "utf8");
const items = await readFile(new URL("../src/MenuItemsEditor.tsx", import.meta.url), "utf8");
const api = await readFile(new URL("../src/api.ts", import.meta.url), "utf8");
const quick = await readFile(new URL("../src/QuickUpdateMode.tsx", import.meta.url), "utf8");
const screens = await readFile(new URL("../src/ScreenManagement.tsx", import.meta.url), "utf8");

test("section journeys remain venue scoped", () => {
  assert.ok(api.includes("api/admin/venues/${venueId}/menus"));
  assert.match(component, /createMenuSection/);
  assert.match(component, /updateMenuSection/);
  assert.match(component, /reorderMenuSections/);
});

test("inline item editing uses venue scoped create and update contracts", () => {
  assert.match(items, /createMenuItem/);
  assert.match(items, /updateMenuItem/);
  assert.match(items, /onBlur=\{\(\) => save\(item\)\}/);
  assert.ok(api.includes("/sections/${sectionId}/items"));
});

test("availability quantity and badges use the presentation contract", () => {
  assert.match(items, /updateMenuItemPresentation/);
  assert.match(items, /Quantity available/);
  assert.match(items, /Dietary \/ allergen tags/);
  assert.match(items, /Bestseller/);
  assert.ok(api.includes("/presentation"));
});

test("tier-aware controls stay visible and use one dismissible prompt", () => {
  assert.match(component, /tierPrompt \?/);
  assert.match(component, /Dismiss tier prompt/);
  assert.match(items, /capabilities\.happyHour/);
  assert.match(items, /capabilities\.allergenBadges/);
  assert.match(items, /feature-preview/);
});

test("collapsed state persists per venue", () => {
  assert.ok(component.includes("localStorage.getItem(storageKey)"));
  assert.ok(component.includes("localStorage.setItem(storageKey"));
});

test("quick update provides daily special and one-scroll availability controls", () => {
  assert.match(quick, /Daily special/);
  assert.match(quick, /updateQuickDailySpecial/);
  assert.match(quick, /updateQuickAvailability/);
  assert.match(quick, /snapshot\.capabilities\.quickUpdate/);
  assert.match(quick, /quick-items/);
  assert.ok(api.includes("/quick-update/daily-special"));
  assert.ok(api.includes("/quick-availability"));
});

test("screen management supports registration health editing and manual push", () => {
  assert.match(screens, /Open registration URL/);
  assert.match(screens, /Last seen/);
  assert.match(screens, /updateManagedScreen/);
  assert.match(screens, /pushManagedScreen/);
  assert.ok(api.includes("api/admin/venues/${venueId}/screens"));
  assert.ok(api.includes("/${screenId}/push"));
});
