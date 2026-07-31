import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';
import {
  dismissUpgradeFeature,
  readDismissedUpgradeFeatures,
  selectUpgradeOpportunity,
  tierPresentation,
  upgradeCatalog
} from '../src/upgradeExperience.mjs';

function createStorage() {
  const values = new Map();
  return { getItem: key => values.get(key) ?? null, setItem: (key, value) => values.set(key, value) };
}

test('defines stable presentation metadata for every approved tier', () => {
  assert.deepEqual(Object.keys(tierPresentation), ['starter', 'restaurant_starter', 'pro', 'business']);
  assert.deepEqual(Object.values(tierPresentation).map(item => item.tone), ['slate', 'green', 'amber', 'purple']);
});

test('catalog uses concrete benefits and valid target tiers', () => {
  assert.ok(upgradeCatalog.length >= 10);
  assert.equal(new Set(upgradeCatalog.map(item => item.featureKey)).size, upgradeCatalog.length);
  for (const item of upgradeCatalog) {
    assert.ok(item.benefit.length > 35);
    assert.ok(tierPresentation[item.requiredTier]);
    assert.doesNotMatch(item.benefit, /^upgrade to/i);
  }
});

test('selects at most one locked opportunity in deterministic catalog order', () => {
  const result = selectUpgradeOpportunity({ happy_hour: { enabled: false }, quick_update: { enabled: false }, all_layouts: { enabled: true } });
  assert.equal(result?.featureKey, 'quick_update');
  assert.equal(selectUpgradeOpportunity({ happy_hour: { enabled: true } }), undefined);
});

test('dismissal is session scoped sanitized and ignored for unknown features', () => {
  const storage = createStorage();
  dismissUpgradeFeature('happy_hour', storage);
  dismissUpgradeFeature('not-a-feature', storage);
  assert.deepEqual([...readDismissedUpgradeFeatures(storage)], ['happy_hour']);
  assert.equal(selectUpgradeOpportunity(
    { happy_hour: { enabled: false }, all_layouts: { enabled: false } },
    readDismissedUpgradeFeatures(storage)
  )?.featureKey, 'all_layouts');
});

test('tier badge is informational and uses the canonical presentation contract', async () => {
  const source = await readFile(new URL('../src/TierBadge.tsx', import.meta.url), 'utf8');
  assert.match(source, /tierPresentation\[tier\]/);
  assert.match(source, /aria-label=\{`\$\{presentation\.label\} tier`\}/);
  assert.match(source, /presentation\.badgeLabel/);
  assert.doesNotMatch(source, /button|onClick/);
});
