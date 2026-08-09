import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const quick = await readFile(new URL("../src/QuickUpdateMode.tsx", import.meta.url), "utf8");
const api = await readFile(new URL("../src/api.ts", import.meta.url), "utf8");

test("quick update is one-scroll availability, with the killed concepts gone", () => {
  assert.match(quick, /updateQuickAvailability/);
  assert.match(quick, /quick-items/);
  assert.match(quick, /Search quick-update items/);
  assert.match(quick, /Select visible/);
  assert.match(quick, /Undo last change/);
  assert.match(quick, /bulkLimit = 25/);
  assert.match(api, /quick-availability/);
  // Daily special is an owner-killed concept and the auto-reset never happens:
  // an item stays off until a person turns it back on (decision 14).
  assert.doesNotMatch(quick, /Daily special/);
  assert.doesNotMatch(quick, /midnight/);
  assert.doesNotMatch(api, /quick-update\/daily-special/);
});
