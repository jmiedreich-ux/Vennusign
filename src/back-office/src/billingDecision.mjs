const pendingKey = 'vennusign.billing.pending-tier';
export const pendingTierStaleAfterMs = 30 * 60 * 1000;

export function writePendingTierDecision(targetTier, storage = sessionStorage, now = Date.now()) {
  const value = { targetTierId: targetTier.id, targetTierName: targetTier.name, requestedUtc: new Date(now).toISOString() };
  storage.setItem(pendingKey, JSON.stringify(value));
  return value;
}

export function readPendingTierDecision(storage = sessionStorage) {
  try {
    const value = JSON.parse(storage.getItem(pendingKey) ?? 'null');
    return value && typeof value.targetTierId === 'string' && typeof value.targetTierName === 'string' &&
      Number.isFinite(Date.parse(value.requestedUtc)) ? value : undefined;
  } catch { return undefined; }
}

export function clearPendingTierDecision(storage = sessionStorage) { storage.removeItem(pendingKey); }

export function resolvePendingTierDecision(pending, currentTierId, now = Date.now()) {
  if (!pending) return undefined;
  if (pending.targetTierId === currentTierId) return 'applied';
  return now - Date.parse(pending.requestedUtc) >= pendingTierStaleAfterMs ? 'stale' : 'pending';
}
