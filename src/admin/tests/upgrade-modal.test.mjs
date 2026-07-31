import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';
import { upgradeFeaturePills } from '../src/upgradeExperience.mjs';

const [modal, app, venue, styles] = await Promise.all([
  readFile(new URL('../src/UpgradeModal.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/App.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/VenueDetail.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/styles.css', import.meta.url), 'utf8')
]);

test('target-tier feature pills preserve the canonical catalog', () => {
  assert.deepEqual(upgradeFeaturePills('pro'), ['All display layouts', 'Happy hour', 'POS integration', 'Staff mobile app']);
});

test('modal is accessible dismissible and presents authoritative tier value', () => {
  assert.match(modal, /role="dialog"/);
  assert.match(modal, /aria-modal="true"/);
  assert.match(modal, /event\.key === "Escape"/);
  assert.match(modal, /currentTier\?\.name/);
  assert.match(modal, /targetTier\.price \* 10/);
  assert.match(modal, /Monthly/);
  assert.match(modal, /Annual · two months included/);
  assert.match(modal, /Maybe later/);
  assert.match(styles, /\.upgrade-modal-backdrop/);
});

test('one modal CTA replaces the sidebar prompt without changing entitlement', () => {
  assert.match(app, /venueUpgrade && !upgradeContext \? <SidebarUpgradeNudge/);
  assert.match(app, /<UpgradeModal/);
  assert.match(app, /onUpgrade=\{continueUpgrade\}/);
  assert.match(venue, /tiers: featureMatrix\.tiers/);
  assert.doesNotMatch(modal, /fetch|enabled\s*=|effectiveFeatures/);
});
