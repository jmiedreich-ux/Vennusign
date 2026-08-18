import { useEffect, useMemo, useState } from "react";
import { signupUrl, signinUrl } from "./config";
import { venueExamples, boardTagLabel, type BoardPeriod, type BoardItem, type VenueExample } from "./boardExamples";
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

const AUTO_ADVANCE_MS = 4800;

// The real Photo Grid layout shows a shimmer-loading placeholder when an item has no
// photo yet - this reuses that same idea (a neutral shimmer, not a fake "photo") with a
// thin accent stripe cycling through a small curated palette, instead of a raw per-item
// hue rotation.
const PHOTO_ACCENT_PALETTE = ["#d9a15b", "#8fae7c", "#c97b63", "#8ba7c9", "#c9a4d4", "#7ec9be"];

function ItemTag({ item }: { item: BoardItem }) {
  if (!item.tag) return null;
  return <em className={`board-tag board-tag--${item.tag}`}>{boardTagLabel[item.tag]}</em>;
}

function HappyHourBanner({ endsLabel }: { endsLabel: string }) {
  const [secondsLeft, setSecondsLeft] = useState(9 * 60 + 42);
  useEffect(() => {
    const id = setInterval(() => setSecondsLeft(s => (s <= 0 ? 12 * 60 : s - 1)), 1000);
    return () => clearInterval(id);
  }, []);
  const minutes = Math.floor(secondsLeft / 60);
  const seconds = secondsLeft % 60;
  return <div className="happy-hour-banner">
    <strong>Happy hour</strong>
    <span>{minutes}:{seconds.toString().padStart(2, "0")} left · {endsLabel}</span>
  </div>;
}

function Board({ venueName, period, compact }: { venueName: string; period: BoardPeriod; compact?: boolean }) {
  const [featuredIndex, setFeaturedIndex] = useState(0);
  const isHero = period.style === "daily-special-hero";
  useEffect(() => {
    if (!isHero) return;
    const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (reduceMotion) return;
    const id = setInterval(() => setFeaturedIndex(i => (i + 1) % period.items.length), 3200);
    return () => clearInterval(id);
  }, [isHero, period.items.length]);

  const glowStyle = period.glow ? ({ "--board-glow": period.glow } as React.CSSProperties) : undefined;

  return <div className={`board board--${period.style}${compact ? " board--compact" : ""}`} style={glowStyle} aria-live="polite">
    {period.happyHourEndsLabel ? <HappyHourBanner endsLabel={period.happyHourEndsLabel} /> : null}
    <div className="board__header">
      <span>{venueName}</span>
      {period.happyHourEndsLabel ? null : <strong>{period.label}</strong>}
    </div>

    {period.style === "daily-special-hero" ? (() => {
      const featured = period.items[featuredIndex];
      const secondary = period.items.filter((_, i) => i !== featuredIndex);
      return <div className="board__hero">
        <em className="board__hero-pill">Featured now</em>
        <p className="board__hero-headline">{period.headline}</p>
        <div className="board__hero-featured">
          <span>{featured.name}</span>
          <data value={featured.price}>{featured.price}</data>
        </div>
        <div className="board__hero-secondary">
          {secondary.map(item => <div key={item.name}>
            <span>{item.name}</span><data value={item.price}>{item.price}</data>
          </div>)}
        </div>
      </div>;
    })() : period.style === "photo-grid" ? <>
      <p className="board__headline">{period.headline}</p>
      <div className="board__photo-grid">
        {period.items.map((item, index) => <div key={item.name} className={`board__photo-card${item.tag === "sold-out" ? " board__photo-card--sold-out" : ""}`}>
          <div className="board__photo-swatch" style={{ "--photo-accent": PHOTO_ACCENT_PALETTE[index % PHOTO_ACCENT_PALETTE.length] } as React.CSSProperties} />
          <div className="board__photo-copy">
            <span>{item.name}</span>
            <data value={item.price}>{item.price}</data>
          </div>
          <ItemTag item={item} />
        </div>)}
      </div>
    </> : period.style === "digital-tap-board" ? <>
      <p className="board__headline">{period.headline}</p>
      <div className="board__tap-grid">
        {period.items.map(item => <div key={item.name} className={`board__tap-card${item.tag === "sold-out" ? " board__tap-card--sold-out" : ""}`}>
          <svg width="22" height="26" viewBox="0 0 22 26" aria-hidden="true"><path d="M3 2h16l-2 20a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2L3 2Z" fill="none" stroke="currentColor" strokeWidth="1.6" /><path d="M3 8h16" stroke="currentColor" strokeWidth="1.2" opacity=".5" /></svg>
          <div className="board__tap-copy">
            <span>{item.name}</span>
            {item.detail ? <small>{item.detail}</small> : null}
          </div>
          <data value={item.price}>{item.price}</data>
          {item.tag === "new" ? <em className="board__tap-brewing">Now brewing</em> : <ItemTag item={item} />}
        </div>)}
      </div>
    </> : <>
      <p className="board__headline">{period.headline}</p>
      <ul className="board__list">
        {period.items.map(item => <li key={item.name} className={item.tag === "sold-out" ? "board__item--sold-out" : undefined}>
          <span>{item.name}</span>
          <data value={item.price} className="board__item-price">{item.price}</data>
          <ItemTag item={item} />
        </li>)}
      </ul>
    </>}
    <small className="board__footnote">Preview only · no venue data is changed</small>
  </div>;
}

// One example of every distinct real layout, so a visitor sees the full range of
// styles at a glance in a horizontal showcase, instead of waiting for the timeline
// above to cycle around to it.
const styleShowcase = venueExamples
  .flatMap(v => v.periods.map(period => ({ venue: v, period })))
  .filter((entry, index, all) => all.findIndex(e => e.period.style === entry.period.style) === index);

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
          <div><span>Real Vennusign board styles</span><h2 id="live-demo-heading">People eat with their eyes first.</h2></div>
          <p>So Vennusign screens are built the same way. Two venues, nine boards, cycling on their own below — touch nothing, or jump straight to one.</p>
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
