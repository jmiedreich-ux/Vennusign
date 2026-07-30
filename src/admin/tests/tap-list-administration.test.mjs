import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const [component, api, venue] = await Promise.all([
  readFile(new URL("../src/TapListAdministration.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/VenueDetail.tsx", import.meta.url), "utf8")
]);

test("tap administration is venue scoped tier visible and exact reorderable", () => {
  assert.ok(api.includes("/api/admin/venues/${venueId}/tap-list"));
  assert.match(component, /Tap List controls remain visible/);
  assert.match(component, /reorderTapRows/);
  assert.match(component, /Now brewing/);
  assert.match(component, /Glass color/);
  assert.match(component, /category price/);
  assert.match(component, /patchCategory/);
  assert.match(venue, /features\.all_layouts/);
});

test("screen management exposes the Classic Chalkboard exact player preview", async () => {
  const [screen, api] = await Promise.all([
    readFile(new URL("../src/ScreenManagement.tsx", import.meta.url), "utf8"),
    readFile(new URL("../src/api.ts", import.meta.url), "utf8")
  ]);
  assert.match(screen, /value="classic_chalkboard"/);
  assert.match(screen, /Classic Chalkboard.*TV preview/);
  assert.match(api, /\| "classic_chalkboard"/);
});
