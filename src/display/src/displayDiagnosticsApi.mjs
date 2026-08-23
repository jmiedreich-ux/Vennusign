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

  // A 2xx response is not proof of a JSON body - a misconfigured proxy or a wrong apiBaseUrl
  // answering 200 with an HTML error page is exactly the class of bug this endpoint exists to
  // diagnose (#731), so it has to degrade to an error result here too, not throw past the caller.
  try {
    return { kind: 'ok', diagnostics: await response.json() };
  } catch {
    return { kind: 'error', message: 'The diagnostics service returned a response that was not valid JSON.' };
  }
}
