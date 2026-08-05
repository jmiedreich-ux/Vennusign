# Vennusign Project Status

## Current State

- Phase 13 — Customer Identity, Signup, and Onboarding: complete.
- Phase 14 and later: paused pending explicit owner approval.
- Product implementation: paused.
- Active planning track: Track 0 — Capability, Packaging, and Entitlement Architecture (#488).
- RWP-13.06 — Trial-First Onboarding (#466): paused until the consolidated Track 0 model and implementation packages are approved.
- Restaurant remains the canonical approved baseline.

## Native-Industry Gate

Bar through RWP-00.26, Café through RWP-00.38, Food Truck through RWP-00.50, Hospitality through RWP-00.62, and Entertainment through RWP-00.74 are complete, merged, validated, verified, and released.

## Consolidation Progress

**RWP-00.75 — Cross-Industry Normalization** is complete in the proposed merge state.

The normalized model is recorded in `track0/consolidation/CROSS_INDUSTRY_MODEL.md` and establishes:

- one primary classification per concern: core, permission, product/domain state, tier candidate, independent add-on candidate, usage/quantity limit, or internal rollout flag;
- essential manual operation, targeting, publication, delivery confidence, correction, and recovery as universal core;
- industry and subtype as non-commercial configuration affecting defaults and terminology only;
- permissions as authority rather than commercial access;
- external systems, managed services, HaaS, and separately delivered services as add-on candidates;
- counts and consumption as limits rather than capabilities;
- distinct customer states for locked, permission-restricted, unavailable, disconnected, stale, unsupported, limit-reached, and rollout-controlled conditions;
- safe mixed-industry inheritance and local override behavior.

No pricing, numeric limits, commercial approval, or implementation is authorized by RWP-00.75.

## Exact Next Action

After RWP-00.75 merges, closes, verifies, and releases, execute **RWP-00.76 — Existing Product Feature, Gate & Limit Inventory (#552)**. Inventory factual current product keys, checks, overrides, limits, locked surfaces, rollout/configuration controls, authority, scope, and consumers. Do not skip to reconciliation or implementation.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Azure SQL and all integration/external-system tests remain skipped under the standing owner instruction.
