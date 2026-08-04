import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const [theme, screens, styles] = await Promise.all([
  readFile(new URL("../src/ThemeBuilder.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/ScreenManagement.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/styles.css", import.meta.url), "utf8")
]);

test("Back Office surfaces declare one primary and overflow destructive actions", () => {
  assert.match(theme, /sticky-action-bar/);
  assert.match(theme, /className="action-primary"[^>]*>Save basic theme/);
  assert.match(theme, /className="action-overflow"/);
  assert.match(theme, /className="action-danger"[^>]*>Reset theme/);
  assert.match(screens, /className="screen-actions action-surface"/);
  assert.match(screens, /className="action-danger"/);
  assert.match(screens, />Unpair screen<\/button>/);
});

test("long-form apply and applied-state undo are explicit and authoritative", () => {
  assert.match(screens, /screen-presentation-draft sticky-action-bar/);
  assert.match(theme, /setUndoTheme\(previous\)/);
  assert.match(theme, /Undo applied theme/);
  assert.match(theme, /saveAdvancedVenueTheme/);
  assert.match(theme, /Previous venue theme restored/);
});

test("shared action utilities remain keyboard focused and responsive", () => {
  assert.match(styles, /Action hierarchy standard/);
  assert.match(styles, /\.action-overflow summary/);
  assert.match(styles, /\.applied-state-undo/);
  assert.match(styles, /@media \(max-width: 600px\) \{ \.action-surface, \.sticky-action-bar/);
  assert.match(styles, /:where\(button, a, input, select, textarea, summary\):focus-visible/);
});
