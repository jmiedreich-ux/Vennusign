import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const [component, api, detail] = await Promise.all([
  readFile(new URL("../src/EmergencyBroadcastAdministration.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/VenueDetail.tsx", import.meta.url), "utf8")
]);

test("broadcast administration is tier visible targetable bounded and cancellable", () => {
  assert.match(component, /Emergency Broadcast requires Pro/);
  assert.match(component, /All venue screens/);
  assert.match(component, /min=\{1\} max=\{1440\}/);
  assert.match(component, /cancelEmergencyBroadcast/);
  assert.match(api, /emergency-broadcasts/);
  assert.match(detail, /features\.emergency_broadcast\?\.enabled/);
});
