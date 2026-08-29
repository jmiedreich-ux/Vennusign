import assert from 'node:assert/strict';
import test from 'node:test';

import { advance, contentForPage, dwellSecondsFor, pageAt, shouldRotate } from '../src/pageRotation.mjs';

/*
 * The owner assigned five pages to one screen, published, and watched it show Appetizers forever.
 * The API sent one page (DisplayController took FirstOrDefault of the assignments) and the player
 * had no cycle - while the back office promised, on the assignment page, that "a screen holding
 * more than one page rotates between them".
 *
 * These test the rules, by running them. The display's other rotation test reads its source and
 * matches expressions against it, which says nothing about what happens at runtime.
 */

const page = (id, name) => ({ pageId: id, name, sections: [{ id: `${id}-s`, name, items: [] }] });

test('one page is not a rotation', () => {
  assert.equal(shouldRotate([]), false);
  assert.equal(shouldRotate([page('a', 'Apps')]), false);
  assert.equal(shouldRotate([page('a', 'Apps'), page('b', 'Mains')]), true);
});

test('the cycle wraps at the end', () => {
  assert.equal(advance(0, 3), 1);
  assert.equal(advance(1, 3), 2);
  assert.equal(advance(2, 3), 0);
});

test('a cycle of one never moves, so a single page is never redrawn on a timer', () => {
  assert.equal(advance(0, 1), 0);
  assert.equal(advance(5, 1), 0);
});

test('the showing page is the one the index names', () => {
  const pages = [page('a', 'Apps'), page('b', 'Mains'), page('c', 'Puddings')];
  assert.equal(pageAt(pages, 0).name, 'Apps');
  assert.equal(pageAt(pages, 1).name, 'Mains');
  assert.equal(pageAt(pages, 2).name, 'Puddings');
});

test('an index past the end wraps rather than blanking the screen', () => {
  // The list shrinks under a running cycle when an operator unassigns a page. A screen must keep
  // drawing something; going blank because the index outran the array is the worse answer.
  const pages = [page('a', 'Apps'), page('b', 'Mains')];
  assert.equal(pageAt(pages, 7).name, 'Mains');
  assert.equal(pageAt(pages, -1).name, 'Mains');
});

test('the layout is handed the showing page as plain sections', () => {
  const content = {
    menuName: 'Dinner',
    sections: [{ id: 'first', name: 'Apps', items: [] }],
    pages: [page('a', 'Apps'), page('b', 'Mains')]
  };

  assert.equal(contentForPage(content, 1).sections[0].name, 'Mains');

  // Everything else about the content survives - the layout gets a menu, not a page.
  assert.equal(contentForPage(content, 1).menuName, 'Dinner');
});

test('content with nothing to rotate is handed back untouched', () => {
  const single = { sections: [{ id: 'only', name: 'Apps', items: [] }], pages: [page('a', 'Apps')] };
  assert.equal(contentForPage(single, 3), single);

  const none = { sections: [{ id: 'only', name: 'Apps', items: [] }] };
  assert.equal(contentForPage(none, 1), none);
});

test('dwell comes from the menu, and falls back rather than spinning', () => {
  assert.equal(dwellSecondsFor({ pageDwellSeconds: 20 }), 20);
  assert.equal(dwellSecondsFor({ pageDwellSeconds: 0 }), 12);
  assert.equal(dwellSecondsFor({}), 12);
  assert.equal(dwellSecondsFor(undefined), 12);
});
