import { useEffect, useMemo, useState, type FormEvent } from "react";
import {
  loadVenueAdminSession,
  loadVenueBillingPresentation,
  VenueAdminApiError,
  type VenueAdminBillingPresentation,
  type VenueAdminSession
} from "./api";
import { loadVenueAdminConfiguration } from "./config";
import {
  canOpenVenueAdminRoute,
  resolveVenueAdminRoute,
  venueAdminRoutes,
  type VenueAdminRoute
} from "./navigation.mjs";
import MenuSectionsEditor from "./MenuSectionsEditor";
import VenueOperations from "./VenueOperations";
import InlineFeatureHint from "./InlineFeatureHint";
import LockedNavigationItem from "./LockedNavigationItem";
import LockedSectionPreview from "./LockedSectionPreview";
import SidebarUpgradeNudge from "./SidebarUpgradeNudge";
import UpgradeModal, { type BillingInterval } from "./UpgradeModal";
import {
  dismissUpgradeFeature,
  listUpgradeOpportunities,
  readDismissedUpgradeFeatures,
  upgradePanelForFeature,
  type UpgradeOpportunity
} from "./upgradeExperience.mjs";
import "./styles.css";

const tokenStorageKey = "vennu.venue-admin.token";

export default function App() {
  const configuration = useMemo(loadVenueAdminConfiguration, []);
  const [accessToken, setAccessToken] = useState(() => sessionStorage.getItem(tokenStorageKey) ?? "");
  const [session, setSession] = useState<VenueAdminSession>();
  const [billing, setBilling] = useState<VenueAdminBillingPresentation>();
  const [route, setRoute] = useState<VenueAdminRoute>(() => resolveVenueAdminRoute(window.location.hash));
  const [error, setError] = useState<string>();
  const [dismissalVersion, setDismissalVersion] = useState(0);
  const [upgradeContext, setUpgradeContext] = useState<Readonly<UpgradeOpportunity>>();
  const [upgradeNotice, setUpgradeNotice] = useState<string>();

  useEffect(() => {
    if (!accessToken) return;
    const controller = new AbortController();
    loadVenueAdminSession(configuration, accessToken, controller.signal)
      .then(value => {
        setSession(value);
        setError(undefined);
        loadVenueBillingPresentation(configuration, accessToken, controller.signal)
          .then(setBilling)
          .catch(reason => {
            if (!(reason instanceof DOMException && reason.name === "AbortError")) {
              setBilling(undefined);
            }
          });
      })
      .catch((reason: unknown) => {
        if (reason instanceof DOMException && reason.name === "AbortError") return;
        sessionStorage.removeItem(tokenStorageKey);
        setAccessToken("");
        setSession(undefined);
        setBilling(undefined);
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
    setBilling(undefined);
    setUpgradeContext(undefined);
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
  const opportunities = billing
    ? listUpgradeOpportunities(
        billing.effectiveFeatures,
        readDismissedUpgradeFeatures()
      )
    : [];
  const routePanel = route.path === "themes" || route.path === "screens"
    ? "design"
    : route.path === "menu"
      ? "menu"
      : route.path === "schedules"
        ? "scheduling"
        : "operations";
  const inlineOpportunity = allowed
    ? opportunities.find(item => upgradePanelForFeature(item.featureKey) === routePanel)
    : undefined;
  const lockedOpportunity = !allowed
    ? opportunities.find(item => item.featureKey === route.upgradeFeature) ?? opportunities[0]
    : undefined;
  const dismiss = (featureKey: string) => {
    dismissUpgradeFeature(featureKey);
    setDismissalVersion(version => version + 1);
  };
  const targetTier = upgradeContext
    ? billing?.availableTiers.find(tier => tier.slug === upgradeContext.requiredTier)
    : undefined;
  const continueUpgrade = (interval: BillingInterval) => {
    if (!upgradeContext) return;
    setUpgradeNotice(`${upgradeContext.title} selected with ${interval} billing. Secure checkout is the next step.`);
    setUpgradeContext(undefined);
  };

  return <div className="shell">
    <aside>
      <div className="brand"><span>V</span><div><strong>Vennu</strong><small>Venue Admin</small></div></div>
      <nav aria-label="Venue Admin">
        {venueAdminRoutes.map(item => {
          const unlocked = canOpenVenueAdminRoute(item, session.capabilities);
          const opportunity = !unlocked
            ? opportunities.find(candidate => candidate.featureKey === item.upgradeFeature)
            : undefined;
          return opportunity ? <LockedNavigationItem
            key={item.path}
            opportunity={opportunity}
            onUpgrade={setUpgradeContext}
          /> : <a
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
      {billing && allowed && !inlineOpportunity && !upgradeContext
        ? <SidebarUpgradeNudge
            key={dismissalVersion}
            effectiveFeatures={billing.effectiveFeatures}
            onUpgrade={setUpgradeContext}
          />
        : null}
      {upgradeNotice ? <p className="sidebar-upgrade-context" role="status">{upgradeNotice}</p> : null}
      <button className="identity" type="button" onClick={signOut}>
        <span>{session.displayName.slice(0, 1)}</span>
        <div><strong>{session.displayName}</strong><small>Sign out</small></div>
      </button>
    </aside>
    <main>
      <header><div><p>Venue workspace</p><h1>{route.label}</h1></div><span>Secure session</span></header>
      {inlineOpportunity && !upgradeContext
        ? <InlineFeatureHint
            key={`${inlineOpportunity.featureKey}-${dismissalVersion}`}
            opportunity={inlineOpportunity}
            onDismiss={dismiss}
            onUpgrade={setUpgradeContext}
          />
        : null}
      {allowed && route.path === "menu"
        ? <MenuSectionsEditor
            configuration={configuration}
            apiKey={accessToken}
            venueId={session.venueId}
          />
        : allowed && ["screens", "themes", "schedules", "tap-list"].includes(route.path)
        ? <VenueOperations
            configuration={configuration}
            accessToken={accessToken}
            venueId={session.venueId}
            capabilities={session.capabilities}
            area={route.path as "screens" | "themes" | "schedules" | "tap-list"}
          />
        : allowed
        ? <section className="placeholder"><p>Foundation ready</p><h2>{route.label}</h2><span>This protected venue-scoped area is ready for the next migration package.</span></section>
        : lockedOpportunity
        ? <LockedSectionPreview
            key={`${lockedOpportunity.featureKey}-${dismissalVersion}`}
            opportunity={lockedOpportunity}
            onDismiss={dismiss}
            onUpgrade={setUpgradeContext}
          />
        : <section className="placeholder locked-panel"><p>Upgrade available</p><h2>{route.label} is locked</h2><span>Your current venue access does not include this capability.</span></section>}
    </main>
    {upgradeContext && billing && targetTier ? <UpgradeModal
      opportunity={upgradeContext}
      currentTier={billing.currentTier}
      targetTier={targetTier}
      onClose={() => setUpgradeContext(undefined)}
      onUpgrade={continueUpgrade}
    /> : null}
  </div>;
}
