import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const source = (name) => readFile(new URL(`../src/${name}`, import.meta.url), 'utf8');
const publicSource = (name) => readFile(new URL(`../public/${name}`, import.meta.url), 'utf8');

const [page, frame, photo, diner, cache, realtime, worker, playerCss] = await Promise.all([
  source('DisplayPage.tsx'),
  source('layouts/DisplayLayout.tsx'),
  source('layouts/PhotoGridLayout.tsx'),
  source('layouts/ClassicDinerLayout.tsx'),
  source('displayCache.mjs'),
  source('displayRealtime.mjs'),
  publicSource('vennu-media-sw.js'),
  source('player.css')
]);

test('player composes both Phase 06 layouts through one registry path', () => {
  assert.match(frame, /key: 'photo_grid'/);
  assert.match(frame, /renderer: PhotoGridLayout/);
  assert.match(frame, /key: 'classic_diner'/);
  assert.match(frame, /renderer: ClassicDinerLayout/);
  assert.match(frame, /layoutRegistry\.resolve/);
});

test('restaurant journeys retain merchandising pricing and overflow contracts', () => {
  for (const contract of [
    'item.isPopular',
    'item.quantityAvailable',
    'content.isHappyHour',
    'item.happyHourPrice',
    'content.photoGridOverflowItems'
  ]) {
    assert.match(photo, new RegExp(contract.replace('.', '\\.')));
  }

  assert.match(diner, /content\.dailySpecial/);
  assert.match(diner, /classic-diner__leader/);
});

test('theme preview and realtime updates stay on the production player path', () => {
  assert.match(page, /previewTheme \? \{ \.\.\.content, theme: previewTheme \} : content/);
  assert.match(page, /applyRealtimeEvent/);
  assert.match(page, /cacheDisplayContent/);
  assert.match(realtime, /ThemeUpdated/);
  assert.match(realtime, /ItemAvailabilityChanged/);
});

test('offline content is screen-bound versioned and recoverable', () => {
  assert.match(cache, /displayContentCacheVersion/);
  assert.match(cache, /cached\.content\?\.screenId === screenId/);
  assert.match(cache, /displayContentCacheMaxAgeMs/);
  assert.match(page, /window\.addEventListener\('online', recoverOnline\)/);
  assert.match(page, /Offline — showing the last saved menu/);
  assert.match(page, /DISPLAY_CONTENT_RECOVERY_INTERVAL_MS/);
  assert.match(playerCss, /html, body, #root[\s\S]*overflow: hidden/);
  assert.match(playerCss, /overscroll-behavior: none/);
});

test('media cache invalidates old versions and falls back only for supported media', () => {
  assert.match(worker, /mediaCacheName/);
  assert.match(worker, /caches\.delete/);
  assert.match(worker, /\['image', 'font', 'style'\]\.includes/);
  assert.match(worker, /await fetch\(request\)/);
  assert.match(worker, /await cache\.match\(request\)/);
});
