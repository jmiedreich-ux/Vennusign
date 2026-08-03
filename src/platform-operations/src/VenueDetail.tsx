import { useEffect, useMemo, useState, type FormEvent } from "react";
import { loadFeatureMatrix, loadVenueSupportDetail, removeVenueFeatureOverride, saveVenueFeatureOverride, switchVenueTier, type FeatureMatrixSnapshot, type VenueSupportDetail } from "./api";
import type { PlatformOperationsConfiguration } from "./config";
import { buildTierSwitchImpact } from "./operatorSafety.mjs";

type Props = { configuration: PlatformOperationsConfiguration; apiKey: string; venueId: string; onBack: () => void };
type PendingAction =
  | { kind: "tier"; targetTierId: string }
  | { kind: "override"; featureId: string; enabled: boolean; reason: string; expiresAt?: string }
  | { kind: "remove"; featureId: string };

export default function VenueDetail({ configuration, apiKey, venueId, onBack }: Props) {
  const [detail, setDetail] = useState<VenueSupportDetail>();
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string>();
  const [actionError, setActionError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const [matrix, setMatrix] = useState<FeatureMatrixSnapshot>();
  const [featureId, setFeatureId] = useState("");
  const [enabled, setEnabled] = useState(true);
  const [reason, setReason] = useState("");
  const [expiresAt, setExpiresAt] = useState("");
  const [saving, setSaving] = useState(false);
  const [version, setVersion] = useState(0);
  const [targetTierId, setTargetTierId] = useState("");
  const [pending, setPending] = useState<PendingAction>();
  const [lastUpdated, setLastUpdated] = useState<Date>();

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true); setLoadError(undefined);
    Promise.all([loadVenueSupportDetail(configuration, apiKey, venueId, controller.signal), loadFeatureMatrix(configuration, apiKey)])
      .then(([value, featureMatrix]) => {
        if (!value) { setLoadError("Venue not found."); return; }
        setDetail(value); setMatrix(featureMatrix); setFeatureId(current => current || featureMatrix.features[0]?.id || ""); setTargetTierId(value.tier?.id ?? ""); setLastUpdated(new Date());
      })
      .catch(error => { if (!(error instanceof DOMException && error.name === "AbortError")) setLoadError("Venue detail could not be loaded."); })
      .finally(() => setLoading(false));
    return () => controller.abort();
  }, [apiKey, configuration, venueId, version]);

  const tierImpact = useMemo(() => pending?.kind === "tier" && detail && matrix ? buildTierSwitchImpact(detail, matrix, pending.targetTierId) : undefined, [detail, matrix, pending]);
  const selectedFeature = matrix?.features.find(feature => feature.id === (pending?.kind === "override" || pending?.kind === "remove" ? pending.featureId : featureId));
  const requestOverride = (event: FormEvent) => {
    event.preventDefault(); setActionError(undefined); setNotice(undefined);
    if (!reason.trim()) { setActionError("Provide a support reason before reviewing the override."); return; }
    setPending({ kind: "override", featureId, enabled, reason: reason.trim(), expiresAt: expiresAt ? new Date(expiresAt).toISOString() : undefined });
  };
  const requestTier = (event: FormEvent) => { event.preventDefault(); setActionError(undefined); setNotice(undefined); setPending({ kind: "tier", targetTierId }); };
  const confirmPending = async () => {
    if (!pending) return;
    setSaving(true); setActionError(undefined); setNotice(undefined);
    try {
      if (pending.kind === "tier") { await switchVenueTier(configuration, apiKey, venueId, pending.targetTierId); setNotice("Tier and Stripe subscription updated. The support view has been refreshed."); }
      else if (pending.kind === "override") { await saveVenueFeatureOverride(configuration, apiKey, venueId, pending.featureId, { enabled: pending.enabled, reason: pending.reason, expiresAt: pending.expiresAt }); setReason(""); setExpiresAt(""); setNotice("Feature override saved and recorded in the operational event feed."); }
      else { await removeVenueFeatureOverride(configuration, apiKey, venueId, pending.featureId); setNotice("Feature override removed and recorded in the operational event feed."); }
      setPending(undefined); setVersion(value => value + 1);
    } catch { setActionError(pending.kind === "tier" ? "The venue tier could not be switched. No success is assumed; review Stripe and subscription state before retrying." : "The feature override action could not be completed. Review the current state before retrying."); }
    finally { setSaving(false); }
  };

  if (loading && !detail) return <p className="state">Loading venue detail…</p>;
  if (loadError || !detail) return <section><button className="back" onClick={onBack}>← Back to venues</button><div className="state error" role="alert"><p>{loadError}</p><button type="button" onClick={() => setVersion(value => value + 1)}>Retry venue detail</button></div></section>;

  const features = Object.values(detail.features).sort((a, b) => a.key.localeCompare(b.key));
  return <section className="venue-detail">
    <div className="detail-actions"><button className="back" onClick={onBack}>← Back to venues</button><button type="button" disabled={loading} onClick={() => setVersion(value => value + 1)}>{loading ? "Refreshing…" : "Refresh support detail"}</button></div>
    <div className="detail-heading"><div><p>{detail.venue.type}</p><h2>{detail.venue.name}</h2><small role="status">{lastUpdated ? `Updated ${lastUpdated.toLocaleTimeString()}` : "Not yet refreshed"}</small></div><span className="health">{detail.subscription?.status ?? "unsubscribed"}</span></div>
    {actionError ? <p className="matrix-message error" role="alert">{actionError}</p> : null}{notice ? <p className="matrix-message" role="status">{notice}</p> : null}
    {pending ? <section className="impact-preview" aria-labelledby="support-impact-title"><p>Review required</p><h3 id="support-impact-title">Confirm support impact</h3>
      {pending.kind === "tier" && tierImpact ? <><p><strong>{tierImpact.currentTierName}</strong> → <strong>{tierImpact.targetTierName}</strong>. This updates the linked Stripe subscription and effective entitlements.</p><ul><li>{tierImpact.screenCount} screens; target limit {tierImpact.targetScreenLimit === -1 ? "unlimited" : tierImpact.targetScreenLimit}{tierImpact.screenLimitExceeded ? " — limit would be exceeded" : ""}</li><li>{tierImpact.enabled.length} features enabled; {tierImpact.disabled.length} disabled</li>{tierImpact.disabled.length ? <li>Disabled: {tierImpact.disabled.join(", ")}</li> : null}</ul></> : null}
      {pending.kind === "override" ? <p>{pending.enabled ? "Unlock" : "Block"} <strong>{selectedFeature?.label ?? pending.featureId}</strong> for {detail.venue.name}. Reason: {pending.reason}{pending.expiresAt ? `; expires ${new Date(pending.expiresAt).toLocaleString()}` : "; no expiry"}.</p> : null}
      {pending.kind === "remove" ? <p>Remove the active override for <strong>{selectedFeature?.label ?? pending.featureId}</strong>. The venue will immediately return to tier-based entitlement resolution.</p> : null}
      <div><button type="button" onClick={() => setPending(undefined)} disabled={saving}>Cancel</button><button className="danger" type="button" onClick={confirmPending} disabled={saving || !!tierImpact?.screenLimitExceeded}>{saving ? "Applying…" : "Confirm change"}</button></div>{tierImpact?.screenLimitExceeded ? <p className="error" role="alert">Choose a tier that supports the venue’s current screen count.</p> : null}
    </section> : null}
    <div className="detail-grid">
      <article><h3>Profile</h3><dl><dt>Timezone</dt><dd>{detail.venue.timezone}</dd><dt>Languages</dt><dd>{[detail.venue.primaryLanguage, detail.venue.secondaryLanguage].filter(Boolean).join(", ")}</dd></dl></article>
      <article><h3>Subscription</h3><dl><dt>Tier</dt><dd>{detail.tier?.name ?? "None"}</dd><dt>Screen limit</dt><dd>{detail.tier?.maxScreens ?? "—"}</dd><dt>Period end</dt><dd>{detail.subscription?.currentPeriodEnd ? new Date(detail.subscription.currentPeriodEnd).toLocaleDateString() : "—"}</dd></dl>{detail.subscription ? <form className="tier-switch" onSubmit={requestTier}><label>Switch tier<select value={targetTierId} onChange={event => setTargetTierId(event.target.value)}>{matrix?.tiers.filter(tier => tier.isActive).map(tier => <option key={tier.id} value={tier.id}>{tier.name}</option>)}</select></label><button disabled={saving || !targetTierId || targetTierId === detail.tier?.id} type="submit">Review Stripe tier change</button></form> : null}</article>
    </div>
    <article><div className="panel-heading"><div><p>Fleet support</p><h3>Screens ({detail.screens.length})</h3></div></div>{detail.screens.length ? <ul className="support-list screen-support-list">{detail.screens.map(screen => <li key={screen.id}><div><strong>{screen.name}</strong><small>{screen.location ?? "No location"} · {screen.platform ?? "Unknown platform"} {screen.appVersion ?? "Unknown version"}</small></div><span className={`health ${screen.status.toLowerCase()}`}>{screen.status} · {screen.lastSeen ? new Date(screen.lastSeen).toLocaleString() : "never seen"}</span></li>)}</ul> : <p className="empty">No screens are assigned. Use the Back Office pairing flow or provisioning support path.</p>}</article>
    <article className="back-office-handoff"><div><p>Customer workspace</p><h3>Menu and Quick Update</h3><span>Day-to-day menu work now runs in the venue-scoped Back Office CMS.</span></div><a href={`${configuration.backOfficeBaseUrl}#/menu`}>Open Back Office</a></article>
    <article className="back-office-handoff"><div><p>Customer workspace</p><h3>Screens, themes, schedules, and tap lists</h3><span>Venue operations run in the protected customer workspace while support context remains here.</span></div><a href={`${configuration.backOfficeBaseUrl}#/screens`}>Open venue operations</a></article>
    <article><h3>Effective features</h3><ul className="support-list">{features.map(feature => <li key={feature.key}><strong>{feature.key}</strong><span>{feature.enabled ? "Enabled" : "Disabled"} · {feature.source}{feature.limitValue ? ` · limit ${feature.limitValue}` : ""}</span></li>)}</ul></article>
    <article className="override-panel"><div><h3>Active overrides ({detail.activeOverrides.length})</h3>{detail.activeOverrides.length ? <ul className="support-list">{detail.activeOverrides.map(item => <li key={item.featureId}><strong>{matrix?.features.find(feature => feature.id === item.featureId)?.label ?? item.featureId}</strong><span>{item.enabled ? "Unlock" : "Block"} · {item.reason}{item.expiresAt ? ` · expires ${new Date(item.expiresAt).toLocaleString()}` : ""}<button type="button" disabled={saving} onClick={() => setPending({ kind: "remove", featureId: item.featureId })}>Review removal</button></span></li>)}</ul> : <p>No active overrides.</p>}</div>
      <form onSubmit={requestOverride}><h3>Add or replace override</h3><label>Feature<select required value={featureId} onChange={event => setFeatureId(event.target.value)}>{matrix?.features.map(feature => <option key={feature.id} value={feature.id}>{feature.label}</option>)}</select></label><div className="override-choice"><label><input type="radio" checked={enabled} onChange={() => setEnabled(true)} /> Unlock</label><label><input type="radio" checked={!enabled} onChange={() => setEnabled(false)} /> Block</label></div><label>Reason<textarea required maxLength={500} value={reason} onChange={event => setReason(event.target.value)} /></label><label>Expires (optional)<input type="datetime-local" value={expiresAt} onChange={event => setExpiresAt(event.target.value)} /></label><button disabled={saving || !featureId} type="submit">Review override</button></form>
    </article>
  </section>;
}
