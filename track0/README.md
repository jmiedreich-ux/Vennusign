# Track 0 Execution Packet

## Required consolidation context

1. `track0/CAPABILITY_MODEL.md`
2. `track0/RESTAURANT_BASELINE.md`
3. `track0/CAPABILITY_MATRIX.md`
4. `track0/consolidation/CROSS_INDUSTRY_MODEL.md`
5. `track0/consolidation/EXISTING_PRODUCT_INVENTORY.md`
6. `track0/consolidation/RECONCILIATION_GAP_ANALYSIS.md` after RWP-00.77
7. Current issue and work package

## Rules

Complete one RWP at a time with issue, claim, branch, PR, review, merge, verification, and release. Work is documentation/planning only. RWP-13.06 and Phase 14+ remain paused. Shared files use short transactional write windows. Azure SQL and integration/external-system tests remain skipped.

## Completion

The five native industries are complete. Consolidation status:

- RWP-00.75 normalization: merged.
- RWP-00.76 factual inventory: merged.
- **RWP-00.77 reconciliation/gap analysis: complete in proposed merge state.**
- **RWP-00.78 unified tier/add-on architecture: exact next item after merge and release.**
- RWP-00.79 limits/scope/inheritance.
- RWP-00.80 customer-journey validation.
- RWP-00.81 owner approval and implementation handoff.

## Classification contract

One primary classification per concern: core capability, permission, product/domain state, tier entitlement candidate, independent add-on candidate, usage/quantity limit, or internal rollout flag.

Essential manual operation remains core. Industry/subtype are non-commercial. Permission is not entitlement. Product state is not a feature flag. Add-ons are independently attachable. Limits are typed allowances, not capabilities. Rollout remains internal.

## Reconciliation contract

Future architecture must preserve server/provider authority, venue-scoped authorization, HaaS separation, and explicit delivery/source state while replacing overloaded flat identifiers with stable capability decisions and structured reason semantics. Recommendations do not authorize implementation.

## Impeccable

UI planning must distinguish upgrade, permission, limit, source/configuration, product state, unsupported context, privacy/rights restriction, and temporary rollout/support conditions. Every state requires truthful text, a specific action, accessible semantics, responsive behavior, and recovery. Browser presentation never becomes authority.
