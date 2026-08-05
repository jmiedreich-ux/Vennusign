# Vennusign Project Status

## Current State

- Phase 13: complete.
- Phase 14 and later: paused pending explicit owner approval.
- Product implementation: paused.
- Active planning track: Track 0 — Capability, Packaging, and Entitlement Architecture (#488).
- RWP-13.06: paused until the consolidated Track 0 model and implementation packages are approved.
- Native-industry gate: complete.

## Consolidation Progress

- RWP-00.75 — Cross-Industry Normalization: complete and merged.
- **RWP-00.76 — Existing Product Feature, Gate & Limit Inventory: complete in proposed merge state.**

The factual inventory is recorded in `track0/consolidation/EXISTING_PRODUCT_INVENTORY.md`.

Current product mechanisms include:

- session capability keys for Back Office routes;
- effective feature keys and locked/upgrade presentation;
- current tier and subscription presentation with server-authoritative provider confirmation;
- `MaxScreens` and `MaxVenues` downgrade checks;
- organization/venue claims and server-validated context switching;
- support tier/override authority;
- separate domain state for availability, schedule, source, delivery, screen, subscription, and HaaS status;
- separate Stripe, POS, HaaS, player/delivery, and source synchronization boundaries.

The inventory records exact known identifiers and current consumers. It does not approve their classification or change a live gate.

## Exact Next Action

After RWP-00.76 merges, closes, verifies, and releases, execute **RWP-00.77 — Capability Reconciliation & Gap Analysis (#553)**. Map the factual product inventory to the normalized model, identify missing, duplicate, obsolete, or incorrectly classified mechanisms, and recommend bounded remediation without implementation.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Azure SQL, live Stripe, and all integration/external-system tests remain skipped.
