import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const quick = await readFile(new URL("../src/QuickUpdateMode.tsx", import.meta.url), "utf8");
const api = await readFile(new URL("../src/api.ts", import.meta.url), "utf8");

test("quick update retains daily-special and one-scroll availability actions", () => {
  assert.match(quick, /Daily special/);
  assert.match(quick, /updateQuickDailySpecial/);
  assert.match(quick, /updateQuickAvailability/);
  assert.match(quick, /quick-items/);
  assert.match(quick, /Search quick-update items/);
  assert.match(quick, /Select visible/);
  assert.match(quick, /Undo last change/);
  assert.match(quick, /bulkLimit = 25/);
  assert.match(api, /quick-update\/daily-special/);
  assert.match(api, /quick-availability/);
});
