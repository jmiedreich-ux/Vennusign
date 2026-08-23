import { useCallback, useEffect, useMemo, useRef, useState, type CSSProperties, type FormEvent, type RefObject } from "react";

import {
  BackOfficeApiError,
  MenuActionRefused,
  loadBackOfficeSession,
  addMenuSection,
  addMenuPage,
  deleteMenuPage,
  duplicateMenuPage,
  deleteMenuSection,
  discardMenuDraft,
  loadBuilderBoard,
  goBackToMenuVersion,
  loadMenuAvailability,
  loadMenuHistory,
  loadPageHistory,
  loadMenuThemes,
  loadMenuAssignments,
  loadScreensShowing,
  moveMenuItem,
  saveMenuPageAssignments,
  placeMenuItem,
  publishMenu,
  renameMenu,
  renameMenuPage,
  reorderMenuPages,
  removeMenuItem,
  renameMenuSection,
  reorderMenuItems,
  transitionMenuItemPlacement,
  reorderMenuSections,
  searchLibraryItems,
  setItemAvailability,
  updateMenuItemValues,
  type BuilderBoard,
  type LibraryItem,
  type MenuAvailability,
  type MenuHistoryEntry,
  type MenuPageAssignment,
  type MenuScreenShowing
} from "./api";
import type { BackOfficeConfiguration } from "./config";
import SkyIcon from "./SkyIcon";
import TransientFeedback from "./TransientFeedback";
import { BoardRenderer } from "../../board-engine/BoardRenderer";
import { BoardFrame } from "../../board-engine/BoardFrame";
import { boardLogicalWidth } from "../../board-engine/boardScale.mjs";
import {
  availabilityLine,
  availabilityImpactNotice,
  canDiscardDraft,
  canvasBoard,
  changeSentence,
  changeValues,
  changedItemsMissingPrice,
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
import { calculateBoardCapacity } from "./boardCapacity.mjs";
import "../../board-engine/board-engine.css";
import "./menu-builder.css";
import { hasMenuCapability, type MenuCapabilityOverrides } from "./menuCapabilities";

type Props = {
  configuration: BackOfficeConfiguration;
  apiKey: string;
  menuId: string;
  venueTimezone: string;
  startBlank?: boolean;
  onBack: () => void;
  /**
   * Hands a freshly accepted venue access token back to the application, so
   * signing back in from the builder signs the whole back office back in rather
   * than leaving one screen holding a credential nothing else knows about (Q199).
   */
  onAccessTokenChange?: (token: string) => void;
  capabilityOverrides?: MenuCapabilityOverrides;
};

function screenState(screen: MenuScreenShowing, now = Date.now()) {
  const raw = (screen.status || "Never paired").toLowerCase();
  if (raw.includes("never")) return { key: "unpaired", text: "Never paired" };
  const lastSeen = screen.lastSeenUtc ? new Date(screen.lastSeenUtc) : null;
  const elapsedMinutes = lastSeen && Number.isFinite(lastSeen.getTime()) ? Math.max(0, Math.floor((now - lastSeen.getTime()) / 60_000)) : null;
  const elapsed = elapsedMinutes === null ? null : elapsedMinutes < 60 ? `${elapsedMinutes}m` : elapsedMinutes < 1_440 ? `${Math.floor(elapsedMinutes / 60)}h` : `${Math.floor(elapsedMinutes / 1_440)}d`;
  if (raw === "online" && elapsedMinutes !== null && elapsedMinutes >= 5) return { key: "stale", text: `Stale · no reply for ${elapsed}` };
  if (raw.includes("offline")) return { key: "offline", text: elapsed ? `Offline · last seen ${elapsed} ago` : "Offline" };
  return { key: "online", text: "Online" };
}

type SaveState = "clean" | "saving" | "failed";

/**
 * A change that was made and has not reached the server yet.
 *
 * It is kept rather than dropped: Q197 and Q199 both turn on the same promise —
 * an edit you made is still an edit, whatever the network or the session did
 * afterwards, and the surface never claims otherwise.
 */
type PendingWrite = {
  action: () => Promise<void>;
  undo?: UndoStep;
  describe: string;
  onSuccess?: () => void;
  /**
   * The caller already drew this change, so there is nothing to wait for and
   * nothing to re-read. See `run`'s note on client-first writes.
   */
  drawn?: boolean;
};

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
      setScale(current => (current === next ? current : next));
      const nextHeight = inner.current.scrollHeight * next;
      setHeight(current => (current === nextHeight ? current : nextHeight));
    };
    measure();
    if (typeof ResizeObserver === "undefined") return;
    const observer = new ResizeObserver(measure);
    if (outer.current) observer.observe(outer.current);
    if (inner.current) observer.observe(inner.current);
    return () => observer.disconnect();
  }, []);

  return (
    <div
      className="builder__stage"
      ref={outer}
      style={{ ...(height ? { height: `${height}px` } : {}), ["--board-scale" as string]: scale }}
    >
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
const panelMemoryKey = "vennusign.menu.builder.panels";

type PanelPreferences = { leftCollapsed: boolean; rightCollapsed: boolean };

function readPanelPreferences(): PanelPreferences {
  try {
    const stored = JSON.parse(localStorage.getItem(panelMemoryKey) ?? "null") as Partial<PanelPreferences> | null;
    return {
      leftCollapsed: stored?.leftCollapsed === true,
      rightCollapsed: stored?.rightCollapsed === true
    };
  } catch {
    return { leftCollapsed: false, rightCollapsed: false };
  }
}

function PanelCollapseButton({
  panel,
  collapsed,
  onClick
}: {
  panel: "sections and history" | "item";
  collapsed: boolean;
  onClick: () => void;
}) {
  const side = panel === "sections and history" ? "left" : "right";
  const action = collapsed ? "Expand" : "Collapse";

  return (
    <button
      type="button"
      className={`builder__panel-collapse builder__panel-collapse--${side}`}
      aria-expanded={!collapsed}
      aria-label={`${action} ${panel} panel`}
      title={`${action} ${panel} panel`}
      data-testid={`${side}-panel-toggle`}
      onClick={onClick}
    >
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d={side === "left" ? "M14 6l-6 6 6 6" : "M10 6l6 6-6 6"} />
      </svg>
    </button>
  );
}

/**
 * Moves focus into a dialog, keeps Tab inside it, and hands focus back on close.
 *
 * Without this the dialogs announced themselves as modal and were not: focus
 * stayed on the trigger, and one Tab reached the Publish button *behind the
 * scrim* — so the most likely accidental keyboard action on this surface was
 * publishing to the screens. `aria-modal` is a promise to assistive technology,
 * and it was being made without being kept.
 */
function useDialogFocus(open: boolean) {
  const dialog = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    const node = dialog.current;
    if (!node) return;

    const returnTo = document.activeElement as HTMLElement | null;
    const focusable = () =>
      [...node.querySelectorAll<HTMLElement>(
        'a[href], button:not([disabled]), input:not([disabled]), textarea:not([disabled]), select:not([disabled]), [tabindex]:not([tabindex="-1"])'
      )].filter(element => element.offsetParent !== null);

    // The heading first, so a screen reader hears what this dialog is before it
    // hears the first thing it can do.
    const heading = node.querySelector<HTMLElement>("h2");
    if (heading) {
      heading.tabIndex = -1;
      heading.focus();
    } else {
      focusable()[0]?.focus();
    }

    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key !== "Tab") return;
      const items = focusable();
      if (items.length === 0) return;
      const first = items[0];
      const last = items[items.length - 1];
      const active = document.activeElement;

      if (event.shiftKey && (active === first || active === heading || !node.contains(active))) {
        event.preventDefault();
        last.focus();
      } else if (!event.shiftKey && active === last) {
        event.preventDefault();
        first.focus();
      } else if (!node.contains(active)) {
        event.preventDefault();
        first.focus();
      }
    };

    node.addEventListener("keydown", onKeyDown);
    document.addEventListener("keydown", onKeyDown, true);
    return () => {
      node.removeEventListener("keydown", onKeyDown);
      document.removeEventListener("keydown", onKeyDown, true);
      returnTo?.focus?.();
    };
  }, [open]);

  return dialog;
}

/**
 * The sign-back-in prompt (Q199).
 *
 * A shift is long and a session is not. When one expires mid-edit the change has
 * already been made — so this asks for the sign-in back and names what it is
 * holding, instead of reporting a failure and leaving a bartender to guess
 * whether the 86 they just flipped actually took.
 */
function SignBackIn({
  configuration,
  holding,
  onSignedIn,
  onDismiss
}: {
  configuration: BackOfficeConfiguration;
  holding: string;
  onSignedIn: (token: string) => void;
  onDismiss: () => void;
}) {
  const dialog = useDialogFocus(true);
  const [checking, setChecking] = useState(false);
  const [problem, setProblem] = useState<string>();

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const token = String(new FormData(event.currentTarget).get("accessToken") ?? "").trim();
    if (!token) return;
    setChecking(true);
    setProblem(undefined);
    try {
      // Proved against the API before it is trusted, so a wrong token is refused
      // here rather than by silently failing the resend behind the dialog.
      await loadBackOfficeSession(configuration, token);
      onSignedIn(token);
    } catch (failure) {
      setProblem(failure instanceof Error ? failure.message : "That token was not accepted.");
      setChecking(false);
    }
  };

  return (
    <>
      {/* No click-away: dismissing is a decision, and "Not now" says so. */}
      <div className="builder__scrim" />
      <div
        className="builder__dialog"
        role="dialog"
        aria-modal="true"
        aria-labelledby="sign-back-in-title"
        data-testid="sign-back-in-dialog"
        ref={dialog}
      >
        <h2 id="sign-back-in-title">Your sign-in expired</h2>
        <p>
          <strong>{holding}</strong> hasn&apos;t reached your screens yet — it is still here. Sign back in and it sends
          straight away.
        </p>
        <form onSubmit={submit}>
          <label className="builder__dialog-label" htmlFor="builder-access-token">
            Venue access token
          </label>
          <input
            id="builder-access-token"
            name="accessToken"
            type="password"
            autoComplete="current-password"
            data-testid="sign-back-in-token"
            required
          />
          {problem ? (
            <p className="builder__dialog-problem" role="alert">
              {problem}
            </p>
          ) : null}
          <div className="builder__dialog-actions">
            <button type="button" className="action-secondary" onClick={onDismiss}>
              Not now
            </button>
            <button type="submit" className="action-primary" data-testid="sign-back-in-submit" disabled={checking}>
              {checking ? "Signing in…" : "Sign in and send"}
            </button>
          </div>
        </form>
      </div>
    </>
  );
}

/**
 * The menu builder.
 *
 * Four columns: a section rail that navigates and nothing else, a canvas that IS
 * the preview, an inspector of four controls, and the publish bar. Every edit
 * writes working state; the draft count follows on its own, because it is the
 * computed difference from what the screens are showing.
 */
export default function MenuBuilder({
  configuration,
  apiKey,
  menuId,
  venueTimezone,
  startBlank = false,
  onBack,
  onAccessTokenChange,
  capabilityOverrides
}: Props) {
  const canManagePages = hasMenuCapability("page-management", capabilityOverrides);
  const canAssignScreens = hasMenuCapability("screen-assignment", capabilityOverrides);
  const canViewCapacity = hasMenuCapability("capacity", capabilityOverrides);
  const canViewHistory = hasMenuCapability("history", capabilityOverrides);
  /*
   * The credential every write reads, at the moment it is sent.
   *
   * A held change replayed after signing back in must go with the NEW token: the
   * closure that captured the old one would 401 forever, which is the loop Q199
   * exists to prevent. Read through `credential()` rather than the `apiKey` prop
   * for anything that talks to the API.
   */
  const currentKey = useRef(apiKey);
  currentKey.current = apiKey;
  const credential = useCallback(() => currentKey.current, []);

  const [data, setData] = useState<BuilderBoard>();
  const [availability, setAvailability] = useState<MenuAvailability[]>([]);
  const [screens, setScreens] = useState<MenuScreenShowing[]>([]);
  const [assignments, setAssignments] = useState<MenuPageAssignment[]>([]);
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
  const [pageHistory, setPageHistory] = useState<MenuHistoryEntry[]>();
  const [pageHistoryError, setPageHistoryError] = useState(false);
  const [editingMenuName, setEditingMenuName] = useState(false);
  const [menuNameDraft, setMenuNameDraft] = useState("");
  const [viewingOpen, setViewingOpen] = useState(false);
  const [panelPreferences, setPanelPreferences] = useState(readPanelPreferences);
  const [assignmentOpen, setAssignmentOpen] = useState(false);
  const [fitOpen, setFitOpen] = useState(false);
  const [assignmentDraft, setAssignmentDraft] = useState<Record<string, "replace" | "rotate" | "remove">>({});
  const [assignmentChoiceScreenId, setAssignmentChoiceScreenId] = useState<string | null>(null);
  const [assignmentChoicePageId, setAssignmentChoicePageId] = useState<string | null>(null);
  const [assignmentAddingScreenId, setAssignmentAddingScreenId] = useState<string | null>(null);
  const [viewingScreenId, setViewingScreenId] = useState<string | null>(null);
  const pageHistoryRequest = useRef(0);
  const undoStack = useRef<UndoStep[]>([]);
  const redoStack = useRef<UndoStep[]>([]);
  const [historyDepth, setHistoryDepth] = useState({ undo: 0, redo: 0 });
  const [confirmDelete, setConfirmDelete] = useState<{ sectionId: string; name: string; items: number; destinationSectionId: string; mode: "move" | "delete" } | null>(null);
  const [activePageId, setActivePageId] = useState<string | null>(null);
  const [addingPage, setAddingPage] = useState(false);
  const [newPageName, setNewPageName] = useState("");
  const [pageMenuId, setPageMenuId] = useState<string | null>(null);
  const pageActionsRef = useRef<HTMLSpanElement>(null);
  const [draggedPageId, setDraggedPageId] = useState<string | null>(null);
  const [editingPage, setEditingPage] = useState<{ pageId: string; name: string } | null>(null);
  const [editingRailSection, setEditingRailSection] = useState<{ sectionId: string; name: string } | null>(null);
  const [draggedSectionId, setDraggedSectionId] = useState<string | null>(null);
  const [confirmPageDelete, setConfirmPageDelete] = useState<{ pageId: string; name: string; destinationPageId: string; sectionCount: number; mode: "move" | "delete" } | null>(null);
  const [confirmItemRemove, setConfirmItemRemove] = useState(false);
  const [confirmPublishMissingPrice, setConfirmPublishMissingPrice] = useState(false);

  const discardRef = useDialogFocus(confirmDiscard);
  const publishMissingPriceRef = useDialogFocus(confirmPublishMissingPrice);
  const deleteRef = useDialogFocus(Boolean(confirmDelete));
  const themeRef = useDialogFocus(themePickerOpen);
  const seeAllRef = useDialogFocus(seeAllOpen);
  const reviewRef = useDialogFocus(reviewOpen);
  const historyRef = useDialogFocus(historyOpen);
  const pageDeleteRef = useDialogFocus(Boolean(confirmPageDelete));
  const fitRef = useDialogFocus(fitOpen);
  const itemRemoveRef = useDialogFocus(confirmItemRemove);
  const closeAssignmentChoice = () => {
    const trigger = assignmentChoiceScreenId;
    setAssignmentChoiceScreenId(null);
    setAssignmentChoicePageId(null);
    if (trigger) window.setTimeout(() => document.querySelector<HTMLButtonElement>(`[data-testid="add-screen-page"][data-screen-id="${trigger}"]`)?.focus(), 0);
  };

  const board = data?.board;
  const pages = board?.pages ?? [];

  const refreshPageHistory = useCallback(async (pageId: string) => {
    const request = ++pageHistoryRequest.current;
    setPageHistoryError(false);
    try {
      const entries = await loadPageHistory(configuration, credential(), menuId, pageId);
      if (pageHistoryRequest.current === request) setPageHistory(entries);
    } catch {
      if (pageHistoryRequest.current === request) setPageHistoryError(true);
    }
  }, [configuration, credential, menuId]);

  useEffect(() => {
    if (pages.length === 0) return;
    if (!activePageId || !pages.some(page => page.pageId === activePageId)) setActivePageId(pages[0].pageId);
  }, [activePageId, pages]);
  useEffect(() => {
    if (!pageMenuId) return;
    const closePageMenu = (event: PointerEvent) => {
      if (!pageActionsRef.current?.contains(event.target as Node)) setPageMenuId(null);
    };
    document.addEventListener("pointerdown", closePageMenu);
    return () => document.removeEventListener("pointerdown", closePageMenu);
  }, [pageMenuId]);
  useEffect(() => {
    if (!board || !activePageId) return;
    const pageSections = sectionsOf(board).filter(section => section.pageId === activePageId);
    if (!pageSections.some(section => section.sectionId === place.sectionId)) {
      setPlace(current => ({ ...current, sectionId: pageSections[0]?.sectionId ?? null, selectedItemId: null }));
    }
  }, [activePageId, board, place.sectionId]);
  useEffect(() => {
    if (!activePageId) {
      pageHistoryRequest.current += 1;
      setPageHistory([]);
      return;
    }
    setPageHistory(undefined);
    void refreshPageHistory(activePageId);
  }, [activePageId, data, refreshPageHistory]);
  const unavailableIds = useMemo(
    () => availability.filter(state => !state.isAvailable).map(state => state.itemId),
    [availability]
  );

  const refresh = useCallback(async () => {
    const [next, states, showing, assigned] = await Promise.all([
      loadBuilderBoard(configuration, credential(), menuId),
      loadMenuAvailability(configuration, credential()),
      loadScreensShowing(configuration, credential()),
      loadMenuAssignments(configuration, credential())
    ]);
    setData(next);
    setAvailability(states);
    setScreens(showing);
    setAssignments(assigned);
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
        if (startBlank) setAddSectionId(sectionsOf(next.board)[0]?.sectionId ?? null);
      })
      .catch(() => {
        if (!cancelled) setError("This menu could not be opened. Check your connection and try again.");
      });
    return () => {
      cancelled = true;
    };
  }, [menuId, refresh, startBlank]);

  useEffect(() => {
    sessionStorage.setItem(placeMemoryKey(menuId), JSON.stringify(place));
  }, [menuId, place]);

  useEffect(() => {
    try {
      localStorage.setItem(panelMemoryKey, JSON.stringify(panelPreferences));
    } catch {
      // Storage can be unavailable in privacy modes. The controls still work
      // for this visit; persistence is the enhancement, never a blocker.
    }
  }, [panelPreferences]);

  const selected = useMemo(() => findItem(board, place.selectedItemId), [board, place.selectedItemId]);
  const selectedAvailability = useMemo(
    () => availability.find(state => state.itemId === place.selectedItemId) ?? null,
    [availability, place.selectedItemId]
  );

  /**
   * Runs a write and keeps the byline honest about it. A failure never clears the
   * change from the surface: it flips the byline amber, holds the change, retries
   * it, and blocks Publish until the queue is confirmed (Q197).
   */
  const writes = useRef<Promise<unknown>>(Promise.resolve());
  const held = useRef<PendingWrite | null>(null);
  const retryTimer = useRef<number>();
  const retryRound = useRef(0);
  const deliverRef = useRef<(entry: PendingWrite) => Promise<boolean>>();
  /** What is being held for a sign-in, and whether the prompt is on screen. */
  const [signBackIn, setSignBackIn] = useState<string | null>(null);
  const [signInDeferred, setSignInDeferred] = useState(false);

  useEffect(() => () => window.clearTimeout(retryTimer.current), []);

  const scheduleRetry = useCallback(() => {
    window.clearTimeout(retryTimer.current);
    /*
     * 1s, 2s, 4s, then every 8s for as long as it takes.
     *
     * It never gives up, because giving up IS the terminal error Q197 exists to
     * remove: the change stays held and Publish stays shut until it lands. The
     * first wait is short on purpose — most of these are one dropped request.
     */
    const round = Math.min(retryRound.current, 3);
    retryRound.current += 1;
    retryTimer.current = window.setTimeout(() => {
      const entry = held.current;
      if (entry) void deliverRef.current?.(entry);
    }, 1000 * 2 ** round);
  }, []);

  const deliver = useCallback(
    async (entry: PendingWrite) => {
      // A drawn write does not lock the surface. `busy` exists so nobody acts on a
      // board whose state is unknown - but a write the caller already drew has a
      // known state, and ordering is guaranteed by the write queue below rather
      // than by freezing the page. Locking these as well is what turned one stalled
      // request into ten minutes of dead controls; the request timeout in
      // api.ts's venueFetch is the other half of that fix.
      if (!entry.drawn) setBusy(true);
      setSaveState("saving");
      setError(undefined);
      // TEMP (#800): console timing on this shared pipeline, ahead of the real
      // observability project (#774). Delete once that ships, or once add-item is
      // confirmed fixed and this stops earning its keep.
      const perfStart = performance.now();
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
      const mine = writes.current.then(entry.action, entry.action);
      writes.current = mine.catch(() => undefined);
      try {
        await mine;
        const perfAfterAction = performance.now();
        // Every write reconciles, drawn or not. Skipping this for drawn writes was
        // wrong in a way the drawn frame hides: draftCount is computed server-side,
        // so without the re-read the builder still believed "Everything is on your
        // screens" after a reorder it had just drawn - and the Publish button, which
        // only renders when draftCount > 0, never appeared. The change was on screen
        // and on the server and could not be published.
        //
        // The lag fix never needed this to go: it comes from drawing BEFORE the
        // write, so by the time this runs the frame is already up and nobody is
        // waiting on it.
        await refresh();
        const perfAfterRefresh = performance.now();
        console.info("[perf:deliver]", {
          describe: entry.describe,
          drawn: entry.drawn ?? false,
          actionMs: Math.round(perfAfterAction - perfStart),
          refreshMs: Math.round(perfAfterRefresh - perfAfterAction),
          totalMs: Math.round(perfAfterRefresh - perfStart)
        });
        held.current = null;
        retryRound.current = 0;
        setSignBackIn(null);
        setSaveState("clean");
        entry.onSuccess?.();
        if (entry.undo) {
          undoStack.current = [...undoStack.current.slice(-49), entry.undo];
          redoStack.current = [];
          setHistoryDepth({ undo: undoStack.current.length, redo: 0 });
        }
      } catch (failure) {
        if (failure instanceof MenuActionRefused) {
          /*
           * The server reached a decision and said no. Retrying repeats the same
           * refusal word for word, so this holds nothing and blocks nothing — the
           * queue IS confirmed; it simply does not contain this change. The board
           * re-reads, because a refusal means the server's state is not the state
           * this screen assumed when it asked.
           */
          held.current = null;
          retryRound.current = 0;
          await refresh().catch(() => undefined);
          setSaveState("clean");
          setError(failure.message);
          return false;
        }

        // Anything else is a change that never landed. It is kept, not dropped.
        held.current = entry;
        setSaveState("failed");
        setError(undefined);

        if (failure instanceof BackOfficeApiError && failure.status === 401) {
          // Q199: the session went, not the change. Ask for the sign-in back.
          setSignBackIn(entry.describe);
          setSignInDeferred(false);
          return false;
        }
        scheduleRetry();
        return false;
      } finally {
        if (!entry.drawn) setBusy(false);
      }
      return true;
    },
    [refresh, scheduleRetry]
  );

  useEffect(() => {
    deliverRef.current = deliver;
  }, [deliver]);

  /**
   * Client first, server last.
   *
   * `drawn` says the caller has already put the change on screen, because it could
   * compute the result itself. Reordering is the plain case: the new order IS the
   * request body, so waiting for the PUT and then re-reading the board asked the
   * server to confirm a list this component authored - two sequential round trips,
   * measured at 3,981 ms for one section drag on B1, to render a permutation
   * already in hand. Writes still go one at a time and in order; that guarantee is
   * about the order saves land in, not about when we are allowed to paint.
   */
  const run = useCallback(
    (action: () => Promise<void>, undoStep?: UndoStep, onSuccess?: () => void, drawn = false) => {
      window.clearTimeout(retryTimer.current);
      retryRound.current = 0;
      return deliver({ action, undo: undoStep, describe: undoStep?.describe ?? "Your last change", onSuccess, drawn });
    },
    [deliver]
  );

  /**
   * Signed back in — so send what was being held (Q199).
   *
   * The token reaches the whole application, not just this screen: a builder
   * quietly holding a credential nothing else knows about would 401 the moment
   * you navigated away, which is the same terminal error one step later.
   */
  const resumeAfterSignIn = useCallback(
    (token: string) => {
      onAccessTokenChange?.(token);
      currentKey.current = token;
      setSignBackIn(null);
      retryRound.current = 0;
      const entry = held.current;
      if (entry) void deliver(entry);
    },
    [deliver, onAccessTokenChange]
  );

  const commitNewPage = async () => {
    const name = newPageName.trim();
    setAddingPage(false);
    setNewPageName("");
    if (!name) return;
    let created: { pageId: string } | undefined;
    await run(async () => { created = await addMenuPage(configuration, credential(), menuId, name); });
    if (created) setActivePageId(created.pageId);
  };

  const commitPageRename = async () => {
    const edit = editingPage;
    setEditingPage(null);
    if (!edit) return;
    const name = edit.name.trim();
    const current = pages.find(page => page.pageId === edit.pageId)?.name;
    if (!name || name === current) return;
    await run(() => renameMenuPage(configuration, credential(), menuId, edit.pageId, name));
  };

  const duplicatePage = async (pageId: string) => {
    setPageMenuId(null);
    let created: { pageId: string } | undefined;
    await run(async () => { created = await duplicateMenuPage(configuration, credential(), menuId, pageId); });
    if (created) setActivePageId(created.pageId);
  };

  const dropPage = async (targetPageId: string) => {
    const sourcePageId = draggedPageId;
    setDraggedPageId(null);
    if (!sourcePageId || sourcePageId === targetPageId) return;
    const ordered = pages.map(page => page.pageId);
    const from = ordered.indexOf(sourcePageId);
    const to = ordered.indexOf(targetPageId);
    if (from < 0 || to < 0) return;
    ordered.splice(to, 0, ordered.splice(from, 1)[0]);
    const before = pages.map(page => page.pageId);
    drawPageOrder(ordered);
    await run(
      () => reorderMenuPages(configuration, credential(), menuId, ordered),
      {
        describe: "Move page",
        undo: () => { drawPageOrder(before); return reorderMenuPages(configuration, credential(), menuId, before); },
        redo: () => { drawPageOrder(ordered); return reorderMenuPages(configuration, credential(), menuId, ordered); }
      },
      undefined,
      true
    );
  };

  const removePage = async (pageId: string, destinationPageId?: string, deleteSections = false) => {
    setPageMenuId(null);
    await run(
      () => deleteMenuPage(configuration, credential(), menuId, pageId, destinationPageId || undefined, deleteSections),
      undefined,
      () => setConfirmPageDelete(null)
    );
  };

  const saveAssignments = async () => {
    const changes = Object.entries(assignmentDraft);
    await run(() => saveMenuPageAssignments(
      configuration,
      credential(),
      menuId,
      changes.map(([key, mode]) => {
        const [screenId, pageId] = key.split(":");
        return { screenId, pageId, mode };
      })
    ), undefined, () => {
      setAssignmentOpen(false);
      setAssignmentDraft({});
      setAssignmentAddingScreenId(null);
    });
  };

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
        created = await addMenuSection(configuration, credential(), menuId, name, activePageId);
      },
      {
        describe: `Add section "${name}"`,
        undo: async () => {
          if (created) await deleteMenuSection(configuration, credential(), menuId, created.sectionId, undefined, true);
        },
        redo: async () => {
          created = await addMenuSection(configuration, credential(), menuId, name, activePageId);
        }
      }
    );
    if (created) setPlace(current => ({ ...current, sectionId: created!.sectionId, selectedItemId: null }));
  };

  /**
   * Draw a reorder now, from the order this component just computed.
   *
   * Rendering is driven by sortOrder, so applying the new ranks locally is the whole
   * change - there is nothing the server can add to a permutation we authored. If it
   * refuses, deliver() re-reads and the truth replaces this; if the call fails, the
   * write is held and retried, so the frame stays right rather than flickering back.
   */
  const drawSectionOrder = (ids: string[]) =>
    setData(current => {
      if (!current?.board) return current;
      const rank = new Map(ids.map((id, index) => [id, index]));
      return {
        ...current,
        board: {
          ...current.board,
          sections: current.board.sections.map(section =>
            rank.has(section.sectionId) ? { ...section, sortOrder: rank.get(section.sectionId)! } : section)
        }
      };
    });

  const drawItemOrder = (sectionId: string, ids: string[]) =>
    setData(current => {
      if (!current?.board) return current;
      const rank = new Map(ids.map((id, index) => [id, index]));
      return {
        ...current,
        board: {
          ...current.board,
          sections: current.board.sections.map(section =>
            section.sectionId !== sectionId ? section : {
              ...section,
              items: (section.items ?? []).map(item =>
                rank.has(item.itemId) ? { ...item, sortOrder: rank.get(item.itemId)! } : item)
            })
        }
      };
    });

  const drawPageOrder = (ids: string[]) =>
    setData(current => {
      if (!current?.board) return current;
      const rank = new Map(ids.map((id, index) => [id, index]));
      return {
        ...current,
        board: {
          ...current.board,
          pages: (current.board.pages ?? []).map(page =>
            rank.has(page.pageId) ? { ...page, sortOrder: rank.get(page.pageId)! } : page)
        }
      };
    });

  const moveSection = async (from: number, to: number) => {
    const ids = sectionsOf(board).filter(section => !activePageId || section.pageId === activePageId).map(section => section.sectionId);
    const next = reorder(ids, from, to);
    if (next.join() === ids.join()) return;
    drawSectionOrder(next);
    await run(
      () => reorderMenuSections(configuration, credential(), menuId, next),
      {
        describe: "Move section",
        undo: () => { drawSectionOrder(ids); return reorderMenuSections(configuration, credential(), menuId, ids); },
        redo: () => { drawSectionOrder(next); return reorderMenuSections(configuration, credential(), menuId, next); }
      },
      undefined,
      true
    );
  };

  const deleteSection = async (sectionId: string, name: string, destinationSectionId?: string, deletePlacements = false) => {
    const sections = sectionsOf(board).filter(section => !activePageId || section.pageId === activePageId);
    const deletedIndex = sections.findIndex(section => section.sectionId === sectionId);
    const nextSection = sections[deletedIndex - 1] ?? sections[deletedIndex + 1] ?? null;
    await run(async () => {
      const outcome = await deleteMenuSection(configuration, credential(), menuId, sectionId, destinationSectionId, deletePlacements);
      setNotice(outcome.movedItemCount > 0
        ? `${outcome.movedItemCount} ${outcome.movedItemCount === 1 ? "item was" : "items were"} moved.`
        : releasedPhrase(outcome.releasedItemCount));
      setPlace(current =>
        current.sectionId === sectionId
          ? { ...current, sectionId: nextSection?.sectionId ?? null, selectedItemId: null }
          : current
      );
    }, undefined, () => setConfirmDelete(null));
    // Deliberately not undoable: the section's id is gone, so an "undo" would put
    // back something that only looked the same. Saying so beats a control that
    // half works — the items are in the library and can be placed again.
    void name;
  };

  // ---- the canvas ----------------------------------------------------------

  const canvasRef = useRef<HTMLDivElement>(null);
  const [itemEdit, setItemEdit] = useState<{ itemId: string; field: "name" | "description" | "price"; value: string; box: DOMRect; typography: CSSProperties; caret: number } | null>(null);
  const itemEditRef = useRef<HTMLInputElement | HTMLTextAreaElement>(null);
  const [inspectorCue, setInspectorCue] = useState<"name" | "description" | "price" | null>(null);
  const inspectorCueTimer = useRef<number>();
  const [headingEdit, setHeadingEdit] = useState<{ sectionId: string; value: string; box: DOMRect; typography: CSSProperties } | null>(null);

  useEffect(() => () => {
    if (inspectorCueTimer.current) window.clearTimeout(inspectorCueTimer.current);
  }, []);

  useEffect(() => {
    const editor = itemEditRef.current;
    if (!editor || !itemEdit) return;
    editor.focus();
    editor.setSelectionRange(itemEdit.caret, itemEdit.caret);
  }, [itemEdit?.itemId, itemEdit?.field]);


  /**
   * Renames a section by typing over the canvas heading (Q96). The duplicate
   * field below the board was removed by the owner's acceptance ruling.
   */
  const renameSection = useCallback(
    (sectionId: string, name: string, current: string) => {
      if (!name || name === current) return;
      void run(() => renameMenuSection(configuration, credential(), menuId, sectionId, name), {
        describe: "Rename section",
        undo: () => renameMenuSection(configuration, credential(), menuId, sectionId, current),
        redo: () => renameMenuSection(configuration, credential(), menuId, sectionId, name)
      });
    },
    [configuration, credential, menuId, run]
  );

  /*
   * Dragging an item to a new place on its own section (Q103).
   *
   * Cross-section moves wait for Board view in milestone 5, and the answer names
   * the path until then: remove and re-add, two draft changes. So a drop onto a
   * different section is refused in those words rather than silently ignored — an
   * affordance that appears to work and does nothing is worse than one that says
   * what it cannot do.
   */
  type DropTarget = { itemId: string; sectionId: string; edge: "before" | "after" };
  const pointerDrag = useRef<{
    pointerId: number;
    itemId: string;
    sectionId: string;
    startX: number;
    startY: number;
    active: boolean;
    target: DropTarget | null;
  } | null>(null);
  const suppressCanvasClick = useRef(false);
  const [dropTarget, setDropTarget] = useState<DropTarget | null>(null);

  const beginItemDrag = (event: React.PointerEvent<HTMLDivElement>) => {
    if (event.button !== 0) return;
    const handle = (event.target as HTMLElement).closest<HTMLElement>('[data-testid="item-drag-handle"]');
    const row = handle?.closest<HTMLElement>("[data-item-id]");
    if (!row?.dataset.itemId || !row.dataset.sectionId) return;
    event.preventDefault();

    pointerDrag.current = {
      pointerId: event.pointerId,
      itemId: row.dataset.itemId,
      sectionId: row.dataset.sectionId,
      startX: event.clientX,
      startY: event.clientY,
      active: false,
      target: null
    };
    event.currentTarget.setPointerCapture(event.pointerId);
    const canvas = event.currentTarget;
    window.addEventListener("pointerup", release => completeItemDrag(release.pointerId, canvas, release.clientX, release.clientY), { once: true });
  };

  const trackItemDrag = (event: React.PointerEvent<HTMLDivElement>) => {
    const drag = pointerDrag.current;
    if (!drag || drag.pointerId !== event.pointerId) return;
    if (!drag.active && Math.hypot(event.clientX - drag.startX, event.clientY - drag.startY) < 5) return;

    drag.active = true;
    event.preventDefault();
    const row = [...event.currentTarget.querySelectorAll<HTMLElement>("[data-item-id]")].find(candidate => {
      const box = candidate.getBoundingClientRect();
      return event.clientX >= box.left && event.clientX <= box.right && event.clientY >= box.top && event.clientY <= box.bottom;
    });
    if (!row?.dataset.itemId || !row.dataset.sectionId) {
      const section = [...event.currentTarget.querySelectorAll<HTMLElement>("[data-section-id]")].find(candidate => {
        const box = candidate.getBoundingClientRect();
        return event.clientX >= box.left && event.clientX <= box.right && event.clientY >= box.top && event.clientY <= box.bottom;
      });
      if (section?.dataset.sectionId && itemsOf(board, section.dataset.sectionId).length === 0) {
        const target: DropTarget = { itemId: "", sectionId: section.dataset.sectionId, edge: "after" };
        drag.target = target;
        setDropTarget(target);
      } else {
        drag.target = null;
        setDropTarget(null);
      }
      return;
    }

    const box = row.getBoundingClientRect();
    const target: DropTarget = {
      itemId: row.dataset.itemId,
      sectionId: row.dataset.sectionId,
      edge: event.clientY < box.top + box.height / 2 ? "before" : "after"
    };
    drag.target = target;
    setDropTarget(current =>
      current?.itemId === target.itemId && current.sectionId === target.sectionId && current.edge === target.edge
        ? current
        : target
    );
  };

  function completeItemDrag(pointerId: number, _currentTarget: HTMLDivElement, clientX: number, clientY: number) {
    const drag = pointerDrag.current;
    if (!drag || drag.pointerId !== pointerId) return;
    if (drag.active) {
      const row = [..._currentTarget.querySelectorAll<HTMLElement>("[data-item-id]")].find(candidate => {
        const box = candidate.getBoundingClientRect();
        return clientX >= box.left && clientX <= box.right && clientY >= box.top && clientY <= box.bottom;
      });
      if (row?.dataset.itemId && row.dataset.sectionId) {
        const box = row.getBoundingClientRect();
        drag.target = { itemId: row.dataset.itemId, sectionId: row.dataset.sectionId, edge: clientY < box.top + box.height / 2 ? "before" : "after" };
      } else {
        const section = [..._currentTarget.querySelectorAll<HTMLElement>("[data-section-id]")].find(candidate => {
          const box = candidate.getBoundingClientRect();
          return clientX >= box.left && clientX <= box.right && clientY >= box.top && clientY <= box.bottom;
        });
        if (section?.dataset.sectionId && itemsOf(board, section.dataset.sectionId).length === 0) {
          drag.target = { itemId: "", sectionId: section.dataset.sectionId, edge: "after" };
        } else {
          drag.target = null;
        }
      }
    }
    pointerDrag.current = null;
    setDropTarget(null);
    if (!drag.active || !drag.target) return;

    suppressCanvasClick.current = true;
    window.setTimeout(() => {
      suppressCanvasClick.current = false;
    }, 0);

    const ids = itemsOf(board, drag.sectionId).map(item => item.itemId);
    const remaining = ids.filter(id => id !== drag.itemId);
    const destinationBefore = itemsOf(board, drag.target.sectionId).map(item => item.itemId).filter(id => id !== drag.itemId);
    const targetIndex = drag.target.itemId ? destinationBefore.indexOf(drag.target.itemId) : -1;
    if (drag.target.itemId && targetIndex < 0) return;
    const insertionIndex = drag.target.itemId ? targetIndex + (drag.target.edge === "after" ? 1 : 0) : 0;
    const destinationAfter = [...destinationBefore];
    destinationAfter.splice(insertionIndex, 0, drag.itemId);

    if (drag.target.sectionId !== drag.sectionId) {
      void run(() => moveMenuItem(configuration, credential(), menuId, drag.itemId, {
        sourceSectionId: drag.sectionId,
        destinationSectionId: drag.target!.sectionId,
        sourceItemIds: remaining,
        destinationItemIds: destinationAfter
      }), {
        describe: "Move item to another section",
        undo: () => moveMenuItem(configuration, credential(), menuId, drag.itemId, {
          sourceSectionId: drag.target!.sectionId,
          destinationSectionId: drag.sectionId,
          sourceItemIds: destinationBefore,
          destinationItemIds: ids
        }),
        redo: () => moveMenuItem(configuration, credential(), menuId, drag.itemId, {
          sourceSectionId: drag.sectionId,
          destinationSectionId: drag.target!.sectionId,
          sourceItemIds: remaining,
          destinationItemIds: destinationAfter
        })
      });
      return;
    }
    if (destinationAfter.join() === ids.join()) return;

    drawItemOrder(drag.sectionId, destinationAfter);
    void run(() => reorderMenuItems(configuration, credential(), menuId, drag.sectionId, destinationAfter), {
      describe: "Reorder items",
      undo: () => { drawItemOrder(drag.sectionId, ids); return reorderMenuItems(configuration, credential(), menuId, drag.sectionId, ids); },
      redo: () => { drawItemOrder(drag.sectionId, destinationAfter); return reorderMenuItems(configuration, credential(), menuId, drag.sectionId, destinationAfter); }
    }, undefined, true);
  }

  const finishItemDrag = (event: React.PointerEvent<HTMLDivElement>) => {
    completeItemDrag(event.pointerId, event.currentTarget, event.clientX, event.clientY);
  };

  const cancelItemDrag = (event: React.PointerEvent<HTMLDivElement>) => {
    if (pointerDrag.current?.pointerId !== event.pointerId) return;
    pointerDrag.current = null;
    setDropTarget(null);
  };

  const commitHeading = () => {
    const edit = headingEdit;
    setHeadingEdit(null);
    if (!edit) return;
    const current = sectionsOf(board).find(section => section.sectionId === edit.sectionId)?.name ?? "";
    renameSection(edit.sectionId, edit.value.trim(), current);
  };

  const commitRailSectionRename = () => {
    const edit = editingRailSection;
    setEditingRailSection(null);
    if (!edit) return;
    const current = sectionsOf(board).find(section => section.sectionId === edit.sectionId)?.name ?? "";
    renameSection(edit.sectionId, edit.name.trim(), current);
  };

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
    const target = dropTarget;
    for (const row of canvas.querySelectorAll<HTMLElement>("[data-item-id]")) {
      row.classList.toggle("is-selected", row.dataset.itemId === place.selectedItemId);
      const editingThis = row.dataset.itemId === itemEdit?.itemId;
      row.querySelector<HTMLElement>(".board-item-name")?.classList.toggle("is-being-edited", editingThis && itemEdit?.field === "name");
      row.querySelector<HTMLElement>(".board-item-description")?.classList.toggle("is-being-edited", editingThis && itemEdit?.field === "description");
      row.querySelector<HTMLElement>(".board-item-price")?.classList.toggle("is-being-edited", editingThis && itemEdit?.field === "price");
      if (target && row.dataset.itemId === target.itemId && row.dataset.sectionId === target.sectionId) {
        row.dataset.dropEdge = target.edge;
      } else {
        delete row.dataset.dropEdge;
      }
    }
    for (const section of canvas.querySelectorAll<HTMLElement>("[data-section-id]")) {
      section.querySelector<HTMLElement>(".board-section-heading")?.classList.toggle(
        "is-being-edited",
        section.dataset.sectionId === headingEdit?.sectionId
      );
    }
  });

  const selectFromCanvas = (event: React.MouseEvent<HTMLDivElement>) => {
    if (suppressCanvasClick.current) return;
    /*
     * Q96: a section is renamed by clicking its heading on the canvas and typing
     * over it, where the design says the act belongs.
     */
    const heading = (event.target as HTMLElement).closest<HTMLElement>(".board-section-heading");
    if (heading && canvasRef.current) {
      const sectionId = heading.closest<HTMLElement>("[data-section-id]")?.dataset.sectionId;
      const named = sectionsOf(board).find(section => section.sectionId === sectionId);
      if (named) {
        const canvasBox = canvasRef.current.getBoundingClientRect();
        const box = heading.getBoundingClientRect();
        const computed = window.getComputedStyle(heading);
        const stage = heading.closest<HTMLElement>(".builder__stage");
        const scale = Number.parseFloat(stage ? window.getComputedStyle(stage).getPropertyValue("--board-scale") : "1") || 1;
        const scaledLength = (value: string) => value.endsWith("px") ? `${Number.parseFloat(value) * scale}px` : value;
        setHeadingEdit({
          sectionId: named.sectionId,
          value: named.name ?? "",
          box: new DOMRect(
            box.left - canvasBox.left + canvasRef.current.scrollLeft,
            box.top - canvasBox.top + canvasRef.current.scrollTop,
            box.width,
            box.height
          ),
          typography: {
            fontFamily: computed.fontFamily,
            fontSize: scaledLength(computed.fontSize),
            fontWeight: computed.fontWeight,
            fontStyle: computed.fontStyle,
            lineHeight: scaledLength(computed.lineHeight),
            letterSpacing: scaledLength(computed.letterSpacing),
            textTransform: computed.textTransform as CSSProperties["textTransform"],
            color: computed.color
          }
        });
        return;
      }
    }

    const row = (event.target as HTMLElement).closest<HTMLElement>("[data-item-id]");
    if (!row) return;
    const itemId = row.dataset.itemId!;
    setPlace(current => ({ ...current, selectedItemId: itemId }));

    const clicked = (event.target as HTMLElement).closest<HTMLElement>(".board-item-name, .board-item-description, .board-item-price");
    if (clicked && canvasRef.current) {
      const canvasBox = canvasRef.current.getBoundingClientRect();
      const cell = clicked.getBoundingClientRect();
      const computed = window.getComputedStyle(clicked);
      const stage = clicked.closest<HTMLElement>(".builder__stage");
      const scale = Number.parseFloat(stage ? window.getComputedStyle(stage).getPropertyValue("--board-scale") : "1") || 1;
      const scaledLength = (value: string) => value.endsWith("px") ? `${Number.parseFloat(value) * scale}px` : value;
      const found = findItem(board, itemId);
      if (!found) return;
      const field = clicked.classList.contains("board-item-price") ? "price" : clicked.classList.contains("board-item-description") ? "description" : "name";
      setItemEdit({
        itemId,
        field,
        value: found.item[field] ?? "",
        box: new DOMRect(
          cell.left - canvasBox.left + canvasRef.current.scrollLeft,
          cell.top - canvasBox.top + canvasRef.current.scrollTop,
          cell.width,
          cell.height
        ),
        caret: Math.max(0, Math.min((found.item[field] ?? "").length, Math.round(((event.clientX - cell.left) / Math.max(cell.width, 1)) * (found.item[field] ?? "").length))),
        typography: {
          fontFamily: computed.fontFamily,
          fontSize: scaledLength(computed.fontSize),
          fontWeight: computed.fontWeight,
          fontStyle: computed.fontStyle,
          lineHeight: scaledLength(computed.lineHeight),
          letterSpacing: scaledLength(computed.letterSpacing),
          textTransform: computed.textTransform as CSSProperties["textTransform"],
          color: computed.color,
          textAlign: computed.textAlign as CSSProperties["textAlign"]
        }
      });
      setInspectorCue(field);
      if (inspectorCueTimer.current) window.clearTimeout(inspectorCueTimer.current);
      return;
    }
  };

  const commitItemEdit = async () => {
    const edit = itemEdit;
    setItemEdit(null);
    if (!edit) return;
    inspectorCueTimer.current = window.setTimeout(() => setInspectorCue(null), 900);
    const found = findItem(board, edit.itemId);
    if (!found) return;

    const was = { name: found.item.name ?? "", description: found.item.description, price: found.item.price };
    const normalized = edit.field === "name"
      ? (edit.value.trim() === "" ? was.name : edit.value)
      : (edit.value.trim() === "" ? null : edit.value);
    if ((was[edit.field] ?? "") === (normalized ?? "")) return;
    const now = { ...was, [edit.field]: normalized };

    /*
     * The inverses carry what they expect to find. Undo restores `was` only while
     * `now` is still there; redo re-applies `now` only while `was` is. Between the
     * edit and the keystroke somebody else may have changed the same item, and an
     * unconditional inverse would erase them without either of you being told.
     */
    await run(() => updateMenuItemValues(configuration, credential(), menuId, edit.itemId, now), {
      describe: `Change ${edit.field}`,
      undo: () => updateMenuItemValues(configuration, credential(), menuId, edit.itemId, was, now),
      redo: () => updateMenuItemValues(configuration, credential(), menuId, edit.itemId, now, was)
    });
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
    searchLibraryItems(configuration, credential(), selected.item.name ?? "", 20)
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

    const was = { name: before.name ?? "", description: before.description, price: before.price };
    await run(() => updateMenuItemValues(configuration, credential(), menuId, before.itemId, next), {
      describe: "Edit item",
      undo: () => updateMenuItemValues(configuration, credential(), menuId, before.itemId, was, next),
      redo: () => updateMenuItemValues(configuration, credential(), menuId, before.itemId, next, was)
    });
  };

  const toggleAvailability = async () => {
    if (!selected) return;
    const isAvailable = selectedAvailability?.isAvailable !== false;
    // Availability commits instantly and never joins the draft. It is deliberately
    // NOT on the undo stack: undo is for the queue, and this already went out.
    await run(async () => {
      const result = await setItemAvailability(configuration, credential(), selected.item.itemId, !isAvailable);
      setNotice(availabilityImpactNotice(selected.item.name ?? "Item", result.isAvailable, result.screenIds, screens));
    });
  };

  const removeFromBoard = async () => {
    if (!selected) return;
    if (!confirmItemRemove) {
      setConfirmItemRemove(true);
      return;
    }
    const { item, sectionId } = selected;
    const sectionItems = itemsOf(board, sectionId);
    const originalOrder = sectionItems.map(candidate => candidate.itemId);
    const removedOrder = originalOrder.filter(candidate => candidate !== item.itemId);
    const selectedIndex = sectionItems.findIndex(candidate => candidate.itemId === item.itemId);
    const nextSelection = sectionItems[selectedIndex + 1]?.itemId ?? sectionItems[selectedIndex - 1]?.itemId ?? null;
    const pageId = activePageId;
    if (!pageId) return;
    const pageName = pages.find(page => page.pageId === pageId)?.name ?? "this page";
    setConfirmItemRemove(false);
    await run(
      async () => {
        await removeMenuItem(configuration, credential(), menuId, pageId, item.itemId);
        setPlace(current => ({ ...current, selectedItemId: nextSelection }));
      },
      {
        describe: `Remove “${item.name}” from “${pageName}”`,
        undo: () => transitionMenuItemPlacement(configuration, credential(), menuId, pageId, item.itemId, {
          sectionId, expectedItemIds: removedOrder, desiredItemIds: originalOrder
        }),
        redo: () => transitionMenuItemPlacement(configuration, credential(), menuId, pageId, item.itemId, {
          sectionId, expectedItemIds: originalOrder, desiredItemIds: removedOrder
        })
      }
    );
  };

  // ---- adding items --------------------------------------------------------

  const [addQuery, setAddQuery] = useState("");
  const [addPrice, setAddPrice] = useState("");
  const [addSectionId, setAddSectionId] = useState<string | null>(null);
  const [hits, setHits] = useState<LibraryItem[]>([]);
  const canonicalItemName = (value: string) => value.toLocaleLowerCase().replaceAll("&", "and").replace(/[^a-z0-9]/g, "");

  // #775: Enter and the create button used to call different functions - submitAdd's dedupe
  // search, or place_ directly - so which control was used changed the result, and neither was
  // guarded against a second submit. addSubmitting is set from the first moment of a submit,
  // before the library search even starts, and covers every entry point below (Enter, the create
  // button, and clicking a search result) through one guarded function, so the button can never
  // bypass the dedupe search and a second submit while the first is in flight is a no-op rather
  // than a duplicate.
  const [addSubmitting, setAddSubmitting] = useState(false);

  const runAddAction = (action: () => Promise<void>) => {
    if (addSubmitting) return;
    setAddSubmitting(true);
    void action().finally(() => setAddSubmitting(false));
  };

  const submitAdd = async (sectionId: string) => {
    const name = addQuery.trim();
    if (!name) return;
    const matches = await searchLibraryItems(configuration, credential(), name, 8);
    const canonical = canonicalItemName(name);
    const match = matches.find(candidate => canonicalItemName(candidate.name) === canonical);
    if (match) {
      await place_(sectionId, { itemId: match.itemId });
      if (addPrice.trim()) setNotice(`Used the existing ${match.name}. Its shared price was not changed.`);
    } else await place_(sectionId, { name, price: addPrice });
  };

  useEffect(() => {
    if (!addSectionId || addQuery.trim().length === 0) {
      setHits([]);
      return;
    }
    let cancelled = false;
    const timer = window.setTimeout(() => {
      searchLibraryItems(configuration, credential(), addQuery, 8)
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

  /*
   * The bulk drawer (Q95 opens it, Q124 governs it).
   *
   * Deliberately NOT modal. Q124 says the button retargets as you move sections,
   * and you move sections in the rail — so a scrim over the rail would make the
   * one behaviour the answer names impossible. It is a panel beside the rail, and
   * Escape closes it.
   */
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [drawerQuery, setDrawerQuery] = useState("");
  const [drawerHits, setDrawerHits] = useState<LibraryItem[]>([]);
  const [picked, setPicked] = useState<string[]>([]);
  const [placedNote, setPlacedNote] = useState<string>();
  const drawerSearch = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (drawerOpen) drawerSearch.current?.focus();
  }, [drawerOpen]);

  useEffect(() => {
    if (!drawerOpen) return;
    let cancelled = false;
    const timer = window.setTimeout(() => {
      // Empty query lists the library rather than nothing: filling a new board is
      // this path's whole point (Q124), and you cannot pick from a blank panel.
      searchLibraryItems(configuration, credential(), drawerQuery, 40)
        .then(found => {
          if (!cancelled) setDrawerHits(found);
        })
        .catch(() => {
          if (!cancelled) setDrawerHits([]);
        });
    }, 150);
    return () => {
      cancelled = true;
      window.clearTimeout(timer);
    };
  }, [credential, drawerOpen, drawerQuery, configuration]);

  useEffect(() => {
    if (!placedNote) return;
    const timer = window.setTimeout(() => setPlacedNote(undefined), 4000);
    return () => window.clearTimeout(timer);
  }, [placedNote]);

  const placeMany = async () => {
    const sectionId = place.sectionId;
    if (!sectionId || picked.length === 0) return;
    const wanted = [...picked];
    const landed: string[] = [];

    await run(
      async () => {
        /*
         * One at a time, through the same guarded placement every single add uses
         * — so the ceiling and the already-on-this-board rule are decided under the
         * lock that writes, once per item, exactly as they are for one.
         *
         * A refusal part-way stops the rest and is reported in the server's words.
         * What already landed stays landed, and the note counts what really
         * happened rather than what was asked for.
         */
        for (const itemId of wanted) {
          const outcome = await placeMenuItem(configuration, credential(), menuId, sectionId, { itemId });
          if (outcome.outcome === "placed" && outcome.itemId) landed.push(outcome.itemId);
        }
      },
      {
        describe: `Add ${wanted.length} item${wanted.length === 1 ? "" : "s"} to this board`,
        undo: async () => {
          for (const itemId of landed) {
            if (activePageId) await removeMenuItem(configuration, credential(), menuId, activePageId, itemId);
          }
        },
        redo: async () => {
          for (const itemId of landed) {
            await placeMenuItem(configuration, credential(), menuId, sectionId, { itemId });
          }
        }
      }
    );

    // Stays open, selection cleared, and says how many landed (Q124).
    setPicked([]);
    setPlacedNote(`${landed.length} placed`);
  };

  const place_ = async (sectionId: string, request: { itemId?: string; name?: string; price?: string }) => {
    let outcome: Awaited<ReturnType<typeof placeMenuItem>> | undefined;
    await run(
      async () => {
        outcome = await placeMenuItem(configuration, credential(), menuId, sectionId, request);
      },
      {
        describe: "Add to this board",
        undo: async () => {
          if (outcome?.itemId && outcome.outcome === "placed") {
            if (activePageId) await removeMenuItem(configuration, credential(), menuId, activePageId, outcome.itemId);
          }
        },
        redo: async () => {
          if (outcome?.itemId) {
            await placeMenuItem(configuration, credential(), menuId, sectionId, { itemId: outcome.itemId });
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
    setAddPrice("");
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
      setNotice(`Undid: ${step.describe}.`);
    } catch (failure) {
      /*
       * The server refuses a stale inverse by name and in its own words — it knows
       * which item changed and this screen does not. Repeating them beats replacing
       * them with a vaguer sentence of our own.
       */
      setError(
        failure instanceof MenuActionRefused
          ? failure.message
          : "That can't be undone now — the menu changed since. Nothing was moved."
      );
      await refresh().catch(() => undefined);
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
      setNotice(`Redid: ${step.describe}.`);
    } catch (failure) {
      setError(
        failure instanceof MenuActionRefused
          ? failure.message
          : "That can't be redone now — the menu changed since. Nothing was moved."
      );
      await refresh().catch(() => undefined);
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
        setConfirmDelete(null);
        setItemEdit(null);
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

  // Q113 still stands - a missing price never blocks Publish - but the owner
  // should be told, by name, before it ships rather than left to notice a
  // "$0.00" board afterward. Only items THIS draft touches are named.
  const missingPriceItems = useMemo(
    () => (board ? changedItemsMissingPrice(board, data?.changes) : []),
    [board, data?.changes]
  );

  const viewingScreens = useMemo(
    () => screens.filter(screen => (data?.screenIds ?? []).includes(screen.screenId)),
    [data?.screenIds, screens]
  );
  const viewingScreen =
    viewingScreens.find(screen => screen.screenId === viewingScreenId) ?? viewingScreens[0] ?? null;

  const publish = async () => {
    await run(async () => {
      const result = await publishMenu(configuration, credential(), menuId);
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

  // The one path both Publish buttons go through: if this draft would ship an
  // item with no price, name it and ask once, rather than publishing straight
  // through and leaving it for someone to notice on a live screen afterward.
  const requestPublish = () => {
    if (missingPriceItems.length > 0) {
      setConfirmPublishMissingPrice(true);
      return;
    }
    void publish();
  };

  const discard = async () => {
    setConfirmDiscard(false);
    await run(async () => {
      const result = await discardMenuDraft(configuration, credential(), menuId);
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

  /*
   * What sits behind a scrim, and must be unreachable while one is up. Applied to
   * the three regions rather than to the builder root, because the dialogs are
   * children of that root and inert would take them down with it.
   */
  const behindScrim =
    confirmDiscard ||
    confirmPublishMissingPrice ||
    confirmItemRemove ||
    Boolean(confirmDelete) ||
    themePickerOpen ||
    seeAllOpen ||
    reviewOpen ||
    historyOpen ||
    findOpen ||
    assignmentOpen ||
    fitOpen ||
    Boolean(signBackIn && !signInDeferred)
      ? ("" as const)
      : undefined;
  const sections = sectionsOf(board).filter(section => !activePageId || section.pageId === activePageId);
  const activePage = pages.find(page => page.pageId === activePageId);
  const activeSection = sections.find(section => section.sectionId === place.sectionId);
  const activePageItemCount = sections.reduce((count, section) => count + itemsOf(board, section.sectionId).length, 0);
  const activePageAssignmentCount = assignments.filter(assignment => assignment.pageId === activePageId).length;
  const activePageScreenNames = assignments.filter(assignment => assignment.pageId === activePageId).map(assignment => screens.find(screen => screen.screenId === assignment.screenId)?.screenName).filter((name): name is string => Boolean(name));
  const currentMenuName = board.name ?? "Untitled menu";
  const beginMenuRename = () => {
    setMenuNameDraft(currentMenuName);
    setEditingMenuName(true);
  };
  const commitMenuRename = () => {
    const name = menuNameDraft.trim();
    if (!name || name === currentMenuName) {
      setEditingMenuName(false);
      return;
    }
    void run(
      () => renameMenu(configuration, credential(), menuId, name),
      undefined,
      () => {
        setEditingMenuName(false);
        setNotice(`Menu renamed to ${name}.`);
      }
    );
  };
  const capacitySections = addSectionId && addQuery.trim()
    ? sections.map(section => section.sectionId === addSectionId
      ? { ...section, items: [...section.items, { itemId: "draft-item", name: addQuery, description: null, price: addPrice || null, sortOrder: section.items.length }] }
      : section)
    : sections;
  const pageBoard = { ...board, sections };
  const capacityBoard = { ...board, sections: capacitySections };
  const shown = canvasBoard(pageBoard, place);
  const activeAssignments = assignments.filter(assignment => assignment.menuId === menuId && assignment.pageId === activePageId);
  const capacityEvaluations = activeAssignments.flatMap(assignment => {
    const screen = screens.find(candidate => candidate.screenId === assignment.screenId);
    return screen
      ? [{ screen, result: calculateBoardCapacity(capacityBoard, { width: screen.widthPixels, height: screen.heightPixels }, board.theme) }]
      : [];
  });
  const capacity = [...capacityEvaluations].sort((left, right) => left.result.limit - right.result.limit)[0]?.result ?? null;
  const offNote = availabilityLine(selectedAvailability, venueTimezone);
  /*
   * A note per 86'd item, keyed by item. The design writes it with the time
   * ("86'd 6:40pm — hidden on all screens right now"), because the first question
   * about an item that is off is when it went off — and that answer is different
   * for every item. Composing ONE note from the first 86'd record and handing it
   * to every dimmed row told the truth about one item and lied about the rest.
   *
   * The engine draws whichever notes it is handed; it never composes one, so a
   * guest board cannot inherit them.
   */
  /*
   * Built plainly, not memoised. This sits BELOW the loading early-return, and a
   * hook below a conditional return changes the hook count between renders — which
   * takes the whole application down with a blank page, exactly as it did here.
   */
  const boardNotes: Record<string, string> = {};
  for (const state of availability) {
    const note = unavailableNote(state, venueTimezone);
    if (note) boardNotes[state.itemId] = note;
  }
  const isOff = selectedAvailability?.isAvailable === false;

  if (canAssignScreens && assignmentOpen) {
    const assignmentKey = (screenId: string, pageId: string) => `${screenId}:${pageId}`;
    const draftCount = Object.keys(assignmentDraft).length;
    return (
      <div className="builder builder--assignments" data-testid="screen-assignments" data-menu-id={menuId}>
        <header className="builder__assignments-top">
          <nav className="builder__crumbs" aria-label="Breadcrumb">
            <button type="button" className="builder__crumb-link" onClick={onBack}>Menus</button><span aria-hidden="true">/</span>
            <button type="button" className="builder__crumb-link" onClick={() => { setAssignmentOpen(false); setAssignmentDraft({}); }}>{board.name}</button><span aria-hidden="true">/</span><strong>Screens</strong>
          </nav>
          <div className="builder__assignments-actions">
            <span className={`builder__assignments-save-state${draftCount > 0 ? " is-dirty" : ""}`} role="status" data-testid="assignment-save-state"><i aria-hidden="true" />{busy ? "Saving changes…" : draftCount > 0 ? `${draftCount} unsaved ${draftCount === 1 ? "change" : "changes"}` : "No unsaved changes"}</span>
            <button type="button" className="builder__assignments-back" data-testid="assignments-back" onClick={() => { setAssignmentOpen(false); setAssignmentDraft({}); setAssignmentAddingScreenId(null); closeAssignmentChoice(); }}>Back to menu</button>
            <button type="button" className="builder__assignments-save" disabled={draftCount === 0 || busy} onClick={() => void saveAssignments()}>Save changes and return</button>
          </div>
        </header>
        <main className="builder__assignments-page">
          {error ? <p className="builder__error" role="alert" data-testid="builder-error">{error}</p> : null}
          <div className="builder__assignments-title">
            <div><h1>Which screens show this menu</h1><p>Pages are assigned, not sections. A screen holding more than one page rotates between them.</p></div>
            {screens.some(screen => screenState(screen).key !== "online") ? (() => { const count = screens.filter(screen => screenState(screen).key !== "online").length; return <p className="builder__assignments-needs"><strong>{count} {count === 1 ? "screen needs" : "screens need"} attention</strong><span>Review stale, offline and unpaired screens below.</span></p>; })() : null}
          </div>
          <div className="builder__assignments-table" role="table" aria-label="Screen assignments">
            <div className="builder__assignments-columns" role="row"><span role="columnheader">Screen</span><span role="columnheader">Geometry</span><span role="columnheader">State</span><span role="columnheader">Pages showing, in order</span><span role="columnheader">Rotation · from the theme</span></div>
            {screens.map(screen => {
              const screenAssignments = assignments.filter(assignment => assignment.screenId === screen.screenId);
              const replace = Object.entries(assignmentDraft).find(([key, mode]) => key.startsWith(`${screen.screenId}:`) && mode === "replace");
              const visibleAssignments = screenAssignments.filter(assignment => assignmentDraft[assignmentKey(screen.screenId, assignment.pageId)] !== "remove" && (!replace || assignment.pageId === replace[0].split(":")[1]));
              const additions = Object.entries(assignmentDraft).filter(([key, mode]) => key.startsWith(`${screen.screenId}:`) && mode !== "remove").map(([key]) => key.split(":")[1]);
              const visiblePageIds = new Set(visibleAssignments.map(assignment => assignment.pageId));
              for (const pageId of additions) visiblePageIds.add(pageId);
              const { key: stateKey, text: stateText } = screenState(screen);
              const canAdd = stateKey !== "unpaired" && pages.some(page => !visiblePageIds.has(page.pageId));
              return <div key={screen.screenId} className={`builder__assignments-screen builder__assignments-screen--${stateKey}`} role="row" data-testid="screen-row" data-screen-id={screen.screenId} data-state={stateKey}>
                <span className="builder__assignments-screen-name" role="cell"><strong>{screen.screenName}</strong><small>{screen.location || "Location not set"}</small></span>
                <span role="cell">{screen.widthPixels > 0 && screen.heightPixels > 0 ? `${screen.widthPixels} × ${screen.heightPixels} · ${screen.heightPixels > screen.widthPixels ? "portrait" : "landscape"}` : "Not reported yet"}</span>
                <span className={`builder__assignments-state builder__assignments-state--${stateKey}`} role="cell"><i aria-hidden="true" />{stateText}</span>
                <span className="builder__assignments-pages" role="cell">
                  {visibleAssignments.map(assignment => { const ownPage = assignment.menuId === menuId; const label = ownPage ? (assignment.pageName ?? pages.find(page => page.pageId === assignment.pageId)?.name ?? "Unnamed page") : `${assignment.menuName ?? "Another menu"} · ${assignment.pageName ?? "Unnamed page"}`; return <span className={`builder__assignment-chip${ownPage ? "" : " is-other-menu"}`} key={`${assignment.menuId}:${assignment.pageId}`}><SkyIcon name="drag" />{label}{ownPage ? <button type="button" aria-label={`Remove ${label} from ${screen.screenName}`} data-testid="remove-page-screen" onClick={() => setAssignmentDraft(current => ({ ...current, [assignmentKey(screen.screenId, assignment.pageId)]: "remove" }))}><SkyIcon name="close" /></button> : null}</span>; })}
                  {additions.filter(pageId => !screenAssignments.some(assignment => assignment.pageId === pageId)).map(pageId => { const page = pages.find(candidate => candidate.pageId === pageId); return <span className="builder__assignment-chip is-staged" key={`new:${pageId}`}>{page?.name ?? "Unnamed page"}<button type="button" aria-label={`Undo adding ${page?.name ?? "page"}`} onClick={() => setAssignmentDraft(current => { const next = { ...current }; delete next[assignmentKey(screen.screenId, pageId)]; return next; })}><SkyIcon name="close" /></button></span>; })}
                  {stateKey === "unpaired" ? <span>Pair it before assigning — we cannot draw for a screen whose size we have never been told.</span> : canAdd ? <span className="builder__assignment-add-wrap"><button type="button" className="builder__assignment-add" data-testid="add-screen-page" data-screen-id={screen.screenId} onClick={() => setAssignmentAddingScreenId(current => current === screen.screenId ? null : screen.screenId)}>+ Add a page</button>{assignmentAddingScreenId === screen.screenId ? <div className="builder__assignment-add-menu" data-testid="add-screen-page-menu">{pages.filter(page => !visiblePageIds.has(page.pageId)).map(page => <button type="button" key={page.pageId} onClick={() => { setAssignmentAddingScreenId(null); if (screenAssignments.length === 0) setAssignmentDraft(current => ({ ...current, [assignmentKey(screen.screenId, page.pageId)]: "replace" })); else { setAssignmentChoiceScreenId(screen.screenId); setAssignmentChoicePageId(page.pageId); } }}>{page.name}</button>)}</div> : null}</span> : null}
                  {assignmentChoiceScreenId === screen.screenId && assignmentChoicePageId ? <span className="builder__assignment-choice" data-testid="assignment-choice"><strong>{screen.screenName} already has {screenAssignments.map(assignment => assignment.menuId === menuId ? (assignment.pageName ?? "an assigned page") : `${assignment.menuName ?? "another menu"} · ${assignment.pageName ?? "an assigned page"}`).join(", ")}</strong><span>Add {pages.find(page => page.pageId === assignmentChoicePageId)?.name} to the rotation, or replace everything already assigned.</span><span><button type="button" className="builder__link" onClick={closeAssignmentChoice}>Back</button><button type="button" onClick={() => { setAssignmentDraft(current => ({ ...current, [assignmentKey(screen.screenId, assignmentChoicePageId)]: "rotate" })); closeAssignmentChoice(); }}>Rotate together</button><button type="button" className="action-danger" onClick={() => { setAssignmentDraft(current => ({ ...current, [assignmentKey(screen.screenId, assignmentChoicePageId)]: "replace" })); closeAssignmentChoice(); }}>Replace</button></span></span> : null}
                </span>
                <span role="cell">{visiblePageIds.size > 1 ? `Set by the ${board.theme ?? "menu"} theme` : "One page — nothing to rotate"}</span>
              </div>;
            })}
          </div>
          <p className="builder__assignments-foot">{screens.length} {screens.length === 1 ? "screen" : "screens"} at this venue. Changes are saved to the menu draft only when you choose Save changes and return; they reach screens when you publish.</p>
        </main>
      </div>
    );
  }

  return (
    <div className="builder" data-testid="menu-builder" data-menu-id={menuId}>
      <header className="builder__top" inert={behindScrim}>
        <nav className="builder__crumbs" aria-label="Breadcrumb">
          <button type="button" className="builder__crumb-link" onClick={onBack} data-testid="back-to-menus">
            Menus
          </button>
          <span aria-hidden="true">/</span>
          {editingMenuName ? <input
            className="builder__menu-name-input"
            data-testid="menu-name-input"
            aria-label="Menu name"
            autoFocus
            maxLength={200}
            value={menuNameDraft}
            onChange={event => setMenuNameDraft(event.target.value)}
            onBlur={commitMenuRename}
            onKeyDown={event => {
              if (event.key === "Enter") event.currentTarget.blur();
              if (event.key === "Escape") {
                setMenuNameDraft(currentMenuName);
                setEditingMenuName(false);
              }
            }}
          /> : <>
            <span className="builder__crumb-current" data-testid="builder-menu-name">{currentMenuName}</span>
            <button type="button" className="builder__menu-name-edit" data-testid="edit-menu-name" aria-label={`Edit ${currentMenuName} menu name`} onClick={beginMenuRename}>
              <SkyIcon name="pencil" />
            </button>
          </>}
          <button type="button" className="builder__top-add-content" data-testid="top-add-content" onClick={() => setDrawerOpen(true)}>+ Add content</button>
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

      <nav className="builder__pages" aria-label="Menu pages" inert={behindScrim} data-testid="page-rail">
        <div className="builder__pages-intro">
          <strong>Menu pages</strong>
          <span>Choose a page to edit and preview.</span>
        </div>
        <div className="builder__page-tabs">
          {pages.map(page => (
            <div
              className="builder__page-tab-wrap"
              key={page.pageId}
              draggable={canManagePages && !editingPage && !busy}
              onDragStart={() => setDraggedPageId(page.pageId)}
              onDragEnd={() => setDraggedPageId(null)}
              onDragOver={event => event.preventDefault()}
              onDrop={() => void dropPage(page.pageId)}
              data-testid="page-tab-wrap"
            >
              <button
                type="button"
                className={`builder__page-tab${activePageId === page.pageId ? " is-active" : ""}`}
                data-testid="page-tab"
                data-page-id={page.pageId}
                data-active={activePageId === page.pageId}
                onClick={() => {
                  setActivePageId(page.pageId);
                  const firstSection = sectionsOf(board).find(section => section.pageId === page.pageId);
                  setPlace(current => ({ ...current, view: "whole-board", sectionId: firstSection?.sectionId ?? null, selectedItemId: null }));
                  setPageMenuId(null);
                }}
              >
                {page.name}
              </button>
            </div>
          ))}
          {canManagePages && addingPage ? (
            <input
              className="builder__page-name-input"
              autoFocus
              value={newPageName}
              placeholder="Page name"
              onChange={event => setNewPageName(event.target.value)}
              onBlur={() => void commitNewPage()}
              onKeyDown={event => { if (event.key === "Enter") void commitNewPage(); if (event.key === "Escape") { setAddingPage(false); setNewPageName(""); } }}
              aria-label="Page name"
              data-testid="page-name-input"
            />
          ) : canManagePages ? (
            <button type="button" className="builder__add-page" aria-label="Add page" data-testid="add-page" onClick={() => setAddingPage(true)}>+</button>
          ) : null}
        </div>
      </nav>

      <div
        className={`builder__columns${panelPreferences.leftCollapsed ? " is-left-collapsed" : ""}${panelPreferences.rightCollapsed ? " is-right-collapsed" : ""}`}
        data-left-panel={panelPreferences.leftCollapsed ? "collapsed" : "expanded"}
        data-right-panel={panelPreferences.rightCollapsed ? "collapsed" : "expanded"}
        inert={behindScrim}
      >
        <nav className={`builder__rail${panelPreferences.leftCollapsed ? " is-collapsed" : ""}`} aria-label="Sections and history">
          <div className="builder__rail-head">
            <h2>Sections</h2>
            <PanelCollapseButton
              panel="sections and history"
              collapsed={panelPreferences.leftCollapsed}
              onClick={() => setPanelPreferences(current => ({ ...current, leftCollapsed: !current.leftCollapsed }))}
            />
          </div>

          <ul className="builder__rail-list">
            {sections.map((section, index) => (
              <li key={section.sectionId} data-testid="section-row" data-section-id={section.sectionId} data-selected={place.view === "one-section" && place.sectionId === section.sectionId} draggable={!editingRailSection && !busy} onDragStart={() => setDraggedSectionId(section.sectionId)} onDragEnd={() => setDraggedSectionId(null)} onDragOver={event => event.preventDefault()} onDrop={() => { const from = sections.findIndex(candidate => candidate.sectionId === draggedSectionId); if (from >= 0) void moveSection(from, index); setDraggedSectionId(null); }}>
                {editingRailSection?.sectionId === section.sectionId ? <input
                  autoFocus
                  className="builder__rail-new builder__rail-rename"
                  value={editingRailSection.name}
                  maxLength={200}
                  aria-label={`Rename ${section.name}`}
                  data-testid="section-rename-input"
                  onChange={event => setEditingRailSection({ sectionId: section.sectionId, name: event.target.value })}
                  onBlur={commitRailSectionRename}
                  onKeyDown={event => {
                    if (event.key === "Enter") commitRailSectionRename();
                    if (event.key === "Escape") setEditingRailSection(null);
                  }}
                /> : <button
                  type="button"
                  className={`builder__rail-row${place.view === "one-section" && place.sectionId === section.sectionId ? " is-selected" : ""}`}
                  onClick={() => setPlace(current => ({ ...current, view: "one-section", sectionId: section.sectionId, selectedItemId: null }))}
                  data-testid="rail-section"
                  data-section-id={section.sectionId}
                  aria-current={place.view === "one-section" && place.sectionId === section.sectionId ? "true" : undefined}
                >
                  <span className="builder__rail-handle" aria-hidden="true">
                    ⠿
                  </span>
                  <span className="builder__rail-name">{section.name}</span>
                  <span className="builder__rail-count">{itemsOf(board, section.sectionId).length}</span>
                </button>}
                <span className="builder__rail-actions">
                  <button type="button" className="builder__rail-rename-action" aria-label={`Rename ${section.name}`} onClick={() => setEditingRailSection({ sectionId: section.sectionId, name: section.name ?? "" })}><SkyIcon name="pencil" /></button>
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
                  <button
                    type="button"
                    className="builder__rail-delete"
                    data-testid="delete-section"
                    aria-label={`Delete ${section.name}`}
                    disabled={busy}
                    onClick={() => {
                      const destination = sections.find(candidate => candidate.sectionId !== section.sectionId)?.sectionId ?? "";
                      const items = itemsOf(board, section.sectionId).length;
                      setConfirmDelete({
                        sectionId: section.sectionId,
                        name: section.name ?? "this section",
                        items,
                        destinationSectionId: destination,
                        mode: items > 0 && destination ? "move" : "delete"
                      });
                    }}
                  >
                    <svg viewBox="0 0 24 24" aria-hidden="true">
                      <path d="M4 7h16M9 7V4h6v3m-9 0 1 13h10l1-13M10 11v5m4-5v5" />
                    </svg>
                  </button>
                </span>
              </li>
            ))}
            {!addingSection ? <li className="builder__rail-add-row"><button type="button" className="builder__rail-add" onClick={() => setAddingSection(true)} data-testid="add-section">+ Add section</button></li> : null}

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
          {canViewHistory ? <section className="builder__page-history" aria-labelledby="page-history-title" data-testid="page-history">
            <header className="builder__page-history-header">
              <h3 id="page-history-title">History · {activePage?.name ?? "Page"}</h3>
              <button type="button" className="builder__link" data-testid="menu-history-link" onClick={() => { setHistoryOpen(true); if (!history) loadMenuHistory(configuration, credential(), menuId).then(setHistory).catch(() => setHistory([])); }}>View all</button>
            </header>
            {pageHistoryError ? <div className="builder__page-history-state" role="alert"><span>History couldn&apos;t load.</span><button type="button" className="builder__link" onClick={() => activePageId && void refreshPageHistory(activePageId)}>Try again</button></div>
              : pageHistory === undefined ? <p className="builder__page-history-state" role="status">Loading history…</p>
              : pageHistory.length === 0 ? null
              : <ol className="builder__page-history-list">{pageHistory.map((entry, index) => <li key={`${entry.occurredUtc}:${entry.kind}:${entry.detail}:${index}`} data-testid="page-history-entry">
                  <i aria-hidden="true" />
                  <span><strong>{entry.detail ?? entry.kind.replaceAll("_", " ")}</strong><small>{entry.author ? `${entry.author} · ` : ""}{venueTime(entry.occurredUtc, venueTimezone)}</small></span>
                </li>)}</ol>}
          </section> : null}
        </nav>

        <main className="builder__canvas">
          <div className="builder__canvas-head">
            {activePageId ? <div className="builder__page-summary" data-testid="page-summary" data-view={place.view === "whole-board" ? "whole-page" : "section"}>
              <div className="builder__view-context" data-testid="view-context">
                <div className="builder__view-breadcrumb">
                  {editingPage?.pageId === activePageId && activePage ? <input
                    className="builder__page-inline-input"
                    autoFocus
                    value={editingPage.name}
                    onChange={event => setEditingPage({ pageId: activePage.pageId, name: event.target.value })}
                    onBlur={() => void commitPageRename()}
                    onKeyDown={event => { if (event.key === "Enter") event.currentTarget.blur(); if (event.key === "Escape") setEditingPage(null); }}
                    aria-label={`Rename ${activePage.name}`}
                    data-testid="page-rename-input"
                  /> : canManagePages ? <span className="builder__page-actions-wrap" ref={pageActionsRef}>
                    <button
                      type="button"
                      className="builder__page-crumb"
                      data-testid="page-actions"
                      aria-label={`${activePage?.name ?? "Page"} page actions`}
                      aria-haspopup="menu"
                      aria-expanded={pageMenuId === activePageId}
                      onClick={() => setPageMenuId(open => open === activePageId ? null : activePageId)}
                    ><span data-testid="page-name">{activePage?.name}</span><SkyIcon name="chevron" size={14} /></button>
                    {pageMenuId === activePageId ? (() => { const page = pages.find(candidate => candidate.pageId === activePageId)!; return <div className="builder__page-menu" data-testid="page-menu" role="menu" aria-label={`${page.name} page actions`}><button type="button" role="menuitem" onClick={() => { setPageMenuId(null); setEditingPage({ pageId: page.pageId, name: page.name }); }}>Rename page</button><button type="button" role="menuitem" onClick={() => void duplicatePage(page.pageId)}>Duplicate page</button><hr /><button type="button" role="menuitem" className="builder__page-menu-danger" disabled={pages.length === 1} onClick={() => { const destinationPageId = pages.find(candidate => candidate.pageId !== page.pageId)?.pageId ?? ""; setPageMenuId(null); setConfirmPageDelete({ pageId: page.pageId, name: page.name, destinationPageId, sectionCount: sectionsOf(board).filter(section => section.pageId === page.pageId).length, mode: "move" }); }}>Delete page</button></div>; })() : null}
                  </span> : <strong className="builder__page-current" data-testid="page-name">{activePage?.name}</strong>}
                  {place.view === "one-section" && activeSection ? <><span className="builder__view-separator" aria-hidden="true">/</span><strong className="builder__section-current" data-testid="section-scope">{activeSection.name}</strong></> : null}
                </div>
                <span className="builder__view-meta">{place.view === "whole-board" ? `${sections.length} ${sections.length === 1 ? "section" : "sections"} · ` : ""}{place.view === "one-section" && activeSection ? itemsOf(board, activeSection.sectionId).length : activePageItemCount} {(place.view === "one-section" && activeSection ? itemsOf(board, activeSection.sectionId).length : activePageItemCount) === 1 ? "item" : "items"}</span>
              </div>
              {activePageAssignmentCount > 0 ? <span className="sr-only" data-testid="page-assignment-count">{activePageAssignmentCount} {activePageAssignmentCount === 1 ? "screen" : "screens"}</span> : null}
              {canAssignScreens ? <button type="button" className="builder__assignment-pill" onClick={() => { setAssignmentDraft({}); setAssignmentChoiceScreenId(null); setAssignmentChoicePageId(null); setAssignmentAddingScreenId(null); setAssignmentOpen(true); }} data-testid="assignment-pill">
                <SkyIcon name="screen-mark" />
                <span className="builder__assignment-pill-copy"><strong>{activePageAssignmentCount > 0 ? `On ${activePageAssignmentCount} ${activePageAssignmentCount === 1 ? "screen" : "screens"}${activePageScreenNames.length > 0 ? ` · ${activePageScreenNames.join(", ")}` : ""}` : "No screens assigned"}</strong><small>Manage screens</small></span>
                <SkyIcon name="chevron" />
              </button> : null}
            </div> : null}
            {place.selectedItemId && isMissingPrice(selected?.item) ? (
              <p className="builder__flag" data-testid="missing-price-flag">
                No price yet. You can still publish it.
              </p>
            ) : null}
          </div>

          {canViewCapacity && capacity && capacity.state !== "fits" ? (
            <div
              className={`builder__capacity builder__capacity--${capacity.state}`}
              data-testid="capacity-banner"
              data-capacity={capacity.state === "overflow" ? "overflowing" : capacity.state}
              data-capacity-limit={capacity.limit}
              data-dropped-items={capacity.dropped.join("|")}
            >
              <SkyIcon name="warning" />
              <span><strong>{capacity.state === "overflow" ? "This page is over capacity" : "This page is nearly full"}</strong><small>{capacity.state === "overflow"
                ? `${capacity.dropped.join(", ")} will be dropped on the tightest screen.`
                : `${capacity.count} of ${capacity.limit} item spaces are used on the tightest screen.`}</small></span>
              <button type="button" className="action-secondary" data-testid="check-fit" onClick={() => setFitOpen(true)}>Check fit</button>
            </div>
          ) : null}

          <div
            className="builder__board-card"
            ref={canvasRef}
            data-selected-item={place.selectedItemId ?? undefined}
            onClick={selectFromCanvas}
            data-testid="canvas"
            onPointerDown={beginItemDrag}
            onPointerMove={trackItemDrag}
            onPointerUpCapture={finishItemDrag}
            onMouseUp={event => {
              const drag = pointerDrag.current;
              if (drag) completeItemDrag(drag.pointerId, event.currentTarget, event.clientX, event.clientY);
            }}
            onPointerCancel={cancelItemDrag}
          >
            {place.view === "whole-board" ? (
              <BoardFrame
                board={shown}
                unavailableItemIds={unavailableIds}
                surface="preview"
                keepUnavailable
                itemsDraggable
                keepEmptySections
                unavailableNotes={boardNotes}
                selectedItemId={place.selectedItemId}
                onRemoveItem={() => void removeFromBoard()}
              />
            ) : (
              <BoardStage>
                <BoardRenderer
                  board={shown}
                  unavailableItemIds={unavailableIds}
                  surface="preview"
                  keepUnavailable
                  itemsDraggable
                  keepEmptySections
                  unavailableNotes={boardNotes}
                  selectedItemId={place.selectedItemId}
                  onRemoveItem={() => void removeFromBoard()}
                />
              </BoardStage>
            )}

            {itemEdit ? (
              itemEdit.field === "description" || itemEdit.field === "name" ? <textarea
                ref={itemEditRef as RefObject<HTMLTextAreaElement>}
                className={`builder__item-edit builder__item-edit--${itemEdit.field}`}
                value={itemEdit.value} aria-label={itemEdit.field === "name" ? "Item name" : "Description"} data-testid={`${itemEdit.field}-edit`} maxLength={itemEdit.field === "name" ? 200 : 1000}
                rows={1}
                style={{ ...itemEdit.typography, left: `${itemEdit.box.left}px`, top: `${itemEdit.box.top}px`, width: `${Math.max(itemEdit.box.width, 160)}px`, height: `${Math.max(itemEdit.box.height, 34)}px` }}
                onChange={event => setItemEdit(current => (current ? { ...current, value: event.target.value } : current))}
                onBlur={() => void commitItemEdit()}
                onKeyDown={event => { if (itemEdit.field === "name" && event.key === "Enter") { event.preventDefault(); void commitItemEdit(); } if (event.key === "Escape") { setItemEdit(null); setInspectorCue(null); } }}
              /> : <input
                ref={itemEditRef as RefObject<HTMLInputElement>}
                className="builder__item-edit builder__item-edit--price"
                value={itemEdit.value} aria-label="Price" data-testid="price-edit" maxLength={12}
                style={{ ...itemEdit.typography, left: `${itemEdit.box.left - 8}px`, top: `${itemEdit.box.top}px`, width: `${Math.max(itemEdit.box.width + 8, 68)}px`, height: `${Math.max(itemEdit.box.height, 24)}px` }}
                onChange={event => setItemEdit(current => (current ? { ...current, value: event.target.value } : current))}
                onBlur={() => void commitItemEdit()}
                onKeyDown={event => { if (event.key === "Enter") void commitItemEdit(); if (event.key === "Escape") { setItemEdit(null); setInspectorCue(null); } }}
              />
            ) : null}

            {/*
              Typing over the heading, in the heading's own place (Q96). It sits at
              the measured box rather than replacing the rendered element, because
              the element belongs to the engine and the engine has no affordances.
            */}
            {headingEdit ? (
              <input
                className="builder__heading-edit"
                autoFocus
                value={headingEdit.value}
                aria-label="Section name"
                data-testid="heading-edit"
                maxLength={200}
                style={{
                  ...headingEdit.typography,
                  left: `${headingEdit.box.left}px`,
                  top: `${headingEdit.box.top}px`,
                  width: `${Math.max(headingEdit.box.width + 8, 120)}px`,
                  height: `${headingEdit.box.height}px`
                }}
                onChange={event =>
                  setHeadingEdit(current => (current ? { ...current, value: event.target.value } : current))
                }
                onBlur={commitHeading}
                onKeyDown={event => {
                  if (event.key === "Enter") commitHeading();
                  if (event.key === "Escape") setHeadingEdit(null);
                }}
              />
            ) : null}
          </div>

          {place.view === "one-section" && place.sectionId ? (
            <div
              className="builder__add-row"
              onBlurCapture={event => {
                if (addQuery.trim() || event.currentTarget.contains(event.relatedTarget as Node | null)) return;
                setAddPrice("");
                setAddSectionId(null);
              }}
            >
              {addSectionId === place.sectionId ? (
                <>
                  <input
                    autoFocus
                    value={addQuery}
                    placeholder="Find an item, or type a new one"
                    aria-label="Add an item"
                    data-testid="add-item-input"
                    role="combobox"
                    aria-autocomplete="list"
                    aria-expanded={hits.length > 0}
                    aria-controls={hits.length > 0 ? `add-item-results-${place.sectionId}` : undefined}
                    aria-activedescendant={hits[0] ? `add-item-option-${hits[0].itemId}` : undefined}
                    aria-busy={addSubmitting}
                    disabled={addSubmitting}
                    onChange={event => setAddQuery(event.target.value)}
                    onKeyDown={event => {
                      if (event.key === "Enter" && addQuery.trim()) {
                        runAddAction(() => submitAdd(place.sectionId!));
                      }
                      if (event.key === "Escape") { setAddQuery(""); setAddPrice(""); setAddSectionId(null); }
                    }}
                  />
                  <input
                    value={addPrice}
                    placeholder="Price (optional)"
                    aria-label="Item price"
                    data-testid="add-item-price"
                    maxLength={12}
                    disabled={addSubmitting}
                    onChange={event => setAddPrice(event.target.value)}
                    onKeyDown={event => {
                      if (event.key === "Enter" && addQuery.trim()) runAddAction(() => submitAdd(place.sectionId!));
                      if (event.key === "Escape") { setAddQuery(""); setAddPrice(""); setAddSectionId(null); }
                    }}
                  />
                  {addSubmitting ? <span className="builder__add-status" role="status" aria-live="polite">Adding…</span> : null}
                  {hits.length > 0 ? <div id={`add-item-results-${place.sectionId}`} role="listbox" className="builder__add-results" data-testid="add-item-results">
                    {hits.map(hit => {
                      const here = hit.boards.some(entry => entry.menuId === menuId);
                      const elsewhere = hit.boards.filter(entry => entry.menuId !== menuId);
                      return (
                          <button key={hit.itemId}
                            type="button"
                            id={`add-item-option-${hit.itemId}`}
                            role="option"
                            data-testid="add-item-result"
                            data-item-id={hit.itemId}
                            aria-selected={hit === hits[0]}
                            className={hit === hits[0] ? "is-selected" : undefined}
                            disabled={addSubmitting}
                            onClick={() => runAddAction(() => place_(place.sectionId!, { itemId: hit.itemId }))}
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
                      );
                    })}
                  </div> : null}
                  {addQuery.trim() ? (
                    <button
                      type="button"
                      className="builder__add-create"
                      data-testid="add-item-create"
                      disabled={addSubmitting}
                      // #775: this used to call place_ directly, skipping submitAdd's dedupe
                      // search entirely - the button and Enter did materially different things,
                      // which is how a name that already existed in the library became a second
                      // library item instead of being reused. Both now go through submitAdd.
                      onClick={() => runAddAction(() => submitAdd(place.sectionId!))}
                    >
                      {addSubmitting ? "Adding…" : <>Create “{addQuery.trim()}” as a new item</>}
                    </button>
                  ) : null}
                  {/* The bulk path lives on the add row, not on the rail's + (Q95). */}
                  <button
                    type="button"
                    className="builder__link builder__add-many"
                    data-testid="open-add-many"
                    onClick={() => setDrawerOpen(true)}
                  >
                    Add many at once
                  </button>
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

        </main>

        <aside className={`builder__inspector${panelPreferences.rightCollapsed ? " is-collapsed" : ""}`} aria-label="Item panel">
          <div className="builder__inspector-toolbar">
            <strong>Items</strong>
            <PanelCollapseButton
              panel="item"
              collapsed={panelPreferences.rightCollapsed}
              onClick={() => setPanelPreferences(current => ({ ...current, rightCollapsed: !current.rightCollapsed }))}
            />
          </div>
          <div className="builder__inspector-body">
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

              {isOff ? (
                <div className="builder__availability is-off" data-testid="availability-panel" data-off="true">
                  <div className="builder__availability-head">
                    <strong>Off right now</strong>
                    <button
                      type="button"
                      className="builder__availability-switch"
                      role="switch"
                      aria-checked={false}
                      aria-label="Turn back on"
                      data-testid="availability-switch"
                      onClick={() => void toggleAvailability()}
                      disabled={busy}
                    >
                      <span aria-hidden="true" />
                    </button>
                  </div>
                  <p>
                    {/*
                      The trailing clause is verbatim copy in the design authority,
                      and it carries the rule: everything else on this page waits for
                      Publish, and this does not.
                    */}
                    {offNote ??
                      `Showing on every screen this board is on. Turning this off hides it on ${
                        targets.total === 1 ? "your screen" : `all ${targets.total} screens`
                      } immediately — not part of your draft.`}
                  </p>
                </div>
              ) : (
                <div className="builder__availability-control">
                  <span className="builder__label">Availability</span>
                  <button
                    type="button"
                    className="builder__availability-switch"
                    role="switch"
                    aria-checked={true}
                    aria-label="Turn off"
                    data-testid="availability-switch"
                    onClick={() => void toggleAvailability()}
                    disabled={busy}
                  >
                    <span aria-hidden="true" />
                  </button>
                </div>
              )}

              <label className={inspectorCue === "name" ? "is-cued" : undefined} data-inspector-row="name">
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

              <label className={inspectorCue === "description" ? "is-cued" : undefined} data-inspector-row="description">
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

              <label className={inspectorCue === "price" ? "is-cued" : undefined} data-inspector-row="price">
                <span className="builder__label">Price</span>
                <input
                  data-inspector-field="price"
                  data-testid="item-price"
                  maxLength={12}
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
                Remove from this page
              </button>

              <p className="builder__theme-footer">
                <button
                  type="button"
                  data-testid="open-theme-picker"
                  onClick={() => {
                    setThemePickerOpen(true);
                    if (!themes) {
                      loadMenuThemes(configuration, credential())
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
          </div>
        </aside>

        {/*
          The bulk drawer. It overlays the canvas and inspector and leaves the rail
          alone, because Q124 says the place button retargets as you move sections
          and the rail is how you move sections.
        */}
        {drawerOpen ? (
          <section
            className="builder__drawer"
            aria-label="Add many at once"
            data-testid="add-many-drawer"
            onKeyDown={event => {
              if (event.key === "Escape") setDrawerOpen(false);
            }}
          >
            <header className="builder__drawer-head">
              <h2>Add many at once</h2>
              <button
                type="button"
                aria-label="Close"
                data-testid="close-add-many"
                onClick={() => setDrawerOpen(false)}
              >
                ✕
              </button>
            </header>

            <input
              ref={drawerSearch}
              className="builder__drawer-search"
              value={drawerQuery}
              placeholder="Search your items"
              aria-label="Search your items"
              data-testid="add-many-search"
              onChange={event => setDrawerQuery(event.target.value)}
            />

            <ul className="builder__drawer-list" data-testid="add-many-list">
              {drawerHits.map(hit => {
                const here = hit.boards.some(entry => entry.menuId === menuId);
                const elsewhere = hit.boards.filter(entry => entry.menuId !== menuId);
                return (
                  <li key={hit.itemId}>
                    {/*
                      An item already on this board is shown and labelled, never
                      offered — placing it again is refused under the lock anyway
                      (Q112), and a checkbox that cannot do anything is a lie.
                    */}
                    <label className={here ? "is-here" : undefined}>
                      <input
                        type="checkbox"
                        data-testid="add-many-pick"
                        data-item-id={hit.itemId}
                        disabled={here}
                        checked={picked.includes(hit.itemId)}
                        onChange={event =>
                          setPicked(current =>
                            event.target.checked
                              ? [...current, hit.itemId]
                              : current.filter(id => id !== hit.itemId)
                          )
                        }
                      />
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
                    </label>
                  </li>
                );
              })}
            </ul>

            <footer className="builder__drawer-foot">
              {placedNote ? (
                <span className="builder__drawer-note" data-testid="add-many-placed" role="status">
                  {placedNote}
                </span>
              ) : null}
              <button
                type="button"
                className="action-primary"
                data-testid="add-many-place"
                disabled={picked.length === 0 || !place.sectionId || busy}
                onClick={() => void placeMany()}
              >
                {/*
                  The button names its target, and the target follows the rail —
                  move sections with the drawer open and this sentence changes
                  under your hand (Q124).
                */}
                Place {picked.length} in{" "}
                {sections.find(section => section.sectionId === place.sectionId)?.name ?? "this section"}
              </button>
            </footer>
          </section>
        ) : null}
      </div>

      <footer className="builder__publish" data-testid="publish-bar" inert={behindScrim}>
        <div className="builder__publish-left">
          <strong data-testid="draft-count" className={data.draftCount ? "is-pending" : ""}>
            {draftPhrase(data.draftCount, { neverPublished: data.publishedVersion === null })}
          </strong>
          <span className="builder__publish-meta">
            {saveState === "failed" ? (
              /*
               * Two different failures, and they must not wear each other's words.
               * A dropped request IS retrying; an expired session is not — it is
               * waiting for you, and saying "retrying…" there would be a promise
               * the screen has no way to keep (Q197/Q199).
               */
              signBackIn ? (
                <span className="builder__save-failed" data-testid="save-needs-sign-in">
                  Couldn&apos;t save your last change — your sign-in expired.{" "}
                  <button
                    type="button"
                    className="builder__link"
                    data-testid="resume-sign-in"
                    onClick={() => setSignInDeferred(false)}
                  >
                    Sign back in
                  </button>{" "}
                  and it sends.
                </span>
              ) : (
                <span className="builder__save-failed" data-testid="save-failed">
                  Couldn&apos;t save your last change — retrying…
                </span>
              )
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
                      loadMenuHistory(configuration, credential(), menuId)
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
              onClick={requestPublish}
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

      {signBackIn && !signInDeferred ? (
        <SignBackIn
          configuration={configuration}
          holding={signBackIn}
          onSignedIn={resumeAfterSignIn}
          onDismiss={() => setSignInDeferred(true)}
        />
      ) : null}

      {confirmDiscard ? (
        <>
          <div className="builder__scrim" onClick={() => setConfirmDiscard(false)} />
          <div className="builder__dialog" role="dialog" aria-modal="true" aria-labelledby="discard-title" data-testid="discard-dialog" ref={discardRef}>
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

      {confirmPublishMissingPrice ? (
        <>
          <div className="builder__scrim" onClick={() => setConfirmPublishMissingPrice(false)} />
          <div
            className="builder__dialog"
            role="dialog"
            aria-modal="true"
            aria-labelledby="publish-missing-price-title"
            data-testid="publish-missing-price-dialog"
            ref={publishMissingPriceRef}
          >
            <h2 id="publish-missing-price-title">
              {missingPriceItems.length === 1 ? "1 item has no price" : `${missingPriceItems.length} items have no price`}
            </h2>
            <p>It will show on your screens with no price unless you go back and set one.</p>
            <ul className="builder__screen-list" data-testid="publish-missing-price-list">
              {missingPriceItems.map(item => (
                <li key={item.itemId}>
                  <strong>{item.name}</strong>
                </li>
              ))}
            </ul>
            <div className="builder__dialog-actions">
              <button type="button" className="action-secondary" onClick={() => setConfirmPublishMissingPrice(false)}>
                Go back
              </button>
              <button
                type="button"
                className="builder__publish-button"
                data-testid="confirm-publish-missing-price"
                onClick={() => {
                  setConfirmPublishMissingPrice(false);
                  void publish();
                }}
              >
                Publish anyway
              </button>
            </div>
          </div>
        </>
      ) : null}

      {confirmDelete ? (
        <>
          <div className="builder__scrim" onClick={() => setConfirmDelete(null)} />
          <div
            className="builder__dialog"
            role="dialog"
            aria-modal="true"
            aria-labelledby="delete-section-title"
            data-testid="delete-section-dialog"
            ref={deleteRef}
          >
            <h2 id="delete-section-title">Delete {confirmDelete.name}?</h2>
            <p>
              {confirmDelete.items === 0
                ? "This section is empty."
                : `${confirmDelete.items} ${confirmDelete.items === 1 ? "item" : "items"} ${confirmDelete.mode === "move" ? "will move" : "will return to the library"}. Library items are kept.`}
            </p>
            {confirmDelete.items > 0 ? <fieldset className="builder__delete-page-choice">
              <legend>What should happen to its items?</legend>
              {sectionsOf(board).filter(section => section.pageId === activePageId && section.sectionId !== confirmDelete.sectionId).length > 0 ? <label>
                <input type="radio" name="delete-section-mode" checked={confirmDelete.mode === "move"} onChange={() => setConfirmDelete(current => current ? { ...current, mode: "move" } : current)} />
                Move items to
                <select value={confirmDelete.destinationSectionId} onChange={event => setConfirmDelete(current => current ? { ...current, destinationSectionId: event.currentTarget.value } : current)}>
                  {sectionsOf(board).filter(section => section.pageId === activePageId && section.sectionId !== confirmDelete.sectionId).map(section => <option key={section.sectionId} value={section.sectionId}>{section.name}</option>)}
                </select>
              </label> : null}
              <label>
                <input type="radio" name="delete-section-mode" checked={confirmDelete.mode === "delete"} onChange={() => setConfirmDelete(current => current ? { ...current, mode: "delete" } : current)} />
                Delete section and return its items to the library
              </label>
            </fieldset> : null}
            <p className="builder__dialog-note">This can&apos;t be undone.</p>
            <div className="builder__dialog-actions">
              <button type="button" className="action-secondary" onClick={() => setConfirmDelete(null)}>
                Keep it
              </button>
              <button
                type="button"
                className="builder__quiet-danger"
                data-testid="confirm-delete-section"
                onClick={() => {
                  const target = confirmDelete;
                  void deleteSection(target.sectionId, target.name, target.items > 0 && target.mode === "move" ? target.destinationSectionId : undefined, target.items === 0 || target.mode === "delete");
                }}
              >
                Delete section
              </button>
            </div>
          </div>
        </>
      ) : null}

      {themePickerOpen ? (
        <>
          <div className="builder__scrim" onClick={() => setThemePickerOpen(false)} />
          <div className="builder__dialog" role="dialog" aria-modal="true" aria-labelledby="theme-title" data-testid="theme-picker" ref={themeRef}>
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
          <div className="builder__dialog" role="dialog" aria-modal="true" aria-labelledby="screens-title" data-testid="see-all-dialog" ref={seeAllRef}>
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
          <div className="builder__dialog" role="dialog" aria-modal="true" aria-labelledby="review-title" data-testid="review-dialog" ref={reviewRef}>
            <h2 id="review-title">{draftPhrase(data.draftCount, { neverPublished: data.publishedVersion === null })}</h2>
            <p>Exactly what publishing will send to your screens — nothing more.</p>
            <ul className="builder__screen-list" data-testid="review-list">
              {data.changes.map((change, index) => (
                <li key={`${change.targetKind}-${change.targetId}-${change.field}-${index}`}>
                  <strong>{changeSentence(change, board)}</strong>
                  {/*
                    The values, not the word "changed". Both were already in hand and
                    used only to pick that word - which at 11pm tells an owner nothing
                    about whether the price is now 12.50 or 14.00.
                  */}
                  <span>{changeValues(change)}</span>
                </li>
              ))}
            </ul>
            <div className="builder__dialog-actions">
              <button type="button" className="action-secondary" onClick={() => setReviewOpen(false)}>
                Close
              </button>
              {/* Reviewing leads into the act, rather than back out of it. */}
              {data.draftCount > 0 && !blocked ? (
                <button
                  type="button"
                  className="builder__publish-button"
                  data-testid="publish-from-review"
                  disabled={busy}
                  onClick={() => {
                    setReviewOpen(false);
                    requestPublish();
                  }}
                >
                  {publishLabel(data.draftCount)}
                </button>
              ) : null}
            </div>
          </div>
        </>
      ) : null}

      {confirmItemRemove && selected ? (
        <div className="builder__scrim" role="presentation">
          <div className="builder__dialog" role="dialog" aria-modal="true" aria-labelledby="remove-item-title" data-testid="remove-item-dialog" ref={itemRemoveRef}>
            <h2 id="remove-item-title">Remove {selected.item.name} from {pages.find(page => page.pageId === activePageId)?.name ?? "this page"}?</h2>
            <p>It stays in your item library, and on any other page using it.</p>
            <div className="builder__dialog-actions">
              <button type="button" className="secondary" onClick={() => setConfirmItemRemove(false)}>Cancel</button>
              <button type="button" className="action-danger" onClick={() => void removeFromBoard()}>Remove from this page</button>
            </div>
          </div>
        </div>
      ) : null}

      {historyOpen ? (
        <>
          <div className="builder__scrim" onClick={() => setHistoryOpen(false)} />
          <div className="builder__dialog" role="dialog" aria-modal="true" aria-labelledby="history-title" data-testid="history-dialog" ref={historyRef}>
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
                          const result = await goBackToMenuVersion(configuration, credential(), menuId, entry.version!);
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

      {fitOpen ? <>
        <div className="builder__scrim" />
        <div className="builder__dialog" role="dialog" aria-modal="true" aria-labelledby="fit-title" ref={fitRef} data-testid="fit-results">
          <h2 id="fit-title">Fit by screen</h2>
          <p>Capacity follows each assigned screen’s geometry and this menu’s theme.</p>
          <ul className="builder__fit-results">
            {capacityEvaluations.map(({ screen, result }) => <li key={screen.screenId}>
              <strong>{screen.screenName}</strong>
              <span>{screen.widthPixels} × {screen.heightPixels} · {result.count} of {result.limit} spaces</span>
              {result.dropped.length > 0
                ? <span>Move or remove {result.dropped.length} {result.dropped.length === 1 ? "item" : "items"}: {result.dropped.join(", ")}</span>
                : <span>Everything on this page fits.</span>}
            </li>)}
          </ul>
          <div className="builder__dialog-actions"><button type="button" onClick={() => setFitOpen(false)}>Done</button></div>
        </div>
      </> : null}

      {confirmPageDelete ? (
        <>
          <div className="builder__scrim" />
          <div className="builder__dialog" role="dialog" aria-modal="true" aria-labelledby="delete-page-title" ref={pageDeleteRef} data-testid="delete-page-dialog">
            <h2 id="delete-page-title">Delete {confirmPageDelete.name}?</h2>
            <p>
              {confirmPageDelete.sectionCount > 0 ? `${confirmPageDelete.sectionCount} ${confirmPageDelete.sectionCount === 1 ? "section" : "sections"} will be ${confirmPageDelete.mode === "move" ? "moved" : "deleted"}. Library items will be kept. ` : "This page is empty. "}{assignments.filter(assignment => assignment.pageId === confirmPageDelete.pageId).length > 0
                ? `${assignments.filter(assignment => assignment.pageId === confirmPageDelete.pageId).map(assignment => screens.find(screen => screen.screenId === assignment.screenId)?.screenName ?? "Unknown screen").join(", ")} will lose this assignment.`
                : "No screens are assigned to this page."}
            </p>
            {confirmPageDelete.sectionCount > 0 ? <fieldset className="builder__delete-page-choice">
              <legend>What should happen to its sections?</legend>
              <label><input type="radio" name="delete-page-mode" value="move" checked={confirmPageDelete.mode === "move"} onChange={() => setConfirmPageDelete(current => current ? { ...current, mode: "move" } : null)} /> Move them to another page</label>
              {confirmPageDelete.mode === "move" ? <label>
              Destination page
              <select
                value={confirmPageDelete.destinationPageId}
                onChange={event => setConfirmPageDelete(current => current ? { ...current, destinationPageId: event.target.value } : null)}
                data-testid="delete-page-destination"
              >
                {pages.filter(page => page.pageId !== confirmPageDelete.pageId).map(page => <option key={page.pageId} value={page.pageId}>{page.name}</option>)}
              </select>
              </label> : null}
              <label><input type="radio" name="delete-page-mode" value="delete" checked={confirmPageDelete.mode === "delete"} onChange={() => setConfirmPageDelete(current => current ? { ...current, mode: "delete" } : null)} /> Delete the page and its sections</label>
            </fieldset> : null}
            <div className="builder__dialog-actions">
              <button type="button" className="action-secondary" onClick={() => setConfirmPageDelete(null)}>Cancel</button>
              <button type="button" className="action-danger" disabled={confirmPageDelete.sectionCount > 0 && confirmPageDelete.mode === "move" && !confirmPageDelete.destinationPageId} onClick={() => void removePage(confirmPageDelete.pageId, confirmPageDelete.sectionCount > 0 && confirmPageDelete.mode === "move" ? confirmPageDelete.destinationPageId : undefined, confirmPageDelete.sectionCount > 0 && confirmPageDelete.mode === "delete")}>Delete page</button>
            </div>
          </div>
        </>
      ) : null}
    </div>
  );
}
