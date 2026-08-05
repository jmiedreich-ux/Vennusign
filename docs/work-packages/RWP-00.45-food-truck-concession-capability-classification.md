# RWP-00.45 — Food Truck & Concession Capability Classification

## Status

Complete in this proposed merge state.

## Issue

- #520

## Objective

Consolidate every Food Truck & Concession concern and assign one primary Track 0 classification: core capability, permission, product/domain state, tier entitlement, independent add-on, usage or quantity limit, or internal rollout flag.

## Dependency verified

- RWP-00.44 is merged, verified on `master`, closed, and released.
- No competing RWP-00.45 branch or pull request existed when this work began.

## Delivered

- Produced a canonical classification matrix for menu, availability, operating state, locations, events, screens, publication, delivery, queue and pickup guidance, permissions, scheduling, promotions, multi-unit coordination, analytics, AI, integrations, managed services, and usage allowances.
- Resolved manual-versus-integrated availability, location-versus-route, event/host, screen-health-versus-managed-service, analytics-versus-external-data, and AI ambiguities.
- Preserved manual menu, availability, location/event communication, targeting, publishing, delivery confirmation, offline awareness, and restoration as core.
- Kept permissions, product state, commercial access, add-ons, limits, and internal rollout independent.
- Defined customer-facing distinctions between included, purchasable, limited, permission-restricted, disconnected, stale, unconfigured, unsupported, and internally staged conditions.

## Boundaries

Documentation and planning only. No product, UI, API, schema, migration, billing, entitlement, permission, feature-gate, limit, rollout, analytics, AI, routing, ordering, payment, inventory, event, host, hardware, notification, or integration behavior was implemented.

Integration and external-system tests were not applicable and remain skipped under the standing project rule.

## Validation

Every concern introduced by RWP-00.39 through RWP-00.44 now has one primary Track 0 classification. Duplicate and ambiguous concepts are resolved without converting essential operation into premium access.

## Handoff

The next sequential item is **RWP-00.46 — Food Truck & Concession Subscription Tier Mapping** (#521). It must not begin until this RWP is merged, verified on `master`, issue #520 is closed, and the claim is released.
