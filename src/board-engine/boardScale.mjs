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
 * How much to scale the logical board so it fits the box without distortion.
 *
 * Fits by the tighter of the two dimensions, so the board keeps its shape: a
 * board stretched to fill a box would show text at proportions no real screen
 * ever displays, and a card is supposed to be a picture of the TV.
 *
 * A box with no measurable size yet - the first paint, or a hidden tab - scales
 * to zero rather than to one. Zero draws nothing for a frame; one would flash a
 * full-size 1920px board through a 300px card first.
 */
export function scaleToFit(boxWidth, boxHeight) {
  const width = positive(boxWidth);
  const height = positive(boxHeight);
  if (width === 0 || height === 0) return 0;

  return Math.min(width / boardLogicalWidth, height / boardLogicalHeight);
}

function positive(value) {
  return typeof value === "number" && Number.isFinite(value) && value > 0 ? value : 0;
}
