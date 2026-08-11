import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const root = new URL("../", import.meta.url);

test("M3-A exposes one in-house path for every approved builder glyph", async () => {
  const icon = await readFile(new URL("src/SkyIcon.tsx", root), "utf8");
  for (const name of ["drag", "pencil", "remove", "chevron", "warning", "screen-mark"]) {
    assert.match(icon, new RegExp(`(?:\\b${name}|\\"${name}\\"):\\s*<`));
  }
  assert.match(icon, /viewBox="0 0 24 24"/);
  assert.match(icon, /strokeWidth="1\.8"/);
  assert.match(icon, /strokeLinecap="round"/);
  assert.match(icon, /aria-hidden="true"/);
});

test("the display face is a page-tab token rather than general chrome", async () => {
  const tokens = await readFile(new URL("src/sky-ui-tokens.css", root), "utf8");
  assert.match(tokens, /--sky-font-family-page-tab:\s*"Playfair Display"/);
  assert.doesNotMatch(tokens, /--sky-font-family:\s*"Playfair Display"/);
});

test("menu capability checks default on and honor an explicit off decision", async () => {
  const { hasMenuCapability } = await import(new URL("src/menuCapabilities.ts", root));
  assert.equal(hasMenuCapability("page-management"), true);
  assert.equal(hasMenuCapability("page-management", { "page-management": false }), false);
  assert.equal(hasMenuCapability("screen-assignment", { "page-management": false }), true);

  const builder = await readFile(new URL("src/MenuBuilder.tsx", root), "utf8");
  assert.match(builder, /hasMenuCapability\("page-management", capabilityOverrides\)/);
  assert.match(builder, /hasMenuCapability\("screen-assignment", capabilityOverrides\)/);
  assert.match(builder, /hasMenuCapability\("capacity", capabilityOverrides\)/);
  assert.match(builder, /canManagePages && activePageId/);
  assert.match(builder, /canAssignScreens \? <button/);
  assert.match(builder, /data-testid="manage-page-screens"/);
  assert.match(builder, /canViewCapacity && capacity/);
});

test("restore is permitted while the three remaining shelf words stay banned", async () => {
  const shelf = await readFile(new URL("../../tests/ui/specs/menus-shelf.spec.ts", root), "utf8");
  const arrays = [...shelf.matchAll(/const banned of \[([^\]]+)\]/g)].map(match => match[1]);
  assert.equal(arrays.length, 2);
  for (const words of arrays) {
    assert.doesNotMatch(words, /restore/i);
    for (const word of ["unpublish", "supersede", "archive"]) assert.match(words, new RegExp(word));
  }
});
