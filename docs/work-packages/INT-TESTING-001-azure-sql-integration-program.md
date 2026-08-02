# INT-TESTING-001 — Azure SQL Integration Testing Program

## Status

In Progress

## Execution Mode

Collaborative only

## Issue and Branch

- Issue: #354
- Branch: `issue/354-int-testing-001`

## Operating Rules

- Only Collaborative agents may execute Azure SQL integration tests under this program.
- Azure SQL integration tests remain skipped and non-blocking in normal local and GitHub Actions validation.
- Every database run uses the approved development database only; no credentials are recorded in repository files, issues, PRs, logs, or test traces.
- Each coverage domain becomes a separately approved, claimed implementation package.

## INT-TESTING-001.01 — Harness and Baseline

- Serialize schema initialization across parallel fixtures.
- Maintain the embedded migration inventory.
- Prove a clean schema migrates and the current Azure SQL suite executes against it.
- Preserve migration history during normal data cleanup.

## Planned Coverage Packages

1. Customer identity, organizations, and memberships.
2. Subscriptions, tiers, entitlements, usage, and commercial idempotency.
3. Menu, content, themes, screen configuration, and presentation persistence.
4. Scheduling, promotions, playlists, and emergency broadcast persistence.
5. POS connections, catalog mappings, webhooks, and synchronization state.
6. Operational events, audit records, and support read models.

## Current Findings

- Clean-schema migration initialization previously raced between parallel fixtures; the harness now serializes initialization in-process.
- Migration `042_add_customer_strong_authentication.sql` must remain in the migration inventory assertion.
- The full current suite passed against a rebuilt Azure SQL development schema: 17 passed, 0 failed, 0 skipped.
- Screen platform nullability is deferred to design-question issue #355.

## Acceptance Criteria

- The baseline suite is repeatable against a clean development schema.
- Integration execution is collaborative-only and explicitly non-blocking in ordinary CI.
- Each expansion domain has a bounded issue/package before implementation.
