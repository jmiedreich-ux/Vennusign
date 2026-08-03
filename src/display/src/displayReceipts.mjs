export function buildDisplayReceiptUrl(apiBaseUrl, screenId) {
  return `${apiBaseUrl.replace(/\/$/, '')}/api/display/${encodeURIComponent(screenId)}/content-receipts`;
}

export async function reportContentReceipt(apiBaseUrl, screenId, content, state, metadata = {}, fetchImpl = fetch) {
  if (!Number.isSafeInteger(content?.contentRevision) || content.contentRevision < 1 || !content?.screenKey) return null;
  const response = await fetchImpl(buildDisplayReceiptUrl(apiBaseUrl, screenId), {
    method: 'POST',
    headers: { Accept: 'application/json', 'Content-Type': 'application/json' },
    body: JSON.stringify({
      revision: content.contentRevision,
      state,
      screenKey: content.screenKey,
      playerVersion: metadata.playerVersion,
      shellVersion: metadata.shellVersion,
      platform: metadata.platform,
      recovered: Boolean(metadata.recovered),
      failureCode: metadata.failureCode,
      failureDetail: metadata.failureDetail
    })
  });
  if (!response.ok) throw new Error(`Content receipt failed with status ${response.status}.`);
  return response.json();
}
