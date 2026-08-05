# RWP-00.33 — Café, Bakery & Dessert Capability Classification

## Status

- **Track:** Track 0 — Capability, Packaging, and Entitlement Architecture
- **Issue:** #508
- **Execution mode:** Sequential within the Café, Bakery & Dessert stream
- **Dependency:** RWP-00.32 merged, verified, closed, and released
- **Scope:** Documentation and planning only
- **Result:** Complete in this proposed merge state

## Objective

Consolidate all Café, Bakery & Dessert concerns and assign exactly one primary Track 0 classification to each: core capability, permission, product/domain state, tier entitlement, independent add-on, usage or quantity limit, or internal rollout flag.

## Delivered

- Canonical classification policy and comprehensive concern matrix
- One primary classification for industry, subtype, terminology, content, operating state, screen, publication, source, recovery, permission, advanced workflow, integration, managed service, limits, and rollout concerns
- Explicit ambiguity resolutions for operational state versus access, permission versus tier, integration versus core, workflow versus limit, history, translation, AI, subtype, monitoring, and analytics
- Customer-facing status distinctions for included, not purchased, not permitted, not configured, disconnected, stale, conflicting, limited, unsupported, rollout-disabled, and business-state conditions
- Cross-industry capability-matrix update intent
- Project-local Impeccable Operate-mode requirements for truthful locked and unavailable states

## Classification result

- Essential manual daily operation is core.
- Represented business, source, target, publication, delivery, and recovery values are product/domain state.
- Actor authority is permission.
- Advanced native Vennusign workflow is a tier entitlement candidate.
- External systems, consumption-backed services, and managed services are independent add-on candidates.
- Counts, volume, frequency, storage, retention, export, support, transaction, and AI consumption are limits.
- Temporary internal exposure, migration, compatibility, and emergency-disable controls are rollout flags.
- Industry and subtype remain non-commercial product configuration.

## Validation

- Reviewed against issue #508 and the merged RWP-00.27–00.32 Café record set.
- Every material concern has one primary classification.
- Required core is unchanged.
- Permission, state, tier, add-on, limit, privacy/source, and rollout are not conflated.
- No product, UI, API, schema, migration, billing, entitlement, permission, limit, rollout, analytics, AI, integration, hardware, or managed-service implementation is included.
- Documentation-only GitHub Actions are authoritative on the exact reviewed PR head.
- Integration and external-system tests remain skipped under the standing owner instruction.

## Shared-record pending queue

After merge and verification:

- update `track0/CAPABILITY_MATRIX.md` with the Café classification result without overwriting other industries;
- mark Café complete through RWP-00.33;
- release this claim and claim RWP-00.34;
- preserve all concurrent shared-record changes; and
- hand off to subscription tier mapping.

## Handoff

**RWP-00.34 — Café, Bakery & Dessert Subscription Tier Mapping** (#509) is next.
