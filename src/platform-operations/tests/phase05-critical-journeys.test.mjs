import assert from "node:assert/strict";
import { readFile, access } from "node:fs/promises";
import test from "node:test";

const source = name => readFile(new URL(`../src/${name}`, import.meta.url), "utf8");
const missing = name => access(new URL(`../src/${name}`, import.meta.url)).then(() => false, () => true);
const [venue, screens, walls, api] = await Promise.all([
  source("VenueDetail.tsx"),
  source("ScreenManagement.tsx"),
  source("VideoWallBuilder.tsx"),
  source("api.ts")
]);

test("venue board management hands customer workflows to Back Office", () => {
  assert.match(venue, /Open Back Office/);
  assert.doesNotMatch(venue, /<MenuSectionsEditor/);
  assert.match(venue, /Open venue operations/);
  assert.doesNotMatch(venue, /<ScreenManagement/);
  assert.match(screens, /VideoWallBuilder/);
});

// Q36: menu content is the venue's to change; ops can look, not touch.
test("the retired ops menu editing journey stays retired", async () => {
  assert.ok(await missing("MenuSectionsEditor.tsx"));
  assert.doesNotMatch(api, /quick-update\/daily-special/);
  assert.doesNotMatch(api, /\/menus\$\{path\}.*method: "PUT"/s);
});

test("screen journey retains one all overflow and wall targets", () => {
  assert.match(screens, /pushManagedScreen/);
  assert.match(screens, /pushAllManagedScreens/);
  assert.match(screens, /loadScreenOverflow/);
  assert.match(walls, /saveVideoWall/);
  assert.match(walls, /removeVideoWall/);
});

test("tier-aware controls remain visible and use effective API capabilities", () => {
  assert.match(walls, /Video Wall is a higher-tier feature/);
  assert.ok(api.includes("/video-walls"));
});
