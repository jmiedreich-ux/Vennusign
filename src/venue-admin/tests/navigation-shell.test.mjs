import assert from "node:assert/strict";
import test from "node:test";
import {
  canOpenVenueAdminRoute,
  resolveVenueAdminRoute,
  venueAdminRoutes
} from "../src/navigation.mjs";

test("venue admin shell has the bounded foundation routes", () => {
  assert.deepEqual(
    venueAdminRoutes.map(route => route.path),
    ["home", "menu", "screens", "themes", "schedules", "tap-list", "settings"]
  );
  assert.equal(resolveVenueAdminRoute("#/screens").path, "screens");
  assert.equal(resolveVenueAdminRoute("#/unknown").path, "home");
});

test("capability routes retain deterministic locked and unlocked states", () => {
  const menu = venueAdminRoutes.find(route => route.path === "menu");
  assert.equal(canOpenVenueAdminRoute(menu, []), false);
  assert.equal(canOpenVenueAdminRoute(menu, ["menus"]), true);
  assert.equal(canOpenVenueAdminRoute(resolveVenueAdminRoute("#/settings"), []), true);
});
