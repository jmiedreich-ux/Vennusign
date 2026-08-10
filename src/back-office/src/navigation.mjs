/*
 * Every area of the Back Office, and how the nav rail draws it.
 *
 * `icon` names a lucide-react export (owner decision Q185, replacing the
 * hand-rolled glyph set). `railLabel` is the short form the 76px rail shows
 * under the icon; the full label still names the page and its route.
 *
 * Decision 19 - "Menus is itself tier-gated... The Menu nav item does not render
 * at all" - is one case of decision 4, which governs every area: "locked by plan
 * means invisible... absent, not disabled - no ghost fields, no reasons, no
 * state". So the rule lives in `isBackOfficeRouteVisible` rather than on any one
 * route. Upgrade and marketing surfaces are their own scheduled work
 * (milestone-plan, After this build); the shell does not carry them.
 */
export const backOfficeRoutes = [
  { path: "home", label: "Home", railLabel: "Home", icon: "House", description: "Today at your venue", group: "Operate" },
  { path: "menu", label: "Menu", railLabel: "Menu", icon: "UtensilsCrossed", description: "Items and quick updates", group: "Operate", capabilityId: "content.item.update", upgradeFeature: "quick_update" },
  { path: "schedules", label: "Schedules", railLabel: "Schedules", icon: "Clock", description: "Timing and broadcasts", group: "Operate", capabilityId: "schedule.entry.manage", upgradeFeature: "meal_periods" },
  { path: "tap-list", label: "Tap list", railLabel: "Taps", icon: "Beer", description: "Draft board operations", group: "Operate", capabilityId: "content.item.update", upgradeFeature: "all_layouts" },
  { path: "screens", label: "Screens", railLabel: "Screens", icon: "Monitor", description: "Boards and playback", group: "Design & delivery", capabilityId: "screen.device.view", upgradeFeature: "all_layouts" },
  { path: "themes", label: "Themes", railLabel: "Themes", icon: "Palette", description: "Brand and layouts", group: "Design & delivery", capabilityId: "branding.theme.manage", upgradeFeature: "all_layouts" },
  { path: "pos", label: "POS integrations", railLabel: "POS", icon: "ArrowLeftRight", description: "Catalog and availability sync", group: "Connect", capabilityId: "content.source.synchronize", upgradeFeature: "pos_integration" },
  { path: "billing", label: "Billing", railLabel: "Billing", icon: "CreditCard", description: "Plan and payments", group: "Account", capabilityId: "account.billing.view" },
  { path: "security", label: "Account & security", railLabel: "Account", icon: "ShieldCheck", description: "Passkeys and recovery", group: "Account", capabilityId: "account.security.manage" },
];

export const backOfficeNavigationGroups = ["Operate", "Design & delivery", "Connect", "Account"].map(label => ({
  label,
  routes: backOfficeRoutes.filter(route => route.group === label)
}));

/**
 * The rail is one flat column, not the grouped sidebar it replaces: at 76px
 * there is no room for group headings, and the design's own spec has a single
 * divider near the bottom rather than four sections.
 *
 * The divider separates the work of running a venue from the account itself.
 * The design sketch shows one "Settings" item there; this ships the two account
 * routes that actually exist instead, because inventing a Settings destination
 * would be exactly the phantom Q100 forbids - anything with no target is absent.
 */
export const backOfficeRailSections = [
  { key: "work", routes: backOfficeRoutes.filter(route => route.group !== "Account") },
  { key: "account", routes: backOfficeRoutes.filter(route => route.group === "Account") }
];

/**
 * What the plan does not include, rather than what this person cannot do.
 *
 * Decisions 4 and 5 draw the line between them, and it is the whole of the
 * gating rule:
 *
 *   - **Outside the plan** - an entitlement or an add-on the account does not
 *     buy - is *invisible*. Criterion 8: renders nothing, no disabled control,
 *     no tooltip, no placeholder. Showing it greyed is advertising, and
 *     advertising is its own scheduled work, not something the shell does.
 *   - **Blocked for a real reason** - a permission this role lacks, a rollout
 *     not yet reached, an allowance already spent - still renders and still
 *     says what it is. Decision 5: blocked is not absent. An editor who cannot
 *     open Screens needs to know Screens exists and who can open it.
 */
const planCategories = new Set(["entitlement", "addon", "add_on"]);

export function isBackOfficeRouteVisible(route, decisions) {
  if (!route?.capabilityId) return true;
  if (canOpenBackOfficeRoute(route, decisions)) return true;

  const decision = decisionForBackOfficeRoute(route, decisions);

  // No decision at all is not evidence the plan excludes it. Treated as blocked
  // rather than absent, so a missing decision fails towards saying something.
  if (!decision) return true;

  return !planCategories.has(String(decision.category ?? "").toLowerCase());
}

/**
 * The route a fragment names, matched on its FIRST segment.
 *
 * Milestone 3 gives the builder its own address — `#/menu/{menuId}` — so that a
 * browser refresh mid-edit, the back button and a pasted link all land back on
 * the menu somebody was working on. An exact match would send every one of those
 * to Home instead. An unknown first segment still falls back to Home, which is
 * what keeps a mistyped address from rendering a blank area.
 */
export function resolveBackOfficeRoute(hash) {
  const value = String(hash ?? "").replace(/^#\/?/, "");
  const [head] = value.split(/[/?]/);
  return backOfficeRoutes.find(route => route.path === head) ?? backOfficeRoutes[0];
}

/**
 * The menu a `#/menu/{menuId}` fragment names, or null for the shelf itself.
 * Returns the id as written: this decides which menu to ASK for, and the API
 * decides whether it is one of this venue's.
 */
export function menuIdFromHash(hash) {
  const value = String(hash ?? "")
    .replace(/^#\/?/, "")
    .split("?")[0];
  const [head, id] = value.split("/");
  return head === "menu" && id ? decodeURIComponent(id) : null;
}

export function decisionForBackOfficeRoute(route, decisions) {
  return route.capabilityId ? decisions.find(decision => decision.capabilityId === route.capabilityId) : undefined;
}

export function canOpenBackOfficeRoute(route, decisions) {
  if (!route.capabilityId) return true;
  const decision = decisionForBackOfficeRoute(route, decisions);
  return decision?.decision === "allowed" || decision?.decision === "allowed-with-conditions";
}
