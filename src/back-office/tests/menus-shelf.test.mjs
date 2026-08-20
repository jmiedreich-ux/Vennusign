import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

import {
  availableShelfFilters,
  boardCounts,
  cardStatus,
  changePhrase,
  filterShelf,
  hasChangesWaiting,
  isShelfAtScale,
  menusInUse,
  menusNotInUse,
  shelfHeadline,
  shelfScaleThreshold,
  shelfSubLine
} from "../src/menusShelf.mjs";

function menu(overrides = {}) {
  return {
    menuId: overrides.menuId ?? "m1",
    name: "Summer Menu",
    theme: null,
    isPutAway: false,
    publishedVersion: 3,
    lastPublishedUtc: "2026-08-09T10:00:00Z",
    lastPublishedBy: "Alex",
    draftCount: 0,
    screenIds: ["s1"],
    board: null,
    ...overrides
  };
}

// ---- the shelf's shape ------------------------------------------------------

test("the shelf changes shape once, at seven menus (Q163)", () => {
  assert.equal(shelfScaleThreshold, 7);

  const six = Array.from({ length: 6 }, (_, index) => menu({ menuId: `m${index}` }));
  assert.equal(isShelfAtScale(six), false);
  assert.equal(isShelfAtScale([...six, menu({ menuId: "m7" })]), true);
});

test("a put-away menu still counts towards the cutover", () => {
  // They are still menus the person has, they still show in the strip, and
  // search still finds them - so they still make the shelf a big shelf.
  const menus = Array.from({ length: 7 }, (_, index) =>
    menu({ menuId: `m${index}`, isPutAway: index > 3 })
  );

  assert.equal(isShelfAtScale(menus), true);
});

test("menus in use come back most recently published first", () => {
  const menus = [
    menu({ menuId: "old", lastPublishedUtc: "2026-08-01T10:00:00Z" }),
    menu({ menuId: "new", lastPublishedUtc: "2026-08-09T10:00:00Z" }),
    menu({ menuId: "away", isPutAway: true })
  ];

  assert.deepEqual(menusInUse(menus).map(item => item.menuId), ["new", "old"]);
  assert.deepEqual(menusNotInUse(menus).map(item => item.menuId), ["away"]);
});

test("a never-published menu sorts by name rather than drifting", () => {
  const menus = [
    menu({ menuId: "b", name: "Bravo", publishedVersion: null, lastPublishedUtc: null }),
    menu({ menuId: "a", name: "Alpha", publishedVersion: null, lastPublishedUtc: null }),
    menu({ menuId: "published", name: "Zulu", lastPublishedUtc: "2026-08-09T10:00:00Z" })
  ];

  // Published first - it has a recency to sort by - then the rest, settled.
  assert.deepEqual(menusInUse(menus).map(item => item.menuId), ["published", "a", "b"]);
});

// ---- search and filters -----------------------------------------------------

test("search finds a put-away menu too", () => {
  // A shelved menu is still one of yours; a search that cannot find it is a
  // search that lies about what the venue has (Q163).
  const menus = [menu({ menuId: "away", name: "Winter Menu", isPutAway: true }), menu({ menuId: "m1" })];

  assert.deepEqual(filterShelf(menus, { search: "winter" }).map(item => item.menuId), ["away"]);
});

test("changes waiting means waiting to reach a screen, so a never-published menu is not one", () => {
  // Found by the browser spec: the filter counted three while the shelf drew two
  // pending bars, because a menu with no publish differs from nothing in every
  // way. Everything about it is waiting, which is a different fact, and the card
  // states it differently. One predicate now, shared by both.
  assert.equal(hasChangesWaiting(menu({ draftCount: 5, publishedVersion: null })), false);
  assert.equal(hasChangesWaiting(menu({ draftCount: 5 })), true);
  assert.equal(hasChangesWaiting(menu({ draftCount: 0 })), false);

  const menus = [
    menu({ menuId: "waiting", draftCount: 2 }),
    menu({ menuId: "never", draftCount: 5, publishedVersion: null })
  ];
  assert.deepEqual(
    filterShelf(menus, { filter: "pending" }).map(item => item.menuId),
    ["waiting"]
  );
});

// Raised by the independent review: a malformed payload threw out of render and
// took the whole shelf with it, rather than drawing one odd card.
test("a malformed payload draws an odd card rather than taking the shelf down", () => {
  const malformed = [
    { ...menu({ menuId: "a" }), screenIds: null },
    { ...menu({ menuId: "b" }), screenIds: undefined },
    { ...menu({ menuId: "c" }), screenIds: "s1" }
  ];

  assert.doesNotThrow(() => shelfHeadline(malformed));
  assert.doesNotThrow(() => shelfSubLine(malformed));
  assert.doesNotThrow(() => malformed.map(cardStatus));

  // And it does not invent screens it cannot see.
  assert.equal(shelfSubLine(malformed), "0 screens in use · 3 menus");
  assert.deepEqual(cardStatus(malformed[0]), { tone: "idle", text: "Not on a screen" });
});

test("a filter that would match nothing is not offered", () => {
  // An empty filter is a dead end that teaches nobody anything about their shelf.
  const nothingPending = [menu({ draftCount: 0, screenIds: [] })];

  assert.deepEqual(availableShelfFilters(nothingPending).map(chip => chip.key), []);
  assert.deepEqual(
    availableShelfFilters([menu({ draftCount: 2 })]).map(chip => chip.key),
    ["on-screens", "pending"]
  );
});

test("filters and search narrow together", () => {
  const menus = [
    menu({ menuId: "a", name: "Summer", draftCount: 2 }),
    menu({ menuId: "b", name: "Winter", draftCount: 0 }),
    menu({ menuId: "c", name: "Summer Late", draftCount: 1 })
  ];

  assert.deepEqual(
    filterShelf(menus, { search: "summer", filter: "pending" }).map(item => item.menuId),
    ["a", "c"]
  );
});

// ---- the headline -----------------------------------------------------------

// Decision 12: a sentence naming what is current and what is not. Never a green
// all-clear, never a status table.
test("the headline names each menu holding changes, capped at three (Q169)", () => {
  const menus = [
    menu({ menuId: "a", name: "Summer Menu", draftCount: 3 }),
    menu({ menuId: "b", name: "Patio Drinks", draftCount: 1 }),
    menu({ menuId: "c", name: "Brunch", draftCount: 2 }),
    menu({ menuId: "d", name: "Late Night", draftCount: 5 })
  ];

  // The recorded shape, verbatim from Q169: the first says it in full, the rest
  // are a name and a number. Most changes first, because that is the one to look
  // at, and the fourth is summarised so a big shelf still reads as one sentence.
  assert.equal(
    shelfHeadline(menus),
    "Late Night is holding 5 changes, Summer Menu 3, Brunch 2, and 1 more menu."
  );

  assert.equal(
    shelfHeadline(menus.slice(0, 2)),
    "Summer Menu is holding 3 changes, Patio Drinks 1."
  );
});

test("the headline uses natural singular and zero forms (Q181)", () => {
  assert.equal(changePhrase(1), "1 change");
  assert.equal(changePhrase(0), "0 changes");
  assert.equal(changePhrase(3), "3 changes");

  assert.match(shelfHeadline([menu({ draftCount: 1 })]), /is holding 1 change\./);
  assert.equal(shelfHeadline([menu({ screenIds: ["only"] })]), "Your screen is showing the latest.");
  assert.equal(shelfHeadline([menu({ screenIds: ["a", "b"] })]), "All 2 screens are showing the latest.");
});

test("a shelf with nothing on a screen says so plainly", () => {
  assert.equal(shelfHeadline([menu({ screenIds: [] })]), "Nothing is on your screens.");
  assert.equal(shelfHeadline([]), "Nothing on your screens yet.");
  assert.equal(shelfHeadline([menu({ isPutAway: true })]), "Nothing on your screens yet.");
});

test("the sub-line counts screens, menus and what is put away", () => {
  const menus = [
    menu({ menuId: "a", screenIds: ["s1", "s2"] }),
    menu({ menuId: "b", screenIds: ["s2"] }),
    menu({ menuId: "away", isPutAway: true })
  ];

  // s2 twice is one screen: the count is of screens, not of assignments.
  assert.equal(shelfSubLine(menus), "2 screens in use · 2 menus · 1 not in use");
  assert.equal(shelfSubLine([menu({ screenIds: ["s1"] })]), "1 screen in use · 1 menu");
});

// ---- a card's own status ----------------------------------------------------

test("never published is its own state, not a kind of not-on-a-screen", () => {
  // A menu nobody has published has no board to show. Saying so is the honest
  // version of a blank card.
  assert.deepEqual(cardStatus(menu({ publishedVersion: null, screenIds: [] })), {
    tone: "idle",
    text: "Never published"
  });

  assert.deepEqual(cardStatus(menu({ screenIds: [] })), { tone: "idle", text: "Not on a screen" });
  assert.deepEqual(cardStatus(menu({ isPutAway: true })), { tone: "idle", text: "Not in use" });
  assert.deepEqual(cardStatus(menu({ screenIds: ["s1"] })), { tone: "live", text: "On your screen" });
  assert.deepEqual(cardStatus(menu({ screenIds: ["s1", "s2"] })), { tone: "live", text: "On 2 screens" });
  assert.deepEqual(cardStatus(menu({ draftCount: 1 })), { tone: "pending", text: "On your screen" });
});

// ---- the counts beside a card ----------------------------------------------

test("a card counts what its board draws, not what the menu holds", () => {
  const board = {
    sections: [
      { sectionId: "drinks", items: [{ itemId: "a" }, { itemId: "b" }] },
      { sectionId: "snacks", items: [{ itemId: "c" }] },
      { sectionId: "empty", items: [] }
    ]
  };

  // The card shows what the screens show, so its count describes the same thing:
  // an empty section is not counted, and neither is an 86'd item.
  assert.deepEqual(boardCounts(board, []), { sections: 2, items: 3 });
  assert.deepEqual(boardCounts(board, ["c"]), { sections: 1, items: 2 });
  assert.deepEqual(boardCounts(board, ["a", "b", "c"]), { sections: 0, items: 0 });
  assert.deepEqual(boardCounts(null, []), { sections: 0, items: 0 });
});

// ---- the words the design fixed --------------------------------------------

test("the card menu is the six named actions, in order, verbatim", async () => {
  const source = await readFile(new URL("../src/MenusHome.tsx", import.meta.url), "utf8");
  const markup = source.replaceAll(/\/\*[\s\S]*?\*\//g, " ").replaceAll(/(^|[^:])\/\/.*$/gm, "$1");

  // Wording IS the design here (README, Verbatim copy), and Put away sits
  // directly after Duplicate with Take off the screens alone below the last
  // divider (Q195, build-decision 16).
  const order = ["Open", "Quick update", "Go back to…", "Duplicate", "Put away", "Take off the screens"];

  // Read the labels out of the menu in the order they are written, and compare
  // the whole list. Matching each one in turn would pass on a menu that also
  // contained three extra items nobody approved.
  const menu = markup.slice(markup.indexOf('data-testid="card-menu"'), markup.indexOf("</details>"));
  const labels = [...menu.matchAll(/>\s*([A-Z][^<>{}]*?)\s*<\/button>/g)].map(match => match[1]);

  assert.deepEqual(labels, order);
});

test("the banned words appear nowhere on the shelf (criterion 5)", async () => {
  const source = await readFile(new URL("../src/MenusHome.tsx", import.meta.url), "utf8");
  const markup = source.replaceAll(/\/\*[\s\S]*?\*\//g, " ").replaceAll(/(^|[^:])\/\/.*$/gm, "$1");

  // "Nobody ever sees the words unpublish, supersede, restore or archive"
  // (decisions 9, 10, 11). Scoped to Menus by Q179.
  for (const banned of ["unpublish", "supersede", "restore", "archive"]) {
    assert.doesNotMatch(markup, new RegExp(banned, "i"), `the shelf says "${banned}"`);
  }
});

test("take off the screens always shows what replaces it first (criterion 6)", async () => {
  const source = await readFile(new URL("../src/MenusHome.tsx", import.meta.url), "utf8");

  // It is never a bare action: the dialog states what people will see instead,
  // with a picture of the venue fallback, before anything is confirmed.
  assert.match(source, /What people will see instead/);
  assert.match(source, /data-testid="venue-fallback"/);
  assert.match(
    source,
    /It stays on your Menus home and keeps its history\. You can put it back at any time\./
  );
});

test("the empty shelf can actually open the name-a-menu dialog", async () => {
  // The regression: the dialog was rendered only in the populated-shelf return,
  // while the empty shelf returned early. Clicking the one button a brand-new
  // customer is offered set the state, re-rendered the same empty state, and
  // showed nothing - no dialog, no error, no console output.
  const source = await readFile(new URL("../src/MenusHome.tsx", import.meta.url), "utf8");

  const emptyBranch = source.slice(
    source.indexOf("menus.length === 0"),
    source.indexOf("menus-home__header"));
  assert.match(emptyBranch, /data-testid="add-a-menu"/,
    "the empty shelf still offers Add a menu");
  assert.match(emptyBranch, /\{nameMenuDialog\}/,
    "the empty shelf must render the dialog its only button opens");

  // And it is one shared dialog, not two copies that can drift apart.
  assert.equal((source.match(/const nameMenuDialog =/g) ?? []).length, 1);
  assert.equal((source.match(/\{nameMenuDialog\}/g) ?? []).length, 2);
  assert.equal((source.match(/data-testid="name-menu-dialog"/g) ?? []).length, 1);
});

