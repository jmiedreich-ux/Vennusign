export type BackOfficeRoute = {
  path: string;
  label: string;
  description: string;
  group: string;
  capability?: string;
  upgradeFeature?: string;
};

export const backOfficeRoutes: BackOfficeRoute[];
export const backOfficeNavigationGroups: Array<{ label: string; routes: BackOfficeRoute[] }>;
export function resolveBackOfficeRoute(hash: string): BackOfficeRoute;
export function canOpenBackOfficeRoute(route: BackOfficeRoute, capabilities: string[]): boolean;
