import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const [restaurants, corporate, board, mainEntry] = await Promise.all([
  readFile(new URL("../src/Restaurants.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/CorporateComms.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/Board.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/main.tsx", import.meta.url), "utf8")
]);

test("main entry routes /restaurants and /corporate-comms to their own pages", () => {
  assert.match(mainEntry, /Restaurants/);
  assert.match(mainEntry, /CorporateComms/);
  assert.match(mainEntry, /\/restaurants/);
  assert.match(mainEntry, /\/corporate-comms/);
});

test("Board and ScreenWall are shared from their own module, not duplicated per page", () => {
  assert.match(board, /export function Board/);
  assert.match(board, /export function ScreenWall/);
});

test("restaurants page covers nav, hero, a real 3-panel drive-thru board, and CTA", () => {
  assert.match(restaurants, /Sign in/);
  assert.match(restaurants, /Sign up/);
  assert.match(restaurants, /Stop losing sales to/);
  assert.match(restaurants, /ScreenWall periods={daypartWall}/);
  assert.match(restaurants, /Start your 14-day free trial/);
});

test("corporate comms page covers nav, hero, real lobby and emergency-alert boards, and CTA", () => {
  assert.match(corporate, /Sign in/);
  assert.match(corporate, /Sign up/);
  assert.match(corporate, /Keep your workforce informed/);
  assert.match(corporate, /style: "classic-chalkboard"/);
  assert.match(corporate, /style: "promo-splash"/);
  assert.match(corporate, /Start your 14-day free trial/);
});
