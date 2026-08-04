import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const root = new URL("../", import.meta.url);

test("Back Office applies the Sky hierarchy and accessible interaction contract", async () => {
  const tokens = await readFile(new URL("src/sky-ui-tokens.css", root), "utf8");
  const styles = await readFile(new URL("src/styles.css", root), "utf8");

  assert.match(tokens, /--sky-sidebar-background:/);
  assert.match(tokens, /--sky-badge-background:/);
  assert.match(tokens, /--sky-panel-shadow:/);
  assert.match(styles, /body, \.shell > main, \.customer-entry \{ background: var\(--sky-page-gradient\)/);
  assert.match(styles, /:where\(button, a, input, select, textarea, summary\):focus-visible/);
  assert.match(styles, /color: var\(--sky-action-primary-text\); background: var\(--sky-action-primary-background\)/);
  assert.match(styles, /prefers-reduced-motion: reduce/);
});
