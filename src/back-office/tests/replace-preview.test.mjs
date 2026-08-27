import { strict as assert } from "node:assert";
import test from "node:test";
import { andMore, priceMovePhrase, replaceSummary } from "../src/menuImportCandidates.mjs";

/*
 * M6.13 - the replace confirm said "12 unpublished changes already present", which is the TARGET
 * menu's own draft: what the replacement discards. Nothing on the screen said what the replacement
 * itself does, and that is the decision the operator is making.
 */

const preview = (over = {}) => ({
  arrivingCount: 0, leavingCount: 0, repricedCount: 0, arriving: [], leaving: [], repriced: [], ...over
});

test("counts summarize, and each part appears only when it has something to say", () => {
  assert.equal(
    replaceSummary(preview({ arrivingCount: 4, leavingCount: 2, repricedCount: 6 })),
    "4 dishes arrive · 2 go · 6 change price"
  );
  assert.equal(replaceSummary(preview({ repricedCount: 3 })), "3 change price");
  assert.equal(replaceSummary(preview({ arrivingCount: 1, leavingCount: 1, repricedCount: 1 })),
    "1 dish arrives · 1 goes · 1 changes price");
});

test("a replacement that changes nothing says nothing, not three zeroes", () => {
  // Re-importing a menu nobody has edited is a real thing to do. "0 arrive · 0 go · 0 change
  // price" is a worse way of saying "nothing" than saying nothing, and the screen has its own
  // sentence for it.
  assert.equal(replaceSummary(preview()), null);
  assert.equal(replaceSummary(null), null);
});

test("both numbers travel, exactly as stored", () => {
  // Q115/Q190: "9.5" never becomes "9.50" and "MP" is a price.
  assert.equal(priceMovePhrase({ name: "Pad Thai", from: "12.95", to: "13.95" }), "12.95 → 13.95");
  assert.equal(priceMovePhrase({ name: "Market Fish", from: "MP", to: "24" }), "MP → 24");
});

test("a missing price reads as words rather than an empty gap", () => {
  // A dish arriving with no price is a fact worth seeing, and "12.95 → " reads like a bug.
  assert.equal(priceMovePhrase({ name: "Soup", from: "6.00", to: null }), "6.00 → no price");
  assert.equal(priceMovePhrase({ name: "Soup", from: null, to: "6.00" }), "no price → 6.00");
});

test("a capped list says how much it is not showing", () => {
  // Silent truncation reads as "that is all of them".
  assert.equal(andMore(["a", "b"], 6), "and 4 more");
  assert.equal(andMore(["a", "b"], 2), null);
  assert.equal(andMore([], 0), null);
});
