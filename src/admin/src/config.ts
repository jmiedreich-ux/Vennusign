export type AdminConfiguration = {
  apiBaseUrl: string;
};

export function loadAdminConfiguration(): AdminConfiguration {
  return {
    apiBaseUrl: (import.meta.env.VITE_VENNU_API_BASE_URL ?? "").replace(/\/$/, "")
  };
}
