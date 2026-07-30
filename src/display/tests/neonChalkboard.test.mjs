import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const registry = await readFile(new URL('../src/layouts/DisplayLayout.tsx', import.meta.url), 'utf8');
const layout = await readFile(new URL('../src/layouts/NeonChalkboardLayout.tsx', import.meta.url), 'utf8');
const css = await readFile(new URL('../src/layouts/neonChalkboard.css', import.meta.url), 'utf8');

test('registers Neon Chalkboard through the additive layout registry', () => {
  assert.match(registry, /key: 'neon_chalkboard'/);
  assert.match(registry, /renderer: NeonChalkboardLayout/);
});

test('renders menu columns, current pricing, sold-out state, and section dividers', () => {
  for (const field of ['venueName', 'menuName', 'sections', 'isHappyHour', 'happyHourPrice', 'isAvailable', 'quantityAvailable']) {
    assert.match(layout, new RegExp(field));
  }
  assert.match(layout, /--neon-section-color/);
  assert.match(css, /column-count: 2/);
  assert.match(css, /--neon-section-color/);
});

test('uses advanced theme values for a TV-safe chalkboard frame', () => {
  for (const variable of ['--vennu-board-background', '--vennu-glow-color', '--vennu-title-color', '--vennu-title-font', '--vennu-item-font', '--vennu-glow-intensity']) {
    assert.match(css, new RegExp(variable));
  }
  assert.match(css, /neon-chalkboard__frame/);
});

test('bounds neon motion and removes it for reduced-motion viewers', () => {
  for (const motion of ['neon-title-flicker', 'neon-glow-breathe', 'chalk-draw-in']) {
    assert.match(css, new RegExp(`@keyframes ${motion}`));
  }
  assert.match(css, /repeating-linear-gradient/);
  assert.match(css, /prefers-reduced-motion: reduce/);
  assert.match(css, /animation: none/);
  assert.match(css, /clip-path: none/);
});
