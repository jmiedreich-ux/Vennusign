import { useEffect, useState, type FormEvent } from "react";
import {
  createManagedScreen,
  loadManagedScreens,
  pushManagedScreen,
  updateManagedScreen,
  type ManagedScreen
} from "./api";
import type { AdminConfiguration } from "./config";

type Props = { configuration: AdminConfiguration; apiKey: string; venueId: string };

export default function ScreenManagement({ configuration, apiKey, venueId }: Props) {
  const [screens, setScreens] = useState<ManagedScreen[]>([]);
  const [newName, setNewName] = useState("");
  const [newLocation, setNewLocation] = useState("");
  const [busyId, setBusyId] = useState<string>();
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();

  const refresh = () => loadManagedScreens(configuration, apiKey, venueId).then(setScreens);
  useEffect(() => { refresh().catch(() => setError("Screens could not be loaded.")); }, [apiKey, configuration, venueId]);

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
        location: screen.location
      });
      await refresh();
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

  return <article className="screen-management">
    <div className="screen-management-heading">
      <div><p>Display fleet</p><h3>Screens ({screens.length})</h3></div>
      <span>Health and registration</span>
    </div>
    {error ? <p className="state error">{error}</p> : null}
    {notice ? <p className="screen-notice" role="status">{notice}</p> : null}
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
        <div className="screen-actions">
          <a href={screen.registrationUrl} target="_blank" rel="noreferrer">Open registration URL</a>
          <button disabled={busyId === screen.id} onClick={() => push(screen)}>Push content</button>
        </div>
      </section>)}</div> : <p>No screens assigned.</p>}
  </article>;
}
