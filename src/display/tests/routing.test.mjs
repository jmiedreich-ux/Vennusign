import assert from 'node:assert/strict';
import test from 'node:test';
import { resolveDisplayRoute } from '../src/routing.ts';

test('resolves the pairing and provisioning routes', () => {
  assert.deepEqual(resolveDisplayRoute('/pair'), { kind: 'pair' });
  assert.deepEqual(resolveDisplayRoute('/provision'), { kind: 'provision' });
});

test('resolves a display route to its screen id', () => {
  assert.deepEqual(resolveDisplayRoute('/display/29b71c98-2063-4315-ba7b-69120159644b'), {
    kind: 'display',
    screenId: '29b71c98-2063-4315-ba7b-69120159644b'
  });
});

test('resolves the diagnostics route for the same screen id, distinct from the display route', () => {
  assert.deepEqual(resolveDisplayRoute('/display/29b71c98-2063-4315-ba7b-69120159644b/diag'), {
    kind: 'diagnostics',
    screenId: '29b71c98-2063-4315-ba7b-69120159644b'
  });
});

test('the diagnostics route tolerates a trailing slash, matching every other route here', () => {
  assert.deepEqual(resolveDisplayRoute('/display/abc/diag/'), { kind: 'diagnostics', screenId: 'abc' });
});

test('an unrecognized path is not-found, including a display path with extra segments', () => {
  assert.deepEqual(resolveDisplayRoute('/display/abc/unknown'), { kind: 'not-found' });
  assert.deepEqual(resolveDisplayRoute('/nothing'), { kind: 'not-found' });
});

test('a URL-encoded screen id round-trips through both the display and diagnostics routes', () => {
  assert.deepEqual(resolveDisplayRoute('/display/sc%2F1'), { kind: 'display', screenId: 'sc/1' });
  assert.deepEqual(resolveDisplayRoute('/display/sc%2F1/diag'), { kind: 'diagnostics', screenId: 'sc/1' });
});
