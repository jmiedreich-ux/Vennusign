# Bar, Brewery & Nightlife Validation, Review & Handoff

## Validation scope

This document completes RWP-00.26 by reviewing RWP-00.15 through RWP-00.25 as one coherent Bar, Brewery & Nightlife Track 0 profile. Restaurant remains the canonical baseline. Only meaningful Bar-specific differences are retained.

This validation does not authorize product, UI, API, schema, billing, entitlement, analytics, integration, AI, player, hardware, consolidation, or later-phase implementation. RWP-13.06 and Phase 14+ remain paused.

## Final result

The Bar, Brewery & Nightlife profile is **complete, internally consistent, and ready for owner review and later cross-industry consolidation**.

No blocking classification collision, subtype-entitlement coupling, permission-entitlement confusion, duplicate commercial baseline, or silent implementation authorization remains. Essential daily operation remains core. Remaining questions are owner-level packaging, limits, commercial policy, legal/policy ownership, integrations, privacy, data, and implementation decisions—not missing industry-profile requirements.

The Bar stream is complete through RWP-00.26. It must not start product implementation or consolidation. Consolidation remains gated until RWP-00.26, RWP-00.38, RWP-00.50, RWP-00.62, and RWP-00.74 are all merged and verified.

## RWP package reviewed

| RWP | Subject | Validation result |
| --- | --- | --- |
| RWP-00.15 | Industry definition | Pass — bounded delta from Restaurant; beverage-led, late-service, event, entry, and nightlife context is explicit without making industry an entitlement. |
| RWP-00.16 | Venue subtypes | Pass — nine bounded primary subtypes, a neutral fallback, and optional traits resolve hybrids without stacking capabilities. |
| RWP-00.17 | Business terminology | Pass — operator/public terminology is consistent, customer-authored language is preserved, and state/action terms remain distinct. |
| RWP-00.18 | Operating characteristics | Pass — cross-midnight periods, rapid availability, service models, event/entry context, responsible wording, and subtype rhythms are defined without inventing law or policy. |
| RWP-00.19 | Required capabilities | Pass — the smallest viable manual daily-operation baseline is complete and cannot be premium-gated or replaced by integration. |
| RWP-00.20 | Optional capabilities | Pass — advanced native workflows, external systems, managed services, cost drivers, fallback, downgrade, cancellation, and failure behavior are separated. |
| RWP-00.21 | Capability classification | Pass — every inventoried concern has one primary Track 0 classification; secondary relationships do not replace it. |
| RWP-00.22 | Subscription tier proposal | Pass as proposal — Operate Today, Plan & Promote, and Scale & Govern are coherent working outcomes; essential core, add-ons, and limits remain separate; owner approval is still required. |
| RWP-00.23 | Onboarding | Pass with the canonical corrections below — the first-screen path is usable without forced purchase or integration; defaults are industry/subtype-driven; deferral, resume, pairing, targeting, publication, accessibility, and recovery are covered. |
| RWP-00.24 | Default dashboard | Pass with canonical subtype reconciliation — exception-first, venue-time-aware hierarchy preserves urgent core actions and distinguishes state, permission, purchase, add-on, limit, connection, and rollout conditions. |
| RWP-00.25 | KPIs and analytics | Pass with canonical subtype reconciliation — core operational evidence is separated from advanced native analysis and external data; source, grain, time, freshness, quality, privacy, retention, correction, and export are explicit. |

## Canonical subtype resolution

RWP-00.16 remains authoritative. The nine primary subtypes are:

1. Pub
2. Sports Bar
3. Cocktail Bar
4. Wine Bar
5. Brewery
6. Brewpub
7. Taproom
8. Nightclub
9. Lounge

The neutral fallback is **Unspecified / General Bar**.

Music, live entertainment, food-led operation, production-led operation, tasting, sports emphasis, late-night operation, reservations, private events, multi-room operation, and similar characteristics are descriptive traits or operating emphases unless one of the nine primary subtype definitions applies. They do not create additional primary subtypes, commercial packages, permissions, limits, or entitlements.

Brewery and Brewpub remain distinct because production/tasting-led operation and combined brewing/food-service operation have meaningfully different defaults. A subtype change preserves authored content, screens, current state, history, authority, integrations, commercial access, and limits.

## Restaurant inheritance

The Bar profile inherits rather than duplicates the Restaurant baseline for:

- ordinary content organization and editing;
- screen pairing and management;
- explicit screen targeting;
- preview and immediate publication;
- target-level delivery confidence;
- online, offline, outdated, failed, partial, and unknown state;
- correction, retry, supersession, undo, and restoration;
- basic layouts, themes, static content, and ordinary rotation;
- ordinary user and object permissions;
- customer-authored names and labels;
- the seven-category Track 0 classification model.

Bar-specific documentation adds or qualifies:

- drinks, taps, pours, flights, bottles/cans, cocktails, wine, releases, tastings, and optional food context;
- high-frequency availability and sold-out changes;
- venue, bar, kitchen, doors, event, last-entry, and locally authored last-call timing;
- cross-midnight operating-day behavior;
- happy hour, game-day, release, tasting, lineup, DJ, live-music, sports, and private-function context;
- bar, table, counter, lounge, patio, stage, viewing-zone, and multi-area service;
- general reservation, cover, ticket, guest-list, and entry guidance without exposing private transaction state;
- bounded responsible-content and age/access wording supplied by authorized operators or authoritative sources;
- subtype-aware onboarding, dashboard, screen-purpose, and analytics emphasis.

No inherited Restaurant capability is removed by industry, subtype, trait, or proposed commercial packaging.

## Terminology and operating consistency

The package consistently distinguishes:

- content from availability state and from the Quick Update capability;
- tap lists from tap positions and from tap-system synchronization;
- happy hour, special, release, and event values from advanced scheduling or campaign workflow;
- venue hours, bar/kitchen periods, doors, event time, last entry, and locally authored last-call information;
- general reservation, cover, ticket, guest-list, and entry guidance from private transactions and eligibility;
- accepted publication from confirmed target delivery;
- offline from outdated, failed, partial, unknown, stale, and disconnected;
- save, preview, publish, confirm, correct, retry, supersede, undo, and restore.

The operating model supports venue-local time, cross-midnight service periods, independently active venue/bar/kitchen/doors/event/last-entry times, rapid availability changes, table/bar/counter/lounge/event-led service, multiple areas, explicit target scope, event changes, privacy-safe entry guidance, manual fallback, and target-level delivery recovery.

Vennusign does not invent jurisdictional last-call, responsible-service, age, entry, licensing, or safety policy. Operators or authorized sources supply such wording and retain responsibility for it.

## Classification validation

### Core capabilities

- manual drink, tap, cocktail, wine, optional food, special, release, event, entry, venue-information, and screen-content management;
- manual Quick Update;
- manual current hours and one-off changes, including cross-midnight periods;
- manual event, delay, cancellation, relocation, pause, resumption, and public guidance;
- screen pairing and management;
- explicit targeting and contextual preview;
- immediate manual publication;
- target-level delivery confidence;
- correction, retry, supersession, undo, and restoration;
- basic layouts/themes and ordinary rotation;
- current operational status and recovery evidence;
- clear local terminology and privacy-safe public wording.

### Product/domain state

Industry, subtype, traits, terminology preference, content, prices, descriptions, labels, serving formats, tap positions, hours, effective periods, availability, venue/service/event state, areas, targets, versions, publication and delivery state, source, freshness, conflict, override, metric, dimension, and retained operational values.

### Permissions

View, edit, controlled wording, approve, publish, restore, screen management, organization/venue administration, integration administration, analytics view/configure/export, commercial administration, and object/area/venue scope.

### Tier entitlement candidates

Advanced schedules/dayparts/recurrence; event series; campaigns; richer presentation and synchronized displays; shared libraries and multi-venue templates; brand governance; approvals and advanced audit; advanced native dashboards, comparisons, analytics, reports, exports, and retained history; selected enterprise administration and policy outcomes.

### Independent add-on candidates

POS/payment; inventory/keg/tap systems; reservations; ticketing, guest list, identity, payment, and access; sports/event/lineup/venue feeds; footfall, occupancy, sensors, CRM, loyalty, advertising, weather, and traffic data; metered AI/translation/images/analysis; managed hardware/connectivity/monitoring/support; custom integrations and managed data services; separately costly identity-provider connections.

### Usage or quantity limits

Venues, areas, screens, devices, users, roles, approvers, lists, items, taps, events, schedules, campaigns, templates, assets, media, storage, bandwidth, history, reports, exports, integrations, connections, transactions, data, queries, refresh frequency, requests, tokens, languages, monitoring endpoints, support, and spend.

### Internal rollout flags

Experiment cohorts, staged releases, compatibility modes, migrations, emergency disablement, and temporary exposure controls.

No subtype is a tier. No permission grants purchase. No product state is a feature flag. No count grants a capability. No rollout control is a customer product.

## Essential-core protection

Without an advanced tier or external system, an authorized operator can still:

- create and edit current venue/public content;
- change availability or sold-out state;
- set current hours and one-off changes;
- communicate specials, releases, events, delays, cancellations, entry guidance, and venue state;
- pair/manage ordinary screens within allowance;
- select exact targets;
- preview and publish immediately;
- see target-level delivery state;
- identify offline/outdated/failed/partial/unknown targets;
- retry, correct, supersede, undo, and restore;
- use accessible mobile and desktop operation;
- see sufficient operational history to diagnose and recover current state.

Advanced workflow and integrations may automate or scale these outcomes but cannot remove or obscure the manual path.

## Packaging validation

The proposed working progression remains:

1. **Operate Today** — accurate current content, hours, events, screens, delivery, and recovery.
2. **Plan & Promote** — advanced recurrence, coordinated events, campaigns, richer presentation, and reusable venue workflows.
3. **Scale & Govern** — multi-venue libraries, brand control, approvals, enterprise coordination, advanced analytics, and governance.

These are planning labels, not approved commercial tier names. Exact tier count, names, prices, trials, capability boundaries, limits, overages, contracts, and downgrade rules remain owner decisions. External, managed, physical, custom, and metered capabilities remain independent add-on candidates where their cost or value is separable.

## First-screen and pricing-timing decision

The first-value path is a real screen displaying accurate venue content with target-level delivery confirmation.

The accepted planning direction is:

- core setup and first-screen activation are completed before full pricing, tier comparison, or add-on sales presentation;
- industry and subtype selection never select a plan;
- optional capability value may be explained contextually only after the included path is understood;
- when screen pairing is deliberately deferred, onboarding preserves content and supplies an exact next action but does not introduce a pricing decision as a substitute for first value;
- urgent core actions never surface an upgrade interstitial;
- trial terms remain unresolved and RWP-13.06 remains paused.

## Representative customer journeys

### Setup and first value

An authorized user selects the organization and venue, selects Bar when appropriate, chooses one canonical primary subtype or the neutral fallback, adds optional traits, confirms venue-local time and a minimal service period, creates starter/manual content, selects a screen purpose, pairs/selects a screen or deliberately defers, explicitly selects targets, previews, publishes, and receives per-target delivery evidence.

**Result:** Pass. Core onboarding does not force a final tier, price, payment method, integration, or private transaction data.

### Daily Taproom operation and recovery

An operator opens the mobile dashboard with venue/time scope visible, marks a tap item sold out, reviews affected list/screens, previews, publishes, and sees per-target results. Offline or outdated targets provide retry, correction, or restore actions.

**Result:** Pass. Availability remains state; Quick Update, publishing, delivery confidence, and recovery remain core.

### Sports Bar event change

An operator updates a delayed or relocated fixture, public guidance, affected viewing areas/screens, and effective local time. Publication distinguishes delivered, pending, partial, failed, offline, and outdated targets.

**Result:** Pass. Manual event communication remains core; an external sports feed is optional.

### Nightclub entry and private function

An authorized operator updates doors, lineup, entry information, last entry, area, and private-event state. Locally approved age/access wording is previewed and targeted. Public operations do not expose personal guest-list, ticket, payment, or eligibility data.

**Result:** Pass. Controlled wording and public guidance are core; transaction-aware access is an authorized add-on.

### Add-on disconnect and manual fallback

An inventory/tap/POS/event connection becomes stale or disconnected. The dashboard distinguishes connection state from product availability state. Authorized manual override remains visible and publishable, with source freshness, conflict, last successful sync, and recovery explicit.

**Result:** Pass. External automation cannot remove manual operation.

### Upgrade and downgrade

An operator discovers an advanced schedule outcome contextually after completing the manual task. Upgrade grants the approved capability without changing permissions or auto-publishing. Downgrade preserves authored content, current screens, manual operation, recovery history, and safe conversion/export options.

**Result:** Pass as a planning proposal. Final commercial policy requires owner approval.

### Mixed-industry organization

A Restaurant-and-Bar organization retains one organization-level commercial policy. Each venue uses local industry/subtype terminology and defaults. Cross-venue actions preview mixed venue types, local time, permissions, state, and targets. Subtypes do not stack entitlements or allowances.

**Result:** Pass.

## KPI and analytics validation

Core evidence covers screen state, publication, delivery, content freshness, current service, exceptions, and recovery. Advanced native analytics may compare content activity, schedules, events, campaigns, venues, and workflows. Sales, stock, reservations, attendance, entry, footfall, conversion, or attribution require authoritative external data.

Every metric requires grain, dimensions, source, venue-local time, operating-day treatment, freshness, quality, included/excluded states, units, unknown/partial/stale behavior, permission, retention, correction, export, and classification. Publication or delivery evidence must not be presented as commercial impact or audience engagement.

## Accessibility and Impeccable validation

UI-facing planning consistently includes clear hierarchy, one dominant task, explicit state language, keyboard support, visible focus, matching visible/accessibility names, logical headings, error summaries, 200% zoom and reflow, mobile/tablet/desktop adaptation, localization expansion, RTL readiness, non-color status, high contrast, reduced motion, low-light/glare/noise/crowding/intermittent-connectivity contexts, safe high-scope confirmation, and actionable recovery.

The approved Sky Blue administrative direction remains intact. No visual or product implementation is authorized.

## Unresolved owner decisions

1. Final tier count, names, positioning, prices, trial behavior, and capability boundaries.
2. Whether Plan & Promote and Scale & Govern remain separate bundles.
3. Exact boundaries among schedules, event workflow, campaigns, advanced presentation, approvals, and analytics.
4. Exact limit dimensions, values, counting scope, overage, archive, retention, and downgrade behavior.
5. Trial duration, card/no-card policy, first-screen activation semantics, expiry behavior, and post-trial core access.
6. Direct versus partner delivery and bundling of integrations, managed services, data, AI, hardware, and support.
7. Jurisdictional policy boundaries and privacy/consent/retention requirements.
8. Employee/venue comparison safeguards and external-data rights.
9. Minimum core history, advanced retention, exports, privacy thresholds, and analytics source contracts.
10. Normalized cross-industry catalog and mapping from current feature keys, gates, permissions, overrides, limits, and locked surfaces.
11. Bounded implementation, migration, compatibility, testing, and rollout packages after owner approval.

These decisions do not block the Bar profile from later consolidation.

## Final checklist

- [x] RWP-00.15 through RWP-00.25 reviewed as one profile.
- [x] Restaurant inheritance is explicit and unnecessary duplication is avoided.
- [x] Canonical subtype and trait treatment is resolved.
- [x] Terminology, operations, capabilities, packaging, onboarding, dashboard, and analytics are consistent.
- [x] Every concern has one primary Track 0 classification.
- [x] Essential operations remain core.
- [x] Permissions, state, entitlement, add-on, limit, privacy/source authority, and rollout remain separate.
- [x] Representative journeys are validated.
- [x] Impeccable, accessibility, responsive, state, failure, and recovery requirements are documented.
- [x] Owner decisions are explicit.
- [x] No product implementation was performed.
- [x] RWP-13.06 and Phase 14+ remain paused.

## Final handoff

After merge, default-branch verification, issue closure, shared-record synchronization, and claim release:

- Bar, Brewery & Nightlife is complete through **RWP-00.26**.
- No additional Bar RWP is approved.
- Do not start Bar implementation.
- Do not start RWP-00.75 until the five-industry gate is satisfied.
- The live tracker and merged repository state remain authoritative for other parallel industry streams.
- Resume only the first unfinished approved RWP in a valid stream and do not duplicate existing ownership.