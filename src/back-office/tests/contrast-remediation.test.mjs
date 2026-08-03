import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const root = new URL("../", import.meta.url);

function relativeLuminance(hex) {
  const channels = hex.match(/[a-f\d]{2}/gi).map(value => Number.parseInt(value, 16) / 255);
  const linear = channels.map(value => value <= 0.04045 ? value / 12.92 : ((value + 0.055) / 1.055) ** 2.4);
  return (0.2126 * linear[0]) + (0.7152 * linear[1]) + (0.0722 * linear[2]);
}

function contrastRatio(foreground, background) {
  const values = [relativeLuminance(foreground), relativeLuminance(background)].sort((a, b) => b - a);
  return (values[0] + 0.05) / (values[1] + 0.05);
}

test("small text uses the contrast-safe shared token", async () => {
  const [tokens, styles, operations] = await Promise.all([
    readFile(new URL("src/sky-ui-tokens.css", root), "utf8"),
    readFile(new URL("src/styles.css", root), "utf8"),
    readFile(new URL("src/operations.css", root), "utf8")
  ]);

  assert.ok(contrastRatio("64748b", "ffffff") >= 4.5);
  assert.match(tokens, /--sky-small-text:\s*var\(--sky-color-text-muted\)/);
  assert.match(`${styles}\n${operations}`, /color:\s*var\(--sky-small-text\)/);
  assert.doesNotMatch(`${styles}\n${operations}`, /#71827b/i);
});

test("locked navigation keeps readable text and a non-color lock cue", async () => {
  const [navigation, styles, app] = await Promise.all([
    readFile(new URL("src/LockedNavigationItem.tsx", root), "utf8"),
    readFile(new URL("src/styles.css", root), "utf8"),
    readFile(new URL("src/App.tsx", root), "utf8")
  ]);

  assert.ok(contrastRatio("a9c5b9", "123b30") >= 4.5);
  assert.match(navigation, /locked-navigation-item__lock/);
  assert.match(navigation, /aria-hidden="true"/);
  assert.match(app, /Locked/);
  assert.doesNotMatch(styles, /nav a\.locked\s*\{[^}]*opacity:/);
  assert.doesNotMatch(styles, /\.locked-navigation-item\s*\{[^}]*opacity:/);
});
