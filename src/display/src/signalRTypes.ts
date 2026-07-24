export const displayRealtimeEvents = {
  contentUpdated: 'ContentUpdated',
  themeUpdated: 'ThemeUpdated',
  itemAvailabilityChanged: 'ItemAvailabilityChanged',
  syncTick: 'SyncTick'
} as const;

export type DisplayRealtimeEventName =
  (typeof displayRealtimeEvents)[keyof typeof displayRealtimeEvents];
