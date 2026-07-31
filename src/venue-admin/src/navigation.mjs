export const venueAdminRoutes = [
  { path: "home", label: "Home", description: "Venue overview" },
  { path: "menu", label: "Menu", description: "Items and quick updates", capability: "menus" },
  { path: "screens", label: "Screens", description: "Boards and playback", capability: "screens" },
  { path: "settings", label: "Settings", description: "Venue and support" }
];

export function resolveVenueAdminRoute(hash) {
  const value = String(hash ?? "").replace(/^#\/?/, "");
  return venueAdminRoutes.find(route => route.path === value) ?? venueAdminRoutes[0];
}

export function canOpenVenueAdminRoute(route, capabilities) {
  return !route.capability || capabilities.includes(route.capability);
}
