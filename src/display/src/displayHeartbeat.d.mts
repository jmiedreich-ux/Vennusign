export const DISPLAY_HEARTBEAT_INTERVAL_MS: number;

export type DisplayHeartbeatOptions = {
  platform?: string;
  appVersion?: string;
  fetchImpl?: typeof fetch;
  setIntervalImpl?: typeof setInterval;
  clearIntervalImpl?: typeof clearInterval;
  intervalMs?: number;
  onResult?: (result: { ok: boolean; message?: string }) => void;
};

export type DisplayHeartbeat = {
  stop: () => void;
};

export function buildDisplayHeartbeatUrl(apiBaseUrl: string, screenId: string): string;

export function sendDisplayHeartbeat(
  apiBaseUrl: string,
  screenId: string,
  fetchImpl?: typeof fetch,
  signal?: AbortSignal,
  metadata?: { platform?: string; appVersion?: string }
): Promise<unknown>;

export function startDisplayHeartbeat(
  apiBaseUrl: string,
  screenId: string,
  options?: DisplayHeartbeatOptions
): DisplayHeartbeat;
