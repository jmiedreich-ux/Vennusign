import { useEffect, useState, type FormEvent } from "react";
import { loadVideoWalls, removeVideoWall, saveVideoWall, type ManagedScreen, type VideoWallSnapshot } from "./api";
import type { AdminConfiguration } from "./config";

type Props = { configuration: AdminConfiguration; apiKey: string; venueId: string; screens: ManagedScreen[] };
const layoutSizes: Record<string, number> = { "2x1": 2, "3x1": 3, "2x2": 4 };

export default function VideoWallBuilder({ configuration, apiKey, venueId, screens }: Props) {
  const [snapshot, setSnapshot] = useState<VideoWallSnapshot>();
  const [name, setName] = useState("Main wall");
  const [layout, setLayout] = useState("2x1");
  const [selected, setSelected] = useState<string[]>([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string>();

  const refresh = () => loadVideoWalls(configuration, apiKey, venueId).then(setSnapshot);
  useEffect(() => { refresh().catch(() => setError("Video walls could not be loaded.")); }, [apiKey, configuration, venueId]);

  const toggle = (screenId: string) =>
    setSelected(current => current.includes(screenId) ? current.filter(id => id !== screenId) : [...current, screenId]);

  const save = async (event: FormEvent) => {
    event.preventDefault(); setBusy(true); setError(undefined);
    try {
      await saveVideoWall(configuration, apiKey, venueId, { name, layout, screenIds: selected });
      setSelected([]); await refresh();
    } catch { setError("The video wall could not be saved."); }
    finally { setBusy(false); }
  };

  const remove = async (wallName: string) => {
    setBusy(true); setError(undefined);
    try { await removeVideoWall(configuration, apiKey, venueId, wallName); await refresh(); }
    catch { setError("The video wall could not be removed."); }
    finally { setBusy(false); }
  };

  return <section className="video-wall-builder">
    <div className="video-wall-heading"><div><p>Pro layout</p><h4>Video wall builder</h4></div><span>2×1 · 3×1 · 2×2</span></div>
    {snapshot && !snapshot.enabled ? <aside className="tier-prompt" role="status"><div><strong>Video Wall is a higher-tier feature</strong><p>The builder stays visible so you can preview the workflow. Upgrade or add a venue override to configure walls.</p></div></aside> : null}
    {error ? <p className="state error">{error}</p> : null}
    <form onSubmit={save}>
      <label>Wall name<input maxLength={100} required value={name} onChange={event => setName(event.target.value)} disabled={!snapshot?.enabled} /></label>
      <label>Configuration<select value={layout} onChange={event => { setLayout(event.target.value); setSelected([]); }} disabled={!snapshot?.enabled}><option value="2x1">2 × 1</option><option value="3x1">3 × 1</option><option value="2x2">2 × 2</option></select></label>
      <fieldset disabled={!snapshot?.enabled}><legend>Assign screens in position order ({selected.length}/{layoutSizes[layout]})</legend>{screens.map(screen =>
        <label key={screen.id}><input type="checkbox" checked={selected.includes(screen.id)} disabled={!selected.includes(screen.id) && selected.length >= layoutSizes[layout]} onChange={() => toggle(screen.id)} /> <span>{screen.name}<small>{screen.location ?? "No location"}</small></span></label>)}</fieldset>
      <button disabled={busy || !snapshot?.enabled || selected.length !== layoutSizes[layout]}>Save wall</button>
    </form>
    {snapshot?.groups.length ? <div className="wall-groups">{snapshot.groups.map(group =>
      <article key={group.name}><div><strong>{group.name}</strong><span>{group.layout}</span></div><ol>{group.screens.map(screen => <li key={screen.id}><span>{screen.position}</span>{screen.name}</li>)}</ol><button disabled={busy || !snapshot.enabled} onClick={() => remove(group.name)}>Remove wall</button></article>)}</div> : <p>No video walls configured.</p>}
  </section>;
}
