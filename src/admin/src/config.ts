export type AdminConfiguration = {
  apiBaseUrl: string;
  displayBaseUrl: string;
};

export function loadAdminConfiguration(): AdminConfiguration {
  return {
    apiBaseUrl: (import.meta.env.VITE_VENNU_API_BASE_URL ?? "").replace(/\/$/, ""),
    displayBaseUrl: (import.meta.env.VITE_VENNU_DISPLAY_BASE_URL ?? window.location.origin).replace(/\/$/, "")
  };
}
