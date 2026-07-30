import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const source = name => readFile(new URL(`../src/${name}`, import.meta.url), "utf8");
const [venue, menus, items, quick, screens, walls, api] = await Promise.all([
  source("VenueDetail.tsx"),
  source("MenuSectionsEditor.tsx"),
  source("MenuItemsEditor.tsx"),
  source("QuickUpdateMode.tsx"),
  source("ScreenManagement.tsx"),
  source("VideoWallBuilder.tsx"),
  source("api.ts")
]);

test("venue board management composes menu and screen journeys", () => {
  assert.match(venue, /MenuSectionsEditor/);
  assert.match(venue, /ScreenManagement/);
  assert.match(menus, /MenuItemsEditor/);
  assert.match(menus, /QuickUpdateMode/);
  assert.match(screens, /VideoWallBuilder/);
});

test("menu journey retains ordered editing presentation and quick update", () => {
  assert.match(menus, /reorderMenuSections/);
  assert.match(items, /updateMenuItemPresentation/);
  assert.match(items, /onBlur=\{\(\) => save\(item\)\}/);
  assert.match(quick, /updateQuickDailySpecial/);
  assert.match(quick, /updateQuickAvailability/);
});

test("screen journey retains one all overflow and wall targets", () => {
  assert.match(screens, /pushManagedScreen/);
  assert.match(screens, /pushAllManagedScreens/);
  assert.match(screens, /loadScreenOverflow/);
  assert.match(walls, /saveVideoWall/);
  assert.match(walls, /removeVideoWall/);
});

test("tier-aware controls remain visible and use effective API capabilities", () => {
  assert.match(items, /feature-preview/);
  assert.match(menus, /tier-prompt/);
  assert.match(walls, /Video Wall is a higher-tier feature/);
  assert.ok(api.includes("/quick-update/daily-special"));
  assert.ok(api.includes("/video-walls"));
});
