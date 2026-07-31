export const venueAdminRoutes = [
  { path: "home", label: "Home", description: "Venue overview" },
  { path: "menu", label: "Menu", description: "Items and quick updates", capability: "menus" },
  { path: "screens", label: "Screens", description: "Boards and playback", capability: "screens" },
  { path: "themes", label: "Themes", description: "Brand and layouts", capability: "themes" },
  { path: "schedules", label: "Schedules", description: "Timing and broadcasts", capability: "scheduling" },
  { path: "tap-list", label: "Tap list", description: "Draft board operations", capability: "tap_list" },
  { path: "settings", label: "Settings", description: "Venue and support" }
];

export function resolveVenueAdminRoute(hash) {
  const value = String(hash ?? "").replace(/^#\/?/, "");
  return venueAdminRoutes.find(route => route.path === value) ?? venueAdminRoutes[0];
}

export function canOpenVenueAdminRoute(route, capabilities) {
  return !route.capability || capabilities.includes(route.capability);
}
