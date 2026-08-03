import { useEffect, useMemo, useState, type FormEvent } from "react";
import { createVenue, loadVenueDirectory, type CreateVenueRequest, type VenueDirectoryItem, type VenueDirectoryQuery } from "./api";
import type { PlatformOperationsConfiguration } from "./config";
import { validateVenueDraft } from "./venueProvisioning.mjs";

type Props = { configuration: PlatformOperationsConfiguration; apiKey: string; initialQuery?: VenueDirectoryQuery; onSelectVenue: (venueId: string) => void };

export default function VenueDirectory({ configuration, apiKey, initialQuery = {}, onSelectVenue }: Props) {
  const [query, setQuery] = useState<VenueDirectoryQuery>(initialQuery);
  const [venues, setVenues] = useState<VenueDirectoryItem[]>([]);
  const [error, setError] = useState<string>();
  const [loading, setLoading] = useState(true);
  const [refreshVersion, setRefreshVersion] = useState(0);
  const [creating, setCreating] = useState(false);
  const [showCreate, setShowCreate] = useState(false);
  const [createError, setCreateError] = useState<string>();
  const initialKey = JSON.stringify(initialQuery);
  const stableQuery = useMemo(() => query, [query.search, query.tier, query.status, query.health]);
  const filtersActive = !!(query.search || query.tier || query.status || query.health);

  useEffect(() => setQuery(initialQuery), [initialKey]);
  useEffect(() => {
    const controller = new AbortController();
    setLoading(true); setError(undefined);
    loadVenueDirectory(configuration, apiKey, stableQuery, controller.signal)
      .then(setVenues)
      .catch(reason => { if (!(reason instanceof DOMException && reason.name === "AbortError")) setError("The venue directory could not be loaded. Retry without losing your filters."); })
      .finally(() => setLoading(false));
    return () => controller.abort();
  }, [apiKey, configuration, stableQuery, refreshVersion]);

  const update = (name: keyof VenueDirectoryQuery, value: string) => setQuery(current => ({ ...current, [name]: value || undefined }));
  const clearFilters = () => setQuery({});
  const submitVenue = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    const validation = validateVenueDraft({ name: data.get("name"), timezone: data.get("timezone"), type: data.get("type"), primaryLanguage: data.get("primaryLanguage"), secondaryLanguage: data.get("secondaryLanguage") } as Partial<CreateVenueRequest>);
    if (!validation.valid) { setCreateError("Complete the required venue details before continuing."); return; }
    setCreating(true); setCreateError(undefined);
    try { const result = await createVenue(configuration, apiKey, validation.venue); onSelectVenue(result.venueId); }
    catch { setCreateError("The venue could not be created. Check the details and try again."); }
    finally { setCreating(false); }
  };

  return <section className="directory">
    <div className="directory-toolbar">
      <input aria-label="Search venues" type="search" placeholder="Search venues…" value={query.search ?? ""} onChange={event => update("search", event.target.value)} />
      <input aria-label="Tier" placeholder="All tiers" value={query.tier ?? ""} onChange={event => update("tier", event.target.value)} />
      <select aria-label="Subscription status" value={query.status ?? ""} onChange={event => update("status", event.target.value)}><option value="">All statuses</option><option value="active">Active</option><option value="trialing">Trialing</option><option value="canceled">Canceled</option><option value="unsubscribed">Unsubscribed</option></select>
      <select aria-label="Screen health" value={query.health ?? ""} onChange={event => update("health", event.target.value)}><option value="">All health</option><option value="online">Online</option><option value="degraded">Degraded</option><option value="offline">Offline</option><option value="no_screens">No screens</option></select>
      {filtersActive ? <button type="button" onClick={clearFilters}>Clear filters</button> : null}
      <button className="create-venue-action" type="button" onClick={() => { setShowCreate(current => !current); setCreateError(undefined); }}>{showCreate ? "Cancel" : "Create venue"}</button>
    </div>
    <p className="directory-results" role="status">{loading ? "Searching venues…" : `${venues.length} ${venues.length === 1 ? "venue" : "venues"} found`}</p>
    {showCreate ? <form className="venue-create" onSubmit={submitVenue}><div><p>New venue</p><h2>Provision a venue</h2><span>Creates the venue with a Starter trial.</span></div><label>Name<input name="name" maxLength={200} required /></label><label>Timezone<input name="timezone" defaultValue="UTC" maxLength={100} required /></label><label>Venue type<input name="type" placeholder="Restaurant, café, bar…" maxLength={50} required /></label><label>Primary language<input name="primaryLanguage" defaultValue="en" maxLength={10} required /></label><label>Secondary language<input name="secondaryLanguage" maxLength={10} /></label><button type="submit" disabled={creating}>{creating ? "Creating…" : "Create and open"}</button>{createError ? <p className="venue-create-error" role="alert">{createError}</p> : null}</form> : null}
    {loading ? <p className="state">Loading venues…</p> : error ? <div className="state error" role="alert"><p>{error}</p><button type="button" onClick={() => setRefreshVersion(value => value + 1)}>Retry directory</button></div> : venues.length === 0 ? <div className="state"><p>{filtersActive ? "No venues match these filters." : "No venues have been provisioned."}</p>{filtersActive ? <button type="button" onClick={clearFilters}>Clear filters</button> : null}</div> :
      <div className="table-wrap"><table><caption className="sr-only">Venue support directory</caption><thead><tr><th>Venue</th><th>Tier</th><th>Status</th><th>Screens</th><th>Last active</th><th>Overrides</th><th>Health</th></tr></thead><tbody>{venues.map(venue => <tr key={venue.venueId}><td><button className="venue-link" onClick={() => onSelectVenue(venue.venueId)}><strong>{venue.name}</strong><small>{venue.type}</small></button></td><td>{venue.tierName ?? "—"}</td><td>{venue.subscriptionStatus}</td><td>{venue.screenCount}</td><td>{venue.lastActiveUtc ? new Date(venue.lastActiveUtc).toLocaleString() : "Never"}</td><td>{venue.overrideCount}</td><td><span className={`health ${venue.health}`}>{venue.health.replace("_", " ")}</span></td></tr>)}</tbody></table></div>}
  </section>;
}
