import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const [home, boardExamples, styles] = await Promise.all([
  readFile(new URL("../src/Home.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/boardExamples.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/styles.css", import.meta.url), "utf8")
]);

test("home page covers nav, hero, and an auto-playing showcase", () => {
  assert.match(home, /Sign in/);
  assert.match(home, /Sign up/);
  assert.match(home, /Vennusign runs your screens\. <span>And proves it\.<\/span>/);
  assert.match(home, /No credit card required to try it/);
  assert.match(home, /AUTO_ADVANCE_MS/);
  assert.match(home, /setAutoPlaying/);
  assert.match(home, /prefers-reduced-motion: reduce/);
});

test("board examples cover restaurant and bar with distinct periods and real layout styles", () => {
  assert.match(boardExamples, /id: "restaurant"/);
  assert.match(boardExamples, /id: "bar"/);
  const restaurantPeriods = ["breakfast", "lunch", "happy-hour", "dinner", "late-night"];
  const barPeriods = ["draft-list", "bar-happy-hour", "game-day", "cocktail-hour"];
  for (const id of [...restaurantPeriods, ...barPeriods]) {
    assert.match(boardExamples, new RegExp(`id: "${id}"`), `missing period ${id}`);
  }
  const realLayouts = ["classic-diner", "photo-grid", "tap-strips", "classic-chalkboard", "neon-chalkboard", "digital-tap-board", "daily-special-hero"];
  for (const style of realLayouts) {
    assert.match(boardExamples, new RegExp(`style: "${style}"`), `missing real layout style ${style}`);
  }
});

test("item tags cover new, chef-pick, limited, sold-out, and popular", () => {
  assert.match(boardExamples, /tag: "new"/);
  assert.match(boardExamples, /tag: "chef-pick"/);
  assert.match(boardExamples, /tag: "limited"/);
  assert.match(boardExamples, /tag: "sold-out"/);
  assert.match(boardExamples, /tag: "popular"/);
});

test("every board style referenced in content has a stylesheet rule, and reduced motion is respected", () => {
  const styleMatches = [...boardExamples.matchAll(/style: "([a-z-]+)"/g)].map(m => m[1]);
  for (const style of new Set(styleMatches)) {
    assert.match(styles, new RegExp(`\\.board--${style} `), `missing CSS for board style ${style}`);
  }
  assert.match(styles, /@media \(prefers-reduced-motion: reduce\)/);
});

test("real display fonts are used, matching the fonts the board layouts actually use", () => {
  for (const font of ["Caveat", "Kalam", "Patrick Hand", "Pacifico", "Righteous"]) {
    assert.ok(styles.includes(font), `expected ${font} to be used in styles.css`);
  }
});
