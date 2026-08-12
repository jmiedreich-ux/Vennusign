import test from "node:test";
import assert from "node:assert/strict";
import { calculateBoardCapacity } from "../src/boardCapacity.mjs";

const board = count => ({ sections: [{ items: Array.from({ length: count }, (_, index) => ({ name: `Item ${index + 1}` })) }] });

test("1920x1080 Midnight golden capacity is pinned and names overflow", () => {
  const result = calculateBoardCapacity(board(25), { width: 1920, height: 1080 }, "midnight");
  assert.equal(result.limit, 16);
  assert.equal(result.state, "overflow");
  assert.deepEqual(result.dropped, Array.from({ length: 9 }, (_, index) => `Item ${index + 17}`));
});

test("capacity moves with a second geometry", () => {
  const landscape = calculateBoardCapacity(board(20), { width: 1920, height: 1080 });
  const portrait4k = calculateBoardCapacity(board(20), { width: 2160, height: 3840 });
  assert.equal(landscape.limit, 16);
  assert.equal(portrait4k.limit, 108);
  assert.notEqual(portrait4k.limit, landscape.limit);
  assert.equal(portrait4k.state, "fits");
});

test("an attached display theme changes the computed fit limit", () => {
  const content = board(20);
  const plain = calculateBoardCapacity(content, { width: 1920, height: 1080 }, null);
  const themed = calculateBoardCapacity(content, { width: 1920, height: 1080 }, "evening");

  assert.notEqual(themed.limit, plain.limit);
  assert.ok(themed.limit < plain.limit);
});
