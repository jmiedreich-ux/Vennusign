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

/**
 * A request that never comes back is worse than one that fails.
 *
 * Without this a hung request never settles, so its caller's `finally` never runs:
 * the builder kept `busy` true and `saveState` at "saving" and refused every
 * control, with no error shown and nothing queued to retry. Seen on dev with the
 * B1 plan at 97% CPU - a section drag, then ten minutes of dead surface. A timeout
 * turns that into an ordinary failure, which the write queue already knows how to
 * hold and retry with backoff, so nothing is lost by being impatient here.
 *
 * Long enough that a cold worker still answers, short enough that a person is not
 * staring at a frozen page waiting to find out.
 */
const REQUEST_TIMEOUT_MS = 45_000;

function withTimeout(signal: AbortSignal | null | undefined): AbortSignal {
  const timeout = AbortSignal.timeout(REQUEST_TIMEOUT_MS);
  if (!signal) return timeout;
  // AbortSignal.any is not everywhere yet, and a caller's own signal must still win.
  const controller = new AbortController();
  const abort = (reason?: unknown) => controller.abort(reason);
  if (signal.aborted) abort((signal as AbortSignal).reason);
  else signal.addEventListener("abort", () => abort(signal.reason), { once: true });
  if (timeout.aborted) abort(timeout.reason);
  else timeout.addEventListener("abort", () => abort(timeout.reason), { once: true });
  return controller.signal;
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
  return fetch(input, { ...init, headers, credentials: "include", signal: withTimeout(init?.signal) });
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

export async function renameMenu(configuration: BackOfficeConfiguration, accessToken: string, menuId: string, name: string): Promise<void> {
  await menuRequest(configuration, accessToken, `/${menuId}`, { method: "PUT", body: JSON.stringify({ name }) });
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
      isListed: boolean;
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
  pageId: string | null;
  pageName: string | null;
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

export type QuickUpdateBoardData = {
  timezone: string;
  menus: Array<Pick<ShelfMenu, "menuId" | "name" | "screenIds" | "board">>;
  availability: MenuAvailability[];
  screens: MenuScreenShowing[];
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

/** How many menus this venue is using, against how many it may have (#908). */
export type MenuAllowance = { used: number; limit: number | null };

/**
 * Fails soft to "no ceiling known", because saying nothing is the honest fallback: a shelf that
 * refused to draw, or one that warned about a limit it could not read, are both worse than one
 * that stays quiet and lets the server refuse later as it always did.
 */
export async function loadMenuAllowance(
  configuration: BackOfficeConfiguration,
  accessToken: string
): Promise<MenuAllowance | null> {
  try {
    const response = await contentRequest(configuration, accessToken, "/menus/allowance");
    if (!response.ok) return null;
    return (await response.json()) as MenuAllowance;
  } catch {
    return null;
  }
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

export async function loadPageHistory(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string,
  pageId: string
): Promise<MenuHistoryEntry[]> {
  return (await contentRequest(configuration, accessToken, `/menus/${menuId}/pages/${pageId}/history`)).json();
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
 * #797: relocates a section, intact with its items, to a different page of the
 * same menu. A conflict (an item already on the destination page, in a
 * different section) is reported back, not thrown - the caller decides how to
 * show it, same as `already_on_board` for a plain add-item.
 */
export async function moveMenuSectionToPage(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string,
  sectionId: string,
  destinationPageId: string
): Promise<{ conflictItemId: string | null; conflictSectionName: string | null }> {
  return (
    await contentRequest(configuration, accessToken, `/menus/${menuId}/sections/${sectionId}/page`, {
      method: "POST",
      body: JSON.stringify({ destinationPageId })
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

export async function moveMenuItem(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string,
  itemId: string,
  request: { sourceSectionId: string; destinationSectionId: string; sourceItemIds: string[]; destinationItemIds: string[] }
): Promise<void> {
  await contentRequest(configuration, accessToken, `/menus/${menuId}/items/${itemId}/placement`, {
    method: "PUT",
    body: JSON.stringify(request)
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
  request: { itemId?: string; name?: string; price?: string }
): Promise<PlaceOutcome> {
  return (
    await contentRequest(configuration, accessToken, `/menus/${menuId}/sections/${sectionId}/items`, {
      method: "POST",
      body: JSON.stringify(request)
    })
  ).json();
}

/** Takes an item off one page. It stays in the library and on other pages. */
export async function removeMenuItem(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string,
  pageId: string,
  itemId: string
): Promise<void> {
  await contentRequest(configuration, accessToken, `/menus/${menuId}/pages/${pageId}/items/${itemId}`, { method: "DELETE" });
}

export async function transitionMenuItemPlacement(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string,
  pageId: string,
  itemId: string,
  request: { sectionId: string; expectedItemIds: string[]; desiredItemIds: string[] }
): Promise<void> {
  await contentRequest(configuration, accessToken, `/menus/${menuId}/pages/${pageId}/items/${itemId}/transition`, {
    method: "PUT",
    body: JSON.stringify(request)
  });
}

/**
 * Edits an item in the open menu. Imported placement-price overrides stay
 * menu-scoped; ordinary library prices remain shared until each board publishes.
 */
/**
 * Edits an item. `expected` makes it conditional: the values the caller believes
 * are still there, checked under the lock that writes.
 *
 * Undo sends it, a plain edit does not. Undoing means "put back what I changed" —
 * and if what you changed is no longer what is there, somebody else has edited it
 * since, and putting your old value back would erase their work without a word.
 *
 * `sectionId` addresses the placement rather than the menu. Price belongs to the
 * placement (A19), and one dish may sit in two sections of one menu at two prices;
 * without the section the server cannot tell which one this edit is about, and
 * refuses rather than picking.
 *
 * `priceScope` is A20 — the operator's answer when a price change could have meant one menu or
 * all of them. "everywhere" is only ever sent because they said so; omitting it means this
 * placement, so a caller that never asks cannot accidentally change every menu.
 */
export type PriceScope = "placement" | "everywhere";

export async function updateMenuItemValues(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  menuId: string,
  itemId: string,
  values: { name: string; description: string | null; price: string | null; isListed: boolean },
  expected?: { name: string; description: string | null; price: string | null; isListed: boolean },
  sectionId?: string,
  priceScope: PriceScope = "placement"
): Promise<void> {
  const where = `menuId=${encodeURIComponent(menuId)}`
    + (sectionId ? `&sectionId=${encodeURIComponent(sectionId)}` : "")
    + (priceScope === "everywhere" ? "&priceScope=everywhere" : "");
  await contentRequest(configuration, accessToken, `/items/${itemId}?${where}`, {
    method: "PUT",
    body: JSON.stringify(
      expected
        ? {
            ...values,
            expectedName: expected.name,
            expectedDescription: expected.description,
            expectedPrice: expected.price,
            expectedIsListed: expected.isListed
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

export async function loadQuickUpdateBoard(
  configuration: BackOfficeConfiguration,
  accessToken: string
): Promise<QuickUpdateBoardData> {
  return (await contentRequest(configuration, accessToken, "/quick-update")).json();
}

export async function restoreAllItemAvailability(
  configuration: BackOfficeConfiguration,
  accessToken: string
): Promise<{ count: number; screenIds: string[] }> {
  return (await contentRequest(configuration, accessToken, "/availability/restore-all", { method: "POST" })).json();
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

/**
 * `onMenus` and `itemCreatedUtc` are A21: what tells two identical-looking candidates apart. Present
 * only where a question offers more than one, so `undefined` means "nobody looked", while an empty
 * array means "on no menu" — which is itself the distinguishing fact.
 */
export type MenuImportCandidate = {
  itemId: string; displayName: string; displayPrice: string | null; matchRule: string; isSafe: boolean;
  onMenus?: string[] | null; itemCreatedUtc?: string | null;
};
export type MenuImportAnswer = { fingerprint: string; choice: string; selectedItemId: string | null; parseRevision: number; answeredUtc: string; answeredBy: string | null };
export type MenuImportQuestion = {
  questionKey: string; fingerprint: string; kind: "identity" | "unreadable"; displayOrder: number; required: boolean;
  lineNumbers: number[]; candidates: MenuImportCandidate[]; answer: MenuImportAnswer | null;
};
export type MenuImportLine = {
  lineNumber: number; rawText: string; disposition: "blank" | "section" | "item" | "unresolved" | "fallback" | "description";
  parsedName: string | null; parsedDescription: string | null; parsedPrice: string | null; parserReason: string | null;
  /** What the residue pass thinks this line is, and why. A suggestion beside the question, never in place of it (A18). */
  suggestedVerdict: "menu_name" | "menu_description" | "section_heading" | "dish" | "leave_out" | null;
  suggestedReason: string | null;
};
export type MenuImportSession = {
  session: { id: string; rawPaste: string; parseRevision: number; status: "reviewing" | "resolved"; lineCount: number; itemCount: number; expiresUtc: string; revision: string; destination: "create" | "replace" | null; proposedMenuName: string | null; completedMenuId: string | null; completedUtc: string | null; targetMenuId: string | null; targetUpdatedUtc: string | null; completedSnapshotId: string | null; targetMenuName: string | null; targetHadPublishedVersion: boolean | null; targetWorkingItemCount: number | null; targetPublishedItemCount: number | null; targetAddedCount: number | null; targetRemovedCount: number | null; targetChangedCount: number | null; completedSnapshotRestoredUtc:string|null; proposedMenuDescription: string | null; suggestedMenuName: string | null; suggestedMenuDescription: string | null };
  lines: MenuImportLine[];
  questions: MenuImportQuestion[];
  /**
   * What replacing the chosen menu would do (M6.13). Present only once a replace target is chosen,
   * and never on a completed session — the replacement already happened, and previewing it would
   * describe a decision nobody is making any more.
   */
  replacePreview?: MenuImportReplacePreview | null;
};

export type MenuImportPriceMove = { name: string; from: string | null; to: string | null };

export type MenuImportReplacePreview = {
  arrivingCount: number;
  leavingCount: number;
  repricedCount: number;
  arriving: string[];
  leaving: string[];
  repriced: MenuImportPriceMove[];
};

export class MenuImportApiError extends Error {
  constructor(public readonly status: number, public readonly reason: string, message: string, public readonly current?: MenuImportSession) { super(message); }
}

async function menuImportRequest(configuration: BackOfficeConfiguration, accessToken: string, path: string, init?: RequestInit): Promise<MenuImportSession> {
  const response = await venueFetch(`${configuration.apiBaseUrl}/api/back-office/menu-imports${path}`, {
    ...init,
    headers: { "Content-Type": "application/json", "X-Vennusign-Back-Office-Token": accessToken, ...init?.headers }
  });
  const body = await response.json().catch(() => ({})) as MenuImportSession & { reason?: string; message?: string; current?: MenuImportSession };
  if (!response.ok) throw new MenuImportApiError(response.status, body.reason ?? "unavailable", body.message ?? unnamedImportFailure(response.status), body.current ? normalizeMenuImport(body.current) : undefined);
  return normalizeMenuImport(body);
}

function normalizeMenuImport(value: MenuImportSession): MenuImportSession {
  return {
    ...value,
    session: { ...value.session, destination: value.session.destination ?? null, proposedMenuName: value.session.proposedMenuName ?? null, completedMenuId: value.session.completedMenuId ?? null, completedUtc: value.session.completedUtc ?? null, targetMenuId:value.session.targetMenuId??null,targetUpdatedUtc:value.session.targetUpdatedUtc??null,completedSnapshotId:value.session.completedSnapshotId??null,targetMenuName:value.session.targetMenuName??null,targetHadPublishedVersion:value.session.targetHadPublishedVersion??null,targetWorkingItemCount:value.session.targetWorkingItemCount??null,targetPublishedItemCount:value.session.targetPublishedItemCount??null,targetAddedCount:value.session.targetAddedCount??null,targetRemovedCount:value.session.targetRemovedCount??null,targetChangedCount:value.session.targetChangedCount??null,completedSnapshotRestoredUtc:value.session.completedSnapshotRestoredUtc??null,proposedMenuDescription:value.session.proposedMenuDescription??null,suggestedMenuName:value.session.suggestedMenuName??null,suggestedMenuDescription:value.session.suggestedMenuDescription??null },
    replacePreview: value.replacePreview ?? null,
    lines: (value.lines ?? []).map((line: MenuImportLine) => ({ ...line, suggestedVerdict: line.suggestedVerdict ?? null, suggestedReason: line.suggestedReason ?? null })),
    questions: (value.questions ?? []).map(question => ({
      ...question,
      lineNumbers: question.lineNumbers ?? [],
      candidates: question.candidates ?? [],
      answer: question.answer ?? null
    }))
  };
}

export function startMenuImport(configuration: BackOfficeConfiguration, accessToken: string, rawPaste: string) {
  return menuImportRequest(configuration, accessToken, "", { method: "POST", body: JSON.stringify({ rawPaste }) });
}

/** One unfinished import, as the shelf needs to describe it (#904). */
export type OpenMenuImport = {
  id: string;
  itemCount: number;
  lineCount: number;
  answersRemaining: number;
  createdUtc: string;
  updatedUtc: string;
  expiresUtc: string;
};

/**
 * The venue's unfinished imports.
 *
 * This is a page-load read on the shelf, so it fails soft: a shelf that will not draw because an
 * optional line about an optional import could not be fetched is worse than no line at all.
 */
export async function loadOpenMenuImports(
  configuration: BackOfficeConfiguration,
  accessToken: string
): Promise<OpenMenuImport[]> {
  try {
    const response = await venueFetch(`${configuration.apiBaseUrl}/api/back-office/menu-imports`, {
      headers: { "X-Vennusign-Back-Office-Token": accessToken }
    });
    if (!response.ok) return [];
    return (await response.json()) as OpenMenuImport[];
  } catch {
    return [];
  }
}

/** Throws one away. */
export async function discardMenuImport(
  configuration: BackOfficeConfiguration,
  accessToken: string,
  sessionId: string
): Promise<void> {
  const response = await venueFetch(`${configuration.apiBaseUrl}/api/back-office/menu-imports/${sessionId}`, {
    method: "DELETE",
    headers: { "X-Vennusign-Back-Office-Token": accessToken }
  });
  if (!response.ok && response.status !== 404) {
    throw new MenuImportApiError(response.status, "unavailable", "That import could not be discarded. Nothing changed.");
  }
}
/**
 * What to say when the server did not say anything.
 *
 * Every designed refusal on this route carries its own sentence - expired, allowance changed, not
 * ready, stale revision. A response with no message is therefore not a refusal at all: it is a
 * fault, or a request that landed while the app was restarting. "This import is unavailable" was
 * the old answer to all of them, and it named nothing, blamed nothing and told nobody what to do -
 * which is the thing decision 5 exists to forbid and the paste-import design states outright:
 * refusals name a person, a number or a clock, never "something went wrong".
 *
 * Reported by the owner against this exact screen: "does not tell me why".
 */
function unnamedImportFailure(status: number) {
  if (status === 0) return "Vennusign could not be reached. Your pasted text is still saved — check your connection and try again.";
  if (status === 404) return "This import has expired or was already finished. Its 24 hours may have run out.";
  if (status === 401 || status === 403) return "You are no longer signed in to this venue. Sign in again and the import will still be here.";
  if (status >= 500) return `Vennusign could not read this import just now (error ${status}). Your pasted text is still saved — try again in a moment.`;
  return `This import could not be opened (error ${status}).`;
}

export function loadMenuImport(configuration: BackOfficeConfiguration, accessToken: string, sessionId: string) {
  return menuImportRequest(configuration, accessToken, `/${sessionId}`);
}
export function answerMenuImport(configuration: BackOfficeConfiguration, accessToken: string, session: MenuImportSession, question: MenuImportQuestion, choice: string, selectedItemId?: string) {
  return menuImportRequest(configuration, accessToken, `/${session.session.id}/answers/${encodeURIComponent(question.questionKey)}`, {
    method: "PUT", headers: { "If-Match": `"${session.session.revision}"` }, body: JSON.stringify({ fingerprint: question.fingerprint, choice, selectedItemId })
  });
}
export function acceptSafeMenuImportMatches(configuration: BackOfficeConfiguration, accessToken: string, session: MenuImportSession) {
  return menuImportRequest(configuration, accessToken, `/${session.session.id}/accept-safe-matches`, { method: "POST", headers: { "If-Match": `"${session.session.revision}"` } });
}
export function setMenuImportLineSection(configuration: BackOfficeConfiguration, accessToken: string, session: MenuImportSession, lineNumber: number, promoted: boolean) {
  return menuImportRequest(configuration, accessToken, `/${session.session.id}/lines/${lineNumber}/${promoted ? "promote-to-section" : "section-promotion"}`, {
    method: promoted ? "POST" : "DELETE", headers: { "If-Match": `"${session.session.revision}"` }
  });
}
export function setMenuImportCreateDestination(configuration: BackOfficeConfiguration, accessToken: string, session: MenuImportSession, menuName: string) {
  return menuImportRequest(configuration, accessToken, `/${session.session.id}/destination/create`, {
    method: "PUT", headers: { "If-Match": `"${session.session.revision}"` }, body: JSON.stringify({ menuName })
  });
}
export async function confirmMenuImportCreate(configuration: BackOfficeConfiguration, accessToken: string, session: MenuImportSession) {
  const response = await venueFetch(`${configuration.apiBaseUrl}/api/back-office/menu-imports/${session.session.id}/destination/create/confirm`, {
    method: "POST", headers: { "Content-Type": "application/json", "X-Vennusign-Back-Office-Token": accessToken, "If-Match": `"${session.session.revision}"` }
  });
  const body = await response.json().catch(() => ({})) as { result?: string; menuId?: string; import?: MenuImportSession; reason?: string; message?: string; current?: MenuImportSession };
  if (!response.ok) throw new MenuImportApiError(response.status, body.reason ?? "unavailable", body.message ?? "This menu could not be created.", body.current ? normalizeMenuImport(body.current) : undefined);
  return { result: body.result!, menuId: body.menuId!, import: normalizeMenuImport(body.import!) };
}
export async function setMenuImportReplaceDestination(configuration:BackOfficeConfiguration,accessToken:string,session:MenuImportSession,menuId:string){
 const response=await venueFetch(`${configuration.apiBaseUrl}/api/back-office/menu-imports/${session.session.id}/destination/replace`,{method:"PUT",headers:{"Content-Type":"application/json","X-Vennusign-Back-Office-Token":accessToken,"If-Match":`"${session.session.revision}"`},body:JSON.stringify({menuId})});
 const body=await response.json().catch(()=>({})) as {import?:MenuImportSession;reason?:string;message?:string;current?:MenuImportSession};
 if(!response.ok)throw new MenuImportApiError(response.status,body.reason??"unavailable",body.message??"That menu could not be selected.",body.current?normalizeMenuImport(body.current):undefined);
 return normalizeMenuImport(body.import!);
}
export async function confirmMenuImportReplace(configuration:BackOfficeConfiguration,accessToken:string,session:MenuImportSession){
 const response=await venueFetch(`${configuration.apiBaseUrl}/api/back-office/menu-imports/${session.session.id}/destination/replace/confirm`,{method:"POST",headers:{"Content-Type":"application/json","X-Vennusign-Back-Office-Token":accessToken,"If-Match":`"${session.session.revision}"`}});
 const body=await response.json().catch(()=>({})) as {result?:string;menuId?:string;import?:MenuImportSession;reason?:string;message?:string;current?:MenuImportSession};
 if(!response.ok)throw new MenuImportApiError(response.status,body.reason??"unavailable",body.message??"This menu could not be replaced. Nothing changed.",body.current?normalizeMenuImport(body.current):undefined);
 return {result:body.result!,menuId:body.menuId!,import:normalizeMenuImport(body.import!)};
}
export async function restoreMenuImportReplacement(configuration:BackOfficeConfiguration,accessToken:string,snapshotId:string){
 const response=await venueFetch(`${configuration.apiBaseUrl}/api/back-office/menu-imports/replacement-snapshots/${snapshotId}/restore`,{method:"POST",headers:{"Content-Type":"application/json","X-Vennusign-Back-Office-Token":accessToken}});const body=await response.json().catch(()=>({})) as {result?:string;menuId?:string;message?:string;reason?:string};if(!response.ok)throw new MenuImportApiError(response.status,body.reason??"unavailable",body.message??"The previous draft could not be restored.");return body;
}
