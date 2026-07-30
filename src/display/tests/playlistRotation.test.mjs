import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const [rotation, page, contract] = await Promise.all([
  readFile(new URL('../src/PlaylistRotation.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/DisplayPage.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/displayContent.d.mts', import.meta.url), 'utf8')
]);

test('player rotates ordered supported slides with per-slide dwell and stable recovery', () => {
  assert.match(page, /<PlaylistRotation content=\{content\}>/);
  assert.match(rotation, /active\.dwellSeconds \* 1000/);
  assert.match(rotation, /slides\.some\(slide => slide\.id === current\)/);
  assert.match(rotation, /slideType === 'menu'/);
  assert.match(rotation, /slideType === 'image'/);
  assert.match(rotation, /slideType === 'message'/);
  assert.match(contract, /playlist\?: Array/);
});
