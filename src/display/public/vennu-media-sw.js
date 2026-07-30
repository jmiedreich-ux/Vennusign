const mediaCachePrefix = 'vennu-display-media-';
const mediaCacheName = `${mediaCachePrefix}v1`;

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
  if (event.request.method !== 'GET' || event.request.destination !== 'image') {
    return;
  }

  event.respondWith(loadImage(event.request));
});

async function loadImage(request) {
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
