import { useEffect, useState } from "react";
import { loadOperationalDashboard, type OperationalDashboard as Dashboard } from "./api";
import type { AdminConfiguration } from "./config";

type Props = { configuration: AdminConfiguration; apiKey: string };

export default function OperationalDashboard({ configuration, apiKey }: Props) {
  const [dashboard, setDashboard] = useState<Dashboard>();
  const [error, setError] = useState<string>();

  useEffect(() => {
    loadOperationalDashboard(configuration, apiKey)
      .then(setDashboard)
      .catch(() => setError("The operational dashboard could not be loaded."));
  }, [apiKey, configuration]);

  if (error) return <p className="state error">{error}</p>;
  if (!dashboard) return <p className="state">Loading operational dashboard…</p>;

  const metrics = [
    ["Total venues", dashboard.totalVenues],
    ["Active", dashboard.activeVenues],
    ["Trialing", dashboard.trialingVenues],
    ["Canceled · 30 days", dashboard.canceledLast30Days],
    ["Screens online", dashboard.onlineScreens],
    ["Screens offline", dashboard.offlineScreens]
  ] as const;

  return <section className="operational-dashboard">
    <div className="metric-grid">{metrics.map(([label, value]) => <article key={label}><span>{label}</span><strong>{value}</strong></article>)}</div>
    <article className="screen-map"><div><p>Fleet health</p><h2>Screen health map</h2><span>{dashboard.onlineScreens} online · {dashboard.offlineScreens} offline</span></div>
      {dashboard.screens.length ? <div className="screen-dots">{dashboard.screens.map(screen => <div className="screen-health-item" key={screen.screenId} title={`${screen.venueName} · ${screen.screenName} · ${screen.status} · ${screen.lastSeen ? new Date(screen.lastSeen).toLocaleString() : "never seen"}`}><span className={screen.status} /><div><strong>{screen.screenName}</strong><small>{screen.venueName}{screen.location ? ` · ${screen.location}` : ""}</small></div></div>)}</div> : <p className="empty">No screens have been registered.</p>}
    </article>
  </section>;
}
