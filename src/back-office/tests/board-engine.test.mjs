import assert from "node:assert/strict";
import { readdir, readFile } from "node:fs/promises";
import test from "node:test";

import { buildBoardDocument, isBoardEmpty, missingPrice } from "../../board-engine/boardDocument.mjs";
import {
  boardThemeSchemaVersion,
  boardThemeStyle,
  canRenderTheme,
  plainBoard,
  resolveBoardTheme
} from "../../board-engine/boardTheme.mjs";
import {
  boardAspectRatio,
  boardLogicalHeight,
  boardLogicalWidth,
  scaleToFit
} from "../../board-engine/boardScale.mjs";

const engineRoot = new URL("../../board-engine/", import.meta.url);

/** A board with two sections, so emptying one still leaves something to draw. */
function board(overrides = {}) {
  return {
    menuId: "menu-1",
    theme: null,
    sections: [
      {
        sectionId: "drinks",
        name: "Drinks",
        sortOrder: 0,
        items: [
          { itemId: "fizz", name: "Berry Fizz", description: "House soda.", price: "9.5", sortOrder: 0 },
          { itemId: "cider", name: "Harbour Cider", description: null, price: "MP", sortOrder: 1 }
        ]
      },
      {
        sectionId: "snacks",
        name: "Snacks",
        sortOrder: 1,
        items: [{ itemId: "olives", name: "Olives", description: null, price: "6", sortOrder: 0 }]
      }
    ],
    ...overrides
  };
}

// ---- what renders ----------------------------------------------------------

test("an 86'd item is not on the board", () => {
  const document = buildBoardDocument(board(), ["fizz"]);

  const items = document.sections.flatMap((section) => section.items.map((item) => item.itemId));
  assert.deepEqual(items, ["cider", "olives"]);
});

test("an editing surface keeps the 86'd item, marked", () => {
  // You cannot turn back on what the surface has hidden from you (Q104).
  const document = buildBoardDocument(board(), ["fizz"], { keepUnavailable: true });

  const items = document.sections.flatMap((section) => section.items);
  assert.deepEqual(items.map((item) => item.itemId), ["fizz", "cider", "olives"]);
  assert.deepEqual(
    items.filter((item) => item.isUnavailable).map((item) => item.itemId),
    ["fizz"]
  );
});

test("every item says whether it is off, on both kinds of document", () => {
  // A guest document says false for everything, because everything it drew is on.
  // A caller reading the flag must never have to know which document it holds.
  for (const document of [
    buildBoardDocument(board(), ["fizz"]),
    buildBoardDocument(board(), ["fizz"], { keepUnavailable: true })
  ]) {
    for (const item of document.sections.flatMap((section) => section.items)) {
      assert.equal(typeof item.isUnavailable, "boolean", item.itemId);
    }
  }
});

test("a section holding only 86'd items survives on an editing surface", () => {
  // It is empty tonight and gone from every TV, but it is where the items you
  // would turn back on live, so an editor still has to be able to reach it.
  const document = buildBoardDocument(board(), ["olives"], { keepUnavailable: true });

  assert.deepEqual(document.sections.map((section) => section.sectionId), ["drinks", "snacks"]);
});

test("a section emptied by an 86 is not on the board either", () => {
  // The whole point of judging emptiness after the removals: this section is not
  // empty, it just has nothing to show tonight.
  const document = buildBoardDocument(board(), ["olives"]);

  assert.deepEqual(document.sections.map((section) => section.sectionId), ["drinks"]);
});

test("a section that was always empty is not on the board", () => {
  const document = buildBoardDocument(
    board({
      sections: [
        { sectionId: "empty", name: "Coming soon", sortOrder: 0, items: [] },
        { sectionId: "drinks", name: "Drinks", sortOrder: 1, items: [{ itemId: "fizz", name: "Berry Fizz", price: "4" }] }
      ]
    }),
    []
  );

  assert.deepEqual(document.sections.map((section) => section.sectionId), ["drinks"]);
});

test("86'ing everything leaves a board with nothing to draw, not a broken one", () => {
  const document = buildBoardDocument(board(), ["fizz", "cider", "olives"]);

  assert.equal(document.sections.length, 0);
  assert.equal(isBoardEmpty(document), true);
});

// Q115/Q190: stored exactly as typed, and rendered exactly as stored.
test("a price is never reformatted", () => {
  const typed = ["12", "9.5", "9.50", "MP", "2 for 15"];
  const document = buildBoardDocument(
    board({
      sections: [
        {
          sectionId: "prices",
          name: "Prices",
          sortOrder: 0,
          items: typed.map((price, index) => ({ itemId: `i${index}`, name: `Item ${index}`, price, sortOrder: index }))
        }
      ]
    }),
    []
  );

  assert.deepEqual(document.sections[0].items.map((item) => item.price), typed);
});

test("an item with no price renders an em dash, never a zero and never a gap", () => {
  const document = buildBoardDocument(
    board({
      sections: [
        {
          sectionId: "prices",
          name: "Prices",
          sortOrder: 0,
          items: [
            { itemId: "none", name: "Market fish", price: null, sortOrder: 0 },
            { itemId: "blank", name: "Soup", price: "", sortOrder: 1 }
          ]
        }
      ]
    }),
    []
  );

  assert.deepEqual(document.sections[0].items.map((item) => item.price), [missingPrice, missingPrice]);
  assert.equal(missingPrice, "—");
});

test("sections and items come out in board order, and the same order every time", () => {
  const shuffled = board({
    sections: [
      {
        sectionId: "snacks",
        name: "Snacks",
        sortOrder: 5,
        items: [
          { itemId: "b", name: "B", price: "2", sortOrder: 2 },
          { itemId: "a", name: "A", price: "1", sortOrder: 1 }
        ]
      },
      { sectionId: "drinks", name: "Drinks", sortOrder: 1, items: [{ itemId: "c", name: "C", price: "3", sortOrder: 0 }] }
    ]
  });

  const document = buildBoardDocument(shuffled, []);

  assert.deepEqual(document.sections.map((section) => section.sectionId), ["drinks", "snacks"]);
  assert.deepEqual(document.sections[1].items.map((item) => item.itemId), ["a", "b"]);
});

test("two rows sharing a sort order still come out in one settled order", () => {
  const tied = board({
    sections: [
      {
        sectionId: "ties",
        name: "Ties",
        sortOrder: 0,
        items: [
          { itemId: "zeta", name: "Zeta", price: "1", sortOrder: 0 },
          { itemId: "alpha", name: "Alpha", price: "1", sortOrder: 0 }
        ]
      }
    ]
  });

  // Unstable order would make a card and the TV differ for no reason a person
  // could see or explain.
  const first = buildBoardDocument(tied, []);
  const second = buildBoardDocument(tied, []);
  assert.deepEqual(first, second);
  assert.deepEqual(first.sections[0].items.map((item) => item.itemId), ["alpha", "zeta"]);
});

test("the same board, 86 set and theme always give the same document", () => {
  const off = ["fizz"];
  assert.deepEqual(buildBoardDocument(board(), off), buildBoardDocument(board(), off));
});

test("no board at all is an empty document, not a crash", () => {
  for (const nothing of [null, undefined, {}, { sections: null }]) {
    const document = buildBoardDocument(nothing, []);
    assert.equal(isBoardEmpty(document), true);
  }
});

// ---- how it looks ----------------------------------------------------------

// Q86: a menu with no theme attached is a valid state. It renders plainly - it
// will look bad, and that is acceptable - but never blank and never invented.
test("no theme attached resolves to every value the board needs", () => {
  const resolved = resolveBoardTheme(null);

  assert.deepEqual(resolved, { ...plainBoard });
  for (const [token, value] of Object.entries(resolved)) {
    assert.ok(typeof value === "string" && value.length > 0, `${token} must have a plain default`);
  }
});

test("undefined and a nonsense theme resolve the same way as none at all", () => {
  for (const nothing of [undefined, null, "coastal", 7, []]) {
    assert.deepEqual(resolveBoardTheme(nothing), { ...plainBoard });
  }
});

test("a theme supplies some values and the plain board supplies the rest", () => {
  const resolved = resolveBoardTheme({
    board: { background: "#0b1219" },
    item: { nameColor: "#e2e8f0" }
  });

  assert.equal(resolved["board-background"], "#0b1219");
  assert.equal(resolved["item-name-color"], "#e2e8f0");
  // Untouched by the theme, so still the plain value - not undefined.
  assert.equal(resolved["item-description-color"], plainBoard["item-description-color"]);
  assert.equal(Object.keys(resolved).length, Object.keys(plainBoard).length);
});

test("a theme can turn the leaders off, which is a choice rather than an absence", () => {
  assert.equal(resolveBoardTheme({ leaders: { style: "none" } })["leader-style"], "none");
  // Anything unrecognised falls back rather than reaching the stylesheet.
  assert.equal(resolveBoardTheme({ leaders: { style: "squiggle" } })["leader-style"], plainBoard["leader-style"]);
});

test("the plain default is plain, not a named look nobody built", () => {
  // The design README's board palette is a THEME's values, and no named theme
  // exists to borrow from; borrowing one would be the invented fallback Q86 forbids.
  assert.equal(plainBoard["board-background"], "#ffffff");
  assert.notEqual(plainBoard["board-background"], "#faf8f2");
});

test("a theme written against a later engine is declined rather than half-drawn", () => {
  assert.equal(canRenderTheme({ schemaVersion: boardThemeSchemaVersion }), true);
  assert.equal(canRenderTheme({ schemaVersion: boardThemeSchemaVersion + 1 }), false);
  assert.equal(canRenderTheme(null), true);
  assert.equal(canRenderTheme({}), true);
});

test("every token reaches the stylesheet as one prefixed custom property", () => {
  const style = boardThemeStyle(null);

  for (const token of Object.keys(plainBoard)) {
    assert.equal(style[`--board-${token}`], plainBoard[token]);
  }
});

// ---- fitting ---------------------------------------------------------------

test("the board is laid out at one screen-shaped logical size", () => {
  assert.equal(boardLogicalWidth, 1920);
  assert.equal(boardLogicalHeight, 1080);
  assert.equal(boardAspectRatio, 16 / 9);
});

test("a board fills the width it is given, cropping the bottom rather than shrinking", () => {
  assert.equal(scaleToFit(1920, 1080), 1);
  assert.equal(scaleToFit(960, 540), 0.5);
  // A short, wide box fills the width and crops the bottom rather than shrinking
  // into a letterbox - the pending card reserves 30px for its amber strip.
  assert.equal(scaleToFit(1920, 540), 1);
  assert.equal(scaleToFit(960, 1080), 0.5);
});

test("a box with no measured size yet scales to nothing rather than to full size", () => {
  // Otherwise the first paint flashes a 1920px board through a 300px card.
  for (const nothing of [0, -1, Number.NaN, undefined, null]) {
    assert.equal(scaleToFit(nothing, 1080), 0);
    assert.equal(scaleToFit(1920, nothing), 0);
  }
});

// ---- what the engine must never do -----------------------------------------

/**
 * The code with its comments removed.
 *
 * These checks are about what the engine EMITS, and a comment explaining a rule
 * would otherwise trip the rule it explains — which is a test that proves the
 * presence of prose rather than the absence of markup. What the browser actually
 * renders is asserted against a real page in the Playwright suite; this is the
 * cheap guard that catches the mistake at the point of writing it.
 */
async function codeWithoutComments(fileName) {
  const text = await readFile(new URL(fileName, engineRoot), "utf8");
  return text
    .replaceAll(/\/\*[\s\S]*?\*\//g, " ")
    .replaceAll(/(^|[^:])\/\/.*$/gm, "$1");
}

test("the engine draws no venue-name strip", async () => {
  // Q98: if a TV carries a venue-name strip, the theme editor owns it. The Menus
  // engine neither draws one nor leaves room for one.
  const code = await codeWithoutComments("BoardRenderer.tsx");

  assert.doesNotMatch(code, /venueName|venue-name|boardTitle|board-title/i);
});

test("the Northside showcase is canvas DOM and CSS, not a flattened image", async () => {
  const renderer = await codeWithoutComments("BoardRenderer.tsx");
  const css = await readFile(new URL("board-engine.css", engineRoot), "utf8");
  const showcaseCss = css.slice(css.indexOf("/* Northside Social showcase"));

  assert.match(renderer, /board\?\.theme/);
  assert.match(renderer, /data-board-showcase/);
  assert.match(renderer, /board-showcase-header/);
  assert.match(renderer, /document\.sections\.map/);
  assert.match(showcaseCss, /linear-gradient|radial-gradient/);
  assert.doesNotMatch(showcaseCss, /url\s*\(/i);
});

test("the engine imports nothing from either application", async () => {
  // What this proves and what it does not: it proves the engine has no dependency
  // that would STOP the display player consuming it in milestone 4. It does not
  // prove the player consumes it - the player still renders the legacy model, so
  // card-and-TV parity is not a property this milestone can assert at all.
  for (const source of ["BoardRenderer.tsx", "BoardFrame.tsx", "boardDocument.mjs", "boardTheme.mjs", "boardScale.mjs"]) {
    const code = await codeWithoutComments(source);
    assert.doesNotMatch(
      code,
      /from\s+["'][^"']*(back-office|display|platform-operations)/,
      `${source} reaches into an application`
    );
  }
});

test("a guest surface carries no annotation markup", async () => {
  // Q135: the TV renders zero annotations, and milestone 2's cards are guest
  // surfaces too, so the default has to be the safe one. The engine hard-codes no
  // annotation copy at all — the one note it can draw arrives as a prop, from the
  // surface that decided to draw it.
  const code = await codeWithoutComments("BoardRenderer.tsx");

  assert.doesNotMatch(code, /86'd|PAGE \d|OF \d|annotation/i);
  assert.match(code, /surface = "guest"/);
});

test("a guest surface cannot be talked into keeping an 86'd item", async () => {
  // The guard is in the renderer rather than in its callers on purpose: a guest
  // board that honoured keepUnavailable would put a struck-through item on a real
  // TV, which is precisely what the availability model exists to prevent. Nothing
  // outside the engine should be able to cause that by passing one wrong prop.
  const code = await codeWithoutComments("BoardRenderer.tsx");

  assert.match(code, /surface === "preview" && keepUnavailable/);
});

test("no two source files differ only by case", async () => {
  // Found the hard way, twice: BoardFrame.tsx beside boardFrame.mjs, and then
  // MenusHome.tsx beside menusHome.mjs. On a case-insensitive filesystem the
  // bundler resolves "./BoardFrame" to the .mjs and the build fails. Worse than
  // the failure is that it resolves DIFFERENTLY on a case-sensitive filesystem,
  // so the same commit builds on one machine and not another.
  //
  // Both places, because the second one taught us the first fix was too narrow.
  for (const directory of [engineRoot, new URL("../src/", import.meta.url)]) {
    const seen = new Map();

    for (const name of await readdir(directory)) {
      const key = name.toLowerCase();
      const clash = seen.get(key);
      assert.equal(clash, undefined, `'${name}' and '${clash}' differ only by case`);
      seen.set(key, name);
    }
  }
});

test("the stylesheet consumes theme variables rather than baking values in", async () => {
  const css = await readFile(new URL("board-engine.css", engineRoot), "utf8");

  for (const token of ["board-background", "section-heading-color", "item-name-color", "leader-color"]) {
    assert.match(css, new RegExp(`var\\(--board-${token}`), `${token} must come from the theme`);
  }

  // The stage scales from the top-left, which is what makes a crop top-aligned
  // (Q191) rather than cutting the heading off.
  assert.match(css, /transform-origin:\s*top left/);
});
