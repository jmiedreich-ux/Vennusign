import type { VenueAdminConfiguration } from "./config";

export type VenueAdminSession = {
  venueId: string;
  displayName: string;
  capabilities: string[];
};

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
