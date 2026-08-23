import assert from 'node:assert/strict';
import test from 'node:test';
import { computeFitScale, boardFitMinScale } from '../src/boardFitScale.mjs';

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
