# Track 0 Execution Packet

## Purpose

This directory is the compact context packet for Track 0 industry-planning and consolidation RWPs. Agents should use it instead of repeatedly loading broad repository history.

## Required reading for every Track 0 RWP

1. `track0/README.md`
2. `track0/CAPABILITY_MODEL.md`
3. `track0/RESTAURANT_BASELINE.md`
4. `track0/INDUSTRY_TEMPLATE.md`
5. `track0/CAPABILITY_MATRIX.md`
6. `track0/consolidation/CROSS_INDUSTRY_MODEL.md`
7. `track0/consolidation/EXISTING_PRODUCT_FEATURE_GATE_LIMIT_INVENTORY.md` after RWP-00.76
8. The current GitHub RWP issue
9. The current industry or consolidation file under `track0/`

Read additional repository documents only when the current issue explicitly requires them or when a conflict cannot be resolved from this packet.

## Execution model

- Complete one RWP at a time in strict sequence.
- Use a dedicated branch and PR.
- Merge, close, verify, and release the claim before starting the next RWP.
- Industry and consolidation work are documentation and product-planning only until explicit owner approval authorizes implementation.
- RWP-13.06 and Phase 14+ remain paused during Track 0.
- Integration and external-system tests remain skipped under the standing owner instruction.
- Shared living records follow `docs/process/SHARED_FILE_WRITE_PROTOCOL.md` using queued semantic updates and short transactional write windows.

## Native-industry completion

The five native-industry endpoints are complete:

- Bar, Brewery & Nightlife — RWP-00.26
- Café, Bakery & Dessert — RWP-00.38
- Food Truck & Concession — RWP-00.50
- Hospitality — RWP-00.62
- Entertainment & Attractions — RWP-00.74

## Consolidation sequence

- **RWP-00.75 — Cross-Industry Normalization:** complete and merged; durable model at `track0/consolidation/CROSS_INDUSTRY_MODEL.md`.
- **RWP-00.76 — Existing Product Feature, Gate & Limit Inventory:** complete in proposed merge state; durable inventory at `track0/consolidation/EXISTING_PRODUCT_FEATURE_GATE_LIMIT_INVENTORY.md`.
- **RWP-00.77 — Capability Reconciliation & Gap Analysis:** exact next item after RWP-00.76 merge and release.
- RWP-00.78 — Unified Tier & Add-On Architecture.
- RWP-00.79 — Limits, Scope & Inheritance Policy.
- RWP-00.80 — Cross-Industry Customer Journey Validation.
- RWP-00.81 — Owner Approval & Implementation Handoff.

Do not skip, combine, or begin a later item before the current item is merged, closed, verified, and released.

## Normalization contract

Every concern has one primary classification: core capability, permission, product/domain state, tier entitlement candidate, independent add-on candidate, usage or quantity limit, or internal rollout flag.

Essential manual operation remains core. Industry and subtype are non-commercial configuration. Permissions do not grant commercial access. Product state is not a feature flag. Limits are not capabilities. External systems and separately delivered managed services remain add-on candidates. Rollout flags remain internal.

## Delta rule

Every native industry inherits the Restaurant baseline. Document only meaningful differences in business and venue types, terminology, operations, content/screen purposes, roles/permissions, integrations, defaults/recommendations, classification, onboarding, dashboard, and analytics needs.

## Impeccable requirement

The project-local Impeccable skill applies whenever an RWP defines or reviews UI-facing behavior, including onboarding, dashboards, navigation, screen presentation, locked states, action hierarchy, responsive behavior, accessibility, or customer journeys.

Use its vocabulary and bounded workflow to shape, audit, adapt, harden, and polish the specification. Record hierarchy, state, feedback, accessibility, responsiveness, recovery, realistic content, localization expansion, and approved Sky Blue direction. Impeccable consultation does not authorize implementation.

## Expected outputs per RWP

- Update the current industry or consolidation document.
- Update `CAPABILITY_MATRIX.md` when classifications, packaging candidates, or final validation results change.
- Record unresolved owner decisions.
- Update the next handoff reference.
- Synchronize project status, tracker, current handoff, packet, and affected shared records in one short completion checkpoint.
- Keep changes bounded to the current issue.
