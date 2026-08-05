# RWP-00.54 — Hospitality Operating Characteristics

## Status

Complete in this proposed merge state.

## Issue

- #529

## Objective

Define the operating characteristics that distinguish Hospitality from the inherited Restaurant baseline, including continuous operation, shifts and handoffs, arrival and departure cycles, guest notices, amenities, outlets, meetings and events, wayfinding, emergency messaging, multilingual needs, property groups, and subtype differences. Tie those characteristics to product defaults, capability presentation, and Track 0 classification without implementing product behavior.

## Dependency verified

- RWP-00.53 is merged, verified, closed, and released.
- Restaurant remains the canonical baseline for embedded food-and-beverage outlets.
- The merged Hospitality industry, subtype, and terminology records are authoritative.
- RWP-00.55 — Hospitality Required Capabilities (#530) is the approved next item.

## Operating model

Hospitality is a lodging-led, guest-facing operating environment where public information may remain relevant continuously even when individual services, outlets, amenities, desks, events, or buildings follow different schedules.

Its distinguishing operating model combines:

- continuous property presence across days, nights, weekends, holidays, and overnight date boundaries;
- shift-based operator responsibility and explicit handoff needs;
- recurring arrival, check-in, stay, departure, and check-out cycles without exposing private guest data;
- many independently changing amenities, outlets, services, event spaces, routes, notices, and screens;
- public information for guests, visitors, attendees, staff-facing audiences, and restricted groups;
- planned and unplanned changes involving closures, relocations, delays, maintenance, weather, transport, and access;
- multiple authoritative sources whose freshness, scope, and privacy requirements differ;
- property-group standards with local property ownership, overrides, and exceptions;
- multilingual and accessibility requirements that must remain usable during time-sensitive operation.

These characteristics tune defaults, guidance, starter content, screen-purpose suggestions, task priority, and presentation. They do not grant commercial access, create permissions, increase limits, or become rollout flags.

## Continuous property operation

A property may remain open while every represented object follows its own operating state and effective period. Property open state must not be inferred from a front desk, outlet, amenity, event, shuttle, entrance, or service state.

The operating model must keep separate:

- property-wide status;
- building, tower, wing, floor, area, or zone status;
- accommodation access context without public guest assignment data;
- front desk, reception, concierge, security, or service-desk hours;
- outlet, amenity, service, transport, event, meeting-space, and route status;
- screen online, content, delivery, and freshness state;
- ordinary hours, today’s effective hours, date-specific exceptions, access hours, and overnight periods.

Overnight ranges must show complete local dates and times. A later implementation must not silently close a still-operating service at midnight, carry yesterday’s exception into today, or present a future schedule as current.

Manual property and service communication remains core even when imported schedules or property systems are unavailable.

## Shifts, handoffs, and operating ownership

Hospitality work commonly moves between day, evening, overnight, event, engineering, food-and-beverage, guest-service, and management teams.

A future operator experience should make the following visible at handoff:

- active guest-facing notices and their effective periods;
- changed hours, closures, relocations, restrictions, and expected next updates;
- unpublished drafts, scheduled publications, failed or partial deliveries, and outdated screens;
- active manual overrides and the source they override;
- stale, disconnected, conflicting, or unknown imported values;
- affected property, area, venue, amenity, service, event, screen, language, and audience scope;
- last successful publication and available restoration point;
- unresolved actions and the responsible role or team when represented.

Shift assignment, acknowledgment, approval, escalation, and task-routing workflows are later candidates. They may improve coordination but cannot replace core visibility, manual correction, publishing, confirmation, and recovery.

## Arrival, stay, and departure cycles

Hospitality content changes around recurring operating periods:

1. **Pre-arrival / before expected traffic:** confirm property information, entrances, reception, parking, transport, check-in guidance, today’s notices, events, screens, and languages.
2. **Arrival and check-in periods:** prioritize reception location, queue or alternate-desk guidance, luggage, parking, transport, access routes, amenities, outlets, meetings, and immediate notices.
3. **During stay:** prioritize service and amenity hours, outlet availability, events, activities, wayfinding, transport, property updates, and disruption communication.
4. **Departure and check-out periods:** prioritize check-out guidance, luggage, transport, breakfast, parking, exits, route changes, and any confirmed service limitations.
5. **Overnight operation:** prioritize reception or contact guidance, overnight entrances, quiet-hour or access information, safety notices, next-day schedules, and accurate date boundaries.

These are presentation priorities, not private guest-state automation. Public screens must not expose names, room assignments linked to a person, reservation codes, stay dates, loyalty or payment state, requests, itineraries, or inferred eligibility.

## Guest notices and operational changes

Manual guest notices are core and must support bounded scope, effective time, audience, priority, language, target, preview, publication result, and recovery.

Common notice types include:

- changed service, amenity, outlet, desk, transport, or access hours;
- temporary closure, limited operation, delay, pause, relocation, cancellation, or reopening;
- maintenance or weather effect stated without sensitive technical detail;
- entrance, elevator, corridor, floor, building, parking, or route change;
- event, meeting, function-space, registration, or room reassignment when public and authorized;
- shuttle, transfer, pickup, drop-off, or transport update;
- property-wide information or an area-specific instruction;
- emergency or urgent operator-authored instruction from an authorized source.

A notice must not imply certainty that the source does not support. “Expected,” “scheduled,” “not confirmed,” and “next update by” remain distinct.

## Amenities, services, and outlets

Each amenity, service, and outlet may have independent:

- identity and property location;
- regular, today’s, special, access, and overnight hours;
- open, closed, limited, unavailable, out-of-service, paused, delayed, relocated, restricted, or unknown state;
- audience or access condition without assuming eligibility;
- guest-facing description and next action;
- manual or imported source, freshness, override, and conflict state;
- screen and content target scope.

Embedded restaurants, bars, cafés, bakeries, concessions, retail, spa, gaming, entertainment, and other venues retain their approved local-industry terminology where applicable. Hospitality supplies the property context and cross-property guest journey; it does not replace the venue’s own operating model.

## Meetings, events, and function spaces

Hospitality properties may host many concurrent meetings, conferences, weddings, sessions, receptions, activities, performances, and private functions.

The operating model should support public and authorized representations of:

- official event or group display name;
- date, local time, status, audience, and source freshness;
- property, building, floor, room, ballroom, function-space, registration, or destination assignment;
- session changes, delays, cancellations, relocations, room changes, and route notices;
- shared spaces with turnover, setup, cleaning, access, or event-specific periods;
- public event directories and selected private-group displays without exposing restricted attendee information.

Manual event and meeting display remains core. External event, sales, room-booking, ticketing, or conference-system synchronization is an add-on candidate. Advanced approval, template, and group-coordination workflows may be tier candidates.

## Wayfinding and changing routes

Wayfinding is operational content, not merely static decoration. It may change because of events, closures, maintenance, accessibility needs, crowd management, weather, or temporary entrances.

Future surfaces should make operators confirm:

- the authoritative destination name;
- property, building, tower, wing, floor, zone, or landmark context;
- current source location only when authoritative;
- route status, temporary change, restricted area, elevator or entrance effect, and alternate route;
- accessibility information only when verified;
- effective period, affected screens, language, and next action.

Manual destination and route communication remains core. Interactive maps, positioning, indoor navigation, sensors, and live routing are later tier or add-on candidates. Unsupported distance, effort, travel time, accessibility, or current-location claims must not be invented.

## Emergency and urgent messaging

Track 0 does not define emergency policy, legal obligations, safety procedures, alarm control, dispatch, or life-safety integration.

It does require a safe planning boundary for authorized urgent communication:

- distinguish urgent guest communication from ordinary promotional content;
- identify source, authority, property or area scope, audience, language, effective time, and targets;
- preview high-impact changes and protect against accidental organization-wide publication;
- preserve mandated or higher-authority content;
- show publication and delivery state by target;
- support correction, expiration, supersession, and restoration without hiding the audit trail;
- keep sensitive operational and security detail out of public content.

Manual authorized urgent notices and reliable publishing are core. Mass-notification, alarm, security, weather, emergency-management, or government-feed integrations are independent add-on candidates and require explicit source, privacy, authority, freshness, fallback, and failure behavior.

## Multilingual operation

Basic manual guest communication must not require a premium integration. At minimum, future planning must support:

- one clearly identified source language;
- manually authored alternate-language content where the customer supplies it;
- visible language coverage and missing-language state;
- local dates, times, numerals, units, names, and directionality;
- content expansion, right-to-left layouts, long names, and 200% zoom;
- non-text accessibility and readable fallback when a translation is missing;
- per-language preview, target scope, publication state, and restoration.

Translation workflow, automated translation, terminology libraries, quality review, AI assistance, and large language portfolios may be tier or add-on candidates. Generated or imported language must remain reviewable and must not silently replace approved manual content.

## Property groups and local control

One organization may manage multiple properties with shared brands, libraries, policies, templates, terminology, campaigns, languages, integrations, or reporting while retaining local operational responsibility.

The operating model requires:

- explicit organization, property-group, property, venue, object, and screen scope;
- local override without silently breaking source relationships or group standards;
- mixed-state visibility across properties;
- safe bulk actions with explicit selection, preview, excluded targets, and confirmation;
- preservation of local names, dates, times, routes, events, languages, authorities, and last known good content;
- no assumption that ownership, management, brand, franchise, commercial access, or permission is shared merely because properties appear in one group.

Group coordination, approval, campaign, analytics, and enterprise administration may be tier candidates. External property-system connections remain add-on candidates. Property, screen, user, language, integration, event, history, storage, and similar quantities remain limits.

## Subtype operating differences

| Subtype | Dominant operating rhythm | Default emphasis |
| --- | --- | --- |
| Hotel | continuous property operation with daily arrival/departure peaks | reception, guest services, rooms context, outlets, amenities, meetings, wayfinding, notices |
| Resort | destination campus with activities, recreation, transport, and dispersed venues | activity schedules, transport, campus wayfinding, weather, pools/beach/spa, outlets, events |
| Motel | simpler property structure with exterior access and strong parking/arrival context | office/front desk, room ranges, parking, exterior routes, breakfast, changed access |
| Hostel | shared facilities, dormitory/private-room mix, activities, quiet and access periods | reception, shared kitchen, common areas, activities, quiet hours, privacy-safe public wording |
| Extended-Stay | longer operating cycles and recurring service schedules | housekeeping windows, laundry, kitchenette guidance, package areas, weekly service, amenities |
| Serviced Apartment | apartment-style units with building and reception variation | building/floor context, reception hours, housekeeping, shared amenities, access and transport |
| Conference Property | meeting-led peaks, room turnover, registration, and many concurrent sessions | event directory, meeting spaces, session changes, registration, wayfinding, group notices |
| Casino Resort | continuous destination operation with gaming, entertainment, dining, security, and restricted areas | tower/campus context, entertainment, outlets, events, transport, restrictions; security detail private |
| Boutique Lodging | smaller operation with customer-authored identity and curated local guidance | house/inn/lodge terminology, host/front desk, local information, personalized but non-private content |
| Neutral / mixed | uncertain or multi-subtype operation | property, accommodation, area, venue, service, notice, destination, screens |

Subtype affects defaults and presentation only. It does not grant capabilities, set permissions, create packages, change privacy, increase limits, or control rollout.

## Capability-presentation implications

Future Hospitality surfaces are **Operate** experiences for authorized property teams. The project-local Impeccable `shape` and `harden` guidance is applied as planning direction.

Presentation should prioritize:

- **At shift start:** active notices, exceptions, stale sources, failed deliveries, outdated screens, scheduled changes, and unresolved handoff items.
- **During arrival/departure peaks:** reception, access, transport, parking, outlet and amenity hours, wayfinding, notices, and screen health.
- **During events:** meeting and function-space changes, registration, directories, routes, language coverage, affected targets, and publication result.
- **During disruption:** authoritative state, affected scope, privacy-safe public wording, language coverage, targets, delivery state, next update, and recovery.
- **Across properties:** exceptions first, local time and date, mixed states, excluded targets, and safe bulk actions.

Required planned states include first use, empty, loading, permission, validation, stale source, source conflict, offline, outdated, publish failure, partial delivery, success, undo, restoration, missing translation, long names, local date/time boundaries, keyboard and assistive-technology operation, non-color-only status, 200% zoom, and phone through large-desktop layouts. Preserve the approved Sky Blue administrative direction.

No UI or product implementation is authorized by this planning result.

## Classification decisions

1. Property, building, area, accommodation context, amenity, service, outlet, event, meeting space, destination, notice, schedule, hours, audience, language coverage, source, freshness, operating state, effective time, override, and delivery state are **product/domain state** where represented.
2. Manual property information, notices, amenity/outlet/service hours and states, event/meeting display, wayfinding, language variants, targeting, publishing, delivery confirmation, outdated awareness, correction, expiration, supersession, and restoration are **core capabilities**.
3. Permissions determine who may view restricted details, edit, approve, publish, restore, manage screens, change group standards, or perform high-scope actions.
4. Shift workflow, group coordination, advanced approvals, campaign management, interactive mapping, advanced localization, analytics, and managed monitoring are **tier-entitlement candidates**.
5. PMS, event, room-booking, transport, point-of-sale, guest-service, access, gaming, translation, AI, weather, emergency-management, map, positioning, and similar external synchronization are **independent add-on candidates** where integration is required.
6. Counts of properties, buildings, rooms or accommodations, venues, outlets, amenities, services, events, meeting spaces, screens, users, roles, languages, integrations, templates, history, storage, or AI use are **usage or quantity limits**.
7. Experiments, migrations, staged delivery, compatibility controls, and emergency-disable mechanisms are **internal rollout flags** and must not be presented as customer availability.
8. Industry, subtype, operating rhythm, product state, permission, entitlement, add-on, limit, source authority, privacy, and rollout remain separate concepts.

## Validation

Documentation-only review confirmed that:

- every operating characteristic named in issue #529 is addressed;
- Restaurant behavior remains inherited for embedded food-and-beverage outlets;
- continuous operation does not collapse independent object states;
- shift and handoff needs preserve core visibility and recovery without making workflow mandatory;
- arrival and departure guidance does not imply private guest state;
- notices, amenities, outlets, meetings, events, wayfinding, urgent messaging, languages, and property groups have explicit scope, source, time, privacy, and fallback boundaries;
- subtype differences tune defaults and presentation only;
- essential manual guest communication and recovery remain core;
- advanced workflow and external automation remain tier or add-on candidates;
- no product, UI, API, schema, migration, billing, entitlement, permission, privacy-system, localization, analytics, PMS, event, room-booking, transport, point-of-sale, guest-service, access, gaming, emergency, map, AI, hardware, or integration implementation was introduced.

GitHub Actions is authoritative for lightweight documentation validation on the exact reviewed pull-request head.

## Skipped under standing owner instruction

- Azure SQL and all external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and other integration-type tests.
- Runtime and implementation validation for product behavior that this RWP does not authorize.

## Exact next action

After this RWP is merged, verified on `master`, issue #529 is closed, and the claim is released, execute **RWP-00.55 — Hospitality Required Capabilities** (#530).

RWP-00.55 must define the smallest viable core set for guest information, wayfinding, amenity and outlet hours, events and meetings, notices, property context, languages, explicit targeting, publish confirmation, offline and outdated awareness, correction, recovery, permissions, and required states. It remains documentation-only and hands off to RWP-00.56.
