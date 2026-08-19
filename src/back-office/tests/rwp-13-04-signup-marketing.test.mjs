import test from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const [app, marketing, styles] = await Promise.all([
  readFile(new URL("../src/CustomerOnboardingApp.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/SignupMarketingExperience.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/styles.css", import.meta.url), "utf8")
]);

test("signup and sign-in are a unified entry page; marketing experience lives on the home page app instead", () => {
  assert.doesNotMatch(app, /<SignupMarketingExperience/);
  assert.doesNotMatch(app, /import SignupMarketingExperience/);
  assert.match(app, /id="signup-auth-card"/);
});

test("marketing experience covers interactive demo, proof, pricing, and pairing", () => {
  assert.match(marketing, /useState/);
  assert.match(marketing, /Interactive product preview/);
  assert.match(marketing, /aria-pressed/);
  assert.match(marketing, /aria-live="polite"/);
  assert.match(marketing, /Product proof points/);
  assert.match(marketing, /Public pricing/);
  assert.match(marketing, /Pair once\. Know when it is live/);
  assert.match(marketing, /Online heartbeat/);
});

test("preview is explicit, non-authoritative, responsive, and reduced-motion compatible", () => {
  assert.match(marketing, /Preview only · no venue data is changed/);
  assert.match(marketing, /cannot start a trial or subscription/);
  assert.doesNotMatch(marketing, /fetch\(|localStorage|sessionStorage/);
  assert.match(styles, /\.signup-demo__tabs button\[aria-pressed="true"\]/);
  assert.match(styles, /\.signup-marketing__proof, \.signup-pairing-story ol \{ grid-template-columns: 1fr/);
  assert.match(styles, /@media \(prefers-reduced-motion: reduce\)/);
});
