/*
 * Every area of the Back Office, and how the nav rail draws it.
 *
 * `icon` names a lucide-react export (owner decision Q185, replacing the
 * hand-rolled glyph set). `railLabel` is the short form the 76px rail shows
 * under the icon; the full label still names the page and its route.
 *
 * `hiddenWhenLocked` is decision 19, as data rather than a branch in the
 * markup: "Menus is itself tier-gated. A venue whose plan is one screen of
 * static content has no menu to build. The Menu nav item does not render at
 * all - no shelf, no import, no empty state."
 *
 * It is set on Menu alone, deliberately. The other areas keep the locked
 * previews Track 1 shipped on purpose as an upgrade path (RWP-11.02/11.04);
 * turning those off is a product decision about upselling, not a Menus one, and
 * quietly deleting them under cover of this milestone would be the wrong way to
 * make it. Raised for the owner rather than settled here.
 */
export const backOfficeRoutes = [
  { path: "home", label: "Home", railLabel: "Home", icon: "House", description: "Today at your venue", group: "Operate" },
  { path: "menu", label: "Menu", railLabel: "Menu", icon: "UtensilsCrossed", description: "Items and quick updates", group: "Operate", capabilityId: "content.item.update", upgradeFeature: "quick_update", hiddenWhenLocked: true },
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
 * Whether this route appears in the rail at all.
 *
 * Absent is not the same as locked. A locked area is one this account could
 * have and does not; an absent one is a whole way of working the plan does not
 * include, and showing it greyed would be advertising a product they are not
 * being sold (decision 19, criterion 8).
 */
export function isBackOfficeRouteVisible(route, decisions) {
  if (!route?.hiddenWhenLocked) return true;
  return canOpenBackOfficeRoute(route, decisions);
}

export function resolveBackOfficeRoute(hash) {
  const value = String(hash ?? "").replace(/^#\/?/, "");
  return backOfficeRoutes.find(route => route.path === value) ?? backOfficeRoutes[0];
}

export function decisionForBackOfficeRoute(route, decisions) {
  return route.capabilityId ? decisions.find(decision => decision.capabilityId === route.capabilityId) : undefined;
}

export function canOpenBackOfficeRoute(route, decisions) {
  if (!route.capabilityId) return true;
  const decision = decisionForBackOfficeRoute(route, decisions);
  return decision?.decision === "allowed" || decision?.decision === "allowed-with-conditions";
}
