import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";
import { requireHostedBillingPortalUrl, subscriptionStatusCopy } from "../src/billingPortal.mjs";

const [app, api, card] = await Promise.all([
  readFile(new URL("../src/App.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/BillingStatusCard.tsx", import.meta.url), "utf8")
]);

test("allows only Stripe-hosted billing management", () => {
  assert.equal(
    requireHostedBillingPortalUrl("https://billing.stripe.com/p/session/test"),
    "https://billing.stripe.com/p/session/test"
  );
  assert.throws(() => requireHostedBillingPortalUrl("http://billing.stripe.com/p/session/test"));
  assert.throws(() => requireHostedBillingPortalUrl("https://billing.stripe.com.example.test/session"));
  assert.match(api, /api\/venue-admin\/billing\/portal-session/);
  assert.doesNotMatch(api, /portal-session.*venueId/);
});

test("presents authoritative subscription states", () => {
  assert.equal(subscriptionStatusCopy({ status: "trialing", trialEndsAt: "2026-08-14", cancelAtPeriodEnd: false }).title, "Trial active");
  assert.equal(subscriptionStatusCopy({ status: "past_due", cancelAtPeriodEnd: false }).title, "Payment needs attention");
  assert.equal(subscriptionStatusCopy({ status: "canceled", cancelAtPeriodEnd: false }).title, "Subscription ended");
  assert.equal(subscriptionStatusCopy({ status: "active", currentPeriodEnd: "2026-09-01", cancelAtPeriodEnd: true }).title, "Plan change scheduled");
});

test("launches hosted portal with pending and error states", () => {
  assert.match(app, /createBillingPortalSession\(configuration, accessToken\)/);
  assert.match(app, /window\.location\.assign\(portalUrl\)/);
  assert.match(card, /disabled=\{isOpening\}/);
  assert.match(card, /role="alert"/);
  assert.match(card, /Vennu does not collect card details/);
});
