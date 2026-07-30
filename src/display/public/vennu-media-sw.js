const mediaCachePrefix = 'vennu-display-media-';
const mediaCacheName = `${mediaCachePrefix}v2`;

self.addEventListener('install', () => {
  self.skipWaiting();
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys()
      .then((keys) => Promise.all(
        keys
          .filter((key) => key.startsWith(mediaCachePrefix) && key !== mediaCacheName)
          .map((key) => caches.delete(key))
      ))
      .then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', (event) => {
  const supportedDestination = ['image', 'font', 'style'].includes(event.request.destination);
  if (event.request.method !== 'GET' || !supportedDestination) {
    return;
  }

  event.respondWith(loadMedia(event.request));
});

async function loadMedia(request) {
  const cache = await caches.open(mediaCacheName);

  try {
    const response = await fetch(request);
    if (response.ok || response.type === 'opaque') {
      await cache.put(request, response.clone());
    }
    return response;
  } catch {
    return (await cache.match(request)) ?? Response.error();
  }
}
