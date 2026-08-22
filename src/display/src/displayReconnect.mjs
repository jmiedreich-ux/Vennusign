/**
 * A screen on a wall never stops trying to come back.
 *
 * SignalR's `withAutomaticReconnect()` with no argument is four attempts - 0s, 2s,
 * 10s, 30s - and then it stops for good. Any interruption longer than about 42
 * seconds therefore ended a display's realtime connection permanently: an API
 * deploy, an App Service restart, a plan resize, a venue's wifi dropping out over
 * lunch. The board went on showing "Live updates unavailable" until a person
 * reloaded it, and a menu board is unattended hardware that nobody reloads.
 *
 * It hid itself, too. DisplayPage's 60s recovery poll kept fetching content, so the
 * only symptoms were the banner and a wall that lagged a publish by up to a minute
 * instead of seconds - which is what the first publish-to-wall measurements were
 * actually recording.
 *
 * So: the same opening cadence, then a steady retry that never gives up. The
 * ceiling is a minute because that already matches the recovery poll - past that
 * point a screen is being carried by polling anyway, and backing off further gains
 * nothing.
 */
export const RECONNECT_CEILING_MS = 60_000;
export const RECONNECT_RAMP_MS = Object.freeze([0, 2_000, 10_000, 30_000]);

/** Never returns null, which is what tells SignalR to stop. */
export function nextReconnectDelayMs(previousRetryCount) {
  const ramped = RECONNECT_RAMP_MS[previousRetryCount];
  return ramped === undefined ? RECONNECT_CEILING_MS : ramped;
}

export const displayRetryPolicy = {
  nextRetryDelayInMilliseconds: context => nextReconnectDelayMs(context.previousRetryCount)
};
