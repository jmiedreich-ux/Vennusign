import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const source = path => readFile(new URL(`../src/${path}`, import.meta.url), 'utf8');
const [registry, neon, neonCss, fonts, split, hero, page, contentCache, mediaWorker] = await Promise.all([
  source('layouts/DisplayLayout.tsx'),
  source('layouts/NeonChalkboardLayout.tsx'),
  source('layouts/neonChalkboard.css'),
  source('notoFonts.mjs'),
  source('layouts/SplitLayout.tsx'),
  source('layouts/DailySpecialHeroLayout.tsx'),
  source('DisplayPage.tsx'),
  source('displayCache.mjs'),
  readFile(new URL('../public/vennu-media-sw.js', import.meta.url), 'utf8')
]);

test('all Phase 07 layouts remain additive and content driven', () => {
  assert.match(registry, /key: 'neon_chalkboard'/);
  assert.match(registry, /key: 'split_layout'/);
  assert.match(registry, /key: 'daily_special_hero'/);
  assert.match(neon, /content\.sections/);
  assert.match(split, /content\.splitRatio/);
  assert.match(hero, /content\.dailySpecial/);
});

test('motion, fonts, pricing, tags, and hero recovery retain Phase 07 contracts', () => {
  assert.match(neonCss, /prefers-reduced-motion: reduce/);
  assert.match(fonts, /Noto Sans Arabic/);
  assert.match(split, /activePrice\(content\.isHappyHour, item\)/);
  assert.match(split, /item\.tags\.map/);
  assert.match(hero, /content\.heroDwellSeconds \?\? 8/);
  assert.match(hero, /rotationItems\.some\(item => item\.id === current\)/);
  assert.match(hero, /prefers-reduced-motion: reduce/);
});

test('realtime and offline recovery remain on the shared player path', () => {
  assert.match(page, /applyRealtimeEvent/);
  assert.match(page, /loadDisplayContentResilient/);
  assert.match(contentCache, /screenId/);
  assert.match(contentCache, /version/);
  assert.match(mediaWorker, /caches\.open/);
  assert.match(mediaWorker, /cache\.match/);
});
