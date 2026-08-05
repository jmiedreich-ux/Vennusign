# Café, Bakery & Dessert Operating Characteristics

## Purpose

This document defines the operating characteristics that meaningfully distinguish Café, Bakery & Dessert venues from the approved Restaurant baseline. It is documentation and product planning only. It does not authorize UI, API, schema, billing, entitlement, ordering, payment, production, inventory, fulfillment, hardware, analytics, AI, or integration implementation.

The profile must support early and variable business days, independently active service periods, batch-led availability, rotating daily products, rapid sell-out and return communication, preorder and pickup information, seasonal demand, counter-led throughput, optional table service, and subtype-specific rhythms.

## Operating principles

1. Venue, service period, content, item, batch, preorder window, pickup context, publication, and screen-delivery state are independent.
2. Ordinary daily operation must remain possible without POS, inventory, production, ordering, loyalty, or fulfillment integrations.
3. Unknown quantity, freshness, readiness, return time, or production facts remain unknown.
4. Operating state is product/domain state, never a feature flag or commercial entitlement.
5. Permission determines who may change or publish state; packaging controls advanced workflow or external services.
6. Every urgent change requires explicit venue, content, and screen scope, preview, publication result, and recovery.
7. Restaurant menu, item, category, price, dietary, screen, publishing, hours, and recovery capabilities remain inherited unless this document states a meaningful delta.

## Business day, early hours, and service periods

Café, bakery, coffee, and dessert operations may begin preparation or public service before typical Restaurant dayparts and may continue after midnight.

- Business day follows the venue operating context rather than calendar date alone.
- Preparation, public opening, preorder cutoff, pickup, counter service, table service, and display periods may begin and end independently.
- A venue may be closed for walk-in service while pickup remains active, or open while one counter, category, batch, or service period is unavailable.
- Cross-midnight and pre-dawn work must remain attached to the intended local business day.
- Venue timezone and effective date must be explicit in future scheduling or operational surfaces.
- Manual current-period selection and immediate publication remain core.
- Recurring schedules, conflict detection, and automated transitions are tier candidates.

Service periods may use customer-authored names such as morning, breakfast, coffee service, bakery opening, lunch, afternoon, dessert, evening, late night, or preorder pickup. Periods may overlap. Planned, active, paused, ended, canceled, and upcoming periods remain distinguishable.

## Batch production and freshness

Where represented, a batch may be planned, in preparation, expected later, available, limited, sold out, ended, canceled, replaced, or unknown.

- Batch state communicates public availability; it does not create production-management or food-safety functionality.
- “Fresh,” “baked today,” “made this morning,” shelf-life, safety, or quality claims must be customer-authored or supplied by an authoritative source.
- Timestamps alone never justify freshness claims.
- Stale or expired guidance must be removable, supersedable, or visibly flagged to authorized operators.
- Manual correction and removal remain core.
- Expiry automation, production synchronization, prediction, and optimization are tier or add-on candidates.

Shift handoff must surface active batch messages, sold-out states, expected-return values, unpublished changes, failed or partial delivery, stale sources, conflicts, and restoration points.

## Rotating products, limited offers, and seasonal demand

Venues may rotate pastries, breads, beans, teas, flavors, toppings, desserts, juices, produce, specials, limited releases, and seasonal collections.

Required manual operation includes add, edit, feature, unfeature, mark unavailable, mark sold out, restore, correct, and publish. A limited label must not imply a known count unless an authoritative count exists.

- Manual rotation and seasonal updates remain core.
- Date-driven rotations, recurring campaigns, reusable seasonal templates, approvals, and cross-venue orchestration are tier candidates.
- POS, inventory, production, supplier, campaign, weather, event, traffic, or loyalty synchronization are add-on candidates.
- Predictive demand and AI recommendations require review and may not replace manual operation.
- Public content must not create unsupported scarcity or urgency.

## Availability and sell-out transitions

Canonical states include available, unavailable, sold out, limited, next batch, available again, preorder closed, and pickup paused.

- Sold out does not delete the item or erase its future schedule.
- Available again does not invent quantity, return time, or batch identity.
- Preorder closed does not imply the item is unavailable for walk-in service.
- Pickup paused does not imply the venue or item is otherwise unavailable.
- A whole-venue closure must not silently destroy item or period state.
- State changes preserve price, description, imagery, options, customer-authored names, source relationships, and history unless intentionally edited.

Rapid manual changes, clear scope review, preview, per-screen publication confirmation, correction, undo, and restoration remain core.

## Preorder, custom order, and pickup information

The profile requires accurate public communication about preorder and pickup without defining order capture or private fulfillment state.

Core manual information includes:

- whether preorder or custom-order service is offered;
- known opening, cutoff, collection, or pickup periods;
- public pickup location, counter, window, area, or route;
- temporary pause, closure, relocation, or changed instructions;
- appropriate public instructions; and
- correction, expiry, supersession, and restoration.

Public screens must not expose guest names, contact details, order numbers, payment state, room numbers, or private order information by default. Ordering, payment, production queue, ready-state, customer notification, and transaction history require later privacy, authorization, data, and integration decisions.

## Counter, table, and mixed service

### Counter-led service

Counter and queue environments prioritize rapid scanning, clear prices, current availability, sizes, options, pickup instructions, and distance readability. Daily quick updates and publication should be more prominent than deep configuration.

### Table or casual service

Cafés, bakery-cafés, patisseries, and dessert venues may also provide table service. Restaurant table-service capability remains inherited. Table service is a product trait, not a commercial entitlement or distinct subtype.

### Mixed service

Walk-in counter service, table service, preorder pickup, delivery pickup, drive-through, and event service may operate simultaneously. Each context may have independent hours, instructions, availability, and screen targets. One context must not automatically control another.

## Screen purposes

Common screen purposes include:

- beverage or food menus;
- bakery-case or rotating-product selections;
- current flavors, batches, sold-out, limited, or expected-return messages;
- seasonal and promotional content;
- preorder, custom-order, and pickup guidance;
- queue, counter, table, or service instructions;
- changed hours, closure, reopening, and service-period communication;
- packaged retail or merchandise information; and
- venue information and wayfinding.

For every publication, operators must understand venue, screen purpose, effective period, selected targets, preview, delivery result, and recovery path. Screen online status does not prove it displays the latest intended revision.

## Source authority and conflict

Operating facts may be manually authored or later supplied by POS, inventory, production, ordering, loyalty, campaign, calendar, weather, event, or other systems.

- Authorized operators must see source identity and freshness.
- Stale or disconnected sources must not appear current.
- Source authority is product configuration and permission policy, not entitlement.
- Manual core operation remains available when integrations are absent or unavailable.
- Conflict handling preserves values, identifies differences, prevents silent loss, and supports retry or manual fallback.
- Customer-authored wording must not be overwritten without explicit authority.

## Multi-venue behavior

Each venue retains its own subtype, timezone, hours, periods, products, batches, availability, pickup contexts, screen targets, source authority, and overrides.

Organization templates may seed content but cannot silently overwrite local facts. Copy and bulk actions require explicit venue and screen scope, mixed-state visibility, impact review, permission checks, partial-success reporting, cancellation, and recovery.

Cross-venue campaigns, approvals, governance, coordination, and analytics are tier candidates. Venue, screen, user, product, schedule, language, history, storage, integration, transaction, export, support, and AI quantities are limits.

## Subtype operating rhythms

| Subtype | Typical rhythm | Daily information emphasis |
| --- | --- | --- |
| Café | Multiple beverage and light-food periods | menu, specials, hours, availability, pickup, venue information |
| Coffee Shop | Early opening, commuter peaks, drink customization | drink sizes/options, seasonal drinks, pastry state, queue and pickup clarity |
| Tea Shop | Specialty preparation and option selection | tea styles, temperature, sweetness, toppings, sizes, seasonal drinks |
| Bakery | Early batches, case rotation, frequent sell-outs | today’s selection, batch state, next batch, preorder, pickup |
| Patisserie | Crafted collections and limited quantities | collections, flavors, sizes, custom-order and pickup information |
| Bakery-Café | Production plus beverages and meal periods | case, beverage, meal, counter, pickup, and optional table-service state |
| Dessert Shop | Afternoon/evening peaks and made-to-order combinations | portions, flavors, toppings, combinations, wait or pickup guidance when known |
| Frozen Dessert Shop | Rapid peaks and rotating flavors | flavors, sizes, vessels, toppings, limited state, take-home options |
| Juice & Smoothie Bar | Made-to-order drinks and seasonal ingredients | bases, sizes, ingredients, add-ins, bowls, availability, pickup |
| Unspecified / General Café | Mixed or not yet classified | neutral menu, category, availability, service-period, pickup, and venue language |

## Capability-classification guardrails

### Core capabilities

Manual product and content management; rapid availability, sell-out, limited, next-batch, return, changed-hours, closure, pickup, preorder, and service-period communication; explicit targeting and preview; immediate publishing; per-target delivery confidence; correction, supersession, undo, and restoration; offline, stale, failure, partial-delivery, and conflict awareness; accessible customer-authored content; and operation without paid integrations remain core.

### Product/domain state

Industry, subtype, venue timezone, business day, service period, service model, item, category, option, batch, freshness guidance, availability, expected return, preorder window, pickup context, seasonal state, source, freshness, target, publication, delivery, and restoration point are state where represented.

### Permissions

Authority to edit, approve, target, publish, override sources, perform bulk actions, restore, or view restricted detail is permission.

### Tier candidates

Recurring scheduling, reusable rotations, advanced presentation, campaigns, approvals, coordinated multi-screen or multi-venue workflow, bulk administration, extended history, analytics, loyalty workflow, and optimization are tier candidates.

### Add-on candidates

POS, inventory, production, ordering, payment, fulfillment, loyalty, supplier, weather, event, traffic, campaign, premium translation, managed hardware, monitoring, and AI services are add-on candidates when they create independent cost or value.

### Limits and rollout

Counts and retention windows are limits. Temporary release, migration, compatibility, or emergency-disable controls are internal rollout flags. They never represent sold-out, closed, pickup, preorder, batch, or other business state.

## Impeccable planning brief

Mode is **Operate**. The primary user is an owner, manager, or authorized staff member correcting current guest-facing information under time pressure.

- Hierarchy starts with venue and service context, then urgent state, product/batch change, targets, publish result, and recovery.
- Daily actions remain primary; scheduling, history, source, packaging, and advanced configuration use progressive disclosure.
- Future flows must cover first use, no content, no screens, loading, validation, permissions, every canonical availability state, unknown timing, stale sources, offline mode, conflicts, concurrent edits, publish success, partial delivery, failure, undo, correction, supersession, and restoration.
- Responsive planning must support phone and desktop, long names, localization expansion, 200% zoom, keyboard operation, assistive technology, reduced motion, non-color status, glare, distance, and crowded environments.
- No builder may invent freshness, quantity, readiness, return, safety, privacy, or integration policy.

## Validation

This profile addresses early hours, business-day boundaries, service periods, batch production, freshness windows, rotating products, sell-outs, preorders, pickup, seasonal demand, counter service, table service, subtype differences, defaults, terminology, content, screen purposes, and classification. Restaurant inheritance remains authoritative. Essential daily operation remains core. No product behavior or jurisdiction-specific policy is introduced.
