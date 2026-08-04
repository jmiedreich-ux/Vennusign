import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';
import {
  notoFontFamilies,
  preloadNotoFonts,
  preloadThemeFonts,
  themeFontFamilies
} from '../src/notoFonts.mjs';

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

test('every player theme family is locally bundled and preloaded at its required weights', async () => {
  const imports = await readFile(new URL('../src/themeFonts.ts', import.meta.url), 'utf8');
  const packages = [
    'inter',
    'pacifico',
    'lobster',
    'righteous',
    'fredoka-one',
    'bungee',
    'permanent-marker',
    'caveat',
    'kalam',
    'patrick-hand',
    'noto-sans-sc',
    'noto-sans-kr',
    'noto-sans-jp',
    'noto-sans-arabic'
  ];
  for (const packageName of packages) {
    assert.match(imports, new RegExp(`@fontsource/${packageName}/`));
  }

  const requests = [];
  const result = await preloadThemeFonts({
    load: async descriptor => {
      requests.push(descriptor);
      return [];
    }
  });

  assert.equal(result.length, themeFontFamilies.reduce((count, font) => count + font.weights.length, 0));
  for (const font of themeFontFamilies) {
    for (const weight of font.weights) {
      assert.ok(requests.includes(`${weight} 1em "${font.family}"`));
    }
  }
});

test('font assets are compiled locally and use the versioned offline media path', async () => {
  const html = await readFile(new URL('../index.html', import.meta.url), 'utf8');
  const main = await readFile(new URL('../src/main.tsx', import.meta.url), 'utf8');
  const worker = await readFile(new URL('../public/vennu-media-sw.js', import.meta.url), 'utf8');
  assert.doesNotMatch(html, /fonts\.(googleapis|gstatic)\.com/);
  assert.match(main, /import '\.\/themeFonts'/);
  assert.match(main, /preloadThemeFonts/);
  assert.match(worker, /'image', 'font', 'style'/);
  assert.match(worker, /vennu-display-media-/);
  assert.match(worker, /v2/);
});
