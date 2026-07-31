# Vennu Session Handoff

## Work Package

- ID: WP-11.08
- Status: Ready for review
- Execution mode: Sequential

## Git State

- Branch: `wp/11.08-billing-portal-status`
- Latest commit: pending
- Issue: #273
- Pull request: pending publication
- CI state: GitHub Actions pending publication

## Completed This Session

- Added claim-bound Stripe Billing Portal session creation without accepting venue, customer, or subscription identifiers from the browser.
- Added server and browser allowlisting for the Stripe-hosted Billing Portal and a server-configured HTTPS return URL.
- Exposed authoritative subscription status, trial/current-period dates, and scheduled end-of-period state without exposing Stripe identifiers.
- Added Venue Admin billing guidance and accessible portal pending/error states.
- Persisted Stripe `cancel_at_period_end` changes through the existing idempotent webhook path and migration 033.

## Files Changed

- Billing Portal API options, gateway, service contract, controller response, and dependency registration.
- `VenueSubscription`, Stripe webhook mapping/handling, and migration `033_add_subscription_period_end_state.sql`.
- Venue Admin billing route, status card, portal client, API client, styles, and tests.
- Focused API/data unit tests, migration inventory, and WP/status/tracker/handoff records.

## Decisions

- Resolve the Stripe customer server-side from the claim-bound venue subscription rather than accepting any Stripe identifier from Venue Admin.
- Permit only absolute HTTPS `billing.stripe.com` session URLs at both API and browser boundaries.
- Treat webhook state as authoritative; the portal launch does not mutate subscription or entitlement state.

## Validation

- Commands: `npm test`; `npm run build`; `git diff --check`; `jq empty tracker/assignments.json`.
- Results: 24 Venue Admin tests and the Venue Admin production build passed locally; diff and tracker checks passed.
- Skipped checks and reason: local .NET tooling is unavailable and GitHub Actions is authoritative. All integration-type and external Stripe tests are skipped by standing owner instruction.

## Remaining Work

- Publish the exact branch, run required GitHub Actions checks, complete ChatGPT review, and merge.
- Then continue with WP-11.09 — HaaS contract lifecycle.

## Known Risks or Blockers

- No known blocker. Live Stripe behavior is intentionally outside this package and covered by the standing integration-test exception.

## Exact Next Action

- Commit and publish WP-11.08, open its pull request, and inspect the exact-head GitHub Actions result.

## Do Not Redo or Reverse

- Do not accept venue or Stripe identifiers from the Billing Portal request.
- Do not add custom payment-method UI or HaaS contract behavior assigned to WP-11.09.
