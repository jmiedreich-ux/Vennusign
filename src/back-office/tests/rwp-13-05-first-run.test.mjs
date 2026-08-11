import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const [app, shell, api, styles] = await Promise.all([
  readFile(new URL("../src/CustomerOnboardingApp.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/App.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/customerOnboardingApi.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/styles.css", import.meta.url), "utf8")
]);

test("pairing entry is bounded, numeric, explicit, and recoverable", () => {
  assert.match(app, /setPairingCode\(event\.target\.value\.replace\(\/\\D\/g, ""\)\.slice\(0, 6\)\)/);
  assert.match(app, /pairingCode\.length === 6 \? "Code ready to pair"/);
  assert.match(app, /disabled=\{busy === "pairing" \|\| pairingCode\.length !== 6\}/);
  assert.match(app, /autoComplete="one-time-code"/);
  assert.match(app, /request a fresh code; your saved venue is unchanged/);
  assert.match(api, /body: JSON\.stringify\(\{ code \}\)/);
});

test("go-live celebration remains authoritative and first run is actionable", () => {
  assert.match(app, /onboarding\.firstScreenStatus === "online"/);
  assert.match(app, /Confirmed by the player heartbeat/);
  assert.match(app, /Choose a starter menu/);
  assert.match(app, /Your first-run checklist/);
  assert.match(app, /Preview and push to the display/);
  assert.match(app, /paired but offline display does not block venue setup/);
});

test("starter offers prefill a reviewed draft and never create implicitly", () => {
  assert.match(shell, /\["restaurant", "cafe", "bar"\]\.includes\(value\)/);
  // The starter choice used to prefill the old editor's create form. Milestone 3
  // retired that surface, so it prefills the name on Add a menu instead - the
  // choice still ends somewhere rather than being dropped on the floor.
  assert.match(shell, /starterMenuNames/);
  assert.match(shell, /starterMenuName=\{starterMenu\}/);
  assert.doesNotMatch(app, /createMenu\(|createMenuSection\(/);
});

test("first-run surfaces are responsive and reduced-motion safe", () => {
  assert.match(styles, /customer-onboarding__pairing-code/);
  assert.match(styles, /customer-onboarding__starter-menus/);
  assert.match(styles, /@keyframes go-live-arrive/);
  assert.match(styles, /@media \(prefers-reduced-motion: reduce\) \{ \.customer-onboarding__celebration \{ animation: none/);
  assert.match(styles, /customer-onboarding__starter-menus > div:last-child, \.customer-onboarding__next-steps ol \{ grid-template-columns: 1fr/);
});
