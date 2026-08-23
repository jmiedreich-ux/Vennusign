// What the device can see about itself, with no server round trip. `Screens.WidthPixels` and
// `HeightPixels` are configured at pairing time and never measured again - this is what lets a
// diagnostics view show the two side by side instead of trusting the configured value.
export function readDeviceGeometry(win = globalThis.window) {
  if (!win) return null;

  const screenSize = win.screen
    ? { width: win.screen.width, height: win.screen.height }
    : null;

  return {
    viewport: { width: win.innerWidth, height: win.innerHeight },
    screen: screenSize,
    devicePixelRatio: win.devicePixelRatio ?? 1,
    orientation: win.screen?.orientation?.type ?? null
  };
}

// Whether the rendered board fits the viewport it is actually shown in. `containerHeight` is the
// board's own scrollHeight (or bounding-box height); a board taller than its viewport with
// nothing to scroll it is content nobody standing at the wall can see or be told is missing.
export function describeBoardFit(containerHeight, viewportHeight) {
  if (!Number.isFinite(containerHeight) || !Number.isFinite(viewportHeight)) {
    return { measured: false };
  }

  const overflowPixels = Math.max(0, Math.round(containerHeight - viewportHeight));

  return {
    measured: true,
    containerHeight: Math.round(containerHeight),
    viewportHeight: Math.round(viewportHeight),
    overflowPixels,
    fits: overflowPixels === 0
  };
}

// Every DisplayFrame theme field becomes a `--vennu-*` CSS variable, but each layout's own
// stylesheet only reads some of them (checked against layouts/*.css directly, not inferred).
// `--vennu-foreground`/`--vennu-accent-foreground`/`--vennu-*-glow` are computed from another
// field rather than serving one of their own, so they are not listed as separate fields here.
export const layoutThemeFieldCoverage = Object.freeze({
  classic_diner: ['accentColor', 'backgroundColor', 'fontFamily'],
  daily_special_hero: ['accentColor', 'backgroundColor', 'fontFamily'],
  photo_grid: ['accentColor', 'backgroundColor', 'fontFamily'],
  split_layout: ['accentColor', 'backgroundColor', 'fontFamily'],
  neon_chalkboard: ['boardBackgroundColor', 'glowColor', 'glowIntensity', 'itemFont', 'titleColor', 'titleFont'],
  classic_chalkboard: [],
  tap_strips: [],
  digital_tap_board: [],
  default: []
});

// A theme can be fully served and mostly ignored - this turns that gap into a fact rather than
// something only visible by diffing CSS by hand, which is how it was found (#790's investigation).
export function describeThemeCoverage(layoutKey, theme) {
  const themeFields = theme ? Object.keys(theme).filter((field) => field !== 'presetKey') : [];
  const consumed = layoutThemeFieldCoverage[layoutKey] ?? null;

  if (consumed === null) {
    return { layoutKey, known: false, themeFieldsServed: themeFields.length };
  }

  return {
    layoutKey,
    known: true,
    themeFieldsServed: themeFields.length,
    themeFieldsConsumed: consumed.length,
    consumedFields: consumed,
    ignoredFields: themeFields.filter((field) => !consumed.includes(field))
  };
}
