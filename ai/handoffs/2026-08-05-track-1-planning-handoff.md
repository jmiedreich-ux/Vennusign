# Vennusign Track 1 Planning Handoff

**Date:** 2026-08-05  
**Status:** Track 1 planning discussions complete; implementation not started  
**Owner review:** Required after the five-RWP execution batch

## Current Position

Track 0 industry and product architecture is complete and closed. Track 1 has now been discussed and approved at the product/architecture level.

No Track 1 implementation has started. The next working session should convert the approved discussions into detailed RWP records and completeness checklists, obtain owner confirmation of those records, and then authorize the five-RWP sequential execution batch.

## Governing Working Process

The owner-approved sequence is:

1. Discuss the intended outcome.
2. Obtain owner agreement.
3. Produce the detailed RWP and completeness checklist.
4. Execute an approved batch of up to five RWPs sequentially.
5. Validate each RWP before advancing.
6. Automatically correct every clear, bounded implementation gap inside the same RWP.
7. Revalidate after correction.
8. Stop only when a new owner decision, major scope expansion, conflicting requirement, unavailable dependency, high-risk action, or unresolved repeated failure prevents a confident correction.
9. Conduct one owner acceptance review after the five-RWP batch.
10. Light planning for the next track may begin while owner acceptance testing for the current track is underway or has not yet started. That next-track planning must not be marked complete until all potential changes arising from the current and earlier tracks have been evaluated and incorporated or explicitly ruled out. Implementation of the next track remains blocked until the owner approves closure of the current track.

A fixable gap must not be deferred into a later cleanup cycle.

## Completion Standard

An implementation area is complete only when all applicable layers are complete:

- data and persistence;
- server rules and authorization;
- API;
- customer-facing UI;
- navigation and entry points;
- buttons and actions;
- create/edit/save/cancel/validation behavior;
- loading, empty, error, offline, recovery and fallback states;
- screen/player output where applicable;
- responsive and accessible behavior;
- audit/history where required;
- focused automated tests;
- demonstrated customer journey;
- documentation and handoff.

Every RWP must include a completeness matrix covering discoverability, navigation, display, create, edit, remove/disable, wired actions, authority, states, responsive UI, player impact, recovery, tests, evidence, exclusions and known limitations.

## Pre-Production Replacement Rule

Vennusign is a pre-production Version 1 system. Existing code, schemas, tables, SQL scripts, APIs, routes and tests may be changed or deleted whenever needed to implement the approved architecture cleanly.

There is no migration requirement and no compatibility obligation. Do not describe this work as migration.

Old generic feature tables, seeds, services, claims, route gates and tests may be deleted or rewritten directly. Development and test databases may be reset and recreated.

## Track 1 — Capability, Entitlement and Authority Foundation

Approved Track 1 RWPs:

1. **Track 1.01 — Canonical Capability Model and Current-Code Reconciliation**
2. **Track 1.02 — Server Capability Decision and Reason Contract**
3. **Track 1.03 — Scoped Permission and Authority Model**
4. **Track 1.04 — Essential Core and Current Gate Replacement**
5. **Track 1.05 — Track Validation and Handoff**

The five RWPs may execute sequentially after all five detailed plans and completeness checklists are approved.

## Track 1.01 Approved Decisions

### Capability definition

Only actual product actions and outcomes belong in the capability registry. The following remain separate typed models:

- permissions;
- product states;
- allowances;
- add-ons and external services;
- layouts/templates;
- rollout controls;
- navigation routes.

### Naming

Canonical format:

```text
domain.resource.action
```

Examples:

```text
screen.device.pair
content.item.availability.update
publishing.release.publish
workflow.approval.request
analytics.delivery_health.view
organization.content.bulk_publish
```

Capability identifiers must not contain tier names, industry names, provider names or UI labels. `publishing.*` remains a distinct domain.

### Approved capability groups

- content;
- publishing;
- screen;
- schedule;
- workflow;
- organization;
- localization;
- analytics;
- branding;
- account;
- support.

### Classification

The proposed Version 1 capability list was classified into:

- universal core;
- advanced native tier capabilities;
- Portfolio/Enterprise governance capabilities;
- deferred or separately treated concerns.

Tier placement is metadata and may change without changing capability IDs.

First-venue creation is a core onboarding operation. Additional venue creation is controlled by allowance. Multi-location governance remains a Portfolio capability.

### Current-code disposition

Every existing generic feature concept must move into exactly one correctly typed Version 1 model or be removed.

Examples:

- `photo_grid`, `classic_diner`, `all_layouts` → layout/template catalog;
- `pos_integration`, `ai_translation`, `ai_custom_builder` → add-on/service model;
- `menus`, `screens`, `themes`, `tap_list`, `scheduling` → navigation destinations, not capabilities;
- online/offline, paired/unpaired, draft/published, available/unavailable → product state;
- screen, venue, user, storage and usage quantities → allowance model;
- broad keys such as `analytics`, `multi_location`, `quick_update`, `staff_app` → split or remove.

## Track 1.02 Approved Decisions

The server is the final authority for every product action. The UI may improve presentation, but it may not independently determine entitlement or authority.

### Decision values

- `allowed`;
- `allowed_with_conditions`;
- `denied`;
- `unavailable`;
- `temporarily_blocked`.

### Required result fields

- decision;
- stable reason code;
- category;
- capability;
- localized message key;
- structured message parameters;
- correlation ID.

Optional structured fields include resolution, conditions, details, context and retry guidance.

### Evaluation areas

The server considers identity/context, capability existence, rollout availability, customer entitlement, permission, add-on status, allowance, resource state and request validation.

Preview evaluation improves the UI, but every state-changing endpoint must reevaluate immediately before execution.

Batch evaluation must be supported for navigation and dashboards.

### Multilingual foundation

Customer-visible server decisions return stable locale-neutral message keys and structured parameters. Product translations are stored in repository-based catalogs with fallback such as:

```text
fr-CA → fr → en-US
```

User, organization and venue locale preferences are stored as data. Product-interface localization remains separate from customer-authored multilingual screen content.

## Track 1.03 Approved Decisions

A capability says what the product can do. A permission says what the actor may do. Scope says where the authority applies.

Approved scope types:

- platform;
- organization;
- venue-group;
- venue;
- resource;
- self.

Roles are collections of permissions. Role assignments are scoped and may be temporary. Authority normally inherits downward through the organization structure.

Initial protected system roles may include:

- Organization Owner;
- Organization Administrator;
- Venue Administrator;
- Content Manager;
- Content Editor;
- Publisher;
- Viewer;
- Support Operator.

Support access is not normal customer membership. It requires dedicated authority, explicit context/reason, prominent indication, time bounds and complete audit evidence.

Every state-changing endpoint enforces both capability availability and scoped permission at the target scope.

## Track 1.04 Approved Decisions

Track 1.04 is a full replacement of the current unfinished gate architecture.

It replaces or removes:

- generic feature tables and seeds;
- tier-feature joins used as direct authority;
- flat claims mixing entitlements, permissions, routes, add-ons and rollout flags;
- route names used as capabilities;
- browser-only entitlement catalogs;
- hard-coded core capability arrays;
- client-side authority decisions;
- old feature-resolution services;
- obsolete tests.

The new typed foundation includes capability, permission, role, role assignment, allowance, add-on, product state, rollout control and decision result models.

Essential core must preserve a safe operating loop for Free and paid customers: create/edit/preview content, pair the permitted screen, publish, confirm, replace, unpublish, retry, restore and recover. Quantity may be limited by allowance, but core recovery must not be blocked.

The obsolete generic feature-gating path must no longer be authoritative anywhere in SQL, server code, sessions, navigation, UI, tests, player administration or support tooling.

## Track 1.05 Approved Decisions

Track 1 closes only when the combined implementation is proven through automated evidence and owner-visible customer journeys.

### Important UI boundary

Track 1 must not assume that full management UIs already exist for roles, tiers, allowances, add-ons, rollout controls or locale administration.

Track 1 test setup may use:

- deterministic seed/reset scripts;
- test fixtures;
- direct data setup;
- mock/test adapters;
- minimal diagnostic pages where visual proof is necessary.

Track 1 must use real customer-facing UI only for the actions and surfaces it actually affects. Missing management interfaces must be explicitly listed as deferred to their proper tracks.

### Automated validation responsibility

Automation proves:

- schema and deterministic scenarios;
- decision-engine permutations;
- permission and scope behavior;
- allowance calculations;
- add-on-state decisions;
- endpoint enforcement;
- localization fallback;
- audit creation;
- route/button decision responses;
- removal of the old authoritative gate path;
- focused server, UI and player tests.

### Owner acceptance responsibility

The owner tests customer-visible behavior rather than internal permutations.

Prepared owner tests must include:

1. Free customer operating loop.
2. Content Editor versus Publisher permission experience.
3. Screen-capacity explanation and recovery.
4. Advanced-capability explanation while core remains usable.
5. Offline-screen publishing, reconnection and status recovery.
6. Translated and fallback system messages.
7. Navigation, visible controls, disabled explanations and absence of dead actions.
8. Overall judgment on clarity, usefulness, recovery and alignment with approved product intent.

The owner is not responsible for manually testing raw database structures, every permission permutation, internal diagnostics, every locale, full onboarding, full billing, real external providers, or management screens not built by Track 1.

### Owner review package

Before owner review, provide:

- prepared accounts and seeded scenarios;
- exact numbered steps and expected results;
- direct page links;
- reset instructions;
- player online/offline controls;
- Pass / Fail / Needs Adjustment recording;
- automated evidence for checks the owner need not repeat;
- explicit deferred-interface list with future track ownership.

Track 2 implementation remains blocked until the owner explicitly approves Track 1 closure. Light Track 2 planning may begin before or during Track 1 owner acceptance testing, but Track 2 planning cannot be marked complete until all potential changes arising from Track 1 and earlier tracks have been evaluated and incorporated or explicitly ruled out.

## Onboarding Boundary

Track 1 does not implement full onboarding.

It supplies foundations required by onboarding, including first-venue and first-screen authority, Free core access, pairing allowance decisions, scope/role behavior, localized messages, route visibility and server enforcement.

Full signup, guided setup, industry selection, starter content, first-publish guidance, trials, conversion and interrupted-flow recovery belong to Track 8.

## Next Action

At the next session:

1. Review this handoff.
2. Revise the existing Track 1 issue titles and descriptions so they match the approved Version 1 replacement language and contain no migration/legacy framing.
3. Produce the detailed RWP records and completeness checklists for Track 1.01 through Track 1.05.
4. Present those records for owner confirmation before execution.
5. After confirmation, authorize one sequential batch of up to five RWPs using automatic bounded remediation and a final owner acceptance gate.

## Current Boundaries

- Track 1 implementation is not yet authorized merely by this handoff.
- Do not start Track 2 implementation before owner approval of Track 1 closure. Light Track 2 planning may begin, but it cannot be marked complete until potential changes from Track 1 and earlier tracks have been evaluated.
- Do not resume RWP-13.06 unchanged.
- Phase 14 and later remain paused.
- No full onboarding, billing-management UI, tier-management UI, role-management UI, allowance-management UI or add-on-management UI should be claimed as delivered by Track 1 unless explicitly added and approved.
- External-system integration tests remain skipped unless separately authorized.
