import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const source = path => readFile(new URL(`../src/${path}`, import.meta.url), "utf8");
const [theme, screens, api] = await Promise.all([
  source("ThemeBuilder.tsx"),
  source("ScreenManagement.tsx"),
  source("api.ts")
]);

test("Phase 07 Pro controls remain visible behind the established soft lock", () => {
  assert.match(theme, /fieldset disabled=\{!advancedEnabled \|\| busy\}/);
  assert.match(screens, /disabled=\{!allLayoutsEnabled\} value="neon_chalkboard"/);
  assert.match(screens, /disabled=\{!allLayoutsEnabled\} value="split_layout"/);
  assert.match(screens, /disabled=\{!allLayoutsEnabled\} value="daily_special_hero"/);
});

test("advanced themes and screen layouts retain exact player-backed previews", () => {
  assert.match(theme, /preview: "theme"/);
  assert.match(theme, /<iframe/);
  assert.match(screens, /configuration\.displayBaseUrl}\/display\/\$\{screen\.id}/);
  assert.match(screens, /setPreviewRevision\(current => current \+ 1\)/);
});

test("Split and hero administration retain validated persisted contracts", () => {
  assert.match(api, /splitRatio: "40_60" \| "50_50"/);
  assert.match(api, /heroDwellSeconds: number/);
  assert.match(screens, /Every 8 seconds · default/);
  assert.match(screens, /Every 30 seconds/);
});
