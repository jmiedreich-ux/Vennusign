import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const component = await readFile(new URL("../src/DateRangePromotionAdministration.tsx", import.meta.url), "utf8");
const api = await readFile(new URL("../src/api.ts", import.meta.url), "utf8");

test("date-range promotion administration remains venue scoped and tier visible", () => {
  assert.ok(api.includes("/api/platform-operations/venues/${venueId}/date-range-promotions"));
  assert.match(component, /Promotion scheduling is visible as a preview/);
  assert.match(component, /min=\{draft\.startLocalDate/);
  assert.match(component, /Promotion priority/);
  assert.match(component, /Promotion body/);
  assert.match(component, /split_layout/);
  assert.match(component, /Save promotion/);
  assert.match(component, /Archive/);
});
