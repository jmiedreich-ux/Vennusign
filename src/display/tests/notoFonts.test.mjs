import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';
import { notoFontFamilies, preloadNotoFonts } from '../src/notoFonts.mjs';

test('defines the four approved Noto families with deterministic fallbacks', () => {
  assert.deepEqual(notoFontFamilies.map(font => font.family), [
    'Noto Sans SC',
    'Noto Sans KR',
    'Noto Sans JP',
    'Noto Sans Arabic'
  ]);
  for (const font of notoFontFamilies) {
    assert.match(font.fallback, /sans-serif$/);
  }
});

test('preloads regular and bold faces for every approved family', async () => {
  const requests = [];
  const result = await preloadNotoFonts({
    load: async descriptor => {
      requests.push(descriptor);
      return [];
    }
  });

  assert.equal(result.length, 8);
  assert.equal(requests.length, 8);
  for (const font of notoFontFamilies) {
    assert.ok(requests.includes(`400 1em "${font.family}"`));
    assert.ok(requests.includes(`700 1em "${font.family}"`));
  }
});

test('font styles and assets use the versioned offline media path', async () => {
  const html = await readFile(new URL('../index.html', import.meta.url), 'utf8');
  const worker = await readFile(new URL('../public/vennu-media-sw.js', import.meta.url), 'utf8');
  for (const family of ['Noto+Sans+SC', 'Noto+Sans+KR', 'Noto+Sans+JP', 'Noto+Sans+Arabic']) {
    assert.ok(html.includes(family));
  }
  assert.match(html, /rel="preload"/);
  assert.match(worker, /'image', 'font', 'style'/);
  assert.match(worker, /vennu-display-media-/);
  assert.match(worker, /v2/);
});
