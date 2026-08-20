export type PlatformOperationsConfiguration = {
  apiBaseUrl: string;
  displayBaseUrl: string;
  backOfficeBaseUrl: string;
};

export function loadPlatformOperationsConfiguration(): PlatformOperationsConfiguration {
  return {
    apiBaseUrl: (import.meta.env.VITE_API_URL ?? "").replace(/\/$/, ""),
    displayBaseUrl: (import.meta.env.VITE_DISPLAY_URL ?? window.location.origin).replace(/\/$/, ""),
    backOfficeBaseUrl: (import.meta.env.VITE_BACK_OFFICE_URL ?? `${window.location.origin}/back-office/`).replace(/\/?$/, "/")
  };
}
