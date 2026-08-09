import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";
import { access } from "node:fs/promises";

const api = await readFile(new URL("../src/api.ts", import.meta.url), "utf8");
const screens = await readFile(new URL("../src/ScreenManagement.tsx", import.meta.url), "utf8");
const videoWall = await readFile(new URL("../src/VideoWallBuilder.tsx", import.meta.url), "utf8");

const missing = async path => access(new URL(path, import.meta.url)).then(() => false, () => true);

// Q36: ops has no menu write path this build. Menu content changes are the
// venue's own, made through its draft-and-publish flow; the impersonation-with-
// consent model that will let support act on a venue's behalf is backlogged.
test("platform operations has no menu editing surface", async () => {
  assert.ok(await missing("../src/MenuSectionsEditor.tsx"));
  assert.ok(await missing("../src/MenuItemsEditor.tsx"));
  assert.ok(await missing("../src/QuickUpdateMode.tsx"));
});

test("the ops api client carries no menu write calls", () => {
  assert.doesNotMatch(api, /quick-update\/daily-special/);
  assert.doesNotMatch(api, /quick-availability/);
  assert.doesNotMatch(api, /\/presentation"/);
  assert.doesNotMatch(api, /createMenuItem|updateMenuItem|createMenuSection|updateMenuSection|reorderMenuSections/);
});

test("screen management supports registration health editing and manual push", () => {
  assert.match(screens, /Open registration URL/);
  assert.match(screens, /Last seen/);
  assert.match(screens, /updateManagedScreen/);
  assert.match(screens, /pushManagedScreen/);
  assert.ok(api.includes("api/platform-operations/venues/${venueId}/screens"));
  assert.ok(api.includes("/${screenId}/push"));
});

test("screen targeting supports send-to-all and deterministic overflow guidance", () => {
  assert.match(screens, /Push to all screens/);
  assert.match(screens, /pushAllManagedScreens/);
  assert.match(screens, /Overflow preview/);
  assert.match(screens, /overflowItems/);
  assert.ok(api.includes("/push-all"));
  assert.ok(api.includes("/overflow?capacity=${capacity}"));
});

test("video wall builder is tier visible and limits supported layouts", () => {
  assert.match(videoWall, /Video Wall is a higher-tier feature/);
  assert.match(videoWall, /2 × 1/);
  assert.match(videoWall, /3 × 1/);
  assert.match(videoWall, /2 × 2/);
  assert.match(videoWall, /saveVideoWall/);
  assert.match(videoWall, /removeVideoWall/);
  assert.ok(api.includes("/video-walls"));
});
