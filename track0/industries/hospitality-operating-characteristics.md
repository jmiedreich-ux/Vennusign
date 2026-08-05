# Hospitality Operating Characteristics

## Authority and scope

This document is the canonical operating-characteristics companion to `track0/industries/hospitality.md` and `track0/industries/hospitality-terminology.md` for RWP-00.54.

It describes the daily operating rhythms that distinguish Hospitality from the Restaurant baseline and ties them to product defaults, content and screen-purpose guidance, capability presentation, state, permission, tier, add-on, limit, and rollout classifications. It does not implement workflows, integrations, privacy systems, or property-management behavior.

## Canonical operating model

Hospitality properties may operate continuously while individual services, amenities, outlets, meetings, events, transport points, public areas, and screens follow different schedules and authority boundaries.

The canonical model therefore separates:

- the overall property operating state;
- arrival, check-in, stay, departure, and check-out cycles;
- the state of each amenity, outlet, service, event, meeting space, route, and area;
- regular, effective, special, access, setup, service, and recovery periods;
- public, staff, restricted, and privacy-sensitive audiences;
- manual, scheduled, imported, overridden, stale, unknown, and restored information;
- content approval, publishing, delivery, offline, outdated, and recovery states.

These are product/domain values and operating relationships. They do not by themselves grant commercial access or authority.

## Continuous property operation

A property may be open twenty-four hours even when many local services are closed. Future surfaces must not collapse property state into the state of every amenity or outlet.

Required distinctions include:

- **Property open:** the lodging property continues operating.
- **Arrival support available:** an authorized arrival or reception service is available; this may differ from full-service check-in.
- **After-hours operation:** the property is operating with reduced services, access rules, staffing, or entrance patterns.
- **Individual service closed or unavailable:** a specific amenity, outlet, event, route, or service is not operating.
- **Restricted access:** a place or service is available only to an eligible audience or during an access period.
- **Emergency or safety state:** a temporary high-priority condition changes ordinary guidance without redefining normal hours.

The product must preserve the last known good public content during temporary delivery failure and show operators which screens are offline, outdated, unknown, or confirmed current.

## Shifts and operational handoffs

Hospitality operation frequently spans front-desk, concierge, events, food and beverage, engineering, housekeeping, security, transport, guest services, and management teams.

A handoff must preserve:

- current notices and their effective periods;
- service, amenity, outlet, event, meeting-space, transport, and route state;
- source, freshness, manual overrides, and unresolved conflicts;
- pending approvals and scheduled publishes;
- target screens, delivered versions, failures, offline screens, and recovery work;
- public versus staff and restricted audience boundaries;
- the last known good state and restoration path.

Shift identity and staff notes may be sensitive. Public signage must never expose internal assignments, unresolved security matters, guest-specific details, room-specific incidents, or confidential operational notes.

Advanced shift logs, acknowledgments, task assignment, escalation, and audit workflow may be later tier candidates. Basic current-state visibility, clear ownership, publish confirmation, and recovery remain core.

## Arrival, check-in, stay, departure, and check-out cycles

The guest journey changes content priority without requiring personalized data.

### Arrival period

Defaults should prioritize:

- property identity and entrance confirmation;
- reception or check-in direction;
- parking, drop-off, transport, luggage, accessibility, and late-arrival guidance;
- building, tower, wing, floor, and room-range wayfinding;
- current amenity, outlet, and service availability;
- high-impact notices, closures, route changes, and safety guidance.

### During-stay period

Defaults should prioritize:

- today’s amenity, outlet, activity, event, and transport schedules;
- service hours and temporary changes;
- meeting and event directories;
- wayfinding and accessible-route changes;
- weather, maintenance, and local-area guidance;
- recovery from stale, unavailable, or failed information.

### Departure period

Defaults should prioritize:

- check-out, transport, parking, luggage, breakfast, and departure guidance;
- confirmed service or route changes;
- property-specific next steps without exposing an individual stay.

Personalized room-ready, reservation, itinerary, billing, loyalty, or service-request content is not assumed. It requires later privacy, authorization, audience, source, and delivery decisions.

## Guest notices and operational communication

Manual notices remain core and must support:

- changed hours;
- temporary closure or unavailable service;
- delay, cancellation, pause, relocation, and reopening;
- maintenance and out-of-service equipment;
- weather, transport, parking, entrance, and route changes;
- event or meeting-room changes;
- amenity and outlet state;
- accessible-route changes;
- safety and emergency instructions;
- recovery or all-clear communication.

A notice must have a named scope, audience, effective period, source or author, public wording, target screens, and removal or supersession behavior. Unknown timing remains unknown. High-impact notices require preview, explicit targeting, delivery confirmation, and clear recovery.

## Amenities and services

Amenities and services have independent hours, access conditions, availability, location, audience, source, and recovery state.

Examples include pools, spas, fitness centers, lounges, business centers, laundry, shared kitchens, clubs, recreation, housekeeping, concierge, room service, breakfast, shuttle, luggage storage, parking, package handling, and local transport.

Future capability presentation should prioritize:

- current availability and effective hours;
- location and access condition;
- temporary closure, limited service, maintenance, relocation, or unknown state;
- clear alternatives only when authoritative;
- the next confirmed update;
- source and freshness for operators;
- privacy-safe guest wording.

Live capacity, room access, reservation eligibility, service-request status, transport location, and predicted wait require authoritative integrations and privacy decisions. Manual general guidance remains core.

## Food and beverage outlets

Restaurants, bars, cafés, room service, breakfast areas, concessions, and related outlets inherit their approved local-industry terminology and capabilities.

Property-wide Hospitality surfaces may summarize:

- outlet identity and location;
- current hours and service period;
- open, limited, closed, temporarily closed, relocated, or unknown state;
- reservation or access guidance without implying eligibility;
- menu or dining discovery where authorized;
- changed service, last seating, last order, and recovery information.

Outlet-specific menu, price, item, dietary, availability, ordering, payment, inventory, and point-of-sale behavior remains governed by the local business type and later integration packaging.

## Meetings, conferences, weddings, and events

Hospitality event operation may involve group arrivals, registration, meeting rooms, ballrooms, breakouts, receptions, dining periods, sponsor content, room changes, and rapid schedule updates.

Manual event directories, room assignments, schedule changes, relocation, cancellation, delay, wayfinding, break guidance, and public notices remain core.

The product must distinguish:

- event from session;
- meeting space from guest room;
- setup, public event, service, and teardown periods;
- private or restricted group content from public directories;
- imported schedule from approved public wording;
- host, planner, property, outlet, sponsor, and operator authority;
- planned location from current confirmed location.

Advanced event ingestion, room-booking synchronization, registration, attendee identity, sponsor workflow, group itinerary, and cross-property event orchestration remain later tier or add-on candidates.

## Wayfinding and temporary route changes

Hospitality wayfinding must support unfamiliar guests across entrances, lobbies, towers, wings, floors, elevators, stairs, parking, transport points, meeting spaces, amenities, outlets, and temporary event areas.

Core manual communication includes:

- destination and route context;
- current-location context only when authoritative;
- building, tower, wing, floor, zone, and landmark;
- entrance, elevator, stair, parking, and transport guidance;
- temporary closure, alternate route, and relocation;
- verified accessible route;
- next action or contact point.

Live indoor positioning, mapping, route optimization, occupancy, and personalized navigation remain optional integration candidates. Manual clear directions and route-change notices remain core.

## Emergency, safety, and high-priority messaging

Vennusign does not define legal, emergency, security, fire, medical, accessibility, gaming, or safety policy.

It must nevertheless support authorized high-priority public communication with:

- explicit scope and audience;
- approved message source and authority;
- priority over ordinary promotional content where later implementation permits;
- preview and confirmation for intended screens;
- delivery, offline, outdated, and unknown-target visibility;
- concise plain-language instructions;
- accessible non-color-only presentation;
- controlled end, replacement, all-clear, and restoration behavior;
- protection against guest-specific, security-sensitive, or confidential detail.

External emergency, fire, building, security, weather, transport, and safety systems remain integration candidates. Manual authorized messaging and delivery confidence remain core.

## Multilingual and accessible operation

Properties may serve guests who read in a second language or use assistive technology while navigating unfamiliar environments.

Basic manually authored accessible content remains core. Future surfaces and display guidance must support:

- plain language and complete translatable messages;
- long labels and text expansion;
- right-to-left readiness;
- local date and time clarity;
- 200% zoom in administrative surfaces;
- keyboard and assistive-technology access;
- non-color-only state cues;
- restrained motion;
- strong hierarchy and distance readability;
- accessible-route and service information only when authoritative.

Translation workflow, premium localization, translation memory, automated translation, and AI assistance remain later tier or add-on candidates. Generated or translated content remains reviewable product state.

## Property groups and multi-property operation

One organization may manage independent brands, different subtypes, multiple buildings, regional properties, or shared service teams.

Required behavior includes:

- explicit organization, property, building, area, venue, outlet, event, and screen scope;
- local terminology and operational overrides;
- shared templates and libraries without silent overwrite;
- safe copying with local review of hours, location, audience, authority, privacy, and targets;
- mixed-state visibility before bulk actions;
- property-specific source and freshness;
- clear ownership and approval boundaries;
- preservation of local history and restore points.

Advanced cross-property coordination, brand governance, approval chains, centralized operations, shared event programs, and portfolio analytics may be tier candidates. Property, building, room, venue, event, user, screen, integration, and history quantities remain independent limits.

## Subtype operating differences

| Subtype | Dominant rhythm | Default emphasis |
| --- | --- | --- |
| **Hotel** | daily arrival/stay/departure cycles with balanced services | lobby orientation, check-in/out guidance, room and floor wayfinding, amenities, outlets, meetings, departures |
| **Resort** | continuous destination operation with activities, transport, weather, and broad amenities | campus navigation, activity schedules, transport, weather, seasonal service, closures, family/adult zones |
| **Motel** | vehicle-led arrival, exterior circulation, compact services, late arrival | entrances, parking, building and room ranges, office/front desk, breakfast, outdoor readability |
| **Hostel** | shared accommodation, communal facilities, activities, quiet hours, multilingual operation | reception, dormitory/private-room zones, shared facilities, community schedule, privacy-safe guidance |
| **Extended-Stay** | longer-horizon recurring services and resident-like rhythms | housekeeping cycles, laundry, packages, kitchens, workspace, weekly service, longer-term notices |
| **Serviced Apartment** | apartment-style units, building access, remote or staffed arrival, recurring services | building/floor/unit zones, reception, access guidance, housekeeping windows, shared amenities, local services |
| **Conference Property** | group arrivals, directories, sessions, room changes, breaks, receptions, rapid turnover | event directories, meeting spaces, schedule changes, relocation, sponsor/host content, coordinated screens |
| **Casino Resort** | continuous high-volume circulation with gaming, entertainment, dining, loyalty, and restricted areas | towers, entrances, gaming areas, event schedules, dining, age/access wording, public/restricted boundaries |
| **Boutique Lodging** | flexible service, curated experience, local recommendations, staff-authored updates | distinctive arrival, local guidance, curated amenities, flexible hours, calm operational clarity |
| **Neutral** | mixed or uncertain property rhythm | property, accommodation, area, venue, outlet, amenity, service, event, notice, destination, screen health |

Subtype changes defaults and capability emphasis only. It does not unlock features, change permissions, increase limits, transfer authority, or alter privacy.

## Capability-presentation implications

Future Hospitality Operate surfaces should prioritize the current operational job:

- **Start of shift:** exceptions, unresolved notices, upcoming arrivals and events, effective hours, screen health, pending publishes, and recovery items.
- **Arrival period:** entrances, reception, transport, parking, luggage, wayfinding, amenity/outlet state, and high-impact notices.
- **During stay:** today’s schedules, service and amenity state, meetings, activities, wayfinding, weather, maintenance, and delivery confidence.
- **Event surge:** event directory, schedule and room changes, routes, breaks, outlet impact, sponsor/host boundaries, screen synchronization, and rapid recovery.
- **Disruption:** affected scope, source and authority, privacy-safe public message, target preview, delivery result, offline/outdated screens, next update, and restoration.
- **Departure period:** check-out, transport, parking, luggage, breakfast, routes, and confirmed service changes.
- **Portfolio view:** exceptions and mixed states first, explicit scope, safe bulk actions, local overrides, and no silent cross-property publication.

The project-local Impeccable `shape` and `harden` guidance applies: explicit task and scope, strong hierarchy, progressive disclosure, first-use/empty/loading/permission/validation/stale/conflict/offline/delivery-failure/success/undo/restoration states, phone and desktop layouts, long names, localization expansion, 200% zoom, keyboard and assistive-technology access, non-color-only state, high-scope confirmation, actionable recovery, and the approved Sky Blue administrative direction.

## Classification decisions

1. Property, service, amenity, outlet, event, meeting space, route, hours, operating state, notice, audience, source, freshness, override, and delivery state are **product/domain state** where represented.
2. Manual property information, notices, hours, amenity/outlet/service state, event/meeting communication, wayfinding, emergency messaging, targeting, publishing, delivery confirmation, offline/outdated awareness, and restoration are **core capabilities**.
3. Permissions control who may edit, approve, publish, restore, view restricted information, or act for each property and object scope.
4. Advanced shift workflow, cross-property coordination, brand governance, approvals, event orchestration, localization workflow, managed monitoring, and portfolio operations are **tier-entitlement candidates**.
5. PMS, event, room-booking, transport, POS, guest-service, access, gaming, mapping, emergency, weather, translation, AI, and other automatic synchronization or assistance are **add-on candidates** where external capability is required.
6. Counts of properties, buildings, rooms, venues, outlets, amenities, services, events, screens, users, languages, integrations, storage, history, and AI use are **limits**.
7. Staged delivery, experiments, migrations, compatibility controls, and emergency disable controls are **internal rollout flags**.
8. Industry, subtype, state, permission, entitlement, add-on, limit, source authority, privacy, and rollout remain separate.

## Boundaries

Documentation and planning only. No UI, API, schema, migration, billing, entitlement, permission, privacy, localization, analytics, PMS, event, room-booking, transport, POS, guest-service, access, gaming, mapping, emergency, weather, translation, AI, hardware, or integration implementation.

RWP-13.06 and Phase 14+ remain paused. RWP-00.55 owns Hospitality Required Capabilities.