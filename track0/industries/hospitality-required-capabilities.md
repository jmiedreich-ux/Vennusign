# Hospitality Required Capabilities

## Purpose

This document defines the smallest viable Hospitality capability set that must remain available without a premium tier or paid integration. It inherits the Restaurant baseline and the approved Hospitality industry, subtype, terminology, and operating-characteristics records.

A required capability is necessary for safe, understandable, recoverable daily property communication. It is not a permission, product-state value, quantity allowance, subscription tier, add-on, or rollout flag.

## Required core capability set

### 1. Property and local-context information

Operators must be able to create, edit, preview, save, publish, expire, correct, supersede, archive, duplicate, and restore public property information for an explicit property, building, area, venue, outlet, amenity, service, event, meeting space, route, or destination.

Required behavior:

- visible property and object scope;
- customer-authored names and neutral terminology fallbacks;
- local dates, times, time zone, effective periods, and overnight ranges;
- public, staff, restricted, and privacy-sensitive audience separation;
- preservation of source, author, history, and last known good content;
- no guest-specific private data on public screens by default.

### 2. Guest notices and operating-state communication

Operators must be able to communicate changed hours, available, limited, open, closed, temporarily closed, unavailable, out-of-service, paused, delayed, canceled, relocated, maintenance-affected, weather-affected, restricted, unknown, reopening, and next-update states at the correct scope.

Required behavior:

- named affected object and property;
- effective start and end or explicit ongoing state;
- honest unknown timing;
- concise guest-facing wording;
- source and freshness for operators;
- correction, expiration, supersession, and restoration;
- preview of high-impact public wording before publish.

### 3. Amenity, service, and outlet hours and availability

Operators must be able to maintain ordinary hours, today’s effective hours, date-specific exceptions, access hours, last service, last entry, last seating, last shuttle, and current availability independently for each represented amenity, service, or outlet.

Required behavior:

- property state remains separate from local service state;
- embedded food-and-beverage outlets retain their approved local-industry terminology and capabilities;
- no inferred eligibility, room access, reservation status, capacity, wait, quantity, or reopening time;
- manual operation remains available when external systems are absent or disconnected.

### 4. Meetings, events, and directories

Operators must be able to present public or authorized event directories, meeting spaces, sessions, schedules, room changes, delays, cancellations, relocations, registration direction, breaks, and next actions.

Required behavior:

- event, session, meeting space, setup, service, and teardown periods remain distinct;
- planned and current locations remain distinct;
- private group, attendee, sponsor, security, and internal operational information is not exposed by default;
- room and schedule changes can be corrected quickly and restored safely;
- host, planner, property, outlet, sponsor, and operator authority remain explicit.

### 5. Manual wayfinding and temporary-route communication

Operators must be able to publish property, building, tower, wing, floor, zone, entrance, elevator, stair, parking, transport, amenity, outlet, event, meeting-space, and temporary-route guidance.

Required behavior:

- official destination name;
- concise route, landmark, hierarchy, or next-action context;
- current location only when authoritative;
- verified accessible-route wording only when known;
- closure, relocation, elevator outage, alternate entrance, and temporary path changes;
- no invented distance, travel time, accessibility, route, or live-position claim.

### 6. Basic multilingual and accessible content

Operators must be able to author and publish basic language variants manually without purchasing translation automation.

Required behavior:

- language clearly identified;
- per-language preview and target review;
- missing or outdated language variants visible to operators;
- complete translatable messages, text expansion, right-to-left readiness, and local date/time clarity;
- keyboard and assistive-technology support, 200% zoom, non-color status cues, restrained motion, and distance readability;
- translated or generated text remains reviewable product state.

Translation workflow, automated translation, translation memory, and AI assistance are not required core capabilities.

### 7. Explicit screen targeting and preview

Operators must explicitly select intended screens or approved target groups before publishing.

Required behavior:

- property, building, area, venue, event, audience, language, orientation, and screen purpose visible where relevant;
- mixed online, offline, outdated, restricted, or unknown targets visible before publish;
- excluded targets and high-scope changes visible;
- screen preview for representative portrait, landscape, and distance-reading contexts;
- no silent cross-property, cross-event, cross-language, or restricted-audience expansion.

### 8. Publishing and delivery confidence

Operators must be able to save draft, approve where required, publish now, schedule publication, confirm delivery, identify failures, retry safely, correct public content, expire or supersede notices, and restore a prior known version.

Required behavior:

- save, approve, schedule, publish, delivery confirmation, and restore remain distinct actions;
- exact content version and target scope visible;
- success, partial delivery, failure, offline, outdated, and unknown states separated;
- failed targets and next recovery action identified;
- retry does not duplicate, retarget, or overwrite newer approved content silently;
- last successfully delivered version and time remain visible.

### 9. Offline, outdated, conflict, and recovery awareness

Operators must see when a screen, source, language variant, notice, schedule, or published version is offline, outdated, stale, conflicting, disconnected, or unknown.

Required behavior:

- source, last update, effective time, and active override visible;
- manual and imported values do not silently alternate;
- reconnecting does not overwrite newer approved content;
- previously delivered public content remains stable during temporary connectivity loss where supported;
- safe correction, retry, rollback, and restoration paths;
- current public state and last known good state remain understandable.

### 10. Permissions and privacy-safe audiences

Permissions are required to control who may view restricted information; edit each property or object scope; manage templates or language variants; approve; publish; schedule; restore; manage screens; and perform high-impact bulk actions.

Permissions do not decide commercial access. Privacy and audience values remain product/domain and authorization concerns. Public operation must remain possible without revealing guest identity, room assignment tied to a person, reservation, loyalty, access, payment, stay, itinerary, or service-request data.

### 11. Required operational states

Every required capability must cover, where applicable:

- first use and no-content state;
- empty result and no-target state;
- loading and saving;
- permission restricted;
- validation failure;
- stale source and source conflict;
- offline and outdated target;
- publish failure and partial delivery;
- scheduled, active, expired, superseded, and restored content;
- success with clear scope;
- undo or restoration where safe;
- long names, overnight dates, language expansion, 200% zoom, keyboard, assistive technology, and non-color-only status.

## What remains outside required core

The following are not required to operate a viable Hospitality property with Vennusign:

- property-management, room-booking, event, transport, POS, guest-service, access, gaming, mapping, emergency, weather, translation, AI, or other automatic synchronization;
- guest-specific personalization;
- live room readiness, reservation, occupancy, queue, capacity, transport, position, or route data;
- advanced approvals, shift workflow, portfolio orchestration, brand governance, campaigns, interactive mapping, managed monitoring, and automated localization;
- premium analytics, optimization, prediction, or content generation.

These may become tier or add-on candidates only after later mapping and owner approval. They may not replace required manual operation.

## Classification decisions

1. The eleven capability groups above are **core capabilities**.
2. Property, hierarchy, audience, language, source, freshness, hours, schedules, notices, operating states, targets, delivery state, and content versions are **product/domain state** where represented.
3. Edit, view, approve, publish, schedule, restore, screen, template, language, and bulk-action authority are **permissions**.
4. Advanced workflow, governance, coordination, analytics, managed monitoring, localization workflow, personalization, and optimization remain **tier-entitlement candidates**.
5. External systems and automatic synchronization remain **independent add-on candidates** where integration is required.
6. Counts of properties, buildings, rooms, venues, outlets, amenities, services, events, screens, users, languages, integrations, storage, history, and AI use are **limits**.
7. Experiments, migrations, staged compatibility, and emergency disable controls are **internal rollout flags**.

## Capability-presentation guidance

Future Operate surfaces should present the smallest task-relevant subset:

- **Quick communication:** affected object, state, effective time, public wording, targets, preview, publish, result, recovery.
- **Hours and availability:** current exception first, ordinary pattern second, source/freshness, affected screens, restore.
- **Event change:** event/session, location, time, audience, route, public wording, targets, delivery.
- **Wayfinding change:** destination, route, temporary condition, verified accessibility, screens, preview.
- **Screen health:** exceptions first, current content, last delivery, online/outdated/unknown state, retry or restore.
- **Portfolio view:** property scope, mixed states, excluded targets, local overrides, no silent bulk publication.

Apply project-local Impeccable `shape`, `clarify`, and `harden` guidance. Preserve the approved Sky Blue administrative direction.

## Boundaries

Documentation and planning only. No product behavior is implemented. RWP-00.56 owns Hospitality Optional Capabilities.