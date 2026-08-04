import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { buildPersonalizedLockedPreview, supportsPersonalizedLockedPreview } from '../src/lockedPreview.mjs';

const [app, preview, api, styles] = await Promise.all([
  readFile(new URL('../src/App.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/LockedSectionPreview.tsx', import.meta.url), 'utf8'),
  readFile(new URL('../src/api.ts', import.meta.url), 'utf8'),
  readFile(new URL('../src/styles.css', import.meta.url), 'utf8')
]);

test('personalized previews are limited to locked theme and layout capabilities', () => {
  assert.equal(supportsPersonalizedLockedPreview('all_layouts'), true);
  assert.equal(supportsPersonalizedLockedPreview('white_label'), true);
  assert.equal(supportsPersonalizedLockedPreview('html_editor'), true);
  assert.equal(supportsPersonalizedLockedPreview('happy_hour'), false);
});

test('venue menu content is bounded, ordered, and presentation-only', () => {
  const result = buildPersonalizedLockedPreview({
    menus: [{ menu: { id: 'm1', name: 'Dinner', isActive: true, dailySpecial: 'Taco Tuesday' }, sections: [
      { id: 's2', name: 'Mains', sortOrder: 2, isActive: true },
      { id: 's1', name: 'Starters', sortOrder: 1, isActive: true }
    ] }],
    itemGroups: [
      { sectionId: 's1', items: [{ name: 'Soup', price: 6, sortOrder: 1, isActive: true, isAvailable: true }] },
      { sectionId: 's2', items: [{ name: 'Burger', price: 14, sortOrder: 1, isActive: true, isAvailable: false }] }
    ]
  });
  assert.deepEqual(result, {
    menuName: 'Dinner', dailySpecial: 'Taco Tuesday', sections: [
      { id: 's1', name: 'Starters', items: [{ name: 'Soup', price: 6, available: true }] },
      { id: 's2', name: 'Mains', items: [{ name: 'Burger', price: 14, available: false }] }
    ]
  });
});

test('preview reuses the authorized venue snapshot and covers essential UI states', () => {
  assert.match(app, /venueId=\{session\.venueId\}/);
  assert.match(preview, /loadMenuEditor\(configuration, accessToken, venueId\)/);
  assert.match(api, /X-Vennusign-Back-Office-Token/);
  assert.doesNotMatch(preview, /fetch\(|create|update|delete/i);
  assert.match(preview, /'loading' \| 'ready' \| 'error'/);
  assert.match(preview, /Preview unavailable/);
  assert.match(preview, /Add active menu items/);
  assert.match(preview, /preview only/);
  assert.match(styles, /\.personalized-locked-preview/);
  assert.match(styles, /@media \(max-width: 480px\)/);
});
