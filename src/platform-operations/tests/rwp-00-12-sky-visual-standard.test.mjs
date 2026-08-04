import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const root = new URL("../", import.meta.url);

test("Platform Operations applies the shared Sky visual standard", async () => {
  const tokens = await readFile(new URL("../back-office/src/sky-ui-tokens.css", root), "utf8");
  const styles = await readFile(new URL("src/styles.css", root), "utf8");

  assert.match(tokens, /--sky-badge-text:/);
  assert.match(styles, /body, \.shell > main \{ background: var\(--sky-page-gradient\)/);
  assert.match(styles, /aside \{ color: var\(--sky-sidebar-text\); background: var\(--sky-sidebar-background\)/);
  assert.match(styles, /:where\(\.environment, \.health, \.event-type, \.upgrade-tier-badge\)/);
  assert.match(styles, /\.health\.online \{ color: var\(--sky-positive-text\)/);
  assert.match(styles, /prefers-reduced-motion: reduce/);
});
