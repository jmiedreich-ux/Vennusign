# Vennu Session Handoff

## Work Package

- ID: WP-13.04
- Status: Complete in proposed merge state
- Execution mode: Sequential

## Git State

- Branch: `wp/13.04-tier-trials-entitlements`
- Implementation head: `8d5c5740774e4bad8b70da8e3f059480bb760259`
- Issue: #347
- Pull request: #349
- CI state: affected-area Actions #751 passed; final completion-record head validation and ChatGPT review remain required before merge.

## Completed This Session

- Moved authoritative Stripe customer/subscription and tier entitlement ownership to organizations.
- Added organization trials before venue creation, first paid Checkout without a pre-existing subscription, and tier-defined venue/screen enforcement.
- Added organization Checkout/subscription metadata, Stripe customer reuse, webhook mapping and legacy venue-metadata promotion.
- Added migration 044 with conservative unambiguous backfill and RepoDb mapping.
- Added organization-first feature, billing portal and tier-switch behavior plus synchronized legacy venue projections that retain the unique Stripe ID on only one projection.
- Added focused regression tests and synchronized project records.

## Files Changed

- Organization subscription model, repository, migration, management and projection services.
- Checkout, webhook, feature, entitlement, provisioning, membership, billing portal and tier-switch paths.
- Focused API/data-access tests and Phase 13 architecture/work-package/status/tracker/handoff records.

## Decisions

- Organization subscription state is authoritative for linked organizations.
- Checkout return state never grants access; verified Stripe webhook state remains authoritative.
- Migration 044 backfills only organizations with one legacy venue subscription; ambiguous histories require explicit reconciliation.
- Venue subscriptions remain compatibility projections and fallback only for venues without organization entitlement.

## Validation

- GitHub Actions #751: API and data-access Release builds/tests, migration/document checks and stable gate passed.
- Frontend, Android TV, Tizen and webOS jobs: correctly skipped as unaffected.
- Integration, live Stripe, Azure SQL, credentialed, hosted-infrastructure, container and cross-system tests: skipped under the standing owner instruction.

## Remaining Work

- Final completion-record head Actions and ChatGPT review/approval.
- Merge PR #349, close issue #347 and delete the package branch.

## Known Risks or Blockers

- Organizations with multiple historical venue subscriptions are intentionally not auto-consolidated and require explicit reconciliation before organization billing changes.
- Live Stripe and Azure SQL behavior is intentionally unvalidated under the standing integration-test exception.

## Exact Next Action

- Validate and approve the final PR #349 head, merge it, then claim WP-13.05 sequentially only if no competing owner exists.

## Do Not Redo or Reverse

- Do not restore venue-scoped Stripe ownership, grant access from Checkout return state, assign one Stripe subscription ID to multiple legacy rows, or start Phase 14+.
- Do not re-track local integration settings or run integration tests in ordinary WP CI.
