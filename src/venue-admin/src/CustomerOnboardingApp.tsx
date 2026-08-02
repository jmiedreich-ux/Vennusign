import { useEffect, useMemo, useState, type FormEvent } from "react";
import { loadVenueAdminConfiguration } from "./config";
import {
  createOnboardingCheckout,
  createOnboardingOrganization,
  externalSignInUrl,
  loadCustomerOnboarding,
  loadCustomerSession,
  loadPublicPlans,
  requestEmailLink,
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
    </section> : <section className="customer-onboarding" aria-labelledby="onboarding-heading">
      <div className="customer-onboarding__welcome">
        <span>Welcome, {session.displayName}</span>
        <h1 id="onboarding-heading">Your opening checklist</h1>
        <p>Progress saves automatically. Entitlement always comes from Vennu’s verified subscription state.</p>
      </div>
      <ol className="customer-onboarding__steps" aria-label="Onboarding progress">
        {steps.map(([key, routeKey, label], index) => <li key={key} className={onboarding?.progress[key] ? "complete" : onboarding?.currentStep === routeKey ? "current" : ""}>
          <span>{onboarding?.progress[key] ? "✓" : index + 1}</span><strong>{label}</strong>
        </li>)}
      </ol>

      {!onboarding?.organizationId ? <form className="customer-onboarding__panel" onSubmit={createOrganization}>
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
      </section> : <section className="customer-onboarding__panel">
        <span>Venue</span><h2>Your plan is ready</h2>
        <p>{onboarding.entitlementStatus === "trialing" && onboarding.trialEndsAt ? `Trial active through ${new Date(onboarding.trialEndsAt).toLocaleDateString()}.` : "Paid entitlement confirmed."}</p>
        <div className="customer-onboarding__deferred" aria-disabled="true"><strong>Venue setup continues in the next release</strong><p>Your account and plan are saved. Venue details and display pairing belong to WP-13.06.</p></div>
      </section>}
    </section>}
  </main>;
}
