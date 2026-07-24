import {
  HubConnectionBuilder,
  LogLevel,
  type HubConnection
} from '@microsoft/signalr';
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
  return `${apiBaseUrl.replace(/\/$/, '')}/hubs/vennu`;
}

async function joinScreen(connection: HubConnection, screenId: string) {
  await connection.invoke('JoinScreen', screenId);
}

export async function connectDisplayRealtime(
  apiBaseUrl: string,
  screenId: string,
  handlers: DisplayRealtimeHandlers
): Promise<DisplayRealtimeConnection> {
  handlers.onConnectionStateChanged('connecting');

  const connection = new HubConnectionBuilder()
    .withUrl(buildHubUrl(apiBaseUrl))
    .withAutomaticReconnect()
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

  connection.onreconnecting(() => handlers.onConnectionStateChanged('reconnecting'));
  connection.onreconnected(async () => {
    try {
      await joinScreen(connection, screenId);
      handlers.onConnectionStateChanged('connected');
    } catch {
      handlers.onConnectionStateChanged('degraded');
    }
  });
  connection.onclose(() => handlers.onConnectionStateChanged('degraded'));

  try {
    await connection.start();
    await joinScreen(connection, screenId);
    handlers.onConnectionStateChanged('connected');
  } catch {
    handlers.onConnectionStateChanged('degraded');
  }

  return {
    stop: () => connection.stop()
  };
}
