import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const source = path => readFile(new URL(`../src/${path}`, import.meta.url), 'utf8');
const [venue, mealPeriods, happyHour, playlists, broadcasts, promotions] = await Promise.all([
  source('VenueDetail.tsx'),
  source('MealPeriodAdministration.tsx'),
  source('HappyHourAdministration.tsx'),
  source('PlaylistAdministration.tsx'),
  source('EmergencyBroadcastAdministration.tsx'),
  source('DateRangePromotionAdministration.tsx')
]);

test('Phase 08 administration remains composed in one venue-scoped journey', () => {
  assert.match(venue, /<MealPeriodAdministration/);
  assert.match(venue, /<HappyHourAdministration/);
  assert.match(venue, /<PlaylistAdministration/);
  assert.match(venue, /<EmergencyBroadcastAdministration/);
  assert.match(venue, /<DateRangePromotionAdministration/);
});

test('scheduling controls retain time range ordering and tier-visible behavior', () => {
  assert.match(mealPeriods, /venue timezone/i);
  assert.match(happyHour, /force_on/);
  assert.match(playlists, /dwellSeconds/);
  assert.match(broadcasts, /Emergency Broadcast requires Pro/);
  assert.match(broadcasts, /Controls remain visible/);
  assert.match(promotions, /min=\{draft\.startLocalDate/);
  assert.match(promotions, /Promotion scheduling is visible as a preview/);
});
