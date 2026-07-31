export type VenueAdminConfiguration = {
  apiBaseUrl: string;
};

export function loadVenueAdminConfiguration(): VenueAdminConfiguration {
  return {
    apiBaseUrl: (import.meta.env.VITE_VENNU_API_BASE_URL ?? "").replace(/\/$/, "")
  };
}
