# Café, Bakery & Dessert Onboarding Experience

## Purpose

This document defines the first-run and resume experience for Café, Bakery & Dessert organizations and venues. It is documentation and product planning only. RWP-13.06 remains paused. No onboarding, signup, pricing, billing, player, pairing, publication, UI, API, schema, migration, or product behavior is implemented.

## First-value outcome

The onboarding aha moment is:

> Accurate, subtype-aware venue information and one useful menu or availability update are visibly delivered to the first intended screen, with clear confirmation and a recovery path.

The user does not need to finish complete venue modeling, connect an external system, configure advanced schedules, choose an optional add-on, or compare tiers before reaching this outcome.

## Supported starting contexts

The journey supports:

- a new organization and first owner;
- an existing organization adding its first Café venue;
- a mixed-industry organization adding another venue type;
- an invited manager or operator with bounded permissions;
- a returning user resuming incomplete setup;
- an experienced user choosing a faster setup path; and
- an organization with an existing paired screen or content library.

The flow explains what belongs to the organization, venue, screen, source, and current user authority.

## Minimum real-product journey

### 1. Organization and venue context

Collect or confirm only enough to establish:

- organization and venue name;
- primary venue industry: Café, Bakery & Dessert;
- venue timezone and local operating context;
- address or public venue context where needed; and
- whether this is a new venue or an existing venue being configured.

Organization industry affects initial defaults only. Venue industry and subtype remain non-commercial configuration.

### 2. Subtype selection

Offer the approved subtypes with neutral descriptions:

- Café;
- Coffee Shop;
- Tea Shop;
- Bakery;
- Patisserie;
- Bakery-Café;
- Dessert Shop;
- Frozen Dessert Shop;
- Juice & Smoothie Bar; and
- Unspecified / General Café.

Selection changes terminology, starter recommendations, suggested products, service contexts, screen purposes, and future dashboard emphasis. It does not unlock features, permissions, limits, or commercial access.

Users may choose a neutral fallback and refine later. A subtype change previews effects while preserving authored content, screens, state, sources, permissions, history, tier access, add-ons, and limits.

### 3. Simple service model

Ask only the questions needed to seed useful defaults:

- counter, table, or mixed service;
- walk-in, preorder, pickup, or a combination;
- common public service contexts such as morning, bakery opening, coffee service, lunch, dessert, late night, or pickup; and
- whether products rotate or frequently sell out.

Drive-through, delivery pickup, event service, complex venue hierarchy, production details, and advanced operations can be deferred.

### 4. Hours and current operating information

Collect regular public hours or allow a “set later” path. Explain that temporary changes and current service state can be updated independently.

Optional prompts may capture:

- service-period names and broad times;
- preorder or custom-order availability;
- pickup location and public instructions; and
- current closure or changed-hours information.

Recurring schedules, conflict detection, exception calendars, and automation are deferred optional workflow.

### 5. First screen purpose and pairing

Ask what the first screen should do, using task-oriented purposes such as:

- beverage or food menu;
- bakery case or today’s selection;
- flavors, products, batches, or current availability;
- seasonal or promotional content;
- preorder or pickup guidance;
- queue, counter, or service instructions; or
- venue information and wayfinding.

Pair a new screen or select an existing valid screen. Clearly separate:

- pairing state;
- online/offline state;
- intended screen purpose;
- latest intended revision; and
- latest confirmed delivery.

A screen being online does not prove it displays the intended content.

### 6. Subtype-aware starter content

Generate or offer an editable starter structure, never fake operational facts.

Examples:

- Coffee Shop: drinks, sizes/options, seasonal drinks, pastries, pickup information;
- Bakery: today’s selection, breads or pastries, batch/return message placeholders, preorder and pickup;
- Tea Shop: tea styles, temperatures, sweetness, toppings or add-ins;
- Dessert or Frozen Dessert: flavors, portions, sizes, vessels, toppings, limited-state presentation;
- Juice & Smoothie Bar: bases, sizes, ingredients, add-ins, bowls, availability, pickup;
- Bakery-Café: bakery case, beverages, food periods, counter/pickup, optional table-service content.

Starter content is visibly sample or draft content until reviewed. It may not invent prices, availability, freshness, quantity, safety, readiness, expected return, or customer-specific facts.

Users may start blank, import later, copy approved organization content, or use starter recommendations selectively.

### 7. One useful live update

Guide the user through one real core action, such as:

- add or edit one product;
- mark an item sold out;
- set a known next-batch or expected-return message;
- update pickup instructions;
- publish changed hours; or
- restore an item to available.

The action uses explicit verb-object labels and shows the venue, service context, affected product/message, target screens, and resulting public wording.

### 8. Preview, publish, confirm

Before publication show:

- intended content and operational state;
- venue, screen purpose, and selected target;
- effective context or time;
- source and freshness where represented; and
- any unknown or unsupported claim that needs removal.

After publication show per-target pending, success, partial, failed, canceled, outdated, or unknown delivery state. The user can retry, correct, supersede, undo, or restore without restarting setup.

### 9. Completion and next actions

Completion confirms the first useful outcome and provides task-based next steps:

- add more products or categories;
- configure service periods;
- add another screen;
- review preorder or pickup information;
- invite a teammate;
- explore schedules, campaigns, integrations, or managed services; or
- view subscription options.

The user enters the actual dashboard, not a disconnected onboarding shell.

## Deferred questions

The following should not block first value:

- full venue, counter, room, area, or service-point hierarchy;
- complete menus and product catalog;
- advanced options and variants;
- recurring rotations and schedules;
- campaigns and approvals;
- multi-venue inheritance;
- advanced language workflow;
- analytics, exports, and retention preferences;
- POS, inventory, production, ordering, payment, fulfillment, loyalty, or other integrations;
- managed hardware, connectivity, monitoring, or support;
- AI configuration; and
- detailed subscription, limit, or add-on decisions not needed for the selected core path.

Deferred items remain discoverable from contextual next steps and setup progress, not as an intimidating checklist.

## Pricing and optional-capability introduction

Pricing and plan information remain directly accessible. The flow should not hide pricing or force a tier comparison before the user understands the included core path.

Preferred introduction:

- do not interrupt venue identity, screen pairing, starter content, or first publication;
- reach first-screen activation before prominent upgrade prompts;
- introduce optional capabilities in context, by outcome;
- preserve the current task and saved work when the user opens pricing; and
- distinguish tier capability, independent add-on, permission, setup, integration, limit, and rollout conditions.

No essential update, targeting, publication, confirmation, or recovery action may require an upgrade.

## Save, resume, and ownership

Every meaningful step creates a durable checkpoint. Resume shows:

- organization and venue selected;
- subtype and service model;
- hours and public information completed or deferred;
- screen pairing and purpose;
- starter content state;
- saved but unpublished changes;
- publication and delivery state;
- current permission; and
- the safest next action.

The system prevents duplicate venue or screen creation and identifies work already completed by another authorized user. Concurrent edits preserve both versions or require explicit reconciliation.

## Role-aware behavior

- Owners and organization administrators can establish organization and venue configuration within their authority.
- Venue managers can configure assigned venues and screens.
- Content operators can complete allowed content and publication tasks without seeing restricted commercial or organization administration.
- Invited users understand why a step is unavailable and who can complete it.

Permission denial is never presented as an upgrade requirement.

## Recovery paths

The experience covers:

- lost or expired invitations;
- existing organization or venue detection;
- invalid or duplicate venue names;
- pairing code expiration, invalid code, already paired screen, and screen ownership conflict;
- screen offline, online but outdated, unknown delivery, and partial delivery;
- starter-content failure or sample-content rejection;
- validation, save, concurrent edit, permission, source, and publication failure;
- stale or disconnected external source;
- browser close, device change, and session expiration; and
- correction, undo, restore, and safe restart from the latest checkpoint.

Failure never silently discards authored content or changes publication state.

## Accessibility and environmental planning

The journey supports:

- keyboard and assistive-technology operation;
- visible labels, focus, instructions, errors, and success feedback;
- 200% zoom and responsive reflow;
- phone and desktop;
- long names and localization expansion;
- non-color status;
- reduced motion;
- low light, glare, crowded counters, and interrupted operation;
- large touch targets and task-focused steps; and
- save/resume without time pressure.

## Impeccable onboarding brief

Mode is **Operate** with an `onboard` focus.

- Teach through real product actions, not a separate tutorial simulation.
- Keep one dominant action per step and show progress without implying every deferred item is required.
- Use subtype-relevant language but preserve neutral fallback.
- Prefer a concrete first useful screen over abstract feature explanation.
- Celebrate verified delivery, not merely account creation or a successful API request.
- Immediately reveal the recovery path when delivery is incomplete.

## Validation

This journey covers onboarding questions, subtype selection, starter products and content, service model, hours, preorder and pickup prompts, defaults, deferred questions, pricing/add-on introduction, accessibility, resume, roles, pairing, publication, confirmation, and recovery. It reaches a real included-core outcome without integrations, upgrades, or complete configuration. RWP-13.06 remains paused.