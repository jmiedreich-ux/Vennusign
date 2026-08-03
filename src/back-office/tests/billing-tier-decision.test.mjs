import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import {
  clearPendingTierDecision,
  pendingTierStaleAfterMs,
  readPendingTierDecision,
  resolvePendingTierDecision,
  writePendingTierDecision
} from '../src/billingDecision.mjs';

const root = resolve(import.meta.dirname, '..');
const source = path => readFileSync(resolve(root, path), 'utf8');
const storage = () => {
  const values = new Map();
  return {
    getItem: key => values.get(key) ?? null,
    setItem: (key, value) => values.set(key, value),
    removeItem: key => values.delete(key)
  };
};

test('pending tier decisions remain webhook-authoritative and recoverable', () => {
  const store = storage();
  const now = Date.parse('2026-08-03T12:00:00Z');
  const pending = writePendingTierDecision({ id: 'growth', name: 'Growth' }, store, now);

  assert.deepEqual(readPendingTierDecision(store), pending);
  assert.equal(resolvePendingTierDecision(pending, 'starter', now + 1000), 'pending');
  assert.equal(resolvePendingTierDecision(pending, 'growth', now + 1000), 'applied');
  assert.equal(resolvePendingTierDecision(pending, 'starter', now + pendingTierStaleAfterMs), 'stale');
  clearPendingTierDecision(store);
  assert.equal(readPendingTierDecision(store), undefined);
});

test('tier review records financial safety, accessibility, and server enforcement', () => {
  const dialog = source('src/TierDecisionDialog.tsx');
  const card = source('src/BillingStatusCard.tsx');
  const controller = source('../Vennu.Api/Controllers/BackOffice/BackOfficeBillingController.cs');

  assert.match(dialog, /showModal\(\)/);
  assert.match(dialog, /Keep current plan/);
  assert.match(dialog, /authoritative webhook state/);
  assert.match(card, /Active screens/);
  assert.match(card, /Organization venues/);
  assert.match(card, /current feature.*would be removed/s);
  assert.match(controller, /EnsureTierCanBeSelectedAsync/);
  assert.match(controller, /tier-portal-session/);
  assert.match(card, /Hardware as a Service/);
});
