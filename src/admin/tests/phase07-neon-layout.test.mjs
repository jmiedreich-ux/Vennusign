import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const screens = await readFile(new URL("../src/ScreenManagement.tsx", import.meta.url), "utf8");
const detail = await readFile(new URL("../src/VenueDetail.tsx", import.meta.url), "utf8");
const api = await readFile(new URL("../src/api.ts", import.meta.url), "utf8");

test("Neon Chalkboard is visible but soft locked without All Layouts", () => {
  assert.match(detail, /allLayoutsEnabled=\{detail\.features\.all_layouts\?\.enabled/);
  assert.match(screens, /Bar layouts require All Layouts/);
  assert.match(screens, /disabled=\{!allLayoutsEnabled\} value="neon_chalkboard"/);
  assert.match(api, /"neon_chalkboard"/);
});
