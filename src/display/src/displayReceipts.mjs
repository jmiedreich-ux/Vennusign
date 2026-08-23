export function buildDisplayReceiptUrl(apiBaseUrl, screenId) {
  return `${apiBaseUrl.replace(/\/$/, '')}/api/display/${encodeURIComponent(screenId)}/content-receipts`;
}

// reportContentReceipt returns null both when it skips (no revision or screenKey) and when the
// caller chooses to swallow a failure - callers that only check for null cannot tell those apart,
// which is how a screen serving null contentRevision came to look identical to one working fine.
// This names the skip reason so a diagnostics view can show it instead of an absence.
export function describeReceiptSkipReason(content) {
  if (!Number.isSafeInteger(content?.contentRevision) || content.contentRevision < 1) {
    return 'no-content-revision';
  }
  if (!content?.screenKey) {
    return 'no-screen-key';
  }
  return null;
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
