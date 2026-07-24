import assert from 'node:assert/strict';
import test from 'node:test';
import {
  applyRealtimeEvent,
  displayRealtimeEvents
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
