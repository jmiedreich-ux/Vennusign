import { buildBoardDocument } from "./boardDocument.mjs";
import type { BoardInput } from "./boardDocument.d.mts";
import { boardThemeStyle, canRenderTheme } from "./boardTheme.mjs";
import type { BoardTheme } from "./boardTheme.d.mts";

/**
 * Where this board is being drawn.
 *
 * "guest" is a real TV in front of real customers and renders zero annotations
 * (Q135). "preview" is a back-office surface that may show them. It is a property
 * of the SURFACE, not of the application: the Menus home cards are back office and
 * still pass "guest", because a card is a picture of the TV and the annotations
 * would be a picture of something nobody ever sees (Q135, M1 cards show none).
 */
export type BoardSurface = "guest" | "preview";

export type BoardRendererProps = {
  /** The published board. Null renders nothing at all — see BoardFrame for the empty case. */
  board: BoardInput | null | undefined;
  /** Items 86'd right now. They are not drawn, and a section they empty is not drawn. */
  unavailableItemIds?: Iterable<string> | null;
  /** The menu theme attached, or null when none is. Null renders plainly (Q86). */
  theme?: BoardTheme;
  surface?: BoardSurface;
  /**
   * An editing surface keeps 86'd items on the board, struck through and dimmed,
   * because you cannot turn back on what has been hidden from you (Q104). Only a
   * "preview" surface may ask; a guest board ignores it, since a TV showing a
   * struck-through item is exactly what the availability model exists to prevent.
   */
  keepUnavailable?: boolean;
  /** The one annotation the board itself carries, per 86'd item. Preview only. */
  unavailableNote?: string | null;
};

/**
 * A board, drawn.
 *
 * Props in, DOM out. This imports nothing from the back office or the display
 * player — no configuration, no API client, no router — which is the single
 * property that lets milestone 4's player consume it rather than reimplement it.
 *
 * It emits no events and offers no affordances either. Selection rings, drag
 * pills and the rest of the builder live in a layer ABOVE this, so that what an
 * editor is looking at and what a guest is looking at stay literally the same
 * component rather than two that are meant to match.
 */
export function BoardRenderer({
  board,
  unavailableItemIds,
  theme,
  surface = "guest",
  keepUnavailable = false,
  unavailableNote = null
}: BoardRendererProps) {
  // A theme written against a later engine is declined outright: rendering the
  // half we understand would be wrong without saying so.
  const usable = canRenderTheme(theme) ? theme : null;
  // Only a preview surface may keep them. A guest board that honoured this would
  // put a struck-through item on a real TV, which is the exact thing the
  // availability model exists to prevent — so the guard is here, not in a caller.
  const keeping = surface === "preview" && keepUnavailable;
  const document = buildBoardDocument(board, unavailableItemIds, { keepUnavailable: keeping });

  return (
    <div
      className="board"
      style={boardThemeStyle(usable)}
      data-board-surface={surface}
      data-testid="board"
    >
      {/*
        No venue-name strip, ever (Q98). If a TV carries one, the theme editor
        owns it; the Menus engine neither draws one nor assumes room for one.
      */}
      {document.sections.map((section) => (
        <section className="board-section" key={section.sectionId} data-testid="board-section">
          {/*
            A heading in look, not in the document outline. A board is a picture
            of a screen, and it can appear many times on one page — a shelf of
            thirteen cards would otherwise inject thirteen sets of h2s between the
            page's own headings, and a screen reader would read a menu board where
            it expected a page structure. The words are still there and still
            styled; they are simply not claiming to organise this document.
          */}
          <p className="board-section-heading" role="presentation">{section.name}</p>
          <ul className="board-items">
            {section.items.map((item) => (
              <li
                className="board-item"
                key={item.itemId}
                data-testid="board-item"
                data-item-id={item.itemId}
                data-unavailable={item.isUnavailable ? "true" : undefined}
              >
                <p className="board-item-line">
                  <span className="board-item-name">{item.name}</span>
                  <span className="board-item-leader" aria-hidden="true" />
                  <span className="board-item-price">{item.price}</span>
                </p>
                {item.description ? (
                  <p className="board-item-description">{item.description}</p>
                ) : null}
                {/*
                  An annotation, not board content (README): it is 11.5px UI ink,
                  never the board's own, and it cannot reach a guest because a
                  guest document contains no unavailable item to attach it to.
                */}
                {item.isUnavailable && unavailableNote ? (
                  <p className="board-item-note" data-testid="board-item-note">{unavailableNote}</p>
                ) : null}
              </li>
            ))}
          </ul>
        </section>
      ))}
    </div>
  );
}
