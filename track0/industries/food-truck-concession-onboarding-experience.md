# Food Truck & Concession Onboarding Experience

## Purpose

This document defines the planning contract for onboarding a Food Truck & Concession customer from first entry through a useful first published screen. It is documentation only and does not authorize implementation. RWP-13.06 remains paused.

The experience must make the operator productive before introducing optional pricing, add-ons, advanced scheduling, or integrations. Industry and subtype choose recommendations; they do not grant access.

## Onboarding outcome

A new operator should finish with:

- the correct organization and local operation selected or created;
- a meaningful Food Truck & Concession subtype or a neutral fallback;
- a starter menu/content set they can immediately recognize and edit;
- the current operating location, event, host, or service context represented when known;
- at least one paired or clearly deferred screen;
- an explicit preview and publish target;
- understandable publication, offline, outdated, or recovery state;
- a resumable next action rather than an unfinished dead end.

## Experience principles

1. Ask only what is needed for a useful first screen.
2. Explain questions in operational language, not classification or entitlement language.
3. Allow uncertain information to remain unspecified.
4. Do not force a permanent address, stable weekly schedule, external integration, or final tier decision.
5. Preserve customer-entered data across back, resume, retry, and subtype changes.
6. Keep optional purchases and integrations secondary until the operator has one screen/content path established.
7. Never imply successful delivery without target-level confirmation.
8. Make mobile use, intermittent connectivity, outdoor readability, keyboard access, 200% zoom, assistive technology, and non-color-only states part of the base design.

## Recommended onboarding sequence

### 1. Confirm organization and authority

Show the organization being configured and the user's current authority. Support:

- creating a new organization when authorized;
- joining or selecting an existing organization;
- selecting the intended operation when several exist;
- explaining view-only, edit, screen-management, publish, and administrative restrictions;
- safe recovery when an invitation, identity, or permission check is incomplete.

Do not ask the operator to solve commercial or ownership questions that an administrator must resolve.

### 2. Select the industry

The first industry question should identify **Food Truck & Concession** as the operating profile when mobility, temporary placement, event participation, or concession service controls the guest experience.

The page should briefly explain inherited outcomes: menu management, rapid updates, accurate location/event communication, screen publishing, delivery confidence, and offline recovery. Industry selection must not show or change a tier.

### 3. Select a primary subtype

Offer the approved primary subtypes:

- Food Truck;
- Food Trailer;
- Food Cart;
- Kiosk;
- Stadium / Arena Concession;
- Festival Vendor;
- Market Stall;
- Pop-Up;
- Catering Concession;
- Unspecified / General Mobile or Concession Operation.

Use short operational examples and allow search or comparison when useful. Ask which model best controls day-to-day setup, service, and guest communication, not what appears on a permit or legal registration.

When physical form and operating context conflict, allow the primary subtype plus descriptive traits. A change of subtype updates recommendations only and must not delete content, reset permissions, alter commercial access, or reassign screens.

### 4. Name the operation and service point

Collect a customer-facing operation name and, when relevant, a stand, stall, kiosk, window, station, or unit name. Show examples based on subtype without forcing generated names.

Support long names, punctuation, multilingual content, and a clear distinction between organization name and local operation/service-point name.

### 5. Establish the current operating context

Ask the smallest useful question for the selected subtype:

- current stop, pitch, market, venue, event, host, stand, gate, section, zone, or temporary location;
- today's service period or hours when known;
- planned, setup, ready, open, limited, paused, relocating, closed, or canceled state where applicable;
- optional last-order, pickup, queue, or service-window guidance.

Allow “not set yet” and explain that location/event context can be updated quickly later. Do not imply live tracking, external synchronization, or a permanent address.

### 6. Create starter menu and content

Offer a starter path that is useful but reversible:

- begin from a short subtype-aware starter menu;
- import or copy existing content where a supported source is already available;
- start blank;
- copy from another authorized operation;
- postpone detailed content and use a simple opening/coming-soon screen.

Starter content should include realistic placeholders for categories, items, combos, prices, availability, location/event identity, service hours, pickup/queue guidance, and a closure or sold-out example. It must be visibly sample content and never publish automatically.

The operator should be able to remove, rename, reorder, and replace everything. The starter menu is not a locked template, tier, or integration.

### 7. Review rapid operating controls

Before first publication, briefly demonstrate the required core controls most likely to matter during service:

- Quick Update;
- item and combo sold-out/unavailable changes;
- operation open, paused, limited, relocated, closed, and serving-again state;
- current location or event update;
- queue, pickup, lane, window, or last-order guidance;
- target selection, preview, publish confirmation, retry, and restore.

Use an optional guided checklist, not a long product tour. The operator may skip and return later.

### 8. Pair or defer a screen

Provide clear choices:

- pair a screen now;
- select an already authorized screen;
- continue with content and pair later.

Pairing should explain the device code/identity, intended operation, physical placement or screen purpose, and expected connectivity. Show wrong-code, expired-code, already-paired, permission-denied, offline, unsupported, and retry states.

Deferring pairing must not discard content. The dashboard should retain a clear next action.

### 9. Select the publish target and preview

Require explicit target selection. Show:

- operation and service-point scope;
- current location/event context;
- selected content/menu;
- intended screens;
- language and accessibility coverage;
- any sample or incomplete content;
- offline, outdated, unknown, or permission state;
- the exact action that will publish.

High-scope or mixed-location publication requires confirmation. Never preselect unrelated screens merely because they belong to the same organization.

### 10. Publish and confirm

After publishing, show per-target state:

- accepted/pending;
- delivered;
- offline;
- outdated;
- failed;
- unknown;
- partial success.

Show the last successful version/time when known and provide retry, correct, restore, or continue-without-screen actions. A successful API request alone is not proof that every screen received the content.

### 11. Introduce optional plan outcomes and add-ons

Only after the operator has a useful content and screen path should onboarding introduce optional outcomes. Explain them by need:

- plan recurring routes/events;
- coordinate several units or stands;
- publish public location pages;
- schedule campaigns;
- gain advanced analytics;
- connect POS, ordering, inventory, venue, weather, maps, messaging, queue, or footfall data;
- use AI assistance;
- obtain managed hardware, connectivity, installation, or support.

Show what remains available without purchase. Distinguish tier access, independent add-on, quantity limit, permission, unconfigured integration, and unsupported provider/region. Do not force a purchase to finish onboarding.

## Deferred questions

The following should normally be deferred until the operator has a working first screen or encounters the relevant need:

- full route and event calendar;
- organization-wide inheritance policies;
- detailed roles and approval workflows;
- all service points, units, stands, windows, and screens;
- integrations and credentials;
- advanced analytics and retention;
- AI preferences and usage controls;
- public location pages and notifications;
- managed hardware/support contracts;
- final tier or add-on purchase decisions;
- detailed downgrade and over-limit resolution.

A deferred question must have a discoverable home and must not block core operation.

## Resume and recovery

Onboarding state should be saved after meaningful steps. Resume must show:

- completed steps;
- unresolved required items;
- optional deferred items;
- current draft content;
- current operation/location/event context;
- pairing and target status;
- last publish attempt and result;
- the safest next action.

Recovery states include:

- network interruption;
- expired session or identity recheck;
- invitation or permission change;
- stale or conflicting operation data;
- screen pairing failure;
- failed or partial publication;
- invalid sample content;
- source import failure;
- duplicate operation or screen;
- plan/limit restriction;
- unsupported region/provider/hardware;
- user exiting before completion.

Do not discard valid work when one step fails. Provide retry, edit, skip when safe, save and exit, or contact-admin actions.

## Accessibility and environmental requirements

The onboarding experience must support:

- phone, tablet, and desktop widths;
- keyboard-only use and visible focus;
- screen-reader labels and logical heading order;
- 200% zoom and text reflow;
- long names and localization expansion;
- right-to-left readiness;
- non-color-only state indicators;
- high contrast and outdoor/glare readability;
- reduced motion;
- explicit error summaries and field-level guidance;
- no time-limited step without extension or recovery.

The project-local Impeccable `shape` and `harden` guidance applies: one dominant task per step, explicit hierarchy, minimal first-run complexity, clear states, safe confirmation, and actionable recovery while preserving the approved Sky Blue administrative direction.

## Completion definition

Onboarding is complete when the operator has either:

1. published verified content to at least one intended screen; or
2. created a valid operation and content draft, deliberately deferred screen pairing/publishing, and received an exact next action.

Pricing selection, integrations, a complete route calendar, all organization users, and every optional setup item are not prerequisites for completion.

## Validation

The planned experience covers subtype selection, mobile/temporary setup, starter menu/content, locations/events, connectivity, deferred questions, later pricing/add-on introduction, accessibility, resume, and recovery. It preserves essential core operation, requires explicit targeting and delivery confidence, and authorizes no implementation.
