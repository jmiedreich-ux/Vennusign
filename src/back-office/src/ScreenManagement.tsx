import { useEffect, useState, type FormEvent } from "react";
import {
  claimPairingCode,
  completeScreenReplacement,
  createManagedScreen,
  loadScreenOverflow,
  loadManagedScreens,
  previewScreenReplacement,
  pushManagedScreen,
  resetManagedScreen,
  setManagedScreenArchived,
  unpairManagedScreen,
  updateManagedScreen,
  BackOfficeApiError,
  type ManagedScreen,
  type ScreenOverflowPreview,
  type ScreenReplacementResult
} from "./api";
import type { BackOfficeConfiguration } from "./config";
import VideoWallBuilder from "./VideoWallBuilder";
import {
  identityHasChanges,
  screenPresentationHasChanges,
  updateIdentityDraft,
  updateScreenPresentationDraft,
  type ScreenIdentityDraft,
  type ScreenPresentationDraft
} from "./actionRecovery.mjs";
import { useDestructiveReview } from "./DestructiveReviewDialog";
import TransientFeedback from "./TransientFeedback";
import EmptyState from "./EmptyState";
import LoadingSkeleton from "./LoadingSkeleton";

type Props = {
  configuration: BackOfficeConfiguration;
  apiKey: string;
  venueId: string;
  allLayoutsEnabled: boolean;
  maxScreens?: number;
  videoWallEnabled: boolean;
  showUpgradePrompt?: boolean;
};

function previewTitle(screen: ManagedScreen) {
  if (screen.displayLayout === "split_layout") return `${screen.name} Split Layout TV preview`;
  if (screen.displayLayout === "daily_special_hero") return `${screen.name} Daily Special Hero TV preview`;
  if (screen.displayLayout === "classic_chalkboard") return `${screen.name} Classic Chalkboard TV preview`;
  if (screen.displayLayout === "tap_strips") return `${screen.name} Tap Strips TV preview`;
  return `${screen.name} Digital Tap Board TV preview`;
}

export default function ScreenManagement({
  configuration,
  apiKey,
  venueId,
  allLayoutsEnabled,
  maxScreens,
  videoWallEnabled,
  showUpgradePrompt = true
}: Props) {
  const [screens, setScreens] = useState<ManagedScreen[]>([]);
  const [screensLoading, setScreensLoading] = useState(true);
  const [newName, setNewName] = useState("");
  const [newLocation, setNewLocation] = useState("");
  const [busyId, setBusyId] = useState<string>();
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const [capacity, setCapacity] = useState(6);
  const [overflow, setOverflow] = useState<ScreenOverflowPreview>();
  const [previewRevision, setPreviewRevision] = useState(0);
  const [previewScreenId, setPreviewScreenId] = useState("");
  const [identityDrafts, setIdentityDrafts] = useState<Record<string, ScreenIdentityDraft>>({});
  const [presentationDrafts, setPresentationDrafts] = useState<Record<string, ScreenPresentationDraft>>({});
  const [pairingCode, setPairingCode] = useState("");
  const [replacementCode, setReplacementCode] = useState("");
  const [replacementTargetId, setReplacementTargetId] = useState("");
  const [replacementPreview, setReplacementPreview] = useState<ScreenReplacementResult>();
  const [screenSearch, setScreenSearch] = useState("");
  const [healthFilter, setHealthFilter] = useState("all");
  const [selectedScreenId, setSelectedScreenId] = useState("");
  const [delivery, setDelivery] = useState<{ screenId: string; state: "pending" | "requested" | "received" | "applied" | "recovered" | "superseded" | "offline" | "failed"; requestedUtc: string; reason?: string }>();
  const [setupOpen, setSetupOpen] = useState(true);
  const { review, reviewDialog } = useDestructiveReview();

  const refresh = async () => {
    const current = await loadManagedScreens(configuration, apiKey, venueId);
    setScreens(current);
    setSelectedScreenId(selected => current.some(screen => screen.id === selected && screen.status.toLowerCase() !== "archived") ? selected : "");
    return current;
  };
  useEffect(() => {
    setScreensLoading(true);
    refresh()
      .then(current => setSetupOpen(!current.some(screen => screen.status.toLowerCase() !== "archived")))
      .catch(() => setError("Screens could not be loaded."))
      .finally(() => setScreensLoading(false));
  }, [apiKey, configuration, venueId]);
  useEffect(() => {
    const poll = () => { if (document.visibilityState === "visible") void refresh().catch(() => undefined); };
    const timer = window.setInterval(poll, 10_000);
    window.addEventListener("online", poll);
    document.addEventListener("visibilitychange", poll);
    return () => { window.clearInterval(timer); window.removeEventListener("online", poll); document.removeEventListener("visibilitychange", poll); };
  }, [apiKey, configuration, venueId]);
  useEffect(() => {
    loadScreenOverflow(configuration, apiKey, venueId, capacity)
      .then(setOverflow)
      .catch(() => setError("The layout preview could not be loaded."));
  }, [apiKey, capacity, configuration, venueId]);

  const create = async (event: FormEvent) => {
    event.preventDefault();
    setBusyId("new"); setError(undefined); setNotice(undefined);
    try {
      await createManagedScreen(configuration, apiKey, venueId, { name: newName, location: newLocation || undefined });
      setNewName(""); setNewLocation(""); await refresh(); setSetupOpen(false);
    } catch (reason: unknown) {
      setError(reason instanceof BackOfficeApiError && reason.status === 409
        ? "Your plan's screen limit has been reached. Upgrade before adding another screen."
        : "The screen could not be created.");
    }
    finally { setBusyId(undefined); }
  };

  const claim = async (event: FormEvent) => {
    event.preventDefault();
    setBusyId("pair"); setError(undefined); setNotice(undefined);
    try {
      const claimed = await claimPairingCode(configuration, apiKey, venueId, pairingCode);
      setPairingCode("");
      setSelectedScreenId(claimed.screenId);
      setNotice("Screen paired successfully. Pairing is complete; Online appears only after the authoritative player heartbeat arrives.");
      await refresh(); setSetupOpen(false);
    } catch (reason: unknown) {
      const status = reason instanceof BackOfficeApiError ? reason.status : 0;
      setError(status === 404
        ? "Pairing code not found. Check the six digits shown on the player."
        : status === 410
          ? "That pairing code expired. Return to the player and generate a new code."
          : status === 409
            ? "That code was already claimed, or the plan limit was reached. Generate a new code or review screen capacity."
            : "Pairing failed. Keep the player on its pairing screen, check the connection, and try again.");
    }
    finally { setBusyId(undefined); }
  };

  const previewReplacement = async (event: FormEvent) => {
    event.preventDefault(); setBusyId("replacement-preview"); setError(undefined); setNotice(undefined); setReplacementPreview(undefined);
    try { setReplacementPreview(await previewScreenReplacement(configuration, apiKey, replacementTargetId, replacementCode)); }
    catch (reason: unknown) {
      const status = reason instanceof BackOfficeApiError ? reason.status : 0;
      setError(status === 410 ? "That replacement code expired. Generate a new code on the replacement player."
        : status === 409 ? "The replacement cannot continue because the code or screen state changed. Refresh and try a new code."
          : status === 404 ? "The selected screen or replacement pairing code was not found."
            : "The replacement impact could not be checked.");
    } finally { setBusyId(undefined); }
  };

  const completeReplacement = async () => {
    if (!replacementPreview || !await review({ title: `Replace the player for ${replacementPreview.targetName}?`, consequence: "The old player credential will stop working immediately. Screen configuration, delivery history, and video-wall position will stay with the logical screen.", confirmLabel: "Replace player" })) return;
    setBusyId("replacement-complete"); setError(undefined); setNotice(undefined);
    try {
      const result = await completeScreenReplacement(configuration, apiKey, replacementTargetId, replacementCode, replacementPreview.targetUpdatedUtc!);
      setNotice(`${result.targetName ?? "Screen"} now uses the replacement player. Its configuration, history, and video-wall position were preserved.`);
      setReplacementCode(""); setReplacementTargetId(""); setReplacementPreview(undefined); await refresh();
    } catch { setError("The replacement did not complete. Nothing should be partially paired; refresh and retry the same code or generate a new one."); }
    finally { setBusyId(undefined); }
  };

  const patchIdentity = (screen: ManagedScreen, value: Partial<ScreenIdentityDraft>) =>
    setIdentityDrafts(current => ({ ...current, [screen.id]: updateIdentityDraft(current[screen.id], screen, value) }));

  const cancelIdentity = (screenId: string) =>
    setIdentityDrafts(current => { const next = { ...current }; delete next[screenId]; return next; });

  const patchPresentation = (screen: ManagedScreen, value: Partial<ScreenPresentationDraft>) =>
    setPresentationDrafts(current => ({ ...current, [screen.id]: updateScreenPresentationDraft(current[screen.id], screen, value) }));

  const cancelPresentation = (screenId: string) =>
    setPresentationDrafts(current => { const next = { ...current }; delete next[screenId]; return next; });

  const save = async (screen: ManagedScreen, completedDraft: "identity" | "presentation") => {
    setBusyId(screen.id); setError(undefined); setNotice(undefined);
    try {
      await updateManagedScreen(configuration, apiKey, venueId, screen.id, {
        name: screen.name,
        location: screen.location,
        photoGridDensity: screen.photoGridDensity,
        displayLayout: screen.displayLayout,
        splitRatio: screen.splitRatio,
        heroDwellSeconds: screen.heroDwellSeconds
      });
      if (completedDraft === "identity") cancelIdentity(screen.id);
      else cancelPresentation(screen.id);
      await refresh();
      setPreviewRevision(current => current + 1);
    } catch { setError("The screen details could not be saved."); }
    finally { setBusyId(undefined); }
  };

  const push = async (screen: ManagedScreen) => {
    setSelectedScreenId(screen.id);
    const requestedUtc = new Date().toISOString();
    setDelivery({ screenId: screen.id, state: "pending", requestedUtc });
    setBusyId(screen.id); setError(undefined); setNotice(undefined);
    try {
      await pushManagedScreen(configuration, apiKey, venueId, screen.id);
      const current = await loadManagedScreens(configuration, apiKey, venueId); setScreens(current);
      const updated = current.find(item => item.id === screen.id);
      const online = screen.status.toLowerCase() === "online" && !isStale(screen);
      const state = (updated?.deliveryState?.toLowerCase() ?? (online ? "requested" : "offline")) as "requested" | "received" | "applied" | "recovered" | "superseded" | "offline" | "failed";
      setDelivery({ screenId: screen.id, state, requestedUtc, reason: online ? `Revision ${updated?.authoritativeRevision ?? "new"} is awaiting player application.` : "Player is offline or stale; the latest revision will recover after reconnect." });
      setNotice(online ? `Revision ${updated?.authoritativeRevision ?? "new"} requested for ${screen.name}.` : `${screen.name} will apply revision ${updated?.authoritativeRevision ?? "new"} after reconnecting.`);
    } catch { setDelivery({ screenId: screen.id, state: "failed", requestedUtc, reason: "The API rejected or could not queue this selected-target delivery." }); setError("Content could not be pushed to the selected screen. Retry without changing the target."); }
    finally { setBusyId(undefined); }
  };

  const setArchived = async (screen: ManagedScreen, archived: boolean) => {
    if (archived && !await review({ title: `Archive ${screen.name}?`, consequence: "This screen will stop receiving content and will no longer count as active. It can be restored later.", confirmLabel: "Archive screen", tone: "caution" })) return;
    setBusyId(screen.id); setError(undefined); setNotice(undefined);
    try {
      await setManagedScreenArchived(configuration, apiKey, venueId, screen.id, archived);
      setNotice(archived ? `${screen.name} archived.` : `${screen.name} restored and ready to reconnect.`);
      await refresh();
    } catch { setError(`The screen could not be ${archived ? "archived" : "restored"}.`); }
    finally { setBusyId(undefined); }
  };

  const reset = async (screen: ManagedScreen) => {
    if (!await review({ title: `Reset ${screen.name}'s connection state?`, consequence: "The current player session will be cleared and the player must reconnect before it can receive new content.", confirmLabel: "Reset connection", tone: "caution" })) return;
    setBusyId(screen.id); setError(undefined); setNotice(undefined);
    try {
      await resetManagedScreen(configuration, apiKey, venueId, screen.id);
      setNotice(`${screen.name} reset. Reopen or restart the player, then wait for it to report online.`);
      await refresh();
    } catch { setError("The screen connection state could not be reset."); }
    finally { setBusyId(undefined); }
  };

  const unpair = async (screen: ManagedScreen) => {
    if (!await review({ title: `Unpair ${screen.name}?`, consequence: "This releases the screen from the venue and invalidates its current pairing. It cannot be restored from this list; a replacement must use a new pairing code.", confirmLabel: "Unpair screen", typedConfirmation: screen.name })) return;
    setBusyId(screen.id); setError(undefined); setNotice(undefined);
    try {
      await unpairManagedScreen(configuration, apiKey, venueId, screen.id);
      setNotice(`${screen.name} unpaired. Pair the replacement player with a new six-digit code.`);
      await refresh();
    } catch { setError("The screen could not be unpaired."); }
    finally { setBusyId(undefined); }
  };

  const activeScreens = screens.filter(screen => screen.status.toLowerCase() !== "archived");
  const isStale = (screen: ManagedScreen) => Boolean(screen.lastSeen) && Date.now() - new Date(screen.lastSeen!).getTime() > 5 * 60 * 1000;
  const visibleScreens = screens.filter(screen => {
    const normalizedStatus = screen.status.toLowerCase();
    const matchesHealth = healthFilter === "all"
      || healthFilter === "stale" && normalizedStatus !== "archived" && isStale(screen)
      || healthFilter === "archived" && normalizedStatus === "archived"
      || healthFilter === normalizedStatus;
    const query = screenSearch.trim().toLowerCase();
    return matchesHealth && (!query || `${screen.name} ${screen.location ?? ""} ${screen.platform ?? ""}`.toLowerCase().includes(query));
  });
  const hasFiniteScreenLimit = typeof maxScreens === "number" && maxScreens >= 0;
  const screenLimitReached = hasFiniteScreenLimit && activeScreens.length >= maxScreens;
  const screenUsage = screensLoading || typeof maxScreens !== "number"
    ? undefined
    : maxScreens < 0
      ? `${activeScreens.length} active screens · Unlimited by plan`
      : `${activeScreens.length} of ${maxScreens} active screens${screenLimitReached ? " · Plan limit reached" : ""}`;
  const selectedScreen = activeScreens.find(screen => screen.id === selectedScreenId);

  return <article className="screen-management">
    {reviewDialog}
    <div className="screen-management-heading">
      <div><p>Display fleet</p><h3>Screens ({activeScreens.length} active · {screens.length - activeScreens.length} archived)</h3></div>
      <span>{selectedScreen ? `Target: ${selectedScreen.name}` : "No delivery target selected"}</span>
    </div>
    {error ? <p className="state error" role="alert">{error}</p> : null}
    {notice ? <TransientFeedback message={notice} onDismiss={() => setNotice(undefined)} /> : null}
    {screenUsage ? <p className="screen-notice" id="screen-quota-status">{screenUsage}</p> : null}
    {showUpgradePrompt && !allLayoutsEnabled ? <aside className="tier-prompt" role="status"><div><strong>Bar layouts require All Layouts</strong><p>Neon Chalkboard and Split Layout remain visible in the selector. Daily Special Hero remains visible too. Upgrade to Pro or add a venue override to choose them.</p></div></aside> : null}
    <details className="screen-workflow-section screen-workflow-section--setup" open={setupOpen} onToggle={event => setSetupOpen(event.currentTarget.open)}>
      <summary><span>Setup</span><strong>Add, pair, or replace a player</strong><small>{activeScreens.length ? "Collapsed after your first active screen; expand when hardware changes." : "Start here to connect the first display."}</small></summary>
      <div className="screen-workflow-section__body">
        <form className="screen-create" onSubmit={create}>
          <input aria-label="New screen name" maxLength={200} required value={newName} onChange={event => setNewName(event.target.value)} placeholder="Screen name" />
          <input aria-label="New screen location" maxLength={200} value={newLocation} onChange={event => setNewLocation(event.target.value)} placeholder="Location (optional)" />
          <button aria-describedby={screenUsage ? "screen-quota-status" : undefined} disabled={busyId === "new" || screenLimitReached}>Add screen</button>
        </form>
        <form className="screen-create" onSubmit={claim}>
          <input
            aria-label="Six-digit pairing code"
            inputMode="numeric"
            maxLength={6}
            minLength={6}
            pattern="[0-9]{6}"
            required
            value={pairingCode}
            onChange={event => setPairingCode(event.target.value.replace(/\D/g, "").slice(0, 6))}
            placeholder="TV pairing code"
          />
          <button aria-describedby={screenUsage ? "screen-quota-status" : undefined} disabled={busyId === "pair" || screenLimitReached}>Pair screen</button>
        </form>
        <p className="screen-notice" role="status">{busyId === "pair" ? "Pairing pending… keep this page and the player open." : "Pairing codes expire and can be used once. If pairing fails, generate a fresh code on the player before retrying."}</p>
        <section className="screen-delivery-target" aria-labelledby="replacement-heading">
          <div><p>Player lifecycle</p><h4 id="replacement-heading">Replace a player</h4><span>Keeps the logical screen, content settings, history, and video-wall position. This is separate from adding or unpairing a screen.</span></div>
          <form className="screen-create" onSubmit={previewReplacement}>
            <label>Logical screen<select required value={replacementTargetId} onChange={event => { setReplacementTargetId(event.target.value); setReplacementPreview(undefined); }}><option value="">Choose the screen to preserve</option>{activeScreens.map(screen => <option key={screen.id} value={screen.id}>{screen.name}</option>)}</select></label>
            <input aria-label="Replacement player pairing code" inputMode="numeric" maxLength={6} minLength={6} pattern="[0-9]{6}" required value={replacementCode} onChange={event => { setReplacementCode(event.target.value.replace(/\D/g, "").slice(0, 6)); setReplacementPreview(undefined); }} placeholder="Replacement code" />
            <button disabled={busyId === "replacement-preview" || !replacementTargetId}>Review replacement</button>
          </form>
          {replacementPreview ? <div className="delivery-state queued" role="status">
            <strong>Ready to replace {replacementPreview.targetName}</strong>
            <span>New player: {replacementPreview.replacementPlatform ?? "Unknown platform"}{replacementPreview.replacementAppVersion ? ` ${replacementPreview.replacementAppVersion}` : ""}</span>
            <span>Preserves configuration and history{replacementPreview.preservesVideoWall ? ` · Video wall ${replacementPreview.wallGroup ?? "assignment"} position ${replacementPreview.wallPosition ?? "preserved"}` : " · No video-wall assignment"}.</span>
            <button type="button" disabled={busyId === "replacement-complete"} onClick={completeReplacement}>Confirm player replacement</button>
            <button type="button" disabled={busyId === "replacement-complete"} onClick={() => { setReplacementPreview(undefined); setReplacementCode(""); }}>Cancel</button>
          </div> : null}
        </section>
      </div>
    </details>
    <section className="screen-workflow-section" aria-labelledby="daily-screens-heading">
      <header><span>Daily</span><h4 id="daily-screens-heading">Operate and monitor screens</h4><p>Choose one delivery target, preview or push content, and review health without opening setup.</p></header>
      <div className="screen-workflow-section__body">
        <section className="screen-delivery-target" aria-labelledby="screen-target-heading">
          <div><p>Authorized delivery</p><h4 id="screen-target-heading">Select one screen target</h4><span>Preview and Push remain disabled until you deliberately choose an active venue screen.</span></div>
          <label>Target screen<select value={selectedScreenId} onChange={event => { setSelectedScreenId(event.target.value); setDelivery(undefined); }}><option value="">Choose a screen</option>{activeScreens.map(screen => <option key={screen.id} value={screen.id}>{screen.name} · {isStale(screen) ? "Stale" : screen.status}</option>)}</select></label>
          <button type="button" disabled={!selectedScreen || busyId === selectedScreenId} onClick={() => selectedScreen && setPreviewScreenId(selectedScreen.id)}>Preview selected screen</button>
          <button disabled={!selectedScreen || busyId === selectedScreenId} onClick={() => selectedScreen && push(selectedScreen)}>Push structured content</button>
          {delivery && delivery.screenId === selectedScreenId ? <div className={`delivery-state ${delivery.state}`} role="status"><strong>Delivery: {delivery.state}</strong><span>{delivery.reason ?? "Request in progress."}</span><small>Requested {new Date(delivery.requestedUtc).toLocaleString()}</small>{delivery.state === "failed" && selectedScreen ? <button onClick={() => push(selectedScreen)}>Retry selected target</button> : null}</div> : null}
        </section>
        <div className="screen-create">
          <input aria-label="Search screens" value={screenSearch} onChange={event => setScreenSearch(event.target.value)} placeholder="Search name, location, or platform" />
          <label>Health<select value={healthFilter} onChange={event => setHealthFilter(event.target.value)}><option value="all">All screens</option><option value="online">Online</option><option value="offline">Offline</option><option value="stale">Stale</option><option value="archived">Archived</option></select></label>
        </div>
    {screensLoading ? <LoadingSkeleton label="Loading screens…" rows={4} /> : visibleScreens.length ? <div className="managed-screen-list screen-fleet-grid">{visibleScreens.map(screen => {
      const presentation = presentationDrafts[screen.id] ?? screen;
      const archived = screen.status.toLowerCase() === "archived";
      return <section className="screen-fleet-card" data-selected={selectedScreenId === screen.id} key={screen.id}>
        <div className="screen-fleet-thumbnail">
          {archived
            ? <div className="screen-fleet-thumbnail__unavailable"><span>Archived</span><small>Restore this screen to load its live preview.</small></div>
            : <iframe loading="lazy" tabIndex={-1} aria-hidden="true" src={`${configuration.displayBaseUrl}/display/${screen.id}`} title="" />}
          <span className={`screen-fleet-thumbnail__status ${isStale(screen) && !archived ? "stale" : screen.status.toLowerCase()}`}>{isStale(screen) && !archived ? "Stale" : screen.status}</span>
        </div>
        <header className="screen-fleet-card__heading">
          <div><h5>{screen.name}</h5><span>{screen.location || screen.displayLayout.replaceAll("_", " ")}</span></div>
          <small>{screen.platform ? `${screen.platform}${screen.appVersion ? ` ${screen.appVersion}` : ""}` : "Platform not reported"}</small>
        </header>
        <div className="screen-actions action-surface" aria-label={`${screen.name} delivery actions`}>
          <button className="action-secondary" type="button" disabled={busyId === screen.id || archived} onClick={() => { setSelectedScreenId(screen.id); setDelivery(undefined); setPreviewScreenId(screen.id); }}>Preview</button>
          <button className="action-primary" type="button" disabled={busyId === screen.id || archived} onClick={() => void push(screen)}>Push</button>
          <details className="action-overflow"><summary>More actions</summary><div>
            <a href={screen.registrationUrl} target="_blank" rel="noreferrer">Open registration URL</a>
            {archived
              ? <button className="action-secondary" type="button" disabled={busyId === screen.id} onClick={() => setArchived(screen, false)}>Restore screen</button>
              : <><button className="action-secondary" type="button" disabled={busyId === screen.id} onClick={() => reset(screen)}>Reset connection</button><button className="action-danger" type="button" disabled={busyId === screen.id} onClick={() => setArchived(screen, true)}>Archive</button><button className="action-danger" type="button" disabled={busyId === screen.id} onClick={() => unpair(screen)}>Unpair screen</button></>}
          </div></details>
        </div>
        {screen.id === previewScreenId && !archived ? <div className="split-layout-preview screen-fleet-card__expanded-preview">
          <div><strong>Exact TV preview</strong><span>Uses this screen’s saved menu, theme, and layout settings.</span></div>
          <button type="button" onClick={() => setPreviewScreenId("")}>Close preview</button>
          <iframe
            key={`${screen.id}-${screen.displayLayout}-${screen.splitRatio}-${screen.heroDwellSeconds}-${previewRevision}`}
            src={`${configuration.displayBaseUrl}/display/${screen.id}`}
            title={previewTitle(screen)}
          />
        </div> : null}
        <div className="managed-screen-health">
          <span className={screen.status.toLowerCase()} />
          <div><strong>{isStale(screen) && screen.status.toLowerCase() !== "archived" ? "Stale" : screen.status}</strong><small>{screen.lastSeen ? `Last seen ${new Date(screen.lastSeen).toLocaleString()}` : "Never seen"}{screen.platform ? ` · ${screen.platform}${screen.appVersion ? ` ${screen.appVersion}` : ""}` : ""}</small></div>
        </div>
        <details className="screen-fleet-card__settings">
          <summary>Edit display and identity</summary>
          <div>
        <label>Name<input disabled={screen.status.toLowerCase() === "archived"} maxLength={200} value={identityDrafts[screen.id]?.name ?? screen.name} onChange={event => patchIdentity(screen, { name: event.target.value })} /></label>
        <label>Location<input disabled={screen.status.toLowerCase() === "archived"} maxLength={200} value={identityDrafts[screen.id]?.location ?? screen.location ?? ""} onChange={event => patchIdentity(screen, { location: event.target.value })} /></label>
        {identityHasChanges(screen, identityDrafts[screen.id]) ? <div className="screen-actions" role="status">
          <span>Unsaved screen identity changes</span>
          <button type="button" disabled={busyId === screen.id || !identityDrafts[screen.id]?.name.trim()} onClick={() => void save({ ...screen, name: identityDrafts[screen.id].name, location: identityDrafts[screen.id].location || undefined }, "identity")}>Save changes</button>
          <button type="button" disabled={busyId === screen.id} onClick={() => cancelIdentity(screen.id)}>Cancel changes</button>
        </div> : null}
        <label>Display layout
          <select
            disabled={screen.status.toLowerCase() === "archived"}
            value={presentation.displayLayout}
            onChange={event => patchPresentation(screen, { displayLayout: event.target.value as ManagedScreen["displayLayout"] })}
          >
            <option value="photo_grid">Photo Grid</option>
            <option value="classic_diner">Classic Diner</option>
            <option disabled={!allLayoutsEnabled} value="neon_chalkboard">Neon Chalkboard · Pro</option>
            <option disabled={!allLayoutsEnabled} value="split_layout">Split Layout · Pro</option>
            <option disabled={!allLayoutsEnabled} value="daily_special_hero">Daily Special Hero · Pro</option>
            <option disabled={!allLayoutsEnabled} value="classic_chalkboard">Classic Chalkboard Drinks · Pro</option>
            <option disabled={!allLayoutsEnabled} value="tap_strips">Tap Strips · Pro</option>
            <option disabled={!allLayoutsEnabled} value="digital_tap_board">Digital Tap Board · Pro</option>
          </select>
        </label>
        {presentation.displayLayout === "photo_grid" ? <label>Photo Grid density
          <select
            disabled={screen.status.toLowerCase() === "archived"}
            value={presentation.photoGridDensity}
            onChange={event => patchPresentation(screen, { photoGridDensity: event.target.value as ManagedScreen["photoGridDensity"] })}
          >
            <option value="2x2">2 × 2 · 4 items</option>
            <option value="3x2">3 × 2 · 6 items</option>
            <option value="4x2">4 × 2 · 8 items</option>
            <option value="3x3">3 × 3 · 9 items</option>
          </select>
        </label> : null}
        {presentation.displayLayout === "daily_special_hero" ? <label>Hero rotation
          <select
            disabled={screen.status.toLowerCase() === "archived"}
            value={presentation.heroDwellSeconds}
            onChange={event => patchPresentation(screen, { heroDwellSeconds: Number(event.target.value) })}
          >
            <option value={4}>Every 4 seconds</option>
            <option value={8}>Every 8 seconds · default</option>
            <option value={12}>Every 12 seconds</option>
            <option value={20}>Every 20 seconds</option>
            <option value={30}>Every 30 seconds</option>
          </select>
        </label> : null}
        {presentation.displayLayout === "split_layout" ? <label>Split ratio
          <select
            disabled={screen.status.toLowerCase() === "archived"}
            value={presentation.splitRatio}
            onChange={event => patchPresentation(screen, { splitRatio: event.target.value as ManagedScreen["splitRatio"] })}
          >
            <option value="40_60">40% hero · 60% menu</option>
            <option value="50_50">50% hero · 50% menu</option>
          </select>
        </label> : null}
        {screenPresentationHasChanges(screen, presentationDrafts[screen.id]) ? <div className="screen-presentation-draft sticky-action-bar" role="status">
          <span><strong>Draft layout</strong> · Nothing changes on the TV until you apply.</span>
          <button className="action-primary" type="button" disabled={busyId === screen.id} onClick={() => void save({ ...screen, ...presentation }, "presentation")}>Apply to TV</button>
          <button className="action-secondary" type="button" disabled={busyId === screen.id} onClick={() => cancelPresentation(screen.id)}>Discard changes</button>
        </div> : null}
          </div>
        </details>
        {screen.authoritativeRevision ? <p className={`delivery-state ${(screen.deliveryState ?? "Requested").toLowerCase()}`} role="status">
          Revision {screen.authoritativeRevision}: {screen.deliveryState ?? "Requested"}{screen.appliedRevision ? ` · applied ${screen.appliedRevision}` : " · acknowledgement pending"}
          {isStale(screen) ? " · player stale/offline" : ""}
          {screen.deliveryFailureCode ? ` · ${screen.deliveryFailureCode}${screen.deliveryFailureDetail ? `: ${screen.deliveryFailureDetail}` : ""}` : ""}
        </p> : null}
      </section>;
    })}</div> : <EmptyState icon={screens.length ? "search" : "screen"} title={screens.length ? "No matching screens" : "No screens assigned"} message={screens.length ? "Adjust the search or health filter to return to the fleet." : "Add or pair the first venue screen from Setup before sending content."} action={<button type="button" onClick={() => screens.length ? (setScreenSearch(""), setHealthFilter("all")) : setSetupOpen(true)}>{screens.length ? "Clear screen filters" : "Open screen setup"}</button>} />}
      </div>
    </section>
    <section className="screen-workflow-section" aria-labelledby="capacity-walls-heading">
      <header><span>Capacity &amp; walls</span><h4 id="capacity-walls-heading">Plan content fit and multi-screen layouts</h4><p>Preview deterministic overflow and manage video walls separately from daily delivery.</p></header>
      <div className="screen-workflow-section__body">
    <section className="overflow-preview">
      <div>
        <p>Layout capacity</p>
        <h4>Overflow preview</h4>
        <span>Deterministic menu order shows exactly which items fit.</span>
      </div>
      <label>Layout
        <select value={capacity} onChange={event => setCapacity(Number(event.target.value))}>
          <option value={4}>2 × 2 · 4 items</option>
          <option value={6}>3 × 2 · 6 items</option>
          <option value={8}>4 × 2 · 8 items</option>
          <option value={9}>3 × 3 · 9 items</option>
        </select>
      </label>
      <div className="overflow-counts">
        <strong>{overflow?.visibleItems ?? 0}<small>Visible</small></strong>
        <strong className={(overflow?.overflowItems ?? 0) > 0 ? "warning" : ""}>{overflow?.overflowItems ?? 0}<small>Overflow</small></strong>
      </div>
      {overflow?.items.length ? <ol>{overflow.items.map(item =>
        <li className={item.visible ? "" : "overflow"} key={item.itemId}>
          <span>{item.itemName}</span><small>{item.sectionName} · {item.visible ? "Visible" : "Overflow"}</small>
        </li>)}</ol> : <p>No available menu items to preview.</p>}
    </section>
    {videoWallEnabled ? <VideoWallBuilder configuration={configuration} apiKey={apiKey} venueId={venueId} screens={activeScreens} showUpgradePrompt={showUpgradePrompt} /> : <p className="screen-capability-note">Video walls are not enabled for this venue. Layout capacity remains available for every plan.</p>}
      </div>
    </section>
  </article>;
}
