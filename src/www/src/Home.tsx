import { useEffect, useMemo, useState } from "react";
import { signupUrl, signinUrl } from "./config";
import { venueExamples, type VenueExample } from "./boardExamples";
import { loadPublicPlans, type PublicOnboardingPlan } from "./plansApi";
import { Board, ScreenWall } from "./Board";

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

const AUTO_ADVANCE_MS = 4800;

// One example of every distinct real layout, so a visitor sees the full range of
// styles at a glance in a horizontal showcase, instead of waiting for the timeline
// above to cycle around to it.
const allPeriods = venueExamples.flatMap(v => v.periods.map(period => ({ venue: v, period })));
const styleShowcase = allPeriods.filter((entry, index, all) => all.findIndex(e => e.period.style === entry.period.style) === index);

// The hero's whole job is to prove "rich screen designs" in the first five seconds -
// so it rotates through a few of the flashiest real boards instead of describing them.
const heroShowcase = ["late-night", "draft-list", "cocktail-hour"]
  .map(id => allPeriods.find(entry => entry.period.id === id)!);

function HeroDeviceShowcase() {
  const [index, setIndex] = useState(0);
  useEffect(() => {
    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;
    const id = setInterval(() => setIndex(i => (i + 1) % heroShowcase.length), 3400);
    return () => clearInterval(id);
  }, []);
  const entry = heroShowcase[index];
  return <div className="home-hero__demo" aria-hidden="true">
    <Board venueName={entry.venue.venueName} period={entry.period} />
  </div>;
}

// Many real venues - the reference case is Chinese takeout counters - don't run one
// screen, they run two or three mounted side by side, each showing something different.
// This proves that arrangement instead of describing it: a shared physical frame around
// several independent boards, exactly like the photos of real backlit menu walls.
const chineseWall = ["lunch-special", "house-favorites", "combo-platters"]
  .map(id => allPeriods.find(entry => entry.period.id === id)!);

// "Preview a live board render" in the verification loop's first step is a specific,
// checkable claim - so it gets a real rendered board next to it, not an illustration.
const loopVisualEntry = allPeriods.find(entry => entry.period.id === "dinner")!;

export default function Home() {
  const flatPeriods = useMemo(
    () => venueExamples.flatMap(v => v.periods.map(period => ({ venue: v as VenueExample, period }))),
    []
  );
  const [activeIndex, setActiveIndex] = useState(0);
  const [autoPlaying, setAutoPlaying] = useState(true);
  const active = flatPeriods[activeIndex];

  useEffect(() => {
    if (!autoPlaying) return;
    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;
    const id = setInterval(() => setActiveIndex(i => (i + 1) % flatPeriods.length), AUTO_ADVANCE_MS);
    return () => clearInterval(id);
  }, [autoPlaying, flatPeriods.length]);

  function jumpTo(index: number) {
    setActiveIndex(index);
    setAutoPlaying(false);
  }

  function selectVenue(id: VenueExample["id"]) {
    const index = flatPeriods.findIndex(entry => entry.venue.id === id);
    if (index >= 0) jumpTo(index);
  }

  const [plans, setPlans] = useState<PublicOnboardingPlan[]>([]);
  useEffect(() => {
    const controller = new AbortController();
    loadPublicPlans(controller.signal).then(setPlans).catch(() => {});
    return () => controller.abort();
  }, []);

  return <div className="site">
    <header className="site-nav">
      <span className="site-nav__brand">Vennusign</span>
      <nav className="site-nav__links">
        <a href="#home-loop-heading">Product</a>
        <a href="/restaurants">Restaurants &amp; QSRs</a>
        <a href="/corporate-comms">Corporate Comms</a>
        <a href="#home-pricing-heading">Pricing</a>
        <a href={signinUrl}>Sign in</a>
        <a className="site-nav__signup" href={signupUrl}>Sign up</a>
      </nav>
    </header>

    <main className="site-home">
      <section className="home-hero" aria-labelledby="home-heading">
        <span className="home-hero__eyebrow">Enterprise digital signage CMS</span>
        <h1 id="home-heading">Vennusign runs your screens. <span>And proves it.</span></h1>
        <p className="home-hero__lede">Stop guessing if your content updated. Publish menus and campaigns, verify delivery with undeniable proof of play, and let dropped players recover themselves.</p>
        <div className="home-hero__actions">
          <a className="signup-marketing__primary" href={signupUrl}>Start your free trial</a>
          <a className="home-hero__secondary" href="#home-faq">Book a demo</a>
        </div>
        <p className="home-hero__no-cc">No credit card required to try it.</p>
        <HeroDeviceShowcase />
        <ul className="home-hero__proof" aria-label="Product proof points">
          <li><strong>One workspace</strong><span>Menus, screens, schedules, and daily service controls.</span></li>
          <li><strong>Safe changes</strong><span>Preview, review, and visible delivery state before you move on.</span></li>
          <li><strong>Resumable setup</strong><span>Your authoritative onboarding progress is saved between visits.</span></li>
        </ul>
      </section>

      <section className="home-problem" aria-labelledby="home-problem-heading">
        <h2 id="home-problem-heading">Dispatched is not the same as displayed.</h2>
        <p>Most digital signage software tells you a file was &ldquo;sent&rdquo; to a player. It doesn&rsquo;t tell you if the screen is black, frozen, or showing yesterday&rsquo;s prices.</p>
        <p>When your revenue relies on accurate menus and timely promotions, blind spots cost you money. Vennusign closes the gap between what you scheduled and what your customers actually see.</p>
      </section>

      <section className="home-loop" aria-labelledby="home-loop-heading">
        <div className="home-loop__intro">
          <h2 id="home-loop-heading">The Vennusign verification loop</h2>
          <p>Every update follows a strict, trackable path. Order matters, and the system proves where your content stands at every step.</p>
        </div>
        <div className="home-loop__grid">
          <div className="home-loop__step">
            <span className="home-loop__number">1</span>
            <h3>Edit &amp; preview</h3>
            <p className="home-loop__tag">See it before they do</p>
            <p>Use the Menu Builder to edit sections, 86 sold-out items, and update pricing. Preview a live board render so you know exactly how it will look on the physical display.</p>
            <div className="home-loop__visual"><Board venueName={loopVisualEntry.venue.venueName} period={loopVisualEntry.period} compact /></div>
          </div>
          <div className="home-loop__step">
            <span className="home-loop__number">2</span>
            <h3>Target &amp; publish</h3>
            <p className="home-loop__tag">Push to one screen or a thousand</p>
            <p>Group your screens by location, region, or hardware type. Push updates instantly or schedule them for a future campaign window.</p>
          </div>
          <div className="home-loop__step">
            <span className="home-loop__number">3</span>
            <h3>Verify (proof)</h3>
            <p className="home-loop__tag">Timestamped and confirmed</p>
            <p>Vennusign doesn&rsquo;t just send the file &mdash; it listens for confirmation. Get detailed, timestamped playback logs verifying your content actually rendered.</p>
          </div>
          <div className="home-loop__step">
            <span className="home-loop__number">4</span>
            <h3>Recover</h3>
            <p className="home-loop__tag">Self-healing hardware</p>
            <p>If a player drops or a screen goes dark, Vennusign detects the failure and instantly initiates automated recovery protocols to bring the screen back online.</p>
          </div>
        </div>
      </section>

      <section className="signup-demo" id="live-product-demo" aria-labelledby="home-proof-heading">
        <header>
          <div><span>Not a mockup &mdash; the real renderer</span><h2 id="home-proof-heading">Every board here is what a Vennusign screen actually runs.</h2></div>
          <p>Seven industries, twelve board styles, cycling on their own below &mdash; touch nothing, or jump straight to one.</p>
        </header>

        <div className="signup-demo__venue-toggle" role="group" aria-label="Choose a venue type">
          {venueExamples.map(v => <button key={v.id} type="button" aria-pressed={v.id === active.venue.id} onClick={() => selectVenue(v.id)}>{v.label}</button>)}
        </div>

        <div className="signup-demo__tabs" role="group" aria-label="Preview a service period">
          {active.venue.periods.map(period => {
            const index = flatPeriods.findIndex(entry => entry.period.id === period.id);
            return <button key={period.id} type="button" aria-pressed={period.id === active.period.id} onClick={() => jumpTo(index)}>{period.label}<span>{period.time}</span></button>;
          })}
        </div>

        <div className="signup-demo__playhead">
          <button type="button" className="signup-demo__playhead-toggle" onClick={() => setAutoPlaying(p => !p)} aria-pressed={autoPlaying}>
            {autoPlaying ? "Auto-playing" : "Paused — resume"}
          </button>
          <div className="signup-demo__playhead-dots" aria-hidden="true">
            {flatPeriods.map((entry, index) => <span key={entry.period.id} className={index === activeIndex ? "is-active" : undefined} />)}
          </div>
        </div>

        <Board venueName={active.venue.venueName} period={active.period} />
      </section>

      <section className="board-showcase" aria-labelledby="board-showcase-heading">
        <div><span>Every one is a real layout</span><h2 id="board-showcase-heading">Chalkboards. Neon. Tap boards. All of it.</h2></div>
        <p>Scroll to see the full range &mdash; these are the same layouts shown one at a time above, side by side.</p>
        <div className="board-showcase__strip">
          {styleShowcase.map(entry => <div key={entry.period.id} className="board-showcase__item">
            <Board venueName={entry.venue.venueName} period={entry.period} compact />
          </div>)}
        </div>
      </section>

      <section className="screen-wall-section" aria-labelledby="screen-wall-heading">
        <div><span>One system, several screens</span><h2 id="screen-wall-heading">Mount two or three together, run each one differently.</h2></div>
        <p>Real counters rarely run one screen &mdash; a lunch-special board next to a photo menu is the common case, not the exception. Same account, same content model, independent screens.</p>
        <ScreenWall periods={chineseWall} />
      </section>

      <section className="home-status" aria-labelledby="home-status-heading">
        <div className="home-status__intro">
          <h2 id="home-status-heading">Every screen tells the truth.</h2>
          <p>No more calling the store manager to ask, &ldquo;Is the TV on?&rdquo; Vennusign uses a fixed set of verifiable states so your network status never relies on guesswork.</p>
        </div>
        <div className="home-status__table-wrap">
          <table className="home-status__table">
            <thead>
              <tr><th>Status</th><th>What it means for you</th><th>Automated action</th></tr>
            </thead>
            <tbody>
              <tr>
                <td><span className="home-status__dot home-status__dot--live" aria-hidden="true" />Live</td>
                <td>Content is applied, playing, and visually confirmed.</td>
                <td>Logs proof-of-play data for your reporting.</td>
              </tr>
              <tr>
                <td><span className="home-status__dot home-status__dot--warning" aria-hidden="true" />Warning</td>
                <td>Screen is connected but experiencing playback variance or delays.</td>
                <td>Flags for review before it affects customers.</td>
              </tr>
              <tr>
                <td><span className="home-status__dot home-status__dot--off" aria-hidden="true" />Off</td>
                <td>The player or screen has lost connection.</td>
                <td>Triggers Vennusign&rsquo;s automated recovery loop.</td>
              </tr>
              <tr>
                <td><span className="home-status__dot home-status__dot--promo" aria-hidden="true" />Promotion</td>
                <td>Scheduled, limited-time campaign overriding default content.</td>
                <td>Reverts to standard play automatically when the window ends.</td>
              </tr>
              <tr>
                <td><span className="home-status__dot home-status__dot--emergency" aria-hidden="true" />Emergency</td>
                <td>Total broadcast override for critical communications.</td>
                <td>Forces all targeted screens to emergency messaging instantly.</td>
              </tr>
            </tbody>
          </table>
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

      <section className="home-hardware" aria-labelledby="home-hardware-heading">
        <div>
          <h2 id="home-hardware-heading">Don&rsquo;t rip and replace. Just verify.</h2>
          <p>Vennusign acts as a verification layer over the hardware you already own. Whether you use ChromeOS, Android, Amazon Fire Sticks, or proprietary commercial media players, you get the same enterprise-grade proof of play and status tracking.</p>
          <div className="home-hardware__badges">
            <span>ChromeOS</span>
            <span>Android</span>
            <span>FireOS</span>
            <span>Windows</span>
          </div>
        </div>
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

      <section className="home-cta-banner" aria-labelledby="home-cta-heading">
        <h2 id="home-cta-heading">Ready to stop guessing?</h2>
        <p>Join the operations teams that trust Vennusign to keep their screens live, accurate, and profitable.</p>
        <div className="home-cta-banner__actions">
          <a className="home-cta-banner__primary" href={signupUrl}>Start your 14-day free trial</a>
          <a className="home-cta-banner__secondary" href="#home-faq">Talk to sales</a>
        </div>
        <p className="home-cta-banner__note">No credit card required to start.</p>
      </section>
    </main>

    <footer className="site-footer">
      <span className="site-nav__brand">Vennusign</span>
      <nav className="site-footer__links">
        <a href="#">Privacy Policy</a>
        <a href="#">Terms of Service</a>
        <a href="#">Contact</a>
      </nav>
      <p className="site-footer__copyright">&copy; 2026 Vennusign. All rights reserved.</p>
    </footer>
  </div>;
}
