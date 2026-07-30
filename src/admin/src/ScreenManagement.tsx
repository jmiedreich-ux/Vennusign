import { useEffect, useState, type FormEvent } from "react";
import {
  createManagedScreen,
  loadScreenOverflow,
  loadManagedScreens,
  pushAllManagedScreens,
  pushManagedScreen,
  updateManagedScreen,
  type ManagedScreen,
  type ScreenOverflowPreview
} from "./api";
import type { AdminConfiguration } from "./config";
import VideoWallBuilder from "./VideoWallBuilder";

type Props = { configuration: AdminConfiguration; apiKey: string; venueId: string; allLayoutsEnabled: boolean };

export default function ScreenManagement({ configuration, apiKey, venueId, allLayoutsEnabled }: Props) {
  const [screens, setScreens] = useState<ManagedScreen[]>([]);
  const [newName, setNewName] = useState("");
  const [newLocation, setNewLocation] = useState("");
  const [busyId, setBusyId] = useState<string>();
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const [capacity, setCapacity] = useState(6);
  const [overflow, setOverflow] = useState<ScreenOverflowPreview>();
  const [previewRevision, setPreviewRevision] = useState(0);

  const refresh = () => loadManagedScreens(configuration, apiKey, venueId).then(setScreens);
  useEffect(() => { refresh().catch(() => setError("Screens could not be loaded.")); }, [apiKey, configuration, venueId]);
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
    } catch { setError("The screen could not be created."); }
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
      setNotice(`Content pushed to ${screen.name}.`);
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

  return <article className="screen-management">
    <div className="screen-management-heading">
      <div><p>Display fleet</p><h3>Screens ({screens.length})</h3></div>
      <button className="push-all" disabled={busyId === "all"} onClick={pushAll}>Push to all screens</button>
    </div>
    {error ? <p className="state error">{error}</p> : null}
    {notice ? <p className="screen-notice" role="status">{notice}</p> : null}
    {!allLayoutsEnabled ? <aside className="tier-prompt" role="status"><div><strong>Bar layouts require All Layouts</strong><p>Neon Chalkboard and Split Layout remain visible in the selector. Daily Special Hero remains visible too. Upgrade to Pro or add a venue override to choose them.</p></div></aside> : null}
    <form className="screen-create" onSubmit={create}>
      <input aria-label="New screen name" maxLength={200} required value={newName} onChange={event => setNewName(event.target.value)} placeholder="Screen name" />
      <input aria-label="New screen location" maxLength={200} value={newLocation} onChange={event => setNewLocation(event.target.value)} placeholder="Location (optional)" />
      <button disabled={busyId === "new"}>Add screen</button>
    </form>
    {screens.length ? <div className="managed-screen-list">{screens.map(screen =>
      <section key={screen.id}>
        <div className="managed-screen-health">
          <span className={screen.status.toLowerCase()} />
          <div><strong>{screen.status}</strong><small>{screen.lastSeen ? `Last seen ${new Date(screen.lastSeen).toLocaleString()}` : "Never seen"}</small></div>
        </div>
        <label>Name<input maxLength={200} value={screen.name} onChange={event => patch(screen.id, { name: event.target.value })} onBlur={() => save(screen)} /></label>
        <label>Location<input maxLength={200} value={screen.location ?? ""} onChange={event => patch(screen.id, { location: event.target.value || undefined })} onBlur={() => save(screen)} /></label>
        <label>Display layout
          <select
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
          </select>
        </label>
        {screen.displayLayout === "photo_grid" ? <label>Photo Grid density
          <select
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
          <button disabled={busyId === screen.id} onClick={() => push(screen)}>Push content</button>
        </div>
        {["split_layout", "daily_special_hero", "classic_chalkboard"].includes(screen.displayLayout) ? <div className="split-layout-preview">
          <div><strong>Exact TV preview</strong><span>Uses this screen’s saved menu, theme, and layout settings.</span></div>
          <iframe
            key={`${screen.id}-${screen.displayLayout}-${screen.splitRatio}-${screen.heroDwellSeconds}-${previewRevision}`}
            src={`${configuration.displayBaseUrl}/display/${screen.id}`}
            title={screen.displayLayout === "split_layout"
              ? `${screen.name} Split Layout TV preview`
              : screen.displayLayout === "daily_special_hero"
                ? `${screen.name} Daily Special Hero TV preview`
                : `${screen.name} Classic Chalkboard TV preview`}
          />
        </div> : null}
      </section>)}</div> : <p>No screens assigned.</p>}
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
    <VideoWallBuilder configuration={configuration} apiKey={apiKey} venueId={venueId} screens={screens} />
  </article>;
}
