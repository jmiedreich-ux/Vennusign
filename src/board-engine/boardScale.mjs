/*
 * The one logical size every board is laid out at, and the arithmetic that fits
 * it into a box.
 *
 * Kept apart from the React component so the sizing can be reasoned about and
 * tested without a browser — the component only observes a box and applies the
 * number this returns.
 */

/** 1920x1080: the shape of the screens this renders for. */
export const boardLogicalWidth = 1920;

export const boardLogicalHeight = 1080;

export const boardAspectRatio = boardLogicalWidth / boardLogicalHeight;

/**
 * How much to scale the logical board so it fills the box's width without
 * distortion, cropping the bottom when the box is shorter than the board.
 *
 * Width first, deliberately. Fitting by the tighter of the two dimensions keeps
 * the shape but letterboxes: a card with changes waiting reserves 30px at the
 * bottom for its amber strip, and the board would then shrink to fit that
 * shorter box - rendering 34% smaller than the card beside it, with a blank
 * gutter down one side. The card that most needs attention would be the one that
 * stopped looking like the TV.
 *
 * So a short box crops instead, and the stage's `transform-origin: top left`
 * makes that crop top-aligned: a board loses its bottom, never its heading, which
 * is what Q191 asks for and what board-engine.css already claimed.
 *
 * The aspect ratio never changes - one scale for both axes - so nothing is ever
 * stretched. A box with no measurable size yet scales to zero rather than one:
 * zero draws nothing for a frame, one would flash a full 1920px board through a
 * 300px card first.
 */
export function scaleToFit(boxWidth, boxHeight) {
  const width = positive(boxWidth);
  const height = positive(boxHeight);
  if (width === 0 || height === 0) return 0;

  // Width alone: a wider-than-16:9 box crops the bottom, a taller one simply
  // leaves space below. Either way the board is drawn at the size the card is
  // wide, which is what makes two cards side by side comparable.
  return width / boardLogicalWidth;
}

function positive(value) {
  return typeof value === "number" && Number.isFinite(value) && value > 0 ? value : 0;
}
