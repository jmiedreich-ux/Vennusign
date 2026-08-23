import assert from 'node:assert/strict';
import test from 'node:test';
import {
  readDeviceGeometry,
  describeBoardFit,
  describeThemeCoverage,
  layoutThemeFieldCoverage
} from '../src/displayGeometry.mjs';

test('reads viewport, screen size, DPR and orientation from a window-like object', () => {
  const geometry = readDeviceGeometry({
    innerWidth: 1920,
    innerHeight: 1080,
    devicePixelRatio: 2,
    screen: { width: 3840, height: 2160, orientation: { type: 'landscape-primary' } }
  });

  assert.deepEqual(geometry, {
    viewport: { width: 1920, height: 1080 },
    screen: { width: 3840, height: 2160 },
    devicePixelRatio: 2,
    orientation: 'landscape-primary'
  });
});

test('returns null with no window available, rather than throwing', () => {
  assert.equal(readDeviceGeometry(undefined), null);
});

test('defaults devicePixelRatio to 1 and orientation to null when the platform does not report them', () => {
  const geometry = readDeviceGeometry({ innerWidth: 1920, innerHeight: 1080, screen: { width: 1920, height: 1080 } });
  assert.equal(geometry.devicePixelRatio, 1);
  assert.equal(geometry.orientation, null);
});

test('board fit reports the exact overflow measured on the live QA screen', () => {
  const fit = describeBoardFit(1354, 1080);
  assert.equal(fit.measured, true);
  assert.equal(fit.overflowPixels, 274);
  assert.equal(fit.fits, false);
});

test('board fit reports no overflow when the board is shorter than its viewport', () => {
  const fit = describeBoardFit(900, 1080);
  assert.equal(fit.overflowPixels, 0);
  assert.equal(fit.fits, true);
});

test('board fit is unmeasured rather than wrong when a dimension is missing', () => {
  assert.deepEqual(describeBoardFit(undefined, 1080), { measured: false });
});

const fullTheme = {
  backgroundColor: '#111315', accentColor: '#FFB74D', fontFamily: 'Inter', presetKey: 'bar_classic',
  titleColor: '#F8F5E9', glowColor: '#00E5FF', boardBackgroundColor: '#071013',
  sectionColors: ['#00E5FF'], glowIntensity: 1, titleFont: 'Righteous', itemFont: 'Caveat'
};

test('photo_grid consumes 3 of 10 renderable theme fields, matching what its CSS reads', () => {
  const coverage = describeThemeCoverage('photo_grid', fullTheme);
  assert.equal(coverage.themeFieldsServed, 10);
  assert.deepEqual(coverage.consumedFields.sort(), ['accentColor', 'backgroundColor', 'fontFamily']);
  assert.ok(coverage.ignoredFields.includes('titleFont'));
});

test('classic_chalkboard and tap_strips consume no theme fields at all', () => {
  assert.equal(describeThemeCoverage('classic_chalkboard', fullTheme).themeFieldsConsumed, 0);
  assert.equal(describeThemeCoverage('tap_strips', fullTheme).themeFieldsConsumed, 0);
});

test('a layout key with no entry in the coverage table is reported as unknown, not zero', () => {
  const coverage = describeThemeCoverage('some_future_layout', fullTheme);
  assert.equal(coverage.known, false);
});

test('every registered layout key resolves to an array, so a missing table entry is a build-time typo, not a runtime surprise', () => {
  for (const fields of Object.values(layoutThemeFieldCoverage)) {
    assert.ok(Array.isArray(fields));
  }
});
