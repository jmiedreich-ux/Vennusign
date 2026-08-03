import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const root = new URL("../", import.meta.url);

test("Platform Operations consumes the same Sky UI token source", async () => {
  const styles = await readFile(new URL("src/styles.css", root), "utf8");
  const sharedTokens = await readFile(new URL("../back-office/src/sky-ui-tokens.css", root), "utf8");

  assert.match(styles, /^@import "\.\.\/\.\.\/back-office\/src\/sky-ui-tokens\.css";/);
  assert.match(styles, /color:\s*var\(--sky-text-primary\)/);
  assert.match(styles, /background:\s*var\(--sky-page-background\)/);
  assert.match(styles, /font-family:\s*var\(--sky-font-family\)/);
  assert.match(sharedTokens, /--sky-action-primary-background:\s*var\(--sky-color-primary\)/);
  assert.match(sharedTokens, /--sky-action-primary-text:\s*var\(--sky-color-ink\)/);
});
