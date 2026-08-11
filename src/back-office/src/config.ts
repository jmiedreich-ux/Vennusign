export type BackOfficeConfiguration = {
  apiBaseUrl: string;
  displayBaseUrl: string;
  menuCapabilityOverrides?: import("./menuCapabilities").MenuCapabilityOverrides;
};

declare global {
  interface Window {
    __VENNUSIGN_BACK_OFFICE_CONFIGURATION__?: Pick<BackOfficeConfiguration, "menuCapabilityOverrides">;
  }
}

export function loadBackOfficeConfiguration(): BackOfficeConfiguration {
  return {
    apiBaseUrl: (import.meta.env.VITE_VENNUSIGN_API_BASE_URL ?? import.meta.env.VITE_VENNU_API_BASE_URL ?? "").replace(/\/$/, ""),
    displayBaseUrl: (import.meta.env.VITE_VENNUSIGN_DISPLAY_BASE_URL ?? import.meta.env.VITE_VENNU_DISPLAY_BASE_URL ?? window.location.origin).replace(/\/$/, ""),
    menuCapabilityOverrides: window.__VENNUSIGN_BACK_OFFICE_CONFIGURATION__?.menuCapabilityOverrides
  };
}
