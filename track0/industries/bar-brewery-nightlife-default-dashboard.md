# Bar, Brewery & Nightlife Default Dashboard

## Authority and scope

This document defines the non-implementation default-dashboard contract for Bar, Brewery & Nightlife under RWP-00.24. It uses the merged industry records through RWP-00.23 and the Restaurant baseline. It does not authorize UI, API, schema, billing, entitlement, analytics, or product implementation.

The dashboard is an **Operate** surface: it prioritizes what the venue must know or change now. It must keep core daily actions immediately available while clearly separating represented state, permission, tier access, add-ons, limits, connection state, and rollout controls.

## Primary operator outcomes

The default dashboard must help an authorized operator:

1. understand whether current public content is accurate and delivered;
2. correct availability, hours, specials, events, entry guidance, or venue state quickly;
3. see what requires attention before or during the current service period;
4. select the right venue, area, screen purpose, and target before publishing;
5. recover from offline, outdated, failed, partial, stale-source, or conflicting states;
6. prepare the next service period, event, release, happy hour, or campaign without obscuring current operation;
7. recognize optional outcomes without losing the included manual path.

## Information hierarchy

### 1. Venue and current-service header

The header identifies:

- organization and venue;
- venue-local date and time;
- current operating day and service period, including cross-midnight context;
- primary subtype and optional descriptive traits;
- current venue state such as open, limited, paused, closed, private event, or unknown where represented;
- current user role and meaningful authority restrictions;
- a clear venue switcher when the user may access several venues.

Local time is never inferred from the user device when venue time is authoritative. A cross-midnight service period must remain associated with the correct operating day.

### 2. Needs attention

An exception-first section appears above routine content when action is required. It may include:

- screens offline or outdated;
- failed or partial publication;
- unknown delivery state;
- stale or conflicting external source;
- content scheduled to expire without replacement;
- current content with incomplete target coverage;
- hours, event, entry, or responsible wording requiring review;
- sample or draft content still visible in an active configuration;
- disconnected add-on with manual fallback available;
- limit reached where an intended action cannot proceed;
- permission restriction preventing the current user from resolving an issue.

Each exception states the affected scope, last known successful state, urgency, and next action. Status is not communicated through color alone.

### 3. Quick actions

The highest-priority included actions remain visible without opening a feature catalog:

- **Quick Update availability**;
- **Update taps or menu items**;
- **Update hours / current service**;
- **Create or update a special / happy hour / release**;
- **Create or update an event**;
- **Update doors / entry / venue guidance**;
- **Preview and publish**;
- **View screens and delivery**;
- **Retry, correct, supersede, or restore** when an exception exists.

Labels use a clear verb and object. Actions unavailable because of permission, limit, purchase, configuration, connection, or rollout state must present those reasons distinctly. Urgent included manual paths must never be hidden behind upgrade messaging.

### 4. Now / Today

The current-operation section summarizes:

- active drink, tap, cocktail, wine, optional food, and specials content;
- current unavailable, sold-out, limited, or recently changed items;
- active happy hours, releases, tastings, game-day offers, and effective periods;
- current venue, bar, kitchen, doors, event, last-entry, and locally authored last-call times;
- current events, sports fixtures, lineup, entertainment, or private functions;
- current public entry, cover, reservation-information, area, or responsible guidance;
- current screen targets and published version.

The section emphasizes exceptions and recently changed values rather than displaying every item by default. It must support a compact mobile view and a richer desktop drill-in.

### 5. Screens and publishing health

The dashboard shows target-level status rather than a single misleading success badge:

- accepted or pending;
- delivered;
- offline;
- outdated;
- failed;
- partial;
- unknown;
- last successful version and time where known.

Summaries group by venue, area, screen purpose, and current content. Operators can inspect affected targets, retry safely, correct content, restore a prior version, or continue with unaffected targets. Managed monitoring is optional; basic delivery confidence and recovery remain core.

### 6. Upcoming work

A forward-looking section shows the next relevant transitions:

- opening, kitchen, bar, doors, event, last-entry, and closing changes;
- scheduled happy hours, specials, releases, tastings, sports fixtures, events, or private functions;
- content expiry and replacement;
- planned target or screen changes;
- draft items awaiting completion or permission;
- connected-source changes requiring review.

The section uses venue-local time and clearly distinguishes a manual one-off change from advanced recurrence, campaign, or approval workflow.

### 7. Recommendations

Recommendations are subtype- and state-aware, not entitlement assumptions. Examples:

- complete missing tap availability before opening;
- add a cancellation or delay update to event screens;
- pair the deferred first screen;
- add an accessible alternative for a dense menu layout;
- review stale imported items while retaining manual override;
- create a reusable schedule only when advanced scheduling is available;
- consider an optional connection only after explaining the included manual path.

Recommendations may explain an optional customer outcome and show plan/add-on discovery, but must not interrupt urgent work or present industry selection as a commercial gate.

## Dashboard content groups

### Content and availability

Show current menu/list coverage, recent Quick Updates, unavailable or sold-out counts, and the age/source of important values. Product values are state; editing is core; edit authority is permission; item/list counts may be limits; automated synchronization is an add-on.

### Hours and service periods

Show venue-local current and next transitions, cross-midnight periods, exceptions, and incomplete schedule state. Manual current hours and one-off changes are core. Advanced recurrence or conflict automation may be tiered.

### Specials, releases, and promotions

Show active and upcoming public offers with effective periods and target scope. Manual creation and publication are core. Campaign orchestration and advanced reusable scheduling are optional.

### Events and entertainment

Show event identity, doors, timing, venue area, current state, public entry guidance, and affected screens. Delay, cancellation, relocation, pause, and resumption must be easy to communicate. External event, sports, ticketing, guest-list, or access data is an add-on and must remain source- and privacy-aware.

### Reservations and private functions

Show only general, privacy-safe operational information unless a separately authorized capability supplies restricted transaction state. The dashboard must not expose names, payment state, eligibility, ticket identity, or guest-list membership on a public or broadly accessible surface.

### Screens and recovery

Show online/offline/outdated/delivery state, affected content, last-known-good version, and actionable recovery. Basic screen management and recovery are included. Device count is a limit; managed hardware/monitoring is an add-on.

## Subtype-aware emphasis

- **Bar / Pub:** current menus, specials, kitchen/bar periods, events, and general entry information.
- **Brewery / Brewpub:** house products, releases, taps, flights, tasting/tour events, production/taproom distinction, optional food.
- **Taproom:** high-frequency tap availability, flights, releases, tasting events, and rapid correction.
- **Cocktail Bar:** cocktail lists, specials, table/lounge service, reservations information, and current hours.
- **Wine Bar:** glass/bottle availability, flights, tastings, pairings, reservations information, and limited products.
- **Sports Bar:** current/upcoming fixtures, viewing zones, game-day offers, schedule changes, and area targeting.
- **Nightclub:** doors, lineup, cover/entry guidance, areas, private events, late-night periods, and event-state changes.
- **Lounge:** table/lounge service, reservations information, entertainment, specials, and private areas.
- **Music / Entertainment Bar:** lineup, doors, stage/area, event timing, delay/cancellation, entry guidance, and target coverage.
- **Unspecified:** neutral content, availability, hours, event, screen, and delivery priorities.

Subtype changes emphasis only. It never removes core actions or grants commercial access.

## Role-aware presentation

The dashboard may tailor action prominence to authority while preserving shared status visibility:

- viewers see current state and understandable restrictions;
- editors see content and operating-state actions within scope;
- publishers see target, preview, publish, correction, and restoration actions;
- screen managers see pairing, purpose, health, and recovery;
- administrators see organization/venue configuration and optional commercial context.

A missing permission is not described as an upgrade. Commercial access is not described as authority. Restricted operational information remains audience-scoped.

## Optional and locked-capability presentation

Optional capabilities should normally appear through recommendations, contextual actions, or a secondary discovery area—not a grid of disabled controls. Presentation must distinguish:

- included and ready;
- included but not permitted;
- included but not configured;
- upgrade required;
- independent add-on required;
- connection disconnected or stale;
- limit reached;
- unsupported provider, device, region, or source;
- rollout-disabled.

Every optional message explains the customer outcome, what remains possible manually, prerequisites, and the next safe action.

## Mobile and desktop priorities

### Mobile

Prioritize:

1. needs attention;
2. Quick Update and current-service actions;
3. publish/delivery exceptions;
4. current event or special;
5. screen recovery;
6. concise upcoming transitions.

Use single-column cards, persistent venue context, large touch targets, minimal data entry, and safe confirmation for high-scope publication.

### Desktop

Support the same hierarchy with wider comparison, multi-column summaries, filters, target inspection, richer upcoming work, and optional analytics entry points. Desktop must not hide urgent actions in dense tables.

## Essential states

The dashboard requires designed states for:

- first use and no venue;
- no content;
- no screen paired;
- content ready but publication deferred;
- loading and delayed source;
- permission restricted;
- partial data;
- offline/outdated screen;
- failed/partial publication;
- stale/conflicting connection;
- no current service period;
- no event or special;
- limit reached;
- upgrade/add-on discovery;
- success, undo, correction, and restoration.

No state may produce a dead end when a safe included next action exists.

## Accessibility and environment

Support keyboard-only operation, visible focus, logical headings, screen-reader names matching visible labels, 200% zoom and reflow, long names, localization expansion, right-to-left readiness, non-color status, high contrast, reduced motion, touch use, low-light conditions, glare, and intermittent connectivity. Time and status updates must not move focus unexpectedly.

Project-local Impeccable `shape`, `clarify`, and `harden` guidance applies: exception-first hierarchy, one dominant action per card, realistic data, explicit state language, restrained visual treatment, accessible feedback, and recoverable errors.

## Boundaries and handoff

Documentation and planning only. No dashboard UI, API, schema, migration, analytics, billing, entitlement, integration, player, or hardware implementation.

RWP-00.25 owns the Bar, Brewery & Nightlife KPI and analytics catalog.