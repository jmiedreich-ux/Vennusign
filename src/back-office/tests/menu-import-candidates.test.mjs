import { strict as assert } from "node:assert";
import test from "node:test";
import { candidateProvenance, listPhrase, madePhrase } from "../src/menuImportCandidates.mjs";

/*
 * A21 - the owner's own library holds two "Pad Thai" at $12.95, and the review screen offered both
 * as the same button twice. This is the line that makes the choice answerable.
 */

const padThai = (over = {}) => ({
  itemId: "a", displayName: "Pad Thai", displayPrice: "12.95", matchRule: "exact_normalized",
  isSafe: false, onMenus: ["Lunch"], itemCreatedUtc: "2026-08-12T10:00:00Z", ...over
});

test("a lone candidate says nothing — there is nothing to tell it from", () => {
  // Decision 18: confirm only what we were unsure of. Provenance under every single-answer
  // question is noise on the screen the owner already called too dense.
  assert.equal(candidateProvenance(padThai(), 1), null);
});

test("two candidates are told apart by their menus and their age", () => {
  const line = candidateProvenance(padThai(), 2);
  assert.match(line, /On Lunch/);
  assert.match(line, /Added Aug 12|Added 12 Aug/);
});

test("on no menu is a fact, not a gap", () => {
  // This is the whole point: the dish on no menu is exactly the one you can now tell from the dish
  // on two. Rendering nothing there would leave the two buttons identical again.
  assert.match(candidateProvenance(padThai({ onMenus: [] }), 2), /On no menu/);
});

test("a candidate nobody looked up says nothing about menus", () => {
  // undefined is "not fetched"; [] is "on none". Collapsing them would print "On no menu" about a
  // dish that might be on five.
  const line = candidateProvenance(padThai({ onMenus: undefined }), 2);
  assert.doesNotMatch(line ?? "", /menu/i);
});

test("a long list does not become something nobody reads", () => {
  assert.equal(listPhrase(["Lunch"]), "Lunch");
  assert.equal(listPhrase(["Lunch", "Dinner"]), "Lunch and Dinner");
  assert.equal(listPhrase(["Lunch", "Dinner", "Brunch", "Late"]), "Lunch, Dinner and 2 more");
});

test("an unusable date is left out rather than printed as Invalid Date", () => {
  assert.equal(madePhrase("not a date"), null);
  assert.equal(madePhrase(null), null);
  const line = candidateProvenance(padThai({ itemCreatedUtc: null }), 2);
  assert.match(line, /On Lunch/);
  assert.doesNotMatch(line, /Added|Invalid/);
});

test("nothing at all in, null out", () => {
  assert.equal(candidateProvenance(null, 2), null);
  // Both facts missing means the line would be empty, and an empty line is not drawn.
  assert.equal(candidateProvenance(padThai({ onMenus: undefined, itemCreatedUtc: null }), 2), null);
});
