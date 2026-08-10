import assert from "node:assert/strict";
import { readdirSync, readFileSync } from "node:fs";
import test from "node:test";

import {
  availabilityLine,
  bannedWords,
  boardsPhrase,
  canDiscardDraft,
  canvasBoard,
  draftPhrase,
  findItem,
  findOnBoard,
  firstOpenState,
  isMissingPrice,
  itemsOf,
  publishBlockedReason,
  publishLabel,
  publishTargets,
  publishedLine,
  releasedPhrase,
  reorder,
  resumeState,
  sectionsOf,
  screenChipCutover,
  sharedItemLine,
  venueTime
} from "../src/builderModel.mjs";

const board = {
  menuId: "menu-1",
  name: "Summer Menu",
  theme: null,
  dwellSeconds: 8,
  loopWarningSeconds: 60,
  sections: [
    {
      sectionId: "s2",
      name: "Mains",
      sortOrder: 1,
      items: [
        { itemId: "i3", name: "Market Oysters", description: "Half dozen", price: "MP", sortOrder: 0 },
        { itemId: "i4", name: "Steak Frites", description: null, price: null, sortOrder: 1 }
      ]
    },
    {
      sectionId: "s1",
      name: "Starters",
      sortOrder: 0,
      items: [
        { itemId: "i2", name: "Olives", description: "Marinated, with chilli", price: "6", sortOrder: 1 },
        { itemId: "i1", name: "Harbor Lemonade", description: "Over crushed ice", price: "9.5", sortOrder: 0 }
      ]
    }
  ]
};

test("sections and items come back in board order, not payload order", () => {
  assert.deepEqual(
    sectionsOf(board).map(section => section.sectionId),
    ["s1", "s2"]
  );
  assert.deepEqual(
    itemsOf(board, "s1").map(item => item.itemId),
    ["i1", "i2"]
  );
});

test("a malformed board draws nothing rather than throwing out of render", () => {
  assert.deepEqual(sectionsOf(null), []);
  assert.deepEqual(sectionsOf({ sections: "nope" }), []);
  assert.deepEqual(itemsOf(board, "missing"), []);
});

test("first open is One-section view, top section, nothing selected (Q116)", () => {
  assert.deepEqual(firstOpenState(board), {
    view: "one-section",
    sectionId: "s1",
    selectedItemId: null
  });
});

test("a remembered section that no longer exists falls back to the top", () => {
  const resumed = resumeState(board, { view: "one-section", sectionId: "deleted", selectedItemId: "i1" });
  assert.equal(resumed.sectionId, "s1");
  // The remembered selection survives, because that item is still on the board.
  assert.equal(resumed.selectedItemId, "i1");
});

test("a remembered selection that no longer exists clears rather than pointing at nothing", () => {
  const resumed = resumeState(board, { view: "whole-board", sectionId: "s2", selectedItemId: "gone" });
  assert.equal(resumed.view, "whole-board");
  assert.equal(resumed.sectionId, "s2");
  assert.equal(resumed.selectedItemId, null);
});

test("One-section view draws exactly one section; Whole board draws the menu", () => {
  assert.deepEqual(
    canvasBoard(board, { view: "one-section", sectionId: "s2" }).sections.map(section => section.sectionId),
    ["s2"]
  );
  assert.equal(canvasBoard(board, { view: "whole-board", sectionId: "s2" }).sections.length, 2);
});

test("One-section view on a section that vanished draws an empty board, not the whole menu", () => {
  assert.deepEqual(canvasBoard(board, { view: "one-section", sectionId: null }).sections, []);
});

test("the draft phrase has natural singular and zero forms (Q181)", () => {
  assert.equal(draftPhrase(0), "Everything is on your screens");
  assert.equal(draftPhrase(1), "1 change not on your screens");
  assert.equal(draftPhrase(3), "3 changes not on your screens");
});

test("a menu nobody has published gets a different sentence, not a bigger number", () => {
  // Its "changes" are its whole contents measured against nothing, so a count
  // describes a comparison with no other side.
  assert.equal(draftPhrase(12, { neverPublished: true }), "Nothing on your screens yet");
  assert.equal(draftPhrase(0, { neverPublished: true }), "Nothing on your screens yet");
});

test("discard is offered only when there is something to go back to", () => {
  // Discard restores the published board. With no published board the act does
  // nothing at all, and an inert control is what decision 5 forbids.
  assert.equal(canDiscardDraft({ draftCount: 5, publishedVersion: null }), false);
  assert.equal(canDiscardDraft({ draftCount: 5, publishedVersion: 3 }), true);
  assert.equal(canDiscardDraft({ draftCount: 0, publishedVersion: 3 }), false);
});

test("the phrase carries no possessive (Q147)", () => {
  for (const count of [0, 1, 2, 12]) {
    assert.ok(!/\byour changes\b|'s changes/i.test(draftPhrase(count)), draftPhrase(count));
  }
});

test("the publish button says the same thing in both bar forms (Q161)", () => {
  assert.equal(publishLabel(1), "Publish 1 change");
  assert.equal(publishLabel(12), "Publish 12 changes");
  // X1's "Publish to 12 screens" was treated as a slip: the button counts changes.
  assert.ok(!/screen/i.test(publishLabel(12)));
});

test("times render in the venue's zone, not the viewer's (Q196)", () => {
  const utc = "2026-08-10T02:12:00Z";
  const denver = venueTime(utc, "America/Denver");
  const london = venueTime(utc, "Europe/London");
  assert.notEqual(denver, london);
  assert.ok(denver.includes("8:12"), denver);
  assert.ok(london.includes("3:12"), london);
});

test("an unknown venue timezone falls back rather than crashing the board", () => {
  assert.ok(venueTime("2026-08-10T02:12:00Z", "Mars/Olympus"));
  assert.equal(venueTime(null, "UTC"), null);
  assert.equal(venueTime("not a date", "UTC"), null);
});

test("a menu nobody has published says so rather than showing an empty slot", () => {
  assert.equal(publishedLine({ lastPublishedUtc: null, lastPublishedBy: null }, "UTC"), "Not published yet");
  assert.equal(
    publishedLine({ lastPublishedUtc: "2026-08-10T16:12:00Z", lastPublishedBy: "Dana" }, "UTC"),
    "Published Mon 4:12pm by Dana"
  );
});

test("where an item lives uses Q123's locked vocabulary", () => {
  const boards = menuId => ({ menuId, menuName: menuId });
  assert.equal(boardsPhrase([boards("menu-1")], "menu-1"), null, "the board you are on is never named");
  assert.equal(boardsPhrase([boards("Late Night")], "menu-1"), "Late Night");
  assert.equal(boardsPhrase([boards("Late Night"), boards("Brunch")], "menu-1"), "Late Night and Brunch");
  assert.equal(boardsPhrase([boards("a"), boards("b"), boards("c")], "menu-1"), "3 boards");
});

test("the shared-item line states the fact and asks nothing (Q5's follow-up)", () => {
  const line = sharedItemLine(
    [
      { menuId: "menu-1", menuName: "Summer Menu" },
      { menuId: "menu-2", menuName: "Late Night" }
    ],
    "menu-1"
  );
  assert.equal(line, "Also on Late Night — it will show this when you publish it.");

  // No item on any other board means no line at all: silence is correct here.
  assert.equal(sharedItemLine([{ menuId: "menu-1", menuName: "Summer Menu" }], "menu-1"), null);

  // It is a statement, not a question. A confirmation on every price edit is what
  // the design follow-up was told to avoid.
  assert.ok(!line.includes("?"));
});

test("the shared-item line agrees with itself about how many boards", () => {
  const line = sharedItemLine(
    [
      { menuId: "menu-2", menuName: "Late Night" },
      { menuId: "menu-3", menuName: "Brunch" }
    ],
    "menu-1"
  );
  assert.equal(line, "Also on Late Night and Brunch — they will show this when you publish them.");
});

test("a missing price is a flag, and an empty string counts as missing", () => {
  assert.equal(isMissingPrice({ price: null }), true);
  assert.equal(isMissingPrice({ price: "   " }), true);
  assert.equal(isMissingPrice({ price: "MP" }), false);
  assert.equal(isMissingPrice({ price: "0" }), false, "zero is a price somebody typed");
});

test("the availability line names the consequence, not the setting (Q104)", () => {
  const line = availabilityLine(
    { itemId: "i1", isAvailable: false, changedUtc: "2026-08-10T18:40:00Z", changedBy: "Alex" },
    "UTC"
  );
  assert.ok(line.includes("86'd"), line);
  assert.ok(line.includes("Hidden on every screen"), line);
  assert.ok(line.includes("immediately"), line);
  assert.equal(availabilityLine({ isAvailable: true }, "UTC"), null);
});

test("publishing waits for an unconfirmed save, and says why (Q197)", () => {
  assert.match(publishBlockedReason({ draftCount: 3, saveState: "failed" }), /hasn't saved/);
  assert.match(publishBlockedReason({ draftCount: 3, saveState: "saving" }), /Saving/);
  assert.equal(publishBlockedReason({ draftCount: 3, saveState: "clean" }), null);
});

test("a put-away menu says what to do rather than offering a dead button", () => {
  assert.match(publishBlockedReason({ draftCount: 1, saveState: "clean", isPutAway: true }), /back on the shelf/);
});

test("the publish bar draws a chip per screen up to six, a count above (Q161)", () => {
  const screens = count =>
    Array.from({ length: count }, (_, index) => ({ screenId: `s${index}`, screenName: `Screen ${index}`, state: "ready" }));

  assert.equal(publishTargets(screens(screenChipCutover)).mode, "chips");
  assert.equal(publishTargets(screens(screenChipCutover)).chips.length, screenChipCutover);
  assert.equal(publishTargets(screens(screenChipCutover + 1)).mode, "count");
  assert.equal(publishTargets(screens(screenChipCutover + 1)).chips.length, 0);
  assert.equal(publishTargets(screens(12)).countPhrase, "12 screens");
  assert.equal(publishTargets(screens(1)).countPhrase, "1 screen");
});

test("every exception is drawn, however many there are (Q167)", () => {
  const screens = [
    ...Array.from({ length: 8 }, (_, index) => ({ screenId: `ok${index}`, screenName: `OK ${index}`, state: "ready" })),
    ...Array.from({ length: 5 }, (_, index) => ({ screenId: `off${index}`, screenName: `Off ${index}`, state: "offline" }))
  ];
  const targets = publishTargets(screens);
  assert.equal(targets.mode, "count");
  assert.equal(targets.exceptions.length, 5, "exceptions are never summarised behind a count");
});

test("reorder moves one entry and leaves the rest alone", () => {
  assert.deepEqual(reorder(["a", "b", "c"], 2, 0), ["c", "a", "b"]);
  assert.deepEqual(reorder(["a", "b", "c"], 0, 2), ["b", "c", "a"]);
  // Out of range is a no-op rather than a crash or a silent truncation.
  assert.deepEqual(reorder(["a", "b"], 5, 0), ["a", "b"]);
  assert.deepEqual(reorder(null, 0, 1), []);
});

test("⌘K searches the board in front of you, name and description (Q121)", () => {
  assert.deepEqual(
    findOnBoard(board, "lemon").map(hit => hit.itemId),
    ["i1"]
  );
  // "the one with the chilli in it" is how people actually look for something.
  assert.deepEqual(
    findOnBoard(board, "chilli").map(hit => hit.itemId),
    ["i2"]
  );
  assert.deepEqual(findOnBoard(board, "   "), []);
  assert.equal(findOnBoard(board, "oyster")[0].sectionName, "Mains");
});

test("deleting a section says where its items went (Q96)", () => {
  assert.equal(releasedPhrase(0), "Section deleted.");
  assert.match(releasedPhrase(1), /Its item went back to your library/);
  assert.match(releasedPhrase(4), /Its 4 items went back to your library/);
});

test("no banned word appears in anything this module says (criterion 5)", () => {
  const said = [
    ...[0, 1, 3].map(draftPhrase),
    ...[1, 3].map(publishLabel),
    ...[0, 1, 4].map(releasedPhrase),
    publishedLine({ lastPublishedUtc: null, lastPublishedBy: null }, "UTC"),
    publishBlockedReason({ draftCount: 1, saveState: "failed" }),
    publishBlockedReason({ draftCount: 1, saveState: "saving" }),
    publishBlockedReason({ draftCount: 1, saveState: "clean", isPutAway: true }),
    availabilityLine({ isAvailable: false, changedUtc: "2026-08-10T18:40:00Z" }, "UTC"),
    sharedItemLine([{ menuId: "other", menuName: "Late Night" }], "menu-1")
  ].filter(Boolean);

  for (const sentence of said) {
    for (const word of bannedWords) {
      assert.ok(!sentence.toLowerCase().includes(word), `"${sentence}" contains "${word}"`);
    }
  }
});

/**
 * Windows resolves file names case-insensitively and CI does not, so a .tsx and a
 * .mjs differing only by case resolve to different files on the two. This has now
 * bitten twice — BoardFrame/boardFrame and MenusHome/menusHome — so the rule is a
 * test rather than a habit.
 */
test("no two source files differ only by case", () => {
  const seen = new Map();
  const walk = directory => {
    for (const entry of readdirSync(directory, { withFileTypes: true })) {
      if (entry.name === "node_modules") continue;
      const path = `${directory}/${entry.name}`;
      if (entry.isDirectory()) {
        walk(path);
        continue;
      }
      const stem = path.replace(/\.(mjs|d\.mts|tsx?|css)$/, "").toLowerCase();
      const existing = seen.get(stem);
      if (existing && existing !== path.replace(/\.(mjs|d\.mts|tsx?|css)$/, "")) {
        assert.fail(`"${existing}" and "${path}" differ only by case`);
      }
      seen.set(stem, path.replace(/\.(mjs|d\.mts|tsx?|css)$/, ""));
    }
  };
  walk("src");
});
