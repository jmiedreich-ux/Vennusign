# Vennu Project Status

## Current Phase

**Phase 03 — Tier System & Feature Flags**

## Milestone

Monetization infrastructure supports persistent subscription tiers, effective feature resolution, venue overrides, usage limits, and later Stripe synchronization.

## Completed

- Phase 02 — Core Backend and Real-Time Engine
- WP-02.08 through WP-02.14
- WP-03.01 — Feature and Tier Core Models
- WP-03.02 — Feature Resolution Engine
- WP-03.03 — Subscription Management
- WP-03.04 — Usage Metering
- WP-03.05 — Stripe Billing Catalog

## Active Work Package

None pending WP-03.05 pull-request validation, review, and merge.

## WP-03.05 Progress

- Added persistent Stripe product, monthly price, and annual price identifiers.
- Added validated cross-tier uniqueness checks and repository lookups.
- Added active public billing-catalog projection with annual two-month-free pricing.
- Added focused unit coverage.

## Standing Validation Exception

- Integration-type tests are skipped for every AWP under the repository owner's standing instruction.
- Restore, Release build, display production build, unit tests, and applicable non-integration validation remain required.

## Next Package

**WP-03.06 — Stripe Event Idempotency**

WP-03.06 is the next unfinished package after WP-03.05 merges.
