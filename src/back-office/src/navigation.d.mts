export type BackOfficeRoute = {
  path: string;
  label: string;
  /** The short form the 76px rail shows under the icon. */
  railLabel: string;
  /** The name of a lucide-react export (Q185). */
  icon: string;
  description: string;
  group: string;
  capabilityId?: string;
  upgradeFeature?: string;
  /** Decision 19: absent from the rail entirely when the plan does not include it. */
  hiddenWhenLocked?: boolean;
};

export const backOfficeRoutes: BackOfficeRoute[];
export const backOfficeNavigationGroups: Array<{ label: string; routes: BackOfficeRoute[] }>;
export const backOfficeRailSections: Array<{ key: string; routes: BackOfficeRoute[] }>;
export function isBackOfficeRouteVisible(
  route: BackOfficeRoute,
  decisions: BackOfficeRouteDecision[]
): boolean;
export function resolveBackOfficeRoute(hash: string): BackOfficeRoute;
export type BackOfficeRouteDecision = { capabilityId: string; decision: string; message: string; category: string; resolution?: string };
export function decisionForBackOfficeRoute(route: BackOfficeRoute, decisions: BackOfficeRouteDecision[]): BackOfficeRouteDecision | undefined;
export function canOpenBackOfficeRoute(route: BackOfficeRoute, decisions: BackOfficeRouteDecision[]): boolean;
