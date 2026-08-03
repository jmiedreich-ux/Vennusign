import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const [api, screens] = await Promise.all([
  readFile(new URL('../src/api.ts', import.meta.url), 'utf8'),
  readFile(new URL('../src/ScreenManagement.tsx', import.meta.url), 'utf8')
]);

test('claims pairing codes through the protected admin-key flow', () => {
  assert.match(api, /claimPairingCode/);
  assert.match(api, /X-Vennusign-Platform-Operations-Key/);
  assert.match(api, /api\/screens\/pairing/);
  assert.match(screens, /Six-digit pairing code/);
  assert.match(screens, /pattern="\[0-9\]\{6\}"/);
  assert.match(screens, /Screen paired successfully/);
});
