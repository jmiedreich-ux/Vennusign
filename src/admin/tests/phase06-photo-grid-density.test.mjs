import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const screenSource = await readFile(new URL("../src/ScreenManagement.tsx", import.meta.url), "utf8");
const apiSource = await readFile(new URL("../src/api.ts", import.meta.url), "utf8");

test("screen management persists all Photo Grid density modes", () => {
  for (const density of ["2x2", "3x2", "4x2", "3x3"]) {
    assert.match(screenSource, new RegExp(`value="${density}"`));
  }
  assert.match(screenSource, /photoGridDensity: screen\.photoGridDensity/);
  assert.match(apiSource, /photoGridDensity: "2x2" \| "3x2" \| "4x2" \| "3x3"/);
});
