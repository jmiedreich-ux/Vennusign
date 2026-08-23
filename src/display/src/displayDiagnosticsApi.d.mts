export type ServerDiagnostics = {
  screenId: string;
  venueId: string | null;
  screenKey: string;
  screenName: string;
  isAssignedToVenue: boolean;
  status: string;
  lastSeenUtc: string | null;
  secondsSinceLastSeen: number | null;
  isStale: boolean;
  platform: string | null;
  appVersion: string | null;
  desiredAppVersion: string | null;
  configuredWidthPixels: number;
  configuredHeightPixels: number;
  authoritativeRevision: number | null;
  appliedRevision: number | null;
  deliveryState: string | null;
  deliveryRequestedUtc: string | null;
  deliveryReceivedUtc: string | null;
  deliveryAppliedUtc: string | null;
  deliveryFailureCode: string | null;
  lastReceiptPlayerVersion: string | null;
  lastReceiptShellVersion: string | null;
  isOnboardingFirstScreen: boolean;
  onboardingGoLiveAchievedUtc: string | null;
};

export type ServerDiagnosticsResult =
  | { kind: 'ok'; diagnostics: ServerDiagnostics }
  | { kind: 'not-found' }
  | { kind: 'error'; message: string };

export function buildDisplayDiagnosticsUrl(apiBaseUrl: string, screenId: string): string;

export function loadServerDiagnostics(
  apiBaseUrl: string,
  screenId: string,
  fetchImpl?: typeof fetch
): Promise<ServerDiagnosticsResult>;
