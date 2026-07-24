import assert from 'node:assert/strict';
import test from 'node:test';
import { startDisplayConnection } from '../src/displayConnection.mjs';

function createFakeConnection() {
  const calls = [];
  const callbacks = {};

  return {
    calls,
    callbacks,
    async start() {
      calls.push(['start']);
    },
    async invoke(method, ...args) {
      calls.push(['invoke', method, ...args]);
    },
    async stop() {
      calls.push(['stop']);
    },
    onreconnecting(callback) {
      callbacks.reconnecting = callback;
    },
    onreconnected(callback) {
      callbacks.reconnected = callback;
    },
    onclose(callback) {
      callbacks.close = callback;
    }
  };
}

test('joins the correct screen immediately after connection', async () => {
  const connection = createFakeConnection();
  const states = [];

  await startDisplayConnection(connection, 'screen-123', (state) => states.push(state));

  assert.deepEqual(connection.calls, [
    ['start'],
    ['invoke', 'JoinScreen', 'screen-123']
  ]);
  assert.deepEqual(states, ['connecting', 'connected']);
});

test('reconnection restores the same screen group membership', async () => {
  const connection = createFakeConnection();
  const states = [];

  await startDisplayConnection(connection, 'screen-123', (state) => states.push(state));
  connection.callbacks.reconnecting();
  await connection.callbacks.reconnected();

  assert.deepEqual(connection.calls, [
    ['start'],
    ['invoke', 'JoinScreen', 'screen-123'],
    ['invoke', 'JoinScreen', 'screen-123']
  ]);
  assert.deepEqual(states, ['connecting', 'connected', 'reconnecting', 'connected']);
});

test('connection failures produce a controlled degraded state', async () => {
  const connection = createFakeConnection();
  const states = [];
  connection.start = async () => {
    throw new Error('offline');
  };

  await startDisplayConnection(connection, 'screen-123', (state) => states.push(state));

  assert.deepEqual(states, ['connecting', 'degraded']);
});
