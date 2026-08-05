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
- Hospitality: RWP-00.53 complete in this proposed merge state; RWP-00.54 is next
- Entertainment & Attractions: RWP-00.65 merged; RWP-00.66 is next

## Hospitality Terminology Result

RWP-00.53 defines:

- neutral organization-wide language for property, accommodation, area, venue, outlet, amenity, service, event, meeting space, notice, and destination;
- guest-journey language for arrival, check-in, stay, departure, and check-out without implying private or unsupported status;
- customer-authored, subtype-preferred, imported-source, and neutral-fallback terminology behavior;
- distinct public states for available, limited, open, closed, temporarily closed, unavailable, out of service, paused, delayed, canceled, relocated, maintenance-affected, weather-affected, restricted, and unknown conditions;
- regular, today’s, special, access, overnight, last-service, last-entry, last-seating, and last-shuttle time distinctions;
- privacy-safe guest wording, operator-facing source and freshness detail, and explicit action labels;
- subtype preferences for Hotel, Resort, Motel, Hostel, Extended-Stay, Serviced Apartment, Conference Property, Casino Resort, Boutique Lodging, and neutral mixed-property contexts;
- source-authority, override, conflict, and mixed-property boundaries;
- core, permission, state, tier, add-on, limit, and rollout classifications.

Manual terminology, notice, hours, wayfinding, and operating-state communication remains core. Terminology affects defaults, labels, guidance, starter recommendations, and presentation only; it does not grant access, alter privacy, transfer authority, increase limits, or change commercial availability.

Public signage must not expose guest identity, room assignments tied to a person, reservation codes, loyalty or access status, payment state, stay dates, service requests, itineraries, or other guest-specific information by default.

## Impeccable Clarification Result

Future Hospitality terminology and notice-management surfaces are **Operate** experiences for authorized property operators.

They must keep property, object, audience, source, effective time, and screen scope visible; use persistent labels and explicit verb-object actions; distinguish all hours and operating states; preview guest-facing wording and high-impact targets; preserve customer-authored content, source relationships, authority, privacy, and the last known good state; and cover first-use, empty, loading, permission, validation, stale-source, conflict, offline, publish-failure, partial-delivery, success, undo, and restoration states.

They must support keyboard and assistive technology, non-color status cues, 200% zoom, long names, localization expansion, right-to-left readiness, clear local dates and times, and phone through large-desktop layouts. Preserve the approved Sky Blue administrative direction.

No UI, API, schema, migration, billing, entitlement, permission, privacy, localization, translation, AI, analytics, property-management, event, room-booking, transport, point-of-sale, guest-service, access, gaming, or integration implementation was authorized or performed.

## Exact Next Hospitality Action

After RWP-00.53 is merged, verified on `master`, issue #528 is closed, and the claim is released, execute **RWP-00.54 — Hospitality Operating Characteristics** (#529).

RWP-00.54 must document continuous property operation, shifts and handoffs, arrival and departure cycles, guest notices, amenities, outlets, meetings and events, wayfinding, emergency messaging, multilingual needs, property groups, subtype differences, defaults, and capability presentation. It remains documentation-only and hands off to RWP-00.55.

## Parallel-Stream Rule

Each industry remains sequential inside its approved RWP range. Restaurant is the canonical baseline, only merged documents are authoritative, and no two runs may concurrently modify the same shared controlled file.

## Boundaries

- Do not start product implementation from Track 0 issues.
- Do not resume RWP-13.06 or Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, limits, rollout controls, privacy systems, localization, analytics, property-management, event, room-booking, transport, point-of-sale, guest-service, access, gaming, AI, hardware, or integrations during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
