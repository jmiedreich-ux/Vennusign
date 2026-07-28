import { useEffect, useState } from "react";
import { loadVenueSupportDetail, type VenueSupportDetail } from "./api";
import type { AdminConfiguration } from "./config";

type Props = { configuration: AdminConfiguration; apiKey: string; venueId: string; onBack: () => void };

export default function VenueDetail({ configuration, apiKey, venueId, onBack }: Props) {
  const [detail, setDetail] = useState<VenueSupportDetail>();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string>();

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    setError(undefined);
    loadVenueSupportDetail(configuration, apiKey, venueId, controller.signal)
      .then(value => value ? setDetail(value) : setError("Venue not found."))
      .catch(reason => {
        if (!(reason instanceof DOMException && reason.name === "AbortError")) setError("Venue detail could not be loaded.");
      })
      .finally(() => setLoading(false));
    return () => controller.abort();
  }, [apiKey, configuration, venueId]);

  if (loading) return <p className="state">Loading venue detail…</p>;
  if (error || !detail) return <section><button className="back" onClick={onBack}>← Back to venues</button><p className="state error">{error}</p></section>;

  const features = Object.values(detail.features).sort((a, b) => a.key.localeCompare(b.key));
  return <section className="venue-detail">
    <button className="back" onClick={onBack}>← Back to venues</button>
    <div className="detail-heading"><div><p>{detail.venue.type}</p><h2>{detail.venue.name}</h2></div><span className="health">{detail.subscription?.status ?? "unsubscribed"}</span></div>
    <div className="detail-grid">
      <article><h3>Profile</h3><dl><dt>Timezone</dt><dd>{detail.venue.timezone}</dd><dt>Languages</dt><dd>{[detail.venue.primaryLanguage, detail.venue.secondaryLanguage].filter(Boolean).join(", ")}</dd></dl></article>
      <article><h3>Subscription</h3><dl><dt>Tier</dt><dd>{detail.tier?.name ?? "None"}</dd><dt>Screen limit</dt><dd>{detail.tier?.maxScreens ?? "—"}</dd><dt>Period end</dt><dd>{detail.subscription?.currentPeriodEnd ? new Date(detail.subscription.currentPeriodEnd).toLocaleDateString() : "—"}</dd></dl></article>
    </div>
    <article><h3>Screens ({detail.screens.length})</h3>{detail.screens.length ? <ul className="support-list">{detail.screens.map(screen => <li key={screen.id}><strong>{screen.name}</strong><span>{screen.location ?? "No location"} · {screen.status} · {screen.lastSeen ? new Date(screen.lastSeen).toLocaleString() : "Never seen"}</span></li>)}</ul> : <p>No screens assigned.</p>}</article>
    <article><h3>Effective features</h3><ul className="support-list">{features.map(feature => <li key={feature.key}><strong>{feature.key}</strong><span>{feature.enabled ? "Enabled" : "Disabled"} · {feature.source}{feature.limitValue ? ` · limit ${feature.limitValue}` : ""}</span></li>)}</ul></article>
    <article><h3>Active overrides ({detail.activeOverrides.length})</h3>{detail.activeOverrides.length ? <ul className="support-list">{detail.activeOverrides.map(item => <li key={item.featureId}><strong>{item.enabled ? "Unlock" : "Block"}</strong><span>{item.reason}{item.expiresAt ? ` · expires ${new Date(item.expiresAt).toLocaleString()}` : ""}</span></li>)}</ul> : <p>No active overrides.</p>}</article>
  </section>;
}
