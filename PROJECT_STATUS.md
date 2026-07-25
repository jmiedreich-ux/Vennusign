# Vennu Project Status

## Current Phase

**Phase 03 — Tier System & Feature Flags**

## Milestone

Monetization infrastructure supports persistent subscription tiers, feature entitlements, venue subscriptions, feature resolution, and later Stripe synchronization.

## Completed

- Phase 02 — Core Backend and Real-Time Engine
- WP-02.08 through WP-02.14

## Active Work Package

**WP-03.01 — Feature and Tier Core Models**

Status: Complete pending PR CI, review, and merge.

## WP-03.01 Completion Evidence

- Added core models for features, subscription tiers, tier-feature mappings, and venue subscriptions.
- Added the Phase 03 DbUp schema with constraints and indexes.
- Seeded Starter, Restaurant Starter, Pro, and Business using roadmap pricing and screen limits.
- Seeded the initial feature catalog and tier mappings.
- Added repositories and dependency injection registration.
- Added repository tests for feature lookup, tier mappings, and venue subscription persistence.

## Blockers

None currently recorded.

## Next Package

**WP-03.02 — Feature Resolution Engine**

Feature resolution, venue overrides, caching, usage limits, Stripe integration, and API enforcement are intentionally outside WP-03.01.
