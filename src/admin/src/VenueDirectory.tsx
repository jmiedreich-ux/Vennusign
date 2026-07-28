import { useEffect, useMemo, useState } from "react";
import { loadVenueDirectory, type VenueDirectoryItem, type VenueDirectoryQuery } from "./api";
import type { AdminConfiguration } from "./config";

type Props = { configuration: AdminConfiguration; apiKey: string; onSelectVenue: (venueId: string) => void };

export default function VenueDirectory({ configuration, apiKey, onSelectVenue }: Props) {
  const [query, setQuery] = useState<VenueDirectoryQuery>({});
  const [venues, setVenues] = useState<VenueDirectoryItem[]>([]);
  const [error, setError] = useState<string>();
  const [loading, setLoading] = useState(true);
  const stableQuery = useMemo(() => query, [query.search, query.tier, query.status, query.health]);

  useEffect(() => {
    const controller = new AbortController();
    setLoading(true);
    setError(undefined);
    loadVenueDirectory(configuration, apiKey, stableQuery, controller.signal)
      .then(setVenues)
      .catch(reason => {
        if (!(reason instanceof DOMException && reason.name === "AbortError")) setError("The venue directory could not be loaded.");
      })
      .finally(() => setLoading(false));
    return () => controller.abort();
  }, [apiKey, configuration, stableQuery]);

  const update = (name: keyof VenueDirectoryQuery, value: string) =>
    setQuery(current => ({ ...current, [name]: value || undefined }));

  return <section className="directory">
    <div className="directory-toolbar">
      <input aria-label="Search venues" placeholder="Search venues…" value={query.search ?? ""} onChange={event => update("search", event.target.value)} />
      <select aria-label="Subscription status" value={query.status ?? ""} onChange={event => update("status", event.target.value)}>
        <option value="">All statuses</option><option value="active">Active</option><option value="trialing">Trialing</option><option value="canceled">Canceled</option><option value="unsubscribed">Unsubscribed</option>
      </select>
      <select aria-label="Screen health" value={query.health ?? ""} onChange={event => update("health", event.target.value)}>
        <option value="">All health</option><option value="online">Online</option><option value="degraded">Degraded</option><option value="offline">Offline</option><option value="no_screens">No screens</option>
      </select>
    </div>
    {loading ? <p className="state">Loading venues…</p> : error ? <p className="state error">{error}</p> : venues.length === 0 ? <p className="state">No venues match these filters.</p> :
      <div className="table-wrap"><table><thead><tr><th>Venue</th><th>Tier</th><th>Status</th><th>Screens</th><th>Last active</th><th>Overrides</th><th>Health</th></tr></thead><tbody>
        {venues.map(venue => <tr key={venue.venueId}><td><button className="venue-link" onClick={() => onSelectVenue(venue.venueId)}><strong>{venue.name}</strong><small>{venue.type}</small></button></td><td>{venue.tierName ?? "—"}</td><td>{venue.subscriptionStatus}</td><td>{venue.screenCount}</td><td>{venue.lastActiveUtc ? new Date(venue.lastActiveUtc).toLocaleString() : "Never"}</td><td>{venue.overrideCount}</td><td><span className={`health ${venue.health}`}>{venue.health.replace("_", " ")}</span></td></tr>)}
      </tbody></table></div>}
  </section>;
}
