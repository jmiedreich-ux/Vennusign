import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const [banner, frame, page, contract] = await Promise.all([
  readFile(new URL('../src/layouts/PromotionBanner.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/layouts/DisplayLayout.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/DisplayPage.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/displayContent.d.mts', import.meta.url), 'utf8')
]);

test('active promotion content renders inside the emergency-broadcast boundary', () => {
  assert.match(contract, /promotion\?:/);
  assert.match(frame, /<PromotionBanner content=\{content\}/);
  assert.match(banner, /data-promotion-id/);
  assert.match(page, /<EmergencyBroadcastOverlay content=\{content\}>/);
});

test('promotion transitions reload authoritative content', () => {
  assert.match(page, /requiresContentReload\(eventName, args\[0\]\)/);
  assert.match(page, /void loadAndActivate\(\)/);
});
