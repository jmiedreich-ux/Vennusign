/**
 * Turning the pages of one menu on a screen that holds several.
 *
 * Kept as plain functions, apart from the hook that drives them, because this is the part with
 * rules in it and rules are worth testing directly. The display's other rotation tests assert on
 * the SHAPE OF THE SOURCE - that a file contains a particular expression - which passes whatever
 * the code does at runtime. These do not.
 */

/**
 * A screen with one page has nothing to rotate to, and a cycle of one is a timer that redraws the
 * same thing forever.
 */
export function shouldRotate(pages) {
  return Array.isArray(pages) && pages.length >= 2;
}

/** The next page, wrapping at the end. */
export function advance(index, count) {
  if (!Number.isFinite(count) || count < 1) return 0;
  return ((Math.trunc(index) % count) + count + 1) % count;
}

/**
 * The page showing at this point in the cycle.
 *
 * An out-of-range index is wrapped rather than refused: the page list can shrink under a running
 * cycle when an operator unassigns a page, and a screen must keep drawing something.
 */
export function pageAt(pages, index) {
  if (!shouldRotate(pages)) return Array.isArray(pages) ? pages[0] ?? null : null;
  const count = pages.length;
  const wrapped = ((Math.trunc(index) % count) + count) % count;
  return pages[wrapped];
}

/**
 * The content the layout should draw: the same content, with the showing page's sections.
 *
 * The layout reads `sections` and does not need to know a page turned - which is what keeps this
 * out of every layout in the folder.
 */
export function contentForPage(content, index) {
  if (!content) return content;
  const pages = content.pages ?? [];
  if (!shouldRotate(pages)) return content;
  const active = pageAt(pages, index);
  return active ? { ...content, sections: active.sections } : content;
}

/** Seconds a page holds the screen. Zero or missing means the menu never said; twelve is the default. */
export function dwellSecondsFor(content) {
  const seconds = content?.pageDwellSeconds;
  return typeof seconds === 'number' && seconds >= 1 ? seconds : 12;
}
