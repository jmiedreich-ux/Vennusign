# Entertainment & Attractions Industry Profile

## Identity

- **Industry:** Entertainment & Attractions
- **RWP range:** RWP-00.63 through RWP-00.74
- **Current status:** Industry definition complete; subtype definition is next
- **Baseline:** Restaurant
- **Current RWP:** RWP-00.63
- **Next sequential RWP:** RWP-00.64 — Venue Subtypes

## Purpose

This profile covers destination-, program-, exhibition-, performance-, recreation-, and attraction-led venues whose visitor experience depends on accurate schedules, admissions guidance, wayfinding, availability, queue, venue-state, safety, accessibility, and event information across changing operating periods and physical areas.

It inherits the Restaurant baseline. This document records only the meaningful differences needed to establish the Entertainment & Attractions boundary and guide later subtype, terminology, operations, capability, packaging, onboarding, dashboard, analytics, and review RWPs.

## Primary customer outcomes

In addition to the Restaurant baseline outcomes, operators must be able to:

- help visitors understand what is happening, where it is happening, when it starts, who it is for, and what action to take next;
- keep programs, performances, screenings, exhibits, activities, attractions, sessions, rounds, tours, admissions, queues, closures, delays, relocations, and accessibility guidance current;
- publish reliably to selected entrance, lobby, box-office, concourse, auditorium, gallery, exhibit, queue, ride, activity, food-and-beverage, retail, parking, transportation, staff, and outdoor displays;
- coordinate organization-wide brand and operational information while allowing venue-, building-, area-, attraction-, event-, session-, and screen-specific differences;
- communicate sold-out, limited, full, delayed, paused, canceled, relocated, weather-affected, unavailable, maintenance, safety, and recovery states without rebuilding content;
- support calm, accessible, multilingual, distance-readable presentation for visitors who may be unfamiliar with the venue, moving through crowds, or under time pressure;
- confirm that time-sensitive information reached every intended display and recover safely from stale, offline, or failed delivery.

## Inherited unchanged from Restaurant

Unless a later Entertainment & Attractions RWP records a meaningful exception, this industry inherits:

- reusable content creation, editing, duplication, archive, restore, preview, and publishing patterns;
- screen pairing and management, explicit targeting, delivery confirmation, offline and outdated detection, and prior-version recovery;
- basic layouts, themes, business hours, venue information, understandable errors, and recovery guidance;
- permissions, product-state separation, limit separation, and packaging discipline;
- candidate scheduling, dayparts, campaigns, coordinated screens, multi-venue sharing, brand controls, approvals, history, analytics, identity, AI, hardware, and integration capabilities.

Restaurant menu, category, item, price, dietary, availability, and special semantics remain inherited for concessions, bars, cafés, restaurants, and retail-like food outlets that use them. They are not assumed to be the primary content model for an entertainment venue or attraction as a whole. Later RWPs will define the required program, event, attraction, session, exhibit, queue, and admissions vocabulary without removing Restaurant behavior from mixed venues.

## Meaningful differences from Restaurant

### Program- and destination-led visitor journey

Entertainment information supports discovery, arrival, admission, orientation, waiting, participation, viewing, intermission, movement, purchase, disruption, and departure rather than only ordering and service-period decisions. Content priorities may change by event, performance, screening, attraction, exhibit, session, timed entry, crowd condition, weather, or venue state.

### Time-bound programs and sessions

A single venue may run overlapping performances, films, tours, talks, games, activities, exhibits, sessions, rounds, or timed-entry windows. Program, event, attraction, exhibit, session, start time, run time, capacity state, admission state, and location are product/domain state used for organization, targeting, defaults, and presentation. They are not entitlements. Exact hierarchy and future limit counting remain deferred.

### Physical hierarchy and visitor movement

One operation may contain campuses, buildings, entrances, gates, floors, zones, galleries, auditoriums, screens, stages, fields, courts, lanes, rides, exhibits, queues, food outlets, retail, parking, transportation points, and temporary event spaces. Venue, building, area, destination, event, session, queue, and screen context are product/domain state. Manual wayfinding and movement guidance remain core even when mapping or ticketing integrations are absent.

### Capacity, admission, queue, and availability states

Visitors may need to know whether an event is on sale, sold out, full, delayed, boarding, seating, paused, closed, weather-affected, relocated, canceled, or available through another session. Manual communication of these states is part of viable daily operation and must not depend on a premium ticketing, access-control, queue-management, show-control, or venue-management integration. Automatic synchronization remains a later packaging question.

### Mixed scheduled and continuously available experiences

Some experiences are bound to a start time and duration; others are continuously available, self-guided, rotating, seasonal, or capacity-controlled. The product must distinguish venue opening state from the state of a specific event, attraction, exhibit, session, activity, queue, or area and provide clear delivery, stale, and recovery information.

### Public, member, participant, and staff audiences

Displays may serve ticket buyers, members, school groups, families, tourists, participants, spectators, performers, vendors, sponsors, and staff. Audience, admission status, age or access guidance, privacy, authorization, and content authority require clearer separation than a typical single-venue restaurant. Public signage must not expose guest-specific ticket, membership, participant, performer, security, or operational information by default.

### High-motion, crowded, outdoor, and low-light environments

Visitors may be walking, queuing, seated at a distance, outdoors, in dim auditoriums or galleries, in bright concourses, or surrounded by competing visual and audio stimuli. Operational information requires strong hierarchy, restrained motion where comprehension matters, non-color status cues, concise language, and dependable portrait, landscape, large-format, mobile, and distance-reading behavior.

## Content and screen-purpose differences

An Entertainment & Attractions venue may use a combination of:

- entrance, welcome, admissions, membership, ticketing, box-office, and security guidance;
- today-now-next program boards, showtimes, screenings, performances, events, sessions, tours, talks, classes, games, and activity schedules;
- auditorium, screen, stage, gallery, exhibit, attraction, ride, lane, court, field, room, gate, and zone assignments;
- queue, wait-time, boarding, seating, capacity, sold-out, delay, pause, cancellation, relocation, reopening, and weather notices;
- campus, building, floor, concourse, parking, transportation, exit, accessibility, restroom, food, retail, and service wayfinding;
- exhibit, collection, interpretation, education, conservation, sponsor, donor, artist, performer, team, film, production, and attraction information;
- intermission, interval, closing-time, last-entry, last-ride, final-session, and departure guidance;
- venue-wide operational, safety, emergency, evacuation, shelter, and recovery communication;
- staff-facing operational information where permissions and privacy remain explicit;
- brand, atmosphere, trailer, highlight, promotion, merchandise, food-and-beverage, donor, sponsor, and destination content where essential guidance remains dominant.

The profile does not presume that every display shares the same audience, admission condition, schedule, content authority, privacy level, physical environment, or urgency.

## Industry boundary

### Included as native concepts

The profile is intended to support entertainment- and attraction-led concepts including:

- cinemas and motion-picture exhibition venues;
- performing-arts theaters and live-performance venues;
- museums, non-retail galleries, science centers, planetariums, halls of fame, and interpretive institutions;
- zoos, aquariums, botanical gardens, wildlife parks, and similar visitor attractions;
- theme parks, amusement parks, water parks, and mixed-attraction parks;
- family entertainment centers, arcades, bowling centers, and activity-led recreation venues;
- spectator sports venues, arenas, stadiums, fields, courts, and event-led sports facilities;
- attractions, tours, visitor centers, heritage sites, and destination experiences with a managed on-site visitor journey;
- related hybrids where entertainment, exhibition, performance, recreation, or attraction attendance is the primary operating identity.

The exact subtype catalog, definitions, and hybrid rules belong to RWP-00.64.

### Included through venue-level mixed-industry behavior

An Entertainment & Attractions organization may contain Restaurant, Bar/Brewery/Nightlife, Café/Bakery/Dessert, Food Truck/Concession, Hospitality, retail, parking, transportation, membership, education, and other operational venues. Those venues may use their own approved business type while sharing organization-level brand, users, libraries, analytics, and commercial authority.

### Outside the canonical boundary

The following are not treated as native Entertainment & Attractions concepts unless a managed visitor-facing entertainment or attraction operation is also present:

- content production, distribution, broadcasting, streaming, recording, publishing, or artist-management businesses without an on-site visitor venue;
- retail art dealers or commercial galleries primarily selling objects rather than preserving or exhibiting a visitor collection;
- private clubs, gyms, fitness studios, community recreation programs, and participant sports operations without a managed spectator or attraction-led visitor experience;
- gambling-only operations, lotteries, online gaming, and gaming-machine suppliers; casino resorts remain Hospitality when lodging is primary and may contain Entertainment venues;
- restaurants, bars, nightlife venues, hotels, retail centers, offices, schools, houses of worship, and transportation facilities where entertainment is incidental;
- public parks, trails, beaches, natural areas, and informal outdoor sites without a managed attraction, program, admissions, or visitor-information operation;
- temporary private events without a durable venue or operator-managed visitor-information use case.

These boundaries determine Vennusign defaults and profile selection only. They are not legal, licensing, accessibility, safety, capacity, film-rating, gaming, labor, tax, statistical, or admission classifications.

## Organization and venue behavior

### Organization primary industry

- An organization may select Entertainment & Attractions as its primary industry.
- Primary industry seeds organization-level terminology, recommendations, starter content, and first-venue setup.
- Primary industry is product/domain configuration, not a subscription entitlement.
- Changing primary industry must not silently add or remove commercial access.

### Venue and local business type

- Each venue may select Entertainment & Attractions and, later, a supported subtype independently of the organization primary industry.
- A restaurant, bar, café, concession, hotel, retail outlet, or other local operation may use its own approved business type where that produces more accurate defaults and terminology.
- Business type controls defaults, labels, screen-purpose recommendations, starter content, and operational guidance; it does not override organization-level entitlement authority.
- Changing business type must preserve customer content and require explicit review before defaults are replaced.

### Venue, area, attraction, event, session, and screen context

- Venue identity, building or area, destination, attraction, exhibit, event, performance, screening, session, queue, admission window, capacity state, delay, closure, relocation, and similar values are product/domain state when represented.
- Permissions determine who may change them.
- Manual updates, targeting, publishing, delivery confirmation, offline awareness, and recovery remain core.
- Counts of venues, areas, attractions, exhibits, events, sessions, screens, users, connections, content, storage, history, or AI consumption remain usage or quantity limits.

### Mixed organizations

- Entertainment & Attractions and other approved industry types may coexist within one organization.
- Shared brand controls, users, libraries, analytics, and commercial access remain organization concerns unless a later approved policy defines another scope.
- Venue-, area-, attraction-, exhibit-, event-, session-, queue-, and screen-specific terminology, content, operating state, target selection, and defaults remain local.
- Organization-wide views must use neutral language when venue-, property-, outlet-, attraction-, event-, session-, and screen-specific terms would be ambiguous.
- Ownership, management, promoter, presenter, tenant, concession, sponsor, team, performer, distributor, rights-holder, and host relationships must not silently transfer permissions, commercial access, content authority, or visitor data.

## Initial capability-classification rules

RWP-00.63 establishes these rules for later detailed work:

1. Organization primary industry and venue subtype are **product/domain state** that select defaults and recommendations.
2. Venue, building, area, attraction, exhibit, event, performance, screening, session, queue, admission window, capacity state, delay, closure, relocation, and similar operational values are **product/domain state** when represented.
3. Manual program, showtime, event, attraction, exhibit, admissions, wayfinding, queue, capacity, delay, closure, relocation, accessibility, and safety communication; explicit targeting; publishing; delivery confirmation; offline awareness; and recovery remain **core capabilities** required for viable daily operation.
4. Authorization, audience, admission, privacy, and content authority are **permissions or scope concerns**, not commercial access.
5. Automatic ticketing, admissions, access-control, queue-management, venue-management, cinema, show-control, collection-management, event, sports, or other external synchronization remains a future integration-packaging question and must not replace manual core operations.
6. Counts, retention, storage, consumption, and connection allowances are **usage or quantity limits**, not capabilities.
7. Visitor-specific, ticket-specific, member-specific, participant-specific, performer-specific, or sensitive operational information requires explicit later privacy, authorization, and presentation decisions; it is not assumed to be public signage content.

Detailed required, optional, packaging, onboarding, dashboard, analytics, and validation classifications are intentionally deferred to their approved RWPs.

## Impeccable planning guardrails

RWP-00.63 is definition work rather than a detailed UI specification. Because no interactive discovery is available during this scheduled planning run, the project-local Impeccable `shape` guidance is applied with these explicit assumptions for later UI-facing RWPs:

- **Job and audience:** venue operators need to update time-sensitive visitor information, select exact targets, verify delivery, and recover quickly; visitors need immediate orientation, schedule, admission, availability, direction, and next-action information while moving through an unfamiliar or crowded environment.
- **Modes:** administrative surfaces use **Operate** mode; schedules, admissions guidance, wayfinding, exhibit interpretation, and visitor information use **Read** mode; trailers, highlights, collections, performances, destinations, and brand storytelling may use **Experience** only when they do not obscure operational guidance.
- **Primary outcome and proof:** an operator can change an event, session, attraction, exhibit, queue, capacity, delay, closure, or relocation message and verify every intended display received it; a visitor can identify what is happening, where and when it happens, whether it is available, and what action to take next.
- **Hierarchy:** safety and urgent operational guidance, destination or program identity, start time or current state, direction, admission or access requirement, availability, and next action outrank promotional content.
- **Material states:** later specifications must cover first-run, no venue or area configured, no current program, on sale, available, limited, sold out, full, preparing, boarding, seating, active, intermission, delayed, paused, relocated, canceled, weather-affected, unavailable, closed, maintenance, emergency, offline, outdated, permission-restricted, admission-restricted, privacy-restricted, publish-failed, delivered, and recovery conditions where applicable.
- **Realistic ranges:** planning must handle a single-screen neighborhood venue through multi-site organizations with many buildings, areas, auditoriums, attractions, exhibits, events, sessions, languages, roles, and display orientations; short and long names; overlapping programs; continuous, timed, seasonal, and temporary experiences; and no-image through media-rich content.
- **Responsive and environmental behavior:** phone use while walking a venue, compact box-office and operations devices, desktop administration, portrait and landscape displays, large-format boards, bright outdoor queues, dim auditoriums and galleries, crowded concourses, long viewing distances, intermittent networks, and accessibility equipment are binding conditions.
- **Accessibility and localization:** color alone must not communicate status or direction; text, icons, arrows, venue or room references, admission conditions, and time formats must remain unambiguous; motion must be restrained around essential guidance; localization must allow expansion and different reading directions; captions, audio-description, sensory, mobility, and other access information must use plain language and stable placement where applicable.
- **Feedback and recovery:** high-impact, venue-wide, session-wide, or safety changes require explicit scope and target confirmation, visible delivery state, stale/offline distinction, safe undo or restoration, and escalation guidance when a display cannot update.
- **Visual direction:** preserve the approved Sky Blue direction for Vennusign administrative surfaces. Visitor-facing themes may express venue, production, exhibition, team, film, attraction, sponsor, or destination identity without weakening operational hierarchy.

These guardrails shape planning only and authorize no UI implementation.

## Owner decisions and deferred questions

The following are intentionally carried into RWP-00.64 or later RWPs rather than decided here:

- exact definitions and boundaries for cinema, theater, museum, gallery, zoo or aquarium, theme or amusement park, family entertainment center, arcade, bowling, sports venue, live-event venue, attraction or tour, and hybrid subtypes;
- whether drive-ins, water parks, escape rooms, trampoline parks, miniature golf, immersive experiences, haunted attractions, historical sites, botanical gardens, visitor centers, observatories, planetariums, and seasonal attractions are native subtypes, hybrid traits, or later-supported profiles;
- the canonical hierarchy among organization, venue, campus, building, entrance, floor, zone, destination, auditorium, screen, stage, gallery, exhibit, attraction, ride, lane, court, field, room, event, session, queue, and display;
- which hierarchy levels count toward future venue, event, session, or usage limits;
- how owner, operator, promoter, presenter, tenant, concession, sponsor, team, performer, distributor, rights-holder, and host authority is represented;
- how public, ticketed, member-only, participant-only, staff-only, performer-only, sponsor-only, and sensitive information scopes are separated;
- whether ticket-specific, member-specific, seat-specific, participant-specific, or personalized display experiences are supported and under what privacy rules;
- the neutral organization-wide term for properties, venues, attractions, events, sessions, outlets, spaces, and units;
- the default business type for a mixed resort, casino, arena district, cultural campus, entertainment complex, or destination when no single operating model is dominant.

## Reference anchors

These references inform the boundary but do not replace Vennusign's product model:

- [U.S. Census Bureau 2022 NAICS 512131 — Motion Picture Theaters (except Drive-Ins)](https://www.census.gov/naics/?details=512131&input=512131&year=2022) covers establishments primarily engaged in operating motion-picture theaters or exhibiting motion pictures or videos at film festivals.
- [U.S. Census Bureau 2022 NAICS 712 — Museums, Historical Sites, and Similar Institutions](https://www.census.gov/naics/?details=712&input=712&year=2022) covers preservation and exhibition of objects, sites, and natural wonders of historical, cultural, or educational value, including museums, historical sites, zoos, botanical gardens, and related institutions.
- [U.S. Census Bureau 2022 NAICS 713 — Amusement, Gambling, and Recreation Industries](https://www.census.gov/naics/?details=713&input=713&year=2022) provides boundary evidence for amusement parks, arcades, recreation facilities, sports participation, and related amusement or recreation operations.

These references are used only as industry-boundary evidence. They do not define Vennusign entitlements, subtype eligibility, legal obligations, accessibility requirements, privacy rules, admission rules, capacity rules, or limit counting.

## Validation checklist

- [x] Restaurant inheritance is explicit.
- [x] Only meaningful deltas are documented.
- [x] Initial concerns have one primary classification.
- [x] Essential manual program, admissions, wayfinding, queue, capacity, operating-state, targeting, publishing, offline-awareness, and recovery operations remain core.
- [x] Permissions, states, entitlements, add-ons, limits, and rollout flags remain separate.
- [x] Impeccable `shape` guidance was consulted for UI-facing planning.
- [x] Job, audience, hierarchy, states, realistic ranges, accessibility, localization, responsive behavior, feedback, privacy, and recovery are documented.
- [x] No product implementation was performed.
- [x] The next sequential RWP is identified as RWP-00.64.
