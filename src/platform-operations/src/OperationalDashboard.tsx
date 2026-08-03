import { useEffect, useMemo, useState } from "react";
import {
  loadOperationalDashboard,
  loadOperationalEvents,
  loadRevenueSnapshot,
  loadRevenueTrend,
  type OperationalEvent,
  type OperationalDashboard as Dashboard,
  type RevenueSnapshot,
  type RevenueTrend,
  type VenueDirectoryQuery
} from "./api";
import type { PlatformOperationsConfiguration } from "./config";

type Props = {
  configuration: PlatformOperationsConfiguration;
  apiKey: string;
  onOpenVenues: (query?: VenueDirectoryQuery, venueId?: string) => void;
};

export default function OperationalDashboard({ configuration, apiKey, onOpenVenues }: Props) {
  const [dashboard, setDashboard] = useState<Dashboard>();
  const [revenue, setRevenue] = useState<RevenueSnapshot>();
  const [revenueTrend, setRevenueTrend] = useState<RevenueTrend>();
  const [events, setEvents] = useState<OperationalEvent[]>([]);
  const [dashboardError, setDashboardError] = useState<string>();
  const [revenueError, setRevenueError] = useState<string>();
  const [trendError, setTrendError] = useState<string>();
  const [eventsError, setEventsError] = useState<string>();
  const [refreshVersion, setRefreshVersion] = useState(0);
  const [refreshing, setRefreshing] = useState(true);
  const [lastUpdated, setLastUpdated] = useState<Date>();
  const [screenFilter, setScreenFilter] = useState<"all" | "offline" | "outdated">("all");

  useEffect(() => {
    const controller = new AbortController();
    setRefreshing(true);
    setDashboardError(undefined); setRevenueError(undefined); setTrendError(undefined); setEventsError(undefined);
    const dashboardRequest = loadOperationalDashboard(configuration, apiKey)
      .then(setDashboard)
      .catch(() => setDashboardError("The operational dashboard could not be loaded. Retry or open the venue directory for support detail."));
    const revenueRequest = loadRevenueSnapshot(configuration, apiKey)
      .then(async value => {
        setRevenue(value);
        await loadRevenueTrend(configuration, apiKey).then(setRevenueTrend).catch(() => setTrendError("Revenue history is temporarily unavailable."));
      })
      .catch(() => setRevenueError("Live Stripe revenue is unavailable. Verify the protected Stripe revenue configuration."));
    const eventsRequest = loadOperationalEvents(configuration, apiKey)
      .then(setEvents)
      .catch(() => setEventsError("Recent commercial events could not be loaded. Retry to distinguish an empty feed from a service failure."));
    Promise.allSettled([dashboardRequest, revenueRequest, eventsRequest]).then(() => {
      if (!controller.signal.aborted) { setRefreshing(false); setLastUpdated(new Date()); }
    });
    return () => controller.abort();
  }, [apiKey, configuration, refreshVersion]);

  const visibleScreens = useMemo(() => dashboard?.screens.filter(screen =>
    screenFilter === "all" || (screenFilter === "offline" ? screen.status === "offline" : screen.versionStatus === "outdated")) ?? [], [dashboard, screenFilter]);
  const currency = new Intl.NumberFormat("en-US", { style: "currency", currency: revenue?.currency ?? "USD" });
  const trendMaximum = Math.max(...(revenueTrend?.points.map(point => point.mrr) ?? [0]), 1);
  const metrics: Array<{ label: string; value: number; query: VenueDirectoryQuery }> = dashboard ? [
    { label: "Total venues", value: dashboard.totalVenues, query: {} },
    { label: "Active", value: dashboard.activeVenues, query: { status: "active" } },
    { label: "Trialing", value: dashboard.trialingVenues, query: { status: "trialing" } },
    { label: "Canceled · 30 days", value: dashboard.canceledLast30Days, query: { status: "canceled" } },
    { label: "Screens online", value: dashboard.onlineScreens, query: { health: "online" } },
    { label: "Screens offline", value: dashboard.offlineScreens, query: { health: "offline" } },
    { label: "Screens outdated", value: dashboard.outdatedScreens, query: {} }
  ] : [];

  return <section className="operational-dashboard">
    <div className="dashboard-toolbar">
      <p role="status">{refreshing ? "Refreshing operational data…" : lastUpdated ? `Updated ${lastUpdated.toLocaleTimeString()}` : "Operational data not yet refreshed"}</p>
      <button type="button" disabled={refreshing} onClick={() => setRefreshVersion(value => value + 1)}>{refreshing ? "Refreshing…" : "Refresh dashboard"}</button>
    </div>
    {dashboardError ? <div className="state error" role="alert"><p>{dashboardError}</p><button type="button" onClick={() => setRefreshVersion(value => value + 1)}>Retry dashboard</button></div> : null}
    {revenueError
      ? <p className="state error" role="alert">{revenueError}</p>
      : revenue
        ? <article className="revenue-panel"><div><p>Live Stripe revenue</p><h2>{currency.format(revenue.mrr)} MRR</h2><span>{currency.format(revenue.arr)} ARR · {currency.format(revenue.averageRevenuePerActiveSubscription)} average</span></div><div className="tier-revenue">{revenue.tiers.map(tier => <div key={tier.tierId}><span>{tier.tierName}</span><strong>{currency.format(tier.mrr)}</strong></div>)}</div>{revenue.unmatchedPriceIds.length ? <p className="revenue-warning">{currency.format(revenue.unmatchedMrr)} uses unmapped Stripe prices: {revenue.unmatchedPriceIds.join(", ")}</p> : null}</article>
        : <p className="state">Loading live Stripe revenue…</p>}
    {revenueTrend?.points.length
      ? <article className="revenue-trend"><div><p>Revenue history</p><h2>Monthly MRR trend</h2><span>Latest persisted daily snapshot in each UTC month</span></div><div className="trend-chart">{revenueTrend.points.map(point => <div className="trend-point" key={point.monthUtc}><span>{point.mrrChangePercent == null ? "No prior month" : `${point.mrrChangePercent >= 0 ? "+" : ""}${point.mrrChangePercent}%`}</span><div style={{ height: `${Math.max(8, point.mrr / trendMaximum * 100)}%` }} title={`${currency.format(point.mrr)} MRR · ${point.activeSubscriptions} active subscriptions`} /><strong>{new Date(point.monthUtc).toLocaleDateString("en-US", { month: "short", year: "2-digit", timeZone: "UTC" })}</strong></div>)}</div></article>
      : trendError ? <p className="state error" role="alert">{trendError}</p> : null}
    {dashboard ? <div className="metric-grid">{metrics.map(metric => <button type="button" key={metric.label} onClick={() => onOpenVenues(metric.query)}><span>{metric.label}</span><strong>{metric.value}</strong><small>Open matching venues</small></button>)}</div> : !dashboardError ? <p className="state">Loading operational dashboard…</p> : null}
    {dashboard ? <article className="screen-map"><div className="panel-heading"><div><p>Fleet health</p><h2>Screen health map</h2><span>{dashboard.onlineScreens} online · {dashboard.offlineScreens} offline · {dashboard.outdatedScreens} outdated</span></div><label>Show<select value={screenFilter} onChange={event => setScreenFilter(event.target.value as typeof screenFilter)}><option value="all">All screens</option><option value="offline">Offline only</option><option value="outdated">Outdated only</option></select></label></div>
      {visibleScreens.length ? <div className="screen-dots">{visibleScreens.map(screen => <button type="button" className="screen-health-item" key={screen.screenId} onClick={() => screen.venueId && onOpenVenues({}, screen.venueId)} disabled={!screen.venueId}><span className={screen.status} /><div><strong>{screen.screenName}</strong><small>{screen.venueName}{screen.location ? ` · ${screen.location}` : ""} · {screen.versionStatus === "outdated" ? `Update ${screen.appVersion ?? "unknown"} → ${screen.desiredAppVersion}` : screen.versionStatus} · {screen.lastSeen ? new Date(screen.lastSeen).toLocaleString() : "never seen"}</small></div></button>)}</div> : <p className="empty">No screens match this support filter.</p>}
    </article> : null}
    <article className="event-feed"><div className="panel-heading"><div><p>Commercial activity</p><h2>Recent events</h2></div>{eventsError ? <button type="button" onClick={() => setRefreshVersion(value => value + 1)}>Retry events</button> : null}</div>
      {eventsError ? <p className="state error" role="alert">{eventsError}</p> : events.length ? <ol>{events.map(item => <li key={item.id}><button type="button" onClick={() => onOpenVenues({}, item.venueId)}><span className={`event-type ${item.eventType}`}>{item.eventType.replace("_", " ")}</span><div><strong>{item.venueName}</strong><p>{item.summary}</p><small>{new Date(item.occurredUtc).toLocaleString()}</small></div></button></li>)}</ol> : <p className="empty">No commercial events have been recorded.</p>}
    </article>
  </section>;
}
