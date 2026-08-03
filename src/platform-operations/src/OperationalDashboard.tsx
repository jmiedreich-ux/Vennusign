import { useEffect, useState } from "react";
import {
  loadOperationalDashboard,
  loadOperationalEvents,
  loadRevenueSnapshot,
  loadRevenueTrend,
  type OperationalEvent,
  type OperationalDashboard as Dashboard,
  type RevenueSnapshot,
  type RevenueTrend
} from "./api";
import type { PlatformOperationsConfiguration } from "./config";

type Props = { configuration: PlatformOperationsConfiguration; apiKey: string };

export default function OperationalDashboard({ configuration, apiKey }: Props) {
  const [dashboard, setDashboard] = useState<Dashboard>();
  const [revenue, setRevenue] = useState<RevenueSnapshot>();
  const [revenueTrend, setRevenueTrend] = useState<RevenueTrend>();
  const [events, setEvents] = useState<OperationalEvent[]>([]);
  const [revenueError, setRevenueError] = useState<string>();
  const [trendError, setTrendError] = useState<string>();
  const [error, setError] = useState<string>();

  useEffect(() => {
    loadOperationalDashboard(configuration, apiKey)
      .then(setDashboard)
      .catch(() => setError("The operational dashboard could not be loaded."));
    loadRevenueSnapshot(configuration, apiKey)
      .then(value => {
        setRevenue(value);
        loadRevenueTrend(configuration, apiKey)
          .then(setRevenueTrend)
          .catch(() => setTrendError("Revenue history is temporarily unavailable."));
      })
      .catch(() => setRevenueError("Live Stripe revenue is unavailable. Verify the protected Stripe revenue configuration."));
    loadOperationalEvents(configuration, apiKey)
      .then(setEvents)
      .catch(() => setEvents([]));
  }, [apiKey, configuration]);

  if (error) return <p className="state error">{error}</p>;
  if (!dashboard) return <p className="state">Loading operational dashboard…</p>;

  const metrics = [
    ["Total venues", dashboard.totalVenues],
    ["Active", dashboard.activeVenues],
    ["Trialing", dashboard.trialingVenues],
    ["Canceled · 30 days", dashboard.canceledLast30Days],
    ["Screens online", dashboard.onlineScreens],
    ["Screens offline", dashboard.offlineScreens],
    ["Screens outdated", dashboard.outdatedScreens]
  ] as const;

  const currency = new Intl.NumberFormat("en-US", { style: "currency", currency: revenue?.currency ?? "USD" });
  const trendMaximum = Math.max(...(revenueTrend?.points.map(point => point.mrr) ?? [0]), 1);

  return <section className="operational-dashboard">
    {revenueError
      ? <p className="state error">{revenueError}</p>
      : revenue
        ? <article className="revenue-panel">
          <div><p>Live Stripe revenue</p><h2>{currency.format(revenue.mrr)} MRR</h2><span>{currency.format(revenue.arr)} ARR · {currency.format(revenue.averageRevenuePerActiveSubscription)} average</span></div>
          <div className="tier-revenue">{revenue.tiers.map(tier => <div key={tier.tierId}><span>{tier.tierName}</span><strong>{currency.format(tier.mrr)}</strong></div>)}</div>
          {revenue.unmatchedPriceIds.length ? <p className="revenue-warning">{currency.format(revenue.unmatchedMrr)} uses unmapped Stripe prices: {revenue.unmatchedPriceIds.join(", ")}</p> : null}
        </article>
        : <p className="state">Loading live Stripe revenue…</p>}
    {revenueTrend?.points.length
      ? <article className="revenue-trend">
        <div><p>Revenue history</p><h2>Monthly MRR trend</h2><span>Latest persisted daily snapshot in each UTC month</span></div>
        <div className="trend-chart">{revenueTrend.points.map(point => <div className="trend-point" key={point.monthUtc}>
          <span>{point.mrrChangePercent == null ? "No prior month" : `${point.mrrChangePercent >= 0 ? "+" : ""}${point.mrrChangePercent}%`}</span>
          <div style={{ height: `${Math.max(8, point.mrr / trendMaximum * 100)}%` }} title={`${currency.format(point.mrr)} MRR · ${point.activeSubscriptions} active subscriptions`} />
          <strong>{new Date(point.monthUtc).toLocaleDateString("en-US", { month: "short", year: "2-digit", timeZone: "UTC" })}</strong>
        </div>)}</div>
      </article>
      : trendError
        ? <p className="state error">{trendError}</p>
        : null}
    <div className="metric-grid">{metrics.map(([label, value]) => <article key={label}><span>{label}</span><strong>{value}</strong></article>)}</div>
    <article className="screen-map"><div><p>Fleet health</p><h2>Screen health map</h2><span>{dashboard.onlineScreens} online · {dashboard.offlineScreens} offline</span></div>
      {dashboard.screens.length ? <div className="screen-dots">{dashboard.screens.map(screen => <div className="screen-health-item" key={screen.screenId} title={`${screen.venueName} · ${screen.screenName} · ${screen.status} · ${screen.lastSeen ? new Date(screen.lastSeen).toLocaleString() : "never seen"} · ${screen.platform ?? "unknown platform"} ${screen.appVersion ?? "unknown version"} · ${screen.versionStatus}`}><span className={screen.status} /><div><strong>{screen.screenName}</strong><small>{screen.venueName}{screen.location ? ` · ${screen.location}` : ""} · {screen.versionStatus === "outdated" ? `Update ${screen.appVersion ?? "unknown"} → ${screen.desiredAppVersion}` : screen.versionStatus}</small></div></div>)}</div> : <p className="empty">No screens have been registered.</p>}
    </article>
    <article className="event-feed"><div><p>Commercial activity</p><h2>Recent events</h2></div>
      {events.length
        ? <ol>{events.map(item => <li key={item.id}><span className={`event-type ${item.eventType}`}>{item.eventType.replace("_", " ")}</span><div><strong>{item.venueName}</strong><p>{item.summary}</p><small>{new Date(item.occurredUtc).toLocaleString()}</small></div></li>)}</ol>
        : <p className="empty">No commercial events have been recorded.</p>}
    </article>
  </section>;
}
