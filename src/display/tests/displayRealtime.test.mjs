import assert from 'node:assert/strict';
import test from 'node:test';
import {
  applyRealtimeEvent,
  displayRealtimeEvents,
  requiresContentReload
} from '../src/displayRealtime.mjs';

const initialContent = {
  screenId: '11111111-1111-1111-1111-111111111111',
  screenName: 'Lobby Board',
  layout: 'default'
};

test('ContentUpdated replaces the complete display payload', () => {
  const replacement = { ...initialContent, layout: 'promotions' };

  assert.equal(
    applyRealtimeEvent(initialContent, displayRealtimeEvents.contentUpdated, replacement),
    replacement
  );
});

test('schedule and promotion transitions request an authoritative content reload', () => {
  assert.equal(requiresContentReload(displayRealtimeEvents.contentUpdated, { change: 'date-range-promotion-transition' }), true);
  assert.equal(requiresContentReload(displayRealtimeEvents.contentUpdated, { change: 'date-range-promotions' }), true);
  assert.equal(requiresContentReload(displayRealtimeEvents.contentUpdated, { change: 'scheduled-content-transition' }), true);
  assert.equal(requiresContentReload(displayRealtimeEvents.contentUpdated, { change: 'happy-hour-transition' }), false);
});

test('happy-hour transition patches authoritative state without replacing content', () => {
  const result = applyRealtimeEvent(initialContent, displayRealtimeEvents.contentUpdated, {
    change: 'happy-hour-transition', isHappyHour: true,
    endsAtUtc: '2026-07-30T22:00:00Z', mode: 'automatic'
  });
  assert.equal(result.screenId, initialContent.screenId);
  assert.equal(result.isHappyHour, true);
  assert.equal(result.happyHourEndsAtUtc, '2026-07-30T22:00:00Z');
});

test('emergency broadcast transition preempts without replacing playlist content', () => {
  const broadcast = { id: 'b1', title: 'Close now', message: 'Please exit', expiresUtc: '2026-07-30T09:00:00Z' };
  const result = applyRealtimeEvent(initialContent, displayRealtimeEvents.contentUpdated, {
    change: 'emergency-broadcast', emergencyBroadcast: broadcast
  });
  assert.equal(result.screenId, initialContent.screenId);
  assert.deepEqual(result.emergencyBroadcast, broadcast);
});

test('ThemeUpdated applies a deterministic theme patch', () => {
  const theme = { background: '#111111', foreground: '#ffffff' };
  const result = applyRealtimeEvent(
    initialContent,
    displayRealtimeEvents.themeUpdated,
    theme
  );

  assert.deepEqual(result, { ...initialContent, theme });
});

test('ItemAvailabilityChanged patches only the specified item', () => {
  const first = applyRealtimeEvent(
    initialContent,
    displayRealtimeEvents.itemAvailabilityChanged,
    'item-1',
    false
  );
  const second = applyRealtimeEvent(
    first,
    displayRealtimeEvents.itemAvailabilityChanged,
    'item-2',
    true
  );

  assert.deepEqual(second.itemAvailability, {
    'item-1': false,
    'item-2': true
  });
});

test('SyncTick records the latest server timestamp', () => {
  const result = applyRealtimeEvent(
    initialContent,
    displayRealtimeEvents.syncTick,
    123456789
  );

  assert.equal(result.syncTimeMs, 123456789);
});

test('unknown events leave display state unchanged', () => {
  assert.equal(applyRealtimeEvent(initialContent, 'UnknownEvent'), initialContent);
});
