export const displayMediaServiceWorkerPath = '/vennu-media-sw.js';

export async function registerDisplayMediaCache(
  serviceWorkerContainer = globalThis.navigator?.serviceWorker
) {
  if (!serviceWorkerContainer) {
    return null;
  }

  return serviceWorkerContainer.register(displayMediaServiceWorkerPath);
}
