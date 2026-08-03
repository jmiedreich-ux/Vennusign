import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const [app, operations, screens, walls, api] = await Promise.all([
  readFile(new URL("../src/App.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/VenueOperations.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/ScreenManagement.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/VideoWallBuilder.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8")
]);

test("venue operations compose screen targeting pairing and video walls", () => {
  assert.match(operations, /<ScreenManagement/);
  assert.match(screens, /claimPairingCode/);
  assert.match(screens, /pushAllManagedScreens/);
  assert.match(screens, /loadScreenOverflow/);
  assert.match(walls, /saveVideoWall/);
  assert.match(api, /api\/back-office\/screens\/pairing/);
});

test("screen creation and pairing expose the subscribed screen quota", () => {
  assert.match(app, /maxScreens=\{billing\?\.currentTier\?\.maxScreens\}/);
  assert.match(operations, /maxScreens=\{maxScreens\}/);
  assert.match(screens, /activeScreens\.length >= maxScreens/);
  assert.match(screens, /busyId === "new" \|\| screenLimitReached/);
  assert.match(screens, /busyId === "pair" \|\| screenLimitReached/);
  assert.match(screens, /reason\.status === 409/);
  assert.match(screens, /Plan limit reached/);
});

test("screen lifecycle recovery is explicit safe and capacity-aware", () => {
  assert.match(screens, /setManagedScreenArchived/);
  assert.match(screens, /resetManagedScreen/);
  assert.match(screens, /unpairManagedScreen/);
  assert.match(screens, /window\.confirm/);
  assert.match(screens, /healthFilter/);
  assert.match(screens, /expired/);
  assert.match(screens, /already claimed/);
  assert.match(api, /setManagedScreenArchived/);
  assert.match(api, /unpairManagedScreen/);
});

test("video wall editing and removal require deliberate recovery-safe actions", () => {
  assert.match(walls, /editingName/);
  assert.match(walls, /Edit wall/);
  assert.match(walls, /window\.confirm/);
  assert.match(walls, /Cancel edit/);
});

test("video wall builder follows the effective video_wall capability", () => {
  assert.match(operations, /capabilities\.includes\("video_wall"\)/);
  assert.match(screens, /videoWallEnabled \? <VideoWallBuilder/);
});
