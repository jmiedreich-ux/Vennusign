export type DisplayConnectionState = 'connecting' | 'connected' | 'reconnecting' | 'degraded';

export type DisplayConnectionLike = {
  invoke(methodName: string, ...args: unknown[]): Promise<unknown>;
  onreconnecting(callback: () => void): void;
  onreconnected(callback: () => void | Promise<void>): void;
  onclose(callback: () => void): void;
  start(): Promise<void>;
  stop(): Promise<void>;
};

export type StartedDisplayConnection = {
  stop: () => Promise<void>;
};

export function startDisplayConnection(
  connection: DisplayConnectionLike,
  screenId: string,
  onStateChanged: (state: DisplayConnectionState) => void
): Promise<StartedDisplayConnection>;
