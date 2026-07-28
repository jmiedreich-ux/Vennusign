import { useEffect, useMemo, useState } from "react";
import { loadFeatureMatrix, saveFeatureMatrix, type FeatureMatrixSnapshot } from "./api";
import type { AdminConfiguration } from "./config";

type Props = { configuration: AdminConfiguration; apiKey: string };
type CellState = Record<string, boolean>;
const cellKey = (tierId: string, featureId: string) => `${tierId}:${featureId}`;

export default function FeatureMatrix({ configuration, apiKey }: Props) {
  const [snapshot, setSnapshot] = useState<FeatureMatrixSnapshot>();
  const [saved, setSaved] = useState<CellState>({});
  const [draft, setDraft] = useState<CellState>({});
  const [error, setError] = useState<string>();
  const [message, setMessage] = useState<string>();
  const [busy, setBusy] = useState(true);

  const initialize = (value: FeatureMatrixSnapshot) => {
    const enabled = Object.fromEntries(value.enabledFeatures.map(item => [cellKey(item.tierId, item.featureId), true]));
    setSnapshot(value); setSaved(enabled); setDraft(enabled);
  };
  const refresh = () => loadFeatureMatrix(configuration, apiKey).then(initialize);
  useEffect(() => { refresh().catch(() => setError("The feature matrix could not be loaded.")).finally(() => setBusy(false)); }, [apiKey, configuration]);

  const changes = useMemo(() => {
    if (!snapshot) return [];
    return snapshot.tiers.flatMap(tier => snapshot.features.map(feature => {
      const key = cellKey(tier.id, feature.id);
      return draft[key] !== !!saved[key] ? { tierId: tier.id, featureId: feature.id, enabled: !!draft[key] } : undefined;
    })).filter((change): change is { tierId: string; featureId: string; enabled: boolean } => !!change);
  }, [draft, saved, snapshot]);

  const grouped = useMemo(() => snapshot?.features.reduce<Record<string, typeof snapshot.features>>((groups, feature) => {
    (groups[feature.category] ??= []).push(feature); return groups;
  }, {}) ?? {}, [snapshot]);

  const setAll = (enabled: boolean) => {
    if (!snapshot) return;
    setDraft(Object.fromEntries(snapshot.tiers.flatMap(tier => snapshot.features.map(feature => [cellKey(tier.id, feature.id), enabled]))));
  };
  const save = async () => {
    if (!changes.length) return;
    setBusy(true); setError(undefined); setMessage(undefined);
    try {
      const result = await saveFeatureMatrix(configuration, apiKey, changes);
      await refresh();
      setMessage(`${result.changedCount} feature ${result.changedCount === 1 ? "assignment" : "assignments"} updated.`);
    } catch { setError("The pending feature changes could not be saved."); }
    finally { setBusy(false); }
  };

  if (busy && !snapshot) return <p className="state">Loading feature matrix…</p>;
  if (!snapshot) return <p className="state error">{error ?? "The feature matrix is unavailable."}</p>;

  return <section className="feature-matrix">
    <div className="matrix-toolbar">
      <div><p>Entitlement editor</p><h2>Feature access by tier</h2><span>{changes.length ? `${changes.length} unsaved changes` : "All changes saved"}</span></div>
      <div><button type="button" onClick={() => setAll(true)}>Enable all</button><button type="button" onClick={() => setAll(false)}>Clear all</button><button type="button" disabled={!changes.length || busy} onClick={() => setDraft(saved)}>Discard</button><button className="primary" type="button" disabled={!changes.length || busy} onClick={save}>Save changes</button></div>
    </div>
    {error && <p className="matrix-message error">{error}</p>}{message && <p className="matrix-message">{message}</p>}
    <div className="matrix-scroll"><table className="matrix-table">
      <thead><tr><th>Feature</th>{snapshot.tiers.map(tier => <th key={tier.id}><strong>{tier.name}</strong><small>{tier.isActive ? "Active" : "Archived"}</small></th>)}</tr></thead>
      <tbody>{Object.entries(grouped).map(([category, features]) => [
        <tr className="category-row" key={`${category}-heading`}><th colSpan={snapshot.tiers.length + 1}>{category.replaceAll("_", " ")}</th></tr>,
        ...features.map(feature => <tr key={feature.id}><th><strong>{feature.label}</strong><small>{feature.key}</small></th>{snapshot.tiers.map(tier => {
          const key = cellKey(tier.id, feature.id); const dirty = !!draft[key] !== !!saved[key];
          return <td className={dirty ? "dirty" : ""} key={tier.id}><label aria-label={`${feature.label} for ${tier.name}`}><input type="checkbox" checked={!!draft[key]} onChange={event => setDraft({ ...draft, [key]: event.target.checked })} /><span /></label></td>;
        })}</tr>)
      ])}</tbody>
    </table></div>
    <aside className="audit-panel"><h3>Recent changes</h3>{snapshot.recentAudit.length ? <ul>{snapshot.recentAudit.slice(0, 10).map(item => {
      const tier = snapshot.tiers.find(value => value.id === item.tierId)?.name ?? "Unknown tier";
      const feature = snapshot.features.find(value => value.id === item.featureId)?.label ?? "Unknown feature";
      return <li key={item.id}><strong>{feature}</strong><span>{tier} · {item.newEnabled ? "enabled" : "disabled"} by {item.adminId}</span><time>{new Date(item.changedUtc).toLocaleString()}</time></li>;
    })}</ul> : <p>No feature changes have been recorded.</p>}</aside>
  </section>;
}
