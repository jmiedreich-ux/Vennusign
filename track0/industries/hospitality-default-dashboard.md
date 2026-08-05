# Hospitality Default Dashboard

## Authority and scope

This document defines the default Hospitality dashboard and starter-menu experience for RWP-00.60. It applies after onboarding and uses the approved Hospitality operating, capability, classification, tier, and onboarding records.

It is planning only. It does not implement UI, data models, analytics, integrations, permissions, alerts, or player behavior.

## Dashboard purpose

The dashboard is an **Operate** surface for authorized Hospitality teams. It answers, in order:

1. What needs attention now?
2. What are guests currently seeing?
3. What changed or is scheduled to change?
4. Which property objects, screens, languages, sources, or targets are affected?
5. What is the safest next action?
6. What can be deferred without misleading guests?

It is not a promotional home page, feature catalog, pricing page, or analytics-first executive dashboard.

## Default information hierarchy

### 1. Attention strip

Always reserve the first region for actionable exceptions, ordered by operational impact and freshness:

- failed or partial publication;
- offline or outdated screens carrying time-sensitive content;
- active high-priority guest notices;
- stale, disconnected, conflicting, or unknown sources;
- changed hours, closures, relocations, restrictions, or expected updates nearing effectiveness;
- missing language variants on active targets;
- scheduled content that has not reached intended targets;
- expired or superseded content still visible;
- unresolved handoff items;
- property or screen setup that makes current public content incomplete.

Each item shows property/object scope, local time, audience, language, source, targets, current public result, next action, and recovery path. Status is never color-only.

### 2. Current guest communication

Summarize what guests are currently meant to see:

- active property overview or welcome content;
- active notices and their effective periods;
- current hours and operating states for selected amenities, services, outlets, and desks;
- active meetings, events, directories, and room changes;
- current wayfinding and temporary-route content;
- active language variants and fallbacks;
- active urgent or high-priority communication;
- last confirmed public version by target.

The dashboard must not claim that a draft, scheduled item, accepted publish request, or unknown delivery state is currently visible.

### 3. Quick actions / starter menu

The primary action set is task-first:

- Update guest information;
- Change hours or availability;
- Post a guest notice;
- Update an event or meeting;
- Change wayfinding;
- Review and publish scheduled content;
- Check screen health;
- Correct or restore content;
- Add a screen;
- Finish property setup;
- Add or update a language;
- View property-group exceptions when available.

Actions use clear verb-object labels and show only when the user has permission. Missing permission is explained without implying missing entitlement.

### 4. Property operating snapshot

Show a compact, exception-first summary of:

- property local date, time, time zone, and operating day;
- selected subtype and public property name;
- reception/front desk and other key service context;
- count and list of active notices;
- amenities, services, and outlets with changed, limited, closed, unavailable, relocated, restricted, or unknown state;
- upcoming effective changes;
- today’s or next meetings and events;
- active route or access changes;
- language coverage;
- screen and delivery health;
- source freshness and manual overrides.

Do not collapse the property state into one “open/closed” value when represented objects have independent states.

### 5. Notices and operating changes

Provide a focused list grouped by:

- active now;
- scheduled next;
- awaiting publication or approval;
- failed or partial delivery;
- expired or needing review;
- recently corrected, superseded, or restored.

Each row includes scope, public wording summary, priority, source, effective time, expiration or next update, language coverage, targets, publication/delivery state, and responsible action where represented.

### 6. Amenities, services, and outlets

Show customer-relevant exceptions rather than an undifferentiated directory.

Default order:

1. changed or unknown state;
2. changed hours or imminent cutoff;
3. closure, limitation, relocation, or restriction;
4. missing public next action;
5. missing language coverage;
6. ordinary available/open entries.

Embedded venues retain local-industry terminology. Do not infer live inventory, reservations, room access, queue, wait, or capacity.

### 7. Meetings and events

Show:

- active and upcoming public/authorized meetings and events;
- registration and destination assignments;
- delays, cancellations, relocations, room changes, and route changes;
- source and freshness;
- publication and delivery result;
- restricted items only to authorized users.

Prioritize changes affecting current arrivals or in-progress events. Manual event operation remains available without an integration.

### 8. Wayfinding and route state

Show active temporary changes, unavailable routes, alternate directions, destination changes, and verified accessible-route information.

Each item includes destination, property/building/floor/area context, effective period, affected screens, language, source, and current delivery result.

Do not invent current position, distance, travel time, accessibility, or safe route.

### 9. Screen and publish health

A screen-health region must distinguish:

- online and current;
- online but outdated;
- offline with last known public content;
- pending delivery;
- failed delivery;
- partial group delivery;
- unknown state;
- unpaired or incomplete setup;
- current content different from intended content.

Show last contact, last confirmed delivery, intended version, current known version, active schedule, and actions to retry, correct, target elsewhere, or restore.

Save, schedule, publish, and delivery confirmation are separate.

### 10. Source, override, and freshness health

For any represented external or shared source, show:

- source identity;
- authoritative scope;
- last successful refresh;
- current freshness state;
- conflict or disconnect;
- active manual override and its effective period;
- last known good value;
- affected property objects and targets;
- safe manual fallback.

Stale imported data must not appear current by default.

### 11. Recovery and restoration

Make recovery visible without requiring a technical support path.

Show:

- recent failed or partial publishes;
- recent corrections, supersessions, expirations, and restorations;
- last known good content or state;
- targets affected;
- safe retry and restore actions;
- conflicts with newer approved content;
- audit/history link when permitted.

Restoration must preserve source, authority, language, target scope, and newer approved work.

## Role-aware presentation

### Property operator

Prioritize quick updates, notices, hours, events, wayfinding, screen health, and current delivery problems for assigned properties.

### Shift lead or manager

Add handoff items, pending approvals, scheduled changes, unresolved exceptions, high-scope actions, source conflicts, and restore decisions.

### Content or brand operator

Prioritize drafts, templates, languages, campaigns, approvals, brand/library state, property exceptions, and target review.

### Technical or screen operator

Prioritize pairing, online/offline/outdated state, delivery failures, player version/health where represented, target assignment, and recovery.

### Property-group or portfolio operator

Prioritize cross-property exceptions, mixed states, local-time context, excluded targets, unresolved local overrides, source health, and safe bulk action.

### Read-only or restricted user

Show permitted information and explain unavailable actions as permission restrictions without presenting upgrade prompts.

Roles tune presentation only; they do not create commercial access or object authority.

## Shift-aware presentation

### Start of shift

Show what changed since the user’s previous shift or selected handoff point:

- active and scheduled notices;
- changed hours or states;
- failed or partial delivery;
- offline/outdated screens;
- source conflicts and overrides;
- missing languages;
- unresolved actions and next updates.

### Arrival / check-in peak

Prioritize reception, entrances, parking, transport, luggage, wayfinding, outlet/amenity hours, notices, and screen health.

### During stay / ordinary operation

Prioritize amenities, services, outlets, events, activities, transport, routes, languages, and current guest notices.

### Event peak

Prioritize directories, meeting spaces, registration, session changes, room moves, routes, language coverage, and delivery confirmation.

### Departure / check-out peak

Prioritize check-out guidance, breakfast, transport, luggage, parking, exits, changed routes, and current service limitations.

### Overnight

Prioritize local date boundaries, overnight entrances, reception/contact guidance, next-day schedules, access information, active notices, and screen/source health.

Shift-awareness is presentation state, not private guest-state automation.

## Property-group and portfolio views

When included, a group view begins with exceptions, not totals.

Show:

- property local time and current operating period;
- affected property, object, language, source, and target;
- mixed states and excluded properties;
- inherited versus local content/configuration;
- mandatory, recommended, copied, linked, and overridden state;
- pending local review or opt-out;
- safe bulk action preview;
- property-specific recovery path.

A group action must not silently overwrite local public truth, urgent notices, privacy boundaries, or last-known-good content.

## Mobile priorities

On phone-sized screens:

1. attention items;
2. quick actions;
3. active notices and changed states;
4. screen/publish health;
5. upcoming events or effective changes;
6. property snapshot;
7. deeper lists and analytics.

Use one-column task cards, persistent property selector, local time, visible target scope, and bottom-safe primary actions. Avoid wide tables, hover-only detail, drag-only ordering, and dense KPI grids.

## Desktop priorities

On desktop:

- retain the same attention-first order;
- allow a stable property/context rail;
- use a main work area with exception list and task panels;
- show side-by-side preview, targets, source/freshness, and delivery result where useful;
- allow dense comparison only after primary task clarity;
- keep high-impact actions explicit and separated.

Large screens do not justify showing every feature at once.

## Empty, loading, failure, and success states

Required dashboard states include:

- first use after onboarding;
- no active notices;
- no configured amenities/events/routes;
- no screens or no assigned screen;
- loading and refresh;
- permission-limited;
- source stale/disconnected/conflicting;
- screen offline/outdated/unknown;
- publish pending/failed/partial;
- missing language;
- scheduled but not active;
- content changed since preview;
- success with confirmed targets;
- correction, supersession, expiry, undo, and restoration.

Every empty or failure state includes a relevant next action or explains why no action is possible.

## Upgrade and add-on presentation

Optional capabilities appear only in context:

- advanced coordination when repeated manual handoff or approval needs are visible;
- portfolio governance when multiple properties exist;
- enterprise administration when identity/audit needs are configured;
- an external add-on when manual data maintenance is burdensome and a relevant source exists.

Keep core quick actions visible. Explain customer outcome, requirement, manual fallback, commercial type, and “not now.” Do not fill the dashboard with disabled modules or pricing cards.

## Impeccable result

The dashboard applies `shape` around immediate operations, `clarify` through exception-first hierarchy and explicit state, `harden` through failure/recovery states, `adapt` for mobile/desktop/role/shift, and bounded `polish` after task flow is correct.

Support keyboard and assistive technology, visible focus, non-color-only state, 200% zoom, long names, localization expansion, right-to-left layouts, reduced motion, local dates/times, and the approved Sky Blue administrative direction.

## Boundaries and handoff

No implementation is authorized. RWP-00.61 owns Hospitality KPI and analytics planning.