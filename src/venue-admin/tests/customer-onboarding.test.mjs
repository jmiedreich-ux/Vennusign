import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const [app, api, passkey, main, styles] = await Promise.all([
  readFile(new URL("../src/CustomerOnboardingApp.tsx", import.meta.url), "utf8"),
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
  assert.match(main, /\/onboarding/);
});

test("onboarding is resumable, credentialed, and webhook-authoritative", () => {
  assert.match(api, /credentials: "include"/);
  assert.match(api, /api\/customer-onboarding/);
  assert.match(api, /requireHostedCheckoutUrl/);
  assert.match(app, /verified webhook/);
  assert.match(app, /Progress saves automatically/);
  assert.match(app, /Venue setup continues in the next release/);
});

test("entry surface records essential accessible states", () => {
  assert.match(app, /role="status"/);
  assert.match(app, /role="alert"/);
  assert.match(app, /aria-label="Onboarding progress"/);
  assert.match(styles, /:focus-visible/);
  assert.match(styles, /@media \(max-width: 820px\)/);
  assert.match(passkey, /navigator\.credentials\.get/);
});
