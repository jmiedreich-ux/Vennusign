import assert from 'node:assert/strict';
import test from 'node:test';
import {
  DISPLAY_HEARTBEAT_INTERVAL_MS,
  buildDisplayHeartbeatUrl,
  sendDisplayHeartbeat,
  startDisplayHeartbeat
} from '../src/displayHeartbeat.mjs';

test('builds the heartbeat URL and sends the existing Online contract', async () => {
  let request;
  const fetchImpl = async (input, init) => {
    request = { input, init };
    return new Response(JSON.stringify({ status: 'Online' }), {
      status: 200,
      headers: { 'Content-Type': 'application/json' }
    });
  };

  await sendDisplayHeartbeat('https://api.example.com/', 'screen/1', fetchImpl);

  assert.equal(buildDisplayHeartbeatUrl('https://api.example.com/', 'screen/1'), 'https://api.example.com/api/display/screen%2F1/heartbeat');
  assert.equal(request.input, 'https://api.example.com/api/display/screen%2F1/heartbeat');
  assert.equal(request.init.method, 'POST');
  assert.deepEqual(JSON.parse(request.init.body), { status: 'Online' });
});

test('starts with one immediate heartbeat and one 30-second loop', async () => {
  let sends = 0;
  let scheduled;
  const fetchImpl = async () => {
    sends += 1;
    return new Response('{}', { status: 200, headers: { 'Content-Type': 'application/json' } });
  };
  const setIntervalImpl = (callback, intervalMs) => {
    scheduled = { callback, intervalMs };
    return 42;
  };

  const heartbeat = startDisplayHeartbeat('', 'screen-1', {
    fetchImpl,
    setIntervalImpl,
    clearIntervalImpl: () => {}
  });

  await Promise.resolve();
  assert.equal(sends, 1);
  assert.equal(scheduled.intervalMs, DISPLAY_HEARTBEAT_INTERVAL_MS);

  scheduled.callback();
  await Promise.resolve();
  assert.equal(sends, 2);

  heartbeat.stop();
});

test('does not overlap slow heartbeat requests', async () => {
  let resolveRequest;
  let sends = 0;
  let scheduled;
  const fetchImpl = () => {
    sends += 1;
    return new Promise((resolve) => {
      resolveRequest = () => resolve(new Response('{}', { status: 200, headers: { 'Content-Type': 'application/json' } }));
    });
  };

  const heartbeat = startDisplayHeartbeat('', 'screen-1', {
    fetchImpl,
    setIntervalImpl: (callback) => {
      scheduled = callback;
      return 7;
    },
    clearIntervalImpl: () => {}
  });

  scheduled();
  assert.equal(sends, 1);

  resolveRequest();
  await Promise.resolve();
  await Promise.resolve();

  scheduled();
  assert.equal(sends, 2);
  heartbeat.stop();
});

test('teardown clears the timer and prevents future sends', async () => {
  let sends = 0;
  let scheduled;
  let clearedTimer;
  const heartbeat = startDisplayHeartbeat('', 'screen-1', {
    fetchImpl: async () => {
      sends += 1;
      return new Response('{}', { status: 200, headers: { 'Content-Type': 'application/json' } });
    },
    setIntervalImpl: (callback) => {
      scheduled = callback;
      return 99;
    },
    clearIntervalImpl: (timerId) => {
      clearedTimer = timerId;
    }
  });

  await Promise.resolve();
  heartbeat.stop();
  scheduled();
  await Promise.resolve();

  assert.equal(clearedTimer, 99);
  assert.equal(sends, 1);
});
