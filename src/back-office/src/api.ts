import type { BackOfficeConfiguration } from "./config";
import { requireHostedCheckoutUrl } from "./checkoutFlow.mjs";
import { requireHostedBillingPortalUrl } from "./billingPortal.mjs";

export type BackOfficeSession = {
  venueId: string;
  displayName: string;
  capabilityDecisions: BackOfficeCapabilityDecision[];
  organizationId?: string;
  organizationName: string;
  venueName: string;
  account: { userId?: string; displayName: string; email?: string };
  contexts: Array<{
    organizationId: string;
    organizationName: string;
    venueId: string;
    venueName: string;
  }>;
};
export type BackOfficeCapabilityDecision = {
  capabilityId: string;
  decision: "allowed" | "allowed-with-conditions" | "denied" | "unavailable" | "temporarily-blocked";
  reasonCode: string;
  category: string;
  messageKey: string;
  message: string;
  parameters: Record<string, string>;
  correlationId: string;
  locale: string;
  resolution?: string;
  retryAfterSeconds?: number;
  conditions: Array<{
    category: string;
    reasonCode: string;
    messageKey: string;
    message: string;
    parameters: Record<string, string>;
    resolution?: string;
  }>;
  isAllowed: boolean;
};
export type BackOfficeTierSummary = {
  id: string; name: string; slug: string; monthlyPrice: number; maxScreens: number; maxVenues: number;
  direction: "start" | "current" | "upgrade" | "downgrade";
  canSelect: boolean; blockingReasons: string[]; lostFeatures: string[];
};
export type BackOfficeBillingUsage = { activeScreens: number; currentScreenLimit: number; organizationVenues: number; currentVenueLimit: number };
export type BackOfficeBillingPresentation = {
  currentTier?: BackOfficeTierSummary;
  subscription?: BackOfficeSubscriptionSummary;
  usage: BackOfficeBillingUsage;
  availableTiers: BackOfficeTierSummary[];
  effectiveFeatures: Record<string, { enabled: boolean; limitValue?: string }>;
  haasBundles: BackOfficeHaasBundleSummary[];
  haasContract?: BackOfficeHaasContractSummary;
};
export type BackOfficeSubscriptionSummary = {
  status: "trialing" | "active" | "past_due" | "canceled";
  trialEndsAt?: string;
  currentPeriodEnd?: string;
  cancelAtPeriodEnd: boolean;
  canManageBilling: boolean;
};
export type BackOfficeHaasBundleSummary = {
  key: string; name: string; termMonths: 18 | 24 | 36;
  monthlyAmount: number; postContractTierSlug: string;
};
export type BackOfficeHaasContractSummary = {
  bundleKey: string; bundleName: string; status: "active" | "past_due" | "completed" | "canceled";
  termMonths: number; monthlyAmount: number; startedUtc: string; contractEndsUtc: string;
  remainingMonths: number; estimatedBuyoutAmount: number; cancelAtPeriodEnd: boolean; endedUtc?: string;
};
export type CheckoutBillingInterval = "monthly" | "annual";

export type MenuSection = {
  id: string; venueId: string; menuId: string; name: string;
  sortOrder: number; isActive: boolean; createdUtc: string; updatedUtc: string;
};
// The owner-killed concepts (happy-hour price, quantities, tags, popular, the
// availability auto-reset) are gone from the item shape: the library stores
// name, description and price, and availability is its own instant fact.
export type MenuItem = {
  id: string; venueId: string; menuSectionId: string; name: string;
  description?: string; price: number;
  sortOrder: number; isAvailable: boolean; isActive: boolean; createdUtc: string; updatedUtc: string;
};
export type MenuItemWrite = {
  name: string; description?: string; price: number;
};
export type MenuEditorSnapshot = {
  menus: Array<{
    menu: { id: string; venueId: string; name: string; isActive: boolean; dailySpecial?: string };
    sections: MenuSection[];
  }>;
  itemGroups: Array<{ sectionId: string; items: MenuItem[] }>;
  capabilities: { happyHour: boolean; allergenBadges: boolean; quickUpdate: boolean };
};
export type PosProvider = "square" | "toast" | "clover";
export type PosProviderStatus = {
  provider: PosProvider;
  connection?: { status: string; externalMerchantId: string; updatedUtc: string; accessTokenExpiresUtc?: string };
  guidance?: string;
  externalActionRequired?: boolean;
};
export type ManagedScreen = {
  id: string; name: string; location?: string; status: string;
  photoGridDensity: "2x2" | "3x2" | "4x2" | "3x3";
  displayLayout: "photo_grid" | "classic_diner" | "neon_chalkboard" | "split_layout" | "daily_special_hero" | "classic_chalkboard" | "tap_strips" | "digital_tap_board";
  splitRatio: "40_60" | "50_50";
  heroDwellSeconds: number;
  lastSeen?: string; platform?: string; appVersion?: string; registrationUrl: string;
  authoritativeRevision?: number; appliedRevision?: number; deliveryState?: "Requested" | "Received" | "Applied" | "Failed" | "Superseded" | "Recovered";
  deliveryRequestedUtc?: string; deliveryAppliedUtc?: string; deliveryFailureCode?: string; deliveryFailureDetail?: string;
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
  venueId: string; backgroundColor: string; accentColor: string;
  fontFamily: "Inter" | "Georgia" | "Arial"; presetKey: string;
  titleColor: string; glowColor: string; boardBackgroundColor: string;
  sectionColors: string[]; glowIntensity: number;
  titleFont: "Pacifico" | "Lobster" | "Righteous" | "Fredoka One" | "Bungee" | "Permanent Marker";
  itemFont: "Caveat" | "Kalam" | "Patrick Hand" | "Permanent Marker";
  updatedUtc: string;
};
export type VenueThemePreset = Omit<VenueTheme, "venueId" | "backgroundColor" | "accentColor" | "fontFamily" | "presetKey" | "updatedUtc"> & {
  key: string; label: string;
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
  venueLocalNow?: string; activeMealPeriodId?: string; nextMealPeriodId?: string; nextStartsLocal?: string;
};
export type HappyHourSnapshot = {
  schedule?: {
    venueId: string; startLocalTime: string; endLocalTime: string;
    activeDaysMask: number; isEnabled: boolean;
    overrideMode: "automatic" | "force_on" | "force_off"; updatedUtc: string;
  };
  isActive: boolean; endsAtUtc?: string;
  mode: "automatic" | "force_on" | "force_off"; isEntitled: boolean;
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

/** Shape the API returns when a capability check refuses a request (HTTP 403). */
export type BackOfficeCapabilityDenial = {
  capabilityId?: string;
  decision?: "denied" | "unavailable" | "temporarily-blocked" | string;
  reasonCode?: string;
  category?: string;
  message?: string;
  resolution?: string;
  retryAfterSeconds?: number;
  correlationId?: string;
};

export class BackOfficeApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
    /** Present when the API refused on capability grounds rather than failing. */
    public readonly denial?: BackOfficeCapabilityDenial
  ) {
    super(message);
  }
}

/**
 * A capability refusal carries the reason, the resolution and any retry-after window.
 * Reading it here is what lets callers separate "temporarily blocked by a rollout" from
 * "this failed to load", which are very different things to tell a user.
 */
async function readCapabilityDenial(response: Response): Promise<BackOfficeCapabilityDenial | undefined> {
  if (response.status !== 403) return undefined;
  try {
    const payload = await response.clone().json() as BackOfficeCapabilityDenial | undefined;
    return payload?.decision ? payload : undefined;
  } catch {
    return undefined;
  }
}

const venueContextStorageKey = "vennusign.back-office.venue-id";

export function clearBackOfficeVenueContext() {
  localStorage.removeItem(venueContextStorageKey);
}

function venueFetch(input: RequestInfo | URL, init?: RequestInit) {
  const headers = new Headers(init?.headers);
  if (headers.get("X-Vennusign-Back-Office-Token") === "customer-session") {
    headers.delete("X-Vennusign-Back-Office-Token");
    const selectedVenueId = localStorage.getItem(venueContextStorageKey);
    if (selectedVenueId && !headers.has("X-Vennusign-Venue-Id")) {
      headers.set("X-Vennusign-Venue-Id", selectedVenueId);
    }
  }
  return fetch(input, { ...init, headers, credentials: "include" });
}

async function requestBackOfficeSession(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  signal?: AbortSignal,
  selectedVenueId?: string
) {
  const headers: Record<string, string> = { "X-Vennusign-Back-Office-Token": accessToken };
  if (selectedVenueId) headers["X-Vennusign-Venue-Id"] = selectedVenueId;
  return venueFetch(`${configuration.apiBaseUrl}/api/back-office/session`, { headers, signal });
}

export async function loadBackOfficeSession(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  signal?: AbortSignal
): Promise<BackOfficeSession> {
  let response = await requestBackOfficeSession(configuration, accessToken, signal);
  if (response.status === 401 && accessToken === "customer-session" && localStorage.getItem(venueContextStorageKey)) {
    clearBackOfficeVenueContext();
    response = await requestBackOfficeSession(configuration, accessToken, signal);
  }
  if (!response.ok) {
    throw new BackOfficeApiError(
      response.status,
      response.status === 401
        ? accessToken === "customer-session"
          ? "No authorized venue workspace is available. Finish venue setup or ask an organization owner to restore your access."
          : "That venue access link is invalid or has expired."
        : "The venue workspace is unavailable."
    );
  }
  return response.json() as Promise<BackOfficeSession>;
}

export async function selectBackOfficeVenue(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  venueId: string,
  signal?: AbortSignal
): Promise<BackOfficeSession> {
  const response = await requestBackOfficeSession(configuration, accessToken, signal, venueId);
  if (!response.ok) {
    throw new BackOfficeApiError(
      response.status,
      response.status === 401
        ? "You no longer have access to that venue. Your current workspace was not changed."
        : "Vennusign could not switch venue workspaces."
    );
  }
  const session = await response.json() as BackOfficeSession;
  localStorage.setItem(venueContextStorageKey, session.venueId);
  return session;
}

export async function loadVenueBillingPresentation(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  signal?: AbortSignal
): Promise<BackOfficeBillingPresentation> {
  const response = await venueFetch(`${configuration.apiBaseUrl}/api/back-office/billing/presentation`, {
    headers: { "X-Vennusign-Back-Office-Token": accessToken },
    signal
  });
  if (!response.ok) {
    throw new BackOfficeApiError(response.status, "Upgrade options are unavailable.");
  }
  return response.json() as Promise<BackOfficeBillingPresentation>;
}

export async function createCheckoutSession(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  targetTierId: string,
  billingInterval: CheckoutBillingInterval,
  signal?: AbortSignal
): Promise<string> {
  const response = await venueFetch(`${configuration.apiBaseUrl}/api/back-office/billing/checkout-session`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "X-Vennusign-Back-Office-Token": accessToken
    },
    body: JSON.stringify({ targetTierId, billingInterval }),
    signal
  });
  if (!response.ok) {
    throw new BackOfficeApiError(response.status, "Secure checkout could not be opened.");
  }
  const payload = await response.json() as { checkoutUrl?: string };
  if (!payload.checkoutUrl) {
    throw new BackOfficeApiError(502, "Secure checkout returned an invalid response.");
  }
  try {
    return requireHostedCheckoutUrl(payload.checkoutUrl);
  } catch {
    throw new BackOfficeApiError(502, "Secure checkout returned an invalid response.");
  }
}

export async function createBillingPortalSession(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  signal?: AbortSignal
): Promise<string> {
  const response = await venueFetch(`${configuration.apiBaseUrl}/api/back-office/billing/portal-session`, {
    method: "POST",
    headers: { "X-Vennusign-Back-Office-Token": accessToken },
    signal
  });
  if (!response.ok) {
    throw new BackOfficeApiError(response.status, "Secure billing management could not be opened.");
  }
  const payload = await response.json() as { portalUrl?: string };
  if (!payload.portalUrl) {
    throw new BackOfficeApiError(502, "Secure billing management returned an invalid response.");
  }
  try {
    return requireHostedBillingPortalUrl(payload.portalUrl);
  } catch {
    throw new BackOfficeApiError(502, "Secure billing management returned an invalid response.");
  }
}

export async function createTierBillingPortalSession(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  targetTierId: string,
  signal?: AbortSignal
): Promise<string> {
  const response = await venueFetch(`${configuration.apiBaseUrl}/api/back-office/billing/tier-portal-session`, {
    method: "POST",
    headers: { "Content-Type": "application/json", "X-Vennusign-Back-Office-Token": accessToken },
    body: JSON.stringify({ targetTierId }),
    signal
  });
  if (!response.ok) throw new BackOfficeApiError(response.status, "This plan change is unavailable until its usage conflicts are resolved.");
  const payload = await response.json() as { portalUrl?: string };
  if (!payload.portalUrl) throw new BackOfficeApiError(502, "Secure billing management returned an invalid response.");
  try { return requireHostedBillingPortalUrl(payload.portalUrl); }
  catch { throw new BackOfficeApiError(502, "Secure billing management returned an invalid response."); }
}

export async function createHaasCheckoutSession(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  bundleKey: string,
  termMonths: number,
  signal?: AbortSignal
): Promise<string> {
  const response = await venueFetch(`${configuration.apiBaseUrl}/api/back-office/billing/haas-checkout-session`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "X-Vennusign-Back-Office-Token": accessToken
    },
    body: JSON.stringify({ bundleKey, termMonths }),
    signal
  });
  if (!response.ok) {
    throw new BackOfficeApiError(response.status, "Hardware bundle Checkout could not be opened.");
  }
  const payload = await response.json() as { checkoutUrl?: string };
  if (!payload.checkoutUrl) {
    throw new BackOfficeApiError(502, "Hardware bundle Checkout returned an invalid response.");
  }
  try {
    return requireHostedCheckoutUrl(payload.checkoutUrl);
  } catch {
    throw new BackOfficeApiError(502, "Hardware bundle Checkout returned an invalid response.");
  }
}

async function menuRequest(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  path = "",
  init?: RequestInit
) {
  const response = await venueFetch(`${configuration.apiBaseUrl}/api/back-office/menus${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      "X-Vennusign-Back-Office-Token": accessToken,
      ...init?.headers
    }
  });
  if (!response.ok) {
    /*
     * The server refuses in plain words - "That would be 62 menus, and this venue
     * is set up for 50. Put one away first, or ask us to raise the limit." -
     * and this used to throw that away for a generic string, so a real refusal
     * reached the person as nothing at all. The reason is a contract; the UI
     * repeats it rather than inventing a friendlier one that would drift.
     */
    const problem = (await response.json().catch(() => ({}))) as { detail?: string; title?: string; message?: string };
    throw new BackOfficeApiError(
      response.status,
      problem.detail ?? problem.message ?? problem.title ?? "Unable to manage menu content."
    );
  }
  return response;
}

export async function loadMenuEditor(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  _venueId: string
): Promise<MenuEditorSnapshot> {
  return (await menuRequest(configuration, accessToken)).json() as Promise<MenuEditorSnapshot>;
}

export async function createMenu(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  name: string
): Promise<MenuEditorSnapshot["menus"][number]["menu"]> {
  return (await menuRequest(configuration, accessToken, "", {
    method: "POST",
    body: JSON.stringify({ name })
  })).json() as Promise<MenuEditorSnapshot["menus"][number]["menu"]>;
}

/*
 * The section and item writes that used to live here went with the editor they
 * served. Milestone 3's builder writes through `api/back-office/content`, where
 * every rule is decided inside the statement that writes it — the next sort order
 * under a lock, the ceiling under a lock, "already on this board", and whether a
 * reorder list still matches the menu.
 *
 * `loadMenuEditor` and `updateQuickAvailability` stay: Home and the locked-section
 * preview still read them, and they are not Menus surfaces.
 */

export async function updateQuickAvailability(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  _venueId: string,
  menuId: string,
  sectionId: string,
  itemId: string,
  isAvailable: boolean
): Promise<void> {
  await menuRequest(
    configuration,
    accessToken,
    `/${menuId}/sections/${sectionId}/items/${itemId}/quick-availability`,
    { method: "PUT", body: JSON.stringify({ isAvailable }) }
  );
}

async function posRequest(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  provider: PosProvider,
  path: string,
  init?: RequestInit
) {
  const response = await venueFetch(`${configuration.apiBaseUrl}/api/back-office/pos/${provider}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      "X-Vennusign-Back-Office-Token": accessToken,
      ...init?.headers
    }
  });
  if (!response.ok) throw new BackOfficeApiError(response.status, `The ${provider} operation could not be completed.`);
  return response;
}

export async function loadPosProviderStatus(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  provider: PosProvider
): Promise<PosProviderStatus> {
  const payload = await (await posRequest(configuration, accessToken, provider, "/status")).json() as Record<string, unknown> | null;
  if (!payload) return { provider };
  const connection = ("connection" in payload ? payload.connection : payload) as PosProviderStatus["connection"] | undefined;
  return {
    provider,
    connection: connection ?? undefined,
    guidance: typeof payload.guidance === "string" ? payload.guidance : undefined,
    externalActionRequired: payload.externalActionRequired === true
  };
}

export async function beginPosConnection(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  provider: "square" | "clover"
): Promise<string> {
  const payload = await (await posRequest(configuration, accessToken, provider, "/connect", { method: "POST" })).json() as { authorizationUrl?: string };
  const url = new URL(payload.authorizationUrl ?? "");
  if (url.protocol !== "https:") throw new BackOfficeApiError(502, "The provider returned an invalid authorization URL.");
  return url.href;
}

export async function importPosCatalog(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  provider: PosProvider
): Promise<{ sectionsCreated: number; itemsCreated: number; itemsUpdated: number }> {
  const result = await (await posRequest(configuration, accessToken, provider, "/catalog/import", { method: "POST" })).json() as { categoriesCreated: number; itemsCreated: number; itemsUpdated: number };
  return { sectionsCreated: result.categoriesCreated, itemsCreated: result.itemsCreated, itemsUpdated: result.itemsUpdated };
}

export async function disconnectPosProvider(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  provider: "square" | "clover"
): Promise<void> {
  await posRequest(configuration, accessToken, provider, "/connection", { method: "DELETE" });
}

async function venueOperationRequest(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  venueId: string,
  area: string,
  path = "",
  init?: RequestInit
) {
  const response = await venueFetch(
    `${configuration.apiBaseUrl}/api/back-office/venues/${venueId}/${area}${path}`,
    {
      ...init,
      headers: {
        "Content-Type": "application/json",
        "X-Vennusign-Back-Office-Token": accessToken,
        ...init?.headers
      }
    }
  );
  if (!response.ok) {
    throw new BackOfficeApiError(
      response.status,
      "Unable to manage this venue operation.",
      await readCapabilityDenial(response));
  }
  return response;
}

const screenRequest = (
  configuration: BackOfficeConfiguration,
  accessToken: string,
  venueId: string,
  path = "",
  init?: RequestInit
) => venueOperationRequest(configuration, accessToken, venueId, "screens", path, init);

export async function loadManagedScreens(configuration: BackOfficeConfiguration, accessToken: string, venueId: string): Promise<ManagedScreen[]> {
  return (await screenRequest(configuration, accessToken, venueId)).json() as Promise<ManagedScreen[]>;
}
export async function createManagedScreen(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, request: ManagedScreenWrite): Promise<ManagedScreen> {
  return (await screenRequest(configuration, accessToken, venueId, "", { method: "POST", body: JSON.stringify(request) })).json() as Promise<ManagedScreen>;
}
export async function updateManagedScreen(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, screenId: string, request: ManagedScreenWrite): Promise<ManagedScreen> {
  return (await screenRequest(configuration, accessToken, venueId, `/${screenId}`, { method: "PUT", body: JSON.stringify(request) })).json() as Promise<ManagedScreen>;
}
export async function pushManagedScreen(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, screenId: string): Promise<void> {
  await screenRequest(configuration, accessToken, venueId, `/${screenId}/push`, { method: "POST" });
}
export async function pushAllManagedScreens(configuration: BackOfficeConfiguration, accessToken: string, venueId: string): Promise<{ screenCount: number }> {
  return (await screenRequest(configuration, accessToken, venueId, "/push-all", { method: "POST" })).json() as Promise<{ screenCount: number }>;
}
export async function setManagedScreenArchived(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, screenId: string, archived: boolean): Promise<ManagedScreen> {
  return (await screenRequest(configuration, accessToken, venueId, `/${screenId}/lifecycle`, { method: "PUT", body: JSON.stringify({ archived }) })).json() as Promise<ManagedScreen>;
}
export async function resetManagedScreen(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, screenId: string): Promise<ManagedScreen> {
  return (await screenRequest(configuration, accessToken, venueId, `/${screenId}/reset`, { method: "POST" })).json() as Promise<ManagedScreen>;
}
export async function unpairManagedScreen(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, screenId: string): Promise<void> {
  await screenRequest(configuration, accessToken, venueId, `/${screenId}/pairing`, { method: "DELETE" });
}
export async function loadScreenOverflow(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, capacity: number): Promise<ScreenOverflowPreview> {
  return (await screenRequest(configuration, accessToken, venueId, `/overflow?capacity=${capacity}`)).json() as Promise<ScreenOverflowPreview>;
}
export async function loadVideoWalls(configuration: BackOfficeConfiguration, accessToken: string, venueId: string): Promise<VideoWallSnapshot> {
  return (await screenRequest(configuration, accessToken, venueId, "/video-walls")).json() as Promise<VideoWallSnapshot>;
}
export async function saveVideoWall(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, request: { name: string; layout: string; screenIds: string[] }): Promise<VideoWallGroup> {
  return (await screenRequest(configuration, accessToken, venueId, "/video-walls", { method: "PUT", body: JSON.stringify(request) })).json() as Promise<VideoWallGroup>;
}
export async function removeVideoWall(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, name: string): Promise<void> {
  await screenRequest(configuration, accessToken, venueId, `/video-walls/${encodeURIComponent(name)}`, { method: "DELETE" });
}
export async function claimPairingCode(configuration: BackOfficeConfiguration, accessToken: string, _venueId: string, code: string): Promise<{ linked: boolean; screenId: string; venueId: string }> {
  const response = await venueFetch(`${configuration.apiBaseUrl}/api/back-office/screens/pairing/${encodeURIComponent(code)}/claim`, {
    method: "POST",
    headers: { "Content-Type": "application/json", "X-Vennusign-Back-Office-Token": accessToken }
  });
  if (!response.ok) throw new BackOfficeApiError(response.status, "Unable to pair this screen.");
  return response.json() as Promise<{ linked: boolean; screenId: string; venueId: string }>;
}

export type ScreenReplacementResult = {
  status: string;
  targetScreenId?: string;
  sourceScreenId?: string;
  targetName?: string;
  replacementPlatform?: string;
  replacementAppVersion?: string;
  wallGroup?: string;
  wallPosition?: number;
  preservesConfiguration: boolean;
  preservesHistory: boolean;
  preservesVideoWall: boolean;
  targetUpdatedUtc?: string;
  completedUtc?: string;
};

export async function previewScreenReplacement(configuration: BackOfficeConfiguration, accessToken: string, targetScreenId: string, pairingCode: string): Promise<ScreenReplacementResult> {
  const response = await venueFetch(`${configuration.apiBaseUrl}/api/back-office/screens/pairing/replacement/preview`, {
    method: "POST",
    headers: { "Content-Type": "application/json", "X-Vennusign-Back-Office-Token": accessToken },
    body: JSON.stringify({ targetScreenId, pairingCode, confirmed: false })
  });
  if (!response.ok) throw new BackOfficeApiError(response.status, "Unable to preview this replacement.");
  return response.json() as Promise<ScreenReplacementResult>;
}

export async function completeScreenReplacement(configuration: BackOfficeConfiguration, accessToken: string, targetScreenId: string, pairingCode: string, expectedTargetUpdatedUtc: string): Promise<ScreenReplacementResult> {
  const response = await venueFetch(`${configuration.apiBaseUrl}/api/back-office/screens/pairing/replacement`, {
    method: "POST",
    headers: { "Content-Type": "application/json", "X-Vennusign-Back-Office-Token": accessToken },
    body: JSON.stringify({ targetScreenId, pairingCode, confirmed: true, expectedTargetUpdatedUtc })
  });
  if (!response.ok) throw new BackOfficeApiError(response.status, "Unable to complete this replacement.");
  return response.json() as Promise<ScreenReplacementResult>;
}

const themeRequest = (configuration: BackOfficeConfiguration, accessToken: string, venueId: string, path = "", init?: RequestInit) =>
  venueOperationRequest(configuration, accessToken, venueId, "theme", path, init);
export async function loadVenueTheme(configuration: BackOfficeConfiguration, accessToken: string, venueId: string): Promise<VenueTheme> {
  return (await themeRequest(configuration, accessToken, venueId)).json() as Promise<VenueTheme>;
}
export async function loadVenueThemePresets(configuration: BackOfficeConfiguration, accessToken: string, venueId: string): Promise<VenueThemePreset[]> {
  return (await themeRequest(configuration, accessToken, venueId, "/presets")).json() as Promise<VenueThemePreset[]>;
}
export async function saveVenueTheme(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, theme: Pick<VenueTheme, "backgroundColor" | "accentColor" | "fontFamily">): Promise<VenueTheme> {
  return (await themeRequest(configuration, accessToken, venueId, "", { method: "PUT", body: JSON.stringify(theme) })).json() as Promise<VenueTheme>;
}
export async function saveAdvancedVenueTheme(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, theme: Pick<VenueTheme, "titleColor" | "glowColor" | "boardBackgroundColor" | "sectionColors" | "glowIntensity" | "titleFont" | "itemFont">): Promise<VenueTheme> {
  return (await themeRequest(configuration, accessToken, venueId, "/advanced", { method: "PUT", body: JSON.stringify(theme) })).json() as Promise<VenueTheme>;
}
export async function applyVenueThemePreset(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, presetKey: string): Promise<VenueTheme> {
  return (await themeRequest(configuration, accessToken, venueId, `/presets/${encodeURIComponent(presetKey)}`, { method: "PUT" })).json() as Promise<VenueTheme>;
}
export async function resetVenueTheme(configuration: BackOfficeConfiguration, accessToken: string, venueId: string): Promise<VenueTheme> {
  return (await themeRequest(configuration, accessToken, venueId, "", { method: "DELETE" })).json() as Promise<VenueTheme>;
}

const areaRequest = (area: string, configuration: BackOfficeConfiguration, accessToken: string, venueId: string, path = "", init?: RequestInit) =>
  venueOperationRequest(configuration, accessToken, venueId, area, path, init);
export async function loadMealPeriods(configuration: BackOfficeConfiguration, accessToken: string, venueId: string): Promise<MealPeriodSnapshot> {
  return (await areaRequest("meal-periods", configuration, accessToken, venueId)).json() as Promise<MealPeriodSnapshot>;
}
export async function createMealPeriod(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, value: MealPeriodWrite): Promise<MealPeriod> {
  return (await areaRequest("meal-periods", configuration, accessToken, venueId, "", { method: "POST", body: JSON.stringify(value) })).json() as Promise<MealPeriod>;
}
export async function updateMealPeriod(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, value: MealPeriod): Promise<MealPeriod> {
  return (await areaRequest("meal-periods", configuration, accessToken, venueId, `/${value.id}`, { method: "PUT", body: JSON.stringify(value) })).json() as Promise<MealPeriod>;
}
export async function reorderMealPeriods(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, orderedIds: string[]): Promise<MealPeriod[]> {
  return (await areaRequest("meal-periods", configuration, accessToken, venueId, "/order", { method: "PUT", body: JSON.stringify({ orderedIds }) })).json() as Promise<MealPeriod[]>;
}
export async function deleteMealPeriod(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, id: string): Promise<void> {
  await areaRequest("meal-periods", configuration, accessToken, venueId, `/${id}`, { method: "DELETE" });
}
export async function loadHappyHour(configuration: BackOfficeConfiguration, accessToken: string, venueId: string): Promise<HappyHourSnapshot> {
  return (await areaRequest("happy-hour", configuration, accessToken, venueId)).json() as Promise<HappyHourSnapshot>;
}
export async function saveHappyHour(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, value: HappyHourWrite): Promise<HappyHourSnapshot> {
  return (await areaRequest("happy-hour", configuration, accessToken, venueId, "", { method: "PUT", body: JSON.stringify(value) })).json() as Promise<HappyHourSnapshot>;
}
export async function loadPlaylist(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, screenId: string): Promise<PlaylistSlide[]> {
  return (await areaRequest(`screens/${screenId}/playlist`, configuration, accessToken, venueId)).json() as Promise<PlaylistSlide[]>;
}
export async function createPlaylistSlide(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, screenId: string, value: PlaylistSlideWrite): Promise<PlaylistSlide> {
  return (await areaRequest(`screens/${screenId}/playlist`, configuration, accessToken, venueId, "", { method: "POST", body: JSON.stringify(value) })).json() as Promise<PlaylistSlide>;
}
export async function updatePlaylistSlide(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, screenId: string, slideId: string, value: PlaylistSlideWrite): Promise<PlaylistSlide> {
  return (await areaRequest(`screens/${screenId}/playlist`, configuration, accessToken, venueId, `/${slideId}`, { method: "PUT", body: JSON.stringify(value) })).json() as Promise<PlaylistSlide>;
}
export async function reorderPlaylist(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, screenId: string, orderedIds: string[]): Promise<PlaylistSlide[]> {
  return (await areaRequest(`screens/${screenId}/playlist`, configuration, accessToken, venueId, "/order", { method: "PUT", body: JSON.stringify({ orderedIds }) })).json() as Promise<PlaylistSlide[]>;
}
export async function deletePlaylistSlide(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, screenId: string, slideId: string): Promise<void> {
  await areaRequest(`screens/${screenId}/playlist`, configuration, accessToken, venueId, `/${slideId}`, { method: "DELETE" });
}
export async function loadEmergencyBroadcasts(configuration: BackOfficeConfiguration, accessToken: string, venueId: string): Promise<EmergencyBroadcast[]> {
  return (await areaRequest("emergency-broadcasts", configuration, accessToken, venueId)).json() as Promise<EmergencyBroadcast[]>;
}
export async function createEmergencyBroadcast(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, value: { screenId?: string; title: string; message: string; mediaUrl?: string; durationMinutes: number }): Promise<EmergencyBroadcast> {
  return (await areaRequest("emergency-broadcasts", configuration, accessToken, venueId, "", { method: "POST", body: JSON.stringify(value) })).json() as Promise<EmergencyBroadcast>;
}
export async function cancelEmergencyBroadcast(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, id: string): Promise<void> {
  await areaRequest("emergency-broadcasts", configuration, accessToken, venueId, `/${id}`, { method: "DELETE" });
}
export async function loadDateRangePromotions(configuration: BackOfficeConfiguration, accessToken: string, venueId: string): Promise<DateRangePromotion[]> {
  return (await areaRequest("date-range-promotions", configuration, accessToken, venueId)).json() as Promise<DateRangePromotion[]>;
}
export async function saveDateRangePromotion(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, value: Omit<DateRangePromotion, "id" | "venueId">, id?: string): Promise<DateRangePromotion> {
  return (await areaRequest("date-range-promotions", configuration, accessToken, venueId, id ? `/${id}` : "", { method: id ? "PUT" : "POST", body: JSON.stringify(value) })).json() as Promise<DateRangePromotion>;
}
export async function archiveDateRangePromotion(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, id: string): Promise<void> {
  await areaRequest("date-range-promotions", configuration, accessToken, venueId, `/${id}`, { method: "DELETE" });
}
export async function loadTapList(configuration: BackOfficeConfiguration, accessToken: string, venueId: string): Promise<TapListSnapshot> {
  return (await areaRequest("tap-list", configuration, accessToken, venueId)).json() as Promise<TapListSnapshot>;
}
export async function saveTapCategory(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, value: Omit<TapCategory, "id" | "venueId" | "sortOrder">, id?: string): Promise<TapCategory> {
  return (await areaRequest("tap-list", configuration, accessToken, venueId, `/categories${id ? `/${id}` : ""}`, { method: id ? "PUT" : "POST", body: JSON.stringify(value) })).json() as Promise<TapCategory>;
}
export async function deleteTapCategory(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, id: string): Promise<void> {
  await areaRequest("tap-list", configuration, accessToken, venueId, `/categories/${id}`, { method: "DELETE" });
}
export async function saveTapItem(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, value: Omit<TapItem, "id" | "venueId" | "sortOrder">, id?: string): Promise<TapItem> {
  return (await areaRequest("tap-list", configuration, accessToken, venueId, `/items${id ? `/${id}` : ""}`, { method: id ? "PUT" : "POST", body: JSON.stringify(value) })).json() as Promise<TapItem>;
}
export async function deleteTapItem(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, id: string): Promise<void> {
  await areaRequest("tap-list", configuration, accessToken, venueId, `/items/${id}`, { method: "DELETE" });
}
export async function reorderTapRows(configuration: BackOfficeConfiguration, accessToken: string, venueId: string, kind: "categories" | "items", ids: string[]): Promise<void> {
  await areaRequest("tap-list", configuration, accessToken, venueId, `/${kind}/order`, { method: "PUT", body: JSON.stringify({ ids }) });
}

// ---------------------------------------------------------------------------
// Menu content: the shelf, the boards it draws, and the named card actions.
//
// The API calls it content, not menus: items, placements and availability are
// content, and "menu" is the operational context using it. The capability ids
// have always said so - content.item.update, content.menu.manage.
// ---------------------------------------------------------------------------

/** A board as the render engine consumes it. Prices are strings, exactly as typed. */
export type BoardResponse = {
  menuId: string;
  name: string | null;
  /** The menu theme attached, or null when none is — a valid, rendered state. */
  theme: string | null;
  dwellSeconds: number;
  loopWarningSeconds: number;
  pages: Array<{ pageId: string; name: string; sortOrder: number }>;
  sections: Array<{
    sectionId: string;
    pageId: string;
    name: string | null;
    sortOrder: number;
    items: Array<{
      itemId: string;
      name: string | null;
      description: string | null;
      price: string | null;
      sortOrder: number;
    }>;
  }>;
};

/**
 * One card on the Menus home shelf.
 *
 * `board` is what this menu's screens are showing, and is null when it has never
 * been published — a state the shelf draws rather than an error. `screenIds` is
 * published truth, never the working assignments.
 */
export type ShelfMenu = {
  menuId: string;
  name: string;
  theme: string | null;
  isPutAway: boolean;
  publishedVersion: number | null;
  lastPublishedUtc: string | null;
  lastPublishedBy: string | null;
  draftCount: number;
  screenIds: string[];
  board: BoardResponse | null;
};

export type MenuAvailability = {
  itemId: string;
  isAvailable: boolean;
  changedUtc: string;
  changedBy: string | null;
};

export type MenuHistoryEntry = {
  kind: string;
  occurredUtc: string;
  author: string | null;
  detail: string | null;
  replacedByVersion: number | null;
  /** The publish this entry names; null for the kinds that are not a publish. */
  version: number | null;
};

export type MenuScreenShowing = {
  screenId: string;
  screenName: string;
  location: string | null;
  status: string;
  lastSeenUtc: string | null;
  widthPixels: number;
  heightPixels: number;
  menuId: string | null;
  menuName: string | null;
  version: number | null;
  publishedUtc: string | null;
  publishedBy: string | null;
};

/**
 * A refusal the API named rather than a failure.
 *
 * These reasons are a contract: the server refuses in plain words and the UI
 * repeats them. Inventing a friendlier sentence here would be a second source of
 * truth about why something was refused, and the two would drift.
 */
export class MenuActionRefused extends Error {
  constructor(
    public readonly reason: string,
    message: string
  ) {
    super(message);
  }
}

async function contentRequest(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  path = "",
  init?: RequestInit
) {
  const response = await venueFetch(`${configuration.apiBaseUrl}/api/back-office/content${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      "X-Vennusign-Back-Office-Token": accessToken,
      ...init?.headers
    }
  });

  if ([400, 404, 409, 422].includes(response.status)) {
    // A named refusal, in the words the server chose.
    const body = (await response.json().catch(() => ({}))) as { reason?: string; message?: string };
    throw new MenuActionRefused(body.reason ?? "refused", body.message ?? "That is not something you can do right now.");
  }

  if (!response.ok) {
    /*
     * The server refuses in plain words - "That would be 62 menus, and this venue
     * is set up for 50. Put one away first, or ask us to raise the limit." -
     * and this used to throw that away for a generic string, so a real refusal
     * reached the person as nothing at all. The reason is a contract; the UI
     * repeats it rather than inventing a friendlier one that would drift.
     */
    const problem = (await response.json().catch(() => ({}))) as { detail?: string; title?: string; message?: string };
    throw new BackOfficeApiError(
      response.status,
      problem.detail ?? problem.message ?? problem.title ?? "Unable to manage menu content."
    );
  }
  return response;
}

/** Every menu the venue has, as the shelf draws it. One call, whatever the count. */
export async function loadShelf(
  configuration: BackOfficeConfiguration,
  accessToken: string
): Promise<ShelfMenu[]> {
  return (await contentRequest(configuration, accessToken, "/menus")).json();
}

/** What is 86'd right now. Availability lives outside the published board. */
export async function loadMenuAvailability(
  configuration: BackOfficeConfiguration,
  accessToken: string
): Promise<MenuAvailability[]> {
  return (await contentRequest(configuration, accessToken, "/availability")).json();
}

export async function loadScreensShowing(
  configuration: BackOfficeConfiguration,
  accessToken: string
): Promise<MenuScreenShowing[]> {
  return (await contentRequest(configuration, accessToken, "/screens/showing")).json();
}

export async function loadMenuHistory(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string
): Promise<MenuHistoryEntry[]> {
  return (await contentRequest(configuration, accessToken, `/menus/${menuId}/history`)).json();
}

export async function duplicateMenu(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string
): Promise<{ menuId: string; name: string; activeMenuCount: number }> {
  return (await contentRequest(configuration, accessToken, `/menus/${menuId}/duplicate`, { method: "POST" })).json();
}

/** Put a menu away, or back on the shelf. Both are deliberate, recorded acts. */
export async function setMenuPutAway(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string,
  isPutAway: boolean
): Promise<{ changed: boolean; isPutAway: boolean; activeMenuCount: number }> {
  return (
    await contentRequest(configuration, accessToken, `/menus/${menuId}/put-away`, {
      method: "PUT",
      body: JSON.stringify({ isPutAway })
    })
  ).json();
}

/**
 * Take the menu off its screens. Permanent, so it waits in the draft and reaches
 * the screens on the next publish (Q68) — this returns the draft it joined.
 */
export async function takeMenuOffScreens(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string
): Promise<{ count: number }> {
  return (await contentRequest(configuration, accessToken, `/menus/${menuId}/screens`, { method: "DELETE" })).json();
}

/** Go back to a published version. Produces a draft; never a second silent publish. */
export async function goBackToMenuVersion(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string,
  version: number
): Promise<{ count: number; replacedChangeCount: number }> {
  return (
    await contentRequest(configuration, accessToken, `/menus/${menuId}/go-back-to/${version}`, { method: "POST" })
  ).json();
}

// ---------------------------------------------------------------------------
// The builder (M3)
// ---------------------------------------------------------------------------

/**
 * A menu open in the builder.
 *
 * `board` is the WORKING state — what the menu says now, which is what the canvas
 * draws, because the canvas is the preview. `changes` is its difference from the
 * board the screens are showing, and the publish fields describe that other board.
 * All of it arrives in one response for a reason: three sentences about two boards
 * cannot be assembled from three reads without eventually describing two different
 * menus.
 */
export type BuilderBoard = {
  board: BoardResponse;
  draftCount: number;
  changes: MenuDraftChange[];
  publishedVersion: number | null;
  lastPublishedUtc: string | null;
  lastPublishedBy: string | null;
  screenIds: string[];
};

export type MenuDraftChange = {
  targetKind: string;
  targetId: string | null;
  field: string;
  beforeValue: string | null;
  afterValue: string | null;
};

/** One add-row search result, naming the boards it already sits on (Q112/Q123). */
export type LibraryItem = {
  itemId: string;
  name: string;
  description: string | null;
  price: string | null;
  isAvailable: boolean;
  boards: Array<{ menuId: string; menuName: string }>;
};

export type PlaceOutcome = {
  outcome: "placed" | "already_on_board";
  itemId: string | null;
  sectionId: string | null;
  sortOrder: number;
  itemCountOnMenu: number;
};

export type MenuPageAssignment = { screenId: string; menuId: string; pageId: string; menuName: string | null; pageName: string | null; assignedUtc: string; assignedBy: string | null };

export async function loadMenuAssignments(configuration: BackOfficeConfiguration, accessToken: string): Promise<MenuPageAssignment[]> {
  return (await contentRequest(configuration, accessToken, "/assignments")).json();
}

export async function assignMenuPage(configuration: BackOfficeConfiguration, accessToken: string, screenId: string, menuId: string, pageId: string, mode: "replace" | "rotate" = "replace"): Promise<MenuPageAssignment> {
  return (await contentRequest(configuration, accessToken, `/screens/${screenId}/menu`, { method: "PUT", body: JSON.stringify({ menuId, pageId, mode }) })).json();
}

export async function removeMenuPageAssignment(configuration: BackOfficeConfiguration, accessToken: string, screenId: string, menuId: string, pageId: string): Promise<void> {
  await contentRequest(configuration, accessToken, `/screens/${screenId}/menus/${menuId}/pages/${pageId}`, { method: "DELETE" });
}

export type MenuPage = { pageId: string; name: string; sortOrder: number };

export async function loadMenuPages(configuration: BackOfficeConfiguration, accessToken: string, menuId: string): Promise<MenuPage[]> {
  return (await contentRequest(configuration, accessToken, `/menus/${menuId}/pages`)).json();
}

export async function addMenuPage(configuration: BackOfficeConfiguration, accessToken: string, menuId: string, name: string): Promise<MenuPage> {
  return (await contentRequest(configuration, accessToken, `/menus/${menuId}/pages`, { method: "POST", body: JSON.stringify({ name }) })).json();
}

export async function renameMenuPage(configuration: BackOfficeConfiguration, accessToken: string, menuId: string, pageId: string, name: string): Promise<void> {
  await contentRequest(configuration, accessToken, `/menus/${menuId}/pages/${pageId}`, { method: "PUT", body: JSON.stringify({ name }) });
}

export async function reorderMenuPages(configuration: BackOfficeConfiguration, accessToken: string, menuId: string, pageIds: string[]): Promise<void> {
  await contentRequest(configuration, accessToken, `/menus/${menuId}/pages/order`, {
    method: "PUT",
    body: JSON.stringify({ pageIds })
  });
}

export async function duplicateMenuPage(configuration: BackOfficeConfiguration, accessToken: string, menuId: string, pageId: string): Promise<MenuPage> {
  return (await contentRequest(configuration, accessToken, `/menus/${menuId}/pages/${pageId}/duplicate`, { method: "POST" })).json();
}

export async function deleteMenuPage(configuration: BackOfficeConfiguration, accessToken: string, menuId: string, pageId: string, moveSectionsToPageId?: string, deleteSections = false): Promise<void> {
  await contentRequest(configuration, accessToken, `/menus/${menuId}/pages/${pageId}`, { method: "DELETE", body: JSON.stringify({ moveSectionsToPageId: moveSectionsToPageId ?? null, deleteSections }) });
}

export async function saveMenuPageAssignments(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string,
  changes: Array<{ screenId: string; pageId: string; mode: "remove" | "replace" | "rotate" }>
): Promise<void> {
  await contentRequest(configuration, accessToken, `/menus/${menuId}/screens`, {
    method: "PUT",
    body: JSON.stringify({ changes })
  });
}

export async function loadBuilderBoard(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string
): Promise<BuilderBoard> {
  return (await contentRequest(configuration, accessToken, `/menus/${menuId}/board`)).json();
}

export async function addMenuSection(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string,
  name: string,
  pageId?: string | null
): Promise<{ sectionId: string; name: string; sortOrder: number }> {
  return (
    await contentRequest(configuration, accessToken, `/menus/${menuId}/sections`, {
      method: "POST",
      body: JSON.stringify({ name, pageId: pageId ?? null })
    })
  ).json();
}

export async function renameMenuSection(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string,
  sectionId: string,
  name: string
): Promise<void> {
  await contentRequest(configuration, accessToken, `/menus/${menuId}/sections/${sectionId}`, {
    method: "PUT",
    body: JSON.stringify({ name })
  });
}

/** Deleting a section atomically moves its placements or releases them to the library. */
export async function deleteMenuSection(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string,
  sectionId: string,
  moveItemsToSectionId?: string,
  deletePlacements = false
): Promise<{ movedItemCount: number; releasedItemCount: number }> {
  return (
    await contentRequest(configuration, accessToken, `/menus/${menuId}/sections/${sectionId}`, {
      method: "DELETE",
      body: JSON.stringify({ moveItemsToSectionId: moveItemsToSectionId ?? null, deletePlacements })
    })
  ).json();
}

/**
 * Reorder refuses whole when the list no longer matches — someone else added or
 * removed something mid-drag. It arrives here as a `MenuActionRefused` with reason
 * `order_stale`, in the server's words.
 */
export async function reorderMenuSections(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string,
  sectionIds: string[]
): Promise<void> {
  await contentRequest(configuration, accessToken, `/menus/${menuId}/sections/order`, {
    method: "PUT",
    body: JSON.stringify({ sectionIds })
  });
}

/** @see reorderMenuSections — same refusal, same reason. */
export async function reorderMenuItems(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string,
  sectionId: string,
  itemIds: string[]
): Promise<void> {
  await contentRequest(configuration, accessToken, `/menus/${menuId}/sections/${sectionId}/items/order`, {
    method: "PUT",
    body: JSON.stringify({ itemIds })
  });
}

/**
 * Put something in a section: an item the library already holds, or a new one born
 * with the typed name. `already_on_board` is not a failure — it carries the section
 * the item sits in so the caller jumps there instead of duplicating it (Q112).
 */
export async function placeMenuItem(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string,
  sectionId: string,
  request: { itemId?: string; name?: string }
): Promise<PlaceOutcome> {
  return (
    await contentRequest(configuration, accessToken, `/menus/${menuId}/sections/${sectionId}/items`, {
      method: "POST",
      body: JSON.stringify(request)
    })
  ).json();
}

/** Takes an item off this board. It stays in the library (Q97). */
export async function removeMenuItem(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string,
  itemId: string
): Promise<void> {
  await contentRequest(configuration, accessToken, `/menus/${menuId}/items/${itemId}`, { method: "DELETE" });
}

/**
 * Edits an item. One item is one shared price across every board it sits on (Q5);
 * each of those boards still changes its own screens only when it publishes.
 */
/**
 * Edits an item. `expected` makes it conditional: the values the caller believes
 * are still there, checked under the lock that writes.
 *
 * Undo sends it, a plain edit does not. Undoing means "put back what I changed" —
 * and if what you changed is no longer what is there, somebody else has edited it
 * since, and putting your old value back would erase their work without a word.
 */
export async function updateMenuItemValues(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  itemId: string,
  values: { name: string; description: string | null; price: string | null },
  expected?: { name: string; description: string | null; price: string | null }
): Promise<void> {
  await contentRequest(configuration, accessToken, `/items/${itemId}`, {
    method: "PUT",
    body: JSON.stringify(
      expected
        ? {
            ...values,
            expectedName: expected.name,
            expectedDescription: expected.description,
            expectedPrice: expected.price
          }
        : values
    )
  });
}

export async function searchLibraryItems(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  query: string,
  take = 20
): Promise<LibraryItem[]> {
  const search = new URLSearchParams({ query, take: String(take) });
  return (await contentRequest(configuration, accessToken, `/items?${search}`)).json();
}

/**
 * The menu themes this venue could attach. Empty until the theme editor exists —
 * read rather than assumed, so the picker needs no change when the first one is
 * built (Q86).
 */
export async function loadMenuThemes(
  configuration: BackOfficeConfiguration,
  accessToken: string
): Promise<Array<{ key: string; name: string }>> {
  return (await contentRequest(configuration, accessToken, "/menu-themes")).json();
}

/** Publish everything waiting on this menu. Atomic server-side (Q198). */
export async function publishMenu(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string
): Promise<{
  version: number;
  changeCount: number;
  publishedUtc: string;
  author: string | null;
  targets: Array<{ screenId: string; state: string }>;
  conflictedScreenIds: string[];
}> {
  return (await contentRequest(configuration, accessToken, `/menus/${menuId}/publish`, { method: "POST" })).json();
}

/** Clears the whole queue for this menu. One confirmation, no undo (Q110). */
export async function discardMenuDraft(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string
): Promise<{ discarded: number }> {
  return (await contentRequest(configuration, accessToken, `/menus/${menuId}/draft`, { method: "DELETE" })).json();
}

/** Turn an item on or off. Commits instantly, never queues, survives a publish. */
export async function setItemAvailability(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  itemId: string,
  isAvailable: boolean
): Promise<{ itemId: string; name: string; isAvailable: boolean; changedUtc: string; changedBy: string | null; screenIds: string[] }> {
  return (
    await contentRequest(configuration, accessToken, `/items/${itemId}/availability`, {
      method: "PUT",
      body: JSON.stringify({ isAvailable })
    })
  ).json();
}

/**
 * The venue's timezone and its configured ceilings. Every Menus surface renders
 * times in the venue's local time (Q196), so the zone is read rather than taken
 * from whichever browser happens to be looking.
 */
export async function loadMenuContext(
  configuration: BackOfficeConfiguration,
  accessToken: string
): Promise<{ timezone: string; ceilings: Record<string, number>; menuCount: number }> {
  return (await contentRequest(configuration, accessToken, "/context")).json();
}
