import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const screens = await readFile(new URL("../src/ScreenManagement.tsx", import.meta.url), "utf8");
const styles = await readFile(new URL("../src/styles.css", import.meta.url), "utf8");

test("Split Layout is visible and soft locked without All Layouts", () => {
  assert.match(screens, /disabled=\{!allLayoutsEnabled\} value="split_layout"/);
  assert.match(screens, /Neon Chalkboard and Split Layout remain visible/);
});

test("persists only the established split ratios", () => {
  assert.match(screens, /splitRatio: screen\.splitRatio/);
  assert.match(screens, /value="40_60"/);
  assert.match(screens, /value="50_50"/);
});

test("uses the exact player route for a 16 by 9 preview", () => {
  assert.ok(screens.includes("configuration.displayBaseUrl}/display/${screen.id}"));
  assert.match(screens, /Split Layout TV preview/);
  assert.match(screens, /setPreviewRevision\(current => current \+ 1\)/);
  assert.match(screens, /screen\.splitRatio}/);
  assert.match(screens, /\$\{previewRevision}/);
  assert.match(styles, /\.split-layout-preview iframe/);
  assert.ok(styles.includes("aspect-ratio: 16 / 9"));
});
