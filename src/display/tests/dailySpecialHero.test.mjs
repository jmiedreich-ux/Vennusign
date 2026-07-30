import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const registry = await readFile(new URL('../src/layouts/DisplayLayout.tsx', import.meta.url), 'utf8');
const layout = await readFile(new URL('../src/layouts/DailySpecialHeroLayout.tsx', import.meta.url), 'utf8');
const css = await readFile(new URL('../src/layouts/dailySpecialHero.css', import.meta.url), 'utf8');

test('registers Daily Special Hero additively', () => {
  assert.match(registry, /key: 'daily_special_hero'/);
  assert.match(registry, /renderer: DailySpecialHeroLayout/);
});

test('selects the requested available item before deterministic media fallbacks', () => {
  assert.match(layout, /item\.name\.trim\(\)\.toLocaleLowerCase\(\) === requestedName/);
  assert.match(layout, /item\.isPopular && Boolean\(item\.imageUrl\)/);
  assert.match(layout, /available\.find\(item => Boolean\(item\.imageUrl\)\)/);
  assert.match(layout, /available\[0\]/);
});

test('renders full-screen media, active price, Today Only, and ordered secondary items', () => {
  assert.match(layout, /Today Only/);
  assert.match(layout, /featured\.description/);
  assert.match(layout, /activePrice\(content\.isHappyHour, featured\)/);
  assert.match(layout, /filter\(item => isAvailable\(item\) && item\.id !== featuredId\)\.slice\(0, 3\)/);
  assert.match(css, /max-height: 100vh/);
  assert.match(css, /object-fit: cover/);
});
