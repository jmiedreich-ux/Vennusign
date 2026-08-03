import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const [app, view, api, styles, controller] = await Promise.all([
  readFile(new URL("../src/App.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/OnboardingSupport.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/styles.css", import.meta.url), "utf8"),
  readFile(new URL("../../Vennu.Api/Controllers/PlatformOperations/PlatformOperationsOnboardingController.cs", import.meta.url), "utf8")
]);

test("Platform Operations onboarding support is protected and read-only", () => {
  assert.match(controller, /PlatformOperationsAuthenticationDefaults\.AuthorizationPolicy/);
  assert.match(controller, /\[HttpGet\]/);
  assert.doesNotMatch(controller, /HttpPost|HttpPut|HttpDelete|HttpPatch/);
  assert.match(api, /X-Vennusign-Platform-Operations-Key/);
  assert.match(app, /path: "onboarding"/);
  assert.doesNotMatch(view, /entitlement.*(?:save|update)|subscription.*(?:save|update)|pair.*(?:save|update)/i);
});

test("support timeline exposes essential states and safe actions", () => {
  for (const step of ["Account", "Plan", "Venue", "First Screen", "Go Live"]) assert.match(view, new RegExp(step));
  for (const state of ["Complete", "Current", "Upcoming"]) assert.match(view, new RegExp(state));
  assert.match(view, /aria-current=\{current \? "step"/);
  assert.match(view, /Loading onboarding journeys/);
  assert.match(view, /No onboarding journeys match this search/);
  assert.match(view, /role="alert"/);
  assert.match(view, /Copy support context/);
  assert.match(view, /Stale \(over 7 days\)/);
  assert.match(styles, /@media \(max-width: 560px\)/);
  assert.match(styles, /:focus-visible/);
});
