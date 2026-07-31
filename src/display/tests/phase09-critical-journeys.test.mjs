import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const [registry, classic, strips, stripStyles, digital, pairing, page, cache] = await Promise.all([
  readFile(new URL('../src/layouts/DisplayLayout.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/layouts/ClassicChalkboardLayout.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/layouts/TapStripsLayout.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/layouts/tapStrips.css', import.meta.url), 'utf8'),
  readFile(new URL('../src/layouts/DigitalTapBoardLayout.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/PairingPage.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/DisplayPage.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/displayCache.mjs', import.meta.url), 'utf8')
]);

test('Phase 09 retains three additive tap layouts and all availability states', () => {
  for (const layout of ['classic_chalkboard', 'tap_strips', 'digital_tap_board']) {
    assert.match(registry, new RegExp(layout));
  }
  assert.match(classic, /isAvailable/);
  assert.match(strips, /isComingSoon/);
  assert.match(digital, /Now Brewing/);
  assert.match(digital, /ordered\.length \/ 6/);
  assert.match(digital, /slice\(index \* 6, index \* 6 \+ 6\)/);
});

test('motion recovery and offline content stay on the shared player boundary', () => {
  assert.match(strips, /animationDelay/);
  assert.match(stripStyles, /prefers-reduced-motion: reduce/);
  assert.match(digital, /window\.setInterval/);
  assert.match(page, /loadDisplayContentResilient/);
  assert.match(cache, /loadDisplayContentResilient/);
});

test('pairing polls regenerates and redirects without a keyboard', () => {
  assert.match(pairing, /PAIRING_POLL_INTERVAL_MS/);
  assert.match(pairing, /error\.status === 410/);
  assert.match(pairing, /window\.location\.replace/);
  assert.doesNotMatch(pairing, /<input|<form/);
});
