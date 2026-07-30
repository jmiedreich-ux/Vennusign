import { useEffect, useState, type FormEvent } from "react";
import { loadFeatureMatrix, loadVenueSupportDetail, removeVenueFeatureOverride, saveVenueFeatureOverride, switchVenueTier, type FeatureMatrixSnapshot, type VenueSupportDetail } from "./api";
import type { AdminConfiguration } from "./config";
import MenuSectionsEditor from "./MenuSectionsEditor";
import ScreenManagement from "./ScreenManagement";
import ThemeBuilder from "./ThemeBuilder";

type Props = { configuration: AdminConfiguration; apiKey: string; venueId: string; onBack: () => void };

export default function VenueDetail({ configuration, apiKey, venueId, onBack }: Props) {
  const [detail, setDetail] = useState<VenueSupportDetail>();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>();
  const [matrix, setMatrix] = useState<FeatureMatrixSnapshot>();
  const [featureId, setFeatureId] = useState("");
  const [enabled, setEnabled] = useState(true);
  const [reason, setReason] = useState("");
  const [expiresAt, setExpiresAt] = useState("");
  const [saving, setSaving] = useState(false);
  const [version, setVersion] = useState(0);
  const [targetTierId, setTargetTierId] = useState("");

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    setError(undefined);
    Promise.all([
      loadVenueSupportDetail(configuration, apiKey, venueId, controller.signal),
      loadFeatureMatrix(configuration, apiKey)
    ])
      .then(([value, featureMatrix]) => {
        if (value) { setDetail(value); setMatrix(featureMatrix); setFeatureId(current => current || featureMatrix.features[0]?.id || ""); setTargetTierId(value.tier?.id ?? ""); }
        else setError("Venue not found.");
      })
      .catch(reason => {
        if (!(reason instanceof DOMException && reason.name === "AbortError")) setError("Venue detail could not be loaded.");
      })
      .finally(() => setLoading(false));
    return () => controller.abort();
  }, [apiKey, configuration, venueId, version]);

  const saveOverride = async (event: FormEvent) => {
    event.preventDefault(); setSaving(true); setError(undefined);
    try {
      await saveVenueFeatureOverride(configuration, apiKey, venueId, featureId, {
        enabled, reason, expiresAt: expiresAt ? new Date(expiresAt).toISOString() : undefined
      });
      setReason(""); setExpiresAt(""); setVersion(value => value + 1);
    } catch { setError("The feature override could not be saved."); }
    finally { setSaving(false); }
  };
  const removeOverride = async (id: string) => {
    setSaving(true); setError(undefined);
    try { await removeVenueFeatureOverride(configuration, apiKey, venueId, id); setVersion(value => value + 1); }
    catch { setError("The feature override could not be removed."); }
    finally { setSaving(false); }
  };
  const saveTier = async (event: FormEvent) => {
    event.preventDefault(); setSaving(true); setError(undefined);
    try { await switchVenueTier(configuration, apiKey, venueId, targetTierId); setVersion(value => value + 1); }
    catch { setError("The venue tier could not be switched."); }
    finally { setSaving(false); }
  };

  if (loading) return <p className="state">Loading venue detail…</p>;
  if (error || !detail) return <section><button className="back" onClick={onBack}>← Back to venues</button><p className="state error">{error}</p></section>;

  const features = Object.values(detail.features).sort((a, b) => a.key.localeCompare(b.key));
  return <section className="venue-detail">
    <button className="back" onClick={onBack}>← Back to venues</button>
    <div className="detail-heading"><div><p>{detail.venue.type}</p><h2>{detail.venue.name}</h2></div><span className="health">{detail.subscription?.status ?? "unsubscribed"}</span></div>
    <div className="detail-grid">
      <article><h3>Profile</h3><dl><dt>Timezone</dt><dd>{detail.venue.timezone}</dd><dt>Languages</dt><dd>{[detail.venue.primaryLanguage, detail.venue.secondaryLanguage].filter(Boolean).join(", ")}</dd></dl></article>
      <article><h3>Subscription</h3><dl><dt>Tier</dt><dd>{detail.tier?.name ?? "None"}</dd><dt>Screen limit</dt><dd>{detail.tier?.maxScreens ?? "—"}</dd><dt>Period end</dt><dd>{detail.subscription?.currentPeriodEnd ? new Date(detail.subscription.currentPeriodEnd).toLocaleDateString() : "—"}</dd></dl>{detail.subscription ? <form className="tier-switch" onSubmit={saveTier}><label>Switch tier<select value={targetTierId} onChange={event => setTargetTierId(event.target.value)}>{matrix?.tiers.filter(tier => tier.isActive).map(tier => <option key={tier.id} value={tier.id}>{tier.name}</option>)}</select></label><button disabled={saving || !targetTierId || targetTierId === detail.tier?.id} type="submit">Update Stripe subscription</button></form> : null}</article>
    </div>
    <ScreenManagement
      configuration={configuration}
      apiKey={apiKey}
      venueId={venueId}
      allLayoutsEnabled={detail.features.all_layouts?.enabled ?? false}
    />
    <ThemeBuilder
      configuration={configuration}
      apiKey={apiKey}
      venueId={venueId}
      advancedEnabled={detail.features.all_layouts?.enabled ?? false}
    />
    <MenuSectionsEditor configuration={configuration} apiKey={apiKey} venueId={venueId} />
    <article><h3>Effective features</h3><ul className="support-list">{features.map(feature => <li key={feature.key}><strong>{feature.key}</strong><span>{feature.enabled ? "Enabled" : "Disabled"} · {feature.source}{feature.limitValue ? ` · limit ${feature.limitValue}` : ""}</span></li>)}</ul></article>
    <article className="override-panel"><div><h3>Active overrides ({detail.activeOverrides.length})</h3>{detail.activeOverrides.length ? <ul className="support-list">{detail.activeOverrides.map(item => <li key={item.featureId}><strong>{matrix?.features.find(feature => feature.id === item.featureId)?.label ?? item.featureId}</strong><span>{item.enabled ? "Unlock" : "Block"} · {item.reason}{item.expiresAt ? ` · expires ${new Date(item.expiresAt).toLocaleString()}` : ""}<button disabled={saving} onClick={() => removeOverride(item.featureId)}>Remove</button></span></li>)}</ul> : <p>No active overrides.</p>}</div>
      <form onSubmit={saveOverride}><h3>Add or replace override</h3><label>Feature<select required value={featureId} onChange={event => setFeatureId(event.target.value)}>{matrix?.features.map(feature => <option key={feature.id} value={feature.id}>{feature.label}</option>)}</select></label><div className="override-choice"><label><input type="radio" checked={enabled} onChange={() => setEnabled(true)} /> Unlock</label><label><input type="radio" checked={!enabled} onChange={() => setEnabled(false)} /> Block</label></div><label>Reason<textarea required maxLength={500} value={reason} onChange={event => setReason(event.target.value)} /></label><label>Expires (optional)<input type="datetime-local" value={expiresAt} onChange={event => setExpiresAt(event.target.value)} /></label><button disabled={saving || !featureId} type="submit">Save override</button></form>
    </article>
  </section>;
}
