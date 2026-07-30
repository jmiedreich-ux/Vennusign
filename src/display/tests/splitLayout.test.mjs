import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const registry = await readFile(new URL('../src/layouts/DisplayLayout.tsx', import.meta.url), 'utf8');
const layout = await readFile(new URL('../src/layouts/SplitLayout.tsx', import.meta.url), 'utf8');
const css = await readFile(new URL('../src/layouts/splitLayout.css', import.meta.url), 'utf8');

test('registers Split Layout additively', () => {
  assert.match(registry, /key: 'split_layout'/);
  assert.match(registry, /renderer: SplitLayout/);
});

test('selects popular available imagery deterministically and keeps the complete menu', () => {
  assert.match(layout, /item\.isPopular && Boolean\(item\.imageUrl\)/);
  assert.match(layout, /items\.find\(item => available\(item\) && Boolean\(item\.imageUrl\)\)/);
  assert.match(layout, /sections\.map/);
  assert.match(layout, /section\.items\.map/);
  assert.match(layout, /isHappyHour/);
  assert.match(layout, /quantityAvailable/);
});

test('supports only the persisted 40/60 and 50/50 core ratios', () => {
  assert.match(layout, /content\.splitRatio \?\? '40_60'/);
  assert.match(css, /grid-template-columns: 40% 60%/);
  assert.match(css, /data-ratio="50_50"/);
  assert.match(css, /grid-template-columns: 50% 50%/);
});

test('keeps pricing and allergen tags visible within TV-safe overflow bounds', () => {
  assert.match(layout, /activePrice\(content\.isHappyHour, item\)/);
  assert.match(layout, /item\.tags\.map/);
  assert.match(css, /max-height: 100vh/);
  assert.match(css, /overflow: hidden/);
  assert.ok(css.includes("max-aspect-ratio: 4 / 3"));
});
