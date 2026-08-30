import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

/*
 * #961: rotating to a new real or virtual page swapped content in the same frame - no
 * transition, just a hard cut. These read the source rather than render it, matching this
 * suite's existing convention for anything that isn't a pure function (see photoGrid.test.mjs).
 */
const layoutSource = await readFile(new URL('../src/layouts/DisplayLayout.tsx', import.meta.url), 'utf8');
const styleSource = await readFile(new URL('../src/layouts/pageTransition.css', import.meta.url), 'utf8');

test('DisplayLayout imports the page transition stylesheet', () => {
  assert.match(layoutSource, /import '\.\/pageTransition\.css'/);
});

test('the rendered page is keyed by what is actually on screen, not remounted on every poll', () => {
  assert.match(layoutSource, /pageSignature/);
  assert.match(layoutSource, /content\.sections.*\.map\(\(section\) => section\.id\)\.join\('\|'\)/);
  assert.match(layoutSource, /key=\{pageSignature\}/);
  assert.match(layoutSource, /className="display-frame__page"/);
});

test('the fade is a real transition, not instant, and respects reduced motion', () => {
  assert.match(styleSource, /\.display-frame__page\s*\{[^}]*animation:\s*vennu-page-fade/);
  assert.match(styleSource, /@keyframes vennu-page-fade/);
  assert.match(styleSource, /prefers-reduced-motion:\s*reduce[\s\S]*animation:\s*none/);
});
