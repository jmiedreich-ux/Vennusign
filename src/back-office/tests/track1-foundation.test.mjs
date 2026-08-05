import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";
import { backOfficeRoutes } from "../src/navigation.mjs";

const [app, api, screens, sessionController] = await Promise.all([
  readFile(new URL("../src/App.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/ScreenManagement.tsx", import.meta.url), "utf8"),
  readFile(new URL("../../Vennu.Api/Controllers/BackOffice/BackOfficeSessionController.cs", import.meta.url), "utf8")
]);

test("Track 1 navigation and screen capacity consume canonical server decisions", () => {
  for (const route of backOfficeRoutes.filter(route => route.capabilityId)) {
    assert.match(route.capabilityId, /^[a-z][a-z0-9_]*\.[a-z][a-z0-9_]*\.[a-z][a-z0-9_]*$/);
  }

  assert.match(app, /decisions=\{session\.capabilityDecisions\}/);
  assert.match(screens, /pairDecision\?\.reasonCode === "allowance\.reached"/);
  assert.doesNotMatch(app, /maxScreens=\{billing\?\.currentTier\?\.maxScreens\}/);
  assert.doesNotMatch(screens, /activeScreens\.length >= maxScreens/);
});

test("Track 1 session projection preserves structured reasons and conditions", () => {
  assert.match(api, /parameters: Record<string, string>/);
  assert.match(api, /conditions: Array</);
  assert.match(api, /messageKey: string/);
  assert.match(api, /correlationId: string/);
  assert.match(api, /locale: string/);
  assert.match(sessionController, /decision\.Parameters/);
  assert.match(sessionController, /decision\.CorrelationId/);
  assert.match(sessionController, /decision\.Locale/);
  assert.match(sessionController, /decision\.Conditions\.Select/);
  assert.match(sessionController, /condition\.Parameters/);
});

test("Track 1 customer copy contains no migration promise", () => {
  assert.match(app, /Use configured venue access/);
  assert.doesNotMatch(app, /temporary legacy venue link|during migration|next migration package/);
});
