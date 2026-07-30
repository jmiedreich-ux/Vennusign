import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const [component, api, detail] = await Promise.all([
  readFile(new URL("../src/HappyHourAdministration.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/VenueDetail.tsx", import.meta.url), "utf8")
]);

test("happy-hour administration stays visible and soft locks without entitlement", () => {
  assert.match(component, /Happy Hour requires Pro/);
  assert.match(component, /disabled=\{!enabled \|\| busy\}/);
  assert.match(detail, /features\.happy_hour\?\.enabled/);
});

test("happy-hour controls expose time days enablement and all override modes", () => {
  assert.match(component, /type="time"/);
  assert.match(component, /activeDaysMask/);
  assert.match(component, /force_on/);
  assert.match(component, /force_off/);
  assert.match(api, /venues\/\$\{venueId\}\/happy-hour/);
});
