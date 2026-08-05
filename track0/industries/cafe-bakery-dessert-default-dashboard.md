# Café, Bakery & Dessert Default Dashboard

## Purpose

This document defines the default daily dashboard for Café, Bakery & Dessert operators. It is product planning only and does not authorize UI, API, schema, analytics, billing, entitlement, or implementation.

The dashboard is **exception-first and task-first**. It helps an authorized operator keep current guest-facing information accurate, publish urgent changes, understand screen delivery, and recover from failures. Promotions and analytics never outrank operational truth.

## Dashboard outcome

The user should be able to answer, at a glance:

1. What venue and service context am I operating?
2. What is wrong, stale, unpublished, or uncertain right now?
3. What changed today or is changing next?
4. Which products, periods, pickup instructions, screens, or sources need attention?
5. What is the fastest safe action and how will I verify delivery?

## Persistent context

The dashboard always exposes:

- organization and venue;
- primary subtype and neutral fallback where relevant;
- local date, time, timezone, and business-day context;
- current service period and service model;
- current user role and scoped authority;
- selected area, counter, menu, pickup context, or screen group when narrowed; and
- source/freshness context for externally supplied values.

Changing scope never silently carries a destructive or high-impact selection into another venue.

## Priority hierarchy

### 1. Urgent public-impact exceptions

Show first when present:

- wrong or unknown availability affecting published content;
- sold-out, limited, next-batch, pickup, preorder, changed-hours, closure, or reopening information needing publication;
- stale, conflicting, disconnected, or unknown authoritative source;
- failed, partial, canceled, or unknown publication result;
- screen offline, outdated, showing an older revision, or not confirmed;
- saved but unpublished operational changes;
- active temporary message missing an end, correction, or supersession path; and
- permission or ownership conflict preventing a needed correction.

Each exception identifies the object, venue, service context, source, affected screens, public impact, age, and safe next action.

### 2. Quick operational actions

Always keep appropriate core actions close to the top:

- `Mark sold out`;
- `Mark available`;
- `Set limited`;
- `Set next batch`;
- `Update expected return`;
- `Update pickup instructions`;
- `Pause or resume preorder/pickup information`;
- `Update changed hours`;
- `Publish closure or reopening`;
- `Edit product or price`;
- `Publish to selected screens`;
- `Retry failed delivery`; and
- `Restore previous version`.

Actions use explicit venue, object, service-context, and target scope. High-impact actions show preview and confirmation.

### 3. Now, today, and next

Show a compact operational timeline:

- current and next service periods;
- regular and temporary hours;
- active and upcoming menus or collections;
- known preorder cutoff and pickup periods;
- current product, batch, or flavor exceptions;
- upcoming planned changes when available; and
- unresolved handoff items.

Manual current-state operation remains usable without advanced scheduling.

### 4. Products and freshness

Provide a concise operational view of:

- available, unavailable, sold-out, limited, next-batch, and available-again items;
- customer-authored or authoritative freshness guidance;
- expected-return values and unknown timing;
- rapidly changing products, flavors, collections, or batches;
- active seasonal or promotional items; and
- source identity, freshness, conflicts, and overrides.

The dashboard never infers quantity, safety, freshness, readiness, or return time.

### 5. Preorder and pickup

Show:

- whether public preorder/custom-order service is offered;
- current opening or cutoff information;
- pickup period, location, counter/window/area, and instructions;
- paused, relocated, closed, resumed, or unknown state;
- private-data warnings; and
- target screens carrying the information.

Public dashboard content excludes guest names, order numbers, payment state, contact details, and private fulfillment data by default.

### 6. Screen and publication health

For each relevant screen or group, show separately:

- pairing and ownership state;
- intended purpose and venue context;
- online, offline, or unknown connection;
- intended revision;
- latest confirmed delivered revision and time;
- current, outdated, pending, partial, failed, canceled, excluded, or unknown delivery;
- source/content freshness where applicable; and
- retry, correction, supersession, or restoration action.

Healthy aggregate status cannot hide one failed or unknown target.

### 7. Upcoming work and setup

After urgent and current operational needs, show:

- incomplete but non-blocking setup;
- unpaired or unused screens;
- draft products or content;
- deferred service periods, pickup details, languages, teammates, or sources;
- schedules, campaigns, templates, approvals, integrations, analytics, or managed services available to explore; and
- subscription or allowance information when contextually relevant.

Setup prompts do not impersonate operational emergencies.

## Subtype emphasis

Every subtype uses the same core architecture with different emphasis:

| Subtype | Dashboard emphasis |
| --- | --- |
| Café | current periods, menu and specials, availability, pickup, hours, screens |
| Coffee Shop | drink/pastry exceptions, sizes/options, seasonal drinks, queue/pickup clarity |
| Tea Shop | tea styles, temperature/sweetness/options, toppings, seasonal products |
| Bakery | today’s selection, batch state, sell-outs, next batch, preorder, pickup |
| Patisserie | collections, limited availability, sizes/flavors, custom order, pickup |
| Bakery-Café | bakery case, beverage/food periods, counter, pickup, optional table service |
| Dessert Shop | flavors, portions, toppings, combinations, availability, wait/pickup guidance when authoritative |
| Frozen Dessert Shop | current flavors, sizes/vessels/toppings, limited state, take-home options |
| Juice & Smoothie Bar | bases, ingredients, add-ins, bowls, availability, pickup |
| Unspecified / General Café | neutral menu, product, availability, period, pickup, venue, and screen language |

Subtype emphasis never changes commercial access.

## Role-aware presentation

- Owners/organization administrators see organization and venue status, subscription/add-on context, and administration within permission.
- Venue managers see current venue operation, exceptions, content, screens, staff-relevant sources, and recovery.
- Content operators see only permitted products, messages, targets, publication, and delivery actions.
- Reviewers see pending approvals and impact without receiving implicit publish authority.
- Read-only users see status and ownership without misleading active controls.

Permission denial states identify who can act and never appear as upgrade prompts.

## Locked and unavailable capability principles

The dashboard distinguishes:

- included core capability;
- optional tier capability not included;
- independent add-on not purchased;
- add-on purchased but not configured or disconnected;
- permission denied;
- usage/quantity limit reached;
- unsupported context;
- rollout-disabled capability; and
- product state such as sold out, closed, or pickup paused.

Locked optional cards cannot obscure urgent core work, dominate the dashboard, reuse business-state language, or erase current data. They explain the outcome, preserve work, and provide an appropriate next action.

## Mobile priorities

Mobile prioritizes:

1. venue and current service context;
2. urgent exceptions;
3. rapid availability/hours/pickup actions;
4. affected-screen selection and publication;
5. delivery confirmation and recovery;
6. current period and handoff; and
7. deferred setup and optional discovery.

Mobile supports one-handed use, large targets, short scan paths, interrupted operation, low connectivity, glare, and crowded counters. Destructive or broad actions require explicit review.

## Desktop priorities

Desktop adds:

- multi-column exception and operational views;
- richer product, period, source, screen, and delivery comparison;
- safe bulk selection with mixed-state visibility;
- organization/venue navigation;
- history and upcoming-work context; and
- progressive disclosure for advanced planning and governance.

Desktop does not bury core action behind analytic or administrative density.

## Required states

The dashboard plans for:

- first use and no-content;
- no screen, unpaired screen, offline, outdated, unknown, and healthy screen;
- no exception and no upcoming work;
- loading, saving, validation, permission, and concurrent edit;
- stale, disconnected, conflicting, and overridden source;
- saved-not-published, pending, success, partial, failure, canceled, superseded, and unknown publication;
- limit reached, tier locked, add-on required, add-on disconnected, unsupported, and rollout-disabled;
- phone and desktop responsive states;
- correction, retry, undo, restore, and recovered states; and
- mixed venue, screen, source, and permission states.

## Accessibility and environmental requirements

- Logical headings and landmarks
- Keyboard and assistive-technology operation
- Visible focus, persistent labels, specific errors, and status announcements
- 200% zoom and reflow
- Non-color status and redundant icon/text cues
- Long names, localization expansion, and local date/time clarity
- Reduced motion
- Large touch targets
- Distance, glare, low-light, crowding, noise, and time-pressure consideration

## Impeccable dashboard brief

Mode is **Operate**.

- Exceptions outrank summaries.
- Current truth outranks prediction.
- Action and verification appear together.
- The visual hierarchy stays calm when healthy and unmistakable when intervention is required.
- Sky Blue administrative direction can support navigation and confidence, while operational severity uses accessible, restrained status treatment.
- Empty states teach the safest next real action.
- Optional discovery remains contextual and secondary.

## Validation

This dashboard covers daily priorities, rapid product/sell-out/freshness updates, preorder/pickup visibility, screen and publication health, service-period awareness, subtype variants, role-aware presentation, locked-capability principles, and mobile/desktop priorities. It preserves required core operation and does not implement UI or analytics.