export type AdminConfiguration = {
  apiBaseUrl: string;
  apiKey: string;
};

export function loadAdminConfiguration(): AdminConfiguration {
  return {
    apiBaseUrl: (import.meta.env.VITE_VENNU_API_BASE_URL ?? "").replace(/\/$/, ""),
    apiKey: import.meta.env.VITE_VENNU_ADMIN_API_KEY ?? ""
  };
}

