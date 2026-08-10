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
import NavRail from "./NavRail";
import MenuSectionsEditor from "./MenuSectionsEditor";
import MenusHome from "./MenusHome";
import PosIntegrationAdministration from "./PosIntegrationAdministration";
import VenueOperations from "./VenueOperations";
import InlineFeatureHint from "./InlineFeatureHint";
import LockedNavigationItem from "./LockedNavigationItem";
import LockedSectionPreview from "./LockedSectionPreview";
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

/**
 * Accepts a configured venue-access token from the URL *fragment* so the owner
 * acceptance workbook can offer a one-click sign-in for each role.
 *
 * The fragment is deliberate: browsers never transmit it, so the token cannot reach
 * server, proxy or CDN request logs. A query string would be sent with the very first
 * request, and stripping it afterwards in script would already be too late.
 *
 * Expected shape is `#/route?accessToken=...`. The token is moved into session storage
 * and the fragment rewritten to the bare route before anything renders, so nothing is
 * left in the address bar, in history, or in a copied link. Only tokens the API already
 * has configured are accepted, so this grants no new access.
 */
function consumeAccessTokenFromUrl(): string | undefined {
  const fragment = window.location.hash.replace(/^#/, "");
  const separator = fragment.indexOf("?");
  if (separator < 0) return undefined;

  const parameters = new URLSearchParams(fragment.slice(separator + 1));
  const supplied = parameters.get("accessToken");
  if (!supplied) return undefined;

  parameters.delete("accessToken");
  const remaining = parameters.toString();
  const route = fragment.slice(0, separator);
  window.history.replaceState(
    null,
    "",
    `${window.location.pathname}${window.location.search}#${route}${remaining ? `?${remaining}` : ""}`
  );
  sessionStorage.setItem(tokenStorageKey, supplied);
  return supplied;
}

export default function App() {
  const configuration = useMemo(loadBackOfficeConfiguration, []);
  const starterMenu = useMemo(() => {
    const value = new URLSearchParams(window.location.search).get("starterMenu");
    return value && ["restaurant", "cafe", "bar"].includes(value) ? value as "restaurant" | "cafe" | "bar" : undefined;
  }, []);
  /**
   * Null means the shelf; anything else means the editor is open.
   *
   * Interim, and deliberately not a route: milestone 3 replaces the editor with
   * the builder and gives it its own address. Inventing a URL for a surface
   * about to be replaced would leave a dead link behind (Q100).
   */
  const [editingMenuId, setEditingMenuId] = useState<string | null>(null);
  const [accessToken, setAccessToken] = useState(() =>
    consumeAccessTokenFromUrl() ?? sessionStorage.getItem(tokenStorageKey) ?? customerSessionAccess);
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
  const inlineOpportunity = allowed && route.path !== "menu"
    ? opportunities.find(item => upgradePanelForFeature(item.featureKey) === routePanel)
    : undefined;
  const lockedOpportunity = !allowed && routeDecision?.resolution === "review_product_access"
    ? opportunities.find(item => item.featureKey === route.upgradeFeature) ?? opportunities[0]
    : undefined;
  /*
   * No upgrade prompts on Menus.
   *
   * Criterion 8 is one of the three this milestone closes: a capability outside
   * the account's plan renders nothing - no disabled control, no tooltip, no
   * placeholder. An inline card advertising a feature the plan does not include
   * was rendering above the venue name on Menus home, which is exactly the
   * placeholder that criterion forbids and, per the owner, marketing surfaces are
   * scheduled work of their own rather than something this shell carries.
   *
   * Scoped to Menus rather than removed everywhere: the other areas' prompts are
   * Track 1's deliberate upgrade path, and retiring them is that work's decision.
   */
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

  // Menus home draws its own page header (the hi-fi has the shelf owning the
  // page), and only when it is actually the shelf rather than the editor behind
  // a card.
  const showsOwnHeader = route.path === "menu" && editingMenuId === null;

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
    <NavRail
      activePath={route.path}
      decisions={session.capabilityDecisions}
      opportunities={opportunities}
      onUpgrade={setUpgradeContext}
      displayName={session.displayName}
      onSignOut={signOut}
      open={navigationOpen}
    >
      {/*
        The sidebar upgrade nudge has no home in a 76px rail. It was drawn for the
        270px sidebar and spilled out of the rail across the page — a carousel of
        marketing panels with pagination dots, at icon width. Rather than shrink a
        marketing surface into a column it does not fit, it is not rendered here:
        upgrade and marketing surfaces are their own scheduled work (milestone-plan,
        After this build), and that work should decide where this lives in the new
        shell. Named in the handoff rather than quietly dropped.
      */}
      {upgradeNotice ? <p className="sidebar-upgrade-context" role="status">{upgradeNotice}</p> : null}
    </NavRail>
    <main>
      {/* The shelf carries its own header — the venue eyebrow, the status
          headline, and the actions beside it — so the generic page title would
          be a second heading saying "Menu" above it. Every other area keeps the
          shell's, which is what it was built for. */}
      {showsOwnHeader ? null : <header><div><p>Venue workspace</p><h1>{route.label}</h1></div><span>Secure session</span></header>}
      {/* Not `hidden`: the attribute loses to this section's own display rule. */}
      {showsOwnHeader ? null : <section className="workspace-context" aria-labelledby="workspace-context-heading">
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
      </section>}
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
        : allowed && route.path === "menu" && editingMenuId === null
        ? <MenusHome
            key={session.venueId}
            configuration={configuration}
            accessToken={accessToken}
            venueName={session.venueName}
            /* Interim wiring, until milestones 3 and 6 (Q100): a card opens the
               editor that exists, and Add a menu uses the create flow that
               exists. Anything with no destination is absent, never greyed. */
            onOpenMenu={setEditingMenuId}
            onAddMenu={() => setEditingMenuId("")}
            onFixScreens={() => { window.location.hash = "#/screens"; }}
          />
        : allowed && route.path === "menu"
        ? <>
            <button
              type="button"
              className="action-secondary menus-home__back"
              onClick={() => setEditingMenuId(null)}
              data-testid="back-to-menus"
            >
              ← Menus
            </button>
            <MenuSectionsEditor
              key={session.venueId}
              configuration={configuration}
              apiKey={accessToken}
              venueId={session.venueId}
              starterMenu={starterMenu}
            />
          </>
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
