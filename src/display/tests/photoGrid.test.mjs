import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const layoutSource = await readFile(new URL('../src/layouts/PhotoGridLayout.tsx', import.meta.url), 'utf8');
const registrySource = await readFile(new URL('../src/layouts/DisplayLayout.tsx', import.meta.url), 'utf8');
const styleSource = await readFile(new URL('../src/layouts/photoGrid.css', import.meta.url), 'utf8');

test('registers Photo Grid through the additive layout registry', () => {
  assert.match(registrySource, /key: 'photo_grid'/);
  assert.match(registrySource, /renderer: PhotoGridLayout/);
});

test('renders venue, menu, sections, item copy, image, and price contracts', () => {
  for (const contract of ['content.venueName', 'content.menuName', 'section.name', 'item.name', 'item.description', 'item.imageUrl', 'item.price']) {
    assert.match(layoutSource, new RegExp(contract.replace('.', '\\.')));
  }
  assert.match(layoutSource, /loading="lazy"/);
  assert.match(layoutSource, /photo-grid__placeholder/);
});

test('uses a responsive card grid and bounded food-photo crop', () => {
  assert.match(styleSource, /grid-template-columns: repeat\(auto-fit/);
  assert.match(styleSource, /aspect-ratio: 16 \/ 10/);
  assert.match(styleSource, /object-fit: cover/);
});

test('renders merchandising states only from display content', () => {
  for (const contract of ['item.isPopular', 'item.isAvailable', 'item.quantityAvailable', 'item.tags', 'content.isHappyHour', 'item.happyHourPrice']) {
    assert.match(layoutSource, new RegExp(contract.replace('.', '\\.')));
  }
  assert.match(layoutSource, /photo-grid__sold-out/);
  assert.match(layoutSource, /photo-grid__popular/);
  assert.match(layoutSource, /photo-grid__badge/);
  assert.match(layoutSource, /<s>/);
  assert.match(layoutSource, /limitedQuantityThreshold = 5/);
});
