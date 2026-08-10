import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Ellipsis, Plus, Search } from "lucide-react";
import { BoardFrame } from "../../board-engine/BoardFrame";
import {
  MenuActionRefused,
  duplicateMenu,
  goBackToMenuVersion,
  loadMenuAvailability,
  loadMenuHistory,
  loadShelf,
  setMenuPutAway,
  takeMenuOffScreens,
  type MenuHistoryEntry,
  type ShelfMenu
} from "./api";
import type { BackOfficeConfiguration } from "./config";
import {
  availableShelfFilters,
  boardCounts,
  cardStatus,
  changePhrase,
  filterShelf,
  hasChangesWaiting,
  isShelfAtScale,
  menusInUse,
  menusNotInUse,
  shelfHeadline,
  shelfSubLine
} from "./menusShelf.mjs";
import "../../board-engine/board-engine.css";
import "./menus-home.css";

type Props = {
  configuration: BackOfficeConfiguration;
  accessToken: string;
  venueName: string;
  /** Opens the existing editor. Interim wiring until milestone 3 (Q100). */
  onOpenMenu: (menuId: string) => void;
  /** The existing create flow, likewise interim. */
  onAddMenu: () => void;
  /** Screens, filtered to the ones needing attention (Q170). */
  onFixScreens: (screenIds: string[]) => void;
};

/**
 * Menus home: the shelf.
 *
 * Every card is a live render of the board that menu's screens are actually
 * showing, drawn by the same engine the TV will use. That is the whole promise
 * of the surface — you look at the shelf and you are looking at your screens.
 */
export default function MenusHome({
  configuration,
  accessToken,
  venueName,
  onOpenMenu,
  onAddMenu,
  onFixScreens
}: Props) {
  const [menus, setMenus] = useState<ShelfMenu[] | null>(null);
  const [unavailable, setUnavailable] = useState<string[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [notice, setNotice] = useState<string | null>(null);
  const [search, setSearch] = useState("");
  const [filter, setFilter] = useState<string | null>(null);
  const [expanded, setExpanded] = useState(false);
  const [busy, setBusy] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    try {
      // Three calls for the page, flat at any menu count: the shelf, what is
      // 86'd, and nothing else. Availability is deliberately not embedded in the
      // shelf read — it is instant and venue-wide, so it is its own fact.
      const [shelf, availability] = await Promise.all([
        loadShelf(configuration, accessToken),
        loadMenuAvailability(configuration, accessToken)
      ]);
      setMenus(shelf);
      setUnavailable(availability.filter((item) => !item.isAvailable).map((item) => item.itemId));
      setError(null);
    } catch {
      setError("Vennusign could not load your menus.");
    }
  }, [configuration, accessToken]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  const act = async (menuId: string, run: () => Promise<string>) => {
    setBusy(menuId);
    try {
      setNotice(await run());
      await refresh();
    } catch (failure) {
      // A named refusal is repeated in the server's own words. Rewriting it here
      // would be a second source of truth about why something was refused.
      setNotice(
        failure instanceof MenuActionRefused
          ? failure.message
          : "Vennusign could not do that. Nothing changed."
      );
    } finally {
      setBusy(null);
    }
  };

  const inUse = useMemo(() => menusInUse(menus ?? []), [menus]);
  const notInUse = useMemo(() => menusNotInUse(menus ?? []), [menus]);
  const atScale = isShelfAtScale(menus ?? []);
  const filters = useMemo(() => availableShelfFilters(menus ?? []), [menus]);

  const shown = useMemo(() => {
    const matched = filterShelf(inUse, { search, filter });
    if (!atScale || expanded || search || filter) return matched;

    // At scale the grid compacts to one row, with every menu that is actually on
    // a screen kept visible and the rest filling by recency (Q165).
    const onScreens = matched.filter((menu) => menu.screenIds.length > 0);
    const rest = matched.filter((menu) => menu.screenIds.length === 0);
    return [...onScreens, ...rest].slice(0, Math.max(onScreens.length, 6));
  }, [inUse, search, filter, atScale, expanded]);

  const hidden = filterShelf(inUse, { search, filter }).length - shown.length;

  const screensNeedingAttention = useMemo(
    () => [...new Set(inUse.filter(hasChangesWaiting).flatMap((menu) => menu.screenIds))],
    [inUse]
  );

  if (error) {
    return (
      <section className="menus-home" data-testid="menus-home">
        <p className="state error" role="alert">{error}</p>
      </section>
    );
  }

  if (menus === null) {
    return (
      <section className="menus-home" data-testid="menus-home">
        <p className="state" role="status">Loading your menus…</p>
      </section>
    );
  }

  // Onboarding is the empty state of this screen, not a wizard (decision 17):
  // there is nothing to fall out of and nothing to re-enter.
  if (menus.length === 0) {
    return (
      <section className="menus-home menus-home--empty" data-testid="menus-home">
        <h1>Let's get your menu in.</h1>
        <p>Pick whatever's easiest. You can fix anything later.</p>
        <button type="button" className="action-primary" onClick={onAddMenu} data-testid="add-a-menu">
          Add a menu
        </button>
      </section>
    );
  }

  return (
    <section className="menus-home" data-testid="menus-home">
      <header className="menus-home__header">
        <div>
          {/* A static label everywhere — no caret, never clickable (Q186), and
              #64748b rather than the layout spec's value, which fails on light (Q184). */}
          <p className="menus-home__venue">{venueName}</p>
          <h1 data-testid="shelf-headline">{shelfHeadline(menus)}</h1>
          <p className="menus-home__subline" data-testid="shelf-subline">{shelfSubLine(menus)}</p>
        </div>
        <div className="menus-home__actions">
          {screensNeedingAttention.length > 0 ? (
            <button
              type="button"
              className="action-secondary"
              onClick={() => onFixScreens(screensNeedingAttention)}
              data-testid="fix-these"
            >
              {screensNeedingAttention.length === 1 ? "Check the screen" : "Check the screens"}
            </button>
          ) : null}
          {atScale ? (
            <button type="button" className="action-primary" onClick={onAddMenu} data-testid="add-a-menu">
              Add a menu
            </button>
          ) : null}
        </div>
      </header>

      {notice ? (
        <p className="state" role="status" data-testid="shelf-notice">{notice}</p>
      ) : null}

      {atScale ? (
        <div className="menus-home__scale" data-testid="shelf-scale-controls">
          <label className="menus-home__search">
            <Search size={15} aria-hidden />
            <span className="visually-hidden">Search menus</span>
            <input
              type="search"
              value={search}
              placeholder="Search menus"
              onChange={(event) => setSearch(event.currentTarget.value)}
              data-testid="shelf-search"
            />
          </label>
          <div className="menus-home__filters" role="group" aria-label="Filter menus">
            {filters.map((chip) => (
              <button
                key={chip.key}
                type="button"
                className={`menus-home__chip${filter === chip.key ? " active" : ""}`}
                aria-pressed={filter === chip.key}
                data-testid="shelf-filter"
                data-filter={chip.key}
                onClick={() => setFilter(filter === chip.key ? null : chip.key)}
              >
                {chip.label}
              </button>
            ))}
          </div>
        </div>
      ) : null}

      <div
        className={`menus-home__grid${atScale ? " menus-home__grid--compact" : ""}`}
        data-testid="shelf-grid"
        data-at-scale={atScale}
      >
        {shown.map((menu) => (
          <MenuCard
            key={menu.menuId}
            menu={menu}
            unavailable={unavailable}
            busy={busy === menu.menuId}
            configuration={configuration}
            accessToken={accessToken}
            venueName={venueName}
            onOpen={() => onOpenMenu(menu.menuId)}
            onAct={act}
          />
        ))}

        {/* The dashed tile stays at six or fewer; past the cutover "Add a menu"
            is a plain button beside search instead (Q166). */}
        {!atScale ? (
          <button type="button" className="menus-home__add-tile" onClick={onAddMenu} data-testid="add-a-menu">
            <span className="menus-home__add-mark" aria-hidden><Plus size={18} /></span>
            <strong>Add a menu</strong>
            <small>Paste it in, or start blank</small>
          </button>
        ) : null}
      </div>

      {hidden > 0 ? (
        <button
          type="button"
          className="menus-home__more"
          onClick={() => setExpanded(true)}
          data-testid="shelf-more"
        >
          {hidden} more ▾
        </button>
      ) : null}

      {notInUse.length > 0 ? (
        <section className="menus-home__idle" data-testid="not-in-use">
          <h2>Not in use</h2>
          <ul>
            {notInUse.map((menu) => (
              <li key={menu.menuId}>
                {/* A chip opens the menu; every action happens from inside it
                    or once it is back on the shelf (Q66). */}
                <button type="button" onClick={() => onOpenMenu(menu.menuId)} data-testid="not-in-use-chip">
                  {menu.name}
                </button>
                <button
                  type="button"
                  className="menus-home__put-back"
                  disabled={busy === menu.menuId}
                  data-testid="put-back"
                  onClick={() =>
                    void act(menu.menuId, async () => {
                      await setMenuPutAway(configuration, accessToken, menu.menuId, false);
                      return `${menu.name} is back on your shelf.`;
                    })
                  }
                >
                  Put back
                </button>
              </li>
            ))}
          </ul>
        </section>
      ) : null}
    </section>
  );
}

type CardProps = {
  menu: ShelfMenu;
  unavailable: string[];
  busy: boolean;
  configuration: BackOfficeConfiguration;
  accessToken: string;
  venueName: string;
  onOpen: () => void;
  onAct: (menuId: string, run: () => Promise<string>) => Promise<void>;
};

function MenuCard({ menu, unavailable, busy, configuration, accessToken, venueName, onOpen, onAct }: CardProps) {
  const [open, setOpen] = useState(false);
  const [takeOff, setTakeOff] = useState(false);
  const [history, setHistory] = useState<MenuHistoryEntry[] | null>(null);
  const details = useRef<HTMLDetailsElement>(null);

  const status = cardStatus(menu);
  const counts = boardCounts(menu.board, unavailable);

  // A menu that has never been published has no bar, however many differences it
  // has from nothing. "5 changes not published" over an empty board is true and
  // useless: everything about it is a change, and the state worth naming is that
  // no screen has ever shown it. The card status line says exactly that.
  //
  // The same predicate the "Changes waiting" filter uses, shared rather than
  // written twice — a filter counting three while the shelf drew two bars is the
  // kind of disagreement nobody reports; they just stop trusting the number.
  const pending = hasChangesWaiting(menu);

  /**
   * Closing the dialog puts focus back on the card that opened it.
   *
   * The trigger lives inside the <details> menu, which is unmounted before the
   * dialog appears - so every close path left focus on the document, and the next
   * Tab restarted from the top of the page rather than continuing from the card.
   * The summary is the stable control that survives all of it.
   */
  const closeTakeOff = () => {
    setTakeOff(false);
    details.current?.querySelector("summary")?.focus();
  };

  const close = (returnFocus = false) => {
    setOpen(false);
    if (details.current) details.current.open = false;
    if (returnFocus) details.current?.querySelector("summary")?.focus();
  };

  /**
   * Escape closes it, and a click anywhere else closes it.
   *
   * A menu you can open and cannot dismiss is the shape a keyboard user gets
   * stuck in: tab walks the six items and then carries on into the next card
   * with the menu still hanging open behind them. Escape returns focus to the
   * button that opened it, because focus left somewhere arbitrary is the same
   * problem wearing a different hat.
   */
  useEffect(() => {
    if (!open) return;

    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        event.stopPropagation();
        close(true);
      }
    };
    const onPointerDown = (event: PointerEvent) => {
      if (!details.current?.contains(event.target as Node)) close();
    };

    document.addEventListener("keydown", onKeyDown, true);
    document.addEventListener("pointerdown", onPointerDown);
    return () => {
      document.removeEventListener("keydown", onKeyDown, true);
      document.removeEventListener("pointerdown", onPointerDown);
    };
  }, [open]);

  const goBack = async () => {
    const entries = history ?? (await loadMenuHistory(configuration, accessToken, menu.menuId));
    setHistory(entries);
    // Addressed by version, which history now carries — without it this action
    // has no way to name what it is going back to.
    const previous = entries.filter((entry) => entry.version !== null).slice(1)[0];
    if (!previous?.version) {
      return "There is no earlier version to go back to.";
    }

    const draft = await goBackToMenuVersion(configuration, accessToken, menu.menuId, previous.version);
    return `${menu.name} is back to how it looked then — ${changePhrase(draft.count)} waiting for you to publish.`;
  };

  return (
    <article className="menu-card" data-testid="menu-card" data-menu-id={menu.menuId} data-pending={pending}>
      <div className={`menu-card__board${pending ? " menu-card__board--pending" : ""}`}>
        <button type="button" className="menu-card__open" onClick={onOpen} data-testid="open-menu">
          {/* The board IS the door — there is no Open button on the card. */}
          <span className="visually-hidden">Open {menu.name}</span>
          {/*
            The board is a picture, so it is named rather than read out. Without
            this the button's accessible name became the entire menu — every item,
            price and description concatenated — and a person using a screen
            reader heard a board where they expected a link to one.
          */}
          <span aria-hidden="true">
          <BoardFrame
            board={menu.board}
            unavailableItemIds={unavailable}
            /* Milestone 2 ships no menu themes, so this is always null and always
               renders the plain board. The prop is here because that is the shape
               the engine consumes, not because a look is being chosen. */
            theme={null}
            /* A card is a picture of the TV, so it carries what the TV carries:
               nothing (Q135). */
            surface="guest"
            fallback={<span className="menu-card__never-published">Never published</span>}
          />
          </span>
        </button>


        {/* The bar occupies reserved space at the bottom of the same container;
            it never overlays board content, and the board crops top-aligned so a
            menu loses its bottom rather than its heading (Q191). */}
        {pending ? (
          <p className="menu-card__pending" data-testid="pending-bar">
            <span>{changePhrase(menu.draftCount)} not published</span>
            <button type="button" onClick={onOpen}>Review →</button>
          </p>
        ) : null}
      </div>

        <details
          className="menu-card__menu"
          ref={details}
          onToggle={(event) => setOpen(event.currentTarget.open)}
        >
          <summary aria-label={`Actions for ${menu.name}`} data-testid="card-actions">
            <Ellipsis size={15} aria-hidden />
          </summary>
          {open ? (
            <div className="menu-card__actions" data-testid="card-menu">
              {/* Verbatim copy, in this order. "Put away" sits directly after
                  Duplicate; "Take off the screens" is alone below the last
                  divider (Q195, build-decision 16). */}
              <button type="button" onClick={onOpen}>Open</button>
              <button type="button" onClick={onOpen}>Quick update</button>
              <hr />
              <button
                type="button"

                disabled={busy || menu.publishedVersion === null}
                data-testid="go-back-to"
                onClick={() => { close(); void onAct(menu.menuId, goBack); }}
              >
                Go back to…
              </button>
              <button
                type="button"

                disabled={busy}
                data-testid="duplicate"
                onClick={() =>
                  { close(); void onAct(menu.menuId, async () => {
                    const copy = await duplicateMenu(configuration, accessToken, menu.menuId);
                    // The name the copy actually got, not the one we asked for.
                    return `${copy.name} is on your shelf.`;
                  }); }
                }
              >
                Duplicate
              </button>
              <button
                type="button"

                disabled={busy}
                data-testid="put-away"
                onClick={() =>
                  { close(); void onAct(menu.menuId, async () => {
                    await setMenuPutAway(configuration, accessToken, menu.menuId, true);
                    return `${menu.name} is not in use. It keeps its history.`;
                  }); }
                }
              >
                Put away
              </button>
              <hr />
              <button
                type="button"

                className="menu-card__danger"
                disabled={busy}
                data-testid="take-off-screens"
                onClick={() => { close(); setTakeOff(true); }}
              >
                Take off the screens
              </button>
            </div>
          ) : null}
        </details>

      <div className="menu-card__meta">
        <div>
          <strong>{menu.name}</strong>
          <span className={`menu-card__status menu-card__status--${status.tone}`} data-testid="card-status">
            {status.text}
          </span>
        </div>
        <small>
          {counts.sections === 0
            ? "Nothing on it yet"
            : `${counts.sections} ${counts.sections === 1 ? "section" : "sections"} · ${counts.items} ${counts.items === 1 ? "item" : "items"}`}
        </small>
      </div>

      {takeOff ? (
        <TakeOffDialog
          menu={menu}
          venueName={venueName}
          busy={busy}
          onCancel={closeTakeOff}
          onConfirm={() =>
            { closeTakeOff(); void onAct(menu.menuId, async () => {
              const draft = await takeMenuOffScreens(configuration, accessToken, menu.menuId);
              return `Taking ${menu.name} off is waiting with your other changes — ${changePhrase(draft.count)} to publish.`;
            }); }
          }
        />
      ) : null}
    </article>
  );
}

/**
 * Take off the screens is never a bare action (criterion 6): the dialog states
 * what replaces the menu, with a picture of it, before anything is confirmed.
 */
function TakeOffDialog({
  menu,
  venueName,
  busy,
  onCancel,
  onConfirm
}: {
  menu: ShelfMenu;
  venueName: string;
  busy: boolean;
  onCancel: () => void;
  onConfirm: () => void;
}) {
  const screens = menu.screenIds.length;
  const panel = useRef<HTMLDivElement>(null);

  /**
   * If it says aria-modal, it has to behave like one.
   *
   * It did not: focus stayed on the document when it opened, Escape did nothing,
   * and Tab walked straight out into the cards behind it. That is worse than an
   * ordinary panel, because a screen reader has been told the rest of the page is
   * inert while it plainly is not — and this is the confirmation standing in front
   * of the one destructive act on the shelf.
   */
  useEffect(() => {
    panel.current?.querySelector<HTMLElement>("button")?.focus();

    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        event.stopPropagation();
        onCancel();
        return;
      }

      if (event.key !== "Tab") return;

      const focusable = panel.current?.querySelectorAll<HTMLElement>("button:not([disabled])");
      if (!focusable?.length) return;

      const first = focusable[0];
      const last = focusable[focusable.length - 1];
      const active = document.activeElement;

      // Wrap at both ends rather than letting focus leave: cancelling should be
      // a decision, not something you tab past without noticing.
      if (event.shiftKey && (active === first || !panel.current?.contains(active))) {
        event.preventDefault();
        last.focus();
      } else if (!event.shiftKey && active === last) {
        event.preventDefault();
        first.focus();
      }
    };

    document.addEventListener("keydown", onKeyDown, true);
    return () => document.removeEventListener("keydown", onKeyDown, true);
  }, [onCancel]);

  return (
    <>
    {/* Clicking away cancels, which is the safe outcome for a destructive act. */}
    <div className="menu-card__scrim" onClick={onCancel} data-testid="take-off-scrim" />
    <div
      className="menu-card__dialog"
      ref={panel}
      role="dialog"
      aria-modal="true"
      aria-label={`Take ${menu.name} off the screens`}
      data-testid="take-off-dialog"
    >
      <h3>Take {menu.name} off the screens?</h3>
      <p>It stays on your Menus home and keeps its history. You can put it back at any time.</p>

      <h4>What people will see instead</h4>
      {/* The venue fallback: a generated logo-and-name card, shown rather than
          authorable. One per venue, used for every empty moment (decisions 14, 36). */}
      <div className="venue-fallback" data-testid="venue-fallback" aria-label="The card your screens will show instead">
        <span aria-hidden>{venueName.slice(0, 1)}</span>
        <strong>{venueName}</strong>
      </div>
      <p className="menu-card__dialog-screens">
        {screens === 0
          ? "This menu is not on a screen right now."
          : screens === 1
            ? "1 screen is showing it."
            : `${screens} screens are showing it.`}
      </p>
      <p className="menu-card__dialog-note">
        It waits with your other changes and reaches the screens when you publish.
      </p>

      <div className="action-surface">
        <button type="button" className="action-secondary" onClick={onCancel}>Cancel</button>
        <button type="button" className="action-danger" disabled={busy} onClick={onConfirm} data-testid="confirm-take-off">
          Take off the screens
        </button>
      </div>
    </div>
    </>
  );
}
