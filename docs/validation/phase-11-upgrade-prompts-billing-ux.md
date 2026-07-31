# Phase 11 — Upgrade Prompts & Billing UX Validation

## Result

Phase 11 is ready for closure when the WP-11.10 GitHub Actions run passes. Integration-type and live Stripe tests remain intentionally skipped under the standing repository-owner instruction.

## Acceptance Matrix

| Journey | Evidence |
| --- | --- |
| Canonical benefit catalog, tier badges, deterministic opportunity selection, and session dismissal | `upgrade-experience-foundation.test.mjs`, `billing-presentation.test.mjs` |
| Visible locked navigation and previews without blocking unlocked workflows | `navigation-shell.test.mjs`, `venue-operations.test.mjs`, Phase 11 critical journeys |
| One contextual prompt per screen and deterministic prompt coordination | `billing-presentation.test.mjs`, Phase 11 critical journeys |
| Quiet rotating sidebar nudge, dismissal, progress, and reduced motion | `billing-presentation.test.mjs`, Phase 11 critical journeys |
| Upgrade modal benefit, tier value, monthly/annual choice, single Checkout CTA, and easy exit | `billing-presentation.test.mjs`, Phase 11 critical journeys |
| Claim-bound Stripe Checkout and browser/API hosted-origin allowlists | `CheckoutSessionServiceTests`, `VenueAdminBillingControllerTests`, `checkout-flow.test.mjs`, Phase 11 critical journeys |
| Bounded success/cancel return and no optimistic entitlement | `checkout-flow.test.mjs`, Phase 11 critical journeys |
| Claim-bound Billing Portal plus active/trial/past-due/canceled/period-end guidance | `BillingPortalSessionServiceTests`, `billing-portal.test.mjs`, Phase 11 critical journeys |
| Webhook-authoritative subscription/end-of-period state | `StripeSubscriptionEventHandlerTests`, `StripeWebhookEventMapperTests` |
| Separate HaaS contract persistence and approved 18/24/36-month pairings | `HaasBillingServiceTests`, migration resource tests, `haas-billing.test.mjs` |
| Confirmed-event HaaS lifecycle and disclosure-only remaining-term estimate | `HaasContractSubscriptionEventHandlerTests`, `haas-billing.test.mjs`, Phase 11 critical journeys |
| No custom card UI, browser Stripe IDs, raw provider payloads, or automatic buyout collection | controller/service/frontend contract tests and reviewed Phase 11 diffs |

## Required Validation

- Dependency restore and complete Release build.
- Super Admin, Venue Admin, and display production builds and frontend tests.
- TV package validation retained by the repository workflow.
- All non-integration unit tests.
- Migration inventory validation through migration 034.
- GitHub Actions review of the exact PR head.

## Explicitly Skipped

- Azure SQL and all other integration tests.
- Live Stripe Checkout, Billing Portal, webhook delivery, invoice, and charge tests.
- Tests requiring external services, credentials, hosted infrastructure, containers, or cross-system integration.

## Boundaries

This closure package adds consolidated validation evidence only. It does not add billing behavior, custom payment handling, external integration infrastructure, automated HaaS buyout collection, or Phase 12 POS functionality.
