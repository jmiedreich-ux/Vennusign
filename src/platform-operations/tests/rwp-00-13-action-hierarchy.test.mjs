import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const [theme, configuration, styles] = await Promise.all([
  readFile(new URL("../src/ThemeBuilder.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/SystemConfiguration.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/styles.css", import.meta.url), "utf8")
]);

test("Platform Operations uses one primary and overflow secondary/destructive actions", () => {
  assert.match(configuration, /configuration-actions action-surface/);
  assert.match(configuration, /className="action-primary"/);
  assert.match(configuration, /className="action-overflow"/);
  assert.match(configuration, /className="action-danger"/);
  assert.match(configuration, />Clear value<\/button>/);
  assert.match(configuration, /className="action-secondary"/);
  assert.match(configuration, />View history<\/button>/);
});

test("long configuration and theme actions stay reachable with reversible theme apply", () => {
  assert.match(configuration, /configuration-actions sticky-action-bar/);
  assert.match(theme, /sticky-action-bar/);
  assert.match(theme, /Undo applied theme/);
  assert.match(theme, /setUndoTheme\(previous\)/);
  assert.match(styles, /\.sticky-action-bar \{ position: sticky; bottom: 12px/);
  assert.match(styles, /\.applied-state-undo/);
});
