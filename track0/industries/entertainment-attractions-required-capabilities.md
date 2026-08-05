# Entertainment & Attractions Required Capabilities

## Authority

This documentation-only companion completes RWP-00.67. It defines the smallest viable capability set required for safe daily Entertainment & Attractions operation without depending on premium tiers, paid integrations, automated data, or future product implementation.

## Required-core principle

A venue must be able to communicate accurate visitor information manually, target the correct screens, verify delivery, correct mistakes, and recover from stale or failed publication before any optional workflow or integration is introduced. Industry or subtype selection changes defaults and presentation only; it does not grant or remove these capabilities.

## Required capability groups

### 1. Venue, area, and visitor-context information

Core operation must support manually authored:

- venue identity and local time zone;
- areas, zones, buildings, floors, galleries, auditoriums, stages, screens, gates, sections, lanes, courts, fields, tracks, departure points, entrances, exits, and assistance points where relevant;
- visitor hours, admission hours, area or attraction hours, last entry, service windows, and local operating-day boundaries;
- public contact, accessibility, arrival, parking, transport, and assistance guidance when authoritative.

Hierarchy values are product/domain state. Their edit authority is permission. Quantity allowances are limits.

### 2. Programs, schedules, shows, screenings, events, sessions, and experiences

Authorized operators require manual creation, editing, duplication, scheduling, cancellation, restoration, and publication for:

- performances, screenings, shows, events, talks, tours, games, matches, sessions, rounds, departures, demonstrations, feedings, classes, and timed-entry windows;
- continuous attractions, exhibits, habitats, installations, collections, activities, and self-guided experiences without fabricated showtimes;
- start time, optional end or duration, location, admission or access context, availability, language, accessibility, arrival guidance, and state only when known.

Today, now, next, final occurrence, last entry, venue close, and experience close remain distinct.

### 3. Closures, delays, pauses, relocation, cancellation, and reopening

Core operation must let authorized staff communicate a bounded state for the exact affected venue, area, experience, event, session, queue, route, gate, or screen.

Required states include available/open, limited, full, sold out, entry paused, delayed, paused, temporarily unavailable, closed, canceled, relocated, weather affected, maintenance affected, access restricted, capacity restricted, reopening/resuming, and unknown where appropriate.

The system must not invent causes, alternatives, revised times, refunds, rebooking, access, capacity, safety, or reopening promises.

### 4. Queue, wait-time, capacity, and admission communication

Manual queue and access communication remains core:

- queue open, forming, limited, full, paused, closed, redirected, virtual where represented, or unavailable;
- manually entered wait estimate, range, qualitative status, stale/unknown state, and source/freshness context;
- manual capacity state such as available, limited, full, sold out, or entry paused;
- admission and access guidance that distinguishes general, timed, member, ticketed, participant, reserved-seat, standing, restricted, or unknown context when authoritative.

Dynamic measurement, occupancy, ticket inventory, attendance, turnstile, reservation, and access-control synchronization are not required core.

### 5. Manual wayfinding and accessible route guidance

Core operation must support destination-based instructions for entrances, gates, auditoriums, stages, screens, galleries, habitats, attractions, areas, queues, assistance points, parking, transport, and exits.

Temporary route, closure, relocation, accessible-route, elevator/lift, ramp, seating, captioning, audio-description, sensory, mobility, and assistance guidance may be displayed only when venue-authored or source-authoritative. Advanced mapping, positioning, and turn-by-turn navigation are optional.

### 6. Notices, safety-related communication, and priority

Authorized operators need manual notices with explicit scope, audience, effective time, priority, expiry, target screens, public wording, and restoration.

High-priority or emergency-related messaging must be concise, authoritative, distance-readable, non-ambiguous, and not dependent on color, motion, or audio alone. Track 0 does not define emergency policy, legal compliance, evacuation procedures, accessibility law, medical guidance, security operations, or life-safety behavior.

### 7. Basic multilingual and accessible content

Core operation must support:

- a declared source language;
- manually authored alternate-language content;
- visible coverage gaps and fallback behavior;
- per-language preview and target confirmation;
- preservation of the current authoritative version when a translation is stale or incomplete;
- long text, localization expansion, right-to-left readiness, keyboard access, assistive technology, 200% zoom, and non-color-only states.

Automated translation, terminology libraries, review workflow, AI wording, and premium localization remain optional candidates.

### 8. Screen targeting, preview, scheduling, and publication

Core operation must let authorized staff:

- select the exact venue, area, attraction, exhibit, event, session, queue, gate, screen, group, or purpose target;
- preview visitor-facing content in context;
- see effective dates and local times;
- confirm high-scope impact before publication;
- publish immediately or schedule when the existing platform supports scheduling;
- avoid silently inheriting content into incompatible mixed-industry or mixed-subtype locations.

Target scope and content state are product/domain state; publish and scheduling authority are permissions.

### 9. Delivery confidence and screen health

After publication, core operation must expose:

- intended targets;
- accepted/pending/failed/partial delivery state;
- online, offline, outdated, or unknown screen state;
- last successful publication or last-known-good content where represented;
- retry, correction, supersession, expiration, unpublish, and restore paths;
- clear separation between content publication success and external-source freshness.

Advanced monitoring, remote support, managed connectivity, and hardware service may be optional, but basic delivery confidence cannot be premium-only.

### 10. Source, freshness, conflict, override, and recovery

Every imported schedule, wait, capacity, admission, score, attraction, exhibit, event, or operational value requires source and freshness context.

Core operator awareness must show stale, disconnected, conflicting, partially synchronized, overridden, or unknown state. Manual override authority must be explicit, reversible, and auditable where history exists. Imported data must not silently overwrite a safer current manual message or appear current after its source becomes stale.

### 11. Permissions, privacy-safe audiences, and authority boundaries

Core operation requires clear authority for view, edit, schedule, approve where configured, publish, restore, target, and restricted-content actions.

Public signage must not expose visitor, ticket holder, seat holder, member, participant, performer, sponsor, security, staff, or operationally sensitive data by default. Owner, operator, promoter, presenter, tenant, sponsor, team, performer, distributor, rights-holder, and host relationships do not silently transfer content authority or commercial access.

## Subtype-required emphasis

| Subtype | Required emphasis |
| --- | --- |
| Cinema | Film/showtime, auditorium, format/accessibility, seating/boarding, sold-out, delay, cancellation, next screening |
| Performing Arts Theater | Production/performance, curtain/start, auditorium/stage, interval, late seating, access guidance |
| Museum | Exhibit/gallery availability, programs, timed entry, tours/talks, route, temporary closure, interpretation |
| Gallery / Exhibition Venue | Exhibition/installation, opening or talk, hall/gallery, entry window, capacity, temporary closure |
| Zoo / Aquarium | Habitat/exhibit availability, feeding/talk schedules, care closure, route, weather, last entry |
| Theme / Amusement Park | Attraction state, queue/wait, access guidance, showtimes, weather, closure/reopening, last ride |
| Family Entertainment Center | Activity/session availability, check-in, party timing, area/lane assignment, capacity, queue |
| Arcade | Game-zone availability, card/token guidance, tournament, maintenance, prize-area information |
| Bowling Center | Lane/session/league/tournament information, check-in, delay, service guidance |
| Sports Venue | Event, gate/section, start, transport, concessions, safety, event state, egress |
| Live-Event Venue | Event/artist, doors/start, room/stage, gate, delay/cancel/relocate, merchandise, egress |
| Attraction / Tour | Experience/tour, departure/entry, route, language, weather, capacity, last entry, closure/reopening |

These are presentation priorities, not entitlements.

## Required-state coverage

Future Operate surfaces must cover first use, empty, loading, validation, permission denied, scheduled, active, delayed, paused, canceled, relocated, closed, stale source, conflicting source, offline, outdated screen, publish failure, partial delivery, success, correction, supersession, expiry, undo, and restoration.

## Impeccable planning result

The dominant mode is **Operate** for time-pressured staff, with public **Read** outcomes. Each task should show the smallest relevant scope, current state, effective time, source, public wording, language coverage, target screens, impact preview, publication result, and recovery path. The future experience must remain usable on phone and desktop, in crowded, outdoor, low-light, high-motion, and intermittent-connectivity environments, while preserving the approved Sky Blue administrative direction.

## Primary classification decisions

1. The eleven required groups are core capabilities.
2. Venue hierarchy, schedules, experiences, states, queues, wait values, capacity states, admissions context, notices, routes, languages, sources, freshness, targets, delivery state, and versions are product/domain state where represented.
3. View, edit, approve, schedule, publish, restore, target, language, and restricted-information authority are permissions.
4. External systems and automatic synchronization are independent add-on candidates or approved tier bundles; manual core remains available.
5. Advanced coordination, governance, analytics, AI, premium localization, enterprise identity, managed hardware, and support remain later optional candidates.
6. Quantities are limits; temporary release controls are rollout flags.

## Deferred to RWP-00.68 and later

Optional integrations and workflows, final classification consolidation, tier bundles, limits, onboarding, dashboard, KPIs, privacy/retention, source precedence, downgrade behavior, and owner decisions remain unresolved until their approved RWPs.
