# Track 0 Execution Packet

## Purpose

This directory is the compact context packet for Track 0 industry-planning and consolidation RWPs.

## Required reading for consolidation

1. `track0/README.md`
2. `track0/CAPABILITY_MODEL.md`
3. `track0/RESTAURANT_BASELINE.md`
4. `track0/CAPABILITY_MATRIX.md`
5. `track0/consolidation/CROSS_INDUSTRY_MODEL.md`
6. `track0/consolidation/EXISTING_PRODUCT_INVENTORY.md` after RWP-00.76
7. The current issue and work-package record

Read broader repository material only when the current issue requires factual product evidence not available in this packet.

## Execution model

- Complete one RWP at a time in strict sequence.
- Use a dedicated issue, claim, branch, PR, review, merge, verification, and release.
- Work remains documentation and product planning until explicit owner approval authorizes implementation.
- RWP-13.06 and Phase 14+ remain paused.
- Azure SQL and integration/external-system tests remain skipped.
- Shared living records use queued semantic updates and short transactional write windows.

## Native-industry completion

RWP-00.26, RWP-00.38, RWP-00.50, RWP-00.62, and RWP-00.74 are complete.

## Consolidation sequence

- RWP-00.75 — Cross-Industry Normalization: merged and verified.
- **RWP-00.76 — Existing Product Feature, Gate & Limit Inventory: complete in proposed merge state.**
- **RWP-00.77 — Capability Reconciliation & Gap Analysis: exact next item after RWP-00.76 merge and release.**
- RWP-00.78 — Unified Tier & Add-On Architecture.
- RWP-00.79 — Limits, Scope & Inheritance Policy.
- RWP-00.80 — Cross-Industry Customer Journey Validation.
- RWP-00.81 — Owner Approval & Implementation Handoff.

Do not skip or combine items.

## Normalization contract

Every concern has one primary classification: core capability, permission, product/domain state, tier entitlement candidate, independent add-on candidate, usage or quantity limit, or internal rollout flag.

Essential manual operation remains core. Industry and subtype are non-commercial configuration. Permission is not entitlement. Product state is not a feature flag. External systems and separately delivered services are add-on candidates. Counts and consumption are limits. Rollout remains internal.

## Current-product inventory contract

RWP-00.76 is factual, not prescriptive. It records exact known keys, authority, scope, source location, and consumers and explicitly marks unknown or non-normalized mechanisms. Reclassification and remediation recommendations begin only in RWP-00.77.

## Impeccable requirement

Apply project-local Impeccable guidance to UI-facing planning. Locked, permission-restricted, unavailable, disconnected, stale, unsupported, limited, privacy/rights-restricted, and rollout-controlled states require distinct semantics, truthful explanations, accessible actions, responsive behavior, and recovery paths. Browser presentation never becomes authorization authority.

## Expected outputs

- update the current consolidation artifact;
- update the capability matrix when classification decisions change;
- record unresolved owner decisions;
- synchronize status, tracker, current handoff, packet, and affected records at the completion checkpoint;
- keep all changes bounded to the current issue.
