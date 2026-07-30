import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const [banner, frame, contract] = await Promise.all([
  readFile(new URL('../src/layouts/HappyHourBanner.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/layouts/DisplayLayout.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/displayContent.d.mts', import.meta.url), 'utf8')
]);

test('player renders authoritative happy-hour banner and countdown across layouts', () => {
  assert.match(frame, /<HappyHourBanner content=\{content\}/);
  assert.match(banner, /content\.isHappyHour/);
  assert.match(banner, /content\.happyHourEndsAtUtc/);
  assert.match(banner, /setInterval/);
  assert.match(contract, /happyHourMode/);
});
