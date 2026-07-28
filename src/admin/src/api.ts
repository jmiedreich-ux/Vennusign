import type { AdminConfiguration } from "./config";

export type AdminSession = {
  displayName: string;
  capabilities: string[];
};

export class AdminApiError extends Error {
  constructor(public readonly status: number, message: string) {
    super(message);
  }
}

export async function loadSession(configuration: AdminConfiguration, signal?: AbortSignal): Promise<AdminSession> {
  if (!configuration.apiKey) {
    throw new AdminApiError(401, "Super Admin access has not been configured.");
  }

  const response = await fetch(`${configuration.apiBaseUrl}/api/admin/session`, {
    headers: { "X-Vennu-Admin-Key": configuration.apiKey },
    signal
  });
  if (!response.ok) {
    throw new AdminApiError(response.status, "Unable to authorize this Super Admin session.");
  }

  return response.json() as Promise<AdminSession>;
}

