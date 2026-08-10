/**
 * A menu theme, as the theme editor will author it and the menu editor attaches
 * it. Every field is optional: what a theme does not say, the plain board says.
 * That is what lets a menu with no theme at all be an ordinary case rather than
 * a special one (Q86).
 *
 * Milestone 2 ships no named looks, so nothing is defined here yet — this is the
 * contract those later themes are written against, and supplying new values for
 * these tokens needs no engine change.
 */
export type MenuThemeDefinition = {
  /**
   * What the theme was written against. A theme needing a capability this engine
   * does not have bumps this, and the engine declines to render it rather than
   * drawing a wrong half of it.
   */
  schemaVersion?: number;
  board?: {
    background?: string;
    padding?: string;
    font?: string;
  };
  section?: {
    headingFont?: string;
    headingColor?: string;
    headingSize?: string;
    headingTracking?: string;
    gap?: string;
  };
  item?: {
    font?: string;
    nameColor?: string;
    nameSize?: string;
    nameTracking?: string;
    priceColor?: string;
    descriptionColor?: string;
    descriptionSize?: string;
    gap?: string;
  };
  leaders?: {
    style?: "dotted" | "none";
    color?: string;
  };
};

/** No theme attached is a valid, rendered state — not blank, and not a failure. */
export type BoardTheme = MenuThemeDefinition | null;

export const plainBoard: Readonly<Record<string, string>>;

export const boardThemeSchemaVersion: number;

export function cssVariableName(token: string): string;

export function resolveBoardTheme(theme: BoardTheme | undefined): Record<string, string>;

export function boardThemeStyle(theme: BoardTheme | undefined): Record<string, string>;

export function canRenderTheme(theme: BoardTheme | undefined): boolean;
