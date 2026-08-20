import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const [app, showcase, timeline, api, passkey, passkeyManagement, security, navigation, main, styles, loader] = await Promise.all([
  readFile(new URL("../src/CustomerOnboardingApp.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/TemplateShowcase.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/CustomerOnboardingTimeline.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/customerOnboardingApi.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/passkeySignIn.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/passkeyManagement.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/AccountSecurity.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/navigation.mjs", import.meta.url), "utf8"),
  readFile(new URL("../src/main.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/styles.css", import.meta.url), "utf8"),
  readFile(new URL("../src/VennusignLoader.tsx", import.meta.url), "utf8")
]);

test("account security exposes discoverable passkey lifecycle and recovery states", () => {
  assert.match(navigation, /path: "security"/);
  assert.match(security, /Add a passkey/);
  assert.match(security, /Remove passkey/);
  assert.match(security, /useDestructiveReview/);
  assert.match(security, /role="status"/);
  assert.match(security, /role="alert"/);
  assert.match(passkeyManagement, /navigator\.credentials\.create/);
  assert.match(passkeyManagement, /Recent authentication is required/);
  assert.match(passkey, /canceled or timed out/);
});

test("public entry is a unified landing page with no marketing content", () => {
  assert.match(app, /Continue with Google/);
  assert.match(app, /Continue with Vennusign/);
  assert.doesNotMatch(app, /Apple/);
  assert.doesNotMatch(app, /Use a passkey/);
  assert.doesNotMatch(app, /Email me a sign-in link/);
  assert.doesNotMatch(app, /SignupMarketingExperience/);
  assert.match(app, /customer-landing/);
  assert.match(app, /customer-landing__divider/);
  assert.match(app, /TemplateShowcase/);
  assert.match(showcase, /prefers-reduced-motion/);
  assert.match(showcase, /Daily Specials/);
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
  for (const field of ["Legal business name", "Primary contact name", "Contact email", "Contact phone", "Business mailing address"]) assert.match(app, new RegExp(field));
  assert.match(api, /primaryContactName/);
  assert.match(api, /mailingAddress/);
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
  assert.match(app, /Open Back Office/);
  assert.match(app, /Back Office rechecks your organization membership/);
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

test("returning visitor sees one remembered method by default, not every option at once", () => {
  assert.match(app, /REMEMBERED_METHOD_KEY/);
  assert.match(app, /localStorage\.getItem\(REMEMBERED_METHOD_KEY\)/);
  assert.match(app, /localStorage\.setItem\(REMEMBERED_METHOD_KEY, method\)/);
  assert.match(app, /session\.authenticationMethod/);
  assert.match(app, /Continue as you did last time/);
  assert.match(app, /More ways to sign in/);
  assert.match(app, /rememberedMethod && KNOWN_METHODS\.has\(rememberedMethod\) && !showAllMethods/);
  // Storage failures (private browsing, disabled storage) fall back to showing every
  // option, the same as a first visit - never a broken or empty screen.
  assert.match(app, /catch \{\s*\/\/ Private browsing or storage disabled/);
});

test("go live is a recorded achievement, and device status is reported separately", () => {
  // The panel must not be driven by firstScreenStatus: a display that has gone live and is
  // currently offline is a complete setup with an offline device, not an unfinished checklist.
  assert.match(app, /customer-onboarding__go-live \$\{onboarding\.progress\.goLive \? "is-online" : "is-waiting"\}/);
  assert.match(app, /\{onboarding\.progress\.goLive \? <div className="customer-onboarding__celebration"/);
  assert.match(app, /Your setup is complete/);
  assert.match(app, /Live since/);
  assert.match(app, /onboarding\.goLiveAchievedUtc \? <div><dt>Live since<\/dt>/);
  // The device row still tells the truth about right now.
  assert.match(app, /<dt>Device<\/dt><dd>\{onboarding\.firstScreenStatus === "online" \? "Online" : "Offline \/ waiting"\}/);
  assert.match(api, /goLiveAchievedUtc\?: string;/);
});

test("one loader is used everywhere, and its motion is optional", () => {
  // Every waiting state is the same screen waking up, so the product does not
  // present four different ideas of what "loading" looks like.
  assert.match(app, /<VennusignLoader variant="modal"/);
  assert.match(loader, /variant === "modal"/);

  // Motion is decoration. Stopped, the screen must still read as a full board
  // rather than an empty rectangle, so the reduced-motion rule restores the rows.
  const reduced = styles.slice(styles.indexOf("prefers-reduced-motion", styles.indexOf(".vennu-loader")));
  assert.match(reduced, /\.vennu-loader__screen[^}]*animation: none/);
  assert.match(reduced, /\.vennu-loader__row[^}]*opacity: 1/);

  // The sentence is the message; the animation is hidden from assistive tech.
  assert.match(loader, /role="status"/);
  assert.match(loader, /aria-live="polite"/);
  assert.match(loader, /aria-busy="true"/);
  assert.match(loader, /vennu-loader__art" aria-hidden="true"/);
});

