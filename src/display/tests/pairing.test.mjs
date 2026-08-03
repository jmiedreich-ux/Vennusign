import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';
import {
  PAIRING_POLL_INTERVAL_MS,
  createPairingCode,
  displayPath,
  loadPairingStatus,
  preparePairingScreen,
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
  const screen = await registerPairingScreen('https://api.example.com/', 'android_tv', '2.1.0', fetchImpl);
  const code = await createPairingCode('https://api.example.com/', screen.screenId, fetchImpl);
  assert.equal(requests[0].input, 'https://api.example.com/api/screens');
  assert.deepEqual(JSON.parse(requests[0].init.body), {
    name: 'Vennusign TV', platform: 'android_tv', appVersion: '2.1.0'
  });
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

test('re-registers when the persisted screen was deleted', async () => {
  const requests = [];
  const fetchImpl = async (input, init) => {
    requests.push({ input, init });
    if (requests.length === 1) return new Response(null, { status: 404 });
    if (requests.length === 2) {
      return new Response(JSON.stringify({ screenId: 'replacement-screen', screenKey: 'sc-NEW123' }), {
        status: 201,
        headers: { 'Content-Type': 'application/json' }
      });
    }
    return new Response(JSON.stringify({
      code: '654321', screenId: 'replacement-screen', expiresAt: '2026-08-03T23:00:00Z'
    }), {
      status: 201,
      headers: { 'Content-Type': 'application/json' }
    });
  };

  const pairing = await preparePairingScreen(
    'https://api.example.com',
    'deleted-screen',
    'browser',
    'web',
    fetchImpl
  );

  assert.equal(requests.length, 3);
  assert.deepEqual(JSON.parse(requests[0].init.body), { screenId: 'deleted-screen' });
  assert.equal(requests[1].input, 'https://api.example.com/api/screens');
  assert.deepEqual(JSON.parse(requests[2].init.body), { screenId: 'replacement-screen' });
  assert.equal(pairing.screenId, 'replacement-screen');
  assert.equal(pairing.code, '654321');
  assert.match(page, /pairing\.screenId !== screenId/);
  assert.match(page, /localStorage\.setItem/);
});

test('routes /pair to the pairing page', () => {
  assert.match(routes, /kind: 'pair'/);
  assert.match(routes, /\^\\\/pair/);
  assert.match(app, /<PairingPage/);
});
