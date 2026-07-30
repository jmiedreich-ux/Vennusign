export const displayMediaServiceWorkerPath: string;

export function registerDisplayMediaCache(
  serviceWorkerContainer?: Pick<ServiceWorkerContainer, 'register'>
): Promise<ServiceWorkerRegistration | null>;
