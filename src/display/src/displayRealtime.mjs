export const displayRealtimeEvents = Object.freeze({
  contentUpdated: 'ContentUpdated',
  themeUpdated: 'ThemeUpdated',
  itemAvailabilityChanged: 'ItemAvailabilityChanged',
  syncTick: 'SyncTick'
});

export function applyRealtimeEvent(content, eventName, ...args) {
  switch (eventName) {
    case displayRealtimeEvents.contentUpdated:
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
