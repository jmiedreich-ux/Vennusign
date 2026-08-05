# Café, Bakery & Dessert Required Capabilities

## Purpose

This document defines the smallest viable Café, Bakery & Dessert capability set that must remain available without a premium tier or paid integration. It inherits the Restaurant baseline and applies the operating model approved in RWP-00.30.

The required core supports ordinary daily operation, urgent guest-facing corrections, reliable screen delivery, and recovery when integrations or connectivity are absent. This is product planning only; it does not authorize UI, API, schema, billing, entitlement, ordering, payment, production, inventory, fulfillment, hardware, analytics, AI, or integration implementation.

## Core principles

1. Essential daily operation remains available through manual Vennusign workflows.
2. Industry and subtype affect terminology, defaults, starter content, and presentation, not commercial access.
3. Availability, batch, freshness, service-period, preorder, pickup, source, publication, and delivery values are product/domain state.
4. Permission controls who may edit, target, publish, override, or restore; permission is not a tier.
5. Integrations may accelerate work but must not be required to keep public information accurate.
6. Operators must always know the scope, source, freshness, target, publication result, and recovery path of an operational change.

## Required capability groups

### 1. Venue and operating-information management

Core operation includes manual management of:

- venue name, public contact information, address, timezone, and business-day context;
- regular, special, temporary, and current hours;
- service periods such as morning, bakery opening, coffee service, lunch, dessert, late night, preorder, and pickup;
- temporary closure, delayed opening, early closing, relocation, and reopening information; and
- public instructions and venue information appropriate to the represented screen purpose.

Current information must be editable and publishable without configuring recurring schedules or external calendars.

### 2. Menu, product, category, size, and option management

Core operation includes creating, editing, organizing, hiding, restoring, and presenting customer-authored:

- menus, case lists, flavor lists, collections, specials, and information content;
- products and categories;
- prices and descriptions;
- sizes, formats, temperatures, milk or base choices, flavors, toppings, and add-ins;
- images and dietary or other customer-authored labels; and
- subtype-appropriate names and ordering.

The product must preserve customer-authored names and existing content when subtype, terminology preference, service model, or packaging changes.

### 3. Rapid availability, sell-out, batch, and freshness updates

Core operation includes rapid manual actions to:

- mark an item available, unavailable, sold out, limited, next batch, or available again;
- add, correct, or remove an authoritative expected-return value;
- add, correct, supersede, or remove customer-authored batch and freshness guidance;
- distinguish walk-in, preorder, pickup, service-period, and whole-venue state;
- preserve item content, price, options, source relationships, and future use when current availability changes; and
- undo or restore an incorrect operational update.

Unknown quantity, return time, readiness, freshness, safety, and production facts must remain unknown. Vennusign must not infer them from timestamps or unrelated data.

### 4. Preorder, custom-order, and pickup presentation

Core operation includes manual public communication of:

- whether preorder or custom-order service is offered;
- known opening, cutoff, collection, and pickup periods;
- pickup location, counter, window, area, route, or changed instructions;
- temporary pause, closure, relocation, or resumption; and
- public instructions that contain no private guest or transaction data.

This capability does not include order capture, payment, production tracking, ready-state automation, customer notification, or fulfillment management.

### 5. Screen pairing, purpose, and explicit targeting

Core operation includes:

- pairing or selecting a screen;
- identifying the screen purpose, venue, area, service context, and intended content;
- selecting explicit target screens before publication;
- showing online, offline, outdated, unknown, and latest-delivery state separately;
- preventing a screen's online status from being treated as proof of current content; and
- supporting venue-level differences without silent organization-wide overwrite.

No paid capability may be required to target and update the first operational screen accurately.

### 6. Preview and publication

Core operation includes:

- previewing the intended content, current operational state, effective context, and selected targets;
- publishing immediately;
- confirming success per target rather than only at request level;
- showing pending, partial, failed, canceled, superseded, and unknown results;
- retaining the intended revision when delivery fails; and
- retrying safely without duplicate or unintended publication.

Publication must preserve customer-authored content, current state, source/freshness context, and restoration points.

### 7. Correction, supersession, undo, and restoration

Core operation includes:

- correcting an active value or message;
- removing or expiring outdated guidance;
- superseding a previous publication;
- undoing an accidental action when safe;
- restoring a previous known-good version; and
- explaining the venue, screens, content, state, and time affected by restoration.

Recovery must remain available when integrations, scheduling, analytics, or premium workflows are unavailable.

### 8. Source, freshness, conflict, and manual fallback

Core operation includes visibility of:

- whether a value is manually authored or externally supplied;
- source identity and last-known freshness when represented;
- disconnected, stale, unknown, conflicting, and overridden states;
- the authoritative value and reason an override is allowed; and
- safe manual fallback without deleting the imported relationship.

Source authority is configuration and permission policy, not entitlement. An integration outage must not prevent an authorized operator from maintaining essential public information.

### 9. Roles and permissions

The required permission model distinguishes at least:

- view public and operational state;
- edit content and product state;
- edit sensitive source or freshness detail;
- approve where an approval workflow exists;
- select targets and publish;
- override an external source;
- perform bulk actions; and
- restore or undo.

A user lacking permission must see a clear explanation and a safe next step. Commercial access and operational authority must never be collapsed into one status.

### 10. Required states and feedback

Future operating surfaces must cover:

- first use and no-content states;
- no-screen and unpaired-screen states;
- loading, saving, validation, permission, conflict, stale-source, and concurrent-edit states;
- every canonical availability and service state;
- publish pending, success, partial success, failure, cancellation, and supersession;
- screen offline, outdated, unknown, and recovered;
- undo, correction, restore, and retry; and
- saved-but-not-published and published-but-not-confirmed distinctions.

Feedback must be specific, timely, non-color-dependent, and associated with the affected object and target.

### 11. Accessibility, responsiveness, and localization readiness

Core operation and public output must support:

- keyboard and assistive-technology use;
- visible labels, focus, status, errors, and success feedback;
- 200% zoom and reflow;
- phone and desktop operation;
- long customer-authored names and localization expansion;
- non-color status communication;
- reduced motion; and
- distance readability, glare, crowded counters, and time-pressured operation.

Basic customer-authored language variants and accessible content remain core where the underlying content model supports them. Advanced localization workflow and translation services remain optional candidates.

## Subtype emphasis without subtype entitlements

All subtypes receive the complete required core. Defaults and emphasis may vary:

| Subtype | Primary required emphasis |
| --- | --- |
| Café | service periods, drinks and food, specials, hours, pickup, and venue information |
| Coffee Shop | rapid drink and pastry availability, sizes/options, queue and pickup clarity |
| Tea Shop | tea styles, temperatures, sweetness, toppings, add-ins, and seasonal drinks |
| Bakery | today's selection, batch state, sell-outs, next batch, preorder, and pickup |
| Patisserie | collections, flavors, sizes, custom-order, limited availability, and pickup |
| Bakery-Café | bakery case, beverage and meal periods, counter, pickup, and optional table service |
| Dessert Shop | portions, flavors, toppings, combinations, temporary state, and pickup guidance |
| Frozen Dessert Shop | current flavors, sizes, vessels, toppings, limited state, and take-home options |
| Juice & Smoothie Bar | bases, sizes, ingredients, add-ins, bowls, availability, and pickup |
| Unspecified / General Café | neutral menu, item, category, service-period, availability, and pickup language |

Subtype selection must never remove a required capability or create a paid gate.

## Classification summary

- **Core capability:** all eleven required groups above.
- **Product/domain state:** represented venue, content, product, batch, freshness, availability, service, preorder, pickup, source, target, publication, delivery, and recovery values.
- **Permission:** authority to edit, approve, target, publish, override, bulk-change, undo, restore, or view restricted detail.
- **Tier candidates:** advanced scheduling, reusable rotations, campaigns, approvals, orchestration, advanced presentation, extended history, analytics, governance, and optimization.
- **Independent add-on candidates:** POS, inventory, production, ordering, payment, fulfillment, loyalty, supplier, weather, event, traffic, translation, identity, hardware, monitoring, support, and AI services.
- **Limits:** counts, volume, frequency, retention, export, storage, transactions, integration consumption, support, and AI usage.
- **Rollout flags:** temporary internal release, migration, compatibility, and emergency-disable controls only.

## Impeccable planning brief

Mode is **Operate**. The primary user is an owner, manager, or authorized staff member correcting guest-facing information under time pressure.

- Lead with venue, current service context, urgent state, affected product or message, target screens, publish result, and recovery.
- Keep daily actions visible; move schedules, history, sources, packaging, and advanced configuration behind progressive disclosure.
- Use explicit verb-object actions such as `Mark sold out`, `Set next batch`, `Update pickup instructions`, `Publish to 3 screens`, and `Restore previous version`.
- Never hide a required action behind an upgrade prompt.
- Locked optional capabilities must explain the operational outcome, preserve the current task, and never imply the user lacks permission when the issue is commercial access.

## Validation

This required set covers every concern in issue #506, preserves Restaurant inheritance, keeps essential operation core, and separates state, permission, tier, add-on, limit, and rollout. No product behavior or jurisdiction-specific policy is introduced.