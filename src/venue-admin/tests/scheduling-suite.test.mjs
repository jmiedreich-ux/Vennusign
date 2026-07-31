import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const operations = await readFile(new URL("../src/VenueOperations.tsx", import.meta.url), "utf8");
const api = await readFile(new URL("../src/api.ts", import.meta.url), "utf8");

test("venue operations compose the complete scheduling suite", () => {
  for (const component of [
    "MealPeriodAdministration",
    "HappyHourAdministration",
    "PlaylistAdministration",
    "EmergencyBroadcastAdministration",
    "DateRangePromotionAdministration"
  ]) {
    assert.match(operations, new RegExp(`<${component}`));
  }
  for (const route of ["meal-periods", "happy-hour", "playlist", "emergency-broadcasts", "date-range-promotions"]) {
    assert.match(api, new RegExp(route));
  }
});
