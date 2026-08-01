# Vennu Session Handoff

## Work Package
- ID: WP-13.04
- Status: Blocked
- Execution mode: Sequential

## Git State
- Branch: `wp/13.04-tier-trials-entitlements`
- Issue: #347
- Pull request: #349 (draft)
- CI: Actions #734 and #735 passed; merge blocked by final architecture review

## Completed
- Verified WP-13.03 merged and WP-13.04 had no competing owner before claiming.
- Added tier-defined trial/expiry/venue policy, removed hardcoded production trial duration, enforced active/unexpired subscription and screen capacity on screen creation, migration 043, tests and architecture records.
- Preserved the existing webhook-authoritative paid activation and feature-resolution pipeline.

## Validation
GitHub Actions #734 and #735 passed. Integration, live Stripe, Azure SQL, hosted infrastructure and cross-system tests are skipped.

## Material Blocker
Existing Stripe/subscription ownership is venue-scoped, while Phase 13 requires plan/trial selection before venue creation and tier-level maximum venue enforcement. Decide and document the authoritative organization commercial-entitlement model, Stripe customer/subscription ownership, migration/compatibility with VenueSubscriptions, and webhook mapping before completing this WP.

## Exact Next Action
Approve the organization-entitlement architecture, implement it on PR #349, rerun Actions, and re-review. Do not start WP-13.05.

## Do Not Redo
Do not grant paid access from Checkout return state, hardcode trial policy, bypass screen/feature limits, or begin WP-13.05 before merge.
