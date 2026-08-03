import { useEffect, useMemo, useState, type FormEvent } from "react";
import { AdminApiError, loadSession, type AdminSession } from "./api";
import { loadAdminConfiguration } from "./config";
import "./styles.css";
import VenueDirectory from "./VenueDirectory";
import VenueDetail from "./VenueDetail";
import TierManagement from "./TierManagement";
import FeatureMatrix from "./FeatureMatrix";
import OperationalDashboard from "./OperationalDashboard";
import OnboardingSupport from "./OnboardingSupport";
import SystemConfiguration from "./SystemConfiguration";

const routes = [
  { path: "dashboard", label: "Dashboard", description: "Revenue and operational health" },
  { path: "venues", label: "Venues", description: "Venue directory and support context" },
  { path: "onboarding", label: "Onboarding", description: "Customer journey support visibility" },
  { path: "tiers", label: "Tiers", description: "Tier catalogue and billing mapping" },
  { path: "features", label: "Features", description: "Feature access matrix" },
  { path: "configuration", label: "Configuration", description: "Environment and application settings" }
] as const;

function currentRoute() {
  const value = window.location.hash.replace(/^#\/?/, "");
  return routes.find(route => route.path === value) ?? routes[0];
}

export default function App() {
  const [selectedVenueId, setSelectedVenueId] = useState<string>();
  const configuration = useMemo(loadAdminConfiguration, []);
  const [apiKey, setApiKey] = useState(() => sessionStorage.getItem("vennu.admin.key") ?? "");
  const [session, setSession] = useState<AdminSession>();
  const [route, setRoute] = useState(currentRoute);
  const [error, setError] = useState<string>();

  useEffect(() => {
    const controller = new AbortController();
    if (!apiKey) return () => controller.abort();
    loadSession(configuration, apiKey, controller.signal)
      .then(setSession)
      .catch((reason: unknown) => {
        sessionStorage.removeItem("vennu.admin.key");
        setApiKey("");
        if (reason instanceof AdminApiError) setError(reason.message);
        else if (!(reason instanceof DOMException && reason.name === "AbortError")) setError("The admin API is unavailable.");
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
    const suppliedKey = String(data.get("adminKey") ?? "").trim();
    if (!suppliedKey) return;
    setError(undefined);
    sessionStorage.setItem("vennu.admin.key", suppliedKey);
    setApiKey(suppliedKey);
  };

  if (!apiKey || error) {
    return <main className="centered"><form className="access-card" onSubmit={authorize}><span>Vennusign Internal</span><h1>Super Admin access</h1><p>{error ?? "Enter the access key supplied through the protected operations channel."}</p><label htmlFor="adminKey">Access key</label><input id="adminKey" name="adminKey" type="password" autoComplete="current-password" required /><button type="submit">Open workspace</button></form></main>;
  }
  if (!session) {
    return <main className="centered"><p className="loading">Opening secure workspace…</p></main>;
  }

  return (
    <div className="shell">
      <aside>
        <div className="brand"><span>V</span><div><strong>Vennusign</strong><small>Super Admin</small></div></div>
        <nav aria-label="Super Admin">
          {routes.map(item => <a className={route.path === item.path ? "active" : ""} href={`#/${item.path}`} key={item.path}><strong>{item.label}</strong><small>{item.description}</small></a>)}
        </nav>
        <button className="identity" type="button" onClick={() => { sessionStorage.removeItem("vennu.admin.key"); setSession(undefined); setApiKey(""); }}><span>{session.displayName.slice(0, 1)}</span><div><strong>{session.displayName}</strong><small>Sign out</small></div></button>
      </aside>
      <main>
        <header><div><p>Internal operations</p><h1>{route.label}</h1></div><span className="environment">Live workspace</span></header>
        {route.path === "dashboard"
          ? <OperationalDashboard configuration={configuration} apiKey={apiKey} />
          : route.path === "venues"
          ? selectedVenueId
            ? <VenueDetail configuration={configuration} apiKey={apiKey} venueId={selectedVenueId} onBack={() => setSelectedVenueId(undefined)} />
            : <VenueDirectory configuration={configuration} apiKey={apiKey} onSelectVenue={setSelectedVenueId} />
          : route.path === "tiers"
            ? <TierManagement configuration={configuration} apiKey={apiKey} />
          : route.path === "onboarding"
            ? <OnboardingSupport configuration={configuration} apiKey={apiKey} />
          : route.path === "features"
            ? <FeatureMatrix configuration={configuration} apiKey={apiKey} />
          : route.path === "configuration"
            ? <SystemConfiguration configuration={configuration} apiKey={apiKey} />
          : null}
      </main>
    </div>
  );
}
