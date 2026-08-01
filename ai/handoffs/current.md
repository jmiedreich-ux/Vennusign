# Vennu Session Handoff

## Work Package

- ID: WP-12.06
- Status: Available
- Execution mode: Sequential

## Git State

- Branch: pending
- Latest commit: `1edf971032758c79c729bdda3d3fd50cb8235883` (WP-12.05 merge)
- Issue: pending
- Pull request: pending
- CI state: WP-12.05 Actions run #634 passed

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
- Claimed WP-12.05 as issue #296.
- Added merchant-to-venue and venue/provider mapping ownership boundaries.
- Added idempotent Square availability, quantity, and USD price application with existing display notifications.
- Added focused handler and repository tests and reviewed and merged WP-12.05 through PR #297.

## Files Changed

- WP-12.05 completion evidence and the synchronized Phase 12 queue.

## Decisions

- Store only protected credentials and keep connection presentation contracts credential-free.
- Keep signature verification provider-specific in `Vennu.Api`; keep durable queue and dispatch contracts provider-neutral.
- Persist before `202 Accepted`; use the database rather than the process signal as the restart-safe work authority.
- Represent Square variations as existing menu items and retain source ownership outside the menu domain.

## Validation

- Commands: `git diff --check`; `jq empty tracker/assignments.json`; Venue Admin unit tests and production build; source and secret review.
- Results: GitHub Actions run #634 passed restore, Release build, frontend/package checks, migration inventory, and all required unit tests against `019a5985b6cec064582596987bb418188240dfc2`.
- Skipped checks and reason: integration and external-provider tests remain skipped by standing owner instruction.

## Remaining Work

- WP-12.06 — Toast Provider and Webhook Sync.

## Known Risks or Blockers

- No blocker. Live Square and integration validation remain deliberately excluded.

## Exact Next Action

- Claim WP-12.06 and add the bounded Toast provider, connection configuration, and verified webhook sync through shared contracts.

## Do Not Redo or Reverse

- Do not redo the POS connection domain, credential protector, migration 035, or provider-neutral contracts.
- Do not persist plaintext credentials or add provider-specific SDK types to shared contracts.
