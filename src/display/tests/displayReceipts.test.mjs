import assert from 'node:assert/strict';
import test from 'node:test';
import { buildDisplayReceiptUrl, reportContentReceipt, describeReceiptSkipReason } from '../src/displayReceipts.mjs';

test('reports the exact authoritative revision and player compatibility metadata', async () => {
  let request;
  const fetchImpl = async (input, init) => { request = { input, init }; return { ok: true, json: async () => ({ state: 'Applied' }) }; };
  await reportContentReceipt('https://api.example/', 'screen/1', { contentRevision: 7, screenKey: 'ABC123XYZ' }, 'Applied',
    { playerVersion: '1.7.0', shellVersion: '2.1.0', platform: 'tizen', recovered: true }, fetchImpl);
  assert.equal(buildDisplayReceiptUrl('https://api.example/', 'screen/1'), 'https://api.example/api/display/screen%2F1/content-receipts');
  assert.deepEqual(JSON.parse(request.init.body), {
    revision: 7, state: 'Applied', screenKey: 'ABC123XYZ', playerVersion: '1.7.0', shellVersion: '2.1.0',
    platform: 'tizen', recovered: true
  });
});

test('does not send a receipt for an unversioned cached snapshot', async () => {
  let calls = 0;
  const result = await reportContentReceipt('', 'screen', { screenKey: 'ABC123XYZ' }, 'Applied', {}, async () => { calls++; });
  assert.equal(result, null);
  assert.equal(calls, 0);
});

test('names why a receipt is skipped, so a null result is not indistinguishable from a swallowed failure', () => {
  assert.equal(describeReceiptSkipReason({ screenKey: 'ABC123XYZ', contentRevision: null }), 'no-content-revision');
  assert.equal(describeReceiptSkipReason({ screenKey: 'ABC123XYZ', contentRevision: 0 }), 'no-content-revision');
  assert.equal(describeReceiptSkipReason({ screenKey: '', contentRevision: 7 }), 'no-screen-key');
  assert.equal(describeReceiptSkipReason({ screenKey: 'ABC123XYZ', contentRevision: 7 }), null);
});
