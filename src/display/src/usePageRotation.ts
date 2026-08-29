import { useEffect, useState } from 'react';
import type { DisplayContent } from './displayContent.mjs';
import { advance, contentForPage, dwellSecondsFor, shouldRotate } from './pageRotation.mjs';

/**
 * A screen holding more than one page shows each in turn.
 *
 * The back office has promised this on its assignment page all along — "a screen holding more than
 * one page rotates between them" — while the API sent one page and the player had no cycle at all,
 * so four of an operator's five pages never appeared. This is the cycle; `DisplayController` now
 * sends every assigned page to feed it.
 *
 * Deliberately NOT the playlist rotator beside it: that cycles promotional slides and hands off for
 * menus. This turns the pages of one menu, which is a different thing on a different clock.
 *
 * One page, or none, returns the content untouched — a screen with nothing to rotate to must not
 * be handed a timer that redraws it forever.
 */
export function useRotatedContent(content: DisplayContent | undefined): DisplayContent | undefined {
  // Called before the content has arrived as well as after: a hook cannot sit behind the
  // component's early returns, so it copes with there being nothing to rotate yet.
  const pages = content?.pages ?? [];
  const [index, setIndex] = useState(0);

  // The identity of the rotation, not of the object: a poll that returns the same pages must not
  // restart the cycle, or a slow dwell would never reach its second page.
  const key = pages.map(page => page.pageId).join('|');
  const dwellSeconds = dwellSecondsFor(content);
  const rotating = shouldRotate(pages);

  useEffect(() => setIndex(0), [key]);

  useEffect(() => {
    if (!rotating) return;
    const timer = window.setTimeout(
      () => setIndex(current => advance(current, pages.length)),
      dwellSeconds * 1000
    );
    return () => window.clearTimeout(timer);
  }, [index, key, dwellSeconds, rotating, pages.length]);

  return content ? contentForPage(content, index) : content;
}
