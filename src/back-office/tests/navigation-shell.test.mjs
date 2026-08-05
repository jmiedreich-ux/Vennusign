import assert from "node:assert/strict";
import test from "node:test";
import {
  canOpenBackOfficeRoute,
  resolveBackOfficeRoute,
  backOfficeRoutes,
  backOfficeNavigationGroups
} from "../src/navigation.mjs";

test("Back Office shell exposes only implemented customer routes", () => {
  assert.deepEqual(
    backOfficeRoutes.map(route => route.path),
    ["home", "menu", "schedules", "tap-list", "screens", "themes", "pos", "billing", "security"]
  );
  assert.equal(resolveBackOfficeRoute("#/screens").path, "screens");
  assert.equal(resolveBackOfficeRoute("#/unknown").path, "home");
  assert.deepEqual(backOfficeNavigationGroups.map(group => group.label), ["Operate", "Design & delivery", "Connect", "Account"]);
});

test("navigation consumes structured server decisions using canonical capability IDs", () => {
  const menu = backOfficeRoutes.find(route => route.path === "menu");
  assert.equal(canOpenBackOfficeRoute(menu, []), false);
  assert.equal(canOpenBackOfficeRoute(menu, [{ capabilityId: "content.item.update", decision: "allowed" }]), true);
  const pos = backOfficeRoutes.find(route => route.path === "pos");
  assert.equal(canOpenBackOfficeRoute(pos, []), false);
  assert.equal(canOpenBackOfficeRoute(pos, [{ capabilityId: "content.source.synchronize", decision: "unavailable" }]), false);
});
