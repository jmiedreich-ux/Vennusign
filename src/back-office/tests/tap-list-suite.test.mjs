import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const [operations, tapList, api] = await Promise.all([
  readFile(new URL("../src/VenueOperations.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/TapListAdministration.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8")
]);

test("venue operations retain tier-visible ordered tap management", () => {
  assert.match(operations, /<TapListAdministration/);
  assert.match(tapList, /reorderTapRows/);
  assert.match(tapList, /isComingSoon/);
  assert.match(api, /tap-list/);
  assert.match(tapList, /Tap description/);
  assert.match(tapList, /Search taps/);
  assert.match(tapList, /bulkLimit = 25/);
  assert.match(tapList, /Retry last change/);
  assert.match(tapList, /window\.confirm/);
  assert.match(tapList, /positions.*overflow/s);
});

test("every operational request uses the venue token", () => {
  assert.match(api, /X-Vennusign-Back-Office-Token/);
  assert.doesNotMatch(api, /X-Vennusign-Platform-Operations-Key/);
});
