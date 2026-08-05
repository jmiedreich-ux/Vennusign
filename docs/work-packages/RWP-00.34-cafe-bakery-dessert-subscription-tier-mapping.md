# RWP-00.34 — Café, Bakery & Dessert Subscription Tier Mapping

## Status

- **Track:** Track 0 — Capability, Packaging, and Entitlement Architecture
- **Issue:** #509
- **Execution mode:** Sequential within the Café, Bakery & Dessert stream
- **Dependency:** RWP-00.33 merged, verified, closed, and released
- **Scope:** Documentation and planning only
- **Result:** Complete in this proposed merge state

## Objective

Propose cross-industry customer-outcome tier groupings while keeping essential Café operation core, industry selection non-commercial, and add-ons and limits separate.

## Delivered

- Operate, Coordinate, Portfolio, and Enterprise planning archetypes
- Complete required manual core in Operate
- Advanced schedules, rotations, campaigns, presentation, approvals, localization, and analytics in Coordinate
- Multi-venue inheritance, governance, bulk workflow, and comparative analysis in Portfolio
- Enterprise identity, administration, audit, risk, reporting, and service workflow in Enterprise
- Independent external-system, metered-service, managed-service, hardware, and custom-service add-ons
- Separate counts, consumption, storage, retention, export, support, and AI limits
- Restaurant and Bar inheritance alignment
- Organization and venue inheritance, upgrade, downgrade, grace, read-only, export, and recovery questions
- Explicit unresolved owner decisions

## Classification result

Essential manual operation remains core. Advanced native workflow and scale are tier candidates. External and managed services remain independent add-on candidates. Quantities and consumption remain limits. Industry and subtype remain product/domain state. Authority remains permission. Temporary exposure remains rollout.

## Validation

- Reviewed against issue #509, the merged Café classification, Restaurant inheritance, and the Bar outcome model.
- No Café-only commercial tier is introduced.
- Upgrade and downgrade preserve content, current safe delivery, manual fallback, source/freshness, and recovery.
- No pricing, billing, entitlement record, feature gate, trial rule, checkout, UI, API, schema, migration, or product implementation is included.
- Documentation-only GitHub Actions are authoritative on the exact reviewed head.
- Integration and external-system tests remain skipped under the standing owner instruction.

## Shared-record pending queue

After merge and verification:

- mark Café complete through RWP-00.34;
- release this claim and claim RWP-00.35;
- preserve concurrent industry changes; and
- hand off to onboarding experience planning.

## Handoff

**RWP-00.35 — Café, Bakery & Dessert Onboarding Experience** (#510) is next.