import { useState } from "react";
import type { PublicOnboardingPlan } from "./customerOnboardingApi";

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

const demoMoments = [
  {
    id: "breakfast",
    label: "Breakfast",
    time: "7:00 AM",
    message: "Morning favorites are live",
    items: ["House roast · $3", "Avocado toast · $11", "Berry bowl · $9"]
  },
  {
    id: "lunch",
    label: "Lunch",
    time: "11:30 AM",
    message: "Lunch menu switches on time",
    items: ["Market sandwich · $14", "Tomato soup · $7", "Citrus salad · $12"]
  },
  {
    id: "evening",
    label: "Evening",
    time: "5:00 PM",
    message: "Dinner presentation is ready",
    items: ["Roasted salmon · $26", "Garden pasta · $21", "Chocolate tart · $9"]
  }
] as const;

export default function SignupMarketingExperience({ plans }: { plans: PublicOnboardingPlan[] }) {
  const [momentId, setMomentId] = useState<(typeof demoMoments)[number]["id"]>("breakfast");
  const activeMoment = demoMoments.find(moment => moment.id === momentId) ?? demoMoments[0];

  return <div className="signup-marketing">
    <section className="signup-marketing__hero" aria-labelledby="signup-heading">
      <div className="signup-marketing__hero-panel">
        <div className="signup-marketing__hero-device" aria-hidden="true">
          <div className="signup-marketing__hero-device-screen">
            <span>VENNU CAFE · Evening</span>
            <strong>Dinner presentation is ready</strong>
            <div className="signup-marketing__hero-device-bars"><i /><i /></div>
          </div>
        </div>
        <span>Digital menus, without the friction</span>
        <h1 id="signup-heading">Put your first screen live.</h1>
        <p>See the product before you sign up. Then create your organization, choose an available plan, and pair a display with one secure code.</p>
        <div className="signup-marketing__actions">
          <a className="signup-marketing__primary" href="#signup-auth-card">Start setup</a>
          <a href="#live-product-demo">Try the live demo</a>
          <a className="signup-marketing__book-demo" href="#signup-faq">Book a demo instead</a>
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
        <p>Choose a service period to preview the guest-facing screen.</p>
      </header>
      <div className="signup-demo__tabs" role="group" aria-label="Preview a service period">
        {demoMoments.map(moment => <button key={moment.id} type="button" aria-pressed={moment.id === activeMoment.id} onClick={() => setMomentId(moment.id)}>{moment.label}<span>{moment.time}</span></button>)}
      </div>
      <div className="signup-demo__screen" aria-live="polite">
        <div className="signup-demo__screen-header"><span>VENNU CAFE</span><strong>{activeMoment.label}</strong></div>
        <p>{activeMoment.message}</p>
        <ul>{activeMoment.items.map(item => <li key={item}>{item}</li>)}</ul>
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
      <p className="signup-pairing-story__hardware">Runs on Vennusign&rsquo;s own web player, Android TV, Fire TV, Samsung Tizen, and LG webOS — pair whatever screen you already have.</p>
    </section>

    <section className="signup-data-note" aria-labelledby="signup-data-heading">
      <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.6} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
        <path d="M12 3l7 3v6c0 4.5-3 7.5-7 9-4-1.5-7-4.5-7-9V6l7-3Z" />
        <path d="M9.5 12l1.8 1.8L15 10" />
      </svg>
      <div>
        <h2 id="signup-data-heading">Your menu data stays yours.</h2>
        <p>Every venue&rsquo;s data is isolated to its own workspace, and every change &mdash; who made it and when &mdash; is recorded and visible to your team.</p>
      </div>
    </section>

    <section id="signup-faq" className="signup-faq" aria-labelledby="signup-faq-heading">
      <div><span>Questions</span><h2 id="signup-faq-heading">Before you start setup.</h2></div>
      <div className="signup-faq__list">
        {faqs.map(faq => <details key={faq.q}>
          <summary>{faq.q}</summary>
          <p>{faq.a}</p>
        </details>)}
      </div>
    </section>

    <section className="signup-pricing" aria-labelledby="signup-pricing-heading">
      <div><span>Public pricing</span><h2 id="signup-pricing-heading">Choose after you create your account.</h2></div>
      {plans.length ? <ul>{plans.map(plan => <li key={plan.id}>
        <strong>{plan.name}</strong>
        <p><span>${plan.monthlyPrice}</span> / month</p>
        <small>{plan.maxVenues === -1 ? "Unlimited venues" : `${plan.maxVenues} venue${plan.maxVenues === 1 ? "" : "s"}`} · {plan.maxScreens === -1 ? "Unlimited screens" : `${plan.maxScreens} screens`}</small>
        {plan.trialDays > 0 ? <em>{plan.trialDays}-day trial available</em> : null}
      </li>)}</ul> : <p className="signup-pricing__empty" role="status">No public plans are available right now. You can still create or return to your account and choose later.</p>}
      <p className="signup-pricing__note">Plan availability and entitlement are confirmed by Vennusign during onboarding. This preview cannot start a trial or subscription.</p>
    </section>
  </div>;
}
