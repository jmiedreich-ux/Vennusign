function trimTrailingSlash(value: string): string {
  return value.replace(/\/+$/, '');
}

const apiBaseUrl = trimTrailingSlash(import.meta.env.VITE_API_BASE_URL ?? '');
const signalRHubUrl =
  import.meta.env.VITE_SIGNALR_HUB_URL ?? `${apiBaseUrl}/hubs/vennusign`;

export const displayConfig = Object.freeze({
  apiBaseUrl,
  signalRHubUrl,
  playerVersion: import.meta.env.VITE_APP_VERSION ?? '0.0.0-development'
});
