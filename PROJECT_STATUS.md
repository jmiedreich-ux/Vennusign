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

- **RWP-00.75 — Cross-Industry Normalization:** complete and merged.
- **RWP-00.76 — Existing Product Feature, Gate & Limit Inventory:** complete in the proposed merge state.

The factual implementation inventory is recorded in `track0/consolidation/EXISTING_PRODUCT_FEATURE_GATE_LIMIT_INVENTORY.md`.

It records:

- the 18 seeded feature keys and their initial tier assignments;
- organization-first commercial authority with legacy venue fallback;
- feature resolution, master-switch, venue-override, cache, and usage-metering behavior;
- direct runtime checks for Quick Update, Happy Hour, allergen badges, and Video Wall;
- screen, venue, and monthly feature-usage limits;
- Back Office, Platform Operations, configuration, and secret-value authorization boundaries;
- feature-matrix administration and audit behavior;
- locked, preview, nudge, billing, and tier-decision UI surfaces;
- factual keys and surfaces with no direct feature-resolution consumer found;
- system configuration as the closest implemented rollout-like control family.

No live feature, entitlement, permission, override, limit, configuration, rollout, UI, API, schema, migration, or billing behavior changed.

## Exact Next Action

After RWP-00.76 merges, issue #552 closes, `master` is verified, and the claim is released, execute **RWP-00.77 — Capability Reconciliation & Gap Analysis (#553)**. Map the factual inventory to the normalized model and record recommendations only; do not implement remediation.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Azure SQL and all integration/external-system tests remain skipped under the standing owner instruction.
