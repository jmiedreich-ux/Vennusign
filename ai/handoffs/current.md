# Vennu Session Handoff

## Work Package

- ID: WP-12.04
- Status: Available
- Execution mode: Sequential

## Git State

- Branch: pending
- Latest commit: `b8bd579c9459e1d4f2e3f7b68472692810a8502b` (WP-12.03 merge)
- Issue: pending
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

## Files Changed

- WP-12.03 completion evidence and the synchronized Phase 12 queue.

## Decisions

- Store only protected credentials and keep connection presentation contracts credential-free.
- Keep provider-specific transport in `Vennu.Api` behind the provider-neutral contracts established in WP-12.01.
- Represent Square variations as existing menu items and retain source ownership outside the menu domain.

## Validation

- Commands: `git diff --check`; `jq empty tracker/assignments.json`; Venue Admin unit tests and production build; source and secret review.
- Results: GitHub Actions run #620 passed restore, Release build, frontend/package checks, migration inventory, and all required unit tests against `8d53b75e7e2f348fe395fb6a0a25497d2883107e`.
- Skipped checks and reason: integration and external-provider tests remain skipped by standing owner instruction.

## Remaining Work

- WP-12.04 — Unified POS Webhook Intake and Idempotency.

## Known Risks or Blockers

- No blocker. Live Square and integration validation remain deliberately excluded.

## Exact Next Action

- Claim WP-12.04 and implement provider-neutral verified webhook intake and durable event idempotency without applying inventory changes.

## Do Not Redo or Reverse

- Do not redo the POS connection domain, credential protector, migration 035, or provider-neutral contracts.
- Do not persist plaintext credentials or add provider-specific SDK types to shared contracts.
