import assert from 'node:assert/strict';
import test from 'node:test';
import {
  identityHasChanges,
  passkeyInventoryView,
  screenPresentationHasChanges,
  updateIdentityDraft,
  updateScreenPresentationDraft
} from '../src/actionRecovery.mjs';

test('screen identity draft remains dirty after a failed save and can be reverted', () => {
  const screen = { name: 'Lobby', location: 'Front' };
  const draft = updateIdentityDraft(undefined, screen, { name: 'Main Lobby' });
  assert.equal(identityHasChanges(screen, draft), true);
  const failedSaveDraft = draft;
  assert.equal(failedSaveDraft.name, 'Main Lobby');
  assert.equal(identityHasChanges(screen, undefined), false);
});

test('passkey inventory never maps a failed load to confirmed empty', () => {
  assert.equal(passkeyInventoryView({ loading: true, failed: false, count: 0 }), 'loading');
  assert.equal(passkeyInventoryView({ loading: false, failed: true, count: 0 }), 'failed');
  assert.equal(passkeyInventoryView({ loading: false, failed: false, count: 0 }), 'empty');
  assert.equal(passkeyInventoryView({ loading: false, failed: false, count: 2 }), 'loaded');
});

test('screen presentation changes remain a draft until explicitly applied or discarded', () => {
  const screen = { displayLayout: 'photo_grid', photoGridDensity: '3x2', splitRatio: '40_60', heroDwellSeconds: 8 };
  const layoutDraft = updateScreenPresentationDraft(undefined, screen, { displayLayout: 'split_layout' });
  const completeDraft = updateScreenPresentationDraft(layoutDraft, screen, { splitRatio: '50_50' });

  assert.deepEqual(completeDraft, { displayLayout: 'split_layout', photoGridDensity: '3x2', splitRatio: '50_50', heroDwellSeconds: 8 });
  assert.equal(screen.displayLayout, 'photo_grid');
  assert.equal(screenPresentationHasChanges(screen, completeDraft), true);
  assert.equal(screenPresentationHasChanges(screen, undefined), false);
});
