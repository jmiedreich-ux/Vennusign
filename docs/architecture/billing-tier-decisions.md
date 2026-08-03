# Billing Tier Decision Safety

Back Office presents plan changes as a decision, never as an inferred entitlement change. The authenticated venue claim remains the boundary for billing data and actions.

## Authoritative decision contract

- `GET /api/back-office/billing/presentation` returns the current tier, active-screen usage, organization-venue usage, public target tiers, target limits, feature losses, and server-evaluated blocking reasons.
- A tier is not selectable when the active screen count or organization venue count exceeds its limit. The checkout and targeted Billing Portal endpoints repeat this evaluation immediately before opening Stripe, so stale browser state cannot bypass the limit.
- Existing paid subscriptions use the Stripe-hosted Billing Portal. First-time paid plans use Stripe-hosted Checkout. Vennusign does not collect payment details.
- A browser return, redirect, or locally persisted pending record never changes access. Stripe webhooks remain authoritative for the subscription and entitlement state.
- The browser stores only a bounded pending tier identifier, display name, and request time. It reports pending, applied, or stale status and offers an authoritative refresh path.
- Hardware as a Service remains a separate commercial contract and persistence path. A software-tier selection cannot silently alter an HaaS term.

## Recovery and safety

The review dialog puts the least-destructive action first, lists the target limits and known feature losses, and leaves all changes uncommitted until the user reviews Stripe's final price, timing, and proration. Closing the dialog, cancelling at Stripe, or returning before a webhook preserves the existing tier. A stale pending decision can be refreshed or reopened without inferring access.

## Authorization and data support

The Back Office venue token selects the venue. Organization usage is derived server-side from that venue's organization membership; clients cannot submit a venue or organization identifier. Target tiers must be active and public. Feature-loss labels are computed by comparing the current and target tier feature matrices.

## Validation boundary

Focused API authorization, decision-evaluator, browser-recovery, accessibility-contract, and production-build checks are non-integration validation. Azure SQL, live Stripe, webhook delivery, credentialed provider, hosted infrastructure, container, physical-device, signing/store, cross-system, and all other integration-type tests remain skipped unless separately authorized.
