import type { VenueAdminConfiguration } from "./config";

export type VenueAdminSession = {
  venueId: string;
  displayName: string;
  capabilities: string[];
};

export class VenueAdminApiError extends Error {
  constructor(public readonly status: number, message: string) {
    super(message);
  }
}

export async function loadVenueAdminSession(
  configuration: VenueAdminConfiguration,
  accessToken: string,
  signal?: AbortSignal
): Promise<VenueAdminSession> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/venue-admin/session`, {
    headers: { "X-Vennu-Venue-Token": accessToken },
    signal
  });
  if (!response.ok) {
    throw new VenueAdminApiError(
      response.status,
      response.status === 401
        ? "That venue access link is invalid or has expired."
        : "The venue workspace is unavailable."
    );
  }
  return response.json() as Promise<VenueAdminSession>;
}
