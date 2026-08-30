import assert from 'node:assert/strict';
import test from 'node:test';
import { computeFitScale, boardFitMinScale, solveFitWidth, clampScale, computeBoardFit, boardFitContainerStyle } from '../src/boardFitScale.mjs';

test('never scales up a board that already fits', () => {
  assert.equal(computeFitScale(900, 1080), 1);
  assert.equal(computeFitScale(1080, 1080), 1);
});

test('shrinks a board exactly to the ratio measured on the live QA screen (#790)', () => {
  const scale = computeFitScale(1354, 1080);
  assert.ok(Math.abs(scale - 1080 / 1354) < 1e-9);
  assert.ok(scale < 1);
});

test('never shrinks below the legibility floor, even for an extremely tall board', () => {
  assert.equal(computeFitScale(20000, 1080), boardFitMinScale);
});

test('is fail-safe (scale 1, no distortion) on invalid or not-yet-measured input', () => {
  assert.equal(computeFitScale(NaN, 1080), 1);
  assert.equal(computeFitScale(1354, NaN), 1);
  assert.equal(computeFitScale(0, 1080), 1);
  assert.equal(computeFitScale(1354, 0), 1);
  assert.equal(computeFitScale(undefined, 1080), 1);
});

test('clampScale never exceeds 1 and never goes below the floor', () => {
  assert.equal(clampScale(1.4), 1);
  assert.equal(clampScale(0.01), boardFitMinScale);
  assert.equal(clampScale(0.7), 0.7);
  assert.equal(clampScale(NaN), 1);
  assert.equal(clampScale(-1), 1);
});

// #794: a uniform scale that only corrects height also shrinks width by the same factor,
// wasting real width that was never overflowing. solveFitWidth finds the container width that,
// once uniformly scaled to fit the viewport height, ALSO renders at exactly the viewport width.
test('solveFitWidth recovers the exact container width for a linear height-vs-width model', () => {
  // h(w) = fixed + k*w - matching two stacked rows of 16:10 images across 3 grid columns,
  // where only the image portion of each card grows with container width (rem-based
  // padding/gaps and vw-based clamp() fonts are constant with respect to container width).
  const k = 0.4167;
  const w0 = 1920, h0 = 1354;
  const fixed = h0 - k * w0;
  const w1 = w0 * 1.5;
  const h1 = fixed + k * w1;

  const viewportWidth = 1920, viewportHeight = 1080;
  const solvedWidth = solveFitWidth({ width: w0, height: h0 }, { width: w1, height: h1 }, viewportWidth, viewportHeight);
  assert.notEqual(solvedWidth, null);
  const scale = clampScale(viewportWidth / solvedWidth);

  // Rendering the board at solvedWidth and then scaling it down uniformly must land on EXACTLY
  // the viewport's width and height - this is a closed-form solve, not an approximation.
  const renderedHeight = (fixed + k * solvedWidth) * scale;
  const renderedWidth = solvedWidth * scale;
  assert.ok(Math.abs(renderedWidth - viewportWidth) < 0.01, `renderedWidth was ${renderedWidth}`);
  assert.ok(Math.abs(renderedHeight - viewportHeight) < 0.01, `renderedHeight was ${renderedHeight}`);
  assert.ok(scale < 1080 / 1354, 'filling the width requires shrinking further than the height-only fix did');
});

test('solveFitWidth returns null (not a fabricated width) on invalid input', () => {
  assert.equal(solveFitWidth({ width: 1920, height: 1354 }, { width: 1920, height: 1354 }, 1920, 1080), null);
  assert.equal(solveFitWidth({ width: NaN, height: 1354 }, { width: 2880, height: 1800 }, 1920, 1080), null);
  assert.equal(solveFitWidth(undefined, { width: 2880, height: 1800 }, 1920, 1080), null);
  assert.equal(solveFitWidth({ width: 1920, height: 1354 }, { width: 2880, height: 1800 }, 0, 1080), null);
});

test('solveFitWidth still finds a width when height does not depend on width at all (a flat line)', () => {
  // slope 0 - the same natural height at any width (no aspect-ratio-locked media at all). The
  // rendered aspect ratio still changes with the chosen container width even though height
  // itself does not, so a solution exists: pick the width whose h/w ratio already matches the
  // viewport's, i.e. exactly computeFitScale's own answer, expressed as a width instead of a
  // scale.
  const width = solveFitWidth({ width: 1920, height: 1354 }, { width: 2880, height: 1354 }, 1920, 1080);
  assert.ok(Math.abs(width - (1354 * 1920) / 1080) < 0.01);
});

test('solveFitWidth returns null when the line is parallel to the target ratio - genuinely no finite solution', () => {
  // slope exactly equals viewportHeight/viewportWidth: every width renders at the same aspect
  // ratio as the target, so scale is already 1 at every width and there is no distinguished
  // width to solve for - not a bug, a real degenerate case.
  const targetRatio = 1080 / 1920;
  const width = solveFitWidth({ width: 1920, height: 1920 * targetRatio }, { width: 2880, height: 2880 * targetRatio }, 1920, 1080);
  assert.equal(width, null);
});

// The regression an independent review of #796 found live: a board with enough rows has height
// growing FASTER with width than the target ratio does (slope > targetRatio) - widening it makes
// the aspect ratio worse, not better, so no positive width ever solves it. This is an ORDINARY
// case (any board with more than ~2 rows of aspect-ratio-locked cards), not a contrived one.
test('solveFitWidth returns null when height grows faster with width than the target ratio (a real multi-row board)', () => {
  // Reproduces the live-reproduced case: naturalWidth=1920, naturalHeight=2911, probe
  // width=2880, height=3465 - a 12-item, 4-row photo_grid board.
  const width = solveFitWidth({ width: 1920, height: 2911 }, { width: 2880, height: 3465 }, 1920, 1080);
  assert.equal(width, null, 'a negative or nonsensical "solved" width must not be returned as if it were real');
});

test('computeBoardFit leaves an already-fitting board untouched', () => {
  const fit = computeBoardFit({ width: 1920, height: 900 }, { width: 2880, height: 1200 }, 1920, 1080);
  assert.deepEqual(fit, { scale: 1, width: null });
});

test('computeBoardFit fills the width when a clean solution exists', () => {
  const k = 0.4167;
  const w0 = 1920, h0 = 1354;
  const fixed = h0 - k * w0;
  const w1 = w0 * 1.5;
  const h1 = fixed + k * w1;

  const fit = computeBoardFit({ width: w0, height: h0 }, { width: w1, height: h1 }, 1920, 1080);
  assert.notEqual(fit.width, null);
  assert.ok(fit.scale > 0 && fit.scale <= 1);
  const renderedWidth = fit.width * fit.scale;
  const renderedHeight = (fixed + k * fit.width) * fit.scale;
  assert.ok(Math.abs(renderedWidth - 1920) < 0.01);
  assert.ok(Math.abs(renderedHeight - 1080) < 0.01);
});

// This is the exact bug the independent review caught: falling back to {scale: 1, width:
// viewportWidth} whenever solveFitWidth can't find a usable answer reintroduces #790's original
// defect (a board can be 2-3x the viewport's height and render completely unshrunk). The correct
// fallback is the height-only #790 behavior - letterboxed, but with nothing lost off the bottom.
test('computeBoardFit falls back to height-only shrink (never scale=1) when no width-filling solution exists', () => {
  const natural = { width: 1920, height: 2911 };
  const probe = { width: 2880, height: 3465 };
  const fit = computeBoardFit(natural, probe, 1920, 1080);

  assert.equal(fit.width, null, 'no width override - the fallback is the natural width, letterboxed');
  assert.ok(fit.scale < 1, `scale must shrink to fit - got ${fit.scale}, the un-fixed bug returned exactly 1`);
  // The raw ratio here (1080/2911 ≈ 0.371) is itself below the legibility floor, so the correct
  // fallback is computeFitScale's own floor-clamped answer, not the raw unclamped ratio.
  assert.equal(fit.scale, computeFitScale(natural.height, 1080), 'must match computeFitScale exactly - same fallback the original #790 fix used');
});

test('computeBoardFit falls back to height-only shrink when a width-filling solution exists but its scale would need clamping', () => {
  // A very tall board with a shallow width-dependence: solveFitWidth finds a mathematically
  // valid width, but the scale it implies falls below the legibility floor. Applying that
  // (unclamped) width at a (clamped) scale would overflow horizontally - a different but
  // equally real defect - so this must fall back rather than apply a mismatched pair.
  const k = 0.05;
  const w0 = 1920, h0 = 5000;
  const fixed = h0 - k * w0;
  const w1 = w0 * 1.5;
  const h1 = fixed + k * w1;

  const fit = computeBoardFit({ width: w0, height: h0 }, { width: w1, height: h1 }, 1920, 1080);

  assert.equal(fit.width, null);
  assert.equal(fit.scale, boardFitMinScale);
});

test('computeBoardFit is fail-safe on invalid input, matching computeFitScale rather than fabricating a fit', () => {
  const fit = computeBoardFit({ width: NaN, height: NaN }, { width: 2880, height: 1800 }, 1920, 1080);
  assert.equal(fit.scale, 1);
  assert.equal(fit.width, null);
});

// #802: a real venue's board (natural=1920x2911, probe=2880x3465 - the exact multi-row case that
// makes solveFitWidth return null, tested above) fell back to {scale: <1, width: null} exactly as
// designed, but the JSX applied 'top left' to it anyway - shrinking the board toward the left
// edge instead of the center, so all the freed horizontal space appeared as a gap on the right.
test('boardFitContainerStyle centers a height-only shrink (width: null, scale < 1) rather than left-justifying it (#802)', () => {
  const fit = computeBoardFit({ width: 1920, height: 2911 }, { width: 2880, height: 3465 }, 1920, 1080);
  assert.ok(fit.width === null && fit.scale < 1, 'precondition: this must be the height-only fallback');

  const style = boardFitContainerStyle(fit);
  assert.equal(style.transformOrigin, 'top center');
  assert.equal(style.width, undefined, 'must not pin a width when the fit did not solve for one');
  assert.equal(style.transform, `scale(${fit.scale})`);
});

test('boardFitContainerStyle uses top-left for a width-filled board, matching the math in computeBoardFit (#794)', () => {
  const k = 0.4167;
  const w0 = 1920, h0 = 1354;
  const fixed = h0 - k * w0;
  const w1 = w0 * 1.5;
  const h1 = fixed + k * w1;
  const fit = computeBoardFit({ width: w0, height: h0 }, { width: w1, height: h1 }, 1920, 1080);
  assert.notEqual(fit.width, null, 'precondition: this must be the width-filled path');

  const style = boardFitContainerStyle(fit);
  assert.equal(style.transformOrigin, 'top left');
  assert.equal(style.width, `${fit.width}px`);
});

test('boardFitContainerStyle fills the viewport width for an already-fitting board', () => {
  /*
   * This used to assert style.width === undefined, on the reasoning that scale(1) needing no
   * width instruction at all. It was wrong: with nothing set, a CSS grid's columns size to their
   * own content rather than the viewport, and a page with few items shrank to a narrow column -
   * watched directly on a real screen, where a page with one item left most of a 1920px board
   * black. scale(1) on an explicit 100% is still the identity transform; the width is what fixes
   * the gap.
   */
  const style = boardFitContainerStyle({ scale: 1, width: null });
  assert.equal(style.transformOrigin, 'top center');
  assert.equal(style.transform, 'scale(1)');
  assert.equal(style.width, '100%');
});

test('boardFitContainerStyle leaves width unset for the #802 height-only fallback (scale < 1, width: null)', () => {
  // The DIFFERENT width:null case: solveFitWidth could not solve a fill width, so this shrinks
  // whatever the container's own natural width already is. Forcing a width here would fight that
  // fallback rather than help it.
  const style = boardFitContainerStyle({ scale: 0.85, width: null });
  assert.equal(style.transformOrigin, 'top center');
  assert.equal(style.transform, 'scale(0.85)');
  assert.equal(style.width, undefined);
});
