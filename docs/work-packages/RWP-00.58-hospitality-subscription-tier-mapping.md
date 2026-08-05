# RWP-00.58 — Hospitality Subscription Tier Mapping

## Status

Complete in this proposed merge state.

## Issue

- #533

## Objective

Propose customer-outcome subscription-tier bundles for Hospitality while keeping essential guest communication core, industry and subtype non-commercial, independent add-ons separate, and limits distinct. Document property-group inheritance, upgrade and downgrade behavior, and owner decisions. Documentation only.

## Dependency verified

- RWP-00.57 is merged, verified, closed, and released.
- The authoritative Hospitality classification is the input to this mapping.
- RWP-00.59 — Hospitality Onboarding Experience (#534) is next.

## Delivered

- Added `track0/industries/hospitality-subscription-tier-mapping.md`.
- Proposed four working customer-outcome archetypes: Operate, Coordinate, Portfolio, and Enterprise.
- Kept the complete RWP-00.55 manual baseline in Operate.
- Mapped advanced Vennusign workflow and governance candidates across higher tiers without making external connections inclusive.
- Kept property, event, room, transport, mapping, translation, AI, analytics-data, identity-provider, managed-hardware, connectivity, monitoring, and related services as independent add-ons.
- Kept all quantity and consumption allowances separate from capability access.
- Defined property-group inheritance and local-control requirements.
- Defined upgrade safeguards and downgrade questions, including grace, read-only, conversion, export, retention, pooled limits, active content, and add-on dependencies.
- Recorded owner decisions required before commercial approval.
- Applied project-local Impeccable `shape`, `clarify`, and `harden` guidance.

## Proposed tier outcomes

- **Operate:** accurate, privacy-safe daily guest communication and recovery.
- **Coordinate:** advanced team, event, language, content, approval, campaign, and wayfinding coordination.
- **Portfolio:** cross-property, brand, region, governance, analytics, and delegated-administration outcomes.
- **Enterprise:** enterprise identity, audit, governance, retention, support, migration, and service-management outcomes.

Names and exact tier contents are proposals only.

## Core and add-on protection

Industry and subtype remain non-commercial configuration. Manual information, hours, states, notices, events, directories, wayfinding, language variants, explicit targeting, publishing, delivery confidence, offline/outdated awareness, correction, expiry, supersession, retry, and restoration remain core.

External synchronization and managed or consumption-backed services remain independently selectable add-ons. Limits do not grant capabilities or authority.

## Property-group result

Group inheritance may offer templates, design libraries, terminology, languages, campaigns, schedules, destinations, connection configuration, reporting definitions, governance policies, entitlements, add-ons, role defaults, or allocated limits when explicitly scoped.

Every property must see source, version, mandatory/recommended/copied/linked state, local overrides, mixed states, affected local values, update impact, and rollback behavior. Current operational truth, urgent local notices, privacy, authority, screen recovery, and last-known-good content cannot be silently overwritten.

## Downgrade result

Recommended default: preserve customer data and current delivery, use a clearly communicated grace period, stop creation of new advanced objects only after impact review, provide export or conversion, protect active public screens, and require explicit approval before destructive cleanup.

Final grace periods, read-only behavior, conversion, pooled-limit redistribution, history retention, advanced-rendering behavior, add-on dependencies, notifications, and deletion require owner approval.

## Owner decisions

Final tier names/count, exact capability placement, numeric limits, pooling, overage, trials, grandfathering, add-on prerequisites, inheritance policy, downgrade behavior, enterprise identity, managed service, pricing timing, and upgrade presentation remain open.

The accepted direction that pricing should not interrupt the path to a first active screen is recorded for owner confirmation in the onboarding RWP.

## Impeccable result

Future packaging surfaces lead with customer outcomes, keep included core actions visible, distinguish entitlement, permission, connection, limit, state, and rollout, avoid disabled-control grids, and use progressive comparison. Preserve keyboard and assistive-technology access, non-color status, 200% zoom, localization expansion, right-to-left readiness, long names, responsive layouts, and the approved Sky Blue administrative direction.

No UI or commercial implementation was introduced.

## Validation

Documentation-only review confirmed:

- essential guest communication remains core;
- industry and subtype are non-commercial;
- tier candidates, add-ons, permissions, state, limits, and rollout remain separate;
- property-group inheritance preserves local control;
- downgrade does not abruptly break current public operation;
- unresolved commercial choices are explicit owner decisions;
- RWP-00.59 is next.

GitHub Actions is authoritative for lightweight documentation validation on the exact pull-request head.

## Skipped under standing owner instruction

All integration and external-system testing and all product or commercial implementation, including UI, API, schema, migrations, billing, entitlements, permissions, privacy systems, localization, analytics, external connections, identity, AI, hardware, connectivity, monitoring, managed services, pricing, trials, contracts, and limits.

## Exact next action

After this RWP is merged, verified on `master`, issue #533 is closed, and the claim is released, execute **RWP-00.59 — Hospitality Onboarding Experience** (#534).

RWP-00.59 must define the complete Hospitality onboarding journey from industry/subtype recognition through property setup and first active screen, keep pricing contextual and non-blocking, remain documentation-only, and hand off to RWP-00.60.