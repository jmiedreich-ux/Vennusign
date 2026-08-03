import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import test from "node:test";

const dashboard = readFileSync(new URL("../src/OperationalDashboard.tsx", import.meta.url), "utf8");
const api = readFileSync(new URL("../src/api.ts", import.meta.url), "utf8");

test("fleet health exposes current, outdated, and unknown app versions", () => {
  assert.match(api, /outdatedScreens/);
  assert.match(api, /desiredAppVersion/);
  assert.match(api, /versionStatus/);
  assert.match(dashboard, /Screens outdated/);
  assert.match(dashboard, /Update \$\{screen\.appVersion/);
});
