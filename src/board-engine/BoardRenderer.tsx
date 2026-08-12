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
  /**
   * The annotation each 86'd item carries, by item id. Preview only.
   *
   * A map rather than a string, because the note states WHEN that item went off
   * and two items rarely go off together. A single board-level note was handed to
   * every dimmed row alike, so a second 86 was labelled with the first one's time
   * — a wrong fact, stated confidently, about the one thing the note exists to say.
   */
  unavailableNotes?: Readonly<Record<string, string>> | null;
  /**
   * Marks item rows as drag sources. An attribute, not a behaviour: the engine
   * still emits no events, and the surface above decides what a drag means. A
   * guest board never sets it, so a TV row is never draggable.
   */
  itemsDraggable?: boolean;
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
  unavailableNotes = null,
  itemsDraggable = false
}: BoardRendererProps) {
  // A theme written against a later engine is declined outright: rendering the
  // half we understand would be wrong without saying so.
  const usable = canRenderTheme(theme) ? theme : null;
  // Only a preview surface may keep them. A guest board that honoured this would
  // put a struck-through item on a real TV, which is the exact thing the
  // availability model exists to prevent — so the guard is here, not in a caller.
  const keeping = surface === "preview" && keepUnavailable;
  const document = buildBoardDocument(board, unavailableItemIds, { keepUnavailable: keeping });
  const isNorthsideSocial = board?.name?.trim().toLocaleLowerCase() === "northside social";

  return (
    <div
      className={`board${isNorthsideSocial ? " board--northside-social" : ""}`}
      style={boardThemeStyle(usable)}
      data-board-surface={surface}
      data-board-showcase={isNorthsideSocial ? "northside-social" : undefined}
      data-testid="board"
    >
      {isNorthsideSocial ? (
        <header className="board-showcase-header" aria-label="Northside Social — Eat, Drink, Gather">
          <svg className="board-showcase-hop" viewBox="0 0 48 58" aria-hidden="true">
            <path d="M24 3c-7 8-11 16-11 25 0 10 5 18 11 26 6-8 11-16 11-26C35 19 31 11 24 3Z" />
            <path d="M24 10v41M24 18l-8-5m8 13-10-5m10 14-9-5m9-12 8-5m-8 13 10-5m-10 14 9-5" />
          </svg>
          <strong>Northside Social</strong>
          <span>Eat <b>·</b> Drink <b>·</b> Gather</span>
        </header>
      ) : null}
      {/*
        Ordinary boards still draw no venue-name strip (Q98). This isolated
        showcase renders the menu's own name as authored display content so the
        prototype can exercise a title-bearing theme without flattening the board.
      */}
      {document.sections.map((section) => (
        <section
          className="board-section"
          key={section.sectionId}
          data-testid="board-section"
          /*
            Named in the DOM so a surface above can act on it — the builder renames
            a section by clicking this heading (Q96). A data attribute, not a
            handler: the engine stays props-in/DOM-out, and the same markup on a TV
            simply carries an id nothing reads.
          */
          data-section-id={section.sectionId}
        >
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
                data-section-id={section.sectionId}
                data-item-draggable={itemsDraggable ? "true" : undefined}
              >
                {itemsDraggable ? (
                  <span className="board-item-drag-handle" data-testid="item-drag-handle" aria-hidden="true">
                    ⠿
                  </span>
                ) : null}
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
                {item.isUnavailable && unavailableNotes?.[item.itemId] ? (
                  <p className="board-item-note" data-testid="board-item-note">
                    {unavailableNotes[item.itemId]}
                  </p>
                ) : null}
              </li>
            ))}
          </ul>
        </section>
      ))}
      {isNorthsideSocial ? (
        <footer className="board-showcase-footer" aria-hidden="true">
          <svg viewBox="0 0 64 78">
            <path d="M32 75V16M32 31c-8-2-14-7-18-14 9 0 15 4 18 10m0 18c8-2 14-7 18-14-9 0-15 4-18 10m0 18c-8-2-14-7-18-14 9 0 15 4 18 10m0 4c8-2 14-7 18-14-9 0-15 4-18 10M32 16c-4-4-4-9 0-14 4 5 4 10 0 14Z" />
          </svg>
        </footer>
      ) : null}
    </div>
  );
}
