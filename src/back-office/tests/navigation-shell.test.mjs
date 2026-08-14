import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";
import {
  canOpenBackOfficeRoute,
  isBackOfficeRouteVisible,
  isMenuImportHash,
  menuIdFromHash,
  menuImportSessionIdFromHash,
  resolveBackOfficeRoute,
  backOfficeRoutes,
  backOfficeNavigationGroups,
  backOfficeRailSections
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

test("paste import routes never masquerade as a menu builder id", () => {
  const sessionId = "160955c9-b634-4dcd-8f60-472ccb0f863c";
  assert.equal(isMenuImportHash("#/menu/import"), true);
  assert.equal(isMenuImportHash(`#/menu/import/${sessionId}`), true);
  assert.equal(menuIdFromHash("#/menu/import"), null);
  assert.equal(menuIdFromHash(`#/menu/import/${sessionId}`), null);
  assert.equal(menuImportSessionIdFromHash("#/menu/import"), null);
  assert.equal(menuImportSessionIdFromHash(`#/menu/import/${sessionId}`), sessionId);
});

test("navigation consumes structured server decisions using canonical capability IDs", () => {
  const menu = backOfficeRoutes.find(route => route.path === "menu");
  assert.equal(canOpenBackOfficeRoute(menu, []), false);
  assert.equal(canOpenBackOfficeRoute(menu, [{ capabilityId: "content.item.update", decision: "allowed" }]), true);
  assert.equal(canOpenBackOfficeRoute(menu, [{ capabilityId: "content.item.availability_update", decision: "allowed" }]), true);
  const pos = backOfficeRoutes.find(route => route.path === "pos");
  assert.equal(canOpenBackOfficeRoute(pos, []), false);
  assert.equal(canOpenBackOfficeRoute(pos, [{ capabilityId: "content.source.synchronize", decision: "unavailable" }]), false);
});

// ---- the 76px rail ---------------------------------------------------------

test("the rail is one flat column with a single divider before the account items", () => {
  // At 76px there is no room for the four collapsible group headings the old
  // 270px sidebar carried; the design's spec is one divider near the bottom.
  assert.deepEqual(backOfficeRailSections.map(section => section.key), ["work", "account"]);

  assert.deepEqual(
    backOfficeRailSections[0].routes.map(route => route.path),
    ["home", "menu", "schedules", "tap-list", "screens", "themes", "pos"]
  );
  assert.deepEqual(
    backOfficeRailSections[1].routes.map(route => route.path),
    ["billing", "security"]
  );

  // Every route reaches the rail: the shell hosts every area, so the gating is
  // built once rather than per page.
  assert.equal(
    backOfficeRailSections.flatMap(section => section.routes).length,
    backOfficeRoutes.length
  );
});

test("every route names an icon and a label short enough for the rail", () => {
  for (const route of backOfficeRoutes) {
    assert.ok(route.icon?.length, `${route.path} has no icon`);
    assert.ok(route.railLabel?.length, `${route.path} has no rail label`);
    // The rail is 76px wide with 56px items; a long word cannot fit at 9px, so
    // it would either wrap into the icon or overflow the rail.
    assert.ok(route.railLabel.length <= 9, `${route.path}'s rail label "${route.railLabel}" is too long for 56px`);
  }
});

// Decision 4, of which decision 19's Menus case is one instance: "locked by plan
// means invisible... absent, not disabled - no ghost fields, no reasons, no
// state." Criterion 8 states the same thing as a check.
test("an area the plan does not include renders nothing at all", () => {
  const menu = backOfficeRoutes.find(route => route.path === "menu");
  const outsideThePlan = [{ capabilityId: "content.item.update", decision: "unavailable", category: "entitlement" }];

  assert.equal(isBackOfficeRouteVisible(menu, outsideThePlan), false);

  // An add-on nobody bought is the same answer: it is not part of this account.
  const pos = backOfficeRoutes.find(route => route.path === "pos");
  assert.equal(
    isBackOfficeRouteVisible(pos, [{ capabilityId: "content.source.synchronize", decision: "denied", category: "addOn" }]),
    false
  );
});

// Decision 5: blocked is not absent. A real state says what it is.
test("an area this role cannot open still renders and still says so", () => {
  const screens = backOfficeRoutes.find(route => route.path === "screens");
  const notThisRole = [{ capabilityId: "screen.device.view", decision: "denied", category: "permission" }];

  assert.equal(isBackOfficeRouteVisible(screens, notThisRole), true);
  assert.equal(canOpenBackOfficeRoute(screens, notThisRole), false);

  // A rollout not yet reached, and an allowance already spent, are facts about
  // today rather than about the plan - so they explain themselves too.
  for (const category of ["rollout", "allowance", "resourceState"]) {
    assert.equal(
      isBackOfficeRouteVisible(screens, [{ capabilityId: "screen.device.view", decision: "denied", category }]),
      true,
      `a ${category} refusal should still render`
    );
  }
});

test("a route with no decision at all is shown rather than quietly dropped", () => {
  // Absence of evidence is not evidence the plan excludes it. Failing towards
  // saying something beats a rail that silently loses an area on a bad response.
  const menu = backOfficeRoutes.find(route => route.path === "menu");

  assert.equal(isBackOfficeRouteVisible(menu, []), true);
  assert.equal(canOpenBackOfficeRoute(menu, []), false);
});

test("an area with no capability behind it is always shown", () => {
  const home = backOfficeRoutes.find(route => route.path === "home");

  assert.equal(isBackOfficeRouteVisible(home, []), true);
});

test("the rail keeps the handles the mobile drawer and the specs address", async () => {
  const rail = await readFile(new URL("../src/NavRail.tsx", import.meta.url), "utf8");

  // The toggle controls #app-sidebar by aria-controls, and every UI spec finds
  // areas through [data-testid="nav-item"][data-route]. Renaming the component
  // must not rename what the rest of the system reaches for.
  assert.match(rail, /id="app-sidebar"/);
  assert.match(rail, /data-testid="nav-item"/);
  assert.match(rail, /data-route=\{route\.path\}/);
  assert.match(rail, /data-unlocked=\{unlocked\}/);
  assert.match(rail, /data-active=/);
  assert.match(rail, /aria-label="VennueSign Back Office"/);
  assert.match(rail, /rail-brand-mark__signal/);
  assert.match(rail, /className="rail-item-label"/);
});

test("the rail's styling comes from tokens rather than baked-in values", async () => {
  const tokens = await readFile(new URL("../src/sky-ui-tokens.css", import.meta.url), "utf8");
  const styles = await readFile(new URL("../src/styles.css", import.meta.url), "utf8");
  const rail = styles.slice(styles.indexOf(".app-rail")).split("@media")[0];

  // The design README's spec, held as tokens so a second shell theme can change
  // them without reopening the component (Q86: one ships, built so others can
  // be added - variables only, no colour baked in).
  assert.match(tokens, /--sky-rail-width:\s*76px/);
  assert.match(tokens, /--sky-rail-item-width:\s*56px/);
  assert.match(tokens, /--sky-font-size-2xs:\s*0\.5625rem/); /* 9px rail labels */

  assert.doesNotMatch(rail, /:\s*#[0-9a-f]{3,8}\b/i);
  assert.match(rail, /\.rail-item-label\s*\{[^}]*var\(--sky-font-size-2xs\)/s);
});
