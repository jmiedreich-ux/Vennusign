import type { VenueAdminConfiguration } from "./config";
import { requireHostedCheckoutUrl } from "./checkoutFlow.mjs";
import { requireHostedBillingPortalUrl } from "./billingPortal.mjs";

export type VenueAdminSession = {
  venueId: string;
  displayName: string;
  capabilities: string[];
};
export type VenueAdminTierSummary = {
  id: string; name: string; slug: string; monthlyPrice: number; maxScreens: number;
};
export type VenueAdminBillingPresentation = {
  currentTier?: VenueAdminTierSummary;
  subscription?: VenueAdminSubscriptionSummary;
  availableTiers: VenueAdminTierSummary[];
  effectiveFeatures: Record<string, { enabled: boolean; limitValue?: string }>;
  haasBundles: VenueAdminHaasBundleSummary[];
  haasContract?: VenueAdminHaasContractSummary;
};
export type VenueAdminSubscriptionSummary = {
  status: "trialing" | "active" | "past_due" | "canceled";
  trialEndsAt?: string;
  currentPeriodEnd?: string;
  cancelAtPeriodEnd: boolean;
  canManageBilling: boolean;
};
export type VenueAdminHaasBundleSummary = {
  key: string; name: string; termMonths: 18 | 24 | 36;
  monthlyAmount: number; postContractTierSlug: string;
};
export type VenueAdminHaasContractSummary = {
  bundleKey: string; bundleName: string; status: "active" | "past_due" | "completed" | "canceled";
  termMonths: number; monthlyAmount: number; startedUtc: string; contractEndsUtc: string;
  remainingMonths: number; estimatedBuyoutAmount: number; cancelAtPeriodEnd: boolean; endedUtc?: string;
};
export type CheckoutBillingInterval = "monthly" | "annual";

export type MenuSection = {
  id: string; venueId: string; menuId: string; name: string;
  sortOrder: number; isActive: boolean; createdUtc: string; updatedUtc: string;
};
export type MenuItem = {
  id: string; venueId: string; menuSectionId: string; name: string;
  description?: string; price: number; happyHourPrice?: number;
  sortOrder: number; isAvailable: boolean; availabilityResetUtc?: string; quantityAvailable?: number;
  tags?: string; isPopular: boolean; createdUtc: string; updatedUtc: string;
};
export type MenuItemWrite = {
  name: string; description?: string; price: number; happyHourPrice?: number;
};
export type MenuEditorSnapshot = {
  menus: Array<{
    menu: { id: string; venueId: string; name: string; isActive: boolean; dailySpecial?: string };
    sections: MenuSection[];
  }>;
  itemGroups: Array<{ sectionId: string; items: MenuItem[] }>;
  capabilities: { happyHour: boolean; allergenBadges: boolean; quickUpdate: boolean };
};
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

export async function loadVenueBillingPresentation(
  configuration: VenueAdminConfiguration,
  accessToken: string,
  signal?: AbortSignal
): Promise<VenueAdminBillingPresentation> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/venue-admin/billing/presentation`, {
    headers: { "X-Vennu-Venue-Token": accessToken },
    signal
  });
  if (!response.ok) {
    throw new VenueAdminApiError(response.status, "Upgrade options are unavailable.");
  }
  return response.json() as Promise<VenueAdminBillingPresentation>;
}

export async function createCheckoutSession(
  configuration: VenueAdminConfiguration,
  accessToken: string,
  targetTierId: string,
  billingInterval: CheckoutBillingInterval,
  signal?: AbortSignal
): Promise<string> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/venue-admin/billing/checkout-session`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "X-Vennu-Venue-Token": accessToken
    },
    body: JSON.stringify({ targetTierId, billingInterval }),
    signal
  });
  if (!response.ok) {
    throw new VenueAdminApiError(response.status, "Secure checkout could not be opened.");
  }
  const payload = await response.json() as { checkoutUrl?: string };
  if (!payload.checkoutUrl) {
    throw new VenueAdminApiError(502, "Secure checkout returned an invalid response.");
  }
  try {
    return requireHostedCheckoutUrl(payload.checkoutUrl);
  } catch {
    throw new VenueAdminApiError(502, "Secure checkout returned an invalid response.");
  }
}

export async function createBillingPortalSession(
  configuration: VenueAdminConfiguration,
  accessToken: string,
  signal?: AbortSignal
): Promise<string> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/venue-admin/billing/portal-session`, {
    method: "POST",
    headers: { "X-Vennu-Venue-Token": accessToken },
    signal
  });
  if (!response.ok) {
    throw new VenueAdminApiError(response.status, "Secure billing management could not be opened.");
  }
  const payload = await response.json() as { portalUrl?: string };
  if (!payload.portalUrl) {
    throw new VenueAdminApiError(502, "Secure billing management returned an invalid response.");
  }
  try {
    return requireHostedBillingPortalUrl(payload.portalUrl);
  } catch {
    throw new VenueAdminApiError(502, "Secure billing management returned an invalid response.");
  }
}

export async function createHaasCheckoutSession(
  configuration: VenueAdminConfiguration,
  accessToken: string,
  bundleKey: string,
  termMonths: number,
  signal?: AbortSignal
): Promise<string> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/venue-admin/billing/haas-checkout-session`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "X-Vennu-Venue-Token": accessToken
    },
    body: JSON.stringify({ bundleKey, termMonths }),
    signal
  });
  if (!response.ok) {
    throw new VenueAdminApiError(response.status, "Hardware bundle Checkout could not be opened.");
  }
  const payload = await response.json() as { checkoutUrl?: string };
  if (!payload.checkoutUrl) {
    throw new VenueAdminApiError(502, "Hardware bundle Checkout returned an invalid response.");
  }
  try {
    return requireHostedCheckoutUrl(payload.checkoutUrl);
  } catch {
    throw new VenueAdminApiError(502, "Hardware bundle Checkout returned an invalid response.");
  }
}

async function menuRequest(
  configuration: VenueAdminConfiguration,
  accessToken: string,
  path = "",
  init?: RequestInit
) {
  const response = await fetch(`${configuration.apiBaseUrl}/api/venue-admin/menus${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      "X-Vennu-Venue-Token": accessToken,
      ...init?.headers
    }
  });
  if (!response.ok) throw new VenueAdminApiError(response.status, "Unable to manage menu content.");
  return response;
}

export async function loadMenuEditor(
  configuration: VenueAdminConfiguration,
  accessToken: string,
  _venueId: string
): Promise<MenuEditorSnapshot> {
  return (await menuRequest(configuration, accessToken)).json() as Promise<MenuEditorSnapshot>;
}

export async function createMenuSection(
  configuration: VenueAdminConfiguration,
  accessToken: string,
  _venueId: string,
  menuId: string,
  name: string
): Promise<MenuSection> {
  return (await menuRequest(configuration, accessToken, `/${menuId}/sections`, {
    method: "POST",
    body: JSON.stringify({ name })
  })).json() as Promise<MenuSection>;
}

export async function updateMenuSection(
  configuration: VenueAdminConfiguration,
  accessToken: string,
  _venueId: string,
  section: MenuSection
): Promise<MenuSection> {
  return (await menuRequest(configuration, accessToken, `/sections/${section.id}`, {
    method: "PUT",
    body: JSON.stringify({ name: section.name, isActive: section.isActive })
  })).json() as Promise<MenuSection>;
}

export async function reorderMenuSections(
  configuration: VenueAdminConfiguration,
  accessToken: string,
  _venueId: string,
  menuId: string,
  sectionIds: string[]
): Promise<void> {
  await menuRequest(configuration, accessToken, `/${menuId}/sections/order`, {
    method: "PUT",
    body: JSON.stringify({ sectionIds })
  });
}

export async function createMenuItem(
  configuration: VenueAdminConfiguration,
  accessToken: string,
  _venueId: string,
  menuId: string,
  sectionId: string,
  item: MenuItemWrite
): Promise<MenuItem> {
  return (await menuRequest(configuration, accessToken, `/${menuId}/sections/${sectionId}/items`, {
    method: "POST",
    body: JSON.stringify(item)
  })).json() as Promise<MenuItem>;
}

export async function updateMenuItem(
  configuration: VenueAdminConfiguration,
  accessToken: string,
  _venueId: string,
  menuId: string,
  sectionId: string,
  itemId: string,
  item: MenuItemWrite
): Promise<MenuItem> {
  return (await menuRequest(configuration, accessToken, `/${menuId}/sections/${sectionId}/items/${itemId}`, {
    method: "PUT",
    body: JSON.stringify(item)
  })).json() as Promise<MenuItem>;
}

export async function updateMenuItemPresentation(
  configuration: VenueAdminConfiguration,
  accessToken: string,
  _venueId: string,
  menuId: string,
  sectionId: string,
  item: MenuItem
): Promise<MenuItem> {
  return (await menuRequest(
    configuration,
    accessToken,
    `/${menuId}/sections/${sectionId}/items/${item.id}/presentation`,
    {
      method: "PUT",
      body: JSON.stringify({
        isAvailable: item.isAvailable,
        quantityAvailable: item.quantityAvailable,
        tags: item.tags?.split(",").map(tag => tag.trim()).filter(Boolean) ?? [],
        isPopular: item.isPopular
      })
    }
  )).json() as Promise<MenuItem>;
}

export async function updateQuickDailySpecial(
  configuration: VenueAdminConfiguration,
  accessToken: string,
  _venueId: string,
  menuId: string,
  dailySpecial?: string
): Promise<void> {
  await menuRequest(configuration, accessToken, `/${menuId}/quick-update/daily-special`, {
    method: "PUT",
    body: JSON.stringify({ dailySpecial })
  });
}

export async function updateQuickAvailability(
  configuration: VenueAdminConfiguration,
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

async function venueOperationRequest(
  configuration: VenueAdminConfiguration,
  accessToken: string,
  venueId: string,
  area: string,
  path = "",
  init?: RequestInit
) {
  const response = await fetch(
    `${configuration.apiBaseUrl}/api/venue-admin/venues/${venueId}/${area}${path}`,
    {
      ...init,
      headers: {
        "Content-Type": "application/json",
        "X-Vennu-Venue-Token": accessToken,
        ...init?.headers
      }
    }
  );
  if (!response.ok) {
    throw new VenueAdminApiError(response.status, "Unable to manage this venue operation.");
  }
  return response;
}

const screenRequest = (
  configuration: VenueAdminConfiguration,
  accessToken: string,
  venueId: string,
  path = "",
  init?: RequestInit
) => venueOperationRequest(configuration, accessToken, venueId, "screens", path, init);

export async function loadManagedScreens(configuration: VenueAdminConfiguration, accessToken: string, venueId: string): Promise<ManagedScreen[]> {
  return (await screenRequest(configuration, accessToken, venueId)).json() as Promise<ManagedScreen[]>;
}
export async function createManagedScreen(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, request: ManagedScreenWrite): Promise<ManagedScreen> {
  return (await screenRequest(configuration, accessToken, venueId, "", { method: "POST", body: JSON.stringify(request) })).json() as Promise<ManagedScreen>;
}
export async function updateManagedScreen(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, screenId: string, request: ManagedScreenWrite): Promise<ManagedScreen> {
  return (await screenRequest(configuration, accessToken, venueId, `/${screenId}`, { method: "PUT", body: JSON.stringify(request) })).json() as Promise<ManagedScreen>;
}
export async function pushManagedScreen(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, screenId: string): Promise<void> {
  await screenRequest(configuration, accessToken, venueId, `/${screenId}/push`, { method: "POST" });
}
export async function pushAllManagedScreens(configuration: VenueAdminConfiguration, accessToken: string, venueId: string): Promise<{ screenCount: number }> {
  return (await screenRequest(configuration, accessToken, venueId, "/push-all", { method: "POST" })).json() as Promise<{ screenCount: number }>;
}
export async function loadScreenOverflow(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, capacity: number): Promise<ScreenOverflowPreview> {
  return (await screenRequest(configuration, accessToken, venueId, `/overflow?capacity=${capacity}`)).json() as Promise<ScreenOverflowPreview>;
}
export async function loadVideoWalls(configuration: VenueAdminConfiguration, accessToken: string, venueId: string): Promise<VideoWallSnapshot> {
  return (await screenRequest(configuration, accessToken, venueId, "/video-walls")).json() as Promise<VideoWallSnapshot>;
}
export async function saveVideoWall(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, request: { name: string; layout: string; screenIds: string[] }): Promise<VideoWallGroup> {
  return (await screenRequest(configuration, accessToken, venueId, "/video-walls", { method: "PUT", body: JSON.stringify(request) })).json() as Promise<VideoWallGroup>;
}
export async function removeVideoWall(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, name: string): Promise<void> {
  await screenRequest(configuration, accessToken, venueId, `/video-walls/${encodeURIComponent(name)}`, { method: "DELETE" });
}
export async function claimPairingCode(configuration: VenueAdminConfiguration, accessToken: string, _venueId: string, code: string): Promise<{ linked: boolean; screenId: string; venueId: string }> {
  const response = await fetch(`${configuration.apiBaseUrl}/api/venue-admin/screens/pairing/${encodeURIComponent(code)}/claim`, {
    method: "POST",
    headers: { "Content-Type": "application/json", "X-Vennu-Venue-Token": accessToken }
  });
  if (!response.ok) throw new VenueAdminApiError(response.status, "Unable to pair this screen.");
  return response.json() as Promise<{ linked: boolean; screenId: string; venueId: string }>;
}

const themeRequest = (configuration: VenueAdminConfiguration, accessToken: string, venueId: string, path = "", init?: RequestInit) =>
  venueOperationRequest(configuration, accessToken, venueId, "theme", path, init);
export async function loadVenueTheme(configuration: VenueAdminConfiguration, accessToken: string, venueId: string): Promise<VenueTheme> {
  return (await themeRequest(configuration, accessToken, venueId)).json() as Promise<VenueTheme>;
}
export async function loadVenueThemePresets(configuration: VenueAdminConfiguration, accessToken: string, venueId: string): Promise<VenueThemePreset[]> {
  return (await themeRequest(configuration, accessToken, venueId, "/presets")).json() as Promise<VenueThemePreset[]>;
}
export async function saveVenueTheme(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, theme: Pick<VenueTheme, "backgroundColor" | "accentColor" | "fontFamily">): Promise<VenueTheme> {
  return (await themeRequest(configuration, accessToken, venueId, "", { method: "PUT", body: JSON.stringify(theme) })).json() as Promise<VenueTheme>;
}
export async function saveAdvancedVenueTheme(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, theme: Pick<VenueTheme, "titleColor" | "glowColor" | "boardBackgroundColor" | "sectionColors" | "glowIntensity" | "titleFont" | "itemFont">): Promise<VenueTheme> {
  return (await themeRequest(configuration, accessToken, venueId, "/advanced", { method: "PUT", body: JSON.stringify(theme) })).json() as Promise<VenueTheme>;
}
export async function applyVenueThemePreset(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, presetKey: string): Promise<VenueTheme> {
  return (await themeRequest(configuration, accessToken, venueId, `/presets/${encodeURIComponent(presetKey)}`, { method: "PUT" })).json() as Promise<VenueTheme>;
}

const areaRequest = (area: string, configuration: VenueAdminConfiguration, accessToken: string, venueId: string, path = "", init?: RequestInit) =>
  venueOperationRequest(configuration, accessToken, venueId, area, path, init);
export async function loadMealPeriods(configuration: VenueAdminConfiguration, accessToken: string, venueId: string): Promise<MealPeriodSnapshot> {
  return (await areaRequest("meal-periods", configuration, accessToken, venueId)).json() as Promise<MealPeriodSnapshot>;
}
export async function createMealPeriod(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, value: MealPeriodWrite): Promise<MealPeriod> {
  return (await areaRequest("meal-periods", configuration, accessToken, venueId, "", { method: "POST", body: JSON.stringify(value) })).json() as Promise<MealPeriod>;
}
export async function updateMealPeriod(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, value: MealPeriod): Promise<MealPeriod> {
  return (await areaRequest("meal-periods", configuration, accessToken, venueId, `/${value.id}`, { method: "PUT", body: JSON.stringify(value) })).json() as Promise<MealPeriod>;
}
export async function deleteMealPeriod(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, id: string): Promise<void> {
  await areaRequest("meal-periods", configuration, accessToken, venueId, `/${id}`, { method: "DELETE" });
}
export async function loadHappyHour(configuration: VenueAdminConfiguration, accessToken: string, venueId: string): Promise<HappyHourSnapshot> {
  return (await areaRequest("happy-hour", configuration, accessToken, venueId)).json() as Promise<HappyHourSnapshot>;
}
export async function saveHappyHour(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, value: HappyHourWrite): Promise<HappyHourSnapshot> {
  return (await areaRequest("happy-hour", configuration, accessToken, venueId, "", { method: "PUT", body: JSON.stringify(value) })).json() as Promise<HappyHourSnapshot>;
}
export async function loadPlaylist(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, screenId: string): Promise<PlaylistSlide[]> {
  return (await areaRequest(`screens/${screenId}/playlist`, configuration, accessToken, venueId)).json() as Promise<PlaylistSlide[]>;
}
export async function createPlaylistSlide(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, screenId: string, value: PlaylistSlideWrite): Promise<PlaylistSlide> {
  return (await areaRequest(`screens/${screenId}/playlist`, configuration, accessToken, venueId, "", { method: "POST", body: JSON.stringify(value) })).json() as Promise<PlaylistSlide>;
}
export async function reorderPlaylist(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, screenId: string, orderedIds: string[]): Promise<PlaylistSlide[]> {
  return (await areaRequest(`screens/${screenId}/playlist`, configuration, accessToken, venueId, "/order", { method: "PUT", body: JSON.stringify({ orderedIds }) })).json() as Promise<PlaylistSlide[]>;
}
export async function deletePlaylistSlide(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, screenId: string, slideId: string): Promise<void> {
  await areaRequest(`screens/${screenId}/playlist`, configuration, accessToken, venueId, `/${slideId}`, { method: "DELETE" });
}
export async function loadEmergencyBroadcasts(configuration: VenueAdminConfiguration, accessToken: string, venueId: string): Promise<EmergencyBroadcast[]> {
  return (await areaRequest("emergency-broadcasts", configuration, accessToken, venueId)).json() as Promise<EmergencyBroadcast[]>;
}
export async function createEmergencyBroadcast(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, value: { screenId?: string; title: string; message: string; mediaUrl?: string; durationMinutes: number }): Promise<EmergencyBroadcast> {
  return (await areaRequest("emergency-broadcasts", configuration, accessToken, venueId, "", { method: "POST", body: JSON.stringify(value) })).json() as Promise<EmergencyBroadcast>;
}
export async function cancelEmergencyBroadcast(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, id: string): Promise<void> {
  await areaRequest("emergency-broadcasts", configuration, accessToken, venueId, `/${id}`, { method: "DELETE" });
}
export async function loadDateRangePromotions(configuration: VenueAdminConfiguration, accessToken: string, venueId: string): Promise<DateRangePromotion[]> {
  return (await areaRequest("date-range-promotions", configuration, accessToken, venueId)).json() as Promise<DateRangePromotion[]>;
}
export async function saveDateRangePromotion(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, value: Omit<DateRangePromotion, "id" | "venueId">, id?: string): Promise<DateRangePromotion> {
  return (await areaRequest("date-range-promotions", configuration, accessToken, venueId, id ? `/${id}` : "", { method: id ? "PUT" : "POST", body: JSON.stringify(value) })).json() as Promise<DateRangePromotion>;
}
export async function archiveDateRangePromotion(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, id: string): Promise<void> {
  await areaRequest("date-range-promotions", configuration, accessToken, venueId, `/${id}`, { method: "DELETE" });
}
export async function loadTapList(configuration: VenueAdminConfiguration, accessToken: string, venueId: string): Promise<TapListSnapshot> {
  return (await areaRequest("tap-list", configuration, accessToken, venueId)).json() as Promise<TapListSnapshot>;
}
export async function saveTapCategory(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, value: Omit<TapCategory, "id" | "venueId" | "sortOrder">, id?: string): Promise<TapCategory> {
  return (await areaRequest("tap-list", configuration, accessToken, venueId, `/categories${id ? `/${id}` : ""}`, { method: id ? "PUT" : "POST", body: JSON.stringify(value) })).json() as Promise<TapCategory>;
}
export async function deleteTapCategory(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, id: string): Promise<void> {
  await areaRequest("tap-list", configuration, accessToken, venueId, `/categories/${id}`, { method: "DELETE" });
}
export async function saveTapItem(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, value: Omit<TapItem, "id" | "venueId" | "sortOrder">, id?: string): Promise<TapItem> {
  return (await areaRequest("tap-list", configuration, accessToken, venueId, `/items${id ? `/${id}` : ""}`, { method: id ? "PUT" : "POST", body: JSON.stringify(value) })).json() as Promise<TapItem>;
}
export async function deleteTapItem(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, id: string): Promise<void> {
  await areaRequest("tap-list", configuration, accessToken, venueId, `/items/${id}`, { method: "DELETE" });
}
export async function reorderTapRows(configuration: VenueAdminConfiguration, accessToken: string, venueId: string, kind: "categories" | "items", ids: string[]): Promise<void> {
  await areaRequest("tap-list", configuration, accessToken, venueId, `/${kind}/order`, { method: "PUT", body: JSON.stringify({ ids }) });
}
