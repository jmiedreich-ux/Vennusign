import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const source = name => readFile(new URL(`../src/${name}`, import.meta.url), "utf8");
const [operations, meals, playlists, broadcasts, promotions, api] = await Promise.all([
  source("VenueOperations.tsx"), source("MealPeriodAdministration.tsx"),
  source("PlaylistAdministration.tsx"), source("EmergencyBroadcastAdministration.tsx"),
  source("DateRangePromotionAdministration.tsx"), source("api.ts")
]);

test("scheduling workspace exposes accessible deep-linked task navigation and precedence", () => {
  assert.match(operations, /role="tablist"/);
  assert.match(operations, /aria-selected/);
  assert.match(operations, /URLSearchParams/);
  assert.match(operations, /ArrowRight/);
  assert.match(operations, /Emergency broadcasts override normal content/);
  assert.match(operations, /No screens are available/);
});

test("meal periods persist priority, enable state, and destructive confirmation", () => {
  assert.match(meals, /reorderMealPeriods/);
  assert.match(meals, /Current:/);
  assert.match(meals, /Next:/);
  assert.match(meals, /useDestructiveReview/);
  assert.match(meals, /save\(\{ \.\.\.period, isEnabled:/);
  assert.match(api, /meal-periods.*\/order/s);
});

test("playlist changes are screen-scoped, editable, and recoverable", () => {
  assert.match(playlists, /Select a screen/);
  assert.match(playlists, /updatePlaylistSlide/);
  assert.match(playlists, /Active days/);
  assert.match(playlists, /useDestructiveReview/);
  assert.match(playlists, /role="status"/);
});

test("live overrides disclose target impact, confirm actions, and preserve history", () => {
  assert.match(broadcasts, /Target impact/);
  assert.match(broadcasts, /useDestructiveReview/);
  assert.match(broadcasts, /delivery acknowledgement is not currently available/);
  assert.match(broadcasts, /Recent broadcast history/);
  assert.match(promotions, /highest numeric priority wins/);
  assert.match(promotions, /useDestructiveReview/);
});
