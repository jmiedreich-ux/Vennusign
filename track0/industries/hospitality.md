# Hospitality Industry Profile

## Identity

- **Industry:** Hospitality
- **RWP range:** RWP-00.51 through RWP-00.62
- **Current status:** Industry definition complete; subtype definition is next
- **Baseline:** Restaurant
- **Current RWP:** RWP-00.51
- **Next sequential RWP:** RWP-00.52 — Venue Subtypes

## Purpose

This profile covers lodging-led properties whose guest experience depends on accurate arrival, stay, event, amenity, dining, wayfinding, service, and safety information across public areas and changing operating periods.

It inherits the Restaurant baseline. This document records only the meaningful differences needed to establish the Hospitality boundary and guide later subtype, terminology, operations, capability, packaging, onboarding, dashboard, and analytics RWPs.

## Primary customer outcomes

In addition to the Restaurant baseline outcomes, operators must be able to:

- help guests understand where to go and what is available from arrival through departure;
- keep amenity hours, service availability, event schedules, meeting-room assignments, dining information, transportation, and temporary notices current;
- publish reliably to selected lobby, corridor, elevator, conference, amenity, dining, retail, staff, and in-room or guest-service displays;
- coordinate property-wide brand and operational information while allowing building-, area-, outlet-, event-, and screen-specific differences;
- communicate closures, relocations, delays, changed hours, maintenance, weather, safety, and recovery guidance without rebuilding content;
- support multilingual, accessible, calm, and distance-readable presentation for guests who may be unfamiliar with the property;
- confirm that high-impact information reached every intended display and recover safely from stale, offline, or failed delivery.

## Inherited unchanged from Restaurant

Unless a later Hospitality RWP records a meaningful exception, this industry inherits:

- reusable content creation, editing, duplication, archive, restore, preview, and publishing patterns;
- screen pairing and management, explicit targeting, delivery confirmation, offline and outdated detection, and prior-version recovery;
- basic layouts, themes, business hours, venue information, understandable errors, and recovery guidance;
- permissions, product-state separation, limit separation, and packaging discipline;
- candidate scheduling, dayparts, campaigns, coordinated screens, multi-venue sharing, brand controls, approvals, history, analytics, identity, AI, hardware, and integration capabilities.

Restaurant menu, category, item, price, dietary, availability, and special semantics remain inherited for food-and-beverage outlets that use them. They are not assumed to be the primary content model for a lodging property as a whole. Later Hospitality RWPs will define the required lodging vocabulary and content objects without removing Restaurant behavior from mixed properties.

## Meaningful differences from Restaurant

### Property-wide guest journey

Hospitality information supports arrival, orientation, stay, events, services, disruptions, and departure rather than only ordering and service-period decisions. Content priorities can change by time of day, guest location, event program, weather, occupancy pattern, or property condition.

### Property hierarchy and local context

One property may contain buildings, towers, wings, floors, lobbies, meeting rooms, ballrooms, restaurants, bars, pools, spas, retail, transportation points, staff areas, and temporary event spaces. Property, area, outlet, event, and screen context are product/domain state used for organization, defaults, targeting, and presentation. They are not entitlements. Exact hierarchy and future limit counting remain deferred.

### Wayfinding and event changes

Guests frequently lack local knowledge. Manual wayfinding, room assignment, event schedule, amenity, closure, relocation, and changed-hours communication is part of viable daily operation and must not depend on a premium integration. Automatic property-management, event, room-booking, point-of-sale, transport, or guest-service synchronization remains a later packaging question.

### Continuous operation and handoffs

Properties may operate continuously while individual services open, close, relocate, pause, or change ownership across shifts. The product must distinguish property availability from the state of a specific amenity, outlet, event, room, or service and provide clear handoff, delivery, stale, and recovery information.

### Mixed public and operational audiences

Public displays may serve arriving guests, conference attendees, diners, visitors, and local customers. Other displays may support staff operations. Audience, privacy, authorization, and content authority require clearer separation than a typical single-venue restaurant. Hospitality signage must not expose guest-specific or sensitive operational information by default.

### Calm, accessible, multilingual presentation

Guests may be tired, stressed, unfamiliar with the property, carrying luggage, navigating with mobility or sensory needs, or reading in a second language. Wayfinding and service information require strong hierarchy, plain language, restrained motion, non-color status cues, and dependable mobile, portrait, landscape, and distance-reading behavior.

## Content and screen-purpose differences

A Hospitality property may use a combination of:

- arrival, welcome, check-in, concierge, and departure guidance;
- lobby, corridor, elevator, floor, building, parking, and transportation wayfinding;
- meeting-room, ballroom, conference, wedding, group, and event schedules;
- amenity hours, availability, access, closure, relocation, and maintenance notices;
- restaurant, bar, café, room-service, breakfast, retail, spa, pool, fitness, entertainment, and local-experience information;
- weather, transport, shuttle, parking, local-area, and guest-service notices;
- property-wide operational, safety, emergency, and recovery communication;
- staff-facing operational information where permissions and privacy remain explicit;
- brand, atmosphere, destination, loyalty, promotion, and sponsor content where essential guidance remains dominant.

The profile does not presume that every display shares the same audience, hours, content authority, privacy level, or physical environment.

## Industry boundary

### Included as native concepts

The profile is intended to support lodging-led concepts including:

- hotels, resort hotels, and motels;
- boutique and lifestyle lodging properties;
- hostels and guest-house-style traveler accommodation;
- extended-stay hotels and serviced-apartment lodging operations;
- conference, convention, wedding, group, and event-led lodging properties;
- casino resorts where lodging is a primary property function;
- related hybrids where short-term or extended-stay guest accommodation is the primary operating identity.

The exact subtype catalog, definitions, and hybrid rules belong to RWP-00.52.

### Included through venue-level mixed-industry behavior

A Hospitality organization may contain Restaurant, Bar/Brewery/Nightlife, Café/Bakery/Dessert, Food Truck/Concession, Entertainment/Attractions, retail, spa, meeting, and other operational venues. Those venues may use their own approved business type while sharing organization-level brand, users, libraries, analytics, and commercial authority.

### Outside the canonical boundary

The following are not treated as native Hospitality concepts unless a lodging-led guest operation is also present:

- residential apartment, condominium, dormitory, or long-term housing operations without a traveler-lodging service model;
- vacation-rental or property-listing businesses with no managed shared guest-facing property environment;
- hospitals, rehabilitation centers, assisted-living, senior-living, or other care facilities;
- standalone restaurants, bars, attractions, casinos, conference centers, event venues, retail centers, offices, and transportation facilities without a lodging-led property;
- campgrounds, recreational-vehicle parks, cruise ships, and broad travel operations until separately approved;
- private homes or informal accommodation without a managed property-level signage use case.

These boundaries determine Vennusign defaults and profile selection only. They are not legal, licensing, accessibility, safety, tax, lodging, gaming, tenancy, or statistical classifications.

## Organization and venue behavior

### Organization primary industry

- An organization may select Hospitality as its primary industry.
- Primary industry seeds organization-level terminology, recommendations, starter content, and first-property setup.
- Primary industry is product/domain configuration, not a subscription entitlement.
- Changing primary industry must not silently add or remove commercial access.

### Property and local business type

- Each property may select Hospitality and, later, a supported subtype independently of the organization primary industry.
- A restaurant, bar, café, concession, attraction, spa, retail outlet, conference venue, or other local operation may use its own approved business type where that produces more accurate defaults and terminology.
- Business type controls defaults, labels, screen-purpose recommendations, starter content, and operational guidance; it does not override organization-level entitlement authority.
- Changing business type must preserve customer content and require explicit review before defaults are replaced.

### Property, area, outlet, event, and screen context

- Property identity, building or area, outlet, room or event assignment, service window, amenity state, closure, relocation, and similar values are product/domain state when represented.
- Permissions determine who may change them.
- Manual updates, targeting, publishing, delivery confirmation, offline awareness, and recovery remain core.
- Counts of properties, venues, areas, outlets, rooms, events, screens, users, connections, storage, history, or AI consumption remain usage or quantity limits.

### Mixed organizations

- Hospitality and other approved industry types may coexist within one organization.
- Shared brand controls, users, libraries, analytics, and commercial access remain organization concerns unless a later approved policy defines another scope.
- Property-, area-, outlet-, event-, and screen-specific terminology, content, operating state, target selection, and defaults remain local.
- Organization-wide views must use neutral language when property-, venue-, outlet-, room-, and event-specific terms would be ambiguous.
- Management, franchise, ownership, operator, tenant, and host relationships must not silently transfer permissions, commercial access, content authority, or guest data.

## Initial capability-classification rules

RWP-00.51 establishes these rules for later detailed work:

1. Organization primary industry and property subtype are **product/domain state** that select defaults and recommendations.
2. Property, building, area, outlet, room, event, amenity, service-window, closure, relocation, and similar operational values are **product/domain state** when represented.
3. Manual guest-information, wayfinding, event, amenity, service, closure, relocation, and changed-hours communication; explicit targeting; publishing; delivery confirmation; offline awareness; and recovery remain **core capabilities** required for viable daily operation.
4. Authorization and content authority are **permissions**, not commercial access.
5. Automatic property-management, event, room-booking, point-of-sale, transport, guest-service, or other external synchronization remains a future integration-packaging question and must not replace manual core operations.
6. Counts, retention, storage, consumption, and connection allowances are **usage or quantity limits**, not capabilities.
7. Guest-specific, reservation-specific, room-specific, or sensitive operational information requires explicit later privacy, authorization, and presentation decisions; it is not assumed to be public signage content.

Detailed required, optional, packaging, onboarding, dashboard, and analytics classifications are intentionally deferred to their approved RWPs.

## Impeccable planning guardrails

RWP-00.51 is definition work rather than a detailed UI specification. Because no interactive discovery is available during this scheduled planning run, the project-local Impeccable `shape` guidance is applied with these explicit assumptions for later UI-facing RWPs:

- **Job and audience:** property operators work across shifts and locations and need to update guest information, select exact targets, verify delivery, and recover quickly; guests need calm, immediate orientation and current service information in unfamiliar surroundings.
- **Modes:** administrative surfaces use **Operate** mode; guest information and wayfinding use **Read** mode; destination or brand storytelling may use **Experience** only when it does not obscure operational guidance.
- **Primary outcome and proof:** an operator can change a property, area, outlet, event, amenity, or service message and verify every intended display received it; a guest can identify where to go, what is open or changed, and what action to take next.
- **Hierarchy:** safety and urgent operational guidance, destination or room identity, direction, event or service state, time, access instructions, and delivery health outrank promotional content.
- **Material states:** later specifications must cover first-run, no property or area configured, no events, scheduled, preparing, active, changed, delayed, relocated, full, unavailable, closed, maintenance, emergency, offline, outdated, permission-restricted, privacy-restricted, publish-failed, delivered, and recovery conditions where applicable.
- **Realistic ranges:** planning must handle a small property with a few screens through multi-property organizations with many buildings, areas, outlets, events, languages, roles, and display orientations; short and long names; overlapping events; continuous and seasonal operations; and no-image through media-rich content.
- **Responsive and environmental behavior:** phone use while walking the property, compact front-desk and back-office devices, desktop administration, portrait and landscape displays, bright lobbies, dim corridors, crowded conferences, long viewing distances, intermittent networks, and accessibility equipment are binding conditions.
- **Accessibility and localization:** color alone must not communicate status or direction; text, icons, arrows, floor or room references, and time formats must remain unambiguous; motion must be restrained; localization must allow expansion and different reading directions; recovery guidance must use plain language.
- **Feedback and recovery:** high-impact or property-wide changes require explicit scope and target confirmation, visible delivery state, stale/offline distinction, safe undo or restoration, and escalation guidance when a display cannot update.
- **Visual direction:** preserve the approved Sky Blue direction for Vennusign administrative surfaces. Guest-facing themes may express property, destination, event, franchise, or luxury identity without weakening wayfinding and operational hierarchy.

These guardrails shape planning only and authorize no UI implementation.

## Owner decisions and deferred questions

The following are intentionally carried into RWP-00.52 or later RWPs rather than decided here:

- exact definitions and boundaries for hotel, resort, motel, hostel, extended-stay, serviced-apartment, conference-property, casino-resort, boutique, and hybrid subtypes;
- whether bed-and-breakfast, guest house, lodge, aparthotel, vacation-club, branded residence, campground, and cruise concepts are native, hybrid, or later-supported profiles;
- the canonical hierarchy among organization, property, building, tower, wing, floor, area, outlet, room, event, and screen;
- which hierarchy levels count toward future venue or usage limits;
- how franchise, ownership, management-company, tenant, concession, and third-party event authority is represented;
- how public, guest-only, staff-only, event-only, and sensitive information scopes are separated;
- whether room-specific, reservation-specific, or personalized display experiences are supported and under what privacy rules;
- the neutral organization-wide term for properties, venues, outlets, spaces, and units;
- the default business type for a mixed resort, casino, conference, or entertainment property when no single operating model is dominant.

## Reference anchors

These references inform the boundary but do not replace Vennusign's product model:

- [U.S. Census Bureau 2022 NAICS 721110 — Hotels (except Casino Hotels) and Motels](https://www.census.gov/naics/?details=721110&input=721110&year=2022) covers short-term lodging in hotels, resort hotels, and motels, often with food, recreation, conference, laundry, parking, and related services.
- [U.S. Census Bureau 2022 NAICS 721120 — Casino Hotels](https://www.census.gov/naics/?details=721120&input=721120&year=2022) distinguishes lodging properties with a casino on the premises from standalone casinos and non-casino hotels.
- [U.S. Census Bureau 2022 NAICS 72119 — Other Traveler Accommodation](https://www.census.gov/naics/?details=72119&input=72119&year=2022) includes short-term lodging such as bed-and-breakfast inns, guest houses, hostels, and housekeeping cabins or cottages outside the hotel, motel, and casino-hotel categories.

These references are used only as industry-boundary evidence. They do not define Vennusign entitlements, subtype eligibility, legal obligations, privacy rules, or limit counting.

## Validation checklist

- [x] Restaurant inheritance is explicit.
- [x] Only meaningful deltas are documented.
- [x] Initial concerns have one primary classification.
- [x] Essential manual guest-information, wayfinding, service-state, targeting, publishing, offline-awareness, and recovery operations remain core.
- [x] Permissions, states, entitlements, add-ons, limits, and rollout flags remain separate.
- [x] Impeccable `shape` guidance was consulted for UI-facing planning.
- [x] Job, audience, hierarchy, states, realistic ranges, accessibility, localization, responsive behavior, feedback, privacy, and recovery are documented.
- [x] No product implementation was performed.
- [x] The next sequential RWP is identified as RWP-00.52.
