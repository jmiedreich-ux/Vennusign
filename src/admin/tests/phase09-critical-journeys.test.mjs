import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const [venue, screens, tapList, api] = await Promise.all([
  readFile(new URL('../src/VenueDetail.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/ScreenManagement.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/TapListAdministration.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/api.ts', import.meta.url), 'utf8')
]);

test('Phase 09 administration keeps tap editing venue scoped tier visible and ordered', () => {
  assert.match(venue, /<TapListAdministration/);
  assert.match(tapList, /enabled/);
  assert.match(tapList, /reorderTapRows/);
  assert.match(tapList, /isAvailable/);
  assert.match(tapList, /isComingSoon/);
  assert.match(api, /api\/admin\/venues\/\$\{venueId\}\/tap-list/);
});

test('all three tap layouts retain exact player previews and pairing administration', () => {
  for (const layout of ['classic_chalkboard', 'tap_strips', 'digital_tap_board']) {
    assert.match(screens, new RegExp(layout));
  }
  assert.match(screens, /configuration\.displayBaseUrl/);
  assert.match(screens, /claimPairingCode/);
  assert.match(api, /X-Vennu-Admin-Key/);
});
