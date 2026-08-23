import assert from 'node:assert/strict';
import test from 'node:test';
import {
  buildDisplayDiagnosticsKey,
  readDisplayDiagnostics,
  recordDisplayDiagnosticEvent,
  displayDiagnosticsMaxEvents
} from '../src/displayDiagnostics.mjs';

function memoryStorage() {
  const map = new Map();
  return {
    getItem: (key) => (map.has(key) ? map.get(key) : null),
    setItem: (key, value) => map.set(key, value),
    removeItem: (key) => map.delete(key)
  };
}

test('reads an empty record when nothing has been written yet', () => {
  const record = readDisplayDiagnostics('screen-1', memoryStorage());
  assert.deepEqual(record.events, []);
  assert.deepEqual(record.latest, {});
});

test('records an event and reads it back under both the timeline and the latest-by-kind map', () => {
  const storage = memoryStorage();
  recordDisplayDiagnosticEvent('screen-1', 'content-fetch', { source: 'network' }, storage, 1000);

  const record = readDisplayDiagnostics('screen-1', storage);
  assert.equal(record.events.length, 1);
  assert.equal(record.events[0].kind, 'content-fetch');
  assert.equal(record.events[0].at, 1000);
  assert.deepEqual(record.latest['content-fetch'].detail, { source: 'network' });
});

test('a different screen id on the same device never sees another screen\'s history', () => {
  const storage = memoryStorage();
  recordDisplayDiagnosticEvent('screen-1', 'heartbeat', { ok: true }, storage, 1000);

  const other = readDisplayDiagnostics('screen-2', storage);
  assert.deepEqual(other.events, []);
});

test('the timeline is capped so an always-on screen does not grow its localStorage forever', () => {
  const storage = memoryStorage();
  for (let index = 0; index < displayDiagnosticsMaxEvents + 5; index += 1) {
    recordDisplayDiagnosticEvent('screen-1', 'connection', { state: 'connected' }, storage, index);
  }

  const record = readDisplayDiagnostics('screen-1', storage);
  assert.equal(record.events.length, displayDiagnosticsMaxEvents);
  assert.equal(record.events.at(-1).at, displayDiagnosticsMaxEvents + 4);
});

test('latest-by-kind tracks each kind independently', () => {
  const storage = memoryStorage();
  recordDisplayDiagnosticEvent('screen-1', 'heartbeat', { ok: true }, storage, 1000);
  recordDisplayDiagnosticEvent('screen-1', 'content-fetch', { source: 'cache' }, storage, 1001);
  recordDisplayDiagnosticEvent('screen-1', 'heartbeat', { ok: false }, storage, 1002);

  const record = readDisplayDiagnostics('screen-1', storage);
  assert.deepEqual(record.latest.heartbeat.detail, { ok: false });
  assert.deepEqual(record.latest['content-fetch'].detail, { source: 'cache' });
  assert.equal(record.events.length, 3);
});

test('a corrupted or foreign record is treated as empty rather than thrown', () => {
  const storage = memoryStorage();
  storage.setItem(buildDisplayDiagnosticsKey('screen-1'), '{not json');
  const record = readDisplayDiagnostics('screen-1', storage);
  assert.deepEqual(record.events, []);
});

test('reading and writing with no storage available does not throw', () => {
  recordDisplayDiagnosticEvent('screen-1', 'heartbeat', { ok: true }, undefined, 1000);
  const record = readDisplayDiagnostics('screen-1', undefined);
  assert.deepEqual(record.events, []);
});
