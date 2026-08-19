import { useEffect } from "react";
import { signupUrl, signinUrl } from "./config";
import { Board } from "./Board";
import type { BoardPeriod } from "./boardExamples";

const venueName = "VENNU TOWER";

const lobbyDirectory: BoardPeriod = {
  id: "lobby-directory",
  label: "Lobby Directory",
  time: "All day",
  headline: "Today at Vennu Tower",
  style: "classic-chalkboard",
  glow: "#8fa8ff",
  items: [],
  categories: [
    {
      name: "Today's Meetings",
      items: [
        { name: "Board Room A", price: "10:00 AM — Town Hall" },
        { name: "Board Room B", price: "2:00 PM — Budget Review" },
        { name: "Huddle Room 3", price: "3:30 PM — Design Sync", tag: "new" }
      ]
    },
    {
      name: "Building Directory",
      items: [
        { name: "Reception", price: "Level 1" },
        { name: "IT Help Desk", price: "Level 2" },
        { name: "Cafeteria", price: "Level 1" }
      ]
    }
  ]
};

const emergencyAlert: BoardPeriod = {
  id: "emergency-alert",
  label: "Emergency Alert",
  time: "As needed",
  headline: "Emergency Alert",
  style: "promo-splash",
  items: [
    { name: "Fire Alarm Activated", price: "SOS" },
    { name: "Assembly Point", price: "North Parking Lot" },
    { name: "Do Not Use Elevators", price: "Use Stairwell B" },
    { name: "Emergency Line", price: "Ext. 4100" }
  ]
};

export default function CorporateComms() {
  useEffect(() => {
    document.title = "Vennusign for Corporate Communications";
  }, []);

  return <div className="site">
    <header className="site-nav">
      <span className="site-nav__brand">Vennusign</span>
      <nav className="site-nav__links">
        <a href="/">Home</a>
        <a href="/restaurants">Restaurants &amp; QSRs</a>
        <a href="/corporate-comms" aria-current="page">Corporate Comms</a>
        <a href="/#home-pricing-heading">Pricing</a>
        <a href={signinUrl}>Sign in</a>
        <a className="site-nav__signup" href={signupUrl}>Sign up</a>
      </nav>
    </header>

    <main className="site-home">
      <section className="home-hero" aria-labelledby="corporate-heading">
        <span className="home-hero__eyebrow">Vennusign for Corporate Communications</span>
        <h1 id="corporate-heading">Keep your workforce informed, <span>not your IT helpdesk busy.</span></h1>
        <p className="home-hero__lede">Deliver critical company updates, live KPI dashboards, and emergency broadcasts with 100% certainty. Vennusign proves your internal comms are live and automatically recovers offline displays so IT never has to touch them.</p>
        <div className="home-hero__actions">
          <a className="signup-marketing__primary" href={signupUrl}>Start your free trial</a>
          <a className="home-hero__secondary" href="/#home-faq">Book a demo</a>
        </div>
      </section>

      <section className="industry-problem" aria-labelledby="corporate-problem-heading">
        <div className="industry-problem__grid">
          <div>
            <h2 id="corporate-problem-heading">When an email gets ignored, the screen is your last line of defense.</h2>
            <p>Reaching your deskless workforce &mdash; the staff on the production floor, in the warehouse, or moving between buildings &mdash; is notoriously difficult. They can&rsquo;t check email mid-shift.</p>
            <p>Digital signage fills that gap, but only if the screens are actually on. If your lobby screen shows a &ldquo;No Signal&rdquo; error or your warehouse KPI board is stuck loading yesterday&rsquo;s data, you are failing to communicate. Vennusign gives HR the tools to broadcast instantly, and gives IT the proof of play to know it worked.</p>
          </div>
          <div className="industry-problem__visual">
            <Board venueName={venueName} period={lobbyDirectory} />
          </div>
        </div>
      </section>

      <section className="industry-features" aria-labelledby="corporate-features-heading">
        <div className="industry-features__intro">
          <h2 id="corporate-features-heading">Secure, verifiable corporate messaging</h2>
          <p>Whether you manage screens across a single campus or global offices, Vennusign acts as a secure layer over your hardware.</p>
        </div>
        <div className="industry-features__grid">
          <div className="industry-features__card industry-features__card--emergency">
            <em>Emergency override</em>
            <h3>1. Instant broadcast override</h3>
            <p>When safety is on the line, you cannot wait for a playlist to loop. Trigger a campus-wide alert instantly to force all targeted screens to display critical emergency instructions.</p>
          </div>
          <div className="industry-features__card industry-features__card--metrics">
            <em>Secure metrics</em>
            <h3>2. Live dashboards, safely</h3>
            <p>Display real-time PowerBI, Tableau, or custom KPI dashboards securely without exposing internal company login credentials to public-facing web browsers.</p>
          </div>
          <div className="industry-features__card industry-features__card--access">
            <em>Role-based access</em>
            <h3>3. Local control, global oversight</h3>
            <p>Let the local warehouse manager post a welcome slide, while Corporate Communications maintains strict control over the branded company news ticker.</p>
          </div>
        </div>
      </section>

      <section className="industry-showcase" aria-labelledby="corporate-showcase-heading">
        <div className="industry-showcase__intro">
          <span>Instant broadcast override</span>
          <h2 id="corporate-showcase-heading">This is what the override screen actually looks like.</h2>
        </div>
        <div className="industry-showcase__board">
          <Board venueName={venueName} period={emergencyAlert} />
        </div>
      </section>

      <section className="industry-loop" aria-labelledby="corporate-loop-heading">
        <div className="industry-loop__intro">
          <h2 id="corporate-loop-heading">IT&rsquo;s favorite digital signage platform</h2>
          <p>Traditional CMS platforms generate constant support tickets. Vennusign is built to keep IT out of the digital signage business entirely.</p>
        </div>
        <div className="industry-loop__steps">
          <div className="industry-loop__step">
            <span className="industry-loop__number">1</span>
            <div>
              <h3>Automated failure detection</h3>
              <p>If a media player freezes, loses network connection, or fails to render content, Vennusign detects the discrepancy instantly.</p>
            </div>
          </div>
          <div className="industry-loop__step">
            <span className="industry-loop__number">2</span>
            <div>
              <h3>Self-healing recovery <span>Resolves 90% of issues without IT</span></h3>
              <p>Before an employee even logs a support ticket, Vennusign initiates automated recovery protocols to restart the player and restore the feed.</p>
            </div>
          </div>
          <div className="industry-loop__step">
            <span className="industry-loop__number">3</span>
            <div>
              <h3>Undeniable proof of play <span>Timestamped for auditing</span></h3>
              <p>Stop guessing if the CEO&rsquo;s town hall announcement actually played in the regional office. Get precise, timestamped confirmation.</p>
            </div>
          </div>
        </div>
      </section>

      <section className="home-cta-banner" aria-labelledby="corporate-cta-heading">
        <h2 id="corporate-cta-heading">Ready to upgrade your internal comms?</h2>
        <p>Give HR the reach they need and give IT the automated reliability they demand.</p>
        <div className="home-cta-banner__actions">
          <a className="home-cta-banner__primary" href={signupUrl}>Start your 14-day free trial</a>
        </div>
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
