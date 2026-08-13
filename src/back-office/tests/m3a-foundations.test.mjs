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

test("page tabs use the normal application typeface", async () => {
  const tokens = await readFile(new URL("src/sky-ui-tokens.css", root), "utf8");
  const styles = await readFile(new URL("src/menu-builder.css", root), "utf8");
  assert.doesNotMatch(tokens, /--sky-font-family-page-tab/);
  assert.doesNotMatch(tokens, /--sky-font-family:\s*"Playfair Display"/);
  assert.match(styles, /\.builder__page-tab\s*\{[^}]*border:\s*0;/s);
  assert.match(styles, /\.builder__page-tab\.is-active::after\s*\{[^}]*var\(--sky-color-primary\)/s);
});

test("Slice 3-A keeps section scope in the rail and gives both side panels persistent independent controls", async () => {
  const builder = await readFile(new URL("src/MenuBuilder.tsx", root), "utf8");
  const styles = await readFile(new URL("src/menu-builder.css", root), "utf8");

  assert.match(builder, /vennusign\.menu\.builder\.panels/);
  assert.match(builder, /leftCollapsed:\s*stored\?\.leftCollapsed === true/);
  assert.match(builder, /rightCollapsed:\s*stored\?\.rightCollapsed === true/);
  assert.match(builder, /data-testid=\{`\$\{side\}-panel-toggle`\}/);
  assert.match(builder, /aria-expanded=\{!collapsed\}/);
  assert.match(builder, /data-testid="page-name"/);
  assert.match(builder, /data-testid="section-scope"/);
  assert.doesNotMatch(builder, /data-testid="section-chips"/);

  assert.match(styles, /--builder-left-panel-width:\s*212px/);
  assert.match(styles, /--builder-right-panel-width:\s*296px/);
  assert.match(styles, /\.builder__rail\.is-collapsed[^}]*overflow:\s*hidden/s);
  assert.match(styles, /\.builder__inspector\.is-collapsed[^}]*overflow:\s*hidden/s);
  assert.match(styles, /\.builder__rail\.is-collapsed \.builder__rail-head h2,[\s\S]*writing-mode:\s*vertical-rl/);
  assert.doesNotMatch(styles, /\.builder__inspector\.is-collapsed \.builder__inspector-toolbar > strong,[\s\n]*\.builder__inspector\.is-collapsed \.builder__inspector-body\s*\{[^}]*display:\s*none/s);
});

test("Slice 3-A history rows stay compact and keep View all with the heading", async () => {
  const builder = await readFile(new URL("src/MenuBuilder.tsx", root), "utf8");
  const styles = await readFile(new URL("src/menu-builder.css", root), "utf8");

  assert.match(builder, /builder__page-history-header[\s\S]*menu-history-link[\s\S]*>View all<\/button>/);
  assert.match(styles, /\.builder__page-history-list strong\s*\{[^}]*font-size:\s*var\(--sky-font-size-xs\)/s);
  assert.match(styles, /\.builder__page-history-list small\s*\{[^}]*font-size:\s*var\(--sky-font-size-xxs\)/s);
});

test("menu capability checks default on and honor an explicit off decision", async () => {
  const { hasMenuCapability } = await import(new URL("src/menuCapabilities.ts", root));
  assert.equal(hasMenuCapability("page-management"), true);
  assert.equal(hasMenuCapability("page-management", { "page-management": false }), false);
  assert.equal(hasMenuCapability("screen-assignment", { "page-management": false }), true);

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
