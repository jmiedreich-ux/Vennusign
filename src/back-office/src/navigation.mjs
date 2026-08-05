export const backOfficeRoutes = [
  { path: "home", label: "Home", description: "Today at your venue", group: "Operate" },
  { path: "menu", label: "Menu", description: "Items and quick updates", group: "Operate", capabilityId: "content.item.update", upgradeFeature: "quick_update" },
  { path: "schedules", label: "Schedules", description: "Timing and broadcasts", group: "Operate", capabilityId: "schedule.entry.manage", upgradeFeature: "meal_periods" },
  { path: "tap-list", label: "Tap list", description: "Draft board operations", group: "Operate", capabilityId: "content.item.update", upgradeFeature: "all_layouts" },
  { path: "screens", label: "Screens", description: "Boards and playback", group: "Design & delivery", capabilityId: "screen.device.view", upgradeFeature: "all_layouts" },
  { path: "themes", label: "Themes", description: "Brand and layouts", group: "Design & delivery", capabilityId: "branding.theme.manage", upgradeFeature: "all_layouts" },
  { path: "pos", label: "POS integrations", description: "Catalog and availability sync", group: "Connect", capabilityId: "content.source.synchronize", upgradeFeature: "pos_integration" },
  { path: "billing", label: "Billing", description: "Plan and payments", group: "Account", capabilityId: "account.billing.view" },
  { path: "security", label: "Account & security", description: "Passkeys and recovery", group: "Account", capabilityId: "account.security.manage" },
];

export const backOfficeNavigationGroups = ["Operate", "Design & delivery", "Connect", "Account"].map(label => ({
  label,
  routes: backOfficeRoutes.filter(route => route.group === label)
}));

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
