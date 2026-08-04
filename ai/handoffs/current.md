# Vennusign Session Handoff

## Current State

- Item: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Mode: owner-led planning with independently scheduled native-industry streams; implementation paused
- Active implementation WP/RWP: none
- Phase 14 and later: paused
- RWP-13.06: paused pending the owner-approved Track 0 model
- Restaurant: canonical approved baseline
- Bar, Brewery & Nightlife: RWP-00.15 merged; RWP-00.16 is next
- Café, Bakery & Dessert: RWP-00.27 merged; RWP-00.28 is next
- Food Truck & Concession: RWP-00.39 merged; RWP-00.40 is next
- Hospitality: RWP-00.51 complete in this proposed merge state; RWP-00.52 is next

## Hospitality Definition Result

The canonical profile is documented at `track0/industries/hospitality.md` as a delta from Restaurant.

It covers lodging-led properties whose guest experience depends on accurate arrival, stay, event, amenity, dining, wayfinding, service, safety, and departure information across public areas and changing operating periods.

Initial native concepts include hotels, resorts, motels, boutique lodging, hostels, extended-stay hotels, serviced-apartment lodging, conference and event-led properties, casino resorts, and related hybrids where guest accommodation is the primary operating identity. Exact subtype boundaries are deferred to RWP-00.52.

Property, building or area, outlet, room or event, amenity, service window, closure, relocation, and related values are product/domain state when represented. Manual guest-information, wayfinding, event, amenity, service, changed-hours, targeting, publishing, delivery confirmation, offline awareness, and recovery remain core capabilities. Organization primary industry and property subtype remain product/domain configuration rather than commercial entitlements. Counts remain limits. Authorization and information scope remain distinct from commercial access. Automatic property-management, event, room-booking, point-of-sale, transport, guest-service, or other synchronization remains a later packaging question.

Restaurant menu semantics remain inherited for food-and-beverage outlets that use them but do not define the primary content model for a lodging property as a whole.

## Impeccable Planning Result

The project-local Impeccable skill and `shape` guidance were consulted for future UI-facing work.

- Administrative surfaces use Operate mode and prioritize exact scope, current operational information, intended targets, delivery state, and recovery.
- Guest information and wayfinding use Read mode and prioritize safety, destination, direction, event or service state, time, access instructions, and next action.
- Later specifications must cover realistic small-property through multi-property ranges and first-run, empty, scheduled, active, changed, delayed, relocated, full, unavailable, closed, maintenance, emergency, offline, outdated, permission, restricted, publish-failure, success, and recovery states.
- Phone use while walking the property, front-desk and desktop administration, portrait and landscape displays, bright lobbies, dim corridors, crowded conferences, long viewing distances, localization, accessibility, and intermittent connectivity are binding conditions.
- High-impact or property-wide changes require explicit scope and target confirmation, visible delivery state, stale/offline distinction, safe restoration, and plain escalation guidance.
- Preserve the Sky Blue direction for Vennusign administrative surfaces.

## Exact Next Hospitality Action

After RWP-00.51 is merged, verified on `master`, issue #526 is closed, and the claim is released, execute **RWP-00.52 — Hospitality Venue Subtypes** (#527).

RWP-00.52 must:

- define hotel, resort, motel, hostel, extended-stay, serviced-apartment, conference-property, casino-resort, boutique-lodging, and hybrid subtypes;
- establish inclusion, exclusion, and ambiguous-boundary rules;
- map meaningful subtype differences without duplicating Restaurant inheritance;
- define property subtype selection, change, and mixed-property behavior;
- distinguish lodging operating model from building form, ownership, franchise, or management structure;
- keep subtypes separate from tiers, entitlements, permissions, information scope, and limits;
- consult Impeccable for any UI-facing subtype selection or change-flow planning;
- remain documentation-only and hand off to RWP-00.53.

## Parallel-Stream Rule

The owner approved independently scheduled native-industry streams. Each industry remains sequential inside its own approved RWP range. Restaurant is the canonical baseline, only merged documents are authoritative, and no two runs may concurrently modify the same shared controlled file.

## Boundaries

- Do not start product implementation from Track 0 issues.
- Do not resume RWP-13.06 until Track 0 produces an owner-approved capability and packaging model.
- Do not start Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, or rollout controls during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
