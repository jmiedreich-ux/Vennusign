export const backOfficeRoutes = [
  { path: "home", label: "Home", description: "Venue overview" },
  { path: "menu", label: "Menu", description: "Items and quick updates", capability: "menus", upgradeFeature: "quick_update" },
  { path: "screens", label: "Screens", description: "Boards and playback", capability: "screens", upgradeFeature: "all_layouts" },
  { path: "themes", label: "Themes", description: "Brand and layouts", capability: "themes", upgradeFeature: "all_layouts" },
  { path: "schedules", label: "Schedules", description: "Timing and broadcasts", capability: "scheduling", upgradeFeature: "meal_periods" },
  { path: "tap-list", label: "Tap list", description: "Draft board operations", capability: "tap_list", upgradeFeature: "all_layouts" },
  { path: "billing", label: "Billing", description: "Plan and payments" },
  { path: "settings", label: "Settings", description: "Venue and support" }
];

export function resolveBackOfficeRoute(hash) {
  const value = String(hash ?? "").replace(/^#\/?/, "");
  return backOfficeRoutes.find(route => route.path === value) ?? backOfficeRoutes[0];
}

export function canOpenBackOfficeRoute(route, capabilities) {
  return !route.capability || capabilities.includes(route.capability);
}
