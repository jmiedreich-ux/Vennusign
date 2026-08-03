export const PAIRING_POLL_INTERVAL_MS: number;
export const PAIRING_SCREEN_STORAGE_KEY: string;
export function registerPairingScreen(baseUrl: string, platform?: string, appVersion?: string, fetchImpl?: typeof fetch): Promise<{ screenId: string; screenKey: string }>;
export function createPairingCode(baseUrl: string, screenId: string, fetchImpl?: typeof fetch): Promise<{ code: string; screenId: string; expiresAt: string }>;
export function preparePairingScreen(baseUrl: string, screenId: string, platform?: string, appVersion?: string, fetchImpl?: typeof fetch): Promise<{ code: string; screenId: string; expiresAt: string }>;
export function loadPairingStatus(baseUrl: string, code: string, fetchImpl?: typeof fetch): Promise<{ linked: boolean; screenId?: string }>;
export function displayPath(screenId: string): string;
