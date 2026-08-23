// A rolling record of what THIS device's player has actually experienced, since none of it
// survives the DisplayPage effect closure otherwise. Written as the player runs; read back by
// /display/{screenId}/diag on the same device. Deliberately per-device: a laptop reading this
// key for a screen it never ran would show that laptop's own (empty) history as if it were the
// wall's, which is worse than showing nothing.
export const displayDiagnosticsVersion = 1;
export const displayDiagnosticsMaxEvents = 20;

const keyPrefix = 'vennu:display-diagnostics:';

export function buildDisplayDiagnosticsKey(screenId) {
  return `${keyPrefix}v${displayDiagnosticsVersion}:${screenId}`;
}

function emptyRecord(screenId) {
  return {
    version: displayDiagnosticsVersion,
    screenId,
    events: [],
    latest: {}
  };
}

export function readDisplayDiagnostics(screenId, storage = globalThis.localStorage) {
  if (!storage) return emptyRecord(screenId);

  const serialized = storage.getItem(buildDisplayDiagnosticsKey(screenId));
  if (!serialized) return emptyRecord(screenId);

  try {
    const parsed = JSON.parse(serialized);
    if (parsed?.version !== displayDiagnosticsVersion || parsed?.screenId !== screenId) {
      return emptyRecord(screenId);
    }
    return { ...emptyRecord(screenId), ...parsed };
  } catch {
    return emptyRecord(screenId);
  }
}

// kind is one of: 'content-fetch' | 'heartbeat' | 'receipt' | 'connection'. detail is a plain,
// JSON-serializable object describing what happened - never content, never PII.
export function recordDisplayDiagnosticEvent(
  screenId,
  kind,
  detail,
  storage = globalThis.localStorage,
  now = Date.now()
) {
  if (!storage) return;

  const record = readDisplayDiagnostics(screenId, storage);
  const event = { kind, at: now, detail };

  record.events = [...record.events, event].slice(-displayDiagnosticsMaxEvents);
  record.latest = { ...record.latest, [kind]: event };

  storage.setItem(buildDisplayDiagnosticsKey(screenId), JSON.stringify(record));
}
