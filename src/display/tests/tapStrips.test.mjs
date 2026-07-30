import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const [layout, registry, css] = await Promise.all([
  readFile(new URL('../src/layouts/TapStripsLayout.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/layouts/DisplayLayout.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/layouts/tapStrips.css', import.meta.url), 'utf8')
]);

test('registers Tap Strips additively on the shared player path', () => {
  assert.match(registry, /key: 'tap_strips'/);
  assert.doesNotMatch(layout, /fetch\(|localStorage|HubConnection/);
});

test('renders deterministic three-column tap strips with beer details and states', () => {
  assert.match(layout, /fonts\[index % fonts\.length\]/);
  assert.match(layout, /index \+ 1/);
  assert.match(layout, /item\.style/);
  assert.match(layout, /ABV/);
  assert.match(layout, /item\.price\.toFixed\(2\)/);
  assert.match(layout, /item\.nameColor/);
  assert.match(layout, /Unavailable/);
  assert.match(layout, /Now brewing/);
  assert.match(css, /grid-template-columns: repeat\(3/);
});

test('draws strips sequentially while preserving reduced-motion and shared recovery', () => {
  assert.match(layout, /animationDelay: `\$\{index \* 70\}ms`/);
  assert.match(css, /prefers-reduced-motion: reduce/);
  assert.match(css, /animation: none/);
  assert.doesNotMatch(layout, /fetch\(|localStorage|HubConnection/);
});
