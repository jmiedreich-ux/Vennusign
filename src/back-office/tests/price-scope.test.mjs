import { strict as assert } from "node:assert";
import { readFile } from "node:fs/promises";
import test from "node:test";
import { priceScopeQuestion } from "../src/builderModel.mjs";

/*
 * A20 - owner ruling, 2026-08-27. A dish already on several menus and a new price could mean one
 * menu or all of them, and both silent answers were rejected: changing every menu is the behaviour
 * A19 withdrew, changing one quietly leaves the others wrong with nothing said.
 */

const here = "menu-1";

test("one menu is not ambiguous, so nothing is asked", () => {
  // Decision 18 - confirm only what we were unsure of. A dialog on every price edit is exactly
  // what Q5's follow-up was told to avoid, and that part of it still holds.
  assert.equal(priceScopeQuestion("Pad Thai", [{ menuId: here, menuName: "Lunch" }], here), null);
  assert.equal(priceScopeQuestion("Pad Thai", [], here), null);
  assert.equal(priceScopeQuestion("Pad Thai", null, here), null);
});

test("the question counts the menu being edited, even when the boards read does not name it", () => {
  // The boards list can come back without the current menu on it - a placement made in this
  // session, a stale read. Counting only the others would say "on 1 menus" and ask anyway.
  const question = priceScopeQuestion("Pad Thai", [{ menuId: "menu-2", menuName: "Dinner" }], here);
  assert.equal(question.total, 2);
  assert.match(question.title, /is on 2 menus/);
});

test("both answers name what will happen, not yes and no", () => {
  const question = priceScopeQuestion("Pad Thai", [
    { menuId: here, menuName: "Lunch" },
    { menuId: "menu-2", menuName: "Dinner" },
    { menuId: "menu-3", menuName: "Brunch" }
  ], here);

  assert.equal(question.total, 3);
  assert.match(question.hereLabel, /here only/i);
  assert.match(question.everywhereLabel, /all 3/);
  // Neither reads as agreement. "Yes" would not say which menus are about to change.
  for (const label of [question.hereLabel, question.everywhereLabel]) {
    assert.ok(!/^yes\b|^no\b/i.test(label));
  }
});

test("the detail line agrees with itself about how many other menus there are", () => {
  const two = priceScopeQuestion("Pad Thai", [{ menuId: here }, { menuId: "menu-2" }], here);
  assert.match(two.hereDetail, /The other menu keeps its price/);

  const three = priceScopeQuestion("Pad Thai", [{ menuId: here }, { menuId: "menu-2" }, { menuId: "menu-3" }], here);
  assert.match(three.hereDetail, /The other 2 menus keep their prices/);
});

test("a dish on one menu twice is still one menu", () => {
  // Two placements on two PAGES of the same menu (UQ_Placements_PageItem). Nothing is ambiguous
  // between menus, so nothing is asked - the section already addresses which placement it is.
  assert.equal(
    priceScopeQuestion("Pad Thai", [{ menuId: here, menuName: "Lunch" }, { menuId: here, menuName: "Lunch" }], here),
    null
  );
});

test("an item with no name still asks a sentence that reads", () => {
  assert.match(priceScopeQuestion("", [{ menuId: "menu-2" }], here).title, /^This item is on 2 menus\./);
});

/*
 * A source assertion, for the same reason menus-shelf.test.mjs uses them: the Playwright spec that
 * covers this properly cannot run while the UI gate is red, and this defect is invisible to every
 * other kind of test.
 *
 * Cancelling used to skip the write and leave the typed price in the field. The menu did not have
 * that price, the panel showed it anyway, and the next edit of ANY field saw a changed price and
 * asked again about a price nobody had kept. Found by driving the real screen.
 */
test("backing out of the price question puts the field back", async () => {
  const source = await readFile(new URL("../src/MenuBuilder.tsx", import.meta.url), "utf8");
  const cancelBranch = source.slice(source.indexOf("if (scope === null) {"), source.indexOf("await run(() => updateMenuItemValues(configuration, credential(), menuId, before.itemId"));

  assert.match(cancelBranch, /setDraftItem/, "cancelling restores the draft, not just the write");
  assert.match(cancelBranch, /before\.price/, "and restores it to the price the menu actually has");
});
