// Every layout sets `min-height: 100vh` on its own root and nothing else - a floor, not a
// ceiling, so a board taller than the viewport just grows past it with no scroll and no
// indicator (#790). This is the generic fix, above all layouts rather than inside any one of
// them: shrink the whole rendered board uniformly until it fits, rather than losing content off
// the bottom. Deliberately never scales UP - a short board stays at its natural size.
export const boardFitMinScale = 0.4;

export function computeFitScale(contentHeight, viewportHeight, minScale = boardFitMinScale) {
  if (!Number.isFinite(contentHeight) || !Number.isFinite(viewportHeight) || contentHeight <= 0 || viewportHeight <= 0) {
    return 1;
  }

  const scale = viewportHeight / contentHeight;
  return Math.min(1, Math.max(minScale, scale));
}
