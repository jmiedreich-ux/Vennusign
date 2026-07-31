export type VenueAdminRoute = {
  path: string;
  label: string;
  description: string;
  capability?: string;
  upgradeFeature?: string;
};

export const venueAdminRoutes: VenueAdminRoute[];
export function resolveVenueAdminRoute(hash: string): VenueAdminRoute;
export function canOpenVenueAdminRoute(route: VenueAdminRoute, capabilities: string[]): boolean;
