export type AdminConfiguration = {
  apiBaseUrl: string;
  displayBaseUrl: string;
  venueAdminBaseUrl: string;
};

export function loadAdminConfiguration(): AdminConfiguration {
  return {
    apiBaseUrl: (import.meta.env.VITE_VENNU_API_BASE_URL ?? "").replace(/\/$/, ""),
    displayBaseUrl: (import.meta.env.VITE_VENNU_DISPLAY_BASE_URL ?? window.location.origin).replace(/\/$/, ""),
    venueAdminBaseUrl: (import.meta.env.VITE_VENNU_VENUE_ADMIN_BASE_URL ?? `${window.location.origin}/venue-admin/`).replace(/\/?$/, "/")
  };
}
