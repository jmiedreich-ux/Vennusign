const apiUrl = (baseUrl, path) => `${baseUrl.replace(/\/+$/, '')}${path}`;

export async function claimPreRegisteredScreen(baseUrl, token, platform, appVersion, fetchImpl = fetch) {
  const response = await fetchImpl(apiUrl(baseUrl, '/api/screens/pre-registration/claim'), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ token, platform, appVersion })
  });
  if (!response.ok) {
    throw new Error(`Pre-registration claim failed with status ${response.status}.`);
  }
  return response.json();
}
