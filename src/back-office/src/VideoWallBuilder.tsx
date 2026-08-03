import { useEffect, useState, type FormEvent } from "react";
import { loadVideoWalls, removeVideoWall, saveVideoWall, type ManagedScreen, type VideoWallSnapshot } from "./api";
import type { BackOfficeConfiguration } from "./config";
import { useDestructiveReview } from "./DestructiveReviewDialog";

type Props = { configuration: BackOfficeConfiguration; apiKey: string; venueId: string; screens: ManagedScreen[]; showUpgradePrompt?: boolean };
const layoutSizes: Record<string, number> = { "2x1": 2, "3x1": 3, "2x2": 4 };

export default function VideoWallBuilder({ configuration, apiKey, venueId, screens, showUpgradePrompt = true }: Props) {
  const [snapshot, setSnapshot] = useState<VideoWallSnapshot>();
  const [name, setName] = useState("Main wall");
  const [layout, setLayout] = useState("2x1");
  const [selected, setSelected] = useState<string[]>([]);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const [editingName, setEditingName] = useState<string>();
  const { review, reviewDialog } = useDestructiveReview();

  const refresh = () => loadVideoWalls(configuration, apiKey, venueId).then(setSnapshot);
  useEffect(() => { refresh().catch(() => setError("Video walls could not be loaded.")); }, [apiKey, configuration, venueId]);

  const toggle = (screenId: string) =>
    setSelected(current => current.includes(screenId) ? current.filter(id => id !== screenId) : [...current, screenId]);

  const save = async (event: FormEvent) => {
    event.preventDefault(); setBusy(true); setError(undefined); setNotice(undefined);
    try {
      await saveVideoWall(configuration, apiKey, venueId, { name, layout, screenIds: selected });
      setSelected([]); setEditingName(undefined); setNotice(`${name} saved and queued for all wall screens.`); await refresh();
    } catch { setError("The video wall could not be saved."); }
    finally { setBusy(false); }
  };

  const remove = async (wallName: string) => {
    if (!await review({ title: `Remove ${wallName}?`, consequence: "The wall grouping will be deleted and its screens will return to independent layouts. Screen records and content remain available.", confirmLabel: "Remove video wall", tone: "caution" })) return;
    setBusy(true); setError(undefined); setNotice(undefined);
    try { await removeVideoWall(configuration, apiKey, venueId, wallName); setNotice(`${wallName} removed.`); await refresh(); }
    catch { setError("The video wall could not be removed."); }
    finally { setBusy(false); }
  };

  const edit = (wallName: string) => {
    const group = snapshot?.groups.find(candidate => candidate.name === wallName);
    if (!group) return;
    setEditingName(group.name);
    setName(group.name);
    setLayout(group.layout);
    setSelected([...group.screens].sort((left, right) => left.position - right.position).map(screen => screen.id));
    setError(undefined);
  };

  const cancelEdit = () => {
    setEditingName(undefined);
    setName("Main wall");
    setLayout("2x1");
    setSelected([]);
  };

  return <section className="video-wall-builder">
    {reviewDialog}
    <div className="video-wall-heading"><div><p>Pro layout</p><h4>Video wall builder</h4></div><span>2×1 · 3×1 · 2×2</span></div>
    {showUpgradePrompt && snapshot && !snapshot.enabled ? <aside className="tier-prompt" role="status"><div><strong>Video Wall is a higher-tier feature</strong><p>The builder stays visible so you can preview the workflow. Upgrade or add a venue override to configure walls.</p></div></aside> : null}
    {error ? <p className="state error" role="alert">{error}</p> : null}
    {notice ? <p className="screen-notice" role="status">{notice}</p> : null}
    <form onSubmit={save}>
      <label>Wall name<input maxLength={100} required value={name} onChange={event => setName(event.target.value)} disabled={!snapshot?.enabled || Boolean(editingName)} /></label>
      <label>Configuration<select value={layout} onChange={event => { setLayout(event.target.value); setSelected([]); }} disabled={!snapshot?.enabled}><option value="2x1">2 × 1</option><option value="3x1">3 × 1</option><option value="2x2">2 × 2</option></select></label>
      <fieldset disabled={!snapshot?.enabled}><legend>Assign screens in position order ({selected.length}/{layoutSizes[layout]})</legend>{screens.map(screen =>
        <label key={screen.id}><input type="checkbox" checked={selected.includes(screen.id)} disabled={!selected.includes(screen.id) && selected.length >= layoutSizes[layout]} onChange={() => toggle(screen.id)} /> <span>{screen.name}<small>{screen.location ?? "No location"}</small></span></label>)}</fieldset>
      <button disabled={busy || !snapshot?.enabled || selected.length !== layoutSizes[layout]}>{editingName ? "Save wall changes" : "Save wall"}</button>
      {editingName ? <button type="button" disabled={busy} onClick={cancelEdit}>Cancel edit</button> : null}
    </form>
    {snapshot?.groups.length ? <div className="wall-groups">{snapshot.groups.map(group =>
      <article key={group.name}><div><strong>{group.name}</strong><span>{group.layout}</span></div><ol>{group.screens.map(screen => <li key={screen.id}><span>{screen.position}</span>{screen.name}</li>)}</ol><button type="button" disabled={busy || !snapshot.enabled} onClick={() => edit(group.name)}>Edit wall</button><button type="button" disabled={busy || !snapshot.enabled} onClick={() => remove(group.name)}>Remove wall</button></article>)}</div> : <p>{snapshot ? "No video walls configured." : "Loading video walls…"}</p>}
  </section>;
}
