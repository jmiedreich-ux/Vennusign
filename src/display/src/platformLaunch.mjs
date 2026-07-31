export const supportedTvPlatforms = Object.freeze(['android_tv', 'fire_tv', 'tizen', 'webos']);

function normalizePlatform(value) {
  return supportedTvPlatforms.includes(value) ? value : 'browser';
}

function cleanVersion(value, fallback) {
  const version = typeof value === 'string' ? value.trim().slice(0, 50) : '';
  return version || fallback;
}

export function readPlatformBootstrap(search) {
  const parameters = new URLSearchParams(search);
  const platform = normalizePlatform(parameters.get('vennuPlatform'));
  if (platform === 'browser') return undefined;
  return {
    platform,
    appVersion: cleanVersion(parameters.get('vennuVersion'), 'unknown')
  };
}

export function resolvePlatformLaunch(pathname, bridge, resetPairing = false) {
  const platform = normalizePlatform(bridge?.platform);
  const appVersion = cleanVersion(bridge?.appVersion, platform === 'browser' ? 'web' : 'unknown');
  const screenId = typeof bridge?.screenId === 'string' && bridge.screenId.trim()
    ? bridge.screenId.trim()
    : undefined;

  if (resetPairing && /^\/pair\/?$/i.test(pathname)) {
    return { platform, appVersion, pathname: '/pair' };
  }

  if (platform !== 'browser' && screenId) {
    return { platform, appVersion, pathname: `/display/${encodeURIComponent(screenId)}` };
  }

  if (platform !== 'browser' && /^\/?$/i.test(pathname)) {
    return { platform, appVersion, pathname: '/pair' };
  }

  return { platform, appVersion, pathname };
}
