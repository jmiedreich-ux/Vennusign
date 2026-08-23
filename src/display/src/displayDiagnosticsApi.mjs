export function buildDisplayDiagnosticsUrl(apiBaseUrl, screenId) {
  return `${apiBaseUrl.replace(/\/$/, '')}/api/display/${encodeURIComponent(screenId)}/diagnostics`;
}

export async function loadServerDiagnostics(apiBaseUrl, screenId, fetchImpl = fetch) {
  let response;
  try {
    response = await fetchImpl(buildDisplayDiagnosticsUrl(apiBaseUrl, screenId), {
      method: 'GET',
      headers: { Accept: 'application/json' }
    });
  } catch {
    return { kind: 'error', message: 'The diagnostics service could not be reached.' };
  }

  if (response.status === 404) {
    return { kind: 'not-found' };
  }

  if (!response.ok) {
    return { kind: 'error', message: `The diagnostics service returned ${response.status}.` };
  }

  return { kind: 'ok', diagnostics: await response.json() };
}
