# Bar, Brewery & Nightlife Subscription Tier Mapping

## Authority and scope

This document proposes a Bar, Brewery & Nightlife tier mapping for owner review under RWP-00.22. It uses capability classifications from RWP-00.21 and preserves the Restaurant baseline. The bundle names are outcome labels, not approved commercial tier names. No pricing, billing, entitlement keys, or live gates are approved or changed.

## Packaging principles

1. Essential daily operation remains universally included.
2. Industry and subtype selection never grant commercial access.
3. Permissions, represented state, add-ons, limits, and rollout controls remain separate from tier bundles.
4. Advanced Vennusign workflow may be tier bundled when its value increases with coordination or scale.
5. External, managed, physical, custom, or metered services remain independent add-on candidates where their value or cost is separable.
6. Downgrade must preserve customer-authored data, last-known-good published content, manual core operation, and an understandable path to reduce use or re-upgrade.
7. Final packaging waits for cross-industry normalization and owner approval.

## Proposed outcome bundles

### Universally Included Core — Operate Today

Every subscribed or validly trialing organization must be able to:

- create and manage drink menus, tap lists, cocktail lists, wine lists, specials, releases, serving formats, optional food content, and venue information;
- use desktop and mobile Quick Update for available, unavailable, and sold-out state;
- manage manual venue, kitchen, bar, doors, event, last-entry, and locally authored last-call timing, including cross-midnight periods and one-off changes;
- create and publish manual specials, releases, tastings, game-day offers, events, viewing-zone information, general entry guidance, and responsible public wording;
- manage and pair screens, select screen purposes, target explicitly, preview, publish immediately, confirm delivery, identify offline/outdated/failed/partial targets, retry, correct, supersede, and restore;
- retain basic layouts/themes and ordinary static or rotating presentation;
- use ordinary permissions without confusing authority with commercial access;
- see core operational status for current content, screens, publication, and delivery.

The core outcome is “keep today’s venue information accurate and safely visible.” It is not a low-value teaser and must not become impractical through restrictive feature gates.

### Candidate Bundle A — Plan & Promote

Outcome: reduce repeated setup and coordinate recurring public communication.

Candidate capabilities:

- advanced dayparts and cross-midnight schedule templates;
- recurring happy hours, specials, releases, and exception calendars;
- recurring event series, lineup templates, sports fixtures, reusable viewing-zone assignments, and coordinated event/offer changes;
- campaign calendars, reusable campaign groups, richer rotations, advanced presentation controls, and event-specific screen sets;
- reusable menu, event, and promotional templates within a venue;
- additional history depth and schedule comparison sufficient for planning and recovery.

This bundle builds on included manual operations. Failure, expiry, downgrade, or limit conditions must not hide current content, block immediate manual changes, or remove delivery confidence.

### Candidate Bundle B — Scale & Govern

Outcome: coordinate multiple venues, brands, teams, screens, and controlled workflows.

Candidate capabilities:

- organization-wide and regional libraries;
- approved brand assets and controlled templates;
- safe sharing, copying, distribution, inheritance preview, and local overrides;
- advanced multi-screen synchronization and video-wall orchestration;
- approval chains, separation of author and publisher duties, controlled responsible-content review, acknowledgments, assignment, escalation, and advanced audit;
- organization dashboards, cross-venue exceptions, comparative operational reporting, campaign/event analysis, saved reports, advanced exports, and longer retention;
- enterprise administration and policy controls that remain separate from ordinary permissions.

This bundle must preserve local venue identity, authored content, time zones, operating state, screen targets, permissions, and restore points. Organization membership does not automatically grant management authority.

## Why two advanced bundles are proposed

The Restaurant baseline already separates day-to-day operation from advanced scheduling/presentation and from multi-venue governance/analytics. Bar operations follow the same outcome progression:

1. **Operate Today:** accurate current lists, hours, events, screens, delivery, and recovery.
2. **Plan & Promote:** recurring schedules, coordinated events, campaigns, and richer presentation.
3. **Scale & Govern:** shared libraries, approvals, brand control, enterprise coordination, and advanced analytics.

This avoids creating Bar-only commercial tiers while allowing subtype-relevant presentation inside the same cross-industry capability bundles.

## Independent add-on candidates

The following remain outside ordinary tier mapping unless owner review later identifies a genuinely included low-cost subset:

- POS synchronization;
- inventory, keg, and tap-management synchronization;
- reservation-system connections;
- guest-list, ticketing, payment, identity, and access-control connections;
- sports, fixture, score, event, lineup, venue, or rights-controlled data feeds;
- premium analytics or external footfall/transaction data;
- AI and externally metered content, image, translation, analysis, or optimization services;
- managed players, screens, installation, connectivity, monitoring, replacement, and enhanced support;
- custom integrations, transformations, exports, and customer-specific data services;
- enterprise identity-provider connections where separately operated or costly.

Each add-on must define source authority, privacy/rights, freshness, permissions, mapping, limits, safe fallback, disconnection, cancellation, retention, and recovery. Manual core operation remains available when an add-on is absent or fails.

## Organization and venue inheritance

- The organization owns one commercial subscription policy under the existing technical baseline.
- The organization’s primary industry selects default terminology, recommendations, and initial guidance only.
- A venue may select Bar or another supported business type without changing the organization’s subscription by itself.
- Tier access is evaluated at organization scope unless a later approved policy explicitly supports an independently purchased venue add-on.
- A venue inherits commercially available capabilities from the organization but still requires local permissions and valid object scope.
- Local subtype changes recommendations only and do not stack, multiply, or unlock tier capabilities.
- Mixed-industry organizations receive the same cross-industry capability bundle, presented with venue-appropriate language and recommendations.
- Organization-wide actions must preview mixed venue types, local time, current state, permissions, targets, and likely impact before publication.

## Limits remain separate

Candidate limits include venues, areas, screens, devices, users, roles, approvers, lists, items, taps, events, schedules, campaigns, templates, assets, media duration, storage, bandwidth, history, reports, exports, integrations, connections, transactions, requests, tokens, languages, monitoring endpoints, support incidents, data, and spend.

A tier may carry values for these dimensions, but the limit is not the capability. Reaching a limit must:

- identify the exact dimension and scope;
- show current use and allowance;
- preserve existing data and published content;
- allow safe deletion, archival, reassignment, or plan review where appropriate;
- preserve urgent manual correction and recovery;
- distinguish limit reached from not purchased, not permitted, not configured, disconnected, stale, unsupported, or rollout-disabled.

## Upgrade behavior

Upgrade should:

- preserve all current content and settings;
- unlock the approved capability without changing permission assignments;
- explain newly available operator outcomes rather than exposing implementation keys;
- identify configuration still required, including sources, targets, approvals, or connections;
- avoid automatic organization-wide publication or inheritance changes;
- permit preview and controlled adoption of templates, schedules, campaigns, or policies.

Industry onboarding must not force an upgrade to complete the first screen or core setup. Pricing and optional capability discovery should appear after the operator understands the included path, normally after or around first-screen activation rather than before core value is demonstrated.

## Downgrade behavior

Downgrade should:

- preserve customer-authored content, current published versions, history required for recovery, and manual core operations;
- stop creation or expansion of unavailable advanced objects without deleting them silently;
- identify scheduled actions, campaigns, approvals, shared dependencies, reports, or synchronized displays affected at period end;
- permit export, simplification, detachment, or conversion to manual operation where appropriate;
- define whether active advanced schedules finish, pause, or require conversion before the downgrade becomes effective;
- retain clear read-only visibility for a bounded period when safe and approved;
- separate tier loss from permission loss, connection failure, and limit overage;
- avoid breaking current screen delivery or concealing offline/outdated status.

Add-on cancellation must separately define disconnect, data retention, source fallback, device/service return or support termination, and any consumption reconciliation.

## Packaging risks

- Gating Quick Update, delivery confidence, or recovery would make the product unsafe and impractical.
- Bundling external integrations into ordinary tiers may hide variable cost and support responsibility.
- Subtype-specific packages would fragment mixed organizations and confuse defaults with entitlement.
- A large “all advanced features” tier may combine unrelated value and make upgrades difficult to explain.
- Separate micro-features for every schedule, event, or display option would create a confusing à-la-carte catalog.
- Tier names based on venue size may misrepresent operational complexity.
- Low screen counts do not necessarily mean simple operations, and high counts do not automatically require every workflow feature.

## Owner decisions required later

- Final number and names of subscription tiers.
- Whether Plan & Promote and Scale & Govern remain separate bundles.
- Exact capability boundaries between advanced event workflow, campaigns, and approvals.
- Which analytics are included, advanced-tier, premium add-on, or externally sourced.
- Whether any low-cost AI assistance is included in a tier allowance.
- Exact limit values and overage behavior.
- Trial access and post-trial downgrade behavior.
- Direct versus partner delivery of managed hardware and services.

## Impeccable planning implications

Future plan-comparison and locked-capability surfaces must lead with operator outcomes, keep included manual actions visible, show permission and configuration requirements separately, avoid surprise paywalls during urgent tasks, and distinguish included, upgrade required, not permitted, limit reached, add-on required, not configured, disconnected, stale, and rollout-disabled states. Support clear comparison, accessible keyboard navigation, 200% zoom, localization expansion, non-color-only status, restrained motion, mobile/desktop adaptation, and the approved Sky Blue administrative direction.

## Boundaries and handoff

Proposal only. No pricing, billing, tier records, trial rules, entitlement keys, live gates, UI, API, schema, migration, checkout, webhook, or product implementation.

RWP-00.23 owns the Bar onboarding specification using this proposal as an unapproved dependency.