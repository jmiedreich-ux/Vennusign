export type BackOfficeRoute = {
  path: string;
  label: string;
  description: string;
  capability?: string;
  upgradeFeature?: string;
};

export const backOfficeRoutes: BackOfficeRoute[];
export function resolveBackOfficeRoute(hash: string): BackOfficeRoute;
export function canOpenBackOfficeRoute(route: BackOfficeRoute, capabilities: string[]): boolean;
