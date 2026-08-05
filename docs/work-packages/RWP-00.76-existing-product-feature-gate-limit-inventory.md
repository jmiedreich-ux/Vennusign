# RWP-00.76 — Existing Product Feature, Gate & Limit Inventory

## Issue

#552

## Mode and status

- Execution mode: Sequential
- Scope: documentation and repository analysis only
- Dependency: RWP-00.75 merged, closed, verified, and released
- Result: complete in proposed merge state

## Objective

Inventory the current repository implementation for feature keys, entitlement checks, permissions, support overrides, quantity and usage limits, locked UI surfaces, rollout/configuration controls, authority, scope, and known consumers before reconciliation against the normalized Track 0 model.

## Durable output

`track0/consolidation/EXISTING_PRODUCT_FEATURE_GATE_LIMIT_INVENTORY.md`

## Delivered

- complete seeded feature-key catalog, including initial tier assignments and the later `video_wall` key;
- organization-first commercial authority and legacy venue fallback;
- exact feature-resolution precedence, cache behavior, and master-switch handling;
- direct runtime `HasFeatureAsync` consumers and write/read behavior;
- venue support override authority, scope, expiry, audit, and resolution behavior;
- screen, venue, and monthly feature-usage limits;
- Back Office, Platform Operations, system-configuration, and secret-value authorization boundaries;
- feature-matrix administration and audit behavior;
- locked, preview, hint, nudge, billing, and tier-decision UI surfaces;
- generic system configuration as the closest implemented rollout-like control family;
- factual unknowns and inconsistencies reserved for RWP-00.77 reconciliation.

## Acceptance review

- Existing feature keys and initial assignments are enumerated.
- Current checks, overrides, limits, permissions, locked surfaces, and configuration controls identify source locations, behavior, authority, scope, and consumers.
- Findings distinguish direct enforcement from presentation-only or no-direct-consumer results.
- No live feature, entitlement, permission, override, limit, configuration, rollout, UI, API, schema, or billing behavior changed.
- RWP-13.06 and Phase 14+ remain paused.
- The handoff identifies RWP-00.77.

## Impeccable review

Locked and upgrade surfaces were reviewed for hierarchy, state truthfulness, accessible naming, action clarity, and recovery. The current implementation provides accessible lock actions and personalized previews for selected keys, but a universal reason/state contract was not found. Reconciliation and remediation recommendations remain explicitly deferred to RWP-00.77.

## Validation

- Static repository analysis against current `master` after RWP-00.75.
- Reviewed migrations, models, repositories, resolution and metering services, authorization controllers, direct consumers, Platform Operations administration, Back Office locked/upgrade components, tests and architecture records found by repository search.
- Documentation consistency review completed.
- GitHub Actions is authoritative for lightweight documentation validation on the exact PR head.
- Azure SQL and all integration/external-system tests remain skipped under the standing owner instruction.

## Handoff

After merge, issue closure, default-branch verification, and claim release, execute **RWP-00.77 — Capability Reconciliation & Gap Analysis (#553)**. Do not implement remediation in RWP-00.77.