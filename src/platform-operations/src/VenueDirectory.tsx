import { useEffect, useMemo, useState, type FormEvent } from "react";
import { createVenue, loadVenueDirectory, type CreateVenueRequest, type VenueDirectoryItem, type VenueDirectoryQuery } from "./api";
import type { PlatformOperationsConfiguration } from "./config";
import { validateVenueDraft } from "./venueProvisioning.mjs";

type Props = { configuration: PlatformOperationsConfiguration; apiKey: string; onSelectVenue: (venueId: string) => void };

export default function VenueDirectory({ configuration, apiKey, onSelectVenue }: Props) {
  const [query, setQuery] = useState<VenueDirectoryQuery>({});
  const [venues, setVenues] = useState<VenueDirectoryItem[]>([]);
  const [error, setError] = useState<string>();
  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [showCreate, setShowCreate] = useState(false);
  const [createError, setCreateError] = useState<string>();
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

  const submitVenue = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    const validation = validateVenueDraft({
      name: data.get("name"),
      timezone: data.get("timezone"),
      type: data.get("type"),
      primaryLanguage: data.get("primaryLanguage"),
      secondaryLanguage: data.get("secondaryLanguage")
    } as Partial<CreateVenueRequest>);
    if (!validation.valid) {
      setCreateError("Complete the required venue details before continuing.");
      return;
    }

    setCreating(true);
    setCreateError(undefined);
    try {
      const result = await createVenue(configuration, apiKey, validation.venue);
      onSelectVenue(result.venueId);
    } catch {
      setCreateError("The venue could not be created. Check the details and try again.");
    } finally {
      setCreating(false);
    }
  };

  return <section className="directory">
    <div className="directory-toolbar">
      <input aria-label="Search venues" placeholder="Search venues…" value={query.search ?? ""} onChange={event => update("search", event.target.value)} />
      <select aria-label="Subscription status" value={query.status ?? ""} onChange={event => update("status", event.target.value)}>
        <option value="">All statuses</option><option value="active">Active</option><option value="trialing">Trialing</option><option value="canceled">Canceled</option><option value="unsubscribed">Unsubscribed</option>
      </select>
      <select aria-label="Screen health" value={query.health ?? ""} onChange={event => update("health", event.target.value)}>
        <option value="">All health</option><option value="online">Online</option><option value="degraded">Degraded</option><option value="offline">Offline</option><option value="no_screens">No screens</option>
      </select>
      <button className="create-venue-action" type="button" onClick={() => { setShowCreate(current => !current); setCreateError(undefined); }}>
        {showCreate ? "Cancel" : "Create venue"}
      </button>
    </div>
    {showCreate ? <form className="venue-create" onSubmit={submitVenue}>
      <div><p>New venue</p><h2>Provision a venue</h2><span>Creates the venue with a Starter trial.</span></div>
      <label>Name<input name="name" maxLength={200} required /></label>
      <label>Timezone<input name="timezone" defaultValue="UTC" maxLength={100} required /></label>
      <label>Venue type<input name="type" placeholder="Restaurant, café, bar…" maxLength={50} required /></label>
      <label>Primary language<input name="primaryLanguage" defaultValue="en" maxLength={10} required /></label>
      <label>Secondary language<input name="secondaryLanguage" maxLength={10} /></label>
      <button type="submit" disabled={creating}>{creating ? "Creating…" : "Create and open"}</button>
      {createError ? <p className="venue-create-error" role="alert">{createError}</p> : null}
    </form> : null}
    {loading ? <p className="state">Loading venues…</p> : error ? <p className="state error">{error}</p> : venues.length === 0 ? <p className="state">No venues match these filters.</p> :
      <div className="table-wrap"><table><thead><tr><th>Venue</th><th>Tier</th><th>Status</th><th>Screens</th><th>Last active</th><th>Overrides</th><th>Health</th></tr></thead><tbody>
        {venues.map(venue => <tr key={venue.venueId}><td><button className="venue-link" onClick={() => onSelectVenue(venue.venueId)}><strong>{venue.name}</strong><small>{venue.type}</small></button></td><td>{venue.tierName ?? "—"}</td><td>{venue.subscriptionStatus}</td><td>{venue.screenCount}</td><td>{venue.lastActiveUtc ? new Date(venue.lastActiveUtc).toLocaleString() : "Never"}</td><td>{venue.overrideCount}</td><td><span className={`health ${venue.health}`}>{venue.health.replace("_", " ")}</span></td></tr>)}
      </tbody></table></div>}
  </section>;
}
