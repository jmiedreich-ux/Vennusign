# Hospitality Business Terminology

## Purpose

This is the canonical terminology companion to `track0/industries/hospitality.md`. It defines default operator-facing and guest-facing language for Hospitality properties while keeping customer-authored names, source authority, privacy, permissions, commercial access, limits, and rollout separate.

Restaurant terminology remains inherited for food-and-beverage outlets. Hospitality terminology governs property-wide arrival, stay, accommodation, amenities, services, events, meetings, wayfinding, notices, operating states, and departure.

## Canonical objects

| Term | Canonical meaning and boundary |
| --- | --- |
| **Organization** | The administrative or commercial account managing one or more properties. It is not automatically the owner, brand, franchise, or management company. |
| **Property** | The neutral term for a lodging-led operating location with one local Hospitality profile. Subtype terms may replace it locally when precise. |
| **Property group** | A managed collection of properties sharing selected administration, brand, libraries, reporting, or commercial authority. Sharing is never assumed. |
| **Guest** | A person receiving or seeking lodging or property services. Public signage must not expose guest-specific stay data. |
| **Visitor** | A person using a public property service without an assumed lodging stay, such as an attendee, diner, spa guest, or local customer. |
| **Stay** | The lodging relationship from arrival through departure. Dates, identity, occupancy, and status require an authorized source and appropriate audience. |
| **Room** | A guest accommodation unit in a hotel-like context. Use **accommodation** or **unit** for neutral or apartment-style contexts. |
| **Accommodation** | Neutral umbrella term for a room, suite, apartment-style unit, dormitory place, or other approved lodging unit. It is not a limit definition. |
| **Building / tower / wing / floor / area / zone** | Property hierarchy used for navigation, targeting, and local context. Preserve customer-authored names. |
| **Venue** | A locally managed place within a property with its own operating identity, schedule, content, or business type. |
| **Outlet** | A guest-facing food, beverage, retail, or service operation within the property. |
| **Amenity** | A guest facility or benefit such as a pool, fitness center, lounge, shared kitchen, laundry, or business center. Access and eligibility remain separate. |
| **Service** | A guest-support offering such as concierge, shuttle, housekeeping, breakfast, room service, luggage storage, parking, or maintenance assistance. |
| **Event** | A scheduled occurrence with a purpose, time, location, and audience. Use meeting, conference, wedding, reception, session, or performance when more precise. |
| **Meeting space** | A room or area intended for meetings, sessions, conferences, or group activity. Private group names require authorization. |
| **Function space** | Neutral umbrella term for ballrooms, meeting rooms, event rooms, reception areas, and flexible group spaces. |
| **Schedule** | A collection of times for events, services, activities, transport, or operating periods. Source and freshness remain visible to operators. |
| **Service hours** | Times during which a service, amenity, outlet, or area is expected to operate. Distinguish regular, effective, special, and access hours. |
| **Notice** | A time-bounded message about an operation, change, disruption, safety matter, event, or guest action. |
| **Wayfinding** | Guidance to a named destination using direction, route context, hierarchy, accessibility information, and temporary changes. It does not imply live navigation. |
| **Destination** | The official guest-facing place a person is trying to reach. |
| **Screen** | A paired Vennusign display endpoint. Identity, target, online state, content state, and delivery state remain distinct. |
| **Publish** | Send approved content to selected screen targets. It is not save, approve, schedule, or confirm delivery. |
| **Restore** | Return to a prior known content version or state after showing scope and effect. |

## Guest journey language

- **Arrival** means approaching or entering the property; **check-in** is the process for beginning a stay.
- **During your stay** is preferred for general guidance that uses no personal stay details.
- **Departure** means leaving the property; **check-out** is the process for ending a stay.
- Room ready, reservation confirmed, late check-out, and similar claims require an authorized source and privacy-appropriate surface.

Public signage must not reveal a guest name, room assignment linked to a person, reservation code, loyalty state, access credential, payment state, stay dates, service request, itinerary, or other guest-specific information by default.

## Operating and availability states

| Operator state | Guest-facing default | Meaning |
| --- | --- | --- |
| **Available** | Available | It can currently be used or provided under normal conditions. |
| **Limited** | Limited availability | It is operating with a known restriction; state the restriction only when authoritative. |
| **Open** | Open | The place or service is operating now. |
| **Closed** | Closed | It is not operating during the current expected period; permanence is not implied. |
| **Temporarily closed** | Temporarily closed | It is expected to resume, but the return time may be unknown. |
| **Unavailable** | Currently unavailable | It cannot currently be used or provided; the physical place may still be open. |
| **Out of service** | Temporarily out of service | Equipment, transport, or a specific service is not functioning. |
| **Paused** | Temporarily paused | An otherwise active service or activity has stopped temporarily. |
| **Delayed** | Delayed | An expected start, departure, arrival, opening, or service time has moved later. |
| **Canceled** | Canceled | A scheduled occurrence will not take place. |
| **Relocated** | Now at [destination] | The service, event, or activity has moved. Show the new destination and effective period when known. |
| **Maintenance** | Closed for maintenance / Service affected by maintenance | Maintenance affects operation; sensitive technical detail remains private. |
| **Weather affected** | Weather update | Weather changed operation, access, transport, or safety guidance. Show the confirmed impact. |
| **Restricted** | Access restricted | Access is limited by audience, time, credential, age, reservation, or condition. Eligibility is not assumed. |
| **Status unknown** | Please check with [contact] | No current authoritative state is available. Public wording must be honest and actionable. |

### Timing safeguards

- **Expected to reopen** is a forecast; **scheduled to reopen** is a plan from an authoritative source.
- Use **reopening time not confirmed** when no reliable time exists.
- **Next update by [time]** is an information checkpoint, not a reopening promise.
- Never invent remaining quantity, wait time, room readiness, service completion, reopening time, or an alternative location.

## Hours and schedules

- **Today’s hours** are the effective local hours for the selected date.
- **Regular hours** are a repeating default and may be overridden.
- **Special hours** are date-specific exceptions.
- **Access hours** describe when an eligible audience may enter; they do not establish eligibility.
- Last service, last entry, last seating, last shuttle, and doors close are separate concepts.
- Overnight periods must show the complete local date-and-time range.
- Operators must see whether hours are manual, scheduled, imported, or overridden, including source and freshness.

## Wayfinding wording

Guest-facing wayfinding should answer:

1. What is the destination’s official name?
2. Which direction, landmark, floor, building, tower, wing, or zone applies?
3. Is there a closure, relocation, restricted route, elevator outage, or alternate route?
4. What should the person do next?

Use **You are here** only when current location is authoritative. Use **Accessible route** only when verified. Avoid unsupported distance or effort claims such as “near” or “short walk.”

## Operator actions

Use explicit verb-object actions:

- Add notice
- Edit property information
- Update today’s hours
- Change amenity availability
- Relocate event
- Cancel event
- Assign meeting space
- Update wayfinding
- Preview screens
- Select screen targets
- Publish now
- Schedule publish
- Confirm delivery
- Restore previous version

Save draft, approve, publish, schedule, confirm delivery, and restore are distinct actions. High-impact actions must name scope, preview affected content and targets, support safe cancellation, and offer recovery where possible.

## Operator-facing versus guest-facing detail

Operators may see object and property scope, audience, source, freshness, effective time, override, conflict, draft/approved/scheduled/published/delivered/failed/offline/outdated/restored state, permission, and recovery guidance.

Guests should see the named place or service, current public state, effective time, location, confirmed change, and clear next action without internal source, approval, or troubleshooting detail.

## Subtype preferences

| Subtype | Preferred local language and emphasis |
| --- | --- |
| **Hotel** | property, lobby, guest room, suite, front desk, concierge, amenity, outlet, meeting room, check-in, check-out |
| **Resort** | resort, campus, activity, recreation, experience, transport, village, pool/beach/spa area, dining venue |
| **Motel** | property, office/front desk, building, room range, parking area, exterior entrance, breakfast area |
| **Hostel** | hostel, reception, dormitory, private room, shared kitchen, common area, quiet hours, activity; no personal bed or guest data publicly |
| **Extended-Stay** | property, suite, kitchenette, laundry, workspace, housekeeping schedule, package area, weekly service |
| **Serviced Apartment** | residence/property, apartment/unit, reception, building, floor, shared amenity, housekeeping window |
| **Conference Property** | conference, convention, group, meeting, session, ballroom, function space, event directory, registration |
| **Casino Resort** | resort, tower, casino, gaming area, entertainment venue, dining outlet, loyalty desk, event; restricted and security detail stays private |
| **Boutique Lodging** | property, customer-authored house/inn/lodge term, guest room, host/front desk, local guide, curated experience |
| **Neutral / mixed** | property, accommodation, area, venue, outlet, amenity, service, event, meeting space, notice, destination |

Subtype preferences tune defaults only. They do not overwrite approved local names, grant capabilities, or create packages.

## Mixed-property and source rules

- Use neutral language when a surface spans properties with incompatible subtypes.
- Embedded restaurants, bars, cafés, concessions, attractions, retail, spa, and other venues may use their approved local-industry terminology.
- Organization templates may recommend but may not silently overwrite local terms or imported authoritative labels.
- Analytics dimensions should remain neutral and stable even when presentation labels vary.
- Imported values retain source, effective time, freshness, and override behavior. Operators must see conflicts and the active public value.
- PMS, event, room-booking, transport, POS, guest-service, access, gaming, translation, AI, and other synchronization remain later packaging decisions. Manual public communication remains core.

## Classification

1. Industry, subtype, terminology preference, customer-authored labels, imported labels, neutral fallbacks, hierarchy, hours, schedule, notice type, destination, and operating state are **product/domain state** where represented.
2. Authorized manual configuration of public terminology, notices, hours, wayfinding, and state wording is a **core capability**.
3. Who may edit, approve, publish, restore, manage screens, view restricted information, or change organization terminology is a **permission**.
4. Advanced brand libraries, coordinated group terminology, approvals, localization workflow, and expanded analytics may later be **tier entitlements**.
5. PMS, event, room-booking, transport, POS, guest-service, access, gaming, translation, AI, and automatic synchronization may later be **add-ons** or tier capabilities.
6. Property, building, room, venue, outlet, area, event, screen, user, language, integration, storage, history, and AI quantities are **limits**.
7. Experiments, migrations, temporary compatibility controls, and emergency disable controls are **rollout flags**.
8. Terminology never grants access, changes privacy, transfers source authority, increases limits, or changes commercial availability.

## Impeccable clarification guidance

Future terminology and notice-management surfaces are **Operate** experiences. They must keep property, object, audience, source, effective time, and screen scope visible; use persistent labels and explicit actions; distinguish all hours and operating states; preview public wording and high-impact targets; preserve authored content, source relationships, privacy, and the last known good state; and cover first-use, empty, loading, permission, validation, stale-source, conflict, offline, publish-failure, partial-delivery, success, undo, and restoration states.

They must support keyboard and assistive technology, 200% zoom, non-color status cues, long names, localization expansion, right-to-left readiness, clear local dates/times, and phone through large-desktop layouts. Preserve the approved Sky Blue administrative direction.

## Boundaries

Documentation and planning only. No UI, API, schema, migration, billing, entitlement, permission, privacy, localization, translation, AI, analytics, PMS, event, room-booking, transport, POS, guest-service, access, gaming, or integration behavior is implemented.

RWP-13.06 and Phase 14+ remain paused. RWP-00.54 owns Hospitality operating characteristics.