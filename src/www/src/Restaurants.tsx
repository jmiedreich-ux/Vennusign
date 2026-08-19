import { useEffect } from "react";
import { signupUrl, signinUrl } from "./config";
import { venueExamples } from "./boardExamples";
import { ScreenWall } from "./Board";

const restaurant = venueExamples.find(v => v.id === "restaurant")!;
const daypartWall = ["breakfast", "lunch", "happy-hour"]
  .map(id => ({ venue: restaurant, period: restaurant.periods.find(p => p.id === id)! }));

export default function Restaurants() {
  useEffect(() => {
    document.title = "Vennusign for Restaurants & QSRs";
  }, []);

  return <div className="site">
    <header className="site-nav">
      <span className="site-nav__brand">Vennusign</span>
      <nav className="site-nav__links">
        <a href="/">Home</a>
        <a href="/restaurants" aria-current="page">Restaurants &amp; QSRs</a>
        <a href="/corporate-comms">Corporate Comms</a>
        <a href="/#home-pricing-heading">Pricing</a>
        <a href={signinUrl}>Sign in</a>
        <a className="site-nav__signup" href={signupUrl}>Sign up</a>
      </nav>
    </header>

    <main className="site-home">
      <section className="home-hero" aria-labelledby="restaurants-heading">
        <span className="home-hero__eyebrow home-hero__eyebrow--warm">Vennusign for Restaurants &amp; QSRs</span>
        <h1 id="restaurants-heading">Stop losing sales to <span>dark screens and outdated menus.</span></h1>
        <p className="home-hero__lede">Update prices instantly, 86 sold-out items, and automatically shift from breakfast to lunch. Vennusign is the only restaurant CMS that verifies your menu actually updated on the screen &mdash; and self-heals when a player drops.</p>
        <div className="home-hero__actions">
          <a className="signup-marketing__primary" href={signupUrl}>Start your free trial</a>
          <a className="home-hero__secondary" href="/#home-faq">Book a demo</a>
        </div>
      </section>

      <section className="industry-problem" aria-labelledby="restaurants-problem-heading">
        <div className="industry-problem__text">
          <h2 id="restaurants-problem-heading">When a drive-thru screen goes dark, you lose money by the minute.</h2>
          <p>For quick-service restaurants, a digital menu board isn&rsquo;t just a sign &mdash; it&rsquo;s a critical point of sale.</p>
          <p>If your current software tells you a price update was &ldquo;sent&rdquo; to a location, but doesn&rsquo;t tell you if the screen in the drive-thru is actually working, you are operating blind. Vennusign closes that gap. We don&rsquo;t just push your menus &mdash; we verify they are live, and we automatically recover them if they crash.</p>
        </div>
      </section>

      <section className="screen-wall-section" aria-labelledby="restaurants-wall-heading">
        <div><span>The 3-panel drive-thru board</span><h2 id="restaurants-wall-heading">Breakfast becomes lunch becomes happy hour &mdash; automatically.</h2></div>
        <p>This is a real Vennusign render, not a mockup: the same counter, three screens, three dayparts, switching on schedule with no one touching a remote.</p>
        <ScreenWall periods={daypartWall} />
      </section>

      <section className="industry-features" aria-labelledby="restaurants-features-heading">
        <div className="industry-features__intro">
          <h2 id="restaurants-features-heading">Built for the speed of restaurant operations</h2>
          <p>From single-location cafes to massive national franchises, Vennusign gives your team centralized control.</p>
        </div>
        <div className="industry-features__grid">
          <div className="industry-features__card">
            <h3>1. Instant &ldquo;86&rdquo; and price updates</h3>
            <p>When you run out of a core ingredient, every second counts. The Vennusign Menu Builder lets you mark items as sold out, update seasonal pricing, and adjust modifiers in seconds without a graphic designer.</p>
          </div>
          <div className="industry-features__card">
            <h3>2. Verified dayparting</h3>
            <p>Schedule your boards to transition from breakfast to lunch automatically. Head office can see timestamped logs confirming that the switch happened flawlessly across all locations.</p>
          </div>
          <div className="industry-features__card">
            <h3>3. Network centralization</h3>
            <p>Group screens by franchise owner, region, or store layout. Push a national promotion to 500 locations instantly, while allowing local managers control over their specific sold-out items.</p>
          </div>
        </div>
      </section>

      <section className="industry-loop" aria-labelledby="restaurants-loop-heading">
        <div className="industry-loop__intro">
          <h2 id="restaurants-loop-heading">What happens when a player crashes at noon?</h2>
          <p>In a busy QSR, the staff behind the counter cannot stop serving food to go reboot a screen. With Vennusign, a crash triggers our automated recovery loop.</p>
        </div>
        <div className="industry-loop__steps">
          <div className="industry-loop__step">
            <span className="industry-loop__number">1</span>
            <div>
              <h3>Detection</h3>
              <p>The Vennusign platform detects that the screen or media player has lost connection or dropped its visual feed.</p>
            </div>
          </div>
          <div className="industry-loop__step">
            <span className="industry-loop__number">2</span>
            <div>
              <h3>Self-healing restart <span>No manual intervention required</span></h3>
              <p>The system instantly initiates automated recovery protocols, attempting to reboot the software and re-establish the connection.</p>
            </div>
          </div>
          <div className="industry-loop__step">
            <span className="industry-loop__number">3</span>
            <div>
              <h3>Status verified <span>Timestamped for your records</span></h3>
              <p>Once the board is back online, the system logs the proof of play and returns the dashboard status from Off to Live.</p>
            </div>
          </div>
        </div>
      </section>

      <section className="home-cta-banner" aria-labelledby="restaurants-cta-heading">
        <h2 id="restaurants-cta-heading">Ready to build a better menu board?</h2>
        <p>Keep your lines moving and your customers informed. See why QSR operators are switching to Vennusign for verified peace of mind.</p>
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
