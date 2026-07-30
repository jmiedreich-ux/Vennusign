import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const [layout, registry, css, contract] = await Promise.all([
  readFile(new URL('../src/layouts/ClassicChalkboardLayout.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/layouts/DisplayLayout.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/layouts/classicChalkboard.css', import.meta.url), 'utf8'),
  readFile(new URL('../src/displayContent.d.mts', import.meta.url), 'utf8')
]);

test('registers Classic Chalkboard through the additive display registry', () => {
  assert.match(registry, /key: 'classic_chalkboard'/);
  assert.match(contract, /tapCategories\?:/);
  assert.match(contract, /tapItems\?:/);
});

test('renders category pricing two-column drinks and unavailable treatment', () => {
  assert.match(layout, /<h1>Drinks<\/h1>/);
  assert.match(layout, /category\.categoryPrice/);
  assert.match(layout, /item\.isAvailable \? '' : 'unavailable'/);
  assert.match(css, /columns: 2/);
  assert.match(css, /li\.unavailable/);
});

test('keeps chalk polish TV safe accessible and recoverable through the shared player path', () => {
  assert.match(layout, /aria-hidden="true"/);
  assert.match(css, /overflow: hidden/);
  assert.match(css, /prefers-reduced-motion: no-preference/);
  assert.doesNotMatch(layout, /fetch\(|localStorage|HubConnection/);
});
