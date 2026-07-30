import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';
import {
  buildDisplayContentCacheKey,
  cacheDisplayContent,
  displayContentCacheMaxAgeMs,
  loadDisplayContentResilient,
  readCachedDisplayContent
} from '../src/displayCache.mjs';
import { DisplayContentError } from '../src/displayContent.mjs';
import {
  displayMediaServiceWorkerPath,
  registerDisplayMediaCache
} from '../src/mediaCache.mjs';

const screenId = '11111111-1111-1111-1111-111111111111';
const content = {
  screenId,
  venueId: null,
  screenKey: 'LOBBY-1',
  screenName: 'Lobby',
  status: 'Online',
  lastSeenUtc: null,
  layout: 'photo_grid'
};

test('successful online content replaces the versioned screen cache', async () => {
  const storage = createStorage();
  const result = await loadDisplayContentResilient('', screenId, {
    storage,
    now: 100,
    fetchImpl: async () => Response.json(content)
  });

  assert.equal(result.source, 'network');
  assert.deepEqual(readCachedDisplayContent(screenId, storage, 100)?.content, content);
});

test('an API outage falls back to fresh content for the same screen', async () => {
  const storage = createStorage();
  cacheDisplayContent(screenId, content, storage, 100);

  const result = await loadDisplayContentResilient('', screenId, {
    storage,
    now: 200,
    fetchImpl: async () => {
      throw new TypeError('offline');
    }
  });

  assert.equal(result.source, 'cache');
  assert.deepEqual(result.content, content);
  assert.equal(result.cachedAt, 100);
});

test('stale, corrupt, and cross-screen entries are invalidated', () => {
  const staleStorage = createStorage();
  cacheDisplayContent(screenId, content, staleStorage, 100);
  assert.equal(
    readCachedDisplayContent(screenId, staleStorage, 100 + displayContentCacheMaxAgeMs + 1),
    null
  );

  const corruptStorage = createStorage();
  corruptStorage.setItem(buildDisplayContentCacheKey(screenId), '{');
  assert.equal(readCachedDisplayContent(screenId, corruptStorage, 100), null);

  const otherScreenStorage = createStorage();
  otherScreenStorage.setItem(buildDisplayContentCacheKey(screenId), JSON.stringify({
    version: 1,
    screenId,
    cachedAt: 100,
    content: { ...content, screenId: 'another-screen' }
  }));
  assert.equal(readCachedDisplayContent(screenId, otherScreenStorage, 100), null);
});

test('a not-found response never revives cached content', async () => {
  const storage = createStorage();
  cacheDisplayContent(screenId, content, storage, 100);

  await assert.rejects(
    () => loadDisplayContentResilient('', screenId, {
      storage,
      now: 200,
      fetchImpl: async () => new Response(null, { status: 404 })
    }),
    (error) => error instanceof DisplayContentError && error.kind === 'not-found'
  );
});

test('media cache registration uses the versioned player worker', async () => {
  let registeredPath;
  const expected = {};
  const result = await registerDisplayMediaCache({
    register: async (path) => {
      registeredPath = path;
      return expected;
    }
  });

  assert.equal(registeredPath, displayMediaServiceWorkerPath);
  assert.equal(result, expected);
});

test('media worker cleans old versions and recovers cached images offline', async () => {
  const worker = await readFile(
    new URL('../public/vennu-media-sw.js', import.meta.url),
    'utf8'
  );

  assert.match(worker, /vennu-display-media-/);
  assert.match(worker, /caches\.delete/);
  assert.match(worker, /event\.request\.destination !== 'image'/);
  assert.match(worker, /await fetch\(request\)/);
  assert.match(worker, /await cache\.match\(request\)/);
});

function createStorage() {
  const values = new Map();
  return {
    get length() {
      return values.size;
    },
    key(index) {
      return [...values.keys()][index] ?? null;
    },
    getItem(key) {
      return values.get(key) ?? null;
    },
    setItem(key, value) {
      values.set(key, value);
    },
    removeItem(key) {
      values.delete(key);
    }
  };
}
