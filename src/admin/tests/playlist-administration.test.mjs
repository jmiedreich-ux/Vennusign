import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const [component, api, detail] = await Promise.all([
  readFile(new URL("../src/PlaylistAdministration.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/VenueDetail.tsx", import.meta.url), "utf8")
]);

test("playlist administration is screen scoped tier visible and reorderable", () => {
  assert.match(component, /Playlist Rotation requires Pro/);
  assert.match(component, /reorderPlaylist/);
  assert.match(component, /min=\{5\} max=\{120\}/);
  assert.match(component, /menu.*image.*message/s);
  assert.match(api, /screens\/\$\{screenId\}\/playlist/);
  assert.match(detail, /features\.playlist_rotation\?\.enabled/);
});
