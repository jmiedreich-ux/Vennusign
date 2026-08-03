import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";
import {
  checkoutRefreshDelays,
  readCheckoutReturnState,
  requireHostedCheckoutUrl,
  stripCheckoutReturnParameter
} from "../src/checkoutFlow.mjs";

const [app, api, modal] = await Promise.all([
  readFile(new URL("../src/App.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/api.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/UpgradeModal.tsx", import.meta.url), "utf8")
]);

test("accepts only bounded checkout return states", () => {
  assert.equal(readCheckoutReturnState("?checkout=success"), "success");
  assert.equal(readCheckoutReturnState("?checkout=cancel"), "cancel");
  assert.equal(readCheckoutReturnState("?checkout=paid"), undefined);
  assert.equal(readCheckoutReturnState("?checkout=%3Cscript%3E"), undefined);
  assert.equal(stripCheckoutReturnParameter("?checkout=success&source=upgrade"), "?source=upgrade");
});

test("allows only the Stripe-hosted checkout origin", () => {
  assert.equal(
    requireHostedCheckoutUrl("https://checkout.stripe.com/c/pay/test"),
    "https://checkout.stripe.com/c/pay/test"
  );
  assert.throws(() => requireHostedCheckoutUrl("http://checkout.stripe.com/c/pay/test"));
  assert.throws(() => requireHostedCheckoutUrl("https://checkout.stripe.com.example.test/pay"));
});

test("launches checkout with pending and error states", () => {
  assert.match(api, /api\/back-office\/billing\/checkout-session/);
  assert.match(api, /requireHostedCheckoutUrl\(payload\.checkoutUrl\)/);
  assert.match(app, /createCheckoutSession\(/);
  assert.match(app, /window\.location\.assign\(checkoutUrl\)/);
  assert.match(modal, /disabled=\{isSubmitting\}/);
  assert.match(modal, /role="alert"/);
});

test("refreshes authoritative state after success without optimistic entitlements", () => {
  assert.deepEqual(checkoutRefreshDelays, [750, 2000, 5000]);
  assert.match(app, /loadBackOfficeSession\(configuration, accessToken, controller\.signal\)/);
  assert.match(app, /loadVenueBillingPresentation\(configuration, accessToken, controller\.signal\)/);
  assert.match(app, /Stripe webhooks remain authoritative/);
  assert.doesNotMatch(app, /setSession\([^)]*capabilities:/);
  assert.doesNotMatch(app, /setBilling\([^)]*currentTier:/);
});
