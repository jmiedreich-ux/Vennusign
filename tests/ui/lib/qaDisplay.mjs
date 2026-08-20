/**
 * A headless display for automated QA, built from the player's own modules.
 *
 * Onboarding cannot complete without a paired screen that reports Online, so
 * automated QA had no way to reach anything behind onboarding: no test could
 * sign in and land in Back Office, and nothing past the opening checklist had
 * browser coverage.
 *
 * This does NOT reimplement the player. It calls `src/display`'s shipped
 * `preparePairingScreen` and `startDisplayHeartbeat` - the same code a real
 * display runs - so a QA display registers, pairs, and beats exactly as a real
 * one does. If pairing changes for customers it changes here in the same commit,
 * and if it breaks for customers these tests break too. A private copy of those
 * requests would have quietly kept passing.
 *
 * There is no test-only endpoint and no privilege a real display lacks; the
 * three endpoints involved are anonymous because players are unauthenticated.
 *
 * HeartbeatMonitor returns a silent screen to Offline after its stale threshold
 * (90 seconds by default), so a display that must keep reading as Online has to
 * keep beating. Always stop() in a finally - `withQaDisplay` does it for you.
 */
import { preparePairingScreen, loadPairingStatus } from "../../../src/display/src/pairing.mjs";
import { startDisplayHeartbeat, sendDisplayHeartbeat } from "../../../src/display/src/displayHeartbeat.mjs";

// "browser" is a real supported platform (ScreenPlatform also accepts "web" and
// normalizes it to this). It must be one the API knows: registration accepts any
// string, but the heartbeat rejects anything outside the supported set - see #730.
// appVersion is free text, so it carries the QA marker instead.
export const QA_PLATFORM = "browser";
export const QA_APP_VERSION = "qa-display";

/**
 * Registers a screen, obtains the six-digit code it would be showing, and keeps
 * it Online until stopped.
 *
 * The first heartbeat is awaited before this resolves, so a caller that needs the
 * screen already Online does not have to poll for it. Pairing codes expire after
 * ten minutes and are single use, so start the display immediately before pairing
 * rather than reusing an earlier one.
 */
export async function startQaDisplay(apiBaseUrl, {
  platform = QA_PLATFORM,
  appVersion = QA_APP_VERSION,
  intervalMs
} = {}) {
  const pairing = await preparePairingScreen(apiBaseUrl, undefined, platform, appVersion);
  const { screenId, code, expiresAt } = pairing;

  // Awaited once here so "the display is Online" is true when this returns.
  // startDisplayHeartbeat's own first beat is fire-and-forget by design, which is
  // right for a real display and useless for a test that asserts immediately.
  await sendDisplayHeartbeat(apiBaseUrl, screenId, fetch, undefined, { platform, appVersion });
  const heartbeat = startDisplayHeartbeat(apiBaseUrl, screenId, { platform, appVersion, ...(intervalMs ? { intervalMs } : {}) });

  return {
    screenId,
    code,
    expiresAt,
    /** One extra beat, awaited - use when a test needs Online to be true right now. */
    beat: () => sendDisplayHeartbeat(apiBaseUrl, screenId, fetch, undefined, { platform, appVersion }),
    /** Has a Back Office user claimed this code yet? */
    pairingStatus: () => loadPairingStatus(apiBaseUrl, code),
    stop: () => heartbeat.stop()
  };
}

/** Runs `fn` with a live QA display and always stops it afterwards. */
export async function withQaDisplay(apiBaseUrl, fn, options = {}) {
  const display = await startQaDisplay(apiBaseUrl, options);
  try {
    return await fn(display);
  } finally {
    display.stop();
  }
}
