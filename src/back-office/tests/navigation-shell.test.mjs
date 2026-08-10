import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";
import {
  canOpenBackOfficeRoute,
  isBackOfficeRouteVisible,
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

test("navigation consumes structured server decisions using canonical capability IDs", () => {
  const menu = backOfficeRoutes.find(route => route.path === "menu");
  assert.equal(canOpenBackOfficeRoute(menu, []), false);
  assert.equal(canOpenBackOfficeRoute(menu, [{ capabilityId: "content.item.update", decision: "allowed" }]), true);
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

// Decision 19: "Menus is itself tier-gated... The Menu nav item does not render
// at all - no shelf, no import, no empty state."
test("a plan without menus shows no Menu item at all, rather than a locked one", () => {
  const menu = backOfficeRoutes.find(route => route.path === "menu");

  assert.equal(isBackOfficeRouteVisible(menu, []), false);
  assert.equal(
    isBackOfficeRouteVisible(menu, [{ capabilityId: "content.item.update", decision: "allowed" }]),
    true
  );
});

test("the other areas keep the locked previews they already had", () => {
  // Absent and locked are different answers, and only Menus has been decided.
  // Turning the rest off is a product decision about upselling (RWP-11.02/11.04),
  // not something this milestone should do quietly on its way past.
  for (const route of backOfficeRoutes.filter(route => route.path !== "menu")) {
    assert.equal(isBackOfficeRouteVisible(route, []), true, `${route.path} should still render when locked`);
  }
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
