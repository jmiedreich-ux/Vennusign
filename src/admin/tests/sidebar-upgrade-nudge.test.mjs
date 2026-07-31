import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';
import { listUpgradeOpportunities } from '../src/upgradeExperience.mjs';

const [component, app, venue, styles] = await Promise.all([
  readFile(new URL('../src/SidebarUpgradeNudge.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/App.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/VenueDetail.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/styles.css', import.meta.url), 'utf8')
]);

test('lists every eligible opportunity in deterministic catalog order', () => {
  assert.deepEqual(
    listUpgradeOpportunities({ happy_hour: { enabled: false }, quick_update: { enabled: false }, video_wall: { enabled: false } }).map(item => item.featureKey),
    ['quick_update', 'happy_hour', 'video_wall']
  );
});

test('sidebar nudge rotates every seven seconds unless reduced motion is requested', () => {
  assert.match(component, /const rotationMilliseconds = 7_000/);
  assert.match(component, /prefers-reduced-motion: reduce/);
  assert.match(component, /window\.setInterval/);
  assert.match(component, /sidebar-upgrade-nudge__dots/);
  assert.match(component, /aria-current/);
});

test('dismissal is per feature and the app exposes only one active prompt surface', () => {
  assert.match(component, /dismissUpgradeFeature\(opportunity\.featureKey\)/);
  assert.match(app, /<SidebarUpgradeNudge/);
  assert.match(app, /route\.path !== "venues"/);
  assert.match(venue, /!onUpgradeFeaturesChange && upgradeOpportunity/);
  assert.match(styles, /\.sidebar-upgrade-nudge/);
});
