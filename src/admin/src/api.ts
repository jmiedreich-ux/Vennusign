import type { AdminConfiguration } from "./config";

export type AdminSession = {
  displayName: string;
  capabilities: string[];
};

export type VenueDirectoryQuery = { search?: string; tier?: string; status?: string; health?: string };
export type VenueDirectoryItem = {
  venueId: string; name: string; type: string; tierId?: string; tierName?: string;
  subscriptionStatus: string; screenCount: number; lastActiveUtc?: string;
  overrideCount: number; health: string;
};
export type VenueSupportDetail = {
  venue: { id: string; name: string; timezone: string; type: string; primaryLanguage: string; secondaryLanguage?: string };
  subscription?: { status: string; stripeSubscriptionId?: string; trialEndsAt?: string; currentPeriodEnd?: string };
  tier?: { id: string; name: string; slug: string; maxScreens: number; isPublic: boolean; isActive: boolean };
  screens: Array<{ id: string; name: string; location?: string; status: string; lastSeen?: string; platform?: string; appVersion?: string }>;
  features: Record<string, { key: string; enabled: boolean; limitValue?: string; source: string }>;
  activeOverrides: Array<{ featureId: string; enabled: boolean; reason: string; expiresAt?: string }>;
};
export type SubscriptionTier = {
  id: string; name: string; slug: string; price: number; maxScreens: number;
  isPublic: boolean; isActive: boolean; stripeProductId?: string;
  stripeMonthlyPriceId?: string; stripeAnnualPriceId?: string;
};
export type TierManagementRequest = Omit<SubscriptionTier, "id">;
export type FeatureMatrixSnapshot = {
  tiers: SubscriptionTier[];
  features: Array<{ id: string; key: string; label: string; category: string; isActive: boolean }>;
  enabledFeatures: Array<{ tierId: string; featureId: string; limitValue?: string }>;
  recentAudit: Array<{
    id: string; tierId: string; featureId: string; adminId: string;
    previousEnabled: boolean; newEnabled: boolean; changedUtc: string;
  }>;
};
export type FeatureMatrixChange = { tierId: string; featureId: string; enabled: boolean };
export type OperationalDashboard = {
  totalVenues: number; activeVenues: number; trialingVenues: number; canceledLast30Days: number;
  onlineScreens: number; offlineScreens: number;
  screens: Array<{
    screenId: string; venueId?: string; venueName: string; screenName: string;
    location?: string; status: "online" | "offline"; lastSeen?: string;
  }>;
};
export type RevenueSnapshot = {
  currency: string; mrr: number; arr: number; averageRevenuePerActiveSubscription: number;
  activeSubscriptions: number; unmatchedMrr: number; unmatchedPriceIds: string[];
  tiers: Array<{ tierId: string; tierName: string; mrr: number }>;
};
export type RevenueTrend = {
  currency: string;
  points: Array<{ monthUtc: string; mrr: number; activeSubscriptions: number; mrrChangePercent: number | null }>;
};
export type OperationalEvent = {
  id: string; venueId: string; venueName: string; eventType: string; summary: string; occurredUtc: string;
};

export class AdminApiError extends Error {
  constructor(public readonly status: number, message: string) {
    super(message);
  }
}

export async function loadSession(configuration: AdminConfiguration, apiKey: string, signal?: AbortSignal): Promise<AdminSession> {
  if (!apiKey) {
    throw new AdminApiError(401, "Super Admin access has not been configured.");
  }

  const response = await fetch(`${configuration.apiBaseUrl}/api/admin/session`, {
    headers: { "X-Vennu-Admin-Key": apiKey },
    signal
  });
  if (!response.ok) {
    throw new AdminApiError(response.status, "Unable to authorize this Super Admin session.");
  }

  return response.json() as Promise<AdminSession>;
}

export async function loadVenueDirectory(configuration: AdminConfiguration, apiKey: string, query: VenueDirectoryQuery, signal?: AbortSignal): Promise<VenueDirectoryItem[]> {
  const parameters = new URLSearchParams();
  Object.entries(query).forEach(([key, value]) => { if (value) parameters.set(key, value); });
  const response = await fetch(`${configuration.apiBaseUrl}/api/admin/venues?${parameters}`, {
    headers: { "X-Vennu-Admin-Key": apiKey },
    signal
  });
  if (!response.ok) throw new AdminApiError(response.status, "Unable to load venue directory.");
  return response.json() as Promise<VenueDirectoryItem[]>;
}

export async function loadVenueSupportDetail(configuration: AdminConfiguration, apiKey: string, venueId: string, signal?: AbortSignal): Promise<VenueSupportDetail | undefined> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/admin/venues/${venueId}`, {
    headers: { "X-Vennu-Admin-Key": apiKey },
    signal
  });
  if (response.status === 404) return undefined;
  if (!response.ok) throw new AdminApiError(response.status, "Unable to load venue support detail.");
  return response.json() as Promise<VenueSupportDetail>;
}

async function tierRequest(configuration: AdminConfiguration, apiKey: string, path = "", init?: RequestInit) {
  const response = await fetch(`${configuration.apiBaseUrl}/api/admin/tiers${path}`, {
    ...init, headers: { "Content-Type": "application/json", "X-Vennu-Admin-Key": apiKey, ...init?.headers }
  });
  if (!response.ok) throw new AdminApiError(response.status, "Unable to manage subscription tiers.");
  return response;
}

export async function loadTiers(configuration: AdminConfiguration, apiKey: string): Promise<SubscriptionTier[]> {
  return (await tierRequest(configuration, apiKey)).json() as Promise<SubscriptionTier[]>;
}
export async function saveTier(configuration: AdminConfiguration, apiKey: string, request: TierManagementRequest, tierId?: string): Promise<SubscriptionTier> {
  return (await tierRequest(configuration, apiKey, tierId ? `/${tierId}` : "", { method: tierId ? "PUT" : "POST", body: JSON.stringify(request) })).json() as Promise<SubscriptionTier>;
}
export async function cloneTier(configuration: AdminConfiguration, apiKey: string, tierId: string): Promise<SubscriptionTier> {
  return (await tierRequest(configuration, apiKey, `/${tierId}/clone`, { method: "POST" })).json() as Promise<SubscriptionTier>;
}
export async function archiveTier(configuration: AdminConfiguration, apiKey: string, tierId: string): Promise<void> {
  await tierRequest(configuration, apiKey, `/${tierId}/archive`, { method: "POST" });
}

async function featureRequest(configuration: AdminConfiguration, apiKey: string, init?: RequestInit) {
  const response = await fetch(`${configuration.apiBaseUrl}/api/admin/features`, {
    ...init, headers: { "Content-Type": "application/json", "X-Vennu-Admin-Key": apiKey, ...init?.headers }
  });
  if (!response.ok) throw new AdminApiError(response.status, "Unable to manage the feature matrix.");
  return response;
}
export async function loadFeatureMatrix(configuration: AdminConfiguration, apiKey: string): Promise<FeatureMatrixSnapshot> {
  return (await featureRequest(configuration, apiKey)).json() as Promise<FeatureMatrixSnapshot>;
}
export async function saveFeatureMatrix(configuration: AdminConfiguration, apiKey: string, changes: FeatureMatrixChange[]): Promise<{ changedCount: number }> {
  return (await featureRequest(configuration, apiKey, { method: "PUT", body: JSON.stringify({ changes }) })).json() as Promise<{ changedCount: number }>;
}

export async function saveVenueFeatureOverride(
  configuration: AdminConfiguration,
  apiKey: string,
  venueId: string,
  featureId: string,
  request: { enabled: boolean; reason: string; expiresAt?: string }
): Promise<void> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/admin/venues/${venueId}/overrides/${featureId}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", "X-Vennu-Admin-Key": apiKey },
    body: JSON.stringify(request)
  });
  if (!response.ok) throw new AdminApiError(response.status, "Unable to save the venue feature override.");
}

export async function removeVenueFeatureOverride(
  configuration: AdminConfiguration,
  apiKey: string,
  venueId: string,
  featureId: string
): Promise<void> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/admin/venues/${venueId}/overrides/${featureId}`, {
    method: "DELETE",
    headers: { "X-Vennu-Admin-Key": apiKey }
  });
  if (!response.ok) throw new AdminApiError(response.status, "Unable to remove the venue feature override.");
}

export async function switchVenueTier(
  configuration: AdminConfiguration,
  apiKey: string,
  venueId: string,
  targetTierId: string
): Promise<void> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/admin/venues/${venueId}/tier`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", "X-Vennu-Admin-Key": apiKey },
    body: JSON.stringify({ targetTierId })
  });
  if (!response.ok) throw new AdminApiError(response.status, "Unable to switch the venue tier.");
}

export async function loadOperationalDashboard(configuration: AdminConfiguration, apiKey: string): Promise<OperationalDashboard> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/admin/dashboard`, {
    headers: { "X-Vennu-Admin-Key": apiKey }
  });
  if (!response.ok) throw new AdminApiError(response.status, "Unable to load the operational dashboard.");
  return response.json() as Promise<OperationalDashboard>;
}

export async function loadRevenueSnapshot(configuration: AdminConfiguration, apiKey: string): Promise<RevenueSnapshot> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/admin/dashboard/revenue`, {
    headers: { "X-Vennu-Admin-Key": apiKey }
  });
  if (!response.ok) throw new AdminApiError(response.status, "Unable to load live Stripe revenue.");
  return response.json() as Promise<RevenueSnapshot>;
}

export async function loadRevenueTrend(
  configuration: AdminConfiguration,
  apiKey: string,
  months = 12
): Promise<RevenueTrend> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/admin/dashboard/revenue/trend?months=${months}`, {
    headers: { "X-Vennu-Admin-Key": apiKey }
  });
  if (!response.ok) throw new AdminApiError(response.status, "Unable to load the revenue trend.");
  return response.json() as Promise<RevenueTrend>;
}

export async function loadOperationalEvents(configuration: AdminConfiguration, apiKey: string): Promise<OperationalEvent[]> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/admin/dashboard/events?limit=20`, {
    headers: { "X-Vennu-Admin-Key": apiKey }
  });
  if (!response.ok) throw new AdminApiError(response.status, "Unable to load recent commercial events.");
  return response.json() as Promise<OperationalEvent[]>;
}
