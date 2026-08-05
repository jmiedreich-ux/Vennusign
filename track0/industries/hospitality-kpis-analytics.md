# Hospitality KPIs & Analytics

## Purpose

This document defines the Hospitality KPI and analytics planning model for RWP-00.61. It separates required operational visibility from optional advanced analysis and identifies source, freshness, privacy, retention, export, permission, add-on, tier, and limit boundaries.

It is documentation only. No telemetry, analytics pipeline, report, dashboard, API, schema, integration, retention policy, alerting, or product behavior is authorized.

## Measurement principles

1. Current guest-communication truth and recovery come before performance optimization.
2. Core operational visibility must work without a PMS, CRS, event, occupancy, booking, transport, POS, access, mapping, sensor, or guest-service integration.
3. Every metric must state scope, property/object grain, time basis, source, freshness, coverage, completeness, and known exclusions.
4. Unknown, unavailable, not configured, stale, disconnected, restricted, and zero are distinct.
5. Manual states may be measured as operator-recorded activity but cannot be presented as verified occupancy, guest demand, revenue, attendance, wait time, route use, or satisfaction.
6. Advanced analysis may be tier-entitled; external data connections remain independent add-on candidates.
7. Retention, rows, refresh frequency, exports, storage, properties, objects, languages, sources, and consumption remain limits.
8. Public or shared reports must not expose private guest, employee, reservation, room, access, payment, identity, or commercially sensitive data without approved authority and aggregation.

## Required core operational visibility

The following visibility is required for safe ordinary operation and must not be premium-only.

### Screen and publication health

- intended screen and target count;
- screens online, offline, outdated, failed, pending, unknown, or current;
- latest publication accepted, pending, partial, failed, superseded, restored, or delivered;
- time since last confirmed delivery by target;
- current-version coverage across intended targets;
- last-known-good version and recovery result where represented;
- active schedule and effective content version;
- current target mismatch or missing assignment.

Current-version coverage is confirmed current intended targets divided by all intended targets. Unknown targets remain visible and in scope; they are never silently counted healthy.

### Guest-notice freshness and effectiveness

- active, scheduled, expired, superseded, corrected, restored, draft, or unpublished notices;
- time since creation, last edit, approval, publication request, and confirmed delivery;
- effective and expiry times by property local time;
- intended versus delivered target and language coverage;
- high-priority notices without confirmed delivery;
- stale, conflicting, or source-overridden notice state;
- notices needing review because the next-update time passed.

These are communication-health facts, not measures of guest awareness or response.

### Property-object operating health

For amenities, services, outlets, desks, meeting spaces, event spaces, transport points, destinations, routes, and other represented objects:

- current open, available, limited, closed, temporarily closed, unavailable, out of service, paused, delayed, canceled, relocated, restricted, maintenance, weather-affected, or unknown state;
- current and next known hours or effective changes;
- stale or conflicting source state;
- active manual override and effective period;
- missing public wording, destination, route, language, target, or screen coverage;
- number of objects requiring action now.

A represented object state is product/domain state, not a performance score.

### Meeting and event communication health

- active and upcoming represented meetings/events;
- room or destination changes, delays, cancellations, relocations, restrictions, and route changes;
- source and freshness;
- intended and delivered language/target coverage;
- public versus restricted scope;
- incomplete or conflicting event details;
- time from change to confirmed publication.

Manual event operation remains core. External event data remains an add-on candidate.

### Wayfinding and route health

- active temporary-route and destination changes;
- unavailable or restricted routes;
- alternate route and accessible-route information when explicitly verified;
- missing destination or target coverage;
- language coverage;
- source and freshness;
- affected screens and confirmed delivery;
- correction and restoration results.

The system must not infer guest position, distance, travel time, accessibility, route safety, or route use without an authoritative source.

### Source and integration operational health

For every configured source:

- connected, disconnected, degraded, stale, conflicting, partially synchronized, unauthorized, unsupported, or unknown state;
- last successful and last attempted refresh;
- source-reported effective time;
- authoritative object scope;
- affected properties, objects, languages, and targets;
- active manual override and fallback;
- unresolved mapping or identity conflicts.

This health visibility is core once an add-on is connected. The connection itself remains an add-on.

## Operational service metrics

Candidate operational measures include:

- time from edit to save;
- time from save to approval where approval exists;
- time from approval or schedule activation to publication request;
- time from publication request to confirmed delivery by target;
- partial or failed publication rate;
- screen outdated duration while expected to serve guests;
- mean and distribution of recovery time after offline, failed, partial, stale, conflict, or outdated conditions;
- correction, supersession, expiry, retry, undo, and restoration frequency;
- active notice age and overdue-review duration;
- language-coverage exception duration;
- source-staleness and manual-override duration.

Every duration must state the event timestamps, local time zone, operating-day rule, exclusions, and whether the time is operator-entered, system-recorded, player-confirmed, or source-supplied.

## Amenity, outlet, and service analytics

Optional advanced analysis may include:

- operating-state duration by object and property;
- planned versus represented hours;
- frequency and duration of closures, limitations, relocations, restrictions, or maintenance states;
- number and timing of communication changes;
- target and language coverage;
- time from operational change to confirmed guest-facing delivery;
- repeated exceptions by object type or property;
- imported utilization, reservation, transaction, queue, or demand analysis when authoritative external data exists.

Manual open/closed or unavailable state does not prove utilization, demand, revenue, inventory, or guest impact.

## Meeting and event analytics

Optional candidates include:

- event and meeting counts by property, space, type, day, source, and public/restricted scope;
- scheduled versus canceled, delayed, relocated, or changed events;
- time from source change to confirmed publication;
- room/destination/route change frequency;
- language and target coverage;
- screen delivery exceptions during event periods;
- external registration, attendance, ticket, room-booking, or event-revenue measures only where approved source data exists.

A published event listing is not proof of attendance, registration, satisfaction, or revenue.

## Wayfinding analytics

Optional candidates include:

- represented destination and route coverage;
- frequency and duration of temporary-route changes;
- missing or stale destination content;
- target and language coverage;
- correction and restore frequency;
- imported map interaction, positioning, route-choice, or sensor data only when an approved source and privacy model exist.

Content delivery is not proof that a guest followed a route or reached a destination.

## Property-group and portfolio analytics

Optional coordinated outcomes include:

- exception and recovery trends by property, region, brand, or group;
- current-version, language, notice, source, and screen coverage distributions;
- inherited, copied, linked, mandatory, recommended, overridden, excluded, and mixed states;
- local adoption and exception timing;
- cross-property source freshness and integration health;
- publication latency and recovery comparisons;
- repeated object, event, route, or language gaps;
- governance, approval, audit, and scheduled-report trends where included.

Comparisons must show local time, property type, data coverage, source differences, property size, seasonality, and non-comparable conditions. Rankings without comparable scope and coverage are prohibited.

## External data dependencies

### PMS, CRS, and lodging systems

Potentially support occupancy, arrivals, departures, room availability, package, housekeeping, service, reservation-derived public information, and property context. Required safeguards include minimum necessary fields, room/guest privacy, effective time, source authority, correction, late data, cancellation, and manual fallback.

### Meeting, event, room-booking, and registration systems

Potentially support event schedules, spaces, registrations, attendance, changes, and capacity context. Public communication and restricted details must remain separate.

### Outlet, POS, ordering, and payment systems

Potentially support transaction, revenue, item, outlet, and service analysis. Payment and identified order data require strict minimization, authorization, currency/tax treatment, refund/void rules, and reconciliation.

### Occupancy, footfall, queue, sensor, access, and positioning systems

Potentially support passage, occupancy, wait, access, route, and utilization analysis. Coverage, placement, confidence, downtime, false positives, aggregation, retention, and privacy limitations must be explicit.

### Transport, parking, valet, access, guest-service, spa, gaming, restaurant, and local systems

Potentially support service timing and operational context. Each remains an independent source with its own authority, privacy, freshness, failure, retention, and fallback rules.

### Survey, loyalty, messaging, and experience systems

Potentially support engagement or satisfaction analysis only with approved purpose, consent/authority, aggregation, identity handling, and attribution. Communication delivery must not be called engagement without evidence.

## Metric specification contract

Every approved metric must document:

- plain-language name and purpose;
- primary audience and action supported;
- numerator and denominator where applicable;
- included and excluded records;
- grain and supported dimensions;
- event, effective, recorded, publication, and delivery time basis;
- local time-zone and operating-day rules;
- source and source authority;
- freshness, latency, coverage, completeness, and confidence;
- handling of missing, late, corrected, duplicate, stale, partial, conflicting, canceled, voided, or disconnected data;
- privacy classification and minimum aggregation;
- retention, deletion, export, and recalculation rules;
- permission scope;
- core, tier, add-on, limit, state, privacy, source, and rollout classification;
- reconciliation and validation method.

## Privacy and permission boundaries

Analytics should default to property, object, screen, content, language, source, event, time period, and aggregate operational outcomes—not identified people.

Do not expose or retain by default:

- guest identity, room, stay, reservation, contact, payment, access, precise location, loyalty, preference, or service detail;
- employee-level performance when aggregate operational analysis is sufficient;
- private group, event, contract, organizer, attendee, revenue, settlement, or restricted-space details outside authorized scope;
- raw device, sensor, network, access, or positioning history longer than necessary;
- sensitive safety, security, access, incident, or property information in public or broad exports.

View, compare, export, schedule, share, administer, and delete analytics are separate permissions. Property-group visibility does not grant restricted property or guest-data access.

## Retention, correction, and export

- Core current-state visibility uses the latest authoritative state plus an included bounded history.
- Longer publication, operational, source, audit, event, property-group, and external-data histories may be tier candidates with explicit limits.
- Corrections preserve source, effective time, recorded time, previous report result, and recalculation policy.
- Disconnected or canceled add-ons define historical availability, read-only state, export, retention, deletion, and re-connection behavior.
- Exports include filters, property/object scope, time zone, units, source, freshness, coverage, formula version, generated time, permission context, and known limitations.
- Scheduled reports are advanced workflow candidates; export count, rows, size, frequency, and retention remain limits.

## Classification matrix

| Outcome | Primary classification |
| --- | --- |
| Current screen, publication, notice, object, route, event, language, source, and recovery status | Core operational visibility |
| Current connected-source health and freshness | Core visibility after add-on connection |
| Recent changes and bounded operational history | Core within included retention limit |
| Property/object/event/route trends and comparisons | Tier-entitlement candidate |
| Property-group and portfolio benchmarking/governance | Tier-entitlement candidate |
| Forecasting, recommendations, anomaly detection, optimization, and natural-language summaries | Advanced tier/AI candidate |
| PMS, event, occupancy, POS, sensor, access, transport, survey, loyalty, or other external data | Independent add-on candidate |
| Properties, rows, refreshes, retention, exports, reports, storage, and consumption | Limits |
| Who may view, compare, export, schedule, share, administer, or delete | Permission |
| Current represented values, freshness, coverage, and formula version | Product/domain state |
| Temporary release or experiment control | Rollout flag |

## Impeccable planning result

Analytics is secondary to urgent operational work. Future surfaces should lead with plain-language questions, actionable exceptions, scope, source, freshness, coverage, and recovery. Avoid decorative KPI walls, false precision, unlabeled rankings, misleading green states, and hidden exclusions.

Apply `clarify`, `harden`, `adapt`, and bounded `persuade`: accessible table/chart alternatives, keyboard and assistive-technology support, non-color-only status, 200% zoom, long names, localization expansion, right-to-left readiness, responsive layouts, export accessibility, local date/time, reduced motion, and the approved Sky Blue administrative direction.

## Validation

The model covers core screen/publish/notice freshness, amenity/outlet/service, meeting/event, wayfinding, and property-group analytics; external PMS/event/occupancy and related dependencies; privacy, permission, retention, correction, deletion, export, and scheduled-report concerns; and clear separation of core, tier, add-on, limit, state, source, privacy, permission, and rollout.

No implementation is authorized. RWP-00.62 owns final Hospitality validation, review, shared-record synchronization, and handoff.
