# Food Truck & Concession Default Dashboard

## Purpose

This document defines the default dashboard information architecture for Food Truck & Concession operators. It is a planning contract only and does not authorize UI or product implementation.

The dashboard must prioritize the current operating task, especially on phones and during short service windows. It should surface exceptions and recovery actions before optional analytics or configuration.

## Primary dashboard outcome

Within a few seconds, an authorized operator should understand:

- which operation, unit, stand, event, host, and location they are viewing;
- whether service is planned, in setup, ready, open, limited, paused, relocating, closed, canceled, or unknown;
- the current menu and high-impact availability/sell-out exceptions;
- whether intended screens are online, offline, outdated, unknown, or failed;
- whether the latest content was delivered to every intended target;
- the most urgent safe action for the current service period;
- whether information is manual, integrated, stale, conflicting, or not configured.

## Dashboard principles

1. Current operation and service context appear before analytics.
2. Exceptions appear before healthy counts.
3. Manual core actions remain visible even when integrations exist.
4. State, permission, entitlement, add-on, limit, connection, and source freshness are never collapsed into one “unavailable” condition.
5. Every status has an action or explanation where the user has authority.
6. Bulk actions require explicit scope, preview, and confirmation.
7. Mobile layouts prioritize one-handed rapid updates and recovery.
8. The dashboard does not infer location, wait time, stock, reopening, delivery, or source freshness.

## Default hierarchy

### 1. Context header

The persistent header should show:

- organization;
- operation/unit/stand/service point;
- primary subtype;
- current location, event, host, market, pitch, gate, section, zone, or “not set”;
- local date/time and current service period;
- current operating state;
- role/permission summary when it materially changes available actions;
- operation switcher for authorized multi-unit users.

Changing context must be explicit and must not preserve a dangerous target selection from another operation.

### 2. Urgent exceptions and recovery

Show only actionable exceptions, ordered by guest impact and scope:

- publish failed or partially delivered;
- screen offline or outdated while expected to be serving;
- no screen paired or no target selected;
- operation open with no current location/event when one is expected;
- stale or conflicting external menu, location, event, queue, or availability source;
- scheduled content about to expire or already expired;
- sold-out or unavailable state affecting high-impact menu items or combos;
- operation paused, relocated, delayed, canceled, or closing early;
- plan/limit, permission, or add-on configuration issue blocking the attempted action.

Each exception should show affected scope, last known time/source, guest impact, and the safest next action. Do not use color alone.

### 3. Rapid service controls

A mobile-first action area should provide authorized core controls:

- Quick Update;
- mark item/combo/category unavailable, sold out, limited, or available;
- change operation/service-point state;
- update current location or event;
- update service period, last orders, pickup, queue, lane, or window guidance;
- publish selected changes;
- retry failed targets;
- restore a prior successful version.

The dashboard should show the selected scope before action. High-scope changes require confirmation. Whole-operation closure must not silently destroy item-level state.

### 4. Menu and availability summary

Show a compact operational summary rather than the full editor:

- active menu/content set;
- last edited and last published times;
- count of available, sold-out, unavailable, limited, or draft items;
- recent high-impact changes;
- items expected to return when an authoritative time exists;
- validation or incomplete-content warnings;
- source and freshness when menu/availability data is integrated.

Primary actions are “Quick Update,” “Edit menu,” “Preview,” and “Publish.” Advanced sales or demand analysis belongs below or in analytics.

### 5. Screen and publication health

Show per-target and aggregate state:

- intended screens and screen purpose;
- online, offline, outdated, unknown, failed, or delivered status;
- latest intended version;
- last successful delivered version and time when known;
- pending, partial, or failed publication;
- mismatched operation/location/event targeting;
- retry, correct, unpublish, or restore actions.

A successful publication request is not equivalent to delivery. Healthy aggregate status must not hide one failed screen.

### 6. Current service and guest guidance

Summarize current guest-facing operational information:

- operating location/event/host;
- open/service window and last-order time;
- queue, pickup, collection, lane, counter, or window guidance;
- temporary disruption, weather, relocation, sponsor, or host notice;
- next approved stop/event only when intentionally represented;
- language and accessibility coverage.

Show unknown and not configured distinctly. Editing this information remains a core action.

### 7. Upcoming work

When relevant and available, show a short horizon:

- next service period, stop, market, event, residency, or host engagement;
- scheduled menu/content change;
- scheduled promotion or expiration;
- setup or screen-check reminder;
- unresolved conflict or required approval;
- event/location information missing before a scheduled publish.

Basic manually represented upcoming service may appear for all customers. Advanced recurring scheduling, approvals, and conflict detection must respect tier access. External source data must show source and freshness.

### 8. Multi-unit overview

For authorized users with several operations, provide an exception-first summary:

- units currently open, limited, paused, closed, relocating, or unknown;
- screens offline, outdated, failed, or unknown;
- latest publish failures or partial delivery;
- high-impact sell-outs or service disruptions;
- operations missing current location/event context;
- stale/disconnected integrations;
- upcoming events or schedule conflicts where entitled.

Do not show a multi-unit overview to users without organization scope. Bulk actions must require explicit selection and preview. Single-unit users should not see empty enterprise widgets.

## Role-aware presentation

### Operator / service staff

Prioritize:

- Quick Update;
- current service state;
- location/event and guest guidance;
- sell-outs;
- screen/publish health;
- retry and restore.

Hide billing, organization policy, and unavailable administrative controls rather than presenting a wall of disabled actions.

### Content editor

Prioritize:

- draft/menu status;
- validation;
- recent changes;
- preview;
- language/accessibility coverage;
- scheduled and active content;
- publication handoff when the user lacks publish authority.

### Publisher / manager

Prioritize:

- scope and target review;
- approvals where configured;
- publication results;
- exceptions across units/screens;
- restore and recovery;
- upcoming schedule and promotion risk.

### Administrator / owner

Add:

- organization and user administration;
- plan, add-on, and limit information;
- integration setup and health;
- multi-unit policy and inheritance;
- billing-authority actions where applicable;
- unresolved owner-level configuration.

Plan access must not grant content or publish permission automatically.

### Host / sponsor / limited collaborator

Show only authorized objects and required content scope. Host, sponsor, promoter, caterer, or property relationships do not imply organization-wide access.

## Mobile-first priorities

On phone widths, the first viewport should contain:

1. current operation/location/service state;
2. urgent exception or “all intended screens current” confirmation;
3. Quick Update;
4. publish/retry action when pending;
5. compact menu/availability and screen-health summaries.

Use progressive disclosure for analytics, full schedules, integrations, and administration. Actions should remain reachable without horizontal scrolling. Context changes and high-scope actions need clear confirmation.

## Desktop priorities

Desktop may add side-by-side exception, menu, screen, and upcoming-service panels while preserving the same hierarchy. Avoid dense control-center layouts that bury the dominant action. Multi-unit tables should support filtering, grouping, keyboard navigation, clear mixed states, and a stable selected scope.

## State coverage

The dashboard must plan for:

- first use and no operation;
- no content/menu;
- no paired screen;
- no current location/event;
- no active service period;
- loading and refreshing;
- permission denied;
- tier unavailable;
- add-on not purchased;
- integration not configured;
- integration disconnected, stale, conflicting, or partially synchronized;
- limit reached;
- scheduled, draft, active, expired, or restored content;
- screen online, offline, outdated, unknown, failed, or delivered;
- publish pending, success, partial success, and failure;
- operation open, limited, paused, relocated, closed, canceled, or unknown;
- save failure, retry, correction, undo, and restoration.

Each condition must use the correct language and recovery path.

## Plan and add-on presentation

Optional capability prompts should appear only in context, after the core action remains available. Examples:

- recurring scheduling after repeated manual stop/event updates;
- multi-unit coordination when the customer has or attempts to add several units;
- POS/inventory add-ons beside manual update, not instead of it;
- public location pages after current-location data is maintained;
- advanced analytics below core operational status;
- managed connectivity when repeated offline conditions exist.

Prompts must say what remains available, who can purchase/configure, whether a limit or permission is involved, and whether the provider/region is supported. Internal rollout states are never shown as offers.

## Impeccable planning result

The dominant mode is **Operate**. Future UI should emphasize immediate state, scope, impact, action, feedback, and recovery. `shape` guidance defines one dominant task per region and a clear hierarchy; `harden` requires first-use, empty, permission, offline, stale, partial, failure, conflict, long-name, mobile, 200% zoom, keyboard, assistive-technology, and non-color-only states; `polish` should preserve the approved Sky Blue direction without decorative noise.

## Validation

The planned dashboard covers location/event status, rapid menu and sell-out updates, screen and publish health, connectivity and recovery, service-window actions, multi-unit visibility, role-aware presentation, and mobile-first priorities. It keeps essential core actions visible and separates operational state from permission, tier, add-on, limit, connection, and rollout conditions. No implementation is authorized.
