import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";
import { adminThemeStorageKey, applyAdminTheme, initializeAdminTheme, normalizeAdminTheme } from "../src/adminTheme.mjs";

test("Midnight preference is normalized, persisted, and applied to the document root", () => {
  const values = new Map([[adminThemeStorageKey, "midnight"]]);
  const storage = { getItem: key => values.get(key) ?? null, setItem: (key, value) => values.set(key, value) };
  const root = { dataset: {} };
  assert.equal(initializeAdminTheme(root, storage), "midnight");
  assert.equal(root.dataset.skyTheme, "midnight");
  assert.equal(applyAdminTheme("sky", root, storage), "sky");
  assert.equal(values.get(adminThemeStorageKey), "sky");
  assert.equal(normalizeAdminTheme("unknown"), "sky");
});

test("shared tokens provide an explicit high-contrast Midnight palette", async () => {
  const tokens = await readFile(new URL("../src/sky-ui-tokens.css", import.meta.url), "utf8");
  assert.match(tokens, /:root\[data-sky-theme="midnight"\]/);
  assert.match(tokens, /color-scheme:\s*dark/);
  assert.match(tokens, /--sky-color-surface:\s*#111827/i);
  assert.match(tokens, /--sky-color-ink:\s*#f8fafc/i);
});

test("Back Office initializes its stored theme without exposing a global theme control", async () => {
  const main = await readFile(new URL("../src/main.tsx", import.meta.url), "utf8");
  assert.match(main, /initializeAdminTheme\(\)/);
  assert.doesNotMatch(main, /AdminThemeToggle|Use Midnight theme/);
});
