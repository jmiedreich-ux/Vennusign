import assert from 'node:assert/strict';
import test from 'node:test';
import {
  createLayoutRegistry,
  fallbackLayoutKey,
  normalizeLayoutKey
} from '../src/layoutRegistry.mjs';

const defaultRenderer = Symbol('default');
const photoGridRenderer = Symbol('photo-grid');

function createTestRegistry() {
  return createLayoutRegistry([
    { key: 'default', label: 'Default', renderer: defaultRenderer },
    { key: 'photo_grid', label: 'Photo Grid', renderer: photoGridRenderer }
  ]);
}

test('normalizes layout keys to the stable registry format', () => {
  assert.equal(normalizeLayoutKey(' Photo-Grid '), 'photo_grid');
  assert.equal(normalizeLayoutKey(''), fallbackLayoutKey);
  assert.equal(normalizeLayoutKey(null), fallbackLayoutKey);
});

test('resolves a registered layout without falling back', () => {
  const result = createTestRegistry().resolve('PHOTO-GRID');

  assert.equal(result.key, 'photo_grid');
  assert.equal(result.isFallback, false);
  assert.equal(result.registration.renderer, photoGridRenderer);
});

test('uses the default layout for an unknown key', () => {
  const result = createTestRegistry().resolve('future-layout');

  assert.equal(result.requestedKey, 'future_layout');
  assert.equal(result.key, 'default');
  assert.equal(result.isFallback, true);
  assert.equal(result.registration.renderer, defaultRenderer);
});

test('marks a missing layout key as a default fallback', () => {
  const result = createTestRegistry().resolve('  ');

  assert.equal(result.requestedKey, 'default');
  assert.equal(result.key, 'default');
  assert.equal(result.isFallback, true);
});

test('rejects duplicate normalized keys', () => {
  assert.throws(
    () =>
      createLayoutRegistry([
        { key: 'photo-grid', label: 'First', renderer: defaultRenderer },
        { key: 'photo_grid', label: 'Second', renderer: photoGridRenderer }
      ]),
    /registered more than once/
  );
});

test('requires the configured fallback layout', () => {
  assert.throws(
    () => createLayoutRegistry([{ key: 'photo_grid', label: 'Photo Grid', renderer: photoGridRenderer }]),
    /Fallback display layout 'default' is not registered/
  );
});
