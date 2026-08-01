# Vennu Session Handoff

## Work Package

- ID: WP-12.05
- Status: In progress
- Execution mode: Sequential

## Git State

- Branch: `wp/12.05-square-inventory-price-sync`
- Latest commit: `6ef18f6c04f7ee301e5871a9d64c808227258fc2` (WP-12.04 merge)
- Issue: #296
- Pull request: pending
- CI state: WP-12.04 Actions run #626 passed

## Completed This Session

- Merged the Phase 12 bounded plan in PR #283 after Actions run #601 passed.
- Claimed WP-12.01 as issue #284.
- Added explicit Square, Toast, and Clover provider/status domain values.
- Added venue-scoped POS connection persistence with one connection per venue/provider and migration 035.
- Added a service boundary that protects credentials before persistence and returns credential-free summaries.
- Added the ASP.NET Core Data Protection implementation and provider-neutral catalog/inventory contracts.
- Added focused repository, service, protector, and migration tests.
- Reviewed and merged WP-12.01 through PR #285.
- Added the Square OAuth connect, callback, credential persistence, status, and revoke-before-delete flow.
- Added protected single-use state and allowlisted server-owned provider/return URLs.
- Reviewed and merged WP-12.02 through PR #288.
- Claimed WP-12.03 as issue #290.
- Added an injectable paged Square Catalog gateway and provider-neutral adapter.
- Added venue/provider-scoped catalog mappings with migration 036.
- Added an idempotent importer that reuses the existing menu domain and reports deterministic conflicts.
- Added a claim-bound Venue Admin import endpoint and focused non-integration tests.
- Reviewed and merged WP-12.03 through PR #291.
- Claimed WP-12.04 as issue #293.
- Added bounded Square signature verification and provider-neutral verified webhook envelopes.
- Added migration 037 and a durable provider/event-ID deduplicated work queue.
- Added oldest-first claims, stale lease recovery, retry state, an in-process worker signal, and provider-neutral dispatch.
- Added focused verifier, controller, repository, dispatcher, and migration tests without a menu mutation handler.
- Reviewed and merged WP-12.04 through PR #294.

## Files Changed

- Square realtime handler, venue/provider lookup boundaries, focused tests, architecture, and WP-12.05 tracking.

## Decisions

- Store only protected credentials and keep connection presentation contracts credential-free.
- Keep signature verification provider-specific in `Vennu.Api`; keep durable queue and dispatch contracts provider-neutral.
- Persist before `202 Accepted`; use the database rather than the process signal as the restart-safe work authority.
- Represent Square variations as existing menu items and retain source ownership outside the menu domain.

## Validation

- Commands: `git diff --check`; `jq empty tracker/assignments.json`; Venue Admin unit tests and production build; source and secret review.
- Results: GitHub Actions run #626 passed restore, Release build, frontend/package checks, migration inventory, and all required unit tests against `1be0ee770fc5844c7b487fee1139d781ae6b0e70`.
- Skipped checks and reason: integration and external-provider tests remain skipped by standing owner instruction.

## Remaining Work

- WP-12.05 — Square Inventory and Price Realtime Sync.

## Known Risks or Blockers

- No blocker. Live Square and integration validation remain deliberately excluded.

## Exact Next Action

- Publish WP-12.05, validate the exact head through GitHub Actions, review its full diff, and merge if green.

## Do Not Redo or Reverse

- Do not redo the POS connection domain, credential protector, migration 035, or provider-neutral contracts.
- Do not persist plaintext credentials or add provider-specific SDK types to shared contracts.
