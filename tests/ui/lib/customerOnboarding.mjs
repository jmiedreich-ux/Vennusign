/**
 * Getting a QA account through onboarding, and keeping it through.
 *
 * Onboarding ends at go-live, which needs a paired display reporting Online, so
 * before this existed no automated run could reach Back Office at all. Two
 * different jobs live here and they are deliberately separate:
 *
 *   ensureOnboarded   - the account should already be in; finish whatever is left
 *                       and get out of the way. Idempotent, and a no-op for an
 *                       account that has already gone live.
 *   onboardFromZero   - actually exercise onboarding as a new customer would.
 *
 * Both drive the real Back Office forms rather than posting to the API. Onboarding
 * is a UI feature; a helper that skipped the pages would leave them untested and
 * would keep passing while they were broken.
 *
 * The display step uses qaDisplay.mjs, which is the shipped player code.
 */
import { startQaDisplay } from "./qaDisplay.mjs";

const ONBOARDING_PATH = "/onboarding";

/** Reads the authoritative snapshot using the page's own customer session. */
export async function loadSnapshot(page, apiBaseUrl) {
  return page.evaluate(async base => {
    const response = await fetch(`${base}/api/customer-onboarding`, { credentials: "include" });
    return response.ok ? response.json() : null;
  }, apiBaseUrl);
}

async function submitOrganization(page, organization) {
  await page.fill("#organizationName", organization.name);
  await page.fill("#primaryContactName", organization.primaryContactName);
  await page.fill("#contactEmail", organization.contactEmail);
  await page.fill("#mailingAddress", organization.mailingAddress);
  await page.getByRole("button", { name: /save and choose a plan/i }).click();
}

/**
 * Starts a trial on the first plan that offers one.
 *
 * Tiers are configured in Platform Operations, so what is on offer is whatever is
 * configured - possibly nothing, and possibly nothing with a trial. That is a real
 * customer-facing state (issue #729), not a test environment quirk, so this says so
 * plainly instead of timing out on a button that was never going to appear.
 */
async function chooseTrialPlan(page) {
  const trialButtons = page.getByRole("button", { name: /try \d+ days free/i });
  await page.locator(".customer-onboarding__plans").waitFor();
  if (await trialButtons.count() === 0) {
    throw new Error(
      "No plan on the onboarding plan step offers a trial. Subscription tiers are configured in " +
      "Platform Operations; configure at least one active public tier with trial days, or a tier " +
      "with Stripe checkout, before an automated account can get past this step."
    );
  }
  await trialButtons.first().click();
}

async function submitVenue(page, venue) {
  await page.fill("#venueName", venue.name);
  await page.fill("#timezone", venue.timezone);
  await page.selectOption("#venueType", venue.type);
  await page.getByRole("button", { name: /save venue and continue/i }).click();
}

/**
 * Pairs a QA display and waits for go-live.
 *
 * The display keeps beating until this returns, because go-live is recorded on the
 * heartbeat that first reports Online - and because a display that stopped beating
 * would be marked Offline again 90 seconds later.
 */
async function pairDisplay(page, apiBaseUrl) {
  const display = await startQaDisplay(apiBaseUrl);
  try {
    await page.fill("#pairingCode", display.code);
    await page.getByRole("button", { name: /pair this display/i }).click();
    await display.beat();

    // The page polls every 10 seconds; nudge it rather than waiting on the timer.
    await page.getByRole("button", { name: /refresh device status/i }).click();
    await page.getByText(/you.re live/i).waitFor({ timeout: 30_000 });
    return display.screenId;
  } finally {
    display.stop();
  }
}

/**
 * Drives whatever onboarding steps are still outstanding, in order, and returns
 * the final snapshot. Safe to call on an account at any point in the journey,
 * including one that is already finished.
 */
export async function completeOnboarding(page, apiBaseUrl, { organization, venue }) {
  let snapshot = await loadSnapshot(page, apiBaseUrl);
  if (snapshot?.progress?.goLive) return snapshot;

  await page.goto(ONBOARDING_PATH);

  if (!snapshot?.organizationId) {
    await submitOrganization(page, organization);
    await page.getByText(/choose how to begin/i).waitFor();
  }
  if (!snapshot?.progress?.plan) {
    await chooseTrialPlan(page);
    await page.getByText(/set up your first venue/i).waitFor();
  }
  if (!snapshot?.venueId) {
    await submitVenue(page, venue);
    await page.getByText(/pair your physical display/i).waitFor();
  }
  if (!snapshot?.firstScreenId) {
    await pairDisplay(page, apiBaseUrl);
  }

  snapshot = await loadSnapshot(page, apiBaseUrl);
  if (!snapshot?.progress?.goLive) {
    throw new Error(`Onboarding did not reach go-live. Current step: ${snapshot?.currentStep ?? "unknown"}.`);
  }
  return snapshot;
}

/**
 * The everyday entry point: leave this account onboarded, doing as little as
 * possible. An account that has already gone live costs one request and no UI.
 */
export async function ensureOnboarded(page, apiBaseUrl, defaults) {
  const snapshot = await loadSnapshot(page, apiBaseUrl);
  if (snapshot?.progress?.goLive) return snapshot;
  return completeOnboarding(page, apiBaseUrl, defaults);
}

/** Details for a brand-new account, unique per run so repeat runs do not collide. */
export function newAccountDetails(label = String(Date.now())) {
  return {
    organization: {
      name: `QA Hospitality ${label}`,
      primaryContactName: "QA Murphy",
      contactEmail: `qa-${label}@vennusign.com`,
      mailingAddress: "1 Test Street, Testville, NY 10001"
    },
    venue: { name: `QA Venue ${label}`, timezone: "America/New_York", type: "restaurant" }
  };
}
