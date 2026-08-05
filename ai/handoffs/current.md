# Vennusign Session Handoff

## Current State

- Item: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Product implementation: paused
- Phase 14 and later: paused
- RWP-13.06: paused pending the owner-approved Track 0 model
- Restaurant: canonical approved baseline
- Bar, Brewery & Nightlife: RWP-00.17 merged; RWP-00.18 is next
- Café, Bakery & Dessert: RWP-00.29 merged; RWP-00.30 is next
- Food Truck & Concession: RWP-00.42 merged; RWP-00.43 is next
- Hospitality: RWP-00.54 complete in this proposed merge state; RWP-00.55 is next
- Entertainment & Attractions: RWP-00.65 merged; RWP-00.66 is next

## Hospitality Operating Result

RWP-00.54 establishes:

- continuous property operation with independent property, area, amenity, service, outlet, event, route, notice, screen, language, and source states;
- explicit shift and handoff visibility for active notices, overrides, stale sources, unpublished changes, failed deliveries, outdated screens, recovery points, and unresolved actions;
- arrival, check-in, stay, departure, check-out, and overnight presentation rhythms without exposing private guest state;
- guest-notice scope, audience, priority, effective time, source, target, publication, expiration, correction, supersession, and recovery boundaries;
- independent amenity, service, outlet, meeting-space, event, transport, access, and wayfinding operation;
- public and authorized meeting/event directories, room changes, delays, cancellations, relocations, registration, and route changes;
- operational wayfinding with verified destinations, routes, accessibility information, temporary changes, and no invented distance or travel claims;
- authorized urgent-message planning without defining emergency policy, alarm behavior, dispatch, or life-safety implementation;
- basic manual multilingual operation, per-language preview and delivery state, missing-language visibility, expansion, right-to-left readiness, and local date/time clarity;
- property-group scope, local overrides, mixed-state visibility, safe bulk action, and separation of brand, ownership, management, permission, authority, entitlement, and limits;
- subtype-specific operating rhythms and presentation priorities;
- one primary Track 0 classification for every operating concern.

Manual guest communication and recovery remain core. Advanced workflow and coordination remain tier candidates. External property, event, room, transport, point-of-sale, guest-service, access, gaming, translation, AI, weather, emergency, map, positioning, and related synchronization remain add-on candidates where an integration is required.

Public information must remain privacy-safe and must not infer room readiness, eligibility, access, capacity, timing, route, translation quality, or source freshness.

## Impeccable Planning Result

Future Hospitality surfaces are **Operate** experiences for authorized property teams.

They prioritize exceptions, active notices, changed hours and states, stale or conflicting sources, publication failures, partial delivery, outdated screens, language gaps, affected scope, explicit targets, and recovery. During arrival/departure peaks they emphasize reception, access, parking, transport, amenities, outlets, wayfinding, notices, and screen health. During events they emphasize directories, meeting spaces, session changes, registration, routes, languages, and publication result. Across properties they surface local dates and times, mixed states, excluded targets, and safe bulk actions.

Required planning states include first use, empty, loading, permission, validation, stale source, source conflict, offline, outdated, publish failure, partial delivery, success, undo, restoration, missing translation, long names, overnight date boundaries, keyboard and assistive-technology operation, non-color status, 200% zoom, and phone through large-desktop layouts. Preserve the approved Sky Blue administrative direction.

No UI, API, schema, migration, billing, entitlement, permission, privacy-system, localization, analytics, property-management, event, room-booking, transport, point-of-sale, guest-service, access, gaming, emergency, map, AI, hardware, or integration implementation was authorized or performed.

## Exact Next Hospitality Action

After RWP-00.54 is merged, verified on `master`, issue #529 is closed, and the claim is released, execute **RWP-00.55 — Hospitality Required Capabilities** (#530).

RWP-00.55 must define the smallest viable core set for guest information, wayfinding, amenity and outlet hours, events and meetings, notices, property context, language variants, explicit targeting, publish confirmation, offline and outdated awareness, correction, recovery, permissions, and required states. It remains documentation-only and hands off to RWP-00.56.

## Parallel-Stream Rule

Each industry remains sequential inside its approved RWP range. Restaurant is the canonical baseline, only merged documents are authoritative, and no two runs may concurrently modify the same shared controlled file.

## Boundaries

- Do not start product implementation from Track 0 issues.
- Do not resume RWP-13.06 or Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, limits, rollout controls, privacy systems, localization, analytics, property-management, event, room-booking, transport, point-of-sale, guest-service, access, gaming, emergency, map, AI, hardware, or integrations during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
