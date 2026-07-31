import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const [operations, screens, walls, api] = await Promise.all([
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
  assert.match(api, /api\/venue-admin\/screens\/pairing/);
});
