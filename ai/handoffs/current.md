# Vennusign Session Handoff

## Current State

- Item: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Mode: owner-led planning with independently scheduled native-industry streams; implementation paused
- Active implementation WP/RWP: none
- Phase 14 and later: paused
- RWP-13.06: paused pending the owner-approved Track 0 model
- Restaurant: canonical approved baseline
- Bar, Brewery & Nightlife: RWP-00.16 merged; RWP-00.17 is next
- Café, Bakery & Dessert: RWP-00.28 merged; RWP-00.29 is next
- Food Truck & Concession: RWP-00.39 merged; RWP-00.40 is next
- Hospitality: RWP-00.51 merged; RWP-00.52 is next
- Entertainment & Attractions: RWP-00.63 complete in this proposed merge state; RWP-00.64 is next

## Entertainment & Attractions Definition Result

The canonical profile is documented at `track0/industries/entertainment-attractions.md` as a delta from Restaurant.

It covers destination-, program-, exhibition-, performance-, recreation-, and attraction-led venues whose visitor experience depends on accurate schedules, admissions guidance, wayfinding, availability, queue, venue-state, safety, accessibility, and event information across changing operating periods and physical areas.

Initial native concepts include cinemas, performing-arts and live-performance venues, museums and non-retail galleries, science centers and planetariums, zoos and aquariums, botanical gardens, theme and amusement parks, family entertainment centers, arcades, bowling centers, spectator sports venues, attractions, tours, heritage sites, and related hybrids where entertainment, exhibition, performance, recreation, or attraction attendance is the primary identity. Exact subtype boundaries are deferred to RWP-00.64.

Venue, building or area, attraction, exhibit, event, performance, screening, session, queue, admission window, capacity state, delay, closure, relocation, and related values are product/domain state when represented. Manual program, showtime, admissions, wayfinding, queue, capacity, delay, closure, relocation, accessibility, safety, targeting, publishing, delivery confirmation, offline awareness, and recovery remain core capabilities. Organization primary industry and venue subtype remain product/domain configuration rather than commercial entitlements. Counts remain limits. Authorization, audience, admission, privacy, and content authority remain distinct from commercial access. Automatic ticketing, admissions, access-control, queue, venue, cinema, show-control, collection, event, sports, attraction, or other synchronization remains a later packaging question.

Restaurant menu semantics remain inherited for concessions and food-and-beverage outlets that use them but do not define the primary content model for an entertainment venue or attraction as a whole.

## Impeccable Planning Result

The project-local Impeccable skill and `shape` guidance were consulted for future UI-facing work.

- Administrative surfaces use Operate mode and prioritize exact scope, current program and operating information, intended targets, delivery state, and recovery.
- Visitor schedules, admissions guidance, wayfinding, exhibit interpretation, and operational information use Read mode; Experience mode is appropriate only when it does not obscure essential guidance.
- Later specifications must cover realistic single-screen through multi-site ranges and first-run, empty, on-sale, available, limited, sold-out, full, preparing, boarding, active, intermission, delayed, paused, relocated, canceled, weather-affected, unavailable, closed, maintenance, emergency, offline, outdated, permission, admission, privacy, publish-failure, success, and recovery states.
- Phone use while walking the venue, box-office and desktop administration, portrait and landscape displays, large-format boards, bright outdoor queues, dim auditoriums and galleries, crowded concourses, long viewing distances, localization, accessibility, and intermittent connectivity are binding conditions.
- High-impact or venue-wide changes require explicit scope and target confirmation, visible delivery state, stale/offline distinction, safe restoration, and plain escalation guidance.
- Preserve the Sky Blue direction for Vennusign administrative surfaces.

No UI, API, schema, migration, or product implementation was authorized or performed.

## Exact Next Entertainment & Attractions Action

After RWP-00.63 is merged, verified on `master`, issue #538 is closed, and the claim is released, execute **RWP-00.64 — Entertainment & Attractions Venue Subtypes** (#539).

RWP-00.64 must:

- define cinema, theater, museum, gallery, zoo or aquarium, theme or amusement park, family entertainment center, arcade, bowling, sports venue, live-event venue, attraction or tour, and hybrid subtypes;
- establish inclusion, exclusion, neighboring-profile, and ambiguous-boundary rules;
- map meaningful subtype differences without duplicating Restaurant inheritance;
- define venue subtype selection, change, mixed-organization, and multi-venue behavior;
- distinguish entertainment operating model from building form, ownership, promoter, presenter, tenant, sponsor, team, performer, distributor, or rights-holder structure;
- keep subtypes separate from tiers, entitlements, permissions, audience scope, admissions scope, and limits;
- consult Impeccable for any UI-facing subtype selection or change-flow planning;
- remain documentation-only and hand off to RWP-00.65.

## Parallel-Stream Rule

The owner approved independently scheduled native-industry streams. Each industry remains sequential inside its own approved RWP range. Restaurant is the canonical baseline, only merged documents are authoritative, and no two runs may concurrently modify the same shared controlled file.

## Boundaries

- Do not start product implementation from Track 0 issues.
- Do not resume RWP-13.06 until Track 0 produces an owner-approved capability and packaging model.
- Do not start Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, limits, admissions systems, privacy systems, or rollout controls during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
