# Hospitality Onboarding Experience

## Authority and scope

This document defines the Hospitality onboarding experience for RWP-00.59. It applies the approved Hospitality industry, subtype, terminology, operating, capability, classification, and tier-mapping records.

It is planning only. It does not implement onboarding, billing, entitlements, screens, pairing, UI, API, schema, integrations, analytics, or RWP-13.06.

## Intended first outcome

A new Hospitality customer should reach one useful active screen without being required to choose a final subscription tier, connect an external system, model the full property, configure every language or object, or answer questions that can be deferred safely.

The first-success path is:

1. identify the property and subtype;
2. enter minimal public property context;
3. select starter objects and content;
4. select or pair one screen;
5. preview the exact public result;
6. publish and confirm delivery;
7. land on a task-first starter menu.

Pricing and upgrade prompts do not interrupt this path. The accepted direction is to introduce pricing context after at least one screen is successfully active, subject to later commercial approval.

## Experience principles

- Ask only what is needed for the next visible outcome.
- Explain why a question matters and whether it can be changed later.
- Use subtype-aware Hospitality language with neutral fallbacks.
- Preserve customer-authored names.
- Save after every meaningful step and support safe exit and resume.
- Provide “skip for now” when omission will not make public content misleading.
- Keep manual operation available without external integrations.
- Confirm scope, wording, language, targets, and effective time before publication.
- Treat delivery confirmation and recovery as part of onboarding success.

## Planned stages

### 1. Welcome and resume

Show the goal: set up the property and publish the first guest screen.

Confirm:

- organization and whether this is the first or an additional property;
- property time zone and source language;
- user authority to configure and publish;
- any existing invitation, property draft, or incomplete session to resume.

Explain automatic save, resume, and that guest names, room assignments, reservation details, and other private stay information are not needed.

### 2. Industry and subtype

Hospitality is the primary industry. Choose one primary subtype:

- Hotel;
- Resort;
- Motel;
- Hostel;
- Extended-Stay;
- Serviced Apartment;
- Conference Property;
- Casino Resort;
- Boutique Lodging;
- Neutral / mixed property.

Subtype tunes terminology, starter objects, screen-purpose suggestions, and operating prompts. It does not change price, permissions, commercial access, privacy, or limits.

Allow “I’m not sure,” preview the main default differences, and support later change with an impact preview that preserves existing content and screens.

### 3. Property identity and public context

Required minimum:

- customer-facing property name;
- property time zone;
- main guest-facing location or area label;
- source language;
- optional public contact or next-action phrase when useful.

Safe to defer:

- brand, property group, region, detailed building hierarchy, full accommodation terminology, and detailed service points;
- unverified accessibility details;
- advanced governance and integrations.

### 4. Starter operating model

Offer subtype-aware suggestions rather than forcing a complete model.

Potential starter objects:

- reception or front desk;
- dining or breakfast outlet;
- parking or arrival point;
- elevator or route landmark;
- pool, spa, gym, laundry, lounge, shared kitchen, activity desk, meeting registration, or shuttle as relevant;
- one meeting or event destination;
- one general guest-notice area.

For each selected object, collect only public name, location context, known hours, current public state, and a clear next action where needed.

### 5. Amenities, services, and outlets

Use customer-outcome groups:

- Eat and drink;
- Relax and wellness;
- Work and meet;
- Get around;
- Guest services;
- Shared facilities;
- Activities and entertainment.

The customer selects only what should appear on the first screen, may add custom items, and may defer complex hours, access rules, menus, reservations, live availability, and external data.

Embedded venue terminology follows its approved local industry where applicable.

### 6. Meetings and events

Show prominently for event-led properties and keep optional elsewhere.

Starter choices:

- a public event or group directory;
- meeting-space names;
- registration or welcome location;
- today’s or next event list;
- a temporary room or route change;
- no event content on the first screen.

Manual entry remains available. Event-system connection is offered later and never required for the first screen.

### 7. Wayfinding

Ask what the first screen should help guests find:

- reception, entrance, exit, elevator, stairs, parking, transport, amenity, outlet, event, meeting space, or custom destination;
- a clear text direction or landmark;
- optional verified accessible route;
- temporary closure or alternate route.

Manual text and customer-supplied static media are core. Advanced maps and live routing are optional later. Do not infer distance, travel time, current position, or accessibility.

### 8. Guest notices and operating status

Offer editable starter templates for:

- welcome and property information;
- changed hours;
- temporary closure or limited operation;
- relocation or alternate route;
- maintenance or weather effect;
- transport change;
- meeting or event change;
- expected next update;
- no active notices.

The customer sets scope, public audience, priority, source language, effective time, expiration, targets, and next action. Templates never add unsupported facts.

### 9. Languages and accessibility

Required minimum:

- identify the source language;
- preview at 200% zoom;
- confirm readable contrast, text size, and non-color-only meaning;
- allow long names and content expansion;
- show language coverage and missing-language state.

Optional now:

- customer-authored alternate-language variants;
- right-to-left preview;
- language-specific dates, times, units, and terminology;
- later translation workflow or translation/AI add-on.

Missing translations require an explicit fallback.

### 10. Starter content and screen purpose

Offer a subtype-aware starter menu:

- Welcome / property overview;
- Today at the property;
- Amenities and services;
- Dining and outlets;
- Meetings and events;
- Wayfinding / directory;
- Transport and arrival;
- Guest notice / service update;
- Mixed information screen.

The starter menu is a task and template chooser, not a pricing tier.

Draft content uses only customer-provided information. Show content source, missing information, scope, language, effective time, target, and public preview. Allow starting blank.

### 11. First screen selection or pairing

Support selecting an existing screen or pairing a new one, naming it, assigning property/area context, choosing orientation and purpose, and seeing authoritative online/offline and current-content state.

Save progress when interrupted. Do not claim success until pairing and delivery are confirmed. Future implementation should ensure the player enters full-screen operation, avoids scrollbars, updates online state, and receives approved content/theme changes.

### 12. Preview, publish, and confirm

Before publish, show:

- property, screen, public wording, selected objects, destinations, language and fallback;
- source and freshness where applicable;
- effective time and expiration;
- exact targets and excluded targets;
- unknown or unverified information;
- current screen content that will be replaced.

After publish, show accepted, pending, delivered, failed, partial, offline, outdated, or unknown by target; last delivered version and time; and retry, correction, supersession, undo, and restore actions.

Success requires authoritative delivery confirmation.

### 13. First-success landing

After one screen is active, land on a task-first Hospitality starter menu:

- Update guest information;
- Change hours or availability;
- Post a guest notice;
- Update an event or meeting;
- Change wayfinding;
- Check screen health;
- Add another screen;
- Finish property setup;
- Add languages;
- Explore optional integrations or advanced workflows.

Show exceptions, missing setup, stale information, and delivery problems before promotional content.

## Deferred-question model

Label each question:

- required now;
- recommended before first publish;
- safe to defer;
- unavailable until another dependency exists.

Deferred items remain in a prioritized setup checklist showing why they matter, affected scope, current fallback, and next action. Do not show a false complete state.

Safe deferrals include detailed hierarchy, full amenity/outlet inventory, all events, advanced routes, additional languages, property groups, brands, approvals, campaigns, analytics, integrations, enterprise identity, managed hardware, and final tier selection.

## Pricing, tiers, add-ons, and limits

Before the first active screen:

- no mandatory tier-selection wall;
- no disabled-feature grid;
- no upgrade prompt that blocks core setup;
- show only unavoidable account constraints that affect the current step.

After first-screen success, introduce customer outcomes contextually:

- Coordinate teams, events, languages, and content;
- Govern multiple properties and brands;
- Meet enterprise identity, audit, governance, and service needs;
- Connect external or managed add-ons.

Show what remains included, the outcome unlocked, whether the need is a tier/add-on/permission/limit, setup requirements, manual fallback, and “not now.” Industry and subtype never change pricing.

## Integration introduction

Offer connections after manual first-screen success or when an immediate verified source is clearly useful.

A connection explanation includes customer outcome, required data, public versus restricted fields, source authority, freshness, manual fallback, conflict handling, failure, disconnect, cancellation, retention, export, deletion, commercial status, and required permission.

“Connect later” remains available.

## Resume and recovery

Persist completed and deferred stages, customer-authored values, subtype, starter objects, drafts, notices, languages, screen selection, targets, previews, and last successful publication.

On return, show completed work, external changes, stale or conflicting data, screen and current-content state, and the next recommended action.

Recovery covers changed permission, existing or duplicate property, screen already assigned, expired pairing session, offline or unknown screen, failed or partial publish, browser or network interruption, stale source, subtype change, missing language, changed target, and content changed after preview.

Do not force a full restart when safe reconciliation is possible.

## Accessibility and responsive planning

Support keyboard-only use, visible focus, screen-reader labels and announcements, non-color-only state, 200% zoom, long names, localization expansion, right-to-left layouts, local dates and times, reduced motion, phone through large desktop, interruption-safe forms, no drag-only action, and text alternatives for instructional media.

Preserve the approved Sky Blue administrative direction with clear hierarchy and restrained status color.

## Impeccable flow result

The planned flow uses `shape` for the first visible outcome, `clarify` for required versus deferred information, `harden` for interruption and failure, and `polish` only after task hierarchy is correct.

## Boundaries and handoff

No implementation is authorized. RWP-13.06 remains paused. RWP-00.60 owns the default Hospitality dashboard and starter-menu experience after onboarding.