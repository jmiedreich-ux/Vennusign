import { useEffect, useMemo, useState, type FormEvent } from "react";
import { loadBackOfficeConfiguration } from "./config";
import {
  createOnboardingCheckout,
  createOnboardingOrganization,
  createOnboardingVenue,
  externalSignInUrl,
  loadCustomerOnboarding,
  loadCustomerSession,
  loadPublicPlans,
  claimOnboardingFirstScreen,
  revokeCustomerSession,
  startOnboardingTrial,
  type CustomerOnboardingSnapshot,
  type CustomerSession,
  type PublicOnboardingPlan
} from "./customerOnboardingApi";
import { signInWithPasskey } from "./passkeySignIn";
import CustomerOnboardingTimeline from "./CustomerOnboardingTimeline";
import TemplateShowcase from "./TemplateShowcase";
import { authenticatedCustomerDestination, canonicalOnboardingPath, safeLocalReturnPath } from "./customerEntryRouting.mjs";

const REMEMBERED_METHOD_KEY = "vennusign.customerAuth.lastMethod";
const KNOWN_METHODS = new Set(["Google", "Vennusign", "Passkey"]);

const GoogleMark = () => <svg width="18" height="18" viewBox="0 0 18 18" aria-hidden="true">
  <path fill="#4285F4" d="M17.64 9.2c0-.64-.06-1.25-.16-1.84H9v3.48h4.84a4.14 4.14 0 0 1-1.8 2.72v2.26h2.9c1.7-1.57 2.68-3.87 2.68-6.62Z"/>
  <path fill="#34A853" d="M9 18c2.43 0 4.47-.8 5.96-2.18l-2.9-2.26c-.8.54-1.84.86-3.06.86-2.35 0-4.34-1.59-5.05-3.72H.96v2.33A9 9 0 0 0 9 18Z"/>
  <path fill="#FBBC05" d="M3.95 10.7A5.4 5.4 0 0 1 3.66 9c0-.59.1-1.17.29-1.7V4.97H.96A9 9 0 0 0 0 9c0 1.45.35 2.83.96 4.03l2.99-2.33Z"/>
  <path fill="#EA4335" d="M9 3.58c1.32 0 2.51.46 3.44 1.35l2.58-2.58C13.46.89 11.43 0 9 0A9 9 0 0 0 .96 4.97l2.99 2.33C4.66 5.17 6.65 3.58 9 3.58Z"/>
</svg>;

function readRememberedMethod(): string | undefined {
  try {
    return localStorage.getItem(REMEMBERED_METHOD_KEY) ?? undefined;
  } catch {
    // Private browsing or storage disabled - fall back to showing every option, same as a first visit.
    return undefined;
  }
}

function rememberMethod(method: string) {
  try {
    localStorage.setItem(REMEMBERED_METHOD_KEY, method);
  } catch {
    // Nothing to persist; next visit just shows every option again.
  }
}

export default function CustomerOnboardingApp() {
  const configuration = useMemo(loadBackOfficeConfiguration, []);
  const entryPath = useMemo(() => window.location.pathname.replace(/\/$/, "") || "/", []);
  const returnPath = useMemo(() => {
    const requested = new URLSearchParams(window.location.search).get("returnPath");
    return safeLocalReturnPath(requested, canonicalOnboardingPath);
  }, []);
  const authenticationReturnPath = useMemo(() => returnPath === canonicalOnboardingPath
    ? canonicalOnboardingPath
    : `${canonicalOnboardingPath}?returnPath=${encodeURIComponent(returnPath)}`, [returnPath]);
  const [plans, setPlans] = useState<PublicOnboardingPlan[]>([]);
  const [session, setSession] = useState<CustomerSession>();
  const [onboarding, setOnboarding] = useState<CustomerOnboardingSnapshot>();
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState<string>();
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const [pairingCode, setPairingCode] = useState("");
  const detectedTimezone = useMemo(() => Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC", []);
  const rememberedMethod = useMemo(readRememberedMethod, []);
  const [showAllMethods, setShowAllMethods] = useState(!rememberedMethod);

  useEffect(() => {
    if (session?.authenticationMethod) rememberMethod(session.authenticationMethod);
  }, [session?.authenticationMethod]);

  useEffect(() => {
    const controller = new AbortController();
    Promise.all([
      loadPublicPlans(configuration, controller.signal),
      loadCustomerSession(configuration, controller.signal).catch(() => undefined)
    ]).then(async ([availablePlans, activeSession]) => {
      setPlans(availablePlans);
      setSession(activeSession);
      if (activeSession) {
        const snapshot = await loadCustomerOnboarding(configuration, controller.signal);
        const destination = authenticatedCustomerDestination(entryPath, returnPath, snapshot);
        if (destination) {
          window.location.replace(destination);
          return;
        }
        setOnboarding(snapshot);
      }
      const returned = new URLSearchParams(window.location.search).get("checkout");
      if (returned === "success") setNotice("Stripe returned successfully. Your plan will complete only after Vennusign receives the verified webhook.");
      if (returned === "canceled") setNotice("Checkout was canceled. Your onboarding progress is saved and no entitlement was changed.");
    }).catch(reason => {
      if (!(reason instanceof DOMException && reason.name === "AbortError")) setError("Vennusign could not safely resolve this account's onboarding journey. Sign in again or contact support; no new journey was created.");
    }).finally(() => setLoading(false));
    return () => controller.abort();
  }, [configuration, entryPath, returnPath]);

  useEffect(() => {
    if (!session || !onboarding?.firstScreenId) return;
    let stopped = false;
    const refreshPresence = async () => {
      try {
        const current = await loadCustomerOnboarding(configuration);
        if (!stopped) setOnboarding(current);
      } catch {
        // Preserve the last authoritative state; the visible manual retry remains available.
      }
    };
    const timer = window.setInterval(() => void refreshPresence(), 10_000);
    const onVisible = () => { if (document.visibilityState === "visible") void refreshPresence(); };
    document.addEventListener("visibilitychange", onVisible);
    return () => { stopped = true; window.clearInterval(timer); document.removeEventListener("visibilitychange", onVisible); };
  }, [configuration, onboarding?.firstScreenId, session]);

  const run = async (key: string, action: () => Promise<void>) => {
    setBusy(key); setError(undefined); setNotice(undefined);
    try { await action(); }
    catch (reason) { setError(reason instanceof Error ? reason.message : "Vennusign could not complete that request."); }
    finally { setBusy(undefined); }
  };

  const usePasskey = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const email = String(new FormData(event.currentTarget).get("passkeyEmail") ?? "").trim();
    void run("passkey", async () => {
      await signInWithPasskey(configuration, email);
      const activeSession = await loadCustomerSession(configuration);
      const snapshot = await loadCustomerOnboarding(configuration);
      const destination = authenticatedCustomerDestination(entryPath, returnPath, snapshot);
      if (destination) {
        window.location.replace(destination);
        return;
      }
      setSession(activeSession);
      setOnboarding(snapshot);
    });
  };

  const createOrganization = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    void run("organization", async () => {
      setOnboarding(await createOnboardingOrganization(configuration, {
        name: String(data.get("organizationName") ?? "").trim(),
        legalName: String(data.get("legalName") ?? "").trim() || undefined,
        primaryContactName: String(data.get("primaryContactName") ?? "").trim(),
        contactEmail: String(data.get("contactEmail") ?? "").trim(),
        contactPhone: String(data.get("contactPhone") ?? "").trim() || undefined,
        mailingAddress: String(data.get("mailingAddress") ?? "").trim()
      }));
      setNotice("Your organization is saved. Choose the plan that fits your venue.");
    });
  };

  const chooseTrial = (plan: PublicOnboardingPlan) => void run(`trial-${plan.id}`, async () => {
    setOnboarding(await startOnboardingTrial(configuration, plan.id));
    setNotice(`${plan.name} trial started. Venue setup is next.`);
  });

  const choosePaid = (plan: PublicOnboardingPlan, interval: "monthly" | "annual") =>
    void run(`${interval}-${plan.id}`, async () => window.location.assign(
      await createOnboardingCheckout(configuration, plan.id, interval)));

  const createVenue = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    void run("venue", async () => {
      setOnboarding(await createOnboardingVenue(configuration, {
        name: String(data.get("venueName") ?? "").trim(),
        timezone: String(data.get("timezone") ?? "").trim(),
        type: String(data.get("venueType") ?? "").trim(),
        primaryLanguage: String(data.get("primaryLanguage") ?? "").trim(),
        secondaryLanguage: String(data.get("secondaryLanguage") ?? "").trim() || undefined
      }));
      setNotice("Venue saved. Now enter the six-digit code shown on your physical display.");
    });
  };

  const claimFirstScreen = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    void run("pairing", async () => {
      setOnboarding(await claimOnboardingFirstScreen(configuration, pairingCode));
      setPairingCode("");
      setNotice("Display paired. It becomes active when the player reports Online.");
    });
  };

  const refreshOnboarding = () => void run("refresh", async () => {
    setOnboarding(await loadCustomerOnboarding(configuration));
    setNotice("Display status refreshed.");
  });

  const signOut = () => void run("signout", async () => {
    await revokeCustomerSession(configuration);
    setSession(undefined); setOnboarding(undefined);
    setNotice("You are signed out. Your onboarding progress remains saved.");
  });

  if (loading) return <main className="customer-entry centered"><p className="loading" role="status">Opening secure signup…</p></main>;

  return <main className="customer-entry">
    <header className="customer-entry__header">
      <a className="customer-entry__brand" href="/">Vennusign</a>
      {session ? <button className="customer-entry__signout" type="button" onClick={signOut} disabled={busy === "signout"}>Sign out</button> : null}
    </header>
    {notice ? <p className="customer-entry__notice" role="status">{notice}</p> : null}
    {error ? <p className="customer-entry__error" role="alert">{error}</p> : null}

    {!session ? <section className="customer-landing">
      <div className="customer-landing__auth" aria-label="Secure account access">
      <div className="customer-entry__auth-card" id="signup-auth-card">
        <h2>Sign in to Vennusign</h2>
        <p>No password to remember.</p>
        {rememberedMethod && KNOWN_METHODS.has(rememberedMethod) && !showAllMethods ? <>
          <p className="customer-entry__remembered-tag">✓ Continue as you did last time</p>
          {rememberedMethod === "Google" ?
            <a className="customer-entry__provider customer-entry__provider--google" href={externalSignInUrl(configuration, "google", authenticationReturnPath)}><GoogleMark />Continue with Google</a>
          : rememberedMethod === "Vennusign" ?
            <a className="customer-entry__provider customer-entry__provider--primary" href={externalSignInUrl(configuration, "vennusign", authenticationReturnPath)}>Continue with Vennusign</a>
          : <form onSubmit={usePasskey}>
            <label htmlFor="passkeyEmail">Email for your passkey</label>
            <input id="passkeyEmail" name="passkeyEmail" type="email" autoComplete="username webauthn" required />
            <button type="submit" disabled={busy === "passkey"}>{busy === "passkey" ? "Checking passkey…" : "Use a passkey"}</button>
          </form>}
          <button className="customer-entry__more-options" type="button" onClick={() => setShowAllMethods(true)}>More ways to sign in</button>
        </> : <>
        <a className="customer-entry__provider customer-entry__provider--primary" href={externalSignInUrl(configuration, "vennusign", authenticationReturnPath)}>Continue with Vennusign</a>
        <a className="customer-entry__provider customer-entry__provider--google" href={externalSignInUrl(configuration, "google", authenticationReturnPath)}><GoogleMark />Continue with Google</a>
        <div className="customer-entry__divider"><span>Or</span></div>
        <form onSubmit={usePasskey}>
          <label htmlFor="passkeyEmail">Email for your passkey</label>
          <input id="passkeyEmail" name="passkeyEmail" type="email" autoComplete="username webauthn" required />
          <button type="submit" disabled={busy === "passkey"}>{busy === "passkey" ? "Checking passkey…" : "Use a passkey"}</button>
        </form>
        </>}
      </div>
      </div>
      <TemplateShowcase />
    </section> : !onboarding ? <section className="customer-onboarding customer-onboarding__panel" aria-labelledby="onboarding-unavailable-heading">
      <span>Progress unavailable</span>
      <h1 id="onboarding-unavailable-heading">We could not safely load your onboarding yet.</h1>
      <p>Your saved progress has not been changed and Vennusign did not create a replacement journey. Refresh, sign in again, or contact support if your organization access changed.</p>
      <button type="button" onClick={() => window.location.reload()}>Refresh onboarding</button>
    </section> : <section className="customer-onboarding" aria-labelledby="onboarding-heading">
      <div className="customer-onboarding__welcome">
        <span>Back Office onboarding · Welcome, {session.displayName}</span>
        <h1 id="onboarding-heading">Your opening checklist</h1>
        <p>Progress saves automatically. Entitlement always comes from Vennusign’s verified subscription state.</p>
      </div>
      <CustomerOnboardingTimeline onboarding={onboarding} />

      <div id="onboarding-current-task" tabIndex={-1}>{!onboarding.organizationId ? <form className="customer-onboarding__panel" onSubmit={createOrganization}>
        <span>Account</span><h2>Name your organization</h2>
        <p>This is the billing and ownership home for all of your venues.</p>
        <label htmlFor="organizationName">Organization name</label>
        <input id="organizationName" name="organizationName" maxLength={200} autoComplete="organization" required />
        <label htmlFor="legalName">Legal business name (optional)</label>
        <input id="legalName" name="legalName" maxLength={200} autoComplete="organization" />
        <label htmlFor="primaryContactName">Primary contact name</label>
        <input id="primaryContactName" name="primaryContactName" maxLength={200} autoComplete="name" defaultValue={session.displayName} required />
        <label htmlFor="contactEmail">Contact email</label>
        <input id="contactEmail" name="contactEmail" type="email" maxLength={320} autoComplete="email" defaultValue={session.email} required />
        <label htmlFor="contactPhone">Contact phone (optional)</label>
        <input id="contactPhone" name="contactPhone" type="tel" maxLength={50} autoComplete="tel" />
        <label htmlFor="mailingAddress">Business mailing address</label>
        <textarea id="mailingAddress" name="mailingAddress" maxLength={500} autoComplete="street-address" rows={4} required />
        <p className="customer-onboarding__help">Used for organization ownership, billing contact, and support. It is shown only in authorized customer or support contexts.</p>
        <button type="submit" disabled={busy === "organization"}>{busy === "organization" ? "Saving…" : "Save and choose a plan"}</button>
      </form> : !onboarding.progress.plan ? <section className="customer-onboarding__panel">
        <span>Plan</span><h2>Choose how to begin</h2>
        {onboarding.checkoutPending ? <p className="customer-entry__notice" role="status">Checkout is awaiting verified subscription confirmation. You can safely retry if you did not finish.</p> : null}
        <div className="customer-onboarding__plans">
          {plans.map(plan => <article key={plan.id} className={onboarding.selectedTierId === plan.id ? "selected" : ""}>
            <h3>{plan.name}</h3><p><strong>${plan.monthlyPrice}</strong> / month</p>
            <ul><li>{plan.maxVenues === -1 ? "Unlimited venues" : `${plan.maxVenues} venue${plan.maxVenues === 1 ? "" : "s"}`}</li><li>{plan.maxScreens === -1 ? "Unlimited screens" : `${plan.maxScreens} screens`}</li></ul>
            {plan.trialDays > 0 ? <button type="button" onClick={() => chooseTrial(plan)} disabled={Boolean(busy)}>{busy === `trial-${plan.id}` ? "Starting…" : `Try ${plan.trialDays} days free`}</button> : null}
            {plan.monthlyCheckoutAvailable ? <button className="quiet" type="button" onClick={() => choosePaid(plan, "monthly")} disabled={Boolean(busy)}>Pay monthly</button> : null}
            {plan.annualCheckoutAvailable ? <button className="quiet" type="button" onClick={() => choosePaid(plan, "annual")} disabled={Boolean(busy)}>Pay annually</button> : null}
          </article>)}
        </div>
      </section> : !onboarding.venueId ? <form className="customer-onboarding__panel" onSubmit={createVenue}>
        <span>Venue</span><h2>Set up your first venue</h2>
        <p>{onboarding.entitlementStatus === "trialing" && onboarding.trialEndsAt ? `Trial active through ${new Date(onboarding.trialEndsAt).toLocaleDateString()}.` : "Paid entitlement confirmed."} These details control schedules and language defaults.</p>
        <label htmlFor="venueName">Venue name</label>
        <input id="venueName" name="venueName" maxLength={200} autoComplete="organization" required />
        <label htmlFor="timezone">IANA timezone</label>
        <input id="timezone" name="timezone" list="onboarding-timezones" defaultValue={detectedTimezone} maxLength={100} required />
        <datalist id="onboarding-timezones">
          {[detectedTimezone, "America/New_York", "America/Chicago", "America/Denver", "America/Los_Angeles", "UTC"].filter((item, index, values) => values.indexOf(item) === index).map(timezone => <option key={timezone} value={timezone} />)}
        </datalist>
        <label htmlFor="venueType">Venue type</label>
        <select id="venueType" name="venueType" required defaultValue="">
          <option value="" disabled>Select a venue type</option>
          <option value="restaurant">Restaurant</option><option value="bar">Bar</option><option value="brewery">Brewery</option><option value="cafe">Cafe</option><option value="retail">Retail</option><option value="other">Other</option>
        </select>
        <div className="customer-onboarding__language-grid">
          <label htmlFor="primaryLanguage">Primary language code<input id="primaryLanguage" name="primaryLanguage" defaultValue="en" pattern="[A-Za-z]{2}" maxLength={2} required /></label>
          <label htmlFor="secondaryLanguage">Secondary language code (optional)<input id="secondaryLanguage" name="secondaryLanguage" pattern="[A-Za-z]{2}" maxLength={2} /></label>
        </div>
        <button type="submit" disabled={busy === "venue"}>{busy === "venue" ? "Saving venue…" : "Save venue and continue"}</button>
      </form> : !onboarding.firstScreenId ? <form className="customer-onboarding__panel customer-onboarding__pairing" onSubmit={claimFirstScreen}>
        <span>First Screen</span><h2>Pair your physical display</h2>
        <p>Open the Vennusign player on the display. The player creates its screen record and shows a six-digit code that expires after 10 minutes.</p>
        <ol className="customer-onboarding__pairing-steps"><li>Start Vennusign on the display.</li><li>Wait for its six-digit code.</li><li>Enter that code here to link it to this venue.</li></ol>
        <label htmlFor="pairingCode">Six-digit pairing code</label>
        <input id="pairingCode" className="customer-onboarding__pairing-code" name="pairingCode" inputMode="numeric" autoComplete="one-time-code" pattern="[0-9]{6}" minLength={6} maxLength={6} required aria-describedby="pairing-code-progress pairing-code-help" value={pairingCode} onChange={event => setPairingCode(event.target.value.replace(/\D/g, "").slice(0, 6))} placeholder="000000" />
        <p id="pairing-code-progress" className="customer-onboarding__pairing-progress" role="status">{pairingCode.length === 6 ? "Code ready to pair" : `${pairingCode.length} of 6 digits entered`}</p>
        <button type="submit" disabled={busy === "pairing" || pairingCode.length !== 6}>{busy === "pairing" ? "Pairing display…" : "Pair this display"}</button>
        <p id="pairing-code-help" className="customer-onboarding__help">Expired or already used? Return to the display and request a fresh code; your saved venue is unchanged.</p>
      </form> : <section className={`customer-onboarding__panel customer-onboarding__go-live ${onboarding.firstScreenStatus === "online" ? "is-online" : "is-waiting"}`}>
        {onboarding.firstScreenStatus === "online" ? <div className="customer-onboarding__celebration" role="status"><span aria-hidden="true">✓</span><strong>You’re live</strong><small>Confirmed by the player heartbeat</small></div> : null}
        <span>Go Live</span><h2>{onboarding.firstScreenStatus === "online" ? "Your first display is online" : "Your first display is paired"}</h2>
        <p>{onboarding.firstScreenStatus === "online" ? "Vennusign received the player heartbeat. Status continues to update automatically." : "The screen record is linked, but pairing alone does not mean the device is active. Start the player and keep it connected; this status updates automatically when its heartbeat arrives."}</p>
        <dl className="customer-onboarding__device-status"><div><dt>Pairing</dt><dd>Linked</dd></div><div><dt>Device</dt><dd>{onboarding.firstScreenStatus === "online" ? "Online" : "Offline / waiting"}</dd></div>{onboarding.firstScreenLastSeenUtc ? <div><dt>Last seen</dt><dd>{new Date(onboarding.firstScreenLastSeenUtc).toLocaleString()}</dd></div> : null}</dl>
        {onboarding.firstScreenStatus === "online" ? <>
          <section className="customer-onboarding__starter-menus" aria-labelledby="starter-menus-heading"><div><span>Optional starting point</span><h3 id="starter-menus-heading">Choose a starter menu</h3><p>Open a named draft starting point. Nothing is created until you review and submit it in Back Office.</p></div><div>
            <a href="/?starterMenu=restaurant#/menu"><strong>Restaurant</strong><span>Lunch & dinner menu</span></a>
            <a href="/?starterMenu=cafe#/menu"><strong>Cafe</strong><span>Drinks & counter menu</span></a>
            <a href="/?starterMenu=bar#/menu"><strong>Bar or brewery</strong><span>Drinks & tap list</span></a>
            <a href="/#/menu"><strong>Start blank</strong><span>Build your own structure</span></a>
          </div></section>
          <section className="customer-onboarding__next-steps" aria-labelledby="first-run-next-heading"><h3 id="first-run-next-heading">Your first-run checklist</h3><ol><li><span>1</span><a href="/#/menu">Add and review menu content</a></li><li><span>2</span><a href="/#/themes">Apply your venue theme</a></li><li><span>3</span><a href="/#/schedules">Set service dayparts</a></li><li><span>4</span><a href="/#/screens">Preview and push to the display</a></li></ol></section>
        </> : null}
        <div className="customer-onboarding__completion-actions"><a href="/">Open Back Office</a><button className="quiet" type="button" onClick={refreshOnboarding} disabled={busy === "refresh"}>{busy === "refresh" ? "Refreshing…" : "Refresh device status"}</button></div>
        <p className="customer-onboarding__help">Back Office rechecks your organization membership and saved venue before it opens. A paired but offline display does not block venue setup.</p>
      </section>}</div>
    </section>}
  </main>;
}
