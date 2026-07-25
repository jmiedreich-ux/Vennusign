export const DISPLAY_HEARTBEAT_INTERVAL_MS = 30_000;

export function buildDisplayHeartbeatUrl(apiBaseUrl, screenId) {
  return `${apiBaseUrl.replace(/\/$/, '')}/api/display/${encodeURIComponent(screenId)}/heartbeat`;
}

export async function sendDisplayHeartbeat(apiBaseUrl, screenId, fetchImpl = fetch, signal) {
  const response = await fetchImpl(buildDisplayHeartbeatUrl(apiBaseUrl, screenId), {
    method: 'POST',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ status: 'Online' }),
    signal
  });

  if (!response.ok) {
    throw new Error(`Display heartbeat failed with status ${response.status}.`);
  }

  return response.json();
}

export function startDisplayHeartbeat(
  apiBaseUrl,
  screenId,
  options = {}
) {
  const fetchImpl = options.fetchImpl ?? fetch;
  const setIntervalImpl = options.setIntervalImpl ?? setInterval;
  const clearIntervalImpl = options.clearIntervalImpl ?? clearInterval;
  const intervalMs = options.intervalMs ?? DISPLAY_HEARTBEAT_INTERVAL_MS;
  const abortController = new AbortController();
  let stopped = false;
  let inFlight = false;

  const send = async () => {
    if (stopped || inFlight) {
      return;
    }

    inFlight = true;
    try {
      await sendDisplayHeartbeat(apiBaseUrl, screenId, fetchImpl, abortController.signal);
    } catch {
      // Temporary heartbeat failures must not crash or duplicate the display loop.
    } finally {
      inFlight = false;
    }
  };

  void send();
  const timerId = setIntervalImpl(() => void send(), intervalMs);

  return {
    stop() {
      if (stopped) {
        return;
      }

      stopped = true;
      clearIntervalImpl(timerId);
      abortController.abort();
    }
  };
}
