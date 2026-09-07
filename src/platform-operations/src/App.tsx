import { useEffect, useMemo, useState, type FormEvent } from "react";
import { PlatformOperationsApiError, loadSession, type PlatformOperationsSession, type VenueDirectoryQuery } from "./api";
import { loadPlatformOperationsConfiguration } from "./config";
import "./styles.css";

const accessKeyStorageKey = "vennusign.platform-operations.key";
const legacyAccessKeyStorageKey = "vennu.admin.key";
import VenueDirectory from "./VenueDirectory";
import VenueDetail from "./VenueDetail";
import TierManagement from "./TierManagement";
import FeatureMatrix from "./FeatureMatrix";
import OperationalDashboard from "./OperationalDashboard";
import OnboardingSupport from "./OnboardingSupport";
import SystemConfiguration from "./SystemConfiguration";
import TestAgent from "./TestAgent";

const routes = [
  { path: "dashboard", label: "Dashboard", description: "Revenue and operational health" },
  { path: "venues", label: "Venues", description: "Venue directory and support context" },
  { path: "onboarding", label: "Onboarding", description: "Customer journey support visibility" },
  { path: "tiers", label: "Tiers", description: "Tier catalogue and billing mapping" },
  { path: "features", label: "Features", description: "Feature access matrix" },
  { path: "configuration", label: "Configuration", description: "Environment and application settings" }
  ,{ path: "test-agent", label: "AI Test Agent", description: "Autonomous product testing experiment" }
] as const;

function currentRoute() {
  const value = window.location.hash.replace(/^#\/?/, "");
  return routes.find(route => route.path === value) ?? routes[0];
}

function accessFailureMessage(reason: unknown) {
  if (reason instanceof PlatformOperationsApiError) {
    if (reason.status === 401) return "That access key was rejected or has expired. Request a current key through the protected operations channel, then try again.";
    if (reason.status === 403) return "That key is valid but does not grant Platform Operations access. Ask an administrator to verify your operations permission.";
    return reason.message;
  }
  if (reason instanceof DOMException && reason.name === "AbortError") return undefined;
  return "The Platform Operations API is unavailable. Check the service status, then try again.";
}

export default function App() {
  const [selectedVenueId, setSelectedVenueId] = useState<string>();
  const configuration = useMemo(loadPlatformOperationsConfiguration, []);
  const [apiKey, setApiKey] = useState(() => sessionStorage.getItem(accessKeyStorageKey) ?? sessionStorage.getItem(legacyAccessKeyStorageKey) ?? "");
  const [session, setSession] = useState<PlatformOperationsSession>();
  const [route, setRoute] = useState(currentRoute);
  const [error, setError] = useState<string>();
  const [venueDirectoryQuery, setVenueDirectoryQuery] = useState<VenueDirectoryQuery>({});

  useEffect(() => {
    const controller = new AbortController();
    if (!apiKey) return () => controller.abort();
    loadSession(configuration, apiKey, controller.signal)
      .then(setSession)
      .catch((reason: unknown) => {
        sessionStorage.removeItem(accessKeyStorageKey);
        sessionStorage.removeItem(legacyAccessKeyStorageKey);
        setApiKey("");
        setError(accessFailureMessage(reason));
      });
    return () => controller.abort();
  }, [apiKey, configuration]);

  useEffect(() => {
    const onHashChange = () => setRoute(currentRoute());
    window.addEventListener("hashchange", onHashChange);
    return () => window.removeEventListener("hashchange", onHashChange);
  }, []);

  const authorize = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    const suppliedKey = String(data.get("platformOperationsKey") ?? "").trim();
    if (!suppliedKey) return;
    setError(undefined);
    sessionStorage.setItem(accessKeyStorageKey, suppliedKey);
    sessionStorage.removeItem(legacyAccessKeyStorageKey);
    setApiKey(suppliedKey);
  };

  const openVenues = (query: VenueDirectoryQuery = {}, venueId?: string) => {
    setVenueDirectoryQuery(query);
    setSelectedVenueId(venueId);
    window.location.hash = "/venues";
  };

  const signOut = () => {
    sessionStorage.removeItem(accessKeyStorageKey);
    sessionStorage.removeItem(legacyAccessKeyStorageKey);
    setError(undefined);
    setSession(undefined);
    setApiKey("");
  };

  if (!apiKey || error) {
    return <main className="centered"><form className="access-card" onSubmit={authorize}><span>Vennusign Internal</span><h1>Platform Operations access</h1><p id="access-guidance" className={error ? "access-error" : undefined} role={error ? "alert" : undefined}>{error ?? "Enter the access key supplied through the protected operations channel."}</p><label htmlFor="platformOperationsKey">Access key</label><input id="platformOperationsKey" name="platformOperationsKey" type="password" autoComplete="current-password" aria-describedby="access-guidance" aria-invalid={error ? true : undefined} required /><button type="submit">Open workspace</button></form></main>;
  }
  if (!session) {
    return <main className="centered"><p className="loading">Opening secure workspace…</p></main>;
  }

  return (
    <div className="shell">
      <aside>
        <div className="brand"><span>V</span><div><strong>Vennusign</strong><small>Platform Operations</small></div></div>
        <nav aria-label="Platform Operations">
          {routes.map(item => <a className={route.path === item.path ? "active" : ""} href={`#/${item.path}`} key={item.path}><strong>{item.label}</strong><small>{item.description}</small></a>)}
        </nav>
        <button className="identity" type="button" onClick={signOut}><span>{session.displayName.slice(0, 1)}</span><div><strong>{session.displayName}</strong><small>Sign out</small></div></button>
      </aside>
      <main>
        <header><div><p>Internal operations</p><h1>{route.label}</h1></div><div className="header-actions"><span className="environment">Live workspace</span><button className="mobile-signout" type="button" onClick={signOut}>Sign out</button></div></header>
        {route.path === "dashboard"
          ? <OperationalDashboard configuration={configuration} apiKey={apiKey} onOpenVenues={openVenues} />
          : route.path === "venues"
          ? selectedVenueId
            ? <VenueDetail configuration={configuration} apiKey={apiKey} venueId={selectedVenueId} onBack={() => setSelectedVenueId(undefined)} />
            : <VenueDirectory configuration={configuration} apiKey={apiKey} initialQuery={venueDirectoryQuery} onSelectVenue={setSelectedVenueId} />
          : route.path === "tiers"
            ? <TierManagement configuration={configuration} apiKey={apiKey} />
          : route.path === "onboarding"
            ? <OnboardingSupport configuration={configuration} apiKey={apiKey} />
          : route.path === "features"
            ? <FeatureMatrix configuration={configuration} apiKey={apiKey} />
          : route.path === "configuration"
            ? <SystemConfiguration configuration={configuration} apiKey={apiKey} />
          : route.path === "test-agent"
            ? <TestAgent configuration={configuration} apiKey={apiKey} />
          : null}
      </main>
    </div>
  );
}
