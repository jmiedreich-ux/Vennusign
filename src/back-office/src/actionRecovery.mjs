export function updateIdentityDraft(current, screen, patch) {
  return { name: current?.name ?? screen.name, location: current?.location ?? screen.location ?? '', ...patch };
}

export function identityHasChanges(screen, draft) {
  return Boolean(draft) && (draft.name !== screen.name || draft.location !== (screen.location ?? ''));
}

export function passkeyInventoryView({ loading, failed, count }) {
  if (loading) return 'loading';
  if (failed) return 'failed';
  return count === 0 ? 'empty' : 'loaded';
}
