# Vennusign Session Handoff

## Current State

- Track: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Native-industry gate: complete through RWP-00.26, 00.38, 00.50, 00.62, and 00.74
- Current consolidation result: RWP-00.75 complete in proposed merge state
- Product implementation: paused
- RWP-13.06: paused
- Phase 14 and later: paused

## RWP-00.75 Result

Restaurant and the five native-industry profiles are normalized in `track0/consolidation/CROSS_INDUSTRY_MODEL.md`.

The model requires one primary classification per concern: core capability, permission, product/domain state, tier entitlement candidate, independent add-on candidate, usage or quantity limit, or internal rollout flag.

Essential manual operation remains core. Industry and subtype remain non-commercial configuration. Permissions govern authority. Represented configuration and operational facts remain product/domain state. Advanced native Vennusign workflows are tier candidates. External systems, managed services, HaaS, hardware/service contracts, and separately metered services are add-on candidates. Counts and consumption are limits. Rollout flags remain internal.

Unavailable product state, missing permission, absent entitlement, unconfigured add-on, disconnected or stale source, exceeded limit, unsupported context, and rollout restriction must remain distinct in future product presentation.

## Exact Next Action

Execute **RWP-00.76 — Existing Product Feature, Gate & Limit Inventory (#552)** only after RWP-00.75 is merged, issue #551 is closed, `master` is verified, and the claim is released.

RWP-00.76 must inventory factual current-product feature keys, capability checks, permissions, support overrides, limits, locked UI surfaces, configuration/rollout controls, authority, scope, and known consumers. It must not change a live gate or begin reconciliation recommendations reserved for RWP-00.77.

## Open Owner Decisions

- final tier names, capability placement, pricing, trials, contracts, grandfathering, numeric limits, pooling, overage, and grace;
- add-on prerequisites, providers, regions, rights, administration, and support commitments;
- organization/venue inheritance and override policy;
- downgrade, read-only, export, retention, deletion, and active-screen protection;
- regulated, privacy, safety, accessibility, rights, sponsor, advertising, camera, biometric, child, alcohol, and gambling obligations;
- player, hardware, monitoring, installation, replacement, and support commitments;
- metric definitions and implementation sequence.

## Boundaries

Do not resume RWP-13.06, start Phase 14+, implement product behavior, approve pricing, or treat candidate packaging as final. Azure SQL and all integration/external-system tests remain skipped under the standing owner instruction.
