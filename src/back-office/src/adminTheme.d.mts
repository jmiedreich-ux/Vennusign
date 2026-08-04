export type AdminTheme = "sky" | "midnight";
export const adminThemeStorageKey: string;
export const adminThemes: readonly AdminTheme[];
export function normalizeAdminTheme(value: unknown): AdminTheme;
export function readAdminTheme(storage?: Pick<Storage, "getItem">): AdminTheme;
export function applyAdminTheme(theme: unknown, root?: HTMLElement, storage?: Pick<Storage, "setItem">): AdminTheme;
export function initializeAdminTheme(root?: HTMLElement, storage?: Pick<Storage, "getItem" | "setItem">): AdminTheme;
