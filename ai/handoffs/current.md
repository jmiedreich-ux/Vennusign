# Vennusign Session Handoff

## Current State

- Item: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Product implementation: paused
- Phase 14 and later: paused
- RWP-13.06: paused pending the owner-approved Track 0 model
- Restaurant: canonical approved baseline
- Bar, Brewery & Nightlife: RWP-00.18 merged; RWP-00.19 is next
- Café, Bakery & Dessert: RWP-00.29 merged; RWP-00.30 is next
- Food Truck & Concession: RWP-00.42 merged; RWP-00.43 is next
- Hospitality: RWP-00.56 complete in this proposed merge state; RWP-00.57 is next
- Entertainment & Attractions: RWP-00.67 complete in this proposed merge state; RWP-00.68 is next

## Bar Operating-Characteristics Result

RWP-00.18 defines cross-midnight operating and service periods; separate kitchen, bar, doors, event, last-entry, and locally authored last-call timing; high-frequency tap, item, release, and temporary-offer changes; table, bar, counter, and hybrid service models; bounded responsible-content and age/access presentation; entertainment and sports operations; and distinct reservation, guest-list, cover, ticket, and private-event state.

Essential manual availability, hours, specials, events, public guidance, targeting, preview, publishing, delivery confirmation, correction, offline/outdated awareness, supersession, and restoration remain core. Operating values are product/domain state. Authority is permission. Advanced workflow is a tier candidate. External synchronization is an add-on candidate. Quantities are limits and temporary delivery controls are rollout flags.

Future Bar Operate surfaces must make venue and area scope, current state, effective local time, source, audience, targets, public wording, publication result, and recovery path explicit. Required states include first use, empty, loading, permission, validation, stale source, conflict, offline, outdated, partial delivery, success, undo, and restoration. Support phone and desktop, local-time clarity, long names, keyboard and assistive technology, 200% zoom, non-color distinctions, restrained motion, and the approved Sky Blue administrative direction.

## Hospitality Optional-Capability Result

RWP-00.56 preserves the RWP-00.55 required manual core and defines optional candidates for automation, scale, coordination, personalization, governance, insight, enterprise administration, managed hardware, connectivity, monitoring, and support.

**Tier candidates:** advanced wayfinding; brand libraries; property-group coordination; campaigns and orchestration; approvals and advanced workflows; localization workflow; advanced analytics; enterprise administration; and selected advanced operational workflow.

**Independent add-on candidates:** property-management; event, conference, and room-booking; transport, parking, access, guest-service, gaming, map, positioning, weather, emergency, translation, AI, identity-provider, managed hardware, connectivity, monitoring, and related external systems or consumption-backed services.

**Limits:** properties, groups, buildings, accommodations, venues, outlets, amenities, services, events, meeting spaces, screens, devices, users, roles, languages, integrations, sources, templates, assets, campaigns, reports, history, storage, transactions, messages, requests, tokens, data, and spend.

Permissions, represented state, privacy, source authority, entitlement, add-on, limit, and rollout remain separate. Every optional candidate requires manual fallback, source and freshness, conflict handling, privacy and audience, consumption and limit behavior, safe failure, disconnect and cancellation, data retention, correction, delivery confidence, and restoration.

Optional-capability surfaces keep the included manual path visible and distinguish not purchased, not permitted, not configured, disconnected, stale, limit reached, unsupported, and internally disabled states. Preserve the approved Sky Blue administrative direction.

## Entertainment Required-Capability Result

RWP-00.67 defines required core venue information, programs and schedules, disruption communication, queue/capacity/admission guidance, manual wayfinding, notices, basic language variants, targeting and publishing, delivery confidence, source and recovery, and permission/privacy boundaries.

Essential manual operation remains available without premium tiers or paid integrations. Product values, sources, audiences, targets, delivery, permissions, packaging, add-ons, limits, privacy, and rollout remain separate.

## Exact Next Actions

- Execute **RWP-00.19 — Bar, Brewery & Nightlife Required Capabilities** (#494).
- After RWP-00.56 is merged, verified, closed, and released, execute **RWP-00.57 — Hospitality Capability Classification** (#532).
- After RWP-00.67 is merged, verified, closed, and released, execute **RWP-00.68 — Entertainment & Attractions Optional Capabilities** (#543).

## Parallel-Stream Rule

Each industry remains sequential inside its approved RWP range. Restaurant is the canonical baseline, only merged industry-specific documents are authoritative, and shared living records follow `docs/process/SHARED_FILE_WRITE_PROTOCOL.md` with queued semantic updates and short transactional write windows.

## Boundaries

- Do not start product implementation from Track 0 issues.
- Do not resume RWP-13.06 or Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, limits, rollout controls, privacy systems, localization, analytics, inventory, tap-management, reservation, ticketing, admissions, access, identity, sports, property-management, event, room-booking, transport, point-of-sale, guest-service, gaming, emergency, mapping, AI, hardware, managed services, or integrations during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
