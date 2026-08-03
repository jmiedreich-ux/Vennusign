import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const source = await readFile(new URL("../src/ThemeBuilder.tsx", import.meta.url), "utf8");
const api = await readFile(new URL("../src/api.ts", import.meta.url), "utf8");

test("basic theme builder exposes six swatches full colors and three fonts", () => {
  for (const name of ["Ember", "Ocean", "Forest", "Cafe", "Mono", "Plum"]) {
    assert.match(source, new RegExp(`name: "${name}"`));
  }
  assert.match(source, /Background color<input type="color"/);
  assert.match(source, /Accent color<input type="color"/);
  for (const font of ["Inter", "Georgia", "Arial"]) {
    assert.match(source, new RegExp(`value="${font}"`));
  }
});

test("draft values drive player-backed preview and save pushes through theme API", () => {
  assert.match(source, /preview: "theme"/);
  assert.match(source, /<iframe/);
  assert.match(source, /saveVenueTheme/);
  assert.match(source, /pushed to all venue screens/);
  assert.match(api, /\/theme/);
  assert.match(api, /method: "PUT"/);
});
