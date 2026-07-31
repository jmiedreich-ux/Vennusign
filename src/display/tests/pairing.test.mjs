import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';
import {
  PAIRING_POLL_INTERVAL_MS,
  createPairingCode,
  displayPath,
  loadPairingStatus,
  registerPairingScreen
} from '../src/pairing.mjs';

const [app, page, routes] = await Promise.all([
  readFile(new URL('../src/App.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/PairingPage.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/routing.ts', import.meta.url), 'utf8')
]);

test('registers and requests a code using the existing pairing APIs', async () => {
  const requests = [];
  const fetchImpl = async (input, init) => {
    requests.push({ input, init });
    return new Response(JSON.stringify(requests.length === 1
      ? { screenId: 'screen-1', screenKey: 'sc-ABC123' }
      : { code: '123456', screenId: 'screen-1', expiresAt: '2026-07-31T01:00:00Z' }), {
      status: requests.length === 1 ? 201 : 200,
      headers: { 'Content-Type': 'application/json' }
    });
  };
  const screen = await registerPairingScreen('https://api.example.com/', fetchImpl);
  const code = await createPairingCode('https://api.example.com/', screen.screenId, fetchImpl);
  assert.equal(requests[0].input, 'https://api.example.com/api/screens');
  assert.equal(requests[1].input, 'https://api.example.com/api/screens/pairing-code');
  assert.equal(code.code, '123456');
});

test('polls status every three seconds and redirects to the encoded display route', async () => {
  let requested;
  await loadPairingStatus('', '12/3456', async input => {
    requested = input;
    return new Response(JSON.stringify({ linked: true, screenId: 'screen/1' }), {
      status: 200, headers: { 'Content-Type': 'application/json' }
    });
  });
  assert.equal(PAIRING_POLL_INTERVAL_MS, 3000);
  assert.equal(requested, '/api/screens/pairing/12%2F3456/status');
  assert.equal(displayPath('screen/1'), '/display/screen%2F1');
  assert.match(page, /window\.setInterval/);
  assert.match(page, /error\.status === 410/);
  assert.match(page, /window\.location\.replace/);
});

test('routes /pair to the pairing page', () => {
  assert.match(routes, /kind: 'pair'/);
  assert.match(routes, /\^\\\/pair/);
  assert.match(app, /<PairingPage \/>/);
});
