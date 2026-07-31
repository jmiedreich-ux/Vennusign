import { useEffect, useMemo, useState, type FormEvent } from "react";
import {
  createCheckoutSession,
  createBillingPortalSession,
  loadVenueAdminSession,
  loadVenueBillingPresentation,
  VenueAdminApiError,
  type VenueAdminBillingPresentation,
  type VenueAdminSession
} from "./api";
import {
  checkoutRefreshDelays,
  readCheckoutReturnState,
  stripCheckoutReturnParameter,
  type CheckoutReturnState
} from "./checkoutFlow.mjs";
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
import BillingStatusCard from "./BillingStatusCard";
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
  const [checkoutLaunching, setCheckoutLaunching] = useState(false);
  const [checkoutError, setCheckoutError] = useState<string>();
  const [billingPortalOpening, setBillingPortalOpening] = useState(false);
  const [billingPortalError, setBillingPortalError] = useState<string>();
  const [checkoutReturn, setCheckoutReturn] = useState<CheckoutReturnState | undefined>(
    () => readCheckoutReturnState(window.location.search)
  );
  const [checkoutReturnNotice, setCheckoutReturnNotice] = useState(() =>
    checkoutReturn === "success"
      ? "Stripe returned successfully. Your plan and feature access are being confirmed from Vennu."
      : "Checkout was canceled. Your current plan and features were not changed."
  );

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
    if (!accessToken || checkoutReturn !== "success") return;
    const controller = new AbortController();
    const refreshAuthoritativeState = async () => {
      try {
        for (const delay of checkoutRefreshDelays) {
          await new Promise<void>(resolve => window.setTimeout(resolve, delay));
          if (controller.signal.aborted) return;
          const [nextSession, nextBilling] = await Promise.all([
            loadVenueAdminSession(configuration, accessToken, controller.signal),
            loadVenueBillingPresentation(configuration, accessToken, controller.signal)
          ]);
          setSession(nextSession);
          setBilling(nextBilling);
        }
        setCheckoutReturnNotice("Your current plan and feature access were refreshed from Vennu. Stripe webhooks remain authoritative if processing is still finishing.");
      } catch (reason: unknown) {
        if (reason instanceof DOMException && reason.name === "AbortError") return;
        setCheckoutReturnNotice("Stripe returned successfully, but Vennu could not refresh your plan yet. No access was changed from the return URL; refresh this page shortly.");
      }
    };
    void refreshAuthoritativeState();
    return () => controller.abort();
  }, [accessToken, checkoutReturn, configuration]);

  useEffect(() => {
    const onHashChange = () => setRoute(resolveVenueAdminRoute(window.location.hash));
    window.addEventListener("hashchange", onHashChange);
    return () => window.removeEventListener("hashchange", onHashChange);
  }, []);

  useEffect(() => {
    if (!upgradeContext) setCheckoutError(undefined);
  }, [upgradeContext]);

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
    setCheckoutLaunching(false);
    setCheckoutError(undefined);
    setBillingPortalOpening(false);
    setBillingPortalError(undefined);
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
  const continueUpgrade = async (interval: BillingInterval) => {
    if (!upgradeContext || !targetTier || checkoutLaunching) return;
    setCheckoutLaunching(true);
    setCheckoutError(undefined);
    setUpgradeNotice(`Opening secure checkout for ${upgradeContext.title} with ${interval} billing…`);
    try {
      const checkoutUrl = await createCheckoutSession(
        configuration,
        accessToken,
        targetTier.id,
        interval
      );
      window.location.assign(checkoutUrl);
    } catch (reason: unknown) {
      const message = reason instanceof VenueAdminApiError
        ? reason.message
        : "Secure checkout could not be opened.";
      setCheckoutError(message);
      setUpgradeNotice(message);
      setCheckoutLaunching(false);
    }
  };
  const dismissCheckoutReturn = () => {
    const search = stripCheckoutReturnParameter(window.location.search);
    window.history.replaceState(null, "", `${window.location.pathname}${search}${window.location.hash}`);
    setCheckoutReturn(undefined);
  };
  const openBillingPortal = async () => {
    if (billingPortalOpening) return;
    setBillingPortalOpening(true);
    setBillingPortalError(undefined);
    try {
      const portalUrl = await createBillingPortalSession(configuration, accessToken);
      window.location.assign(portalUrl);
    } catch (reason: unknown) {
      setBillingPortalError(reason instanceof VenueAdminApiError
        ? reason.message
        : "Secure billing management could not be opened.");
      setBillingPortalOpening(false);
    }
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
      {checkoutReturn ? <section className={`checkout-return checkout-return--${checkoutReturn}`} role="status">
        <div>
          <strong>{checkoutReturn === "success" ? "Confirming your plan" : "Checkout canceled"}</strong>
          <p>{checkoutReturnNotice}</p>
        </div>
        <button type="button" onClick={dismissCheckoutReturn}>Dismiss</button>
      </section> : null}
      {inlineOpportunity && !upgradeContext
        ? <InlineFeatureHint
            key={`${inlineOpportunity.featureKey}-${dismissalVersion}`}
            opportunity={inlineOpportunity}
            onDismiss={dismiss}
            onUpgrade={setUpgradeContext}
          />
        : null}
      {allowed && route.path === "billing" && billing
        ? <BillingStatusCard
            currentTier={billing.currentTier}
            subscription={billing.subscription}
            isOpening={billingPortalOpening}
            error={billingPortalError}
            onManage={openBillingPortal}
          />
        : allowed && route.path === "menu"
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
      isSubmitting={checkoutLaunching}
      error={checkoutError}
    /> : null}
  </div>;
}
