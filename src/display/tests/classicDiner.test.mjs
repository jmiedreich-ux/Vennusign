import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const layoutSource = await readFile(new URL('../src/layouts/ClassicDinerLayout.tsx', import.meta.url), 'utf8');
const registrySource = await readFile(new URL('../src/layouts/DisplayLayout.tsx', import.meta.url), 'utf8');
const styleSource = await readFile(new URL('../src/layouts/classicDiner.css', import.meta.url), 'utf8');

test('registers Classic Diner through the additive layout registry', () => {
  assert.match(registrySource, /key: 'classic_diner'/);
  assert.match(registrySource, /renderer: ClassicDinerLayout/);
});

test('renders venue menu sections ordered item copy and pricing', () => {
  for (const contract of ['content.venueName', 'content.menuName', 'section.name', 'item.name', 'item.description', 'item.price', 'item.happyHourPrice']) {
    assert.match(layoutSource, new RegExp(contract.replace('.', '\\.')));
  }
  assert.match(layoutSource, /activePrice\(content\.isHappyHour/);
  assert.match(layoutSource, /formatPrice/);
});

test('uses warm high-contrast two and three column TV-safe styling', () => {
  assert.match(styleSource, /background: var\(--vennu-background, #f6edda\)/);
  assert.match(styleSource, /color: #241b12/);
  assert.match(styleSource, /column-count: 2/);
  assert.match(styleSource, /@media \(min-width: 1600px\)/);
  assert.match(styleSource, /column-count: 3/);
  assert.match(styleSource, /break-inside: avoid/);
});

test('renders dot leaders category bars and the existing daily special contract', () => {
  assert.match(layoutSource, /content\.dailySpecial/);
  assert.match(layoutSource, /classic-diner__special/);
  assert.match(layoutSource, /classic-diner__leader/);
  assert.match(styleSource, /border-bottom: 0\.13em dotted/);
  assert.match(styleSource, /font-variant-numeric: tabular-nums/);
  assert.match(styleSource, /\.classic-diner__section h2[\s\S]*background: var\(--vennu-accent, #7b241c\)/);
});
