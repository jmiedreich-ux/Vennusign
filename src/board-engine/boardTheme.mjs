/*
 * How a board looks — a menu theme resolved into a complete set of values.
 *
 * Owner decision, Q86: menu themes and shell themes are categorically different
 * things. A menu theme is created in the theme editor and attached to a menu; the
 * shell theme is the software's own chrome. Milestone 2 ships NO named looks, so
 * nothing here knows the name of any theme. It consumes a definition.
 *
 * The whole design rests on one rule: **every token has a plain default**, and a
 * theme supplies some, all or none of them. A menu with no theme attached is then
 * not a special case at all - it is simply "none of them", and it renders plainly.
 * That is what makes an unthemed menu render badly rather than blank, without a
 * single `if (theme === null)` anywhere in the renderer.
 *
 * Two consequences worth stating:
 *
 *   - A theme built later is new VALUES, so the engine needs no change. A theme
 *     that needs a new CAPABILITY - photos, say - is an engine change by
 *     definition, and `schemaVersion` is where that becomes visible instead of
 *     silently rendering wrong.
 *   - The defaults are deliberately plain, not "Coastal". The design README's
 *     board palette is labelled as a theme's values, and no named theme exists to
 *     borrow from. Borrowing one would be inventing a fallback, which Q86
 *     explicitly forbids.
 */

/**
 * The plain board. Near-black on near-white, in a serif voice: board typography
 * is deliberately not UI typography, because a board is menu content and should
 * read as a menu rather than as an application (design README, board rendering).
 */
export const plainBoard = Object.freeze({
  "board-background": "#ffffff",
  "board-padding": "4.5rem 5rem",
  "board-font": "'Playfair Display', Georgia, 'Times New Roman', serif",

  "section-heading-font": "inherit",
  "section-heading-color": "#1a1a1a",
  "section-heading-size": "1.6rem",
  "section-heading-tracking": "0.22em",
  "section-gap": "2.75rem",

  "item-font": "inherit",
  "item-name-color": "#1a1a1a",
  "item-name-size": "2rem",
  "item-name-tracking": "0.055em",
  "item-price-color": "#1a1a1a",
  "item-description-color": "#4a4a4a",
  "item-description-size": "1.5rem",
  "item-gap": "1.15rem",

  "leader-style": "dotted",
  "leader-color": "#9a9a9a"
});

/** The CSS custom property a token is written to. One prefix, no exceptions. */
export function cssVariableName(token) {
  return `--board-${token}`;
}

/**
 * Resolves a menu theme definition into every value the board needs.
 *
 * @param {import("./boardTheme.d.mts").MenuThemeDefinition | null | undefined} theme
 *   The attached theme, or null/undefined when none is attached — a valid state.
 * @returns {Record<string, string>} Every token, always. Never a partial set, so
 *   the renderer can never emit an undefined CSS value.
 */
export function resolveBoardTheme(theme) {
  const resolved = { ...plainBoard };
  if (!theme || typeof theme !== "object") return resolved;

  assign(resolved, "board-background", theme.board?.background);
  assign(resolved, "board-padding", theme.board?.padding);
  assign(resolved, "board-font", theme.board?.font);

  assign(resolved, "section-heading-font", theme.section?.headingFont);
  assign(resolved, "section-heading-color", theme.section?.headingColor);
  assign(resolved, "section-heading-size", theme.section?.headingSize);
  assign(resolved, "section-heading-tracking", theme.section?.headingTracking);
  assign(resolved, "section-gap", theme.section?.gap);

  assign(resolved, "item-font", theme.item?.font);
  assign(resolved, "item-name-color", theme.item?.nameColor);
  assign(resolved, "item-name-size", theme.item?.nameSize);
  assign(resolved, "item-name-tracking", theme.item?.nameTracking);
  assign(resolved, "item-price-color", theme.item?.priceColor);
  assign(resolved, "item-description-color", theme.item?.descriptionColor);
  assign(resolved, "item-description-size", theme.item?.descriptionSize);
  assign(resolved, "item-gap", theme.item?.gap);

  // "none" is a real choice a theme can make, so it is honoured rather than
  // treated as absent; anything unrecognised falls back to the plain default.
  const leaderStyle = theme.leaders?.style;
  if (leaderStyle === "none" || leaderStyle === "dotted") {
    resolved["leader-style"] = leaderStyle;
  }

  assign(resolved, "leader-color", theme.leaders?.color);

  return resolved;
}

/** The resolved theme as inline CSS custom properties, ready for a style prop. */
export function boardThemeStyle(theme) {
  const resolved = resolveBoardTheme(theme);
  const style = {};
  for (const [token, value] of Object.entries(resolved)) {
    style[cssVariableName(token)] = value;
  }

  return style;
}

/**
 * True when this engine can draw everything the theme asks for. A theme written
 * against a later engine is refused rather than half-rendered: showing some of a
 * look is worse than showing the plain one, because it is wrong without saying so.
 */
export function canRenderTheme(theme) {
  if (!theme || typeof theme !== "object") return true;
  const version = theme.schemaVersion;
  return version === undefined || version === null || version <= boardThemeSchemaVersion;
}

/** What this engine understands. A theme needing more bumps it, and this engine declines. */
export const boardThemeSchemaVersion = 1;

function assign(target, token, value) {
  if (typeof value === "string" && value.length > 0) {
    target[token] = value;
  }
}
