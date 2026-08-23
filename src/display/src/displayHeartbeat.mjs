export const DISPLAY_HEARTBEAT_INTERVAL_MS = 30_000;

export function buildDisplayHeartbeatUrl(apiBaseUrl, screenId) {
  return `${apiBaseUrl.replace(/\/$/, '')}/api/display/${encodeURIComponent(screenId)}/heartbeat`;
}

export async function sendDisplayHeartbeat(apiBaseUrl, screenId, fetchImpl = fetch, signal, metadata = {}) {
  const response = await fetchImpl(buildDisplayHeartbeatUrl(apiBaseUrl, screenId), {
    method: 'POST',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ status: 'Online', platform: metadata.platform, appVersion: metadata.appVersion }),
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
      await sendDisplayHeartbeat(
        apiBaseUrl,
        screenId,
        fetchImpl,
        abortController.signal,
        { platform: options.platform, appVersion: options.appVersion }
      );
      options.onResult?.({ ok: true });
    } catch (error) {
      // Temporary heartbeat failures must not crash or duplicate the display loop - but the
      // failure is worth recording, not just swallowing, or it takes an hour of log-correlating
      // to notice a screen has stopped reporting itself at all.
      options.onResult?.({ ok: false, message: error instanceof Error ? error.message : 'Heartbeat failed.' });
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
