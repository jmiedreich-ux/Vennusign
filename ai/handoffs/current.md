# Vennu Session Handoff

## Work Package

- ID: WP-12.04
- Status: In review
- Execution mode: Sequential

## Git State

- Branch: `wp/12.04-unified-pos-webhook-intake`
- Latest commit: pending publication
- Issue: #293
- Pull request: pending
- CI state: WP-12.03 Actions run #620 passed

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

## Files Changed

- Unified POS webhook ingress, verification, queue, worker, dispatcher, migration 037, tests, and Phase 12 records.

## Decisions

- Store only protected credentials and keep connection presentation contracts credential-free.
- Keep signature verification provider-specific in `Vennu.Api`; keep durable queue and dispatch contracts provider-neutral.
- Persist before `202 Accepted`; use the database rather than the process signal as the restart-safe work authority.
- Represent Square variations as existing menu items and retain source ownership outside the menu domain.

## Validation

- Commands: `git diff --check`; `jq empty tracker/assignments.json`; Venue Admin unit tests and production build; source and secret review.
- Results: available local checks pending; authoritative GitHub Actions is pending publication.
- Skipped checks and reason: integration and external-provider tests remain skipped by standing owner instruction.

## Remaining Work

- Validate, review, merge, and record WP-12.04 completion; WP-12.05 follows.

## Known Risks or Blockers

- No blocker. Live Square and integration validation remain deliberately excluded.

## Exact Next Action

- Publish WP-12.04, use GitHub Actions as authority, review the exact head, and merge only when green.

## Do Not Redo or Reverse

- Do not redo the POS connection domain, credential protector, migration 035, or provider-neutral contracts.
- Do not persist plaintext credentials or add provider-specific SDK types to shared contracts.
