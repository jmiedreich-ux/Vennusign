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
// Returns null - not a fallback width - when no positive width solves the aspect ratio: a caller
// that silently treated a failed solve as "width already correct" reintroduced the exact #790
// bug this feature exists to prevent (a board with enough rows has height growing FASTER with
// width than the target ratio does, which makes the closed-form width negative - a real,
// ordinary case, not a contrived one - and "no shrink at all" is the wrong response to it).
//
// sampleA/sampleB: { width, height } measured at two different container widths.
export function solveFitWidth(sampleA, sampleB, viewportWidth, viewportHeight) {
  const { width: w0, height: h0 } = sampleA ?? {};
  const { width: w1, height: h1 } = sampleB ?? {};

  const inputs = [w0, h0, w1, h1, viewportWidth, viewportHeight];
  if (!inputs.every((value) => Number.isFinite(value)) || w0 <= 0 || w1 <= 0 || viewportWidth <= 0 || viewportHeight <= 0 || w0 === w1) {
    return null;
  }

  const slope = (h1 - h0) / (w1 - w0);
  const targetRatio = viewportHeight / viewportWidth;
  const denominator = targetRatio - slope;

  // The line is (at or past) parallel to the target ratio - no finite width makes this board's
  // aspect ratio match the viewport's.
  if (Math.abs(denominator) < 1e-6) {
    return null;
  }

  const solvedWidth = (h0 - slope * w0) / denominator;
  return Number.isFinite(solvedWidth) && solvedWidth > 0 ? solvedWidth : null;
}

// Decides the actual {scale, width} to apply, given two width/height samples and the real
// viewport - the single place that turns solveFitWidth's math into a rendering decision, so the
// fallback logic itself is unit-tested rather than only the closed-form solve underneath it.
//
// A width-filling solution is used only when it is both real (solveFitWidth found one) and
// usable without a mismatch: applying a scale the legibility floor had to clamp, while width
// stays at the unclamped solved value, renders wider than the viewport - a different but equally
// real overflow. Either way out falls back to computeFitScale's original #790 behavior: shrink to
// fit the height alone, keep the natural width. Letterboxed-but-correct beats filled-but-broken.
export function computeBoardFit(natural, probe, viewportWidth, viewportHeight, minScale = boardFitMinScale) {
  if (!Number.isFinite(natural?.height) || !Number.isFinite(viewportHeight) || natural.height <= viewportHeight) {
    return { scale: 1, width: null };
  }

  const solvedWidth = solveFitWidth(natural, probe, viewportWidth, viewportHeight);
  if (solvedWidth !== null) {
    const rawScale = viewportWidth / solvedWidth;
    if (Number.isFinite(rawScale) && rawScale > 0 && rawScale <= 1 && rawScale >= minScale) {
      return { scale: rawScale, width: solvedWidth };
    }
  }

  return { scale: computeFitScale(natural.height, viewportHeight, minScale), width: null };
}

// Turns a BoardFit into the inline style that actually applies it - kept here, next to the
// decision logic, rather than inline in JSX, because the two transform-origins are NOT
// interchangeable and picking the wrong one for a given `width` is exactly what #802 was: a
// board shrunk by the height-only fallback (width: null, scale < 1) rendered left-justified
// instead of centered, because 'top left' was applied unconditionally in both branches.
//
// width !== null (the #794 width-fill path): the container is widened to `width` px BEFORE the
// transform, and computeBoardFit's math assumes the scaled box's top-left corner lands at the
// viewport's top-left - 'top center' here would misalign it.
//
// width === null: the container keeps its natural (viewport) width, so scaling from 'top left'
// shrinks it toward the left edge, leaving the freed space entirely on the right - only invisible
// when scale is exactly 1 (the identity transform). Once scale can be < 1 here too (the #790
// height-only fallback), 'top center' is required to keep the board visually centered.
/*
 * Two different situations both arrive here as `width: null`, and they must NOT be handled the
 * same way.
 *
 * scale === 1 is the untouched, already-fits case (the `naturalFit` constant). It left the
 * container with no width instruction at all - not "keep your natural width", but nothing. Every
 * layout in the folder sets min-height on its own root and trusts THIS container for width, so
 * "nothing" meant whatever a CSS grid's content happened to size itself to: a page with few items
 * shrank to a narrow column instead of stretching, invisible until rotation put a wide page and a
 * narrow one on the same screen seconds apart. scale(1) on 100% is a no-op, so this case now gets
 * an explicit full width, costing it nothing.
 *
 * scale < 1 with width: null is the DIFFERENT, #802 case: solveFitWidth could not solve a fill
 * width for a multi-row board, so this falls back to a pure height shrink of whatever the
 * container's natural (already viewport-filling, per #794's own reasoning) width already is.
 * Forcing width here would fight that fallback rather than help it - left untouched.
 */
export function boardFitContainerStyle({ scale, width }) {
  if (width === null) {
    return scale === 1
      ? { width: '100%', transform: `scale(${scale})`, transformOrigin: 'top center' }
      : { transform: `scale(${scale})`, transformOrigin: 'top center' };
  }
  return { width: `${width}px`, transform: `scale(${scale})`, transformOrigin: 'top left' };
}
