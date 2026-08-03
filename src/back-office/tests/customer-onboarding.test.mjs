import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const [app, timeline, api, passkey, main, styles] = await Promise.all([
  readFile(new URL("../src/CustomerOnboardingApp.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/CustomerOnboardingTimeline.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/customerOnboardingApi.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/passkeySignIn.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/main.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/styles.css", import.meta.url), "utf8")
]);

test("public entry exposes passwordless routes and returning-user recovery", () => {
  assert.match(app, /Continue with Google/);
  assert.match(app, /Continue with Apple/);
  assert.match(app, /Use a passkey/);
  assert.match(app, /Email me a sign-in link/);
  assert.match(app, /Available plans/);
  assert.match(app, /No public plans are available right now/);
  assert.match(main, /\/signup/);
  assert.match(main, /\/signin/);
  assert.match(main, /\/onboarding/);
});

test("onboarding is resumable, credentialed, and webhook-authoritative", () => {
  assert.match(api, /credentials: "include"/);
  assert.match(api, /api\/customer-onboarding/);
  assert.match(api, /requireHostedCheckoutUrl/);
  assert.match(app, /verified webhook/);
  assert.match(app, /Progress saves automatically/);
  assert.match(app, /Set up your first venue/);
  assert.match(app, /Pair your physical display/);
  assert.match(app, /pairing alone does not mean the device is active/);
  assert.match(api, /api\/customer-onboarding\/venue/);
  assert.match(api, /api\/customer-onboarding\/first-screen/);
});

test("entry surface records essential accessible states", () => {
  assert.match(app, /role="status"/);
  assert.match(app, /role="alert"/);
  assert.match(timeline, /aria-label="Customer onboarding timeline"/);
  assert.match(app, /We could not safely load your onboarding yet/);
  assert.match(app, /Refresh onboarding/);
  assert.match(styles, /:focus-visible/);
  assert.match(styles, /@media \(max-width: 820px\)/);
  assert.match(passkey, /navigator\.credentials\.get/);
  assert.match(app, /pattern="\[0-9\]\{6\}"/);
  assert.match(app, /Refresh device status/);
  assert.match(app, /window\.setInterval\(\(\) => void refreshPresence\(\), 10_000\)/);
  assert.match(app, /status updates automatically/);
});

test("customer timeline is ordered, resumable, and server-authoritative", () => {
  assert.match(timeline, /Account/);
  assert.match(timeline, /Plan/);
  assert.match(timeline, /Venue/);
  assert.match(timeline, /First Screen/);
  assert.match(timeline, /Go Live/);
  assert.match(timeline, /aria-current=\{isCurrent \? "step"/);
  assert.match(timeline, /Complete/);
  assert.match(timeline, /Current/);
  assert.match(timeline, /Upcoming/);
  assert.match(timeline, /Last saved/);
  assert.match(timeline, /#onboarding-current-task/);
  assert.match(timeline, /onboarding\.progress/);
  assert.doesNotMatch(timeline, /fetch\(|request\(|onClick/);
});
