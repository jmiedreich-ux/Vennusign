import assert from 'node:assert/strict';
import test from 'node:test';
import {
  DisplayContentError,
  buildDisplayContentUrl,
  loadDisplayContent
} from '../src/displayContent.mjs';

test('builds the routed screen content URL', () => {
  assert.equal(
    buildDisplayContentUrl('https://api.example.com', 'screen/1'),
    'https://api.example.com/api/display/screen%2F1/content'
  );
});

test('loads and returns the existing display content contract', async () => {
  const expected = {
    screenId: '11111111-1111-1111-1111-111111111111',
    venueId: null,
    screenKey: 'LOBBY-1',
    screenName: 'Lobby Board',
    status: 'Online',
    lastSeenUtc: null,
    layout: 'default'
  };

  let requests = 0;
  const fetchImpl = async () => {
    requests += 1;
    return new Response(JSON.stringify(expected), {
      status: 200,
      headers: { 'Content-Type': 'application/json' }
    });
  };

  const result = await loadDisplayContent('', expected.screenId, fetchImpl);

  assert.deepEqual(result, expected);
  assert.equal(requests, 1);
});

test('maps a 404 response to the deterministic not-found state', async () => {
  const fetchImpl = async () => new Response(null, { status: 404 });

  await assert.rejects(
    () => loadDisplayContent('', 'missing-screen', fetchImpl),
    (error) => error instanceof DisplayContentError && error.kind === 'not-found'
  );
});

test('maps other failed responses to the deterministic API-error state', async () => {
  const fetchImpl = async () => new Response(null, { status: 503 });

  await assert.rejects(
    () => loadDisplayContent('', 'screen-1', fetchImpl),
    (error) => error instanceof DisplayContentError && error.kind === 'api-error'
  );
});

test('maps network failures to the deterministic API-error state', async () => {
  const fetchImpl = async () => {
    throw new TypeError('network down');
  };

  await assert.rejects(
    () => loadDisplayContent('', 'screen-1', fetchImpl),
    (error) => error instanceof DisplayContentError && error.kind === 'api-error'
  );
});
