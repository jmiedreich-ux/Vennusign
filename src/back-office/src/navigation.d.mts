export type BackOfficeRoute = {
  path: string;
  label: string;
  description: string;
  group: string;
  capabilityId?: string;
  upgradeFeature?: string;
};

export const backOfficeRoutes: BackOfficeRoute[];
export const backOfficeNavigationGroups: Array<{ label: string; routes: BackOfficeRoute[] }>;
export function resolveBackOfficeRoute(hash: string): BackOfficeRoute;
export type BackOfficeRouteDecision = { capabilityId: string; decision: string; message: string; category: string; resolution?: string };
export function decisionForBackOfficeRoute(route: BackOfficeRoute, decisions: BackOfficeRouteDecision[]): BackOfficeRouteDecision | undefined;
export function canOpenBackOfficeRoute(route: BackOfficeRoute, decisions: BackOfficeRouteDecision[]): boolean;
