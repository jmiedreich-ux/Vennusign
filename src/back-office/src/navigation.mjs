export const backOfficeRoutes = [
  { path: "home", label: "Home", description: "Today at your venue", group: "Operate" },
  { path: "menu", label: "Menu", description: "Items and quick updates", group: "Operate", capability: "menus", upgradeFeature: "quick_update" },
  { path: "schedules", label: "Schedules", description: "Timing and broadcasts", group: "Operate", capability: "scheduling", upgradeFeature: "meal_periods" },
  { path: "tap-list", label: "Tap list", description: "Draft board operations", group: "Operate", capability: "tap_list", upgradeFeature: "all_layouts" },
  { path: "screens", label: "Screens", description: "Boards and playback", group: "Design & delivery", capability: "screens", upgradeFeature: "all_layouts" },
  { path: "themes", label: "Themes", description: "Brand and layouts", group: "Design & delivery", capability: "themes", upgradeFeature: "all_layouts" },
  { path: "pos", label: "POS integrations", description: "Catalog and availability sync", group: "Connect", capability: "pos_integration", upgradeFeature: "pos_integration" },
  { path: "billing", label: "Billing", description: "Plan and payments", group: "Account" },
  { path: "security", label: "Account & security", description: "Passkeys and recovery", group: "Account" },
];

export const backOfficeNavigationGroups = ["Operate", "Design & delivery", "Connect", "Account"].map(label => ({
  label,
  routes: backOfficeRoutes.filter(route => route.group === label)
}));

export function resolveBackOfficeRoute(hash) {
  const value = String(hash ?? "").replace(/^#\/?/, "");
  return backOfficeRoutes.find(route => route.path === value) ?? backOfficeRoutes[0];
}

export function canOpenBackOfficeRoute(route, capabilities) {
  return !route.capability || capabilities.includes(route.capability);
}
