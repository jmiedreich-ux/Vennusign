import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

test("Platform Operations initializes and exposes the shared admin theme contract", async () => {
  const main = await readFile(new URL("../src/main.tsx", import.meta.url), "utf8");
  const toggle = await readFile(new URL("../src/AdminThemeToggle.tsx", import.meta.url), "utf8");
  const styles = await readFile(new URL("../src/styles.css", import.meta.url), "utf8");
  assert.match(main, /initializeAdminTheme\(\)/);
  assert.match(main, /<AdminThemeToggle \/>/);
  assert.match(toggle, /\.\.\/\.\.\/back-office\/src\/adminTheme\.mjs/);
  assert.match(toggle, /aria-pressed=\{midnight\}/);
  assert.match(styles, /\.admin-theme-toggle:focus-visible/);
});
