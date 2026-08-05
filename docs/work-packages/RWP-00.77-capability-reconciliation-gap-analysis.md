# RWP-00.77 — Capability Reconciliation & Gap Analysis

## Issue

#553

## Status

- Sequential documentation/planning package
- Depends on merged RWP-00.76 factual inventory
- Complete in proposed merge state

## Delivered

`track0/consolidation/RECONCILIATION_GAP_ANALYSIS.md` maps current product mechanisms to the normalized Track 0 model and records:

- critical separation of capability, permission, entitlement, product state, add-on, limit, privacy/rights, and rollout;
- overloaded and duplicated current keys;
- required manual core currently combined with advanced automation;
- missing canonical industry/object/state/source/recovery identifiers;
- current organization/venue inheritance gaps;
- dormant or incomplete candidates requiring consumer verification;
- a recommended typed server capability-decision contract;
- Impeccable reason/state guidance for upgrade, permission, limit, source, product state, unsupported, restricted, and temporary conditions;
- a bounded remediation sequence without implementation.

## Key findings

- Flat session capability strings do not constitute a complete permission or entitlement model.
- `quick_update`, `meal_periods`, language behavior, and Happy Hour need core-versus-advanced separation.
- `all_layouts`, `pos_integration`, `multi_location`, `video_wall`, and `happy_hour` overload multiple concepts.
- Current screen/venue limits, feature limit strings, layout capacity, and HaaS terms require typed domain separation.
- Provider-authoritative billing, venue-scoped sessions, HaaS separation, and explicit delivery state are aligned foundations to preserve.

## Validation and boundaries

Reviewed against RWP-00.75, RWP-00.76, current product contracts, capability matrix, Track 0 packet, issue #553, and Impeccable locked-state requirements. Recommendations only: no key, gate, permission, feature, limit, rollout, UI, API, schema, migration, billing, integration, or product behavior changed. Azure SQL, live Stripe, and all integration/external-system tests remain skipped. GitHub Actions is authoritative for lightweight documentation validation.

## Handoff

After merge, closure, verification, and release, RWP-00.78 — Unified Tier & Add-On Architecture (#554) is the exact next item.
