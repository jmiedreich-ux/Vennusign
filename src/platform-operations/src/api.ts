import type { PlatformOperationsConfiguration } from "./config";

export type PlatformOperationsSession = {
  displayName: string;
  capabilities: string[];
};
export type SystemConfigurationHealth = { enabled: boolean; healthy: boolean; lastSuccessfulLoadUtc?: string; lastFailureUtc?: string; lastFailure?: string };
export type SystemConfigurationRevision = { revisionNumber: number; valueFingerprint: string; isSecret: boolean; isClear: boolean; changedBy: string; changeSource: string; createdUtc: string };
export type SystemConfigurationManifest = {
  schemaVersion: number; sourceEnvironment: string; exportedUtc: string;
  settings: Array<{ key: string; applicationScope: string; valueType: string; requiresRestart: boolean; value?: string }>;
};
export type SystemConfigurationImportPreview = {
  operationId: string; targetEnvironment: string;
  settings: Array<{ key: string; applicationScope: string; status: "New" | "Conflict" | "Unchanged" | "NoValue" | "Invalid"; value?: string; expectedVersion?: string; message?: string }>;
};
export type SystemConfigurationSetting = {
  definitionId: string; key: string; applicationScope: string; description: string; valueType: string;
  isRequired: boolean; isSecret: boolean; value?: string; hasConfiguredValue: boolean;
  requiresRestart: boolean; exportPolicy: string; version?: string; lastUpdatedUtc?: string; rotationReminderDays?: number;
};

async function configurationRequest(configuration: PlatformOperationsConfiguration, apiKey: string, path: string, init?: RequestInit) {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/configuration${path}`, {
    ...init,
    headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey, ...init?.headers }
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, response.status === 409 ? "This setting changed. Reload before saving again." : "Unable to manage configuration.");
  return response;
}
export async function loadSystemConfigurationHealth(configuration: PlatformOperationsConfiguration, apiKey: string): Promise<SystemConfigurationHealth> {
  return (await configurationRequest(configuration, apiKey, "/health")).json() as Promise<SystemConfigurationHealth>;
}
export async function loadSystemConfigurationRevisions(configuration: PlatformOperationsConfiguration, apiKey: string, setting: SystemConfigurationSetting, environmentName: string): Promise<SystemConfigurationRevision[]> {
  return (await configurationRequest(configuration, apiKey, `/${setting.definitionId}/revisions?environmentName=${encodeURIComponent(environmentName)}`)).json() as Promise<SystemConfigurationRevision[]>;
}
export async function rollbackSystemConfiguration(configuration: PlatformOperationsConfiguration, apiKey: string, setting: SystemConfigurationSetting, environmentName: string, revisionNumber: number): Promise<void> {
  await configurationRequest(configuration, apiKey, `/${setting.definitionId}/rollback`, { method: "POST", body: JSON.stringify({ environmentName, revisionNumber, expectedVersion: setting.version }) });
}

export async function exportSystemConfiguration(configuration: PlatformOperationsConfiguration, apiKey: string, environmentName: string): Promise<SystemConfigurationManifest> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/configuration-transfer/export?environmentName=${encodeURIComponent(environmentName)}`, { headers: { "X-Vennusign-Platform-Operations-Key": apiKey } });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to export configuration.");
  return response.json() as Promise<SystemConfigurationManifest>;
}

export async function previewSystemConfigurationImport(configuration: PlatformOperationsConfiguration, apiKey: string, targetEnvironment: string, manifest: SystemConfigurationManifest): Promise<SystemConfigurationImportPreview> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/configuration-transfer/preview`, { method: "POST", headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey }, body: JSON.stringify({ targetEnvironment, manifest }) });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to preview configuration import.");
  return response.json() as Promise<SystemConfigurationImportPreview>;
}

export async function applySystemConfigurationImport(configuration: PlatformOperationsConfiguration, apiKey: string, preview: SystemConfigurationImportPreview, selected: string[]): Promise<void> {
  const settings = preview.settings.filter(item => selected.includes(`${item.applicationScope}:${item.key}`));
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/configuration-transfer/apply`, { method: "POST", headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey }, body: JSON.stringify({ operationId: preview.operationId, targetEnvironment: preview.targetEnvironment, settings }) });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, response.status === 409 ? "The import preview is stale. Preview the file again." : "Unable to apply configuration import.");
}

export async function loadSystemConfiguration(configuration: PlatformOperationsConfiguration, apiKey: string, environmentName: string, applicationScope?: string): Promise<SystemConfigurationSetting[]> {
  const query = new URLSearchParams({ environmentName });
  if (applicationScope) query.set("applicationScope", applicationScope);
  return (await configurationRequest(configuration, apiKey, `?${query}`)).json() as Promise<SystemConfigurationSetting[]>;
}

export async function saveSystemConfiguration(configuration: PlatformOperationsConfiguration, apiKey: string, setting: SystemConfigurationSetting, environmentName: string, value: string): Promise<SystemConfigurationSetting> {
  return (await configurationRequest(configuration, apiKey, `/${setting.definitionId}`, { method: "PUT", body: JSON.stringify({ environmentName, value, expectedVersion: setting.version }) })).json() as Promise<SystemConfigurationSetting>;
}

export async function clearSystemConfiguration(configuration: PlatformOperationsConfiguration, apiKey: string, setting: SystemConfigurationSetting, environmentName: string): Promise<SystemConfigurationSetting> {
  return (await configurationRequest(configuration, apiKey, `/${setting.definitionId}`, { method: "DELETE", body: JSON.stringify({ environmentName, expectedVersion: setting.version }) })).json() as Promise<SystemConfigurationSetting>;
}

export type VenueDirectoryQuery = { search?: string; tier?: string; status?: string; health?: string };
export type CreateVenueRequest = {
  name: string; timezone: string; type: string; primaryLanguage: string; secondaryLanguage?: string;
};
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
  onlineScreens: number; offlineScreens: number; outdatedScreens: number;
  screens: Array<{
    screenId: string; venueId?: string; venueName: string; screenName: string;
    location?: string; status: "online" | "offline"; lastSeen?: string;
    platform?: string; appVersion?: string; desiredAppVersion?: string;
    versionStatus: "current" | "outdated" | "unknown";
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
export type OnboardingSupportItem = {
  userId: string; customerName: string; customerEmail: string;
  organizationId?: string; organizationName?: string; venueId?: string; venueName?: string;
  tierId?: string; tierName?: string; subscriptionStatus: string; trialEndsAt?: string;
  firstScreenId?: string; firstScreenName?: string; firstScreenStatus: "not-paired" | "paired-offline" | "online";
  firstScreenLastSeenUtc?: string; lastActivityUtc: string;
};
export type DateRangePromotion = {
  id: string; venueId: string; name: string; startLocalDate: string; endLocalDate: string;
  targetLayout?: string; title?: string; body?: string; priority: number; isEnabled: boolean;
};
export type TapCategory = {
  id: string; venueId: string; name: string; categoryPrice?: number;
  sortOrder: number; isActive: boolean;
};
export type TapItem = {
  id: string; venueId: string; tapCategoryId?: string; name: string; style?: string;
  abv?: number; ibu?: number; description?: string; price: number;
  glassColor?: string; nameColor?: string; isAvailable: boolean; isComingSoon: boolean; sortOrder: number;
};
export type TapListSnapshot = { categories: TapCategory[]; items: TapItem[] };
export type ManagedScreen = {
  id: string; name: string; location?: string; status: string;
  photoGridDensity: "2x2" | "3x2" | "4x2" | "3x3";
  displayLayout: "photo_grid" | "classic_diner" | "neon_chalkboard" | "split_layout" | "daily_special_hero" | "classic_chalkboard" | "tap_strips" | "digital_tap_board";
  splitRatio: "40_60" | "50_50";
  heroDwellSeconds: number;
  lastSeen?: string; registrationUrl: string;
};
export type ManagedScreenWrite = {
  name: string;
  location?: string;
  photoGridDensity?: ManagedScreen["photoGridDensity"];
  displayLayout?: ManagedScreen["displayLayout"];
  splitRatio?: ManagedScreen["splitRatio"];
  heroDwellSeconds?: number;
};
export type ScreenOverflowPreview = {
  capacity: number; totalItems: number; visibleItems: number; overflowItems: number;
  items: Array<{ itemId: string; sectionName: string; itemName: string; visible: boolean }>;
};
export type VideoWallGroup = {
  name: string; layout: string;
  screens: Array<{ id: string; name: string; position: number }>;
};
export type VideoWallSnapshot = { enabled: boolean; groups: VideoWallGroup[] };
export type VenueTheme = {
  venueId: string;
  backgroundColor: string;
  accentColor: string;
  fontFamily: "Inter" | "Georgia" | "Arial";
  presetKey: string;
  titleColor: string;
  glowColor: string;
  boardBackgroundColor: string;
  sectionColors: string[];
  glowIntensity: number;
  titleFont: "Pacifico" | "Lobster" | "Righteous" | "Fredoka One" | "Bungee" | "Permanent Marker";
  itemFont: "Caveat" | "Kalam" | "Patrick Hand" | "Permanent Marker";
  updatedUtc: string;
};
export type VenueThemePreset = Omit<VenueTheme, "venueId" | "backgroundColor" | "accentColor" | "fontFamily" | "presetKey" | "updatedUtc"> & {
  key: string;
  label: string;
};
export type MealPeriod = {
  id: string; venueId: string; name: string; startLocalTime: string; endLocalTime: string;
  activeDaysMask: number; isEnabled: boolean; sortOrder: number;
  targetLayout?: string; menuFilter?: string; themePresetKey?: string;
};
export type MealPeriodWrite = Pick<MealPeriod, "name" | "startLocalTime" | "endLocalTime" | "activeDaysMask" | "isEnabled" | "targetLayout" | "menuFilter" | "themePresetKey">;
export type MealPeriodSnapshot = {
  mealPeriods: MealPeriod[];
  conflicts: Array<{ firstId: string; firstName: string; secondId: string; secondName: string }>;
};
export type HappyHourSnapshot = {
  schedule?: {
    venueId: string; startLocalTime: string; endLocalTime: string;
    activeDaysMask: number; isEnabled: boolean;
    overrideMode: "automatic" | "force_on" | "force_off"; updatedUtc: string;
  };
  isActive: boolean;
  endsAtUtc?: string;
  mode: "automatic" | "force_on" | "force_off";
  isEntitled: boolean;
};
export type HappyHourWrite = {
  startLocalTime: string; endLocalTime: string; activeDaysMask: number;
  isEnabled: boolean; overrideMode: HappyHourSnapshot["mode"];
};
export type PlaylistSlide = {
  id: string; venueId: string; screenId: string;
  slideType: "menu" | "image" | "message"; title?: string; body?: string; mediaUrl?: string;
  dwellSeconds: number; startLocalTime?: string; endLocalTime?: string; activeDaysMask?: number;
  isEnabled: boolean; sortOrder: number;
};
export type PlaylistSlideWrite = Omit<PlaylistSlide, "id" | "venueId" | "screenId" | "sortOrder">;
export type EmergencyBroadcast = {
  id: string; venueId: string; screenId?: string; title: string; message: string; mediaUrl?: string;
  startsUtc: string; expiresUtc: string; isActive: boolean;
};

export class PlatformOperationsApiError extends Error {
  constructor(public readonly status: number, message: string) {
    super(message);
  }
}

async function mealPeriodRequest(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, path = "", init?: RequestInit) {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/venues/${venueId}/meal-periods${path}`, {
    ...init,
    headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey, ...init?.headers }
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to manage meal periods.");
  return response;
}

export async function loadMealPeriods(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string): Promise<MealPeriodSnapshot> {
  return (await mealPeriodRequest(configuration, apiKey, venueId)).json() as Promise<MealPeriodSnapshot>;
}

export async function createMealPeriod(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, value: MealPeriodWrite): Promise<MealPeriod> {
  return (await mealPeriodRequest(configuration, apiKey, venueId, "", { method: "POST", body: JSON.stringify(value) })).json() as Promise<MealPeriod>;
}

export async function updateMealPeriod(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, value: MealPeriod): Promise<MealPeriod> {
  return (await mealPeriodRequest(configuration, apiKey, venueId, `/${value.id}`, { method: "PUT", body: JSON.stringify(value) })).json() as Promise<MealPeriod>;
}

export async function deleteMealPeriod(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, id: string): Promise<void> {
  await mealPeriodRequest(configuration, apiKey, venueId, `/${id}`, { method: "DELETE" });
}

async function happyHourRequest(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, init?: RequestInit) {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/venues/${venueId}/happy-hour`, {
    ...init,
    headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey, ...init?.headers }
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to manage happy hour.");
  return response;
}

export async function loadHappyHour(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string): Promise<HappyHourSnapshot> {
  return (await happyHourRequest(configuration, apiKey, venueId)).json() as Promise<HappyHourSnapshot>;
}

export async function saveHappyHour(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, value: HappyHourWrite): Promise<HappyHourSnapshot> {
  return (await happyHourRequest(configuration, apiKey, venueId, { method: "PUT", body: JSON.stringify(value) })).json() as Promise<HappyHourSnapshot>;
}

const playlistUrl = (configuration: PlatformOperationsConfiguration, venueId: string, screenId: string) =>
  `${configuration.apiBaseUrl}/api/platform-operations/venues/${venueId}/screens/${screenId}/playlist`;
async function playlistRequest(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, screenId: string, path = "", init?: RequestInit) {
  const response = await fetch(`${playlistUrl(configuration, venueId, screenId)}${path}`, {
    ...init, headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey, ...init?.headers }
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to manage playlist.");
  return response;
}
export async function loadPlaylist(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, screenId: string): Promise<PlaylistSlide[]> {
  return (await playlistRequest(configuration, apiKey, venueId, screenId)).json() as Promise<PlaylistSlide[]>;
}
export async function createPlaylistSlide(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, screenId: string, value: PlaylistSlideWrite): Promise<PlaylistSlide> {
  return (await playlistRequest(configuration, apiKey, venueId, screenId, "", { method: "POST", body: JSON.stringify(value) })).json() as Promise<PlaylistSlide>;
}
export async function reorderPlaylist(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, screenId: string, orderedIds: string[]): Promise<PlaylistSlide[]> {
  return (await playlistRequest(configuration, apiKey, venueId, screenId, "/order", { method: "PUT", body: JSON.stringify({ orderedIds }) })).json() as Promise<PlaylistSlide[]>;
}
export async function deletePlaylistSlide(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, screenId: string, slideId: string) {
  await playlistRequest(configuration, apiKey, venueId, screenId, `/${slideId}`, { method: "DELETE" });
}
async function broadcastRequest(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, path = "", init?: RequestInit) {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/venues/${venueId}/emergency-broadcasts${path}`, {
    ...init, headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey, ...init?.headers }
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to manage emergency broadcast.");
  return response;
}
export async function loadEmergencyBroadcasts(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string): Promise<EmergencyBroadcast[]> {
  return (await broadcastRequest(configuration, apiKey, venueId)).json() as Promise<EmergencyBroadcast[]>;
}
export async function createEmergencyBroadcast(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, value: {
  screenId?: string; title: string; message: string; mediaUrl?: string; durationMinutes: number;
}): Promise<EmergencyBroadcast> {
  return (await broadcastRequest(configuration, apiKey, venueId, "", { method: "POST", body: JSON.stringify(value) })).json() as Promise<EmergencyBroadcast>;
}
export async function cancelEmergencyBroadcast(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, id: string) {
  await broadcastRequest(configuration, apiKey, venueId, `/${id}`, { method: "DELETE" });
}

export async function loadSession(configuration: PlatformOperationsConfiguration, apiKey: string, signal?: AbortSignal): Promise<PlatformOperationsSession> {
  if (!apiKey) {
    throw new PlatformOperationsApiError(401, "Platform Operations access has not been configured.");
  }

  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/session`, {
    headers: { "X-Vennusign-Platform-Operations-Key": apiKey },
    signal
  });
  if (!response.ok) {
    throw new PlatformOperationsApiError(response.status, "Unable to authorize this Platform Operations session.");
  }

  return response.json() as Promise<PlatformOperationsSession>;
}

export async function loadVenueDirectory(configuration: PlatformOperationsConfiguration, apiKey: string, query: VenueDirectoryQuery, signal?: AbortSignal): Promise<VenueDirectoryItem[]> {
  const parameters = new URLSearchParams();
  Object.entries(query).forEach(([key, value]) => { if (value) parameters.set(key, value); });
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/venues?${parameters}`, {
    headers: { "X-Vennusign-Platform-Operations-Key": apiKey },
    signal
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to load venue directory.");
  return response.json() as Promise<VenueDirectoryItem[]>;
}

export async function loadOnboardingSupport(
  configuration: PlatformOperationsConfiguration,
  apiKey: string,
  search = "",
  signal?: AbortSignal
): Promise<OnboardingSupportItem[]> {
  const parameters = new URLSearchParams();
  if (search.trim()) parameters.set("search", search.trim());
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/onboarding?${parameters}`, {
    headers: { "X-Vennusign-Platform-Operations-Key": apiKey }, signal
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, response.status === 403 ? "This Platform Operations session cannot view customer onboarding." : "Unable to load customer onboarding support.");
  return response.json() as Promise<OnboardingSupportItem[]>;
}

export async function createVenue(
  configuration: PlatformOperationsConfiguration,
  apiKey: string,
  request: CreateVenueRequest
): Promise<{ venueId: string }> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/venues`, {
    method: "POST",
    headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey },
    body: JSON.stringify(request)
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to create this venue.");
  return response.json() as Promise<{ venueId: string }>;
}

export async function loadVenueSupportDetail(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, signal?: AbortSignal): Promise<VenueSupportDetail | undefined> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/venues/${venueId}`, {
    headers: { "X-Vennusign-Platform-Operations-Key": apiKey },
    signal
  });
  if (response.status === 404) return undefined;
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to load venue support detail.");
  return response.json() as Promise<VenueSupportDetail>;
}

async function tierRequest(configuration: PlatformOperationsConfiguration, apiKey: string, path = "", init?: RequestInit) {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/tiers${path}`, {
    ...init, headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey, ...init?.headers }
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to manage subscription tiers.");
  return response;
}

export async function loadTiers(configuration: PlatformOperationsConfiguration, apiKey: string): Promise<SubscriptionTier[]> {
  return (await tierRequest(configuration, apiKey)).json() as Promise<SubscriptionTier[]>;
}
export async function saveTier(configuration: PlatformOperationsConfiguration, apiKey: string, request: TierManagementRequest, tierId?: string): Promise<SubscriptionTier> {
  return (await tierRequest(configuration, apiKey, tierId ? `/${tierId}` : "", { method: tierId ? "PUT" : "POST", body: JSON.stringify(request) })).json() as Promise<SubscriptionTier>;
}
export async function cloneTier(configuration: PlatformOperationsConfiguration, apiKey: string, tierId: string): Promise<SubscriptionTier> {
  return (await tierRequest(configuration, apiKey, `/${tierId}/clone`, { method: "POST" })).json() as Promise<SubscriptionTier>;
}
export async function archiveTier(configuration: PlatformOperationsConfiguration, apiKey: string, tierId: string): Promise<void> {
  await tierRequest(configuration, apiKey, `/${tierId}/archive`, { method: "POST" });
}

async function featureRequest(configuration: PlatformOperationsConfiguration, apiKey: string, init?: RequestInit) {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/features`, {
    ...init, headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey, ...init?.headers }
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to manage the feature matrix.");
  return response;
}
export async function loadFeatureMatrix(configuration: PlatformOperationsConfiguration, apiKey: string): Promise<FeatureMatrixSnapshot> {
  return (await featureRequest(configuration, apiKey)).json() as Promise<FeatureMatrixSnapshot>;
}
export async function saveFeatureMatrix(configuration: PlatformOperationsConfiguration, apiKey: string, changes: FeatureMatrixChange[]): Promise<{ changedCount: number }> {
  return (await featureRequest(configuration, apiKey, { method: "PUT", body: JSON.stringify({ changes }) })).json() as Promise<{ changedCount: number }>;
}

export async function saveVenueFeatureOverride(
  configuration: PlatformOperationsConfiguration,
  apiKey: string,
  venueId: string,
  featureId: string,
  request: { enabled: boolean; reason: string; expiresAt?: string }
): Promise<void> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/venues/${venueId}/overrides/${featureId}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey },
    body: JSON.stringify(request)
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to save the venue feature override.");
}

export async function removeVenueFeatureOverride(
  configuration: PlatformOperationsConfiguration,
  apiKey: string,
  venueId: string,
  featureId: string
): Promise<void> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/venues/${venueId}/overrides/${featureId}`, {
    method: "DELETE",
    headers: { "X-Vennusign-Platform-Operations-Key": apiKey }
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to remove the venue feature override.");
}

export async function switchVenueTier(
  configuration: PlatformOperationsConfiguration,
  apiKey: string,
  venueId: string,
  targetTierId: string
): Promise<void> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/venues/${venueId}/tier`, {
    method: "PUT",
    headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey },
    body: JSON.stringify({ targetTierId })
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to switch the venue tier.");
}

export async function loadOperationalDashboard(configuration: PlatformOperationsConfiguration, apiKey: string): Promise<OperationalDashboard> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/dashboard`, {
    headers: { "X-Vennusign-Platform-Operations-Key": apiKey }
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to load the operational dashboard.");
  return response.json() as Promise<OperationalDashboard>;
}

export async function loadRevenueSnapshot(configuration: PlatformOperationsConfiguration, apiKey: string): Promise<RevenueSnapshot> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/dashboard/revenue`, {
    headers: { "X-Vennusign-Platform-Operations-Key": apiKey }
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to load live Stripe revenue.");
  return response.json() as Promise<RevenueSnapshot>;
}

export async function loadRevenueTrend(
  configuration: PlatformOperationsConfiguration,
  apiKey: string,
  months = 12
): Promise<RevenueTrend> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/dashboard/revenue/trend?months=${months}`, {
    headers: { "X-Vennusign-Platform-Operations-Key": apiKey }
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to load the revenue trend.");
  return response.json() as Promise<RevenueTrend>;
}

export async function loadOperationalEvents(configuration: PlatformOperationsConfiguration, apiKey: string): Promise<OperationalEvent[]> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/dashboard/events?limit=20`, {
    headers: { "X-Vennusign-Platform-Operations-Key": apiKey }
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to load recent commercial events.");
  return response.json() as Promise<OperationalEvent[]>;
}

async function promotionRequest(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, path = "", init?: RequestInit) {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/venues/${venueId}/date-range-promotions${path}`, {
    ...init, headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey, ...init?.headers }
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to manage date-range promotions.");
  return response;
}
export async function loadDateRangePromotions(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string): Promise<DateRangePromotion[]> {
  return (await promotionRequest(configuration, apiKey, venueId)).json() as Promise<DateRangePromotion[]>;
}
export async function saveDateRangePromotion(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, value: Omit<DateRangePromotion, "id" | "venueId">, id?: string): Promise<DateRangePromotion> {
  return (await promotionRequest(configuration, apiKey, venueId, id ? `/${id}` : "", {
    method: id ? "PUT" : "POST", body: JSON.stringify(value)
  })).json() as Promise<DateRangePromotion>;
}
export async function archiveDateRangePromotion(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, id: string): Promise<void> {
  await promotionRequest(configuration, apiKey, venueId, `/${id}`, { method: "DELETE" });
}

async function tapRequest(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, path = "", init?: RequestInit) {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/venues/${venueId}/tap-list${path}`, {
    ...init, headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey, ...init?.headers }
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to manage the tap list.");
  return response;
}
export async function loadTapList(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string): Promise<TapListSnapshot> {
  return (await tapRequest(configuration, apiKey, venueId)).json() as Promise<TapListSnapshot>;
}
export async function saveTapCategory(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, value: Omit<TapCategory, "id" | "venueId" | "sortOrder">, id?: string): Promise<TapCategory> {
  return (await tapRequest(configuration, apiKey, venueId, `/categories${id ? `/${id}` : ""}`, { method: id ? "PUT" : "POST", body: JSON.stringify(value) })).json() as Promise<TapCategory>;
}
export async function deleteTapCategory(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, id: string): Promise<void> {
  await tapRequest(configuration, apiKey, venueId, `/categories/${id}`, { method: "DELETE" });
}
export async function saveTapItem(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, value: Omit<TapItem, "id" | "venueId" | "sortOrder">, id?: string): Promise<TapItem> {
  return (await tapRequest(configuration, apiKey, venueId, `/items${id ? `/${id}` : ""}`, { method: id ? "PUT" : "POST", body: JSON.stringify(value) })).json() as Promise<TapItem>;
}
export async function deleteTapItem(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, id: string): Promise<void> {
  await tapRequest(configuration, apiKey, venueId, `/items/${id}`, { method: "DELETE" });
}
export async function reorderTapRows(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, kind: "categories" | "items", ids: string[]): Promise<void> {
  await tapRequest(configuration, apiKey, venueId, `/${kind}/order`, { method: "PUT", body: JSON.stringify({ ids }) });
}

// The ops menu editing client is retired (Q36): menu content changes are the
// venue's own, made through its draft-and-publish flow. Ops has no menu write
// path until the backlogged impersonation-with-consent model exists.

async function screenRequest(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, path = "", init?: RequestInit) {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/venues/${venueId}/screens${path}`, {
    ...init,
    headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey, ...init?.headers }
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to manage venue screens.");
  return response;
}

export async function loadManagedScreens(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string): Promise<ManagedScreen[]> {
  return (await screenRequest(configuration, apiKey, venueId)).json() as Promise<ManagedScreen[]>;
}

export async function claimPairingCode(
  configuration: PlatformOperationsConfiguration,
  apiKey: string,
  venueId: string,
  code: string
): Promise<{ linked: boolean; screenId: string; venueId: string }> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/screens/pairing/${encodeURIComponent(code)}/claim`, {
    method: "POST",
    headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey },
    body: JSON.stringify({ venueId })
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to pair this screen.");
  return response.json() as Promise<{ linked: boolean; screenId: string; venueId: string }>;
}

export async function createManagedScreen(
  configuration: PlatformOperationsConfiguration,
  apiKey: string,
  venueId: string,
  request: ManagedScreenWrite
): Promise<ManagedScreen> {
  return (await screenRequest(configuration, apiKey, venueId, "", { method: "POST", body: JSON.stringify(request) })).json() as Promise<ManagedScreen>;
}

export async function updateManagedScreen(
  configuration: PlatformOperationsConfiguration,
  apiKey: string,
  venueId: string,
  screenId: string,
  request: ManagedScreenWrite
): Promise<ManagedScreen> {
  return (await screenRequest(configuration, apiKey, venueId, `/${screenId}`, { method: "PUT", body: JSON.stringify(request) })).json() as Promise<ManagedScreen>;
}

export async function pushManagedScreen(
  configuration: PlatformOperationsConfiguration,
  apiKey: string,
  venueId: string,
  screenId: string
): Promise<void> {
  await screenRequest(configuration, apiKey, venueId, `/${screenId}/push`, { method: "POST" });
}

export async function pushAllManagedScreens(
  configuration: PlatformOperationsConfiguration,
  apiKey: string,
  venueId: string
): Promise<{ screenCount: number }> {
  return (await screenRequest(configuration, apiKey, venueId, "/push-all", { method: "POST" })).json() as Promise<{ screenCount: number }>;
}

export async function loadScreenOverflow(
  configuration: PlatformOperationsConfiguration,
  apiKey: string,
  venueId: string,
  capacity: number
): Promise<ScreenOverflowPreview> {
  return (await screenRequest(configuration, apiKey, venueId, `/overflow?capacity=${capacity}`)).json() as Promise<ScreenOverflowPreview>;
}

export async function loadVideoWalls(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string): Promise<VideoWallSnapshot> {
  return (await screenRequest(configuration, apiKey, venueId, "/video-walls")).json() as Promise<VideoWallSnapshot>;
}

export async function saveVideoWall(
  configuration: PlatformOperationsConfiguration,
  apiKey: string,
  venueId: string,
  request: { name: string; layout: string; screenIds: string[] }
): Promise<VideoWallGroup> {
  return (await screenRequest(configuration, apiKey, venueId, "/video-walls", {
    method: "PUT", body: JSON.stringify(request)
  })).json() as Promise<VideoWallGroup>;
}

export async function removeVideoWall(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, name: string): Promise<void> {
  await screenRequest(configuration, apiKey, venueId, `/video-walls/${encodeURIComponent(name)}`, { method: "DELETE" });
}

async function themeRequest(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string, path = "", init?: RequestInit) {
  const response = await fetch(`${configuration.apiBaseUrl}/api/platform-operations/venues/${venueId}/theme${path}`, {
    ...init,
    headers: { "Content-Type": "application/json", "X-Vennusign-Platform-Operations-Key": apiKey, ...init?.headers }
  });
  if (!response.ok) throw new PlatformOperationsApiError(response.status, "Unable to manage the venue theme.");
  return response;
}

export async function loadVenueTheme(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string): Promise<VenueTheme> {
  return (await themeRequest(configuration, apiKey, venueId)).json() as Promise<VenueTheme>;
}

export async function loadVenueThemePresets(configuration: PlatformOperationsConfiguration, apiKey: string, venueId: string): Promise<VenueThemePreset[]> {
  return (await themeRequest(configuration, apiKey, venueId, "/presets")).json() as Promise<VenueThemePreset[]>;
}

export async function saveVenueTheme(
  configuration: PlatformOperationsConfiguration,
  apiKey: string,
  venueId: string,
  theme: Pick<VenueTheme, "backgroundColor" | "accentColor" | "fontFamily">
): Promise<VenueTheme> {
  return (await themeRequest(configuration, apiKey, venueId, "", {
    method: "PUT",
    body: JSON.stringify(theme)
  })).json() as Promise<VenueTheme>;
}

export async function saveAdvancedVenueTheme(
  configuration: PlatformOperationsConfiguration,
  apiKey: string,
  venueId: string,
  theme: Pick<VenueTheme, "titleColor" | "glowColor" | "boardBackgroundColor" | "sectionColors" | "glowIntensity" | "titleFont" | "itemFont">
): Promise<VenueTheme> {
  return (await themeRequest(configuration, apiKey, venueId, "/advanced", {
    method: "PUT",
    body: JSON.stringify(theme)
  })).json() as Promise<VenueTheme>;
}

export async function applyVenueThemePreset(
  configuration: PlatformOperationsConfiguration,
  apiKey: string,
  venueId: string,
  presetKey: string
): Promise<VenueTheme> {
  return (await themeRequest(configuration, apiKey, venueId, `/presets/${encodeURIComponent(presetKey)}`, {
    method: "PUT"
  })).json() as Promise<VenueTheme>;
}
