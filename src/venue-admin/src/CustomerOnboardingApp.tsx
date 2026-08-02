import { useEffect, useMemo, useState, type FormEvent } from "react";
import { loadVenueAdminConfiguration } from "./config";
import {
  createOnboardingCheckout,
  createOnboardingOrganization,
  createOnboardingVenue,
  externalSignInUrl,
  loadCustomerOnboarding,
  loadCustomerSession,
  loadPublicPlans,
  requestEmailLink,
  claimOnboardingFirstScreen,
  revokeCustomerSession,
  startOnboardingTrial,
  type CustomerOnboardingSnapshot,
  type CustomerSession,
  type PublicOnboardingPlan
} from "./customerOnboardingApi";
import { signInWithPasskey } from "./passkeySignIn";

const steps = [
  ["account", "account", "Account"],
  ["plan", "plan", "Plan"],
  ["venue", "venue", "Venue"],
  ["firstScreen", "first-screen", "First Screen"],
  ["goLive", "go-live", "Go Live"]
] as const;

export default function CustomerOnboardingApp() {
  const configuration = useMemo(loadVenueAdminConfiguration, []);
  const [plans, setPlans] = useState<PublicOnboardingPlan[]>([]);
  const [session, setSession] = useState<CustomerSession>();
  const [onboarding, setOnboarding] = useState<CustomerOnboardingSnapshot>();
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState<string>();
  const [error, setError] = useState<string>();
  const [notice, setNotice] = useState<string>();
  const detectedTimezone = useMemo(() => Intl.DateTimeFormat().resolvedOptions().timeZone || "UTC", []);

  useEffect(() => {
    const controller = new AbortController();
    Promise.all([
      loadPublicPlans(configuration, controller.signal),
      loadCustomerSession(configuration, controller.signal).catch(() => undefined)
    ]).then(async ([availablePlans, activeSession]) => {
      setPlans(availablePlans);
      setSession(activeSession);
      if (activeSession) setOnboarding(await loadCustomerOnboarding(configuration, controller.signal));
      const returned = new URLSearchParams(window.location.search).get("checkout");
      if (returned === "success") setNotice("Stripe returned successfully. Your plan will complete only after Vennu receives the verified webhook.");
      if (returned === "canceled") setNotice("Checkout was canceled. Your onboarding progress is saved and no entitlement was changed.");
    }).catch(reason => {
      if (!(reason instanceof DOMException && reason.name === "AbortError")) setError("Vennu signup is temporarily unavailable.");
    }).finally(() => setLoading(false));
    return () => controller.abort();
  }, [configuration]);

  const run = async (key: string, action: () => Promise<void>) => {
    setBusy(key); setError(undefined); setNotice(undefined);
    try { await action(); }
    catch (reason) { setError(reason instanceof Error ? reason.message : "Vennu could not complete that request."); }
    finally { setBusy(undefined); }
  };

  const sendEmail = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const email = String(new FormData(event.currentTarget).get("email") ?? "").trim();
    void run("email", async () => {
      await requestEmailLink(configuration, email);
      setNotice("If that verified account exists, a secure sign-in link is on its way.");
    });
  };

  const usePasskey = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const email = String(new FormData(event.currentTarget).get("passkeyEmail") ?? "").trim();
    void run("passkey", async () => {
      await signInWithPasskey(configuration, email);
      const activeSession = await loadCustomerSession(configuration);
      setSession(activeSession);
      setOnboarding(await loadCustomerOnboarding(configuration));
    });
  };

  const createOrganization = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const name = String(new FormData(event.currentTarget).get("organizationName") ?? "").trim();
    void run("organization", async () => {
      setOnboarding(await createOnboardingOrganization(configuration, name));
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
    const code = String(new FormData(event.currentTarget).get("pairingCode") ?? "").trim();
    void run("pairing", async () => {
      setOnboarding(await claimOnboardingFirstScreen(configuration, code));
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
      <a className="customer-entry__brand" href="/">Vennu</a>
      {session ? <button className="customer-entry__signout" type="button" onClick={signOut} disabled={busy === "signout"}>Sign out</button> : null}
    </header>
    {notice ? <p className="customer-entry__notice" role="status">{notice}</p> : null}
    {error ? <p className="customer-entry__error" role="alert">{error}</p> : null}

    {!session ? <section className="customer-entry__auth" aria-labelledby="signup-heading">
      <div className="customer-entry__intro">
        <span>Digital menus, without the friction</span>
        <h1 id="signup-heading">Put your first screen live.</h1>
        <p>Sign in first. Then name your organization, choose a plan, and resume at any time.</p>
        <ol><li>Secure account</li><li>Tier-defined trial or paid plan</li><li>Venue and first screen</li></ol>
        <section className="customer-entry__plan-preview" aria-labelledby="available-plans-heading">
          <h2 id="available-plans-heading">Available plans</h2>
          {plans.length ? <ul>{plans.map(plan => <li key={plan.id}>
            <strong>{plan.name}</strong>
            <span>${plan.monthlyPrice} / month</span>
            {plan.trialDays > 0 ? <span>{plan.trialDays}-day trial available</span> : null}
          </li>)}</ul> : <p>No public plans are available right now. You can still sign in and return later.</p>}
        </section>
      </div>
      <div className="customer-entry__auth-card">
        <h2>Start with your account</h2>
        <p>No password to remember.</p>
        <a className="customer-entry__provider" href={externalSignInUrl(configuration, "google")}>Continue with Google</a>
        <a className="customer-entry__provider customer-entry__provider--dark" href={externalSignInUrl(configuration, "apple")}>Continue with Apple</a>
        <div className="customer-entry__divider"><span>Returning customers</span></div>
        <form onSubmit={usePasskey}>
          <label htmlFor="passkeyEmail">Email for your passkey</label>
          <input id="passkeyEmail" name="passkeyEmail" type="email" autoComplete="username webauthn" required />
          <button type="submit" disabled={busy === "passkey"}>{busy === "passkey" ? "Checking passkey…" : "Use a passkey"}</button>
        </form>
        <form onSubmit={sendEmail}>
          <label htmlFor="email">Verified account email</label>
          <input id="email" name="email" type="email" autoComplete="email" required />
          <button className="quiet" type="submit" disabled={busy === "email"}>{busy === "email" ? "Sending…" : "Email me a sign-in link"}</button>
        </form>
      </div>
    </section> : !onboarding ? <section className="customer-onboarding customer-onboarding__panel" aria-labelledby="onboarding-unavailable-heading">
      <span>Progress unavailable</span>
      <h1 id="onboarding-unavailable-heading">We could not safely load your onboarding yet.</h1>
      <p>Your saved progress has not been changed. Refresh to try again before creating or selecting anything.</p>
      <button type="button" onClick={() => window.location.reload()}>Refresh onboarding</button>
    </section> : <section className="customer-onboarding" aria-labelledby="onboarding-heading">
      <div className="customer-onboarding__welcome">
        <span>Welcome, {session.displayName}</span>
        <h1 id="onboarding-heading">Your opening checklist</h1>
        <p>Progress saves automatically. Entitlement always comes from Vennu’s verified subscription state.</p>
      </div>
      <ol className="customer-onboarding__steps" aria-label="Onboarding progress">
        {steps.map(([key, routeKey, label], index) => <li key={key} className={onboarding.progress[key] ? "complete" : onboarding.currentStep === routeKey ? "current" : ""}>
          <span>{onboarding.progress[key] ? "✓" : index + 1}</span><strong>{label}</strong>
        </li>)}
      </ol>

      {!onboarding.organizationId ? <form className="customer-onboarding__panel" onSubmit={createOrganization}>
        <span>Account</span><h2>Name your organization</h2>
        <p>This is the billing and ownership home for all of your venues.</p>
        <label htmlFor="organizationName">Organization name</label>
        <input id="organizationName" name="organizationName" maxLength={200} required />
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
      </form> : !onboarding.firstScreenId ? <form className="customer-onboarding__panel" onSubmit={claimFirstScreen}>
        <span>First Screen</span><h2>Pair your physical display</h2>
        <p>Open the Vennu player on the display. The player creates its screen record and shows a six-digit code that expires after 10 minutes.</p>
        <ol className="customer-onboarding__pairing-steps"><li>Start Vennu on the display.</li><li>Wait for its six-digit code.</li><li>Enter that code here to link it to this venue.</li></ol>
        <label htmlFor="pairingCode">Six-digit pairing code</label>
        <input id="pairingCode" name="pairingCode" inputMode="numeric" autoComplete="one-time-code" pattern="[0-9]{6}" minLength={6} maxLength={6} required />
        <button type="submit" disabled={busy === "pairing"}>{busy === "pairing" ? "Pairing display…" : "Pair this display"}</button>
        <p className="customer-onboarding__help">Expired or already used? Return to the display and request a fresh code; your saved venue is unchanged.</p>
      </form> : <section className="customer-onboarding__panel">
        <span>Go Live</span><h2>{onboarding.firstScreenStatus === "online" ? "Your first display is online" : "Your first display is paired"}</h2>
        <p>{onboarding.firstScreenStatus === "online" ? "Vennu received the player heartbeat. This onboarding journey is ready for the next timeline release." : "The screen record is linked, but pairing alone does not mean the device is active. Start the player and keep it connected until it reports Online."}</p>
        <dl className="customer-onboarding__device-status"><div><dt>Pairing</dt><dd>Linked</dd></div><div><dt>Device</dt><dd>{onboarding.firstScreenStatus === "online" ? "Online" : "Offline / waiting"}</dd></div>{onboarding.firstScreenLastSeenUtc ? <div><dt>Last seen</dt><dd>{new Date(onboarding.firstScreenLastSeenUtc).toLocaleString()}</dd></div> : null}</dl>
        <button type="button" onClick={refreshOnboarding} disabled={busy === "refresh"}>{busy === "refresh" ? "Refreshing…" : "Refresh device status"}</button>
      </section>}
    </section>}
  </main>;
}
