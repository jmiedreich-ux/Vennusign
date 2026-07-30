export const displayRealtimeEvents = Object.freeze({
  contentUpdated: 'ContentUpdated',
  themeUpdated: 'ThemeUpdated',
  itemAvailabilityChanged: 'ItemAvailabilityChanged',
  syncTick: 'SyncTick'
});

export function requiresContentReload(eventName, payload) {
  return eventName === displayRealtimeEvents.contentUpdated
    && ['date-range-promotions', 'date-range-promotion-transition', 'scheduled-content-transition']
      .includes(payload?.change);
}

export function applyRealtimeEvent(content, eventName, ...args) {
  switch (eventName) {
    case displayRealtimeEvents.contentUpdated:
      if (args[0]?.change === 'happy-hour-transition') {
        return {
          ...content,
          isHappyHour: Boolean(args[0].isHappyHour),
          happyHourEndsAtUtc: args[0].endsAtUtc ?? null,
          happyHourMode: args[0].mode ?? 'automatic'
        };
      }
      if (args[0]?.change === 'emergency-broadcast') {
        return { ...content, emergencyBroadcast: args[0].emergencyBroadcast ?? null };
      }
      return args[0];

    case displayRealtimeEvents.themeUpdated:
      return {
        ...content,
        theme: args[0]
      };

    case displayRealtimeEvents.itemAvailabilityChanged: {
      const [itemId, available] = args;
      return {
        ...content,
        itemAvailability: {
          ...(content.itemAvailability ?? {}),
          [itemId]: Boolean(available)
        }
      };
    }

    case displayRealtimeEvents.syncTick:
      return {
        ...content,
        syncTimeMs: args[0]
      };

    default:
      return content;
  }
}
