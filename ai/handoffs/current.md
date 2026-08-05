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
- Entertainment & Attractions: RWP-00.69 complete in this proposed merge state; RWP-00.70 is next

## Bar Operating-Characteristics Result

RWP-00.18 defines cross-midnight operating and service periods; separate kitchen, bar, doors, event, last-entry, and locally authored last-call timing; high-frequency tap, item, release, and temporary-offer changes; table, bar, counter, and hybrid service models; bounded responsible-content and age/access presentation; entertainment and sports operations; and distinct reservation, guest-list, cover, ticket, and private-event state.

Essential manual availability, hours, specials, events, public guidance, targeting, preview, publishing, delivery confirmation, correction, offline/outdated awareness, supersession, and restoration remain core. Operating values are product/domain state. Authority is permission. Advanced workflow is a tier candidate. External synchronization is an add-on candidate. Quantities are limits and temporary delivery controls are rollout flags.

## Hospitality Optional-Capability Result

RWP-00.56 preserves the required manual core and defines optional candidates for automation, scale, coordination, personalization, governance, insight, enterprise administration, managed hardware, connectivity, monitoring, and support.

Tier candidates include advanced wayfinding, brand libraries, property-group coordination, campaigns, approvals and advanced workflows, localization workflow, advanced analytics, enterprise administration, and selected advanced operational workflow. Independent add-on candidates include property-management, event, room-booking, transport, parking, access, guest-service, gaming, maps, positioning, weather, emergency, translation, AI, identity providers, managed hardware, connectivity, monitoring, and related external systems or services.

## Entertainment Capability-Classification Result

RWP-00.69 gives every Entertainment & Attractions concern one primary Track 0 classification.

- Industry, subtype, terminology, hierarchy, schedules, content, operational values, sources, targets, delivery, and versions are **product/domain state**.
- Essential manual operation, exact targeting, publication confidence, correction, and recovery are **core capabilities**.
- Authority is **permission**.
- Recurring native advanced outcomes are **tier entitlement candidates**.
- Independent integrations and managed services are **add-on candidates**.
- Quantity and consumption boundaries are **limits**.
- Temporary exposure control is an **internal rollout flag** only.

The classification resolves manual versus automated wait, capacity versus sold out, maps versus wayfinding, approval versus permission, basic health versus managed monitoring, manual languages versus premium localization, analytics versus source data, AI access versus generated content state, enterprise identity versus authorization, and subtype versus packaging.

Future commercial and administrative surfaces must keep the included manual path visible and distinguish not purchased, not permitted, not configured, disconnected, stale, unsupported, limit reached, and internally disabled states. Preserve the approved Sky Blue administrative direction.

## Exact Next Actions

- Execute **RWP-00.19 — Bar, Brewery & Nightlife Required Capabilities** (#494).
- After RWP-00.56 is merged, verified, closed, and released, execute **RWP-00.57 — Hospitality Capability Classification** (#532).
- After RWP-00.69 is merged, verified, closed, and released, execute **RWP-00.70 — Entertainment & Attractions Subscription Tier Mapping** (#545).

## Parallel-Stream Rule

Each industry remains sequential inside its approved RWP range. Restaurant is the canonical baseline, only merged industry-specific documents are authoritative, and shared living records follow `docs/process/SHARED_FILE_WRITE_PROTOCOL.md` with queued semantic updates and short transactional write windows.

## Boundaries

- Do not start product implementation from Track 0 issues.
- Do not resume RWP-13.06 or Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, limits, rollout controls, privacy systems, localization, analytics, inventory, tap-management, reservation, ticketing, admissions, access, identity, sports, property-management, event, room-booking, transport, point-of-sale, guest-service, gaming, emergency, mapping, AI, hardware, managed services, or integrations during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
