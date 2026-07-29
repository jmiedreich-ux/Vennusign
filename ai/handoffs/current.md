# Vennu Session Handoff

## Work Package

- ID: WP-04.10
- Status: Review
- Branch: `wp/04.10-venue-tier-switching`
- Issue: #48
- Pull request: #49

## Completed

- Added Stripe subscription-item tier switching while preserving the current billing interval and applying prorations.
- Added local tier synchronization, entitlement-cache invalidation, normalized upgrade/downgrade events, and compensating restoration.
- Added a protected Super Admin endpoint and responsive tier controls to the venue support view.
- Added focused unit tests and Stripe tier-switching operating guidance.

## Validation

- Admin production build passed locally.
- `git diff --check` passed.
- .NET validation is deferred to GitHub Actions because the SDK is not installed locally.
- Integration-type tests intentionally skipped under the standing repository-owner instruction.

## Exact Next Action

Publish the branch, run the required non-integration GitHub workflow, review the exact green head, merge WP-04.10, then claim WP-04.11.

## Do Not Redo or Reverse

- Preserve the Stripe-first ordering and compensating restoration.
- Preserve the current Stripe billing interval during a tier switch.
- Do not add checkout, customer portal, cancellation, or proration-preview scope.
