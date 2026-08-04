export function updateIdentityDraft(current, screen, patch) {
  return { name: current?.name ?? screen.name, location: current?.location ?? screen.location ?? '', ...patch };
}

export function identityHasChanges(screen, draft) {
  return Boolean(draft) && (draft.name !== screen.name || draft.location !== (screen.location ?? ''));
}

export function updateScreenPresentationDraft(current, screen, patch) {
  return {
    displayLayout: current?.displayLayout ?? screen.displayLayout,
    photoGridDensity: current?.photoGridDensity ?? screen.photoGridDensity,
    splitRatio: current?.splitRatio ?? screen.splitRatio,
    heroDwellSeconds: current?.heroDwellSeconds ?? screen.heroDwellSeconds,
    ...patch
  };
}

export function screenPresentationHasChanges(screen, draft) {
  return Boolean(draft) && (
    draft.displayLayout !== screen.displayLayout
    || draft.photoGridDensity !== screen.photoGridDensity
    || draft.splitRatio !== screen.splitRatio
    || draft.heroDwellSeconds !== screen.heroDwellSeconds
  );
}

export function passkeyInventoryView({ loading, failed, count }) {
  if (loading) return 'loading';
  if (failed) return 'failed';
  return count === 0 ? 'empty' : 'loaded';
}
