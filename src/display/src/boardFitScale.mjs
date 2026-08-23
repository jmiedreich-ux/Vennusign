// Every layout sets `min-height: 100vh` on its own root and nothing else - a floor, not a
// ceiling, so a board taller than the viewport just grows past it with no scroll and no
// indicator (#790). This is the generic fix, above all layouts rather than inside any one of
// them: shrink the whole rendered board uniformly until it fits, rather than losing content off
// the bottom. Deliberately never scales UP - a short board stays at its natural size.
export const boardFitMinScale = 0.4;

export function clampScale(scale, minScale = boardFitMinScale) {
  if (!Number.isFinite(scale) || scale <= 0) {
    return 1;
  }

  return Math.min(1, Math.max(minScale, scale));
}

export function computeFitScale(contentHeight, viewportHeight, minScale = boardFitMinScale) {
  if (!Number.isFinite(contentHeight) || !Number.isFinite(viewportHeight) || contentHeight <= 0 || viewportHeight <= 0) {
    return 1;
  }

  return clampScale(viewportHeight / contentHeight, minScale);
}

// A uniform scale that only corrects for height (computeFitScale) shrinks width by the same
// factor, wasting real width the board never needed to give up (#794). This solves for the
// container width a uniformly-scaled board needs so it renders at EXACTLY the viewport's width
// once scaled, while still fitting the viewport's height.
//
// The model is exact, not approximate, because every width-independent part of a layout's CSS -
// rem-based padding/gaps, vw-based clamp() font sizes (vw tracks the real browser viewport, not
// this container) - contributes a constant term regardless of the container's width. The only
// part that grows with container width is each card's aspect-ratio-locked media box, which is
// exactly linear in width. So rendered height truly is an affine function of container width,
// and two samples at two different widths pin that line down exactly - this is a closed-form
// solve, not an iterative approximation, and it terminates in one step.
//
// sampleA/sampleB: { width, height } measured at two different container widths.
export function solveFitWidth(sampleA, sampleB, viewportWidth, viewportHeight) {
  const { width: w0, height: h0 } = sampleA ?? {};
  const { width: w1, height: h1 } = sampleB ?? {};

  const inputs = [w0, h0, w1, h1, viewportWidth, viewportHeight];
  if (!inputs.every((value) => Number.isFinite(value)) || w0 <= 0 || w1 <= 0 || viewportWidth <= 0 || viewportHeight <= 0 || w0 === w1) {
    return viewportWidth;
  }

  const slope = (h1 - h0) / (w1 - w0);
  const targetRatio = viewportHeight / viewportWidth;
  const denominator = targetRatio - slope;

  // The line is (at or past) parallel to the target ratio - no finite width makes this board's
  // aspect ratio match the viewport's. Falls back to the natural (unmodified) width; the caller's
  // height-only scale still applies on top of that, so nothing is lost, just not width-filled.
  if (Math.abs(denominator) < 1e-6) {
    return viewportWidth;
  }

  const solvedWidth = (h0 - slope * w0) / denominator;
  return Number.isFinite(solvedWidth) && solvedWidth > 0 ? solvedWidth : viewportWidth;
}
