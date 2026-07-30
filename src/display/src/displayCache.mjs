import { DisplayContentError, loadDisplayContent } from './displayContent.mjs';

export const displayContentCacheVersion = 1;
export const displayContentCacheMaxAgeMs = 7 * 24 * 60 * 60 * 1000;

const cachePrefix = 'vennu:display-content:';

export function buildDisplayContentCacheKey(screenId) {
  return `${cachePrefix}v${displayContentCacheVersion}:${screenId}`;
}

export function cacheDisplayContent(
  screenId,
  content,
  storage = globalThis.localStorage,
  now = Date.now()
) {
  if (!storage) {
    return;
  }

  removeLegacyDisplayContent(screenId, storage);
  storage.setItem(buildDisplayContentCacheKey(screenId), JSON.stringify({
    version: displayContentCacheVersion,
    screenId,
    cachedAt: now,
    content
  }));
}

export function readCachedDisplayContent(
  screenId,
  storage = globalThis.localStorage,
  now = Date.now()
) {
  if (!storage) {
    return null;
  }

  removeLegacyDisplayContent(screenId, storage);
  const key = buildDisplayContentCacheKey(screenId);
  const serialized = storage.getItem(key);

  if (!serialized) {
    return null;
  }

  try {
    const cached = JSON.parse(serialized);
    const valid = cached.version === displayContentCacheVersion
      && cached.screenId === screenId
      && cached.content?.screenId === screenId
      && Number.isFinite(cached.cachedAt)
      && now - cached.cachedAt <= displayContentCacheMaxAgeMs;

    if (!valid) {
      storage.removeItem(key);
      return null;
    }

    return cached;
  } catch {
    storage.removeItem(key);
    return null;
  }
}

export async function loadDisplayContentResilient(
  apiBaseUrl,
  screenId,
  options = {}
) {
  const {
    fetchImpl = fetch,
    storage = globalThis.localStorage,
    now = Date.now()
  } = options;

  try {
    const content = await loadDisplayContent(apiBaseUrl, screenId, fetchImpl);
    cacheDisplayContent(screenId, content, storage, now);
    return { content, source: 'network', cachedAt: now };
  } catch (error) {
    if (!(error instanceof DisplayContentError) || error.kind !== 'api-error') {
      throw error;
    }

    const cached = readCachedDisplayContent(screenId, storage, now);
    if (!cached) {
      throw error;
    }

    return {
      content: cached.content,
      source: 'cache',
      cachedAt: cached.cachedAt
    };
  }
}

function removeLegacyDisplayContent(screenId, storage) {
  for (let index = storage.length - 1; index >= 0; index -= 1) {
    const key = storage.key(index);
    if (key?.startsWith(cachePrefix)
      && key.endsWith(`:${screenId}`)
      && key !== buildDisplayContentCacheKey(screenId)) {
      storage.removeItem(key);
    }
  }
}
