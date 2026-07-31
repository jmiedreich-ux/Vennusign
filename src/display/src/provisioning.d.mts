export function claimPreRegisteredScreen(
  baseUrl: string,
  token: string,
  platform: string,
  appVersion: string,
  fetchImpl?: typeof fetch
): Promise<{ screenId: string; screenKey: string; venueId: string; displayPath: string }>;
