import {
  HubConnectionBuilder,
  LogLevel
} from '@microsoft/signalr';
// @ts-expect-error - plain .mjs helper, kept testable by the display's node --test suite.
import { displayRetryPolicy } from './displayReconnect.mjs';
import { startDisplayConnection } from './displayConnection.mjs';
import {
  displayRealtimeEvents,
  type DisplayRealtimeEventName
} from './signalRTypes';

export type DisplayConnectionState = 'connecting' | 'connected' | 'reconnecting' | 'degraded';

export type DisplayRealtimeHandlers = {
  onConnectionStateChanged: (state: DisplayConnectionState) => void;
  onEvent: (eventName: DisplayRealtimeEventName, ...args: unknown[]) => void;
};

export type DisplayRealtimeConnection = {
  stop: () => Promise<void>;
};

function buildHubUrl(apiBaseUrl: string) {
  return `${apiBaseUrl.replace(/\/$/, '')}/hubs/vennusign`;
}


export async function connectDisplayRealtime(
  apiBaseUrl: string,
  screenId: string,
  handlers: DisplayRealtimeHandlers
): Promise<DisplayRealtimeConnection> {
  const connection = new HubConnectionBuilder()
    .withUrl(buildHubUrl(apiBaseUrl))
    .withAutomaticReconnect(displayRetryPolicy)
    .configureLogging(LogLevel.Warning)
    .build();

  connection.on(displayRealtimeEvents.contentUpdated, (payload: unknown) =>
    handlers.onEvent(displayRealtimeEvents.contentUpdated, payload)
  );
  connection.on(displayRealtimeEvents.themeUpdated, (theme: unknown) =>
    handlers.onEvent(displayRealtimeEvents.themeUpdated, theme)
  );
  connection.on(
    displayRealtimeEvents.itemAvailabilityChanged,
    (itemId: string, available: boolean) =>
      handlers.onEvent(displayRealtimeEvents.itemAvailabilityChanged, itemId, available)
  );
  connection.on(displayRealtimeEvents.syncTick, (serverTimeMs: number) =>
    handlers.onEvent(displayRealtimeEvents.syncTick, serverTimeMs)
  );

  return startDisplayConnection(
    connection,
    screenId,
    handlers.onConnectionStateChanged
  );
}
