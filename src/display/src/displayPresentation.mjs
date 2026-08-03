const statePresentations = {
  loading: {
    eyebrow: 'Vennusign player',
    title: 'Getting your screen ready',
    message: 'Loading the latest display content and checking the live connection.',
    busy: true,
    tone: 'loading'
  },
  'not-found': {
    eyebrow: 'Screen setup',
    title: 'Display not found',
    message: 'This screen is not available. Confirm the display link or pair the player again.',
    busy: false,
    tone: 'error',
    actionLabel: 'Try again'
  },
  'api-error': {
    eyebrow: 'Connection problem',
    title: 'Display unavailable',
    message: 'The player could not load display content. Check the network connection, then try again.',
    busy: false,
    tone: 'error',
    actionLabel: 'Try again'
  },
  provisioning: {
    eyebrow: 'Vennusign player',
    title: 'Preparing this screen',
    message: 'Claiming the pre-registered player and checking its display assignment.',
    busy: true,
    tone: 'loading'
  },
  'provisioning-error': {
    eyebrow: 'Screen setup',
    title: 'Provisioning unavailable',
    message: 'This delivery token could not be used. Contact Vennusign support for a new token.',
    busy: false,
    tone: 'error'
  },
  'route-not-found': {
    eyebrow: 'Screen setup',
    title: 'Display not found',
    message: 'Open a display link with a valid screen identifier or pair this player again.',
    busy: false,
    tone: 'error'
  },
  unexpected: {
    eyebrow: 'Player error',
    title: 'Display unavailable',
    message: 'An unexpected error interrupted the player. Reload it to try again.',
    busy: false,
    tone: 'error',
    actionLabel: 'Reload player'
  }
};

export function getDisplayStatePresentation(kind) {
  return statePresentations[kind] ?? statePresentations.unexpected;
}

export function describeCachedContent(cachedAt, now = Date.now()) {
  const elapsedMinutes = Math.max(0, Math.floor((now - cachedAt) / 60_000));
  let age = 'just now';

  if (elapsedMinutes >= 1 && elapsedMinutes < 60) {
    age = `${elapsedMinutes} minute${elapsedMinutes === 1 ? '' : 's'} ago`;
  } else if (elapsedMinutes >= 60) {
    const elapsedHours = Math.floor(elapsedMinutes / 60);
    age = `${elapsedHours} hour${elapsedHours === 1 ? '' : 's'} ago`;
  }

  return `Offline — showing saved content from ${age}. New updates will appear when the connection returns.`;
}

export function getConnectionPresentation(state) {
  switch (state) {
    case 'connecting':
      return { label: 'Connecting live updates', tone: 'working', visible: true };
    case 'reconnecting':
      return { label: 'Live updates paused — reconnecting', tone: 'working', visible: true };
    case 'degraded':
      return { label: 'Live updates unavailable — current content remains on screen', tone: 'offline', visible: true };
    default:
      return { label: 'Live updates connected', tone: 'online', visible: false };
  }
}
