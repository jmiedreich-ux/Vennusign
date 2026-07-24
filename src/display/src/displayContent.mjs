export class DisplayContentError extends Error {
  constructor(kind, message) {
    super(message);
    this.name = 'DisplayContentError';
    this.kind = kind;
  }
}

export function buildDisplayContentUrl(apiBaseUrl, screenId) {
  return `${apiBaseUrl}/api/display/${encodeURIComponent(screenId)}/content`;
}

export async function loadDisplayContent(apiBaseUrl, screenId, fetchImpl = fetch) {
  let response;

  try {
    response = await fetchImpl(buildDisplayContentUrl(apiBaseUrl, screenId), {
      method: 'GET',
      headers: { Accept: 'application/json' }
    });
  } catch {
    throw new DisplayContentError('api-error', 'The display service could not be reached.');
  }

  if (response.status === 404) {
    throw new DisplayContentError('not-found', 'This screen could not be found.');
  }

  if (!response.ok) {
    throw new DisplayContentError('api-error', 'The display content could not be loaded.');
  }

  return response.json();
}
