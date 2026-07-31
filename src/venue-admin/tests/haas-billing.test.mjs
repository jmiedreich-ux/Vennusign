import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const [api, app, card, controller, migration] = await Promise.all([
  readFile(new URL("../src/api.ts", import.meta.url), "utf8"),
  readFile(new URL("../src/App.tsx", import.meta.url), "utf8"),
  readFile(new URL("../src/BillingStatusCard.tsx", import.meta.url), "utf8"),
  readFile(new URL("../../Vennu.Api/Controllers/VenueAdmin/VenueAdminBillingController.cs", import.meta.url), "utf8"),
  readFile(new URL("../../Vennu.Data/Scripts/034_create_haas_contracts.sql", import.meta.url), "utf8")
]);

test("HaaS checkout stays claim-bound and accepts only bundle term metadata", () => {
  assert.match(api, /haas-checkout-session/);
  assert.match(api, /JSON\.stringify\(\{ bundleKey, termMonths \}\)/);
  assert.doesNotMatch(api, /haas-checkout-session[\s\S]{0,500}venueId/);
  assert.match(controller, /CreateHaasCheckoutAsync|CreateHaasCheckoutSession/);
  assert.match(controller, /FindFirstValue\(VenueAdminAuthenticationDefaults\.VenueIdClaim\)/);
});

test("HaaS checkout launches only through the hosted checkout allowlist", () => {
  assert.match(api, /requireHostedCheckoutUrl\(payload\.checkoutUrl\)/);
  assert.match(app, /createHaasCheckoutSession\(/);
  assert.match(app, /window\.location\.assign\(checkoutUrl\)/);
});

test("HaaS disclosure shows remaining term without collecting a buyout", () => {
  assert.match(card, /remainingMonths/);
  assert.match(card, /estimatedBuyoutAmount/);
  assert.match(card, /Disclosure only/);
  assert.match(card, /only after Stripe confirms/);
  assert.doesNotMatch(card, /Pay buyout|Collect buyout/);
});

test("HaaS persistence enforces approved bundle term pairings", () => {
  for (const pair of [
    "BundleKey = 'starter_kit' AND TermMonths = 18",
    "BundleKey = 'bar_pack' AND TermMonths = 24",
    "BundleKey = 'full_house' AND TermMonths = 36"
  ]) assert.match(migration, new RegExp(pair.replace(/[()]/g, "\\$&")));
});
