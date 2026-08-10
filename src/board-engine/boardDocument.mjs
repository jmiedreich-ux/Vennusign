/*
 * What a board renders — decided once, in one place, for every surface.
 *
 * This is a pure function of (published board, what is 86'd). No DOM, no React,
 * no clock: the same inputs always give the same output, which is what lets a
 * shelf card, the builder canvas and the TV agree with each other by
 * construction rather than by everyone remembering the same rules.
 *
 * The rules, and where each comes from:
 *
 *   - An 86'd item is not rendered at all (M1's availability model: 86 is a fact
 *     about tonight, it never waits for a publish and it never shows on a board).
 *   - A section with nothing visible left in it is not rendered. Order matters:
 *     86'ing the last item in a section empties that section, so the emptiness
 *     has to be judged after the removals, not before.
 *   - Prices render exactly as typed (Q115/Q190). "9.5" never becomes "9.50",
 *     and "MP" is a price. A missing price renders as an em dash, never as a
 *     zero and never as an empty gap.
 *   - Nothing here draws a venue-name strip (Q98). If a TV carries one, the
 *     theme editor owns it; the Menus engine neither draws one nor assumes one.
 */

/** A price that was never typed. An em dash, so the column still lines up. */
export const missingPrice = "—";

/**
 * The board as it should appear.
 *
 * @param {import("./boardDocument.d.mts").BoardInput | null | undefined} board
 *   The published board, in the shape the content API returns it.
 * @param {Iterable<string> | null | undefined} unavailableItemIds
 *   Ids of items that are 86'd right now. Availability lives outside the
 *   published snapshot on purpose - it is instant, and it survives a publish -
 *   so it arrives here separately and is applied at render time.
 */
export function buildBoardDocument(board, unavailableItemIds) {
  const off = toIdSet(unavailableItemIds);

  const sections = (board?.sections ?? [])
    .slice()
    .sort(bySortOrderThen("sectionId"))
    .map((section) => ({
      sectionId: String(section?.sectionId ?? ""),
      name: typeof section?.name === "string" ? section.name : "",
      items: (section?.items ?? [])
        .filter((item) => !off.has(String(item?.itemId ?? "")))
        .slice()
        .sort(bySortOrderThen("itemId"))
        .map((item) => ({
          itemId: String(item?.itemId ?? ""),
          name: typeof item?.name === "string" ? item.name : "",
          description: nonEmpty(item?.description),
          // Exactly as typed, or an em dash. Never reformatted, never zero.
          price: nonEmpty(item?.price) ?? missingPrice
        }))
    }))
    // Judged after the 86 removals: a section can be full of items and still
    // have nothing to show tonight.
    .filter((section) => section.items.length > 0);

  return {
    menuId: String(board?.menuId ?? ""),
    theme: nonEmpty(board?.theme),
    sections
  };
}

/** True when the board has nothing to draw — every section empty, or no board at all. */
export function isBoardEmpty(document) {
  return (document?.sections?.length ?? 0) === 0;
}

function toIdSet(ids) {
  const set = new Set();
  for (const id of ids ?? []) {
    if (id !== null && id !== undefined) set.add(String(id));
  }

  return set;
}

/**
 * Sort order first, then the id, so two rows sharing a sort order still come out
 * in the same order every time. An unstable board would make a card and the TV
 * disagree for no reason a person could see.
 */
function bySortOrderThen(idKey) {
  return (left, right) => {
    const difference = numberOr(left?.sortOrder) - numberOr(right?.sortOrder);
    if (difference !== 0) return difference;
    return String(left?.[idKey] ?? "").localeCompare(String(right?.[idKey] ?? ""));
  };
}

function numberOr(value) {
  return typeof value === "number" && Number.isFinite(value) ? value : 0;
}

function nonEmpty(value) {
  if (typeof value !== "string") return null;
  return value.length > 0 ? value : null;
}
