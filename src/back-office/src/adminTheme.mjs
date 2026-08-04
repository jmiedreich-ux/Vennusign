export const adminThemeStorageKey = "vennusign.admin-theme";
export const adminThemes = Object.freeze(["sky", "midnight"]);

export function normalizeAdminTheme(value) {
  return adminThemes.includes(value) ? value : "sky";
}

export function readAdminTheme(storage = globalThis.localStorage) {
  try {
    return normalizeAdminTheme(storage?.getItem(adminThemeStorageKey));
  } catch {
    return "sky";
  }
}

export function applyAdminTheme(theme, root = globalThis.document?.documentElement, storage = globalThis.localStorage) {
  const normalized = normalizeAdminTheme(theme);
  if (root) root.dataset.skyTheme = normalized;
  try {
    storage?.setItem(adminThemeStorageKey, normalized);
  } catch {
    // A blocked storage policy must not prevent the visual preference applying.
  }
  return normalized;
}

export function initializeAdminTheme(root = globalThis.document?.documentElement, storage = globalThis.localStorage) {
  return applyAdminTheme(readAdminTheme(storage), root, storage);
}
