import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const source = path => readFile(new URL(`../src/${path}`, import.meta.url), 'utf8');
const [page, realtime, cache, playlist, emergency, promotion, happyHour] = await Promise.all([
  source('DisplayPage.tsx'),
  source('displayRealtime.mjs'),
  source('displayCache.mjs'),
  source('PlaylistRotation.tsx'),
  source('EmergencyBroadcastOverlay.tsx'),
  source('layouts/PromotionBanner.tsx'),
  source('layouts/HappyHourBanner.tsx')
]);

test('schedule transitions retain authoritative realtime and offline recovery', () => {
  assert.match(realtime, /scheduled-content-transition/);
  assert.match(realtime, /date-range-promotion-transition/);
  assert.match(page, /requiresContentReload/);
  assert.match(page, /loadDisplayContentResilient/);
  assert.match(cache, /screenId/);
  assert.match(cache, /version/);
});

test('Phase 08 display precedence and recovery remain explicit', () => {
  assert.match(page, /<EmergencyBroadcastOverlay[\s\S]*<PlaylistRotation[\s\S]*<DisplayLayout/);
  assert.match(emergency, /return children/);
  assert.match(playlist, /active\.dwellSeconds \* 1000/);
  assert.match(promotion, /data-promotion-id/);
  assert.match(happyHour, /happyHourEndsAtUtc/);
});
