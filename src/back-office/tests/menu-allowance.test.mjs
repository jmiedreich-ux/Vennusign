import { strict as assert } from "node:assert";
import test from "node:test";
import { isAtMenuLimit, menuAllowanceNotice } from "../src/menusShelf.mjs";

/*
 * #908 - a venue at its menu limit found out four screens in: choose a new menu, choose paste,
 * paste the whole thing, answer the review, refused at the confirm step. The limit is knowable
 * when the shelf loads.
 */

test("a venue with room to spare is told nothing", () => {
  // Decision 12: summarize the normal, name the exception. A count nobody is near is noise.
  assert.equal(menuAllowanceNotice({ used: 3, limit: 50 }), null);
  assert.equal(isAtMenuLimit({ used: 3, limit: 50 }), false);
});

test("the last menu is named, and so is the wall", () => {
  assert.match(menuAllowanceNotice({ used: 49, limit: 50 }).text, /One menu left/);
  assert.equal(menuAllowanceNotice({ used: 49, limit: 50 }).tone, "nearly");

  const full = menuAllowanceNotice({ used: 50, limit: 50 });
  assert.equal(full.tone, "full");
  assert.match(full.text, /all 50/);
  // Names the way out, not only the wall.
  assert.match(full.text, /Put one away|raise the limit/);
  assert.equal(isAtMenuLimit({ used: 50, limit: 50 }), true);
});

test("over the limit is still over the limit", () => {
  // A ceiling can be lowered under a venue that is already past it.
  assert.equal(menuAllowanceNotice({ used: 62, limit: 50 }).tone, "full");
  assert.equal(isAtMenuLimit({ used: 62, limit: 50 }), true);
});

test("no ceiling configured is not a ceiling of zero", () => {
  // The bug this guards: `limit ?? 0` would draw "you are using all 0 of your menus" and disable
  // the only button on the page for every venue whose plan sets no ceiling.
  assert.equal(menuAllowanceNotice({ used: 4, limit: null }), null);
  assert.equal(isAtMenuLimit({ used: 4, limit: null }), false);
  assert.equal(menuAllowanceNotice(null), null);
  assert.equal(isAtMenuLimit(null), false);
});
