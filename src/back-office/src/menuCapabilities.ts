export type MenuCapability =
  | "page-management"
  | "screen-assignment"
  | "capacity"
  | "history"
  | "theme"
  | "publish"
  | "restore"
  | "import-photo"
  | "import-paste"
  | "import-spreadsheet";

export type MenuCapabilityOverrides = Partial<Record<MenuCapability, boolean>>;

/**
 * M3-A ships at maximum tier. Until tier resolution exists, an omitted decision
 * is therefore enabled; an explicit false removes only the control it guards.
 */
export function hasMenuCapability(
  capability: MenuCapability,
  overrides?: MenuCapabilityOverrides
): boolean {
  return overrides?.[capability] ?? true;
}
