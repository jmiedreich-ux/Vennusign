import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const source = await readFile(new URL("../src/ThemeBuilder.tsx", import.meta.url), "utf8");
const detail = await readFile(new URL("../src/VenueDetail.tsx", import.meta.url), "utf8");
const api = await readFile(new URL("../src/api.ts", import.meta.url), "utf8");

test("full theme controls use the established all-layouts soft lock", () => {
  assert.match(detail, /Open venue operations/);
  assert.match(source, /Full themes require All Layouts/);
  assert.match(source, /fieldset disabled=\{!advancedEnabled \|\| busy\}/);
  assert.match(source, /Upgrade to Pro or add a venue override/);
});

test("presets and advanced values use their protected API operations", () => {
  assert.match(source, /loadVenueThemePresets/);
  assert.match(source, /applyVenueThemePreset/);
  assert.match(source, /saveAdvancedVenueTheme/);
  assert.match(api, /\/presets/);
  assert.match(api, /\/advanced/);
  assert.match(source, /sectionColors\.length === 4/);
  assert.match(source, /min="0\.2" max="2" step="0\.05"/);
});

test("the exact player preview receives the complete advanced draft", () => {
  for (const key of ["preset", "title", "glow", "board", "sections", "intensity", "titleFont", "itemFont"]) {
    assert.match(source, new RegExp(`${key}:`));
  }
  assert.match(source, /<iframe/);
});
