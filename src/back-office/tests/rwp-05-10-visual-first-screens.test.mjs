import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const [screens, styles] = await Promise.all([
  readFile(new URL("../src/ScreenManagement.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/operations.css", import.meta.url), "utf8")
]);

test("screen fleet cards lead with a lazy live thumbnail and health state", () => {
  assert.match(screens, /screen-fleet-grid/);
  assert.match(screens, /className="screen-fleet-thumbnail"/);
  assert.match(screens, /loading="lazy"/);
  assert.match(screens, /screen-fleet-thumbnail__status/);
  assert.match(screens, /Restore this screen to load its live preview/);
});

test("Preview and Push stay visible while management remains secondary", () => {
  assert.match(screens, />Preview<\/button>/);
  assert.match(screens, />Push<\/button>/);
  assert.match(screens, /setSelectedScreenId\(screen\.id\)/);
  assert.match(screens, /screen-fleet-card__settings/);
  assert.match(screens, />Edit display and identity<\/summary>/);
  assert.match(screens, /className="action-overflow"/);
  assert.match(screens, />Archive<\/button>/);
  assert.match(screens, />Unpair screen<\/button>/);
});

test("fleet cards preserve responsive focusable controls and exact preview", () => {
  assert.match(styles, /repeat\(auto-fit, minmax\(min\(100%, 310px\), 1fr\)\)/);
  assert.match(styles, /\.screen-fleet-card\[data-selected="true"\]/);
  assert.match(styles, /\.screen-fleet-card__settings > summary/);
  assert.match(screens, /title=\{previewTitle\(screen\)\}/);
  assert.match(screens, /role="status"/);
});
