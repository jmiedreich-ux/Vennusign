import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const [home, boardExamples, styles] = await Promise.all([
  readFile(new URL("../src/Home.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/boardExamples.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/styles.css", import.meta.url), "utf8")
]);

test("home page covers nav, hero, and both venue examples", () => {
  assert.match(home, /Sign in/);
  assert.match(home, /Sign up/);
  assert.match(home, /Put your first screen live/);
  assert.match(home, /No credit card required to try it/);
  assert.match(home, /venueExamples\.map/);
});

test("board examples cover restaurant and bar with distinct periods and styles", () => {
  assert.match(boardExamples, /id: "restaurant"/);
  assert.match(boardExamples, /id: "bar"/);
  const restaurantPeriods = ["breakfast", "lunch", "happy-hour", "dinner", "late-night"];
  const barPeriods = ["draft-list", "bar-happy-hour", "game-day", "cocktail-hour"];
  for (const id of [...restaurantPeriods, ...barPeriods]) {
    assert.match(boardExamples, new RegExp(`id: "${id}"`), `missing period ${id}`);
  }
  const styleMatches = [...boardExamples.matchAll(/style: "([a-z-]+)"/g)].map(m => m[1]);
  assert.equal(new Set(styleMatches).size, styleMatches.length, "board styles should each be distinct");
  assert.ok(styleMatches.length >= 9, "expected at least 9 distinct board styles");
});

test("item tags cover new, chef-pick, limited, and sold-out", () => {
  assert.match(boardExamples, /tag: "new"/);
  assert.match(boardExamples, /tag: "chef-pick"/);
  assert.match(boardExamples, /tag: "limited"/);
  assert.match(boardExamples, /tag: "sold-out"/);
});

test("every board style referenced in content has a stylesheet rule, and reduced motion is respected", () => {
  const styleMatches = [...boardExamples.matchAll(/style: "([a-z-]+)"/g)].map(m => m[1]);
  for (const style of new Set(styleMatches)) {
    assert.match(styles, new RegExp(`\\.signup-demo__screen--${style} `), `missing CSS for board style ${style}`);
  }
  assert.match(styles, /@media \(prefers-reduced-motion: reduce\)/);
});
