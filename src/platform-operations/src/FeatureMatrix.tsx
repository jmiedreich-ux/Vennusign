import { useEffect, useMemo, useState } from "react";
import { loadFeatureMatrix, saveFeatureMatrix, type FeatureMatrixSnapshot } from "./api";
import type { PlatformOperationsConfiguration } from "./config";
import { summarizeFeatureMatrixImpact } from "./operatorSafety.mjs";

type Props = { configuration: PlatformOperationsConfiguration; apiKey: string };
type CellState = Record<string, boolean>;
const cellKey = (tierId: string, featureId: string) => `${tierId}:${featureId}`;

export default function FeatureMatrix({ configuration, apiKey }: Props) {
  const [snapshot, setSnapshot] = useState<FeatureMatrixSnapshot>();
  const [saved, setSaved] = useState<CellState>({});
  const [draft, setDraft] = useState<CellState>({});
  const [error, setError] = useState<string>();
  const [message, setMessage] = useState<string>();
  const [busy, setBusy] = useState(true);
  const [reviewing, setReviewing] = useState(false);
  const initialize = (value: FeatureMatrixSnapshot) => { const enabled = Object.fromEntries(value.enabledFeatures.map(item => [cellKey(item.tierId, item.featureId), true])); setSnapshot(value); setSaved(enabled); setDraft(enabled); };
  const refresh = () => loadFeatureMatrix(configuration, apiKey).then(initialize);
  useEffect(() => { refresh().catch(() => setError("The feature matrix could not be loaded.")).finally(() => setBusy(false)); }, [apiKey, configuration]);
  const changes = useMemo(() => snapshot ? snapshot.tiers.flatMap(tier => snapshot.features.map(feature => { const key = cellKey(tier.id, feature.id); return draft[key] !== !!saved[key] ? { tierId: tier.id, featureId: feature.id, enabled: !!draft[key] } : undefined; })).filter((change): change is { tierId: string; featureId: string; enabled: boolean } => !!change) : [], [draft, saved, snapshot]);
  const grouped = useMemo(() => snapshot?.features.reduce<Record<string, typeof snapshot.features>>((groups, feature) => { (groups[feature.category] ??= []).push(feature); return groups; }, {}) ?? {}, [snapshot]);
  const impact = snapshot ? summarizeFeatureMatrixImpact(changes, snapshot) : undefined;
  const setAll = (enabled: boolean) => { if (snapshot) setDraft(Object.fromEntries(snapshot.tiers.flatMap(tier => snapshot.features.map(feature => [cellKey(tier.id, feature.id), enabled])))); };
  const save = async () => { if (!changes.length) return; setBusy(true); setError(undefined); setMessage(undefined); try { const result = await saveFeatureMatrix(configuration, apiKey, changes); await refresh(); setReviewing(false); setMessage(`${result.changedCount} feature ${result.changedCount === 1 ? "assignment" : "assignments"} updated and audited.`); } catch { setError("The pending feature changes could not be saved. Reload before retrying if another operator changed the matrix."); } finally { setBusy(false); } };
  if (busy && !snapshot) return <p className="state">Loading feature matrix…</p>;
  if (!snapshot) return <p className="state error" role="alert">{error ?? "The feature matrix is unavailable."}</p>;
  return <section className="feature-matrix">
    <div className="matrix-toolbar"><div><p>Entitlement editor</p><h2>Feature access by tier</h2><span role="status">{changes.length ? `${changes.length} unsaved changes` : "All changes saved"}</span></div><div><button type="button" onClick={() => setAll(true)}>Enable all</button><button type="button" onClick={() => setAll(false)}>Clear all</button><button type="button" disabled={!changes.length || busy} onClick={() => { setDraft(saved); setReviewing(false); }}>Discard</button><button className="primary" type="button" disabled={!changes.length || busy} onClick={() => setReviewing(true)}>Review changes</button></div></div>
    {error ? <p className="matrix-message error" role="alert">{error}</p> : null}{message ? <p className="matrix-message" role="status">{message}</p> : null}
    {reviewing && impact ? <section className="impact-preview" aria-labelledby="matrix-impact-title"><p>Bulk entitlement review</p><h3 id="matrix-impact-title">Confirm {impact.changedCount} changes across {impact.tierCount} tiers</h3><p>{impact.enabledCount} assignments will be enabled and {impact.disabledCount} disabled. Effective entitlements are recalculated for subscriptions on: {impact.tierNames.join(", ")}.</p><div><button type="button" onClick={() => setReviewing(false)}>Continue editing</button><button className="danger" type="button" disabled={busy} onClick={save}>{busy ? "Saving…" : "Confirm entitlement changes"}</button></div></section> : null}
    <div className="matrix-scroll"><table className="matrix-table"><caption className="sr-only">Feature access assignments by subscription tier</caption><thead><tr><th>Feature</th>{snapshot.tiers.map(tier => <th key={tier.id}><strong>{tier.name}</strong><small>{tier.isActive ? "Active" : "Archived"}</small></th>)}</tr></thead><tbody>{Object.entries(grouped).map(([category, features]) => [<tr className="category-row" key={`${category}-heading`}><th colSpan={snapshot.tiers.length + 1}>{category.replaceAll("_", " ")}</th></tr>, ...features.map(feature => <tr key={feature.id}><th><strong>{feature.label}</strong><small>{feature.key}</small></th>{snapshot.tiers.map(tier => { const key = cellKey(tier.id, feature.id); const dirty = !!draft[key] !== !!saved[key]; return <td className={dirty ? "dirty" : ""} key={tier.id}><label aria-label={`${feature.label} for ${tier.name}`}><input type="checkbox" checked={!!draft[key]} onChange={event => { setReviewing(false); setDraft({ ...draft, [key]: event.target.checked }); }} /><span /></label></td>; })}</tr>)])}</tbody></table></div>
    <aside className="audit-panel"><h3>Recent changes</h3>{snapshot.recentAudit.length ? <ul>{snapshot.recentAudit.slice(0, 10).map(item => { const tier = snapshot.tiers.find(value => value.id === item.tierId)?.name ?? "Unknown tier"; const feature = snapshot.features.find(value => value.id === item.featureId)?.label ?? "Unknown feature"; return <li key={item.id}><strong>{feature}</strong><span>{tier} · {item.newEnabled ? "enabled" : "disabled"} by {item.adminId}</span><time>{new Date(item.changedUtc).toLocaleString()}</time></li>; })}</ul> : <p>No feature changes have been recorded.</p>}</aside>
  </section>;
}
