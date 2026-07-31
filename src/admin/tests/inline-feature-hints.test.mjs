import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';
import { upgradePanelForFeature } from '../src/upgradeExperience.mjs';

const [hint, venue, styles] = await Promise.all([
  readFile(new URL('../src/InlineFeatureHint.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/VenueDetail.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/styles.css', import.meta.url), 'utf8')
]);

test('maps locked features to the most relevant panel deterministically', () => {
  assert.equal(upgradePanelForFeature('all_layouts'), 'design');
  assert.equal(upgradePanelForFeature('pos_integration'), 'menu');
  assert.equal(upgradePanelForFeature('happy_hour'), 'scheduling');
  assert.equal(upgradePanelForFeature('multi_location'), 'operations');
  assert.equal(upgradePanelForFeature('future_feature'), 'operations');
});

test('inline hint is concrete quiet accessible and dismissible', () => {
  assert.match(hint, /<TierBadge tier=\{opportunity\.requiredTier\}/);
  assert.match(hint, /\{opportunity\.benefit\}/);
  assert.match(hint, /See what it unlocks/);
  assert.match(hint, /aria-label=\{`Dismiss \$\{opportunity\.title\} suggestion`\}/);
  assert.match(hint, /onDismiss\(opportunity\.featureKey\)/);
  assert.match(styles, /border-left: 4px solid #3b82f6/);
});

test('one selected opportunity is inserted into exactly one mapped panel', () => {
  assert.match(venue, /const inlineHint = !onUpgradeFeaturesChange && upgradeOpportunity/);
  for (const panel of ['design', 'menu', 'scheduling', 'operations']) {
    assert.match(venue, new RegExp(`upgradePanel === "${panel}" \\? inlineHint : null`));
  }
  assert.doesNotMatch(venue, /<LockedSectionPreview/);
});
