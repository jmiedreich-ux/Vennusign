import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const [component, api, detail] = await Promise.all([
  readFile(new URL("../src/MealPeriodAdministration.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/VenueDetail.tsx", import.meta.url), "utf8")
]);

test("meal-period administration exposes venue-local day, time, enablement, and conflict guidance", () => {
  assert.match(component, /type="time"/);
  assert.match(component, /activeDaysMask/);
  assert.match(component, /Overlapping periods/);
  assert.match(component, /Enabled/);
  assert.match(component, /Target layout/);
  assert.match(component, /Menu filter/);
  assert.match(component, /Theme preset/);
});

test("meal-period CRUD uses the protected venue-scoped admin route", () => {
  assert.match(api, /venues\/\$\{venueId\}\/meal-periods/);
  assert.match(api, /method: "POST"/);
  assert.match(api, /method: "PUT"/);
  assert.match(api, /method: "DELETE"/);
  assert.match(detail, /<MealPeriodAdministration/);
});
