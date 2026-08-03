import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const root = new URL("../", import.meta.url);

test("Sky UI exposes the locked shared palette and foundations", async () => {
  const tokens = await readFile(new URL("src/sky-ui-tokens.css", root), "utf8");

  for (const [name, value] of Object.entries({
    "--sky-color-primary": "#87ceeb",
    "--sky-color-surface": "#f8fafc",
    "--sky-color-ink": "#0f172a",
    "--sky-color-secondary": "#e0f2fe",
    "--sky-color-border": "#e2e8f0",
    "--sky-color-live": "#178a52",
    "--sky-color-off": "#b03a33",
    "--sky-color-warning": "#c9871a",
    "--sky-color-emergency": "#c22e26",
    "--sky-color-promotion": "#7c5cbf"
  })) {
    assert.match(tokens, new RegExp(`${name}:\\s*${value}`, "i"));
  }

  assert.match(tokens, /--sky-action-primary-text:\s*var\(--sky-color-ink\)/);
  assert.doesNotMatch(tokens, /--sky-action-primary-text:\s*(?:#fff(?:fff)?|white)\b/i);
  assert.match(tokens, /--sky-focus-ring:/);
  assert.match(tokens, /--sky-space-7:/);
  assert.match(tokens, /--sky-radius-pill:/);
  assert.match(tokens, /--sky-font-family:/);
});

test("Back Office consumes the shared token source", async () => {
  const styles = await readFile(new URL("src/styles.css", root), "utf8");

  assert.match(styles, /^@import "\.\/sky-ui-tokens\.css";/);
  assert.match(styles, /color:\s*var\(--sky-text-primary\)/);
  assert.match(styles, /background:\s*var\(--sky-page-background\)/);
  assert.match(styles, /font-family:\s*var\(--sky-font-family\)/);
});
