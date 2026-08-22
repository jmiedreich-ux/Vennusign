import { test } from 'node:test';
import assert from 'node:assert/strict';
import {
  displayRetryPolicy,
  nextReconnectDelayMs,
  RECONNECT_CEILING_MS
} from '../src/displayReconnect.mjs';

test('the opening cadence matches what SignalR would have done on its own', () => {
  assert.equal(nextReconnectDelayMs(0), 0);
  assert.equal(nextReconnectDelayMs(1), 2_000);
  assert.equal(nextReconnectDelayMs(2), 10_000);
  assert.equal(nextReconnectDelayMs(3), 30_000);
});

test('it never stops trying, which is the whole point', () => {
  // SignalR's own policy returns null on the fifth call and the connection is
  // closed for good. A screen on a wall has nobody to reload it, so past the ramp
  // this holds at the ceiling rather than giving up.
  for (const attempt of [4, 5, 20, 500, 100_000]) {
    assert.equal(nextReconnectDelayMs(attempt), RECONNECT_CEILING_MS);
  }
});

test('the ceiling matches the content recovery poll', () => {
  // Past a minute a screen is being carried by DISPLAY_CONTENT_RECOVERY_INTERVAL_MS
  // anyway, so backing off further buys nothing and only lengthens the outage.
  assert.equal(RECONNECT_CEILING_MS, 60_000);
});

test('the policy object is shaped the way SignalR calls it', () => {
  assert.equal(typeof displayRetryPolicy.nextRetryDelayInMilliseconds, 'function');
  assert.equal(displayRetryPolicy.nextRetryDelayInMilliseconds({ previousRetryCount: 0 }), 0);
  assert.equal(
    displayRetryPolicy.nextRetryDelayInMilliseconds({ previousRetryCount: 9 }),
    RECONNECT_CEILING_MS
  );
});
