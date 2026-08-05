# Entertainment & Attractions Onboarding Experience

## Authority and goal

This documentation-only companion completes RWP-00.71. It defines the Entertainment & Attractions onboarding journey without implementing UI or resuming RWP-13.06.

**Aha moment:** an authorized operator sees accurate, venue-specific visitor information on the first paired online screen and can confidently update, verify, and restore it.

Onboarding should get a mixed-experience operator to that moment quickly. It must not teach the entire product, force advanced configuration, require an external integration, or interrupt first value with a mandatory tier comparison.

## Onboarding principles

- Use real product setup and real content, not a disconnected tutorial.
- Ask only for information needed to create a viable venue, first screen purpose, first content, and safe publication.
- Provide clear defaults from industry and subtype while showing that they are editable recommendations, not entitlements.
- Support save-and-resume, skip, defer, back, review, and correction.
- Preserve customer-authored content and source authority through every change.
- Keep pricing and optional capability prompts contextual. Prefer showing upgrade choices after first-screen activation; do not force pricing before operational value is understood.
- Never require ticketing, admissions, queue, map, venue, event, sports, translation, identity, AI, or hardware-service integrations for first value.
- Explain why each requested value matters and how it affects public screens.

## Entry paths

### New organization

Use when the customer has no configured organization or venue. Establish minimum organization identity and authority, then create the first venue.

### Existing organization adding a venue

Reuse organization-level defaults only after preview. Ask whether the new venue should inherit, copy, link, or start neutral. Never silently inherit active schedules, notices, sources, audiences, or targets.

### Invited operator

Respect the inviter’s scoped permissions. Show the assigned venue and immediate task. Do not force organization setup the operator cannot change.

### Returning incomplete setup

Resume at the last durable checkpoint, show what is complete, what remains, and whether any previously paired screen or source has changed state.

### Experienced user

Allow skipping guided explanations and proceeding directly through the required fields and review. Guided help remains replayable.

## Minimum journey

### Step 1 — Venue identity and context

Collect only:

- venue name;
- local time zone;
- country/locale where needed for date, time, and language defaults;
- Entertainment & Attractions industry confirmation;
- one primary subtype or neutral fallback;
- optional descriptive traits only when useful.

Explain that subtype affects terminology, starter suggestions, and screen-purpose recommendations, not price or access.

Required states:

- first use;
- prefilled organization default;
- long or duplicate name;
- unsupported or ambiguous subtype;
- no permission;
- save failure;
- resume after interruption.

### Step 2 — Simple venue structure

Ask for the smallest structure needed for the first screen:

- venue-wide only; or
- one relevant area, attraction, exhibit, auditorium, stage, gate, zone, route, session location, or other subtype-appropriate context.

Do not force complete campus modeling. Offer “Add more later.” Show examples appropriate to the chosen subtype.

If importing or inheriting structure is available later, present it as optional and show source, freshness, authority, and impact before adoption.

### Step 3 — First screen purpose

Ask what visitors should learn from the first screen. Recommended purposes may include:

- today / now / next;
- showtimes or performance schedule;
- exhibit or attraction availability;
- queue, wait, capacity, or admission guidance;
- entrance, gate, section, auditorium, gallery, habitat, route, or wayfinding;
- closure, delay, relocation, cancellation, or reopening notice;
- visitor welcome and venue information;
- event, campaign, membership, sponsor, retail, merchandise, food-and-beverage, or service information.

Essential operational purposes must not be hidden behind an upgrade. Optional purpose suggestions may be shown contextually but must identify prerequisites and preserve a core alternative.

### Step 4 — Pair or select the first screen

The onboarding plan must support:

- new player pairing;
- selecting an already paired unassigned screen;
- identifying venue, area, purpose, orientation, and display context;
- online, connecting, offline, outdated, incompatible, permission, and pairing-failure states;
- automatic status refresh after successful pairing;
- fullscreen player expectation with no visible scroll bars;
- clear recovery, retry, replacement, and “finish content first” paths.

The screen should not be considered active merely because a code was entered. Activation requires confirmed pairing, target assignment, online or clearly understood offline state, and a successful content delivery or an explicit deferred state.

### Step 5 — Create starter content

Offer a small subtype-aware starter set, not a large template gallery. Examples:

- Cinema: film/showtime, auditorium, format/accessibility, sold-out/delay state.
- Theater or live venue: production/event, doors/start, stage/room, access guidance.
- Museum or gallery: exhibition, gallery, operating window, temporary closure, program.
- Zoo/aquarium/park: attraction or habitat availability, talk/show schedule, route, weather/closure.
- Sports venue: event, start, gate/section, transport, venue state.
- Family entertainment, arcade, or bowling: activity/session/lane information, check-in, capacity, queue.
- Attraction/tour: departure or entry window, route, language, weather, last entry.

Starter content must:

- use realistic sample values clearly marked as examples until replaced;
- never invent safety, accessibility, admission, capacity, wait, reopening, legal, rights, sponsor, or source facts;
- expose public wording and operator-only source/freshness context separately;
- support blank start, starter selection, and later template discovery;
- preserve the customer’s edits when subtype or starter choices change.

### Step 6 — Add one useful live update

Teach the core operating model through one real action, such as:

- add or edit today’s occurrence;
- mark an attraction or exhibit temporarily unavailable;
- publish a closure, delay, relocation, or reopening notice;
- add manual queue, wait, capacity, admission, or last-entry guidance;
- add a destination or temporary route;
- add a manually authored alternate language.

Show scope, effective time, source, public wording, target screen, and restoration path. Do not teach all capabilities.

### Step 7 — Preview and publish

Before first publication, show:

- venue and area scope;
- selected screen and purpose;
- public content and language;
- effective date/time in venue local time;
- source and freshness where relevant;
- accessibility and distance-readability checks;
- whether sample placeholders remain;
- expected result and restoration point.

Require confirmation for high-scope notices or venue-wide targets. Publish authority remains permission.

### Step 8 — Confirm first value

Success requires:

- publish accepted;
- intended target visible;
- online/offline/outdated state visible;
- confirmed or clearly pending/partial/failed delivery;
- last-known-good content and retry/recovery path available;
- a concise next action.

Celebrate lightly. The primary message is operational confidence, not completion of every setup option.

## Deferred setup after first value

Contextual follow-up may invite the customer to:

- add more areas, attractions, exhibits, events, sessions, queues, routes, gates, or screens;
- configure recurring schedules, additional screen purposes, templates, brand assets, or language variants;
- invite staff and assign permissions;
- define organization or venue-group inheritance;
- explore advanced maps, coordinated screens, campaigns, approvals, localization, analytics, enterprise administration, or managed hardware;
- connect ticketing, admissions, access, venue, cinema, queue, footfall, map, event, sports, translation, AI, identity, or other sources;
- review tier and add-on options after enough context exists.

Every optional prompt must state what remains possible manually.

## Pricing and upgrade timing

- Do not force pricing before the customer has a configured venue and a viable first-screen path.
- Prefer contextual upgrade education after the first screen is active.
- Allow deliberate access to pricing from account or plan navigation without hiding it completely.
- Explain the outcome, prerequisites, venue scope, limits, outage behavior, downgrade behavior, and manual fallback.
- Distinguish not purchased, not permitted, not configured, disconnected, stale, unsupported, limit reached, and internally disabled.
- Never use subtype to imply a required plan.

## Save, resume, skip, and recovery

Every major step should create a durable checkpoint. On return, show:

- completed steps;
- incomplete required values;
- deferred optional values;
- paired-screen state;
- unpublished drafts;
- source or delivery changes since last visit;
- exact next action.

A user may skip guidance, not required safe-publication data. Skipping onboarding does not mark screens active or publish examples automatically.

If publication or pairing fails, preserve the draft, target selection, source context, and recovery instructions. Never require restarting setup.

## Permissions and role-aware behavior

- Organization administrators may create venues and assign authority.
- Venue administrators may configure permitted local structure, screens, roles, and sources.
- Content operators may be routed directly to content, targeting, preview, and publication within their scope.
- Approvers see pending review only when approval workflow is configured.
- Users without required authority receive a clear explanation and request-access path; disabled controls alone are insufficient.

Commercial access and permission remain separate.

## Accessibility, responsiveness, and environmental constraints

- Full keyboard and assistive-technology operation.
- Persistent labels, explicit error association, logical focus order, and recoverable validation.
- 200% zoom, localization expansion, long venue and event names, pluralization, and right-to-left readiness.
- Phone through large desktop support.
- Low-light and crowded-workspace clarity without color-only meaning or uncontrolled motion.
- Pairing instructions usable while moving between a workstation and physical display.
- No time-limited step without extension or recovery.

## Required onboarding states

- first use;
- returning incomplete;
- invited user;
- experienced skip;
- empty and sample-data states;
- loading and save-in-progress;
- validation and duplicate values;
- permission denied;
- pairing pending, successful, failed, offline, outdated, incompatible, replaced;
- draft, scheduled, publish pending, successful, partial, failed;
- sample content still present;
- source stale, disconnected, conflict, override;
- limit reached;
- optional capability not purchased, not configured, or unavailable;
- resume, retry, undo, correction, and restoration.

## Success measures for later implementation

- time from account entry to first confirmed screen delivery;
- completion and skip rate by role and entry path;
- drop-off by required step;
- pairing success and recovery rate;
- percentage reaching first publish without external integration;
- rate of sample placeholders removed before publication;
- first-publish failure and correction rate;
- time to second meaningful update;
- accessibility and mobile completion success;
- upgrade exploration after first value rather than before it.

These measures are definitions only; RWP-00.73 owns the industry KPI and analytics model.

## Impeccable `onboard` result

The flow teaches one real operating loop—define a venue, pair a screen, create accurate content, preview scope, publish, verify, and recover—rather than touring features. Guidance is concise, dismissible, replayable, and progressive. Empty states provide a concrete first action and starter option. Experienced users may skip explanation. Optional capability discovery happens at the point of need. The visual direction remains the approved Sky Blue administrative world.

## Owner decisions carried forward

- exact signup-to-venue ownership flow;
- whether pairing may occur before or after first content creation;
- default starter set and sample-data policy;
- minimum required venue hierarchy;
- pricing access and first-screen activation threshold;
- trial behavior and upgrade prompts;
- screen/player hardware fulfillment and managed-service paths;
- invite, approval, and administrator recovery;
- data retention for abandoned onboarding;
- accessibility, safety, privacy, rights, and legal review requirements.

## Boundaries

This document does not resume or implement RWP-13.06, onboarding UI, signup, pricing, billing, entitlements, player behavior, pairing behavior, templates, publication, permissions, integrations, or analytics. RWP-00.72 owns the default Entertainment & Attractions dashboard plan.
