/**
 * The published board, in the shape the content API returns it
 * (`GET api/back-office/content/menus/{id}/published-board`). The engine
 * declares the shape it consumes rather than importing one from an app, so the
 * back office and the display player can both hand it the same thing without
 * either owning the other.
 */
export type BoardInput = {
  menuId: string;
  name?: string | null;
  /** The menu theme attached, or null when none is — a valid state (Q86). */
  theme?: string | null;
  dwellSeconds?: number;
  loopWarningSeconds?: number;
  sections?: readonly BoardInputSection[] | null;
};

export type BoardInputSection = {
  sectionId: string;
  name?: string | null;
  sortOrder?: number;
  items?: readonly BoardInputItem[] | null;
};

export type BoardInputItem = {
  itemId: string;
  name?: string | null;
  description?: string | null;
  /** Exactly as typed: "12", "9.5" or "MP". Never a number. */
  price?: string | null;
  sortOrder?: number;
};

/** What should appear: 86'd items gone, empty sections gone, prices settled. */
export type BoardDocument = {
  menuId: string;
  theme: string | null;
  sections: readonly BoardSection[];
};

export type BoardSection = {
  sectionId: string;
  name: string;
  items: readonly BoardItem[];
};

export type BoardItem = {
  itemId: string;
  name: string;
  description: string | null;
  /** As typed, or `missingPrice` when none was ever entered. */
  price: string;
  /**
   * 86'd right now. Always false on a guest document, because a guest document
   * never contains one — it is only ever true where a surface asked to keep them.
   */
  isUnavailable: boolean;
};

/**
 * `keepUnavailable` is the editing surface's exception: an 86'd item stays in the
 * document, marked, because you cannot turn back on what the surface has hidden
 * from you (Q104). A guest board never sets it.
 */
export type BoardDocumentOptions = { keepUnavailable?: boolean };

export const missingPrice: string;

export function buildBoardDocument(
  board: BoardInput | null | undefined,
  unavailableItemIds?: Iterable<string> | null,
  options?: BoardDocumentOptions
): BoardDocument;

export function isBoardEmpty(document: BoardDocument | null | undefined): boolean;
