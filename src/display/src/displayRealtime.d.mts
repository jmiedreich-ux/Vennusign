export const displayRealtimeEvents: Readonly<{
  contentUpdated: 'ContentUpdated';
  themeUpdated: 'ThemeUpdated';
  itemAvailabilityChanged: 'ItemAvailabilityChanged';
  syncTick: 'SyncTick';
}>;

export function applyRealtimeEvent<T>(
  content: T,
  eventName: string,
  ...args: unknown[]
): T;
