import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';
import {
  describeCachedContent,
  getConnectionPresentation,
  getDisplayStatePresentation
} from '../src/displayPresentation.mjs';

test('player states provide visible, actionable loading and failure presentation', () => {
  const loading = getDisplayStatePresentation('loading');
  const unavailable = getDisplayStatePresentation('api-error');

  assert.equal(loading.busy, true);
  assert.equal(loading.tone, 'loading');
  assert.match(loading.message, /loading/i);
  assert.equal(unavailable.busy, false);
  assert.equal(unavailable.tone, 'error');
  assert.equal(unavailable.actionLabel, 'Try again');
  assert.match(unavailable.message, /network connection/i);
});

test('offline copy reports saved-content age and recovery behavior', () => {
  const now = Date.parse('2026-08-03T23:20:00Z');

  assert.equal(
    describeCachedContent(now - 60_000, now),
    'Offline — showing saved content from 1 minute ago. New updates will appear when the connection returns.'
  );
  assert.match(describeCachedContent(now - 2 * 60 * 60_000, now), /2 hours ago/);
});

test('connection transitions distinguish live, recovering, and degraded states', () => {
  assert.deepEqual(getConnectionPresentation('connected'), {
    label: 'Live updates connected', tone: 'online', visible: false
  });
  assert.equal(getConnectionPresentation('reconnecting').tone, 'working');
  assert.equal(getConnectionPresentation('degraded').tone, 'offline');
  assert.equal(getConnectionPresentation('degraded').visible, true);
});

test('heartbeat motion has an explicit reduced-motion override', async () => {
  const css = await readFile(new URL('../src/player.css', import.meta.url), 'utf8');
  assert.match(css, /@media \(prefers-reduced-motion: reduce\)/);
  assert.match(css, /player-state-screen__heartbeat[^{]*\{ animation: none;/);
});
