export const PAIRING_POLL_INTERVAL_MS = 3_000;
export const PAIRING_SCREEN_STORAGE_KEY = 'vennu.pairing.screenId';

const apiUrl = (baseUrl, path) => `${baseUrl.replace(/\/+$/, '')}${path}`;

async function jsonRequest(url, init, fetchImpl) {
  const response = await fetchImpl(url, init);
  if (!response.ok) {
    const error = new Error(`Pairing request failed with status ${response.status}.`);
    error.status = response.status;
    throw error;
  }
  return response.json();
}

export async function registerPairingScreen(baseUrl, platform = 'browser', appVersion = 'web', fetchImpl = fetch) {
  return jsonRequest(apiUrl(baseUrl, '/api/screens'), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      name: 'Vennusign TV',
      platform,
      appVersion
    })
  }, fetchImpl);
}

export async function createPairingCode(baseUrl, screenId, fetchImpl = fetch) {
  return jsonRequest(apiUrl(baseUrl, '/api/screens/pairing-code'), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ screenId })
  }, fetchImpl);
}

export async function preparePairingScreen(
  baseUrl,
  screenId,
  platform = 'browser',
  appVersion = 'web',
  fetchImpl = fetch
) {
  if (screenId) {
    try {
      const pairing = await createPairingCode(baseUrl, screenId, fetchImpl);
      return { ...pairing, screenId };
    } catch (error) {
      if (!(error instanceof Error && 'status' in error && error.status === 404)) {
        throw error;
      }
    }
  }

  const registration = await registerPairingScreen(baseUrl, platform, appVersion, fetchImpl);
  const pairing = await createPairingCode(baseUrl, registration.screenId, fetchImpl);
  return { ...pairing, screenId: registration.screenId };
}

export async function loadPairingStatus(baseUrl, code, fetchImpl = fetch) {
  return jsonRequest(
    apiUrl(baseUrl, `/api/screens/pairing/${encodeURIComponent(code)}/status`),
    undefined,
    fetchImpl
  );
}

export function displayPath(screenId) {
  return `/display/${encodeURIComponent(screenId)}`;
}
