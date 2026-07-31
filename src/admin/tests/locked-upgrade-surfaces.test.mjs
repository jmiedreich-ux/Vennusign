import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const [navigation, preview, venue, styles] = await Promise.all([
  readFile(new URL('../src/LockedNavigationItem.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/LockedSectionPreview.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/VenueDetail.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/styles.css', import.meta.url), 'utf8')
]);

test('locked navigation stays visible subdued and opens shared upgrade context', () => {
  assert.match(navigation, /locked-navigation-item/);
  assert.match(navigation, /<TierBadge tier=\{opportunity\.requiredTier\}/);
  assert.match(navigation, /onUpgrade\(opportunity\)/);
  assert.doesNotMatch(navigation, /disabled|href=/);
  assert.match(styles, /\.locked-navigation-item[\s\S]*opacity: \.5/);
});

test('locked section preview shows one concrete benefit and keeps its mockup non-interactive', () => {
  assert.match(preview, /aria-labelledby/);
  assert.match(preview, /aria-hidden="true"/);
  assert.match(preview, /\{opportunity\.benefit\}/);
  assert.match(preview, /See upgrade options/);
  assert.match(preview, /Not now/);
  assert.match(styles, /filter: blur\(\.3px\)/);
});

test('venue detail retains one selected upgrade surface and every existing workflow', () => {
  assert.match(venue, /selectUpgradeOpportunity\(detail\.features/);
  assert.match(venue, /const inlineHint = !onUpgradeContextChange && upgradeOpportunity/);
  assert.match(preview, /export default function LockedSectionPreview/);
  for (const workflow of ['ScreenManagement', 'ThemeBuilder', 'MenuSectionsEditor', 'HappyHourAdministration']) {
    assert.match(venue, new RegExp(`<${workflow}`));
  }
});
