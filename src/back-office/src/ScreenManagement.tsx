import { useEffect, useState, type FormEvent } from "react";
import {
  claimPairingCode,
  createManagedScreen,
  loadScreenOverflow,
  loadManagedScreens,
  pushAllManagedScreens,
  pushManagedScreen,
  resetManagedScreen,
  setManagedScreenArchived,
  unpairManagedScreen,
  updateManagedScreen,
  BackOfficeApiError,
  type ManagedScreen,
  type ScreenOverflowPreview
} from "./api";
import type { BackOfficeConfiguration } from "./config";
import VideoWallBuilder from "./VideoWallBuilder";

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
  const [pairingCode, setPairingCode] = useState("");
  const [screenSearch, setScreenSearch] = useState("");
  const [healthFilter, setHealthFilter] = useState("all");

  const refresh = () => loadManagedScreens(configuration, apiKey, venueId).then(setScreens);
  useEffect(() => {
    setScreensLoading(true);
    refresh()
      .catch(() => setError("Screens could not be loaded."))
      .finally(() => setScreensLoading(false));
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
      setNewName(""); setNewLocation(""); await refresh();
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
      await claimPairingCode(configuration, apiKey, venueId, pairingCode);
      setPairingCode("");
      setNotice("Screen paired successfully.");
      await refresh();
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

  const patch = (screenId: string, value: Partial<ManagedScreen>) =>
    setScreens(current => current.map(screen => screen.id === screenId ? { ...screen, ...value } : screen));

  const save = async (screen: ManagedScreen) => {
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
      await refresh();
      setPreviewRevision(current => current + 1);
    } catch { setError("The screen details could not be saved."); }
    finally { setBusyId(undefined); }
  };

  const push = async (screen: ManagedScreen) => {
    setBusyId(screen.id); setError(undefined); setNotice(undefined);
    try {
      await pushManagedScreen(configuration, apiKey, venueId, screen.id);
      setNotice(screen.status.toLowerCase() === "online"
        ? `Update queued for ${screen.name}.`
        : `Update queued for ${screen.name}; it will apply when the player reconnects.`);
    } catch { setError("Content could not be pushed to the screen."); }
    finally { setBusyId(undefined); }
  };

  const pushAll = async () => {
    setBusyId("all"); setError(undefined); setNotice(undefined);
    try {
      const result = await pushAllManagedScreens(configuration, apiKey, venueId);
      setNotice(result.screenCount
        ? `Content pushed to all ${result.screenCount} screens.`
        : "No assigned screens to push.");
    } catch { setError("Content could not be pushed to all screens."); }
    finally { setBusyId(undefined); }
  };

  const setArchived = async (screen: ManagedScreen, archived: boolean) => {
    if (archived && !window.confirm(`Archive ${screen.name}? It will stop receiving content and can be restored later.`)) return;
    setBusyId(screen.id); setError(undefined); setNotice(undefined);
    try {
      await setManagedScreenArchived(configuration, apiKey, venueId, screen.id, archived);
      setNotice(archived ? `${screen.name} archived.` : `${screen.name} restored and ready to reconnect.`);
      await refresh();
    } catch { setError(`The screen could not be ${archived ? "archived" : "restored"}.`); }
    finally { setBusyId(undefined); }
  };

  const reset = async (screen: ManagedScreen) => {
    if (!window.confirm(`Reset ${screen.name}'s connection state? The player will need to reconnect.`)) return;
    setBusyId(screen.id); setError(undefined); setNotice(undefined);
    try {
      await resetManagedScreen(configuration, apiKey, venueId, screen.id);
      setNotice(`${screen.name} reset. Reopen or restart the player, then wait for it to report online.`);
      await refresh();
    } catch { setError("The screen connection state could not be reset."); }
    finally { setBusyId(undefined); }
  };

  const unpair = async (screen: ManagedScreen) => {
    if (!window.confirm(`Unpair ${screen.name}? This releases it from the venue for replacement. This cannot be undone from this list.`)) return;
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

  return <article className="screen-management">
    <div className="screen-management-heading">
      <div><p>Display fleet</p><h3>Screens ({activeScreens.length} active · {screens.length - activeScreens.length} archived)</h3></div>
      <button className="push-all" disabled={busyId === "all" || activeScreens.length === 0} onClick={pushAll}>Push to all active screens</button>
    </div>
    {error ? <p className="state error" role="alert">{error}</p> : null}
    {notice ? <p className="screen-notice" role="status">{notice}</p> : null}
    {screenUsage ? <p className="screen-notice" id="screen-quota-status">{screenUsage}</p> : null}
    {showUpgradePrompt && !allLayoutsEnabled ? <aside className="tier-prompt" role="status"><div><strong>Bar layouts require All Layouts</strong><p>Neon Chalkboard and Split Layout remain visible in the selector. Daily Special Hero remains visible too. Upgrade to Pro or add a venue override to choose them.</p></div></aside> : null}
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
    <div className="screen-create">
      <input aria-label="Search screens" value={screenSearch} onChange={event => setScreenSearch(event.target.value)} placeholder="Search name, location, or platform" />
      <label>Health<select value={healthFilter} onChange={event => setHealthFilter(event.target.value)}><option value="all">All screens</option><option value="online">Online</option><option value="offline">Offline</option><option value="stale">Stale</option><option value="archived">Archived</option></select></label>
    </div>
    {screensLoading ? <p role="status">Loading screens…</p> : visibleScreens.length ? <div className="managed-screen-list">{visibleScreens.map(screen =>
      <section key={screen.id}>
        <div className="managed-screen-health">
          <span className={screen.status.toLowerCase()} />
          <div><strong>{isStale(screen) && screen.status.toLowerCase() !== "archived" ? "Stale" : screen.status}</strong><small>{screen.lastSeen ? `Last seen ${new Date(screen.lastSeen).toLocaleString()}` : "Never seen"}{screen.platform ? ` · ${screen.platform}${screen.appVersion ? ` ${screen.appVersion}` : ""}` : ""}</small></div>
        </div>
        <label>Name<input disabled={screen.status.toLowerCase() === "archived"} maxLength={200} value={screen.name} onChange={event => patch(screen.id, { name: event.target.value })} onBlur={() => save(screen)} /></label>
        <label>Location<input disabled={screen.status.toLowerCase() === "archived"} maxLength={200} value={screen.location ?? ""} onChange={event => patch(screen.id, { location: event.target.value || undefined })} onBlur={() => save(screen)} /></label>
        <label>Display layout
          <select
            disabled={screen.status.toLowerCase() === "archived"}
            value={screen.displayLayout}
            onChange={event => {
              const updated = { ...screen, displayLayout: event.target.value as ManagedScreen["displayLayout"] };
              patch(screen.id, { displayLayout: updated.displayLayout });
              void save(updated);
            }}
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
        {screen.displayLayout === "photo_grid" ? <label>Photo Grid density
          <select
            disabled={screen.status.toLowerCase() === "archived"}
            value={screen.photoGridDensity}
            onChange={event => {
              const updated = { ...screen, photoGridDensity: event.target.value as ManagedScreen["photoGridDensity"] };
              patch(screen.id, { photoGridDensity: updated.photoGridDensity });
              void save(updated);
            }}
          >
            <option value="2x2">2 × 2 · 4 items</option>
            <option value="3x2">3 × 2 · 6 items</option>
            <option value="4x2">4 × 2 · 8 items</option>
            <option value="3x3">3 × 3 · 9 items</option>
          </select>
        </label> : null}
        {screen.displayLayout === "daily_special_hero" ? <label>Hero rotation
          <select
            disabled={screen.status.toLowerCase() === "archived"}
            value={screen.heroDwellSeconds}
            onChange={event => {
              const updated = { ...screen, heroDwellSeconds: Number(event.target.value) };
              patch(screen.id, { heroDwellSeconds: updated.heroDwellSeconds });
              void save(updated);
            }}
          >
            <option value={4}>Every 4 seconds</option>
            <option value={8}>Every 8 seconds · default</option>
            <option value={12}>Every 12 seconds</option>
            <option value={20}>Every 20 seconds</option>
            <option value={30}>Every 30 seconds</option>
          </select>
        </label> : null}
        {screen.displayLayout === "split_layout" ? <label>Split ratio
          <select
            disabled={screen.status.toLowerCase() === "archived"}
            value={screen.splitRatio}
            onChange={event => {
              const updated = { ...screen, splitRatio: event.target.value as ManagedScreen["splitRatio"] };
              patch(screen.id, { splitRatio: updated.splitRatio });
              void save(updated);
            }}
          >
            <option value="40_60">40% hero · 60% menu</option>
            <option value="50_50">50% hero · 50% menu</option>
          </select>
        </label> : null}
        <div className="screen-actions">
          <a href={screen.registrationUrl} target="_blank" rel="noreferrer">Open registration URL</a>
          <button type="button" disabled={busyId === screen.id || screen.status.toLowerCase() === "archived"} onClick={() => push(screen)}>Push content</button>
          {screen.status.toLowerCase() === "archived"
            ? <button type="button" disabled={busyId === screen.id} onClick={() => setArchived(screen, false)}>Restore</button>
            : <><button type="button" disabled={busyId === screen.id} onClick={() => reset(screen)}>Reset connection</button><button type="button" disabled={busyId === screen.id} onClick={() => setArchived(screen, true)}>Archive</button><button type="button" disabled={busyId === screen.id} onClick={() => unpair(screen)}>Unpair for replacement</button></>}
        </div>
        {screen.status.toLowerCase() !== "archived" && ["split_layout", "daily_special_hero", "classic_chalkboard", "tap_strips", "digital_tap_board"].includes(screen.displayLayout) ? <div className="split-layout-preview">
          <div><strong>Exact TV preview</strong><span>Uses this screen’s saved menu, theme, and layout settings.</span></div>
          <iframe
            key={`${screen.id}-${screen.displayLayout}-${screen.splitRatio}-${screen.heroDwellSeconds}-${previewRevision}`}
            src={`${configuration.displayBaseUrl}/display/${screen.id}`}
            title={previewTitle(screen)}
          />
        </div> : null}
      </section>)}</div> : <p>{screens.length ? "No screens match the current filters." : "No screens assigned."}</p>}
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
    {videoWallEnabled ? <VideoWallBuilder configuration={configuration} apiKey={apiKey} venueId={venueId} screens={activeScreens} showUpgradePrompt={showUpgradePrompt} /> : null}
  </article>;
}
