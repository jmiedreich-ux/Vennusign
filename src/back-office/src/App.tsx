import { useEffect, useMemo, useState, type FormEvent } from "react";
import {
  createCheckoutSession,
  createBillingPortalSession,
  createTierBillingPortalSession,
  createHaasCheckoutSession,
  loadBackOfficeSession,
  loadVenueBillingPresentation,
  selectBackOfficeVenue,
  clearBackOfficeVenueContext,
  BackOfficeApiError,
  type BackOfficeBillingPresentation,
  type BackOfficeTierSummary,
  type BackOfficeSession
} from "./api";
import {
  checkoutRefreshDelays,
  readCheckoutReturnState,
  stripCheckoutReturnParameter,
  type CheckoutReturnState
} from "./checkoutFlow.mjs";
import { loadBackOfficeConfiguration } from "./config";
import {
  canOpenBackOfficeRoute,
  decisionForBackOfficeRoute,
  resolveBackOfficeRoute,
  backOfficeNavigationGroups,
  type BackOfficeRoute
} from "./navigation.mjs";
import MenuSectionsEditor from "./MenuSectionsEditor";
import PosIntegrationAdministration from "./PosIntegrationAdministration";
import VenueOperations from "./VenueOperations";
import InlineFeatureHint from "./InlineFeatureHint";
import LockedNavigationItem from "./LockedNavigationItem";
import LockedSectionPreview from "./LockedSectionPreview";
import SidebarUpgradeNudge from "./SidebarUpgradeNudge";
import UpgradeSheet, { type BillingInterval } from "./UpgradeSheet";
import BillingStatusCard from "./BillingStatusCard";
import TierDecisionDialog from "./TierDecisionDialog";
import AccountSecurity from "./AccountSecurity";
import DaypartHome from "./DaypartHome";
import { useDestructiveReview } from "./DestructiveReviewDialog";
import {
  clearPendingTierDecision,
  readPendingTierDecision,
  resolvePendingTierDecision,
  writePendingTierDecision,
  type PendingTierDecision
} from "./billingDecision.mjs";
import {
  dismissUpgradeFeature,
  listUpgradeOpportunities,
  readDismissedUpgradeFeatures,
  upgradePanelForFeature,
  type UpgradeOpportunity
} from "./upgradeExperience.mjs";
import "./styles.css";
import { revokeCustomerSession } from "./customerOnboardingApi";

const tokenStorageKey = "vennusign.back-office.token";
const customerSessionAccess = "customer-session";

export default function App() {
  const configuration = useMemo(loadBackOfficeConfiguration, []);
  const starterMenu = useMemo(() => {
    const value = new URLSearchParams(window.location.search).get("starterMenu");
    return value && ["restaurant", "cafe", "bar"].includes(value) ? value as "restaurant" | "cafe" | "bar" : undefined;
  }, []);
  const [accessToken, setAccessToken] = useState(() => sessionStorage.getItem(tokenStorageKey) ?? customerSessionAccess);
  const [session, setSession] = useState<BackOfficeSession>();
  const [billing, setBilling] = useState<BackOfficeBillingPresentation>();
  const [route, setRoute] = useState<BackOfficeRoute>(() => resolveBackOfficeRoute(window.location.hash));
  const [navigationOpen, setNavigationOpen] = useState(false);
  const [error, setError] = useState<string>();
  const [dismissalVersion, setDismissalVersion] = useState(0);
  const [upgradeContext, setUpgradeContext] = useState<Readonly<UpgradeOpportunity>>();
  const [upgradeNotice, setUpgradeNotice] = useState<string>();
  const [checkoutLaunching, setCheckoutLaunching] = useState(false);
  const [checkoutError, setCheckoutError] = useState<string>();
  const [billingPortalOpening, setBillingPortalOpening] = useState(false);
  const [billingPortalError, setBillingPortalError] = useState<string>();
  const [tierDecision, setTierDecision] = useState<BackOfficeTierSummary>();
  const [tierDecisionOpening, setTierDecisionOpening] = useState(false);
  const [tierDecisionError, setTierDecisionError] = useState<string>();
  const [pendingTier, setPendingTier] = useState<PendingTierDecision | undefined>(() => readPendingTierDecision());
  const [pendingTierNotice, setPendingTierNotice] = useState<string>();
  const [haasOpening, setHaasOpening] = useState<string>();
  const [haasError, setHaasError] = useState<string>();
  const [contextSwitching, setContextSwitching] = useState(false);
  const [contextNotice, setContextNotice] = useState<string>();
  const { review, reviewDialog } = useDestructiveReview();
  const [checkoutReturn, setCheckoutReturn] = useState<CheckoutReturnState | undefined>(
    () => readCheckoutReturnState(window.location.search)
  );
  const [checkoutReturnNotice, setCheckoutReturnNotice] = useState(() =>
    checkoutReturn === "success"
      ? "Stripe returned successfully. Your plan and feature access are being confirmed from Vennusign."
      : "Checkout was canceled. Your current plan and features were not changed."
  );

  useEffect(() => {
    if (!accessToken) return;
    const controller = new AbortController();
    loadBackOfficeSession(configuration, accessToken, controller.signal)
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
        if (accessToken !== customerSessionAccess) {
          sessionStorage.removeItem(tokenStorageKey);
        }
        setAccessToken("");
        setSession(undefined);
        setBilling(undefined);
        setError(reason instanceof BackOfficeApiError ? reason.message : "The venue workspace is unavailable.");
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
            loadBackOfficeSession(configuration, accessToken, controller.signal),
            loadVenueBillingPresentation(configuration, accessToken, controller.signal)
          ]);
          setSession(nextSession);
          setBilling(nextBilling);
        }
        setCheckoutReturnNotice("Your current plan and feature access were refreshed from Vennusign. Stripe webhooks remain authoritative if processing is still finishing.");
      } catch (reason: unknown) {
        if (reason instanceof DOMException && reason.name === "AbortError") return;
        setCheckoutReturnNotice("Stripe returned successfully, but Vennusign could not refresh your plan yet. No access was changed from the return URL; refresh this page shortly.");
      }
    };
    void refreshAuthoritativeState();
    return () => controller.abort();
  }, [accessToken, checkoutReturn, configuration]);

  useEffect(() => {
    // Choosing a destination must dismiss the drawer, or the selected page stays hidden behind it.
    const onHashChange = () => { setRoute(resolveBackOfficeRoute(window.location.hash)); setNavigationOpen(false); };
    window.addEventListener("hashchange", onHashChange);
    return () => window.removeEventListener("hashchange", onHashChange);
  }, []);

  useEffect(() => {
    if (!upgradeContext) setCheckoutError(undefined);
  }, [upgradeContext]);

  useEffect(() => {
    if (!pendingTier || !billing) return;
    const resolution = resolvePendingTierDecision(pendingTier, billing.currentTier?.id);
    if (resolution === "applied") {
      clearPendingTierDecision();
      setPendingTier(undefined);
      setPendingTierNotice(`${pendingTier.targetTierName} is now authoritative in Vennusign.`);
    } else if (resolution === "stale") {
      setPendingTierNotice(`The ${pendingTier.targetTierName} request is still not confirmed. Refresh from Vennusign or reopen Stripe; no access is inferred from the earlier return.`);
    } else {
      setPendingTierNotice(`Waiting for Stripe webhook confirmation of ${pendingTier.targetTierName}. Your current Vennusign entitlements remain active meanwhile.`);
    }
  }, [billing, pendingTier]);

  const authorize = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const token = String(new FormData(event.currentTarget).get("accessToken") ?? "").trim();
    if (!token) return;
    sessionStorage.setItem(tokenStorageKey, token);
    setError(undefined);
    setAccessToken(token);
  };

  const signOut = () => {
    if (accessToken === customerSessionAccess) void revokeCustomerSession(configuration).catch(() => undefined);
    sessionStorage.removeItem(tokenStorageKey);
    setAccessToken("");
    setSession(undefined);
    setBilling(undefined);
    setUpgradeContext(undefined);
    setCheckoutLaunching(false);
    setCheckoutError(undefined);
    setBillingPortalOpening(false);
    setBillingPortalError(undefined);
    setTierDecision(undefined);
    setTierDecisionError(undefined);
    setPendingTier(undefined);
    setPendingTierNotice(undefined);
    clearPendingTierDecision();
    setHaasOpening(undefined);
    setHaasError(undefined);
    clearBackOfficeVenueContext();
  };

  if (!accessToken || error) {
    return <main className="centered"><section className="access-card venue-access-choice" aria-labelledby="venue-access-heading">
        <span>Vennusign Back Office</span>
        <h1 id="venue-access-heading">Open your venue</h1>
        <p role={error ? "alert" : undefined}>{error ?? "Sign in with your Vennusign customer account to continue."}</p>
        <a className="customer-sign-in" href="/signin?returnPath=/">Sign in with your customer account</a>
        {error ? <a className="customer-recovery" href="/onboarding">Set up or recover an organization and venue</a> : null}
        <details><summary>Use configured venue access</summary><form onSubmit={authorize}>
          <p>Use a configured access token supplied for this venue. Customer sign-in remains the standard entry.</p>
          <label htmlFor="accessToken">Configured venue access token</label>
          <input id="accessToken" name="accessToken" type="password" autoComplete="current-password" required />
          <button type="submit">Open venue</button>
        </form></details>
      </section>
    </main>;
  }

  if (!session) {
    return <main className="centered"><p className="loading">Opening your venue…</p></main>;
  }

  const allowed = canOpenBackOfficeRoute(route, session.capabilityDecisions);
  const routeDecision = decisionForBackOfficeRoute(route, session.capabilityDecisions);
  const allowedCapabilityIds = session.capabilityDecisions
    .filter(decision => decision.decision === "allowed" || decision.decision === "allowed-with-conditions")
    .map(decision => decision.capabilityId);
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
  const lockedOpportunity = !allowed && routeDecision?.resolution === "review_product_access"
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
    const usesPortal = Boolean(billing?.subscription?.canManageBilling);
    setUpgradeNotice(`Opening secure ${usesPortal ? "Stripe review" : "checkout"} for ${upgradeContext.title}${usesPortal ? "" : ` with ${interval} billing`}…`);
    const pending = writePendingTierDecision(targetTier);
    setPendingTier(pending);
    try {
      const hostedUrl = usesPortal
        ? await createTierBillingPortalSession(configuration, accessToken, targetTier.id)
        : await createCheckoutSession(configuration, accessToken, targetTier.id, interval);
      window.location.assign(hostedUrl);
    } catch (reason: unknown) {
      clearPendingTierDecision();
      setPendingTier(undefined);
      const message = reason instanceof BackOfficeApiError
        ? reason.message
        : "Secure billing review could not be opened.";
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
      setBillingPortalError(reason instanceof BackOfficeApiError
        ? reason.message
        : "Secure billing management could not be opened.");
      setBillingPortalOpening(false);
    }
  };
  const refreshBillingDecision = async () => {
    try {
      setBilling(await loadVenueBillingPresentation(configuration, accessToken));
    } catch { setPendingTierNotice("Vennusign could not refresh billing state. Your existing entitlements remain unchanged."); }
  };
  const continueTierDecision = async (interval: BillingInterval) => {
    if (!tierDecision || tierDecisionOpening) return;
    setTierDecisionOpening(true);
    setTierDecisionError(undefined);
    const pending = writePendingTierDecision(tierDecision);
    setPendingTier(pending);
    try {
      const url = billing?.subscription?.canManageBilling
        ? await createTierBillingPortalSession(configuration, accessToken, tierDecision.id)
        : await createCheckoutSession(configuration, accessToken, tierDecision.id, interval);
      window.location.assign(url);
    } catch (reason: unknown) {
      clearPendingTierDecision();
      setPendingTier(undefined);
      setTierDecisionError(reason instanceof BackOfficeApiError ? reason.message : "Secure plan review could not be opened.");
      setTierDecisionOpening(false);
    }
  };
  const startHaasCheckout = async (bundle: NonNullable<typeof billing>["haasBundles"][number]) => {
    if (haasOpening) return;
    setHaasOpening(bundle.key);
    setHaasError(undefined);
    try {
      const checkoutUrl = await createHaasCheckoutSession(
        configuration,
        accessToken,
        bundle.key,
        bundle.termMonths);
      window.location.assign(checkoutUrl);
    } catch (reason: unknown) {
      setHaasError(reason instanceof BackOfficeApiError
        ? reason.message
        : "Hardware bundle Checkout could not be opened.");
      setHaasOpening(undefined);
    }
  };
  const switchVenue = async (venueId: string) => {
    if (venueId === session.venueId || contextSwitching) return;
    const destination = session.contexts.find(context => context.venueId === venueId);
    if (!destination) {
      setContextNotice("That venue is no longer available in this account.");
      return;
    }
    const confirmed = await review({
      title: `Switch to ${destination.venueName}?`,
      consequence: `The active workspace will change to ${destination.organizationName} — ${destination.venueName}. Save unfinished edits before continuing. Account permissions will be rechecked by the server.`,
      confirmLabel: "Switch workspace",
      tone: "caution"
    });
    if (!confirmed) return;

    setContextSwitching(true);
    setContextNotice(`Checking access to ${destination.venueName}…`);
    try {
      const nextSession = await selectBackOfficeVenue(configuration, accessToken, venueId);
      setSession(nextSession);
      setBilling(undefined);
      clearPendingTierDecision();
      setPendingTier(undefined);
      setPendingTierNotice(undefined);
      setTierDecision(undefined);
      setTierDecisionError(undefined);
      setContextNotice(`Now working in ${nextSession.organizationName} — ${nextSession.venueName}.`);
      try {
        setBilling(await loadVenueBillingPresentation(configuration, accessToken));
      } catch {
        setContextNotice(`Now working in ${nextSession.organizationName} — ${nextSession.venueName}. Billing details are temporarily unavailable.`);
      }
    } catch (reason: unknown) {
      setContextNotice(reason instanceof BackOfficeApiError
        ? reason.message
        : "Vennusign could not switch venue workspaces.");
    } finally {
      setContextSwitching(false);
    }
  };

  return <div className="shell">
    {reviewDialog}
    {/* Below the sidebar breakpoint the nav collapses behind this control, so page
        content starts at the top of the viewport instead of below a full-height nav. */}
    <button
      type="button"
      className="app-nav-toggle"
      data-testid="nav-toggle"
      aria-expanded={navigationOpen}
      aria-controls="app-sidebar"
      onClick={() => setNavigationOpen(open => !open)}
    >
      <span aria-hidden="true">{navigationOpen ? "✕" : "☰"}</span>
      {navigationOpen ? "Close menu" : "Menu"}
    </button>
    <aside className="app-sidebar" id="app-sidebar" data-open={navigationOpen}>
      <div className="brand"><span>V</span><div><strong>Vennusign</strong><small>Back Office</small></div></div>
      <nav className="grouped-navigation" aria-label="Back Office">
        {backOfficeNavigationGroups.map(group => <details key={group.label} open={group.routes.some(item => item.path === route.path) || group.label === "Operate"}>
          <summary>{group.label}</summary>
          <div>{group.routes.map(item => {
          const unlocked = canOpenBackOfficeRoute(item, session.capabilityDecisions);
          const navigationDecision = decisionForBackOfficeRoute(item, session.capabilityDecisions);
          const opportunity = !unlocked && navigationDecision?.resolution === "review_product_access"
            ? opportunities.find(candidate => candidate.featureKey === item.upgradeFeature)
            : undefined;
          return opportunity ? <LockedNavigationItem
            key={item.path}
            opportunity={opportunity}
            onUpgrade={setUpgradeContext}
            route={item.path}
          /> : <a
            className={`${route.path === item.path ? "active " : ""}${unlocked ? "" : "locked"}`.trim()}
            href={`#/${item.path}`}
            key={item.path}
            data-testid="nav-item"
            data-route={item.path}
            data-unlocked={unlocked}
            data-active={route.path === item.path}
            aria-disabled={!unlocked}
            title={unlocked ? undefined : navigationDecision?.message}
          >
            <strong>{item.label}{unlocked ? "" : " · Locked"}</strong>
            <small>{item.description}</small>
          </a>;
        })}</div></details>)}
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
      <section className="workspace-context" aria-labelledby="workspace-context-heading">
        <div className="workspace-context__active">
          <p id="workspace-context-heading">Active workspace</p>
          <strong title={`${session.organizationName} — ${session.venueName}`}>{session.organizationName}</strong>
          <span>{session.venueName}</span>
        </div>
        <div className="workspace-context__controls">
          {session.contexts.length > 1 ? <label htmlFor="workspace-context-select">
            Organization and venue
            <select
              id="workspace-context-select"
              value={session.venueId}
              disabled={contextSwitching}
              onChange={event => void switchVenue(event.currentTarget.value)}
            >
              {session.contexts.map(context => <option key={context.venueId} value={context.venueId}>
                {context.organizationName} — {context.venueName}
              </option>)}
            </select>
          </label> : <span className="workspace-context__single">Only authorized workspace</span>}
          <div className="workspace-context__account">
            <small>Signed in as</small>
            <strong>{session.account.displayName}</strong>
            {session.account.email ? <span>{session.account.email}</span> : null}
          </div>
        </div>
      </section>
      {contextNotice ? <p className="workspace-context__notice" role="status" aria-live="polite">{contextNotice}</p> : null}
      {checkoutReturn ? <section className={`checkout-return checkout-return--${checkoutReturn}`} role="status">
        <div>
          <strong>{checkoutReturn === "success" ? "Confirming your plan" : "Checkout canceled"}</strong>
          <p>{checkoutReturnNotice}</p>
        </div>
        <button type="button" onClick={dismissCheckoutReturn}>Dismiss</button>
      </section> : null}
      {pendingTierNotice ? <section className="billing-pending" role="status" aria-live="polite"><div><strong>Plan confirmation</strong><p>{pendingTierNotice}</p></div><button type="button" onClick={() => void refreshBillingDecision()}>Refresh authoritative state</button></section> : null}
      {inlineOpportunity && !upgradeContext
        ? <InlineFeatureHint
            key={`${inlineOpportunity.featureKey}-${dismissalVersion}`}
            opportunity={inlineOpportunity}
            onDismiss={dismiss}
            onUpgrade={setUpgradeContext}
          />
        : null}
      {allowed && route.path === "home"
        ? <DaypartHome
            key={session.venueId}
            configuration={configuration}
            accessToken={accessToken}
            venueId={session.venueId}
            venueName={session.venueName}
            capabilities={allowedCapabilityIds}
          />
        : allowed && route.path === "billing" && billing
        ? <BillingStatusCard
            currentTier={billing.currentTier}
            subscription={billing.subscription}
            isOpening={billingPortalOpening}
            error={billingPortalError}
            onManage={openBillingPortal}
            usage={billing.usage}
            availableTiers={billing.availableTiers}
            onSelectTier={tier => { setTierDecision(tier); setTierDecisionError(undefined); }}
            haasBundles={billing.haasBundles}
            haasContract={billing.haasContract}
            haasOpening={haasOpening}
            haasError={haasError}
            onStartHaas={startHaasCheckout}
          />
        : allowed && route.path === "security"
        ? <AccountSecurity configuration={configuration} customerSession={accessToken === customerSessionAccess} />
        : allowed && route.path === "pos"
        ? <PosIntegrationAdministration key={session.venueId} configuration={configuration} accessToken={accessToken} />
        : allowed && route.path === "menu"
        ? <MenuSectionsEditor
            key={session.venueId}
            configuration={configuration}
            apiKey={accessToken}
            venueId={session.venueId}
            starterMenu={starterMenu}
          />
        : allowed && ["screens", "themes", "schedules", "tap-list"].includes(route.path)
        ? <VenueOperations
            key={session.venueId}
            configuration={configuration}
            accessToken={accessToken}
            venueId={session.venueId}
            capabilities={allowedCapabilityIds}
            decisions={session.capabilityDecisions}
            area={route.path as "screens" | "themes" | "schedules" | "tap-list"}
          />
        : allowed
        ? <section className="placeholder"><p>Foundation ready</p><h2>{route.label}</h2><span>This protected venue-scoped area is ready for its planned product package.</span></section>
        : lockedOpportunity
        ? <LockedSectionPreview
            key={`${lockedOpportunity.featureKey}-${dismissalVersion}`}
            opportunity={lockedOpportunity}
            configuration={configuration}
            accessToken={accessToken}
            venueId={session.venueId}
            venueName={session.venueName}
            onDismiss={dismiss}
            onUpgrade={setUpgradeContext}
          />
        : <section className="placeholder locked-panel" role="status" data-testid="locked-panel" data-decision={routeDecision?.decision} data-category={routeDecision?.category} data-route={route.path}><p>{routeDecision?.decision === "temporarily-blocked" ? "Temporarily unavailable" : routeDecision?.category === "permission" ? "Permission required" : "Unavailable"}</p><h2>{route.label} is not available</h2><span>{routeDecision?.message ?? "Vennusign could not verify access to this area. Refresh the session and try again."}</span>{routeDecision?.resolution === "sign_in_again" ? <button type="button" onClick={signOut}>Sign in again</button> : null}</section>}
    </main>
    {upgradeContext && billing && targetTier ? <UpgradeSheet
      opportunity={upgradeContext}
      currentTier={billing.currentTier}
      targetTier={targetTier}
      onClose={() => setUpgradeContext(undefined)}
      onUpgrade={continueUpgrade}
      isSubmitting={checkoutLaunching}
      error={checkoutError}
    /> : null}
    {tierDecision && billing ? <TierDecisionDialog
      currentTier={billing.currentTier}
      targetTier={tierDecision}
      usage={billing.usage}
      usesPortal={Boolean(billing.subscription?.canManageBilling)}
      isSubmitting={tierDecisionOpening}
      error={tierDecisionError}
      onClose={() => { if (!tierDecisionOpening) { setTierDecision(undefined); setTierDecisionError(undefined); } }}
      onConfirm={continueTierDecision}
    /> : null}
  </div>;
}
