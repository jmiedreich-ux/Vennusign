export type PlatformOperationsConfiguration = {
  apiBaseUrl: string;
  displayBaseUrl: string;
  backOfficeBaseUrl: string;
};

export function loadPlatformOperationsConfiguration(): PlatformOperationsConfiguration {
  return {
    apiBaseUrl: (import.meta.env.VITE_VENNUSIGN_API_BASE_URL ?? import.meta.env.VITE_VENNU_API_BASE_URL ?? "").replace(/\/$/, ""),
    displayBaseUrl: (import.meta.env.VITE_VENNUSIGN_DISPLAY_BASE_URL ?? import.meta.env.VITE_VENNU_DISPLAY_BASE_URL ?? window.location.origin).replace(/\/$/, ""),
    backOfficeBaseUrl: (import.meta.env.VITE_VENNUSIGN_BACK_OFFICE_BASE_URL ?? import.meta.env.VITE_VENNU_VENUE_ADMIN_BASE_URL ?? `${window.location.origin}/back-office/`).replace(/\/?$/, "/")
  };
}
