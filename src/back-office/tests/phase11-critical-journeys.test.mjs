import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const read = path => readFile(new URL(path, import.meta.url), "utf8");
const [app, navigation, prompts, sidebar, modal, checkout, portal, billing, haas, controller, webhook, migration, checkoutRequest, haasRequest] = await Promise.all([
  read("../src/App.tsx"), read("../src/navigation.mjs"), read("../src/upgradeExperience.mjs"),
  read("../src/SidebarUpgradeNudge.tsx"), read("../src/UpgradeSheet.tsx"), read("../src/checkoutFlow.mjs"),
  read("../src/billingPortal.mjs"), read("../src/BillingStatusCard.tsx"), read("../tests/haas-billing.test.mjs"),
  read("../../Vennu.Api/Controllers/BackOffice/BackOfficeBillingController.cs"),
  read("../../Vennu.Api/Controllers/StripeWebhooksController.cs"),
  read("../../Vennu.Data/Scripts/034_create_haas_contracts.sql"),
  read("../../Vennu.Api/Contracts/BackOffice/CreateCheckoutSessionRequest.cs"),
  read("../../Vennu.Api/Contracts/BackOffice/CreateHaasCheckoutSessionRequest.cs")
]);

test("upgrade prompts remain deterministic dismissible and non-blocking", () => {
  assert.match(prompts, /listUpgradeOpportunities/);
  assert.match(prompts, /sessionStorage/);
  assert.match(app, /inlineOpportunity/);
  assert.match(app, /lockedOpportunity/);
  assert.match(app, /dismissUpgradeFeature/);
  assert.match(navigation, /upgradeFeature/);
  assert.match(sidebar, /7_000/);
  assert.match(sidebar, /prefers-reduced-motion/);
});

test("the upgrade journey remains one sheet and one hosted Checkout CTA", () => {
  assert.match(modal, /role="dialog"/);
  assert.match(modal, /Maybe later/);
  assert.match(modal, /Monthly/);
  assert.match(modal, /Annual/);
  assert.match(app, /createCheckoutSession\(/);
  assert.match(checkout, /checkout\.stripe\.com/);
  assert.match(checkout, /success|cancel/);
  assert.doesNotMatch(checkout, /entitlement|featureSet|unlock/i);
});

test("billing management remains claim-bound hosted and webhook-authoritative", () => {
  assert.match(controller, /VenueIdClaim/);
  assert.match(controller, /portal-session/);
  assert.match(portal, /billing\.stripe\.com/);
  assert.match(portal, /trialing/);
  assert.match(portal, /past_due/);
  assert.match(portal, /canceled/);
  assert.match(billing, /subscriptionStatusCopy/);
  assert.match(webhook, /StripeHaasWebhookEventMapper/);
  assert.doesNotMatch(`${checkoutRequest}\n${haasRequest}`, /Stripe|VenueId|CustomerId/);
});

test("HaaS guardrails retain fixed terms separate persistence and disclosure-only buyout", () => {
  assert.match(haas, /claim-bound/);
  assert.match(haas, /Disclosure only/);
  assert.match(migration, /CREATE TABLE dbo\.HaasContracts/);
  for (const term of [18, 24, 36]) assert.match(migration, new RegExp(`TermMonths = ${term}`));
  assert.doesNotMatch(app, /collectBuyout|chargeBuyout|payBuyout/i);
});
