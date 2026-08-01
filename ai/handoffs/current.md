# Vennu Session Handoff

## Work Package

- ID: WP-12.03
- Status: In review
- Execution mode: Sequential

## Git State

- Branch: `wp/12.03-square-catalog-import`
- Latest commit: pending publication
- Issue: #290
- Pull request: pending
- CI state: WP-12.02 Actions run #612 passed

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

## Files Changed

- Square catalog transport, importer, durable source mappings, migration 036, endpoint, tests, and Phase 12 records.

## Decisions

- Store only protected credentials and keep connection presentation contracts credential-free.
- Keep provider-specific transport in `Vennu.Api` behind the provider-neutral contracts established in WP-12.01.
- Represent Square variations as existing menu items and retain source ownership outside the menu domain.

## Validation

- Commands: `git diff --check`; `jq empty tracker/assignments.json`; Venue Admin unit tests and production build; source and secret review.
- Results: available local checks passed; authoritative GitHub Actions is pending publication.
- Skipped checks and reason: integration and external-provider tests remain skipped by standing owner instruction.

## Remaining Work

- Validate, review, merge, and record WP-12.03 completion; WP-12.04 follows.

## Known Risks or Blockers

- No blocker. Live Square and integration validation remain deliberately excluded.

## Exact Next Action

- Publish WP-12.03, use GitHub Actions as authority, review the exact head, and merge only when green.

## Do Not Redo or Reverse

- Do not redo the POS connection domain, credential protector, migration 035, or provider-neutral contracts.
- Do not persist plaintext credentials or add provider-specific SDK types to shared contracts.
