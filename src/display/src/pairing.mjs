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

export async function registerPairingScreen(baseUrl, fetchImpl = fetch) {
  return jsonRequest(apiUrl(baseUrl, '/api/screens'), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      name: 'Vennu TV',
      platform: 'web-tv',
      appVersion: '1.0'
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
