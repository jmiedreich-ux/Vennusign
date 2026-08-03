import assert from "node:assert/strict";
import test from "node:test";
import {
  canOpenBackOfficeRoute,
  resolveBackOfficeRoute,
  backOfficeRoutes
} from "../src/navigation.mjs";

test("Back Office shell has the bounded foundation routes", () => {
  assert.deepEqual(
    backOfficeRoutes.map(route => route.path),
    ["home", "menu", "screens", "themes", "schedules", "tap-list", "billing", "settings"]
  );
  assert.equal(resolveBackOfficeRoute("#/screens").path, "screens");
  assert.equal(resolveBackOfficeRoute("#/unknown").path, "home");
});

test("capability routes retain deterministic locked and unlocked states", () => {
  const menu = backOfficeRoutes.find(route => route.path === "menu");
  assert.equal(canOpenBackOfficeRoute(menu, []), false);
  assert.equal(canOpenBackOfficeRoute(menu, ["menus"]), true);
  assert.equal(canOpenBackOfficeRoute(resolveBackOfficeRoute("#/settings"), []), true);
});
