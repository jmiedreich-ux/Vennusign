import { useCallback, useEffect, useMemo, useRef, useState } from "react";

import {
  MenuActionRefused,
  addMenuSection,
  deleteMenuSection,
  discardMenuDraft,
  loadBuilderBoard,
  goBackToMenuVersion,
  loadMenuAvailability,
  loadMenuHistory,
  loadMenuThemes,
  loadScreensShowing,
  placeMenuItem,
  publishMenu,
  removeMenuItem,
  renameMenuSection,
  reorderMenuItems,
  reorderMenuSections,
  searchLibraryItems,
  setItemAvailability,
  updateMenuItemValues,
  type BuilderBoard,
  type LibraryItem,
  type MenuAvailability,
  type MenuHistoryEntry,
  type MenuScreenShowing
} from "./api";
import type { BackOfficeConfiguration } from "./config";
import TransientFeedback from "./TransientFeedback";
import { BoardRenderer } from "../../board-engine/BoardRenderer";
import { BoardFrame } from "../../board-engine/BoardFrame";
import { boardLogicalWidth } from "../../board-engine/boardScale.mjs";
import {
  availabilityLine,
  canDiscardDraft,
  canvasBoard,
  changeSentence,
  draftPhrase,
  findItem,
  findOnBoard,
  isMissingPrice,
  itemsOf,
  publishBlockedReason,
  publishLabel,
  publishTargets,
  publishedLine,
  releasedPhrase,
  reorder,
  resumeState,
  sectionsOf,
  sharedItemLine,
  unavailableNote,
  venueTime
} from "./builderModel.mjs";
import type { BuilderPlace } from "./builderModel.d.mts";
import "../../board-engine/board-engine.css";
import "./menu-builder.css";

type Props = {
  configuration: BackOfficeConfiguration;
  apiKey: string;
  menuId: string;
  venueTimezone: string;
  onBack: () => void;
};

type SaveState = "clean" | "saving" | "failed";

/**
 * The board at its logical width, scaled to whatever the canvas is.
 *
 * The engine lays a board out at 1920 wide and every surface scales it — that is
 * what makes a shelf card, this canvas and (from milestone 4) the TV the same DOM
 * rather than three layouts that are meant to match. BoardFrame does the same for
 * a fixed 16:9 box; One-section view needs the width scaled but the height free,
 * because it grows and scrolls for editing (Q105).
 *
 * Rendering the board unscaled here was a real defect: at canvas width the type
 * came out around six times too large, and no unit test could see it.
 */
function BoardStage({ children }: { children: React.ReactNode }) {
  const outer = useRef<HTMLDivElement>(null);
  const inner = useRef<HTMLDivElement>(null);
  const [scale, setScale] = useState(1);
  const [height, setHeight] = useState<number>();

  useEffect(() => {
    const measure = () => {
      if (!outer.current || !inner.current) return;
      const next = outer.current.clientWidth / boardLogicalWidth;
      setScale(next);
      setHeight(inner.current.scrollHeight * next);
    };
    measure();
    if (typeof ResizeObserver === "undefined") return;
    const observer = new ResizeObserver(measure);
    if (outer.current) observer.observe(outer.current);
    if (inner.current) observer.observe(inner.current);
    return () => observer.disconnect();
  });

  return (
    <div className="builder__stage" ref={outer} style={height ? { height: `${height}px` } : undefined}>
      <div
        ref={inner}
        className="builder__stage-inner"
        style={{ width: `${boardLogicalWidth}px`, transform: `scale(${scale})` }}
      >
        {children}
      </div>
    </div>
  );
}

/**
 * One reversible act. ⌘Z issues the inverse, ⌘⇧Z issues it forward again.
 * Session-scoped, capped, never persisted, and never named in a settings page —
 * it is a keystroke, not a feature (decision 7).
 */
type UndoStep = { describe: string; undo: () => Promise<void>; redo: () => Promise<void> };

const placeMemoryKey = (menuId: string) => `vennusign.menu.builder.${menuId}`;

/**
 * The menu builder.
 *
 * Four columns: a section rail that navigates and nothing else, a canvas that IS
 * the preview, an inspector of four controls, and the publish bar. Every edit
 * writes working state; the draft count follows on its own, because it is the
 * computed difference from what the screens are showing.
 */
export default function MenuBuilder({ configuration, apiKey, menuId, venueTimezone, onBack }: Props) {
  const [data, setData] = useState<BuilderBoard>();
  const [availability, setAvailability] = useState<MenuAvailability[]>([]);
  const [screens, setScreens] = useState<MenuScreenShowing[]>([]);
  const [place, setPlace] = useState<BuilderPlace>({ view: "one-section", sectionId: null, selectedItemId: null });
  const [saveState, setSaveState] = useState<SaveState>("clean");
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const [busy, setBusy] = useState(false);
  const [confirmDiscard, setConfirmDiscard] = useState(false);
  const [findOpen, setFindOpen] = useState(false);
  const [findQuery, setFindQuery] = useState("");
  const [themePickerOpen, setThemePickerOpen] = useState(false);
  const [themes, setThemes] = useState<Array<{ key: string; name: string }>>();
  const [seeAllOpen, setSeeAllOpen] = useState(false);
  const [reviewOpen, setReviewOpen] = useState(false);
  const [historyOpen, setHistoryOpen] = useState(false);
  const [history, setHistory] = useState<MenuHistoryEntry[]>();
  const [viewingOpen, setViewingOpen] = useState(false);
  const [viewingScreenId, setViewingScreenId] = useState<string | null>(null);
  const undoStack = useRef<UndoStep[]>([]);
  const redoStack = useRef<UndoStep[]>([]);
  const [historyDepth, setHistoryDepth] = useState({ undo: 0, redo: 0 });

  const board = data?.board;
  const unavailableIds = useMemo(
    () => availability.filter(state => !state.isAvailable).map(state => state.itemId),
    [availability]
  );

  const refresh = useCallback(async () => {
    const [next, states, showing] = await Promise.all([
      loadBuilderBoard(configuration, apiKey, menuId),
      loadMenuAvailability(configuration, apiKey),
      loadScreensShowing(configuration, apiKey)
    ]);
    setData(next);
    setAvailability(states);
    setScreens(showing);
    return next;
  }, [apiKey, configuration, menuId]);

  useEffect(() => {
    let cancelled = false;
    refresh()
      .then(next => {
        if (cancelled) return;
        // Return visits restore where you left off; a first visit is One-section
        // view, top section, nothing selected (Q116).
        let remembered = null;
        try {
          remembered = JSON.parse(sessionStorage.getItem(placeMemoryKey(menuId)) ?? "null");
        } catch {
          remembered = null;
        }
        setPlace(resumeState(next.board, remembered));
      })
      .catch(() => {
        if (!cancelled) setError("This menu could not be opened. Check your connection and try again.");
      });
    return () => {
      cancelled = true;
    };
  }, [menuId, refresh]);

  useEffect(() => {
    sessionStorage.setItem(placeMemoryKey(menuId), JSON.stringify(place));
  }, [menuId, place]);

  const selected = useMemo(() => findItem(board, place.selectedItemId), [board, place.selectedItemId]);
  const selectedAvailability = useMemo(
    () => availability.find(state => state.itemId === place.selectedItemId) ?? null,
    [availability, place.selectedItemId]
  );

  /**
   * Runs a write and keeps the byline honest about it. A failure never clears the
   * change from the surface: it flips the byline amber, holds the retry, and
   * blocks Publish until the queue is confirmed (Q197).
   */
  const writes = useRef<Promise<unknown>>(Promise.resolve());

  const run = useCallback(
    async (action: () => Promise<void>, undoStep?: UndoStep) => {
      setBusy(true);
      setSaveState("saving");
      setError(undefined);
      /*
       * Writes go one at a time, in the order they were made.
       *
       * Two edits to the same item can otherwise be in flight together, and the
       * server applies whichever it finishes last — so a slow first save can land
       * AFTER a fast second one and quietly restore the older value. The old
       * editor had the same shape of bug from the other end (its refresh replaced
       * the newer draft when the older save returned), and a spec has guarded it
       * ever since; this is that guarantee moved to where it can actually hold.
       */
      const mine = writes.current.then(action, action);
      writes.current = mine.catch(() => undefined);
      try {
        await mine;
        await refresh();
        setSaveState("clean");
        if (undoStep) {
          undoStack.current = [...undoStack.current.slice(-49), undoStep];
          redoStack.current = [];
          setHistoryDepth({ undo: undoStack.current.length, redo: 0 });
        }
      } catch (failure) {
        setSaveState("failed");
        setError(
          failure instanceof MenuActionRefused
            ? failure.message
            : "Couldn't save your last change — retrying won't lose it. Check your connection."
        );
      } finally {
        setBusy(false);
      }
    },
    [refresh]
  );

  // ---- the section rail ----------------------------------------------------

  const [addingSection, setAddingSection] = useState(false);
  const [newSectionName, setNewSectionName] = useState("");
  const newSectionRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (addingSection) newSectionRef.current?.focus();
  }, [addingSection]);

  const commitNewSection = async () => {
    const name = newSectionName.trim();
    setAddingSection(false);
    setNewSectionName("");
    if (!name) return;

    let created: { sectionId: string } | undefined;
    await run(
      async () => {
        created = await addMenuSection(configuration, apiKey, menuId, name);
      },
      {
        describe: `Add section "${name}"`,
        undo: async () => {
          if (created) await deleteMenuSection(configuration, apiKey, menuId, created.sectionId);
        },
        redo: async () => {
          created = await addMenuSection(configuration, apiKey, menuId, name);
        }
      }
    );
    if (created) setPlace(current => ({ ...current, sectionId: created!.sectionId, selectedItemId: null }));
  };

  const moveSection = async (from: number, to: number) => {
    const ids = sectionsOf(board).map(section => section.sectionId);
    const next = reorder(ids, from, to);
    if (next.join() === ids.join()) return;
    await run(
      () => reorderMenuSections(configuration, apiKey, menuId, next),
      {
        describe: "Move section",
        undo: () => reorderMenuSections(configuration, apiKey, menuId, ids),
        redo: () => reorderMenuSections(configuration, apiKey, menuId, next)
      }
    );
  };

  const deleteSection = async (sectionId: string, name: string | null) => {
    await run(async () => {
      const outcome = await deleteMenuSection(configuration, apiKey, menuId, sectionId);
      setNotice(releasedPhrase(outcome.releasedItemCount));
      setPlace(current =>
        current.sectionId === sectionId ? { ...current, sectionId: null, selectedItemId: null } : current
      );
    });
    // Deliberately not undoable: the section's id is gone, so an "undo" would put
    // back something that only looked the same. Saying so beats a control that
    // half works — the items are in the library and can be placed again.
    void name;
  };

  // ---- the canvas ----------------------------------------------------------

  const canvasRef = useRef<HTMLDivElement>(null);
  const [priceEdit, setPriceEdit] = useState<{ itemId: string; value: string; box: DOMRect } | null>(null);

  /*
   * The selection ring is drawn ON the board row, but the engine knows nothing
   * about selection — it renders a board and nothing else, which is the property
   * that lets the player consume it. So the marker is applied here, after render,
   * to the row the engine emitted. Nothing about the board's own layout changes:
   * an outline draws outside the box, so the text does not move by a pixel when
   * something is selected, and the canvas cannot re-typeset itself.
   */
  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    for (const row of canvas.querySelectorAll<HTMLElement>("[data-item-id]")) {
      row.classList.toggle("is-selected", row.dataset.itemId === place.selectedItemId);
    }
  });

  const selectFromCanvas = (event: React.MouseEvent<HTMLDivElement>) => {
    const row = (event.target as HTMLElement).closest<HTMLElement>("[data-item-id]");
    if (!row) return;
    const itemId = row.dataset.itemId!;
    setPlace(current => ({ ...current, selectedItemId: itemId }));

    // In-place editing is the price only (Q118). Clicking a name or description
    // selects the item and focuses the matching inspector field instead.
    const priceCell = (event.target as HTMLElement).closest<HTMLElement>(".board-item-price");
    if (priceCell && canvasRef.current) {
      const canvasBox = canvasRef.current.getBoundingClientRect();
      const cell = priceCell.getBoundingClientRect();
      const found = findItem(board, itemId);
      setPriceEdit({
        itemId,
        value: found?.item.price ?? "",
        box: new DOMRect(cell.left - canvasBox.left, cell.top - canvasBox.top, cell.width, cell.height)
      });
      return;
    }

    const field = (event.target as HTMLElement).closest(".board-item-description") ? "description" : "name";
    window.requestAnimationFrame(() => {
      document.querySelector<HTMLElement>(`[data-inspector-field="${field}"]`)?.focus();
    });
  };

  const commitPrice = async () => {
    const edit = priceEdit;
    setPriceEdit(null);
    if (!edit) return;
    const found = findItem(board, edit.itemId);
    if (!found || (found.item.price ?? "") === edit.value) return;

    const before = found.item;
    await run(
      () =>
        updateMenuItemValues(configuration, apiKey, edit.itemId, {
          name: before.name ?? "",
          description: before.description,
          price: edit.value.trim() === "" ? null : edit.value
        }),
      {
        describe: "Change price",
        undo: () =>
          updateMenuItemValues(configuration, apiKey, edit.itemId, {
            name: before.name ?? "",
            description: before.description,
            price: before.price
          }),
        redo: () =>
          updateMenuItemValues(configuration, apiKey, edit.itemId, {
            name: before.name ?? "",
            description: before.description,
            price: edit.value.trim() === "" ? null : edit.value
          })
      }
    );
  };

  // ---- the inspector -------------------------------------------------------

  const [draftItem, setDraftItem] = useState<{ name: string; description: string; price: string } | null>(null);
  const [itemBoards, setItemBoards] = useState<LibraryItem["boards"]>([]);

  useEffect(() => {
    if (!selected) {
      setDraftItem(null);
      setItemBoards([]);
      return;
    }
    setDraftItem({
      name: selected.item.name ?? "",
      description: selected.item.description ?? "",
      price: selected.item.price ?? ""
    });

    // Which other boards this item sits on, for Q5's shared-price line. Read from
    // the library rather than assumed, so a menu renamed elsewhere is named right.
    let cancelled = false;
    searchLibraryItems(configuration, apiKey, selected.item.name ?? "", 20)
      .then(hits => {
        if (cancelled) return;
        setItemBoards(hits.find(hit => hit.itemId === selected.item.itemId)?.boards ?? []);
      })
      .catch(() => {
        if (!cancelled) setItemBoards([]);
      });
    return () => {
      cancelled = true;
    };
  }, [apiKey, configuration, selected]);

  const saveItem = async () => {
    if (!selected || !draftItem) return;
    const before = selected.item;
    const next = {
      // An emptied name reverts rather than saving blank (Q119).
      name: draftItem.name.trim() === "" ? (before.name ?? "") : draftItem.name,
      description: draftItem.description.trim() === "" ? null : draftItem.description,
      price: draftItem.price.trim() === "" ? null : draftItem.price
    };
    if (
      next.name === (before.name ?? "") &&
      next.description === before.description &&
      next.price === before.price
    ) {
      return;
    }

    await run(() => updateMenuItemValues(configuration, apiKey, before.itemId, next), {
      describe: "Edit item",
      undo: () =>
        updateMenuItemValues(configuration, apiKey, before.itemId, {
          name: before.name ?? "",
          description: before.description,
          price: before.price
        }),
      redo: () => updateMenuItemValues(configuration, apiKey, before.itemId, next)
    });
  };

  const toggleAvailability = async () => {
    if (!selected) return;
    const isAvailable = selectedAvailability?.isAvailable !== false;
    // Availability commits instantly and never joins the draft. It is deliberately
    // NOT on the undo stack: undo is for the queue, and this already went out.
    await run(async () => {
      await setItemAvailability(configuration, apiKey, selected.item.itemId, !isAvailable);
      setNotice(
        isAvailable
          ? `${selected.item.name} is off. It is already gone from every screen showing it.`
          : `${selected.item.name} is back on. It is showing again now.`
      );
    });
  };

  const removeFromBoard = async () => {
    if (!selected) return;
    const { item, sectionId } = selected;
    await run(
      async () => {
        await removeMenuItem(configuration, apiKey, menuId, item.itemId);
        setPlace(current => ({ ...current, selectedItemId: null }));
      },
      {
        describe: "Remove from this board",
        undo: async () => {
          await placeMenuItem(configuration, apiKey, menuId, sectionId, { itemId: item.itemId });
        },
        redo: () => removeMenuItem(configuration, apiKey, menuId, item.itemId)
      }
    );
  };

  // ---- adding items --------------------------------------------------------

  const [addQuery, setAddQuery] = useState("");
  const [addSectionId, setAddSectionId] = useState<string | null>(null);
  const [hits, setHits] = useState<LibraryItem[]>([]);

  useEffect(() => {
    if (!addSectionId || addQuery.trim().length === 0) {
      setHits([]);
      return;
    }
    let cancelled = false;
    const timer = window.setTimeout(() => {
      searchLibraryItems(configuration, apiKey, addQuery, 8)
        .then(found => {
          if (!cancelled) setHits(found);
        })
        .catch(() => {
          if (!cancelled) setHits([]);
        });
    }, 150);
    return () => {
      cancelled = true;
      window.clearTimeout(timer);
    };
  }, [addQuery, addSectionId, apiKey, configuration]);

  const place_ = async (sectionId: string, request: { itemId?: string; name?: string }) => {
    let outcome: Awaited<ReturnType<typeof placeMenuItem>> | undefined;
    await run(
      async () => {
        outcome = await placeMenuItem(configuration, apiKey, menuId, sectionId, request);
      },
      {
        describe: "Add to this board",
        undo: async () => {
          if (outcome?.itemId && outcome.outcome === "placed") {
            await removeMenuItem(configuration, apiKey, menuId, outcome.itemId);
          }
        },
        redo: async () => {
          if (outcome?.itemId) {
            await placeMenuItem(configuration, apiKey, menuId, sectionId, { itemId: outcome.itemId });
          }
        }
      }
    );

    if (!outcome) return;
    if (outcome.outcome === "already_on_board") {
      // Not an error: jump to where it already is (Q112).
      setPlace(current => ({
        ...current,
        sectionId: outcome!.sectionId ?? current.sectionId,
        selectedItemId: outcome!.itemId
      }));
      setNotice("That one is already on this board — here it is.");
    } else {
      setPlace(current => ({ ...current, sectionId, selectedItemId: outcome!.itemId }));
      // A new item opens with the name focused so it can be corrected at once (Q113).
      if (request.name) {
        window.requestAnimationFrame(() => {
          document.querySelector<HTMLElement>('[data-inspector-field="name"]')?.focus();
        });
      }
    }
    setAddQuery("");
    setAddSectionId(null);
  };

  // ---- undo, redo and find -------------------------------------------------

  const undo = useCallback(async () => {
    const step = undoStack.current.at(-1);
    if (!step) return;
    undoStack.current = undoStack.current.slice(0, -1);
    setBusy(true);
    try {
      await step.undo();
      await refresh();
      redoStack.current = [...redoStack.current, step];
      setNotice(`Undid: ${step.describe.toLowerCase()}.`);
    } catch {
      // A failed inverse says so rather than clobbering: somebody else may have
      // changed the same thing, and pretending otherwise loses their work.
      setError("That can't be undone now — the menu changed since. Nothing was moved.");
    } finally {
      setHistoryDepth({ undo: undoStack.current.length, redo: redoStack.current.length });
      setBusy(false);
    }
  }, [refresh]);

  const redo = useCallback(async () => {
    const step = redoStack.current.at(-1);
    if (!step) return;
    redoStack.current = redoStack.current.slice(0, -1);
    setBusy(true);
    try {
      await step.redo();
      await refresh();
      undoStack.current = [...undoStack.current, step];
      setNotice(`Redid: ${step.describe.toLowerCase()}.`);
    } catch {
      setError("That can't be redone now — the menu changed since. Nothing was moved.");
    } finally {
      setHistoryDepth({ undo: undoStack.current.length, redo: redoStack.current.length });
      setBusy(false);
    }
  }, [refresh]);

  useEffect(() => {
    const onKey = (event: KeyboardEvent) => {
      const meta = event.metaKey || event.ctrlKey;
      if (meta && event.key.toLowerCase() === "k") {
        event.preventDefault();
        setFindOpen(true);
        return;
      }
      if (meta && event.key.toLowerCase() === "z") {
        const typing = document.activeElement?.matches("input, textarea");
        if (typing) return;
        event.preventDefault();
        void (event.shiftKey ? redo() : undo());
        return;
      }
      if (event.key === "Escape") {
        setFindOpen(false);
        setThemePickerOpen(false);
        setSeeAllOpen(false);
        setReviewOpen(false);
        setHistoryOpen(false);
        setViewingOpen(false);
        setConfirmDiscard(false);
        setPriceEdit(null);
        setAddSectionId(null);
        return;
      }
      if ((event.key === "Delete" || event.key === "Backspace") && place.selectedItemId) {
        const typing = document.activeElement?.matches("input, textarea");
        if (typing) return;
        event.preventDefault();
        void removeFromBoard();
      }
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [place.selectedItemId, redo, undo]);

  // ---- publishing ----------------------------------------------------------

  const targets = useMemo(() => {
    const showing = screens.filter(screen => (data?.screenIds ?? []).includes(screen.screenId));
    return publishTargets(
      showing.map(screen => ({
        screenId: screen.screenId,
        screenName: screen.screenName,
        state: screen.menuId === menuId ? "ready" : "taken"
      }))
    );
  }, [data?.screenIds, menuId, screens]);

  const blocked = publishBlockedReason({ draftCount: data?.draftCount ?? 0, saveState });

  const viewingScreens = useMemo(
    () => screens.filter(screen => (data?.screenIds ?? []).includes(screen.screenId)),
    [data?.screenIds, screens]
  );
  const viewingScreen =
    viewingScreens.find(screen => screen.screenId === viewingScreenId) ?? viewingScreens[0] ?? null;

  const publish = async () => {
    await run(async () => {
      const result = await publishMenu(configuration, apiKey, menuId);
      undoStack.current = [];
      redoStack.current = [];
      setHistoryDepth({ undo: 0, redo: 0 });
      setNotice(
        result.conflictedScreenIds.length > 0
          ? `Published. ${result.conflictedScreenIds.length} screen${
              result.conflictedScreenIds.length === 1 ? "" : "s"
            } now belong to another menu and were left alone.`
          : "Published. Your screens are showing it."
      );
    });
  };

  const discard = async () => {
    setConfirmDiscard(false);
    await run(async () => {
      const result = await discardMenuDraft(configuration, apiKey, menuId);
      undoStack.current = [];
      redoStack.current = [];
      setHistoryDepth({ undo: 0, redo: 0 });
      setNotice(`${result.discarded} change${result.discarded === 1 ? "" : "s"} discarded.`);
    });
  };

  // ---- render --------------------------------------------------------------

  if (!board) {
    return (
      <div className="builder builder--loading" data-testid="builder-loading">
        <p role="status">{error ?? "Opening this menu…"}</p>
        <button type="button" className="action-secondary" onClick={onBack}>
          ← Menus
        </button>
      </div>
    );
  }

  const sections = sectionsOf(board);
  const shown = canvasBoard(board, place);
  const offNote = availabilityLine(selectedAvailability, venueTimezone);
  /*
   * One note for every 86'd row on the canvas. The design writes it with the time
   * ("86'd 6:40pm — hidden on all screens right now"), because the first question
   * about an item that is off is when it went off. The engine draws whichever
   * note it is handed; it never composes one, so a guest board cannot inherit it.
   */
  const boardNote = unavailableNote(
    availability.find(state => !state.isAvailable) ?? null,
    venueTimezone
  );
  const isOff = selectedAvailability?.isAvailable === false;

  return (
    <div className="builder" data-testid="menu-builder" data-menu-id={menuId}>
      <header className="builder__top">
        <nav className="builder__crumbs" aria-label="Breadcrumb">
          <button type="button" className="builder__crumb-link" onClick={onBack} data-testid="back-to-menus">
            Menus
          </button>
          <span aria-hidden="true">/</span>
          <span className="builder__crumb-current" data-testid="builder-menu-name">
            {board.name}
          </span>
        </nav>

        <div className="builder__top-actions">
          <div className="builder__history" role="group" aria-label="Undo and redo">
            <button
              type="button"
              onClick={() => void undo()}
              disabled={historyDepth.undo === 0 || busy}
              aria-label="Undo"
              data-testid="undo"
            >
              ↶
            </button>
            <button
              type="button"
              onClick={() => void redo()}
              disabled={historyDepth.redo === 0 || busy}
              aria-label="Redo"
              data-testid="redo"
            >
              ↷
            </button>
          </div>

          {/*
            Q101: the menu's target screens, offline ones included, named without a
            resolution until milestone 4 reports geometry. With none paired it says
            so rather than offering an empty list.
          */}
          {targets.total === 0 ? (
            <span className="builder__viewing-as" data-testid="viewing-as">No screens yet</span>
          ) : (
            <div className="builder__viewing">
              <button
                type="button"
                className="builder__viewing-as"
                data-testid="viewing-as"
                aria-expanded={viewingOpen}
                aria-haspopup="listbox"
                onClick={() => setViewingOpen(open => !open)}
              >
                Viewing as <strong>{viewingScreen?.screenName ?? "a screen"}</strong> ▾
              </button>
              {viewingOpen ? (
                <ul className="builder__viewing-list" role="listbox" data-testid="viewing-as-list">
                  {viewingScreens.map(screen => (
                    <li key={screen.screenId}>
                      <button
                        type="button"
                        role="option"
                        aria-selected={screen.screenId === viewingScreen?.screenId}
                        data-testid="viewing-as-option"
                        onClick={() => {
                          setViewingScreenId(screen.screenId);
                          setViewingOpen(false);
                        }}
                      >
                        <strong>{screen.screenName}</strong>
                        {/* Named without a resolution: geometry arrives in milestone 4. */}
                        <span>{screen.menuId === menuId ? "showing this menu" : "another menu now"}</span>
                      </button>
                    </li>
                  ))}
                </ul>
              ) : null}
            </div>
          )}

          {/*
            Play stays visible and says plainly what it cannot do yet (Q102), rather
            than vanishing or greying out. Its destination arrives in milestone 5.
          */}
          <button
            type="button"
            className="builder__play"
            data-testid="play"
            onClick={() =>
              setNotice(
                targets.total === 0
                  ? "Nothing to play against yet — pair a screen first."
                  : "Play arrives with the board view. Your screens already show what you publish."
              )
            }
          >
            ▶ Play
          </button>
        </div>
      </header>

      <div className="builder__columns">
        <nav className="builder__rail" aria-label="Sections">
          <div className="builder__rail-head">
            <h2>Sections</h2>
            <button type="button" onClick={() => setAddingSection(true)} aria-label="Add a section" data-testid="add-section">
              +
            </button>
          </div>

          <ul className="builder__rail-list">
            {sections.map((section, index) => (
              <li key={section.sectionId}>
                <button
                  type="button"
                  className={`builder__rail-row${place.sectionId === section.sectionId ? " is-selected" : ""}`}
                  onClick={() => setPlace(current => ({ ...current, sectionId: section.sectionId, selectedItemId: null }))}
                  data-testid="rail-section"
                  data-section-id={section.sectionId}
                  aria-current={place.sectionId === section.sectionId}
                >
                  <span className="builder__rail-handle" aria-hidden="true">
                    ⠿
                  </span>
                  <span className="builder__rail-name">{section.name}</span>
                  <span className="builder__rail-count">{itemsOf(board, section.sectionId).length}</span>
                </button>
                <span className="builder__rail-move">
                  <button
                    type="button"
                    onClick={() => void moveSection(index, index - 1)}
                    disabled={index === 0 || busy}
                    aria-label={`Move ${section.name} up`}
                  >
                    ↑
                  </button>
                  <button
                    type="button"
                    onClick={() => void moveSection(index, index + 1)}
                    disabled={index === sections.length - 1 || busy}
                    aria-label={`Move ${section.name} down`}
                  >
                    ↓
                  </button>
                </span>
              </li>
            ))}

            {addingSection ? (
              <li>
                <input
                  ref={newSectionRef}
                  className="builder__rail-new"
                  value={newSectionName}
                  maxLength={200}
                  placeholder="Section name"
                  aria-label="New section name"
                  data-testid="new-section-name"
                  onChange={event => setNewSectionName(event.target.value)}
                  onBlur={() => void commitNewSection()}
                  onKeyDown={event => {
                    if (event.key === "Enter") void commitNewSection();
                    if (event.key === "Escape") {
                      setAddingSection(false);
                      setNewSectionName("");
                    }
                  }}
                />
              </li>
            ) : null}
          </ul>
        </nav>

        <main className="builder__canvas">
          <div className="builder__canvas-head">
            <div className="builder__view-toggle" role="group" aria-label="View">
              {(["one-section", "whole-board"] as const).map(view => (
                <button
                  key={view}
                  type="button"
                  className={place.view === view ? "is-selected" : ""}
                  aria-pressed={place.view === view}
                  data-testid={`view-${view}`}
                  onClick={() => setPlace(current => ({ ...current, view }))}
                >
                  {view === "one-section" ? "One section" : "Whole board"}
                </button>
              ))}
            </div>
            {place.selectedItemId && isMissingPrice(selected?.item) ? (
              <p className="builder__flag" data-testid="missing-price-flag">
                No price yet. You can still publish it.
              </p>
            ) : null}
          </div>

          <div
            className="builder__board-card"
            ref={canvasRef}
            data-selected-item={place.selectedItemId ?? undefined}
            onClick={selectFromCanvas}
            data-testid="canvas"
          >
            {place.view === "whole-board" ? (
              <BoardFrame
                board={shown}
                unavailableItemIds={unavailableIds}
                surface="preview"
                keepUnavailable
                unavailableNote={boardNote}
              />
            ) : (
              <BoardStage>
                <BoardRenderer
                  board={shown}
                  unavailableItemIds={unavailableIds}
                  surface="preview"
                  keepUnavailable
                  unavailableNote={boardNote}
                />
              </BoardStage>
            )}

            {priceEdit ? (
              <input
                className="builder__price-edit"
                autoFocus
                value={priceEdit.value}
                aria-label="Price"
                data-testid="price-edit"
                maxLength={40}
                style={{
                  left: `${priceEdit.box.left}px`,
                  top: `${priceEdit.box.top}px`,
                  width: `${Math.max(priceEdit.box.width, 64)}px`,
                  height: `${priceEdit.box.height}px`
                }}
                onChange={event => setPriceEdit(current => (current ? { ...current, value: event.target.value } : current))}
                onBlur={() => void commitPrice()}
                onKeyDown={event => {
                  if (event.key === "Enter") void commitPrice();
                  if (event.key === "Escape") setPriceEdit(null);
                }}
              />
            ) : null}
          </div>

          {place.view === "one-section" && place.sectionId ? (
            <div className="builder__add-row">
              {addSectionId === place.sectionId ? (
                <>
                  <input
                    autoFocus
                    value={addQuery}
                    placeholder="Find an item, or type a new one"
                    aria-label="Add an item"
                    data-testid="add-item-input"
                    onChange={event => setAddQuery(event.target.value)}
                    onKeyDown={event => {
                      if (event.key === "Enter" && addQuery.trim()) {
                        void place_(place.sectionId!, { name: addQuery.trim() });
                      }
                    }}
                  />
                  <ul className="builder__add-results" data-testid="add-item-results">
                    {hits.map(hit => {
                      const here = hit.boards.some(entry => entry.menuId === menuId);
                      const elsewhere = hit.boards.filter(entry => entry.menuId !== menuId);
                      return (
                        <li key={hit.itemId}>
                          <button
                            type="button"
                            data-testid="add-item-result"
                            data-item-id={hit.itemId}
                            onClick={() => void place_(place.sectionId!, { itemId: hit.itemId })}
                          >
                            <span className="builder__add-name">{hit.name}</span>
                            <span className="builder__add-where">
                              {here
                                ? "already on this board"
                                : elsewhere.length === 0
                                  ? "not on a board yet"
                                  : elsewhere.length === 1
                                    ? `on ${elsewhere[0].menuName}`
                                    : elsewhere.length === 2
                                      ? `on ${elsewhere[0].menuName} and ${elsewhere[1].menuName}`
                                      : `on ${elsewhere.length} boards`}
                              {hit.isAvailable ? "" : " · 86'd right now"}
                            </span>
                          </button>
                        </li>
                      );
                    })}
                    {addQuery.trim() ? (
                      <li>
                        <button
                          type="button"
                          className="builder__add-create"
                          data-testid="add-item-create"
                          onClick={() => void place_(place.sectionId!, { name: addQuery.trim() })}
                        >
                          Create “{addQuery.trim()}” as a new item
                        </button>
                      </li>
                    ) : null}
                  </ul>
                </>
              ) : (
                <button
                  type="button"
                  className="builder__add-open"
                  data-testid="open-add-item"
                  onClick={() => setAddSectionId(place.sectionId)}
                >
                  + Add an item
                </button>
              )}
            </div>
          ) : null}

          {place.view === "one-section" && place.sectionId ? (
            <div className="builder__section-actions">
              <label>
                <span className="builder__label">Section name</span>
                <input
                  defaultValue={sections.find(section => section.sectionId === place.sectionId)?.name ?? ""}
                  key={place.sectionId}
                  maxLength={200}
                  data-testid="section-name"
                  onBlur={event => {
                    const name = event.target.value.trim();
                    const current = sections.find(section => section.sectionId === place.sectionId)?.name ?? "";
                    if (!name || name === current) {
                      event.target.value = current;
                      return;
                    }
                    void run(() => renameMenuSection(configuration, apiKey, menuId, place.sectionId!, name), {
                      describe: "Rename section",
                      undo: () => renameMenuSection(configuration, apiKey, menuId, place.sectionId!, current),
                      redo: () => renameMenuSection(configuration, apiKey, menuId, place.sectionId!, name)
                    });
                  }}
                />
              </label>
              <button
                type="button"
                className="builder__quiet-danger"
                data-testid="delete-section"
                onClick={() =>
                  void deleteSection(
                    place.sectionId!,
                    sections.find(section => section.sectionId === place.sectionId)?.name ?? null
                  )
                }
              >
                Delete this section
              </button>
            </div>
          ) : null}
        </main>

        <aside className="builder__inspector" aria-label="Item">
          {!selected || !draftItem ? (
            <p className="builder__inspector-empty" data-testid="inspector-empty">
              Select an item on the board to edit it.
            </p>
          ) : (
            <>
              <div className="builder__inspector-head">
                <h2>{selected.item.name}</h2>
                <button
                  type="button"
                  aria-label="Close"
                  onClick={() => setPlace(current => ({ ...current, selectedItemId: null }))}
                >
                  ✕
                </button>
              </div>

              <div
                className={`builder__availability${isOff ? " is-off" : ""}`}
                data-testid="availability-panel"
                data-off={isOff ? "true" : "false"}
              >
                <div className="builder__availability-head">
                  <strong>{isOff ? "Off right now" : "On the board"}</strong>
                  <button
                    type="button"
                    role="switch"
                    aria-checked={!isOff}
                    aria-label={isOff ? "Turn back on" : "Turn off"}
                    data-testid="availability-switch"
                    onClick={() => void toggleAvailability()}
                    disabled={busy}
                  >
                    <span aria-hidden="true" />
                  </button>
                </div>
                <p>
                  {offNote ??
                    "Showing on every screen this board is on. Turning it off hides it everywhere, immediately."}
                </p>
              </div>

              <label>
                <span className="builder__label">Name</span>
                <input
                  data-inspector-field="name"
                  data-testid="item-name"
                  maxLength={200}
                  value={draftItem.name}
                  onChange={event => setDraftItem({ ...draftItem, name: event.target.value })}
                  onBlur={() => void saveItem()}
                />
              </label>

              <label>
                <span className="builder__label">Description</span>
                <textarea
                  data-inspector-field="description"
                  data-testid="item-description"
                  maxLength={1000}
                  rows={3}
                  value={draftItem.description}
                  onChange={event => setDraftItem({ ...draftItem, description: event.target.value })}
                  onBlur={() => void saveItem()}
                />
              </label>

              <label>
                <span className="builder__label">Price</span>
                <input
                  data-inspector-field="price"
                  data-testid="item-price"
                  maxLength={40}
                  value={draftItem.price}
                  placeholder="9.5, MP, or leave it"
                  onChange={event => setDraftItem({ ...draftItem, price: event.target.value })}
                  onBlur={() => void saveItem()}
                />
              </label>

              {/*
                Q5's design follow-up, resolved as a statement rather than a step:
                one item is one shared price, and saying so quietly beats asking.
              */}
              {sharedItemLine(itemBoards, menuId) ? (
                <p className="builder__shared" data-testid="shared-item-line">
                  {sharedItemLine(itemBoards, menuId)}
                </p>
              ) : null}

              <button type="button" className="builder__quiet-danger" data-testid="remove-item" onClick={() => void removeFromBoard()}>
                Remove from this board
              </button>

              <p className="builder__theme-footer">
                <button
                  type="button"
                  data-testid="open-theme-picker"
                  onClick={() => {
                    setThemePickerOpen(true);
                    if (!themes) {
                      loadMenuThemes(configuration, apiKey)
                        .then(setThemes)
                        .catch(() => setThemes([]));
                    }
                  }}
                >
                  {board.theme ? `Theme: ${board.theme}` : "No theme on this menu"}
                </button>
              </p>
            </>
          )}
        </aside>
      </div>

      <footer className="builder__publish" data-testid="publish-bar">
        <div className="builder__publish-left">
          <strong data-testid="draft-count" className={data.draftCount ? "is-pending" : ""}>
            {draftPhrase(data.draftCount, { neverPublished: data.publishedVersion === null })}
          </strong>
          <span className="builder__publish-meta">
            {saveState === "failed" ? (
              <span className="builder__save-failed" data-testid="save-failed">
                Couldn&apos;t save your last change — retrying…
              </span>
            ) : (
              publishedLine(data, venueTimezone)
            )}
            {data.publishedVersion !== null ? (
              <>
                {" · "}
                <button
                  type="button"
                  className="builder__link"
                  data-testid="go-back-to"
                  onClick={() => {
                    setHistoryOpen(true);
                    if (!history) {
                      loadMenuHistory(configuration, apiKey, menuId)
                        .then(setHistory)
                        .catch(() => setHistory([]));
                    }
                  }}
                >
                  go back to…
                </button>
              </>
            ) : null}
            {canDiscardDraft(data) ? (
              <>
                {" · "}
                <button type="button" className="builder__link" data-testid="discard-draft" onClick={() => setConfirmDiscard(true)}>
                  discard draft
                </button>
              </>
            ) : null}
          </span>
        </div>

        <div className="builder__publish-screens" data-testid="publish-screens" data-mode={targets.mode}>
          {targets.mode === "chips" ? (
            targets.chips.map(screen => (
              <span
                key={screen.screenId}
                className={`builder__chip builder__chip--${screen.state}`}
                data-testid="screen-chip"
              >
                <strong>{screen.screenName}</strong>
                <small>{screen.state === "ready" ? "showing this menu" : "another menu now"}</small>
              </span>
            ))
          ) : (
            <>
              <span className="builder__chip" data-testid="screen-count">
                <strong>{targets.countPhrase}</strong>
                <small>
                  <button type="button" className="builder__link" data-testid="see-all-screens" onClick={() => setSeeAllOpen(true)}>
                    See all →
                  </button>
                </small>
              </span>
              {targets.exceptions.map(screen => (
                <span key={screen.screenId} className="builder__chip builder__chip--taken" data-testid="screen-exception">
                  <strong>{screen.screenName}</strong>
                  <small>another menu now</small>
                </span>
              ))}
            </>
          )}
        </div>

        <div className="builder__publish-right">
          {blocked ? (
            <span className="builder__blocked" data-testid="publish-blocked">
              {blocked}
            </span>
          ) : null}
          {data.draftCount > 0 ? (
            <button type="button" className="builder__link" data-testid="review-first" onClick={() => setReviewOpen(true)}>
              Review first
            </button>
          ) : null}
          {data.draftCount > 0 ? (
            <button
              type="button"
              className="builder__publish-button"
              data-testid="publish"
              disabled={busy || Boolean(blocked)}
              onClick={() => void publish()}
            >
              {publishLabel(data.draftCount)}
            </button>
          ) : null}
        </div>
      </footer>

      {/*
        The app's shared pattern, not a second one: success is a polite, dismissible,
        time-bounded toast; an error stays put until it is dealt with. A private
        toast here would be one more thing to keep in step with the other five.
      */}
      {notice ? (
        <div data-testid="builder-notice">
          <TransientFeedback message={notice} onDismiss={() => setNotice(undefined)} />
        </div>
      ) : null}
      {error ? (
        <p className="builder__error" role="alert" data-testid="builder-error">
          {error}
        </p>
      ) : null}

      {confirmDiscard ? (
        <>
          <div className="builder__scrim" onClick={() => setConfirmDiscard(false)} />
          <div className="builder__dialog" role="dialog" aria-modal="true" aria-labelledby="discard-title" data-testid="discard-dialog">
            <h2 id="discard-title">Discard {draftPhrase(data.draftCount).replace(" not on your screens", "")}?</h2>
            <p>This clears everything waiting on this menu. It can&apos;t be undone.</p>
            <p className="builder__dialog-note">Your screens keep showing what they are showing now.</p>
            <div className="builder__dialog-actions">
              <button type="button" className="action-secondary" onClick={() => setConfirmDiscard(false)}>
                Keep them
              </button>
              <button type="button" className="builder__quiet-danger" data-testid="confirm-discard" onClick={() => void discard()}>
                Discard
              </button>
            </div>
          </div>
        </>
      ) : null}

      {themePickerOpen ? (
        <>
          <div className="builder__scrim" onClick={() => setThemePickerOpen(false)} />
          <div className="builder__dialog" role="dialog" aria-modal="true" aria-labelledby="theme-title" data-testid="theme-picker">
            <h2 id="theme-title">Menu themes</h2>
            {themes && themes.length > 0 ? (
              <ul>
                {themes.map(theme => (
                  <li key={theme.key}>{theme.name}</li>
                ))}
              </ul>
            ) : (
              <p data-testid="theme-empty">
                You haven&apos;t built a look for your menus yet. When you do, it will show up here to attach.
              </p>
            )}
            <div className="builder__dialog-actions">
              <button type="button" className="action-secondary" onClick={() => setThemePickerOpen(false)}>
                Close
              </button>
            </div>
          </div>
        </>
      ) : null}

      {seeAllOpen ? (
        <>
          <div className="builder__scrim" onClick={() => setSeeAllOpen(false)} />
          <div className="builder__dialog" role="dialog" aria-modal="true" aria-labelledby="screens-title" data-testid="see-all-dialog">
            <h2 id="screens-title">Screens showing this menu</h2>
            <ul className="builder__screen-list">
              {screens
                .filter(screen => (data.screenIds ?? []).includes(screen.screenId))
                .map(screen => (
                  <li key={screen.screenId}>
                    <strong>{screen.screenName}</strong>
                    <span>{screen.menuId === menuId ? "showing this menu" : "another menu now"}</span>
                  </li>
                ))}
            </ul>
            <div className="builder__dialog-actions">
              <button type="button" className="action-secondary" onClick={() => setSeeAllOpen(false)}>
                Close
              </button>
            </div>
          </div>
        </>
      ) : null}

      {reviewOpen ? (
        <>
          <div className="builder__scrim" onClick={() => setReviewOpen(false)} />
          <div className="builder__dialog" role="dialog" aria-modal="true" aria-labelledby="review-title" data-testid="review-dialog">
            <h2 id="review-title">{draftPhrase(data.draftCount, { neverPublished: data.publishedVersion === null })}</h2>
            <p>Exactly what publishing will send to your screens — nothing more.</p>
            <ul className="builder__screen-list" data-testid="review-list">
              {data.changes.map((change, index) => (
                <li key={`${change.targetKind}-${change.targetId}-${change.field}-${index}`}>
                  <strong>{changeSentence(change, board)}</strong>
                  <span>{change.beforeValue === null ? "new" : change.afterValue === null ? "removed" : "changed"}</span>
                </li>
              ))}
            </ul>
            <div className="builder__dialog-actions">
              <button type="button" className="action-secondary" onClick={() => setReviewOpen(false)}>
                Close
              </button>
            </div>
          </div>
        </>
      ) : null}

      {historyOpen ? (
        <>
          <div className="builder__scrim" onClick={() => setHistoryOpen(false)} />
          <div className="builder__dialog" role="dialog" aria-modal="true" aria-labelledby="history-title" data-testid="history-dialog">
            <h2 id="history-title">Go back to…</h2>
            <p>
              Going back produces a draft against your screens. It never publishes on its own — you still decide when
              your screens change.
            </p>
            <ul className="builder__screen-list" data-testid="history-list">
              {(history ?? [])
                .filter(entry => entry.kind === "published" && entry.version !== null)
                .map(entry => (
                  <li key={entry.version}>
                    <strong>
                      {venueTime(entry.occurredUtc, venueTimezone)}
                      {entry.author ? ` by ${entry.author}` : ""}
                    </strong>
                    <button
                      type="button"
                      className="builder__link"
                      data-testid="go-back-to-version"
                      onClick={() =>
                        void run(async () => {
                          const result = await goBackToMenuVersion(configuration, apiKey, menuId, entry.version!);
                          setHistoryOpen(false);
                          undoStack.current = [];
                          redoStack.current = [];
                          setHistoryDepth({ undo: 0, redo: 0 });
                          setNotice(
                            result.replacedChangeCount > 0
                              ? `Back to that version. ${result.replacedChangeCount} change${
                                  result.replacedChangeCount === 1 ? "" : "s"
                                } you had waiting were replaced.`
                              : "Back to that version. Publish when you want your screens to follow."
                          );
                        })
                      }
                    >
                      Go back to this
                    </button>
                  </li>
                ))}
              {(history ?? []).filter(entry => entry.kind === "published").length === 0 ? (
                <li data-testid="history-empty">
                  <strong>Nothing to go back to yet</strong>
                  <span>this menu has not been published</span>
                </li>
              ) : null}
            </ul>
            <div className="builder__dialog-actions">
              <button type="button" className="action-secondary" onClick={() => setHistoryOpen(false)}>
                Close
              </button>
            </div>
          </div>
        </>
      ) : null}

      {findOpen ? (
        <>
          <div className="builder__scrim" onClick={() => setFindOpen(false)} />
          <div className="builder__find" role="dialog" aria-modal="true" aria-label="Find an item on this board" data-testid="find-dialog">
            <input
              autoFocus
              value={findQuery}
              placeholder="Find an item on this board"
              aria-label="Find an item on this board"
              data-testid="find-input"
              onChange={event => setFindQuery(event.target.value)}
            />
            <ul data-testid="find-results">
              {findOnBoard(board, findQuery).map(hit => (
                <li key={hit.itemId}>
                  <button
                    type="button"
                    data-testid="find-result"
                    onClick={() => {
                      setPlace(current => ({ ...current, sectionId: hit.sectionId, selectedItemId: hit.itemId }));
                      setFindOpen(false);
                      setFindQuery("");
                    }}
                  >
                    <strong>{hit.name}</strong>
                    <span>{hit.sectionName}</span>
                  </button>
                </li>
              ))}
            </ul>
          </div>
        </>
      ) : null}
    </div>
  );
}
