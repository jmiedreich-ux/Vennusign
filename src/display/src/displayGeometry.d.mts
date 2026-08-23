export type DeviceGeometry = {
  viewport: { width: number; height: number };
  screen: { width: number; height: number } | null;
  devicePixelRatio: number;
  orientation: string | null;
};

export function readDeviceGeometry(win?: Window): DeviceGeometry | null;

export type BoardFit =
  | { measured: false }
  | { measured: true; containerHeight: number; viewportHeight: number; overflowPixels: number; fits: boolean };

export function describeBoardFit(containerHeight: number | undefined, viewportHeight: number | undefined): BoardFit;

export const layoutThemeFieldCoverage: Readonly<Record<string, readonly string[]>>;

export type ThemeCoverage =
  | { layoutKey: string; known: false; themeFieldsServed: number }
  | {
      layoutKey: string;
      known: true;
      themeFieldsServed: number;
      themeFieldsConsumed: number;
      consumedFields: readonly string[];
      ignoredFields: string[];
    };

export function describeThemeCoverage(layoutKey: string, theme: Record<string, unknown> | null | undefined): ThemeCoverage;
