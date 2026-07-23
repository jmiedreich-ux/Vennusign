function trimTrailingSlash(value: string): string {
  return value.replace(/\/+$/, '');
}

const apiBaseUrl = trimTrailingSlash(import.meta.env.VITE_API_BASE_URL ?? '');
const signalRHubUrl =
  import.meta.env.VITE_SIGNALR_HUB_URL ?? `${apiBaseUrl}/hubs/vennu`;

export const displayConfig = Object.freeze({
  apiBaseUrl,
  signalRHubUrl,
});
