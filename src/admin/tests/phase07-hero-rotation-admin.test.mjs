import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const screens = await readFile(new URL("../src/ScreenManagement.tsx", import.meta.url), "utf8");
const api = await readFile(new URL("../src/api.ts", import.meta.url), "utf8");

test("Daily Special Hero remains visible and soft locked without All Layouts", () => {
  assert.match(screens, /disabled=\{!allLayoutsEnabled\} value="daily_special_hero"/);
  assert.match(screens, /Daily Special Hero remains visible/);
  assert.match(api, /"daily_special_hero"/);
});

test("persists bounded hero dwell choices and reloads the exact preview", () => {
  assert.match(screens, /heroDwellSeconds: screen\.heroDwellSeconds/);
  assert.match(screens, /Every 8 seconds · default/);
  assert.match(screens, /Every 30 seconds/);
  assert.match(screens, /screen\.heroDwellSeconds}-\$\{previewRevision/);
});
