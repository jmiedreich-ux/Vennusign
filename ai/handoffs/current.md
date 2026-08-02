# Vennu Session Handoff

## Work Package

- ID: WP-13.04
- Status: In Progress
- Execution mode: Sequential

## Git State

- Branch: `wp/13.04-tier-trials-entitlements`
- Issue: #347
- Pull request: #349 (draft)
- CI: Actions #734 and #735 passed the earlier venue-policy implementation; new exact-head validation is required after organization ownership is implemented.

## Completed

- Verified WP-13.03 merged and resumed the existing WP-13.04 Sequential claim.
- Added tier-defined trial/expiry/venue policy, removed hardcoded production trial duration, enforced active/unexpired subscription and screen capacity on screen creation, migration 043, tests and architecture records.
- Preserved webhook-authoritative paid activation; Checkout return state does not grant access.
- Incorporated the merged security and integration-test baseline from `master` without changing the standing integration-test skip for ordinary WP execution.
- Recorded the owner's decision that Stripe customer and subscription ownership belongs to the organization.

## Validation

Actions #734 and #735 passed the earlier branch head. Integration, live Stripe, Azure SQL, hosted infrastructure and cross-system tests are skipped. Affected-area validation is pending for the resumed implementation.

## Remaining Work

- Add authoritative organization subscription persistence and repository behavior.
- Update trial, Checkout, webhook, feature, screen and venue-limit paths with safe legacy venue-subscription compatibility.
- Add migration/backfill, focused tests, completion records, exact-head Actions evidence and ChatGPT review.

## Concurrent Program

INT-TESTING-001 remains active in Collaborative mode on issue #354. This Sequential WP does not run or extend Azure SQL integration coverage.

## Exact Next Action

Implement the approved organization-entitlement architecture on PR #349, run affected-area Actions, and re-review. Do not start WP-13.05.

## Do Not Redo

- Do not grant paid access from Checkout return state, hardcode trial policy, bypass screen/feature limits, or begin WP-13.05 before merge.
- Do not re-track local integration settings or run Azure SQL integration tests in this Sequential WP.
