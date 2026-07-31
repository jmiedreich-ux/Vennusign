import { useEffect, useMemo, useState, type FormEvent } from "react";
import { loadVenueAdminSession, VenueAdminApiError, type VenueAdminSession } from "./api";
import { loadVenueAdminConfiguration } from "./config";
import {
  canOpenVenueAdminRoute,
  resolveVenueAdminRoute,
  venueAdminRoutes,
  type VenueAdminRoute
} from "./navigation.mjs";
import MenuSectionsEditor from "./MenuSectionsEditor";
import "./styles.css";

const tokenStorageKey = "vennu.venue-admin.token";

export default function App() {
  const configuration = useMemo(loadVenueAdminConfiguration, []);
  const [accessToken, setAccessToken] = useState(() => sessionStorage.getItem(tokenStorageKey) ?? "");
  const [session, setSession] = useState<VenueAdminSession>();
  const [route, setRoute] = useState<VenueAdminRoute>(() => resolveVenueAdminRoute(window.location.hash));
  const [error, setError] = useState<string>();

  useEffect(() => {
    if (!accessToken) return;
    const controller = new AbortController();
    loadVenueAdminSession(configuration, accessToken, controller.signal)
      .then(value => {
        setSession(value);
        setError(undefined);
      })
      .catch((reason: unknown) => {
        if (reason instanceof DOMException && reason.name === "AbortError") return;
        sessionStorage.removeItem(tokenStorageKey);
        setAccessToken("");
        setSession(undefined);
        setError(reason instanceof VenueAdminApiError ? reason.message : "The venue workspace is unavailable.");
      });
    return () => controller.abort();
  }, [accessToken, configuration]);

  useEffect(() => {
    const onHashChange = () => setRoute(resolveVenueAdminRoute(window.location.hash));
    window.addEventListener("hashchange", onHashChange);
    return () => window.removeEventListener("hashchange", onHashChange);
  }, []);

  const authorize = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const token = String(new FormData(event.currentTarget).get("accessToken") ?? "").trim();
    if (!token) return;
    sessionStorage.setItem(tokenStorageKey, token);
    setError(undefined);
    setAccessToken(token);
  };

  const signOut = () => {
    sessionStorage.removeItem(tokenStorageKey);
    setAccessToken("");
    setSession(undefined);
  };

  if (!accessToken || error) {
    return <main className="centered">
      <form className="access-card" onSubmit={authorize}>
        <span>Vennu Venue Admin</span>
        <h1>Open your venue</h1>
        <p>{error ?? "Use the protected access link supplied for your venue workspace."}</p>
        <label htmlFor="accessToken">Venue access token</label>
        <input id="accessToken" name="accessToken" type="password" autoComplete="current-password" required />
        <button type="submit">Continue</button>
      </form>
    </main>;
  }

  if (!session) {
    return <main className="centered"><p className="loading">Opening your venue…</p></main>;
  }

  const allowed = canOpenVenueAdminRoute(route, session.capabilities);

  return <div className="shell">
    <aside>
      <div className="brand"><span>V</span><div><strong>Vennu</strong><small>Venue Admin</small></div></div>
      <nav aria-label="Venue Admin">
        {venueAdminRoutes.map(item => {
          const unlocked = canOpenVenueAdminRoute(item, session.capabilities);
          return <a
            className={`${route.path === item.path ? "active " : ""}${unlocked ? "" : "locked"}`.trim()}
            href={`#/${item.path}`}
            key={item.path}
            aria-disabled={!unlocked}
          >
            <strong>{item.label}{unlocked ? "" : " · Locked"}</strong>
            <small>{item.description}</small>
          </a>;
        })}
      </nav>
      <button className="identity" type="button" onClick={signOut}>
        <span>{session.displayName.slice(0, 1)}</span>
        <div><strong>{session.displayName}</strong><small>Sign out</small></div>
      </button>
    </aside>
    <main>
      <header><div><p>Venue workspace</p><h1>{route.label}</h1></div><span>Secure session</span></header>
      {allowed && route.path === "menu"
        ? <MenuSectionsEditor
            configuration={configuration}
            apiKey={accessToken}
            venueId={session.venueId}
          />
        : allowed
        ? <section className="placeholder"><p>Foundation ready</p><h2>{route.label}</h2><span>This protected venue-scoped area is ready for the next migration package.</span></section>
        : <section className="placeholder locked-panel"><p>Upgrade available</p><h2>{route.label} is locked</h2><span>Your current venue access does not include this capability.</span></section>}
    </main>
  </div>;
}
