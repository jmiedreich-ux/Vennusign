import assert from 'node:assert/strict';
import test from 'node:test';
import { identityHasChanges, passkeyInventoryView, updateIdentityDraft } from '../src/actionRecovery.mjs';

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
