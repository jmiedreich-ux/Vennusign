import { useEffect, useState } from "react";
import { signupUrl, signinUrl } from "./config";
import { venueExamples, boardTagLabel, type BoardPeriod } from "./boardExamples";
import { loadPublicPlans, type PublicOnboardingPlan } from "./plansApi";

const faqs = [
  {
    q: "What is Vennusign?",
    a: "A digital signage platform that knows what matters for your industry — menus for restaurants, offers for retail, schedules for venues. Manage your content in one workspace, then push changes live to every paired screen."
  },
  {
    q: "What hardware do I need?",
    a: "Vennusign runs on its own web player, Android TV, Fire TV, Samsung Tizen, and LG webOS — pair whatever screen you already have with a one-time six-digit code."
  },
  {
    q: "Can I preview a change before it goes live?",
    a: "Yes. Every change is previewed and reviewed before it reaches a screen, and delivery state stays visible so you always know what is actually live."
  },
  {
    q: "What does it cost?",
    a: "Plans are shown below with venue and screen limits. Choose one after you create your account — some plans include a trial period."
  },
  {
    q: "Do I have to finish setup in one sitting?",
    a: "No. Your onboarding progress is saved automatically, so you can leave and pick up exactly where you left off."
  },
  {
    // TODO: replace with a real support address before this ships.
    q: "Can I talk to someone before I start?",
    a: "Yes — email [support email] and we will walk through it with you before you create an account."
  }
] as const;

export default function Home() {
  const [venueId, setVenueId] = useState<(typeof venueExamples)[number]["id"]>("restaurant");
  const venue = venueExamples.find(v => v.id === venueId) ?? venueExamples[0];
  const [periodId, setPeriodId] = useState<string>(venue.periods[0].id);
  const activePeriod: BoardPeriod = venue.periods.find(p => p.id === periodId) ?? venue.periods[0];

  const [plans, setPlans] = useState<PublicOnboardingPlan[]>([]);
  useEffect(() => {
    const controller = new AbortController();
    loadPublicPlans(controller.signal).then(setPlans).catch(() => {});
    return () => controller.abort();
  }, []);

  function selectVenue(id: (typeof venueExamples)[number]["id"]) {
    setVenueId(id);
    const next = venueExamples.find(v => v.id === id) ?? venueExamples[0];
    setPeriodId(next.periods[0].id);
  }

  return <div className="site">
    <header className="site-nav">
      <span className="site-nav__brand">Vennusign</span>
      <nav className="site-nav__links">
        <a href={signinUrl}>Sign in</a>
        <a className="site-nav__signup" href={signupUrl}>Sign up</a>
      </nav>
    </header>

    <main className="site-home">
      <section className="signup-marketing__hero" aria-labelledby="home-heading">
        <div className="signup-marketing__hero-panel">
          <div className="signup-marketing__hero-device" aria-hidden="true">
            <div className="signup-marketing__hero-device-screen">
              <span>VENNU CAFE · Evening</span>
              <strong>Dinner presentation is ready</strong>
              <div className="signup-marketing__hero-device-bars"><i /><i /></div>
            </div>
          </div>
          <span>Digital menus, without the friction</span>
          <h1 id="home-heading">Put your first screen live.</h1>
          <p>See the product before you sign up. Then create your organization, choose an available plan, and pair a display with one secure code.</p>
          <div className="signup-marketing__actions">
            <a className="signup-marketing__primary" href={signupUrl}>Start setup</a>
            <a href="#live-product-demo">Try the live demo</a>
            <a className="signup-marketing__book-demo" href="#home-faq">Book a demo instead</a>
          </div>
          <p className="signup-marketing__no-cc">No credit card required to try it.</p>
        </div>
        <ul className="signup-marketing__proof" aria-label="Product proof points">
          <li><strong>One workspace</strong><span>Menus, screens, schedules, and daily service controls.</span></li>
          <li><strong>Safe changes</strong><span>Preview, review, and visible delivery state before you move on.</span></li>
          <li><strong>Resumable setup</strong><span>Your authoritative onboarding progress is saved between visits.</span></li>
        </ul>
      </section>

      <section className="signup-demo" id="live-product-demo" aria-labelledby="live-demo-heading">
        <header>
          <div><span>Interactive product preview</span><h2 id="live-demo-heading">A day on your menu</h2></div>
          <p>Two venue types, real service periods, and a different board look for each.</p>
        </header>

        <div className="signup-demo__venue-toggle" role="group" aria-label="Choose a venue type">
          {venueExamples.map(v => <button key={v.id} type="button" aria-pressed={v.id === venue.id} onClick={() => selectVenue(v.id)}>{v.label}</button>)}
        </div>

        <div className="signup-demo__tabs" role="group" aria-label="Preview a service period">
          {venue.periods.map(period => <button key={period.id} type="button" aria-pressed={period.id === activePeriod.id} onClick={() => setPeriodId(period.id)}>{period.label}<span>{period.time}</span></button>)}
        </div>

        <div className={`signup-demo__screen signup-demo__screen--${activePeriod.style}`} aria-live="polite">
          <div className="signup-demo__screen-header">
            <span>{venue.venueName}</span>
            <strong>
              {activePeriod.label}
              {activePeriod.style === "live" ? <em className="signup-demo__live-badge">Live</em> : null}
            </strong>
          </div>
          <p>{activePeriod.headline}</p>
          <ul>
            {activePeriod.items.map(item => <li key={item.name} className={item.tag === "sold-out" ? "signup-demo__item--sold-out" : undefined}>
              <span>{item.name}</span>
              <span className="signup-demo__item-price">{item.price}</span>
              {item.tag ? <em className={`signup-demo__tag signup-demo__tag--${item.tag}`}>{boardTagLabel[item.tag]}</em> : null}
            </li>)}
          </ul>
          <small>Preview only · no venue data is changed</small>
        </div>
      </section>

      <section className="signup-pairing-story" aria-labelledby="pairing-story-heading">
        <div><span>From account to screen</span><h2 id="pairing-story-heading">Pair once. Know when it is live.</h2></div>
        <ol>
          <li><span>1</span><div><strong>Open Vennusign on the display</strong><p>The player shows a single-use six-digit pairing code.</p></div></li>
          <li><span>2</span><div><strong>Enter the code in setup</strong><p>Vennusign links the display to the authorized venue selected by your saved journey.</p></div></li>
          <li><span>3</span><div><strong>Wait for the Online heartbeat</strong><p>Pairing and live status stay distinct, so setup never claims success before the player reports in.</p></div></li>
        </ol>
        <p className="signup-pairing-story__hardware">Runs on Vennusign&rsquo;s own web player, Android TV, Fire TV, Samsung Tizen, and LG webOS &mdash; pair whatever screen you already have.</p>
      </section>

      <section className="signup-data-note" aria-labelledby="home-data-heading">
        <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.6} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
          <path d="M12 3l7 3v6c0 4.5-3 7.5-7 9-4-1.5-7-4.5-7-9V6l7-3Z" />
          <path d="M9.5 12l1.8 1.8L15 10" />
        </svg>
        <div>
          <h2 id="home-data-heading">Your menu data stays yours.</h2>
          <p>Every venue&rsquo;s data is isolated to its own workspace, and every change &mdash; who made it and when &mdash; is recorded and visible to your team.</p>
        </div>
      </section>

      <section id="home-faq" className="signup-faq" aria-labelledby="home-faq-heading">
        <div><span>Questions</span><h2 id="home-faq-heading">Before you start setup.</h2></div>
        <div className="signup-faq__list">
          {faqs.map(faq => <details key={faq.q}>
            <summary>{faq.q}</summary>
            <p>{faq.a}</p>
          </details>)}
        </div>
      </section>

      <section className="signup-pricing" aria-labelledby="home-pricing-heading">
        <div><span>Public pricing</span><h2 id="home-pricing-heading">Choose after you create your account.</h2></div>
        {plans.length ? <ul>{plans.map(plan => <li key={plan.id}>
          <strong>{plan.name}</strong>
          <p><span>${plan.monthlyPrice}</span> / month</p>
          <small>{plan.maxVenues === -1 ? "Unlimited venues" : `${plan.maxVenues} venue${plan.maxVenues === 1 ? "" : "s"}`} · {plan.maxScreens === -1 ? "Unlimited screens" : `${plan.maxScreens} screens`}</small>
          {plan.trialDays > 0 ? <em>{plan.trialDays}-day trial available</em> : null}
        </li>)}</ul> : <p className="signup-pricing__empty" role="status">No public plans are available right now. You can still create an account and choose later.</p>}
        <p className="signup-pricing__note">Plan availability and entitlement are confirmed by Vennusign during onboarding.</p>
        <a className="signup-marketing__primary site-home__final-cta" href={signupUrl}>Start setup</a>
      </section>
    </main>
  </div>;
}
