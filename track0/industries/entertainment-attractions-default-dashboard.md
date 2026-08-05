# Entertainment & Attractions Default Dashboard

## Purpose

This document defines the default Entertainment & Attractions dashboard information architecture for RWP-00.72. It is a planning contract only and does not authorize UI or product implementation.

The dashboard must help an authorized operator understand what visitors are being told now, what has changed, whether every intended screen received it, and what safe action is most urgent. Exceptions, stale information, and recovery appear before optional analytics or promotion.

## Primary dashboard outcome

Within a few seconds, an authorized user should understand:

- which organization, venue, area, experience, event, session, queue, route, gate, screen group, or operating context is selected;
- what is open, available, scheduled, delayed, paused, closed, canceled, relocated, restricted, full, sold out, entry-paused, weather-affected, reopening, or unknown;
- the most important current and upcoming visitor information;
- whether queue, wait, capacity, admission, boarding, seating, route, or accessibility guidance is current and source-authoritative;
- whether intended screens are online, offline, outdated, unknown, or failed;
- whether the latest content was delivered to every intended target;
- which notices, schedules, sources, or screens need correction, retry, or restoration;
- whether a condition is product state, permission, tier access, add-on configuration, limit, source health, privacy, or internal rollout.

## Dashboard principles

1. Current visitor impact and operational exceptions appear before analytics.
2. Manual core actions remain visible without integrations.
3. Healthy aggregate status never hides one failed, outdated, or unknown target.
4. Schedules, queues, waits, capacities, admission, routes, and reopening are never inferred.
5. State, permission, tier, add-on, limit, source, privacy, and rollout are presented separately.
6. Every actionable status includes scope, source/freshness, impact, authority, and next action.
7. High-scope, destructive, or venue-wide changes require preview and confirmation.
8. Mobile layouts prioritize rapid, interruption-safe operating updates.
9. Multi-venue views preserve local operational truth and explicit scope.
10. Promotional or commercial content never displaces urgent visitor or delivery issues.

## Default hierarchy

### 1. Context header

The persistent header should show:

- organization and venue;
- primary subtype and neutral/mixed context where relevant;
- selected area, attraction, exhibit, event, session, queue, route, gate, screen group, or venue-wide scope;
- local date, time, and current operating period;
- current venue/experience operating state;
- user role and permission summary when it changes available actions;
- authorized venue/context switcher.

Changing context must be explicit. Target, audience, time, language, source, and selected objects from the prior context must not silently carry over.

### 2. Urgent exceptions and recovery

Show actionable exceptions ordered by public impact, urgency, scope, and recoverability:

- publish failed, partially delivered, or still pending beyond expectation;
- screen offline, outdated, unknown, incompatible, or displaying an older version;
- no paired screen or no intended target;
- venue, attraction, exhibit, show, screening, event, game, session, queue, gate, or route state contradicting current public content;
- canceled, delayed, relocated, closed, restricted, weather-affected, or reopened item not reflected on all targets;
- stale or conflicting schedule, queue, wait, capacity, admission, route, ticketing, venue, event, sports, map, or other source;
- expired or soon-to-expire notice with no replacement;
- sample or placeholder content still public;
- missing language fallback or accessibility-critical content;
- permission, plan, add-on, privacy, or limit condition blocking an attempted action;
- last-known-good content unavailable for restoration.

Each exception shows affected venue/area/experience and targets, public impact, source and timestamp, current fallback, and safest next action. Do not rely on color alone.

### 3. Quick operational actions

A task-first action area should provide authorized core actions:

- Update what is happening now;
- change venue, attraction, exhibit, show, event, game, session, queue, gate, route, or operating state;
- add or update a closure, delay, pause, cancellation, relocation, restriction, weather effect, reopening, or next-update notice;
- update a schedule, occurrence, doors, start, end, last-entry, boarding, seating, or check-in time;
- update manual queue, wait, capacity, sold-out, entry-paused, admission, boarding, or seating guidance;
- update wayfinding or an alternate route;
- preview and publish selected changes;
- retry failed targets;
- correct, supersede, unpublish, undo, or restore a prior successful version.

Show selected scope and exact targets before action. Whole-venue changes must not silently erase more specific local state.

### 4. Now, today, and next

Summarize the current visitor operating picture:

- venue hours and current operating state;
- experiences, attractions, exhibits, programs, shows, screenings, performances, games, tours, activities, and sessions happening now or next;
- current delays, pauses, closures, cancellations, relocations, restrictions, or weather effects;
- doors, start, end, last-entry, boarding, seating, and check-in times where authoritative;
- queue, wait, capacity, admission, and availability guidance;
- current public next action;
- language and accessibility coverage.

Show unknown, not configured, stale, and not applicable distinctly. Manual editing remains core.

### 5. Schedule and occurrence health

Show a compact operational schedule view rather than a full planning calendar:

- current and next occurrences;
- schedule source and freshness;
- recently changed, canceled, delayed, relocated, or expired occurrences;
- conflicts, missing locations, missing targets, missing public wording, and incomplete time-zone data;
- recurring schedule, blackout, event-phase, or approval issues when entitled;
- manual override and return-to-source state.

Primary actions are Edit schedule, Add occurrence, Update state, Preview, and Publish. External schedule sources remain optional add-ons.

### 6. Queue, wait, capacity, and admission

When relevant, show:

- queue open, limited, paused, closed, or unknown;
- current wait value, range, qualitative guidance, source, and update time;
- capacity available, limited, full, sold out, entry paused, or unknown;
- admission, ticket, pass, membership, reservation, timed entry, guest-list, credential, boarding, seating, or check-in guidance;
- stale threshold and manual fallback;
- affected attractions, events, gates, sessions, or screens.

Manual values are product state, not feature flags. External queue, occupancy, footfall, ticketing, admissions, and access systems are independent add-ons. Predictions must never appear as measured facts.

### 7. Wayfinding and visitor journey

Summarize current visitor guidance:

- active destinations and routes;
- entrance, exit, gate, section, auditorium, gallery, habitat, attraction, stage, screen, field, court, track, restroom, food, retail, parking, transport, first aid, accessibility service, quiet space, or custom destination;
- blocked route, temporary closure, relocation, alternate route, and verified accessible route;
- source and freshness for imported maps or positioning;
- affected screens and public wording.

Manual text and static guidance remain core. Advanced mapping and live routing are optional. Never infer current position, distance, travel time, or accessibility.

### 8. Notices and high-priority communication

Show active and upcoming notices with:

- public title and concise message;
- scope and audience;
- priority;
- effective time, expiration, and next-update time;
- source language and variants;
- source/owner and freshness;
- intended targets and delivery state;
- correction, expiration, supersession, unpublish, and restore actions.

Safety-related or emergency-style notices remain customer-authored and permission-controlled. The dashboard does not create jurisdiction-specific policy or make unverified claims.

### 9. Screen and publication health

Show per-target and aggregate state:

- intended screens, purpose, venue/area context, and target group;
- online, offline, outdated, unknown, incompatible, failed, pending, or delivered state;
- latest intended version;
- last successful delivered version and time;
- partial delivery and excluded targets;
- mismatched venue, area, language, orientation, source, or schedule;
- retry, correct, supersede, unpublish, undo, or restore actions.

A successful publication request is not equivalent to delivery. Aggregate success must not hide one failed target.

### 10. Source and freshness health

Show only sources relevant to current public outcomes:

- manual, imported, integrated, inherited, copied, or calculated origin;
- authoritative source and precedence;
- last successful refresh;
- stale, disconnected, conflicting, partial, overridden, or unknown state;
- affected public objects and targets;
- fallback and last-known-good behavior;
- reconnect, retry, accept source, keep override, or resolve conflict action.

Ticketing, admissions, access, queue, footfall, maps, cinema, venue, event, sports, translation, AI, identity, analytics, and other external sources remain separately purchased and configured add-ons.

### 11. Upcoming work

Show a short, operationally useful horizon:

- next venue opening, show, performance, screening, game, event, session, tour, talk, activity, or last-entry window;
- scheduled notice, route, content, campaign, or expiration;
- missing information before a scheduled publication;
- upcoming event-phase or screen check;
- unresolved source conflict, approval, translation, accessibility, or rights item;
- planned closure, relocation, weather effect, or return to operation.

Basic manually represented upcoming work may appear for all customers. Advanced recurring schedules, conflict detection, workflow, portfolio coordination, campaigns, and analytics respect tier access.

### 12. Multi-venue and estate overview

For authorized users with multiple venues, show an exception-first summary:

- venues, campuses, districts, parks, cinemas, museums, arenas, stadiums, touring groups, or other estates by current state;
- urgent notices, closures, delays, cancellations, relocations, and weather effects;
- screens offline, outdated, unknown, failed, or partially delivered;
- stale/disconnected sources and source conflicts;
- queue, capacity, admission, gate, route, event, or schedule exceptions;
- missing local review, language, approval, or recovery action;
- upcoming high-impact events and coordinated changes where entitled.

Do not show organization-wide data without scope permission. Bulk actions require explicit selection, compatible-target review, mixed-state visibility, preview, and confirmation. Local operational truth and urgent local notices cannot be silently overwritten.

## Role-aware presentation

### Front-line operator

Prioritize current context, Quick Update, notices, schedules, queue/wait/capacity/admission, wayfinding, screen health, retry, and restore. Hide billing and unrelated administration.

### Content editor

Prioritize drafts, validation, public wording, source/freshness, languages, accessibility, preview, schedule, notices, and publication handoff when publish authority is absent.

### Publisher or duty manager

Prioritize target/scope review, approvals where configured, publication results, high-impact exceptions, retry, correction, supersession, restore, and upcoming operational risk.

### Venue administrator

Add venue structure, screen assignment, roles, sources, templates, limits, integrations, and local inheritance controls.

### Portfolio or enterprise administrator

Add cross-venue exceptions, standards, inherited libraries, governance, access reviews, audit, retention, and service configuration only where entitled and permitted.

### Promoter, team, tenant, sponsor, rights holder, contractor, or limited collaborator

Show only assigned objects, content, events, screens, and actions. A business relationship never implies venue-wide or organization-wide access.

Commercial access and permission remain separate.

## Mobile-first priorities

On phone widths, the first viewport should contain:

1. current venue/area/experience context and operating state;
2. highest-impact exception or “all intended screens current” confirmation;
3. Quick Update;
4. publish/retry/restore action when needed;
5. compact now/next and screen-health summaries.

Use progressive disclosure for full schedules, multi-venue tables, sources, analytics, and administration. Actions remain reachable without horizontal scrolling. Context changes and high-scope actions require clear confirmation.

## Desktop priorities

Desktop may provide side-by-side exception, now/next, schedule, queue/capacity, notice, source, and screen-health panels while preserving the same hierarchy. Avoid a dense control-center layout that buries the dominant task.

Multi-venue tables support keyboard navigation, filtering, grouping, stable selection, explicit mixed states, local time zones, and per-row recovery actions.

## Required state coverage

Plan explicitly for:

- first use and no venue;
- no content or no public objects;
- no paired screen or no target;
- no current schedule, occurrence, queue, route, notice, or source;
- loading and refreshing;
- permission denied;
- tier unavailable;
- add-on not purchased;
- integration not configured, disconnected, stale, conflicting, overridden, partial, or unknown;
- privacy or rights restriction;
- limit reached;
- draft, scheduled, active, expired, canceled, superseded, unpublished, or restored content;
- screen online, offline, outdated, unknown, incompatible, failed, pending, or delivered;
- publication success, partial success, failure, and recovery;
- venue/experience available, limited, full, sold out, delayed, paused, closed, canceled, relocated, restricted, weather-affected, reopening, or unknown;
- save failure, conflict, retry, correction, undo, and restoration.

Every condition uses the correct language and recovery path.

## Tier and add-on presentation

Optional capability prompts appear only in context after core manual action remains available. Examples:

- coordinated screens and event moments after repeated multi-screen updates;
- recurring schedules and conflict detection after repeated manual schedule work;
- advanced localization after multiple language variants;
- portfolio coordination after additional venues are configured;
- ticketing, admissions, queue, map, venue, event, sports, translation, AI, analytics, identity, or managed-service add-ons beside manual fallback;
- advanced analytics below current operational and delivery state.

Prompts say what remains included, who may purchase/configure, whether a permission or limit is involved, dependencies, source/freshness, outage behavior, downgrade behavior, and “not now.” Internal rollout states are never customer offers.

## Impeccable planning result

The dominant mode is **Operate**. `shape` establishes one primary task per region and an exception-first hierarchy. `clarify` keeps public wording, operator metadata, source/freshness, target, and state distinctions understandable. `harden` requires first-use, empty, permission, offline, stale, partial, failure, conflict, long-name, localization, mobile, 200% zoom, keyboard, assistive-technology, and non-color-only states. `polish` preserves the approved Sky Blue administrative direction without decorative noise.

## Validation and handoff

The dashboard plan covers schedules, closures, queues/wait/capacity, admission, wayfinding, notices, screen/publish health, high-priority/recovery visibility, source freshness, role-aware presentation, multi-venue views, and mobile/desktop priorities. Essential manual operation remains core, and product state remains separate from permission, tier, add-on, limit, privacy, source health, and rollout.

No implementation is authorized. RWP-00.73 owns the Entertainment & Attractions KPI and analytics definition.