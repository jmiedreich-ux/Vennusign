import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const [layout, registry, css] = await Promise.all([
  readFile(new URL('../src/layouts/DigitalTapBoardLayout.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/layouts/DisplayLayout.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/layouts/digitalTapBoard.css', import.meta.url), 'utf8')
]);

test('registers Digital Tap Board additively on the shared player path', () => {
  assert.match(registry, /key: 'digital_tap_board'/);
  assert.doesNotMatch(layout, /fetch\(|localStorage|HubConnection/);
});

test('renders a deterministic six-card two-column beer board', () => {
  assert.match(layout, /ordered\.slice\(0, 6\)/);
  assert.match(layout, /<svg/);
  assert.match(layout, /item\.glassColor/);
  assert.match(layout, /ABV/);
  assert.match(layout, /IBU/);
  assert.match(layout, /item\.price\.toFixed\(2\)/);
  assert.match(css, /repeating-linear-gradient/);
  assert.match(css, /grid-template-columns: repeat\(2/);
});
