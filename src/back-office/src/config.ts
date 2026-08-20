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
    apiBaseUrl: (import.meta.env.VITE_API_URL ?? "").replace(/\/$/, ""),
    displayBaseUrl: (import.meta.env.VITE_DISPLAY_URL ?? window.location.origin).replace(/\/$/, ""),
    menuCapabilityOverrides: window.__VENNUSIGN_BACK_OFFICE_CONFIGURATION__?.menuCapabilityOverrides
  };
}
