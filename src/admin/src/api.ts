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
  tier?: { name: string; slug: string; maxScreens: number; isPublic: boolean; isActive: boolean };
  screens: Array<{ id: string; name: string; location?: string; status: string; lastSeen?: string; platform?: string; appVersion?: string }>;
  features: Record<string, { key: string; enabled: boolean; limitValue?: string; source: string }>;
  activeOverrides: Array<{ featureId: string; enabled: boolean; reason: string; expiresAt?: string }>;
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
