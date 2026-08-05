# Bar, Brewery & Nightlife KPIs & Analytics

## Authority and scope

This document defines the documentation-only KPI and analytics catalog for Bar, Brewery & Nightlife under RWP-00.25. It separates essential operational visibility from advanced Vennusign analytics, external-data add-ons, permissions, limits, privacy, retention, and rollout controls. It does not authorize analytics implementation, tracking, schema, migration, integration, pricing, or product behavior.

## Classification principles

1. **Core operational visibility** explains whether current content, screens, publication, and recovery are working.
2. **Tier analytics candidates** provide native comparison, history, optimization, coordination, and reporting beyond current-operation needs.
3. **Independent analytics or data add-ons** depend on external, licensed, managed, metered, physical, or customer-specific data and services.
4. **Product/domain state** is the represented metric, dimension, source, freshness, quality, target, event, or operational value.
5. **Permissions** determine who may view, configure, export, or administer analytics.
6. **Limits** measure retention, volume, frequency, rows, exports, reports, queries, sources, requests, storage, or spend.
7. **Rollout flags** temporarily control delivery only and never represent metric availability, entitlement, state, authority, or a limit.

The absence of an external source must never remove core screen and publication visibility or prevent manual operation.

## Core operational KPIs

These outcomes must remain available with the required manual product baseline.

### Screen health and reachability

- total intended screens by venue, area, and purpose;
- online, offline, outdated, unknown, and unsupported counts;
- last contact or last known state where authoritative;
- screens missing a current intended target or version;
- time since a screen became offline or outdated;
- unresolved screen exceptions and oldest unresolved exception.

Basic health must not depend on managed monitoring. Device count is a limit; managed monitoring and service are add-ons.

### Publication and delivery confidence

- publish attempts and result state;
- accepted/pending, delivered, failed, partial, unknown, and restored target counts;
- intended versus delivered target coverage;
- latest successful publication by venue, area, purpose, and screen;
- time from publish request to confirmed delivery where available;
- failed/partial attempts awaiting action;
- retry, correction, supersession, undo, and restoration outcomes;
- current last-known-good version and age.

A successful request is not treated as delivery to every screen. Metrics must preserve target scope and distinguish accepted from delivered.

### Content freshness and operational accuracy

- active content version and effective period;
- content approaching expiry without replacement;
- stale or unknown source values;
- draft/sample/incomplete content affecting an active path;
- time since important hours, events, specials, or availability values were reviewed or changed;
- current unavailable, sold-out, limited, or unknown item counts where represented;
- unresolved source conflict or manual-override state;
- current venue/service state and next scheduled transition.

Freshness is a represented value with source and time context, not a quality promise invented by Vennusign.

### Operational responsiveness and recovery

- age of unresolved delivery, screen, source, target, or content exceptions;
- safe retry success rate;
- correction or supersession completion;
- restoration success and restored version age;
- time from detected exception to first operator action where captured;
- time from operator action to resolved state;
- recurring exception counts by type and scope;
- manual fallback use after a disconnected or stale source.

Core metrics should help an operator act; they are not employee-performance scoring by default.

### Current service and schedule visibility

- venue-local current operating day and service period;
- current and upcoming venue, bar, kitchen, doors, event, last-entry, and closing transitions;
- schedule conflicts or gaps visible to the operator where already supported;
- active one-off exceptions;
- cross-midnight periods mapped to the correct operating day;
- content whose effective time does not align with the intended service period.

Manual current-state visibility is core. Advanced schedule analysis is a tier candidate.

## Content, promotion, and event analytics candidates

These are advanced native analytics candidates when derived from Vennusign-controlled data. They do not become core merely because the content object is core.

### Content activity

- content creation, edit, approval, publication, supersession, expiry, and restoration trends;
- most-used content types, templates, screen purposes, layouts, and target groups;
- content reuse and local override patterns;
- change volume by venue, area, role, daypart, and event context;
- content coverage across screens, purposes, areas, and languages;
- repeated corrections and drift from shared standards.

Activity is not audience engagement unless an authoritative measurement source supports that conclusion.

### Specials, happy hours, releases, and promotions

- active and scheduled offer counts;
- publication coverage and delivery success for each offer;
- time-to-publish before effective start;
- late, missed, conflicting, or extended effective periods;
- usage across venues, areas, screen purposes, and dayparts;
- recurrence, reuse, and campaign participation;
- optional comparison with externally sourced sales, inventory, or footfall only when an add-on supplies the data.

Vennusign-only data can measure communication execution, not prove commercial performance.

### Events and entertainment

- event, sports fixture, tasting, lineup, trivia, DJ, live-music, private-function, and game-day content counts;
- planned versus published event coverage;
- delay, cancellation, relocation, pause, resumption, or correction frequency;
- affected screens, areas, and viewing zones;
- time from authoritative change to public update where source and timestamps support it;
- event-specific publication and delivery outcomes;
- recurring-event reuse and exception handling;
- external ticket, reservation, attendance, sports, lineup, or footfall outcomes only through authorized add-ons.

### Daypart and service-period analysis

- content and offer coverage by breakfast, daytime, happy hour, evening, late-night, event, private-function, or customer-defined period;
- transitions published on time;
- cross-midnight mapping quality;
- conflicts among venue, bar, kitchen, doors, event, last-entry, and locally authored last-call periods;
- screens carrying content outside its intended period;
- recurring manual corrections that may indicate a scheduling opportunity.

## Venue-level and organization-level analytics

### Venue-level outcomes

A venue view should prioritize:

- current exceptions and health;
- content freshness;
- delivery and target coverage;
- service-period alignment;
- current/next specials and events;
- availability-change recency;
- recovery actions;
- source and connection quality where relevant.

### Organization-level outcomes

Advanced portfolio analytics may provide:

- cross-venue exceptions and unresolved-risk ranking;
- comparable screen, delivery, freshness, campaign, event, and schedule outcomes;
- shared-template adoption and local override patterns;
- brand or regional coverage and drift;
- venue-local-time normalization;
- mixed-industry comparison using neutral metrics;
- saved reports, scheduled reports, exports, longer history, and governance views.

Organization membership does not grant analytics access. Venue-specific restricted data remains permission- and audience-scoped. Mixed-industry metrics must use neutral dimensions without forcing Bar terminology on other venues.

## Subtype-specific analytics needs

- **Bar / Pub:** specials, food/bar period alignment, events, availability recency, and delivery coverage.
- **Brewery / Brewpub:** house product and release communication, taproom/food context, tasting/tour events, and external production/inventory data when connected.
- **Taproom:** tap-change frequency, sold-out/unavailable recency, releases, flights, tastings, and optional tap/inventory synchronization quality.
- **Cocktail Bar:** cocktail-list changes, specials, reservation-information coverage, lounge/table context, and optional reservation or sales data.
- **Wine Bar:** glass/bottle availability, limited products, flights, tastings, pairings, and optional inventory/sales data.
- **Sports Bar:** fixture and viewing-zone coverage, game-day offers, schedule changes, area targeting, and optional sports, sales, or footfall data.
- **Nightclub:** doors, lineup, entry/cover communication, event changes, areas, private functions, and optional ticketing/guest-list/access/footfall data.
- **Lounge:** table/lounge service, reservations information, entertainment, specials, private areas, and optional reservation/sales data.
- **Music / Entertainment Bar:** lineup, doors, stage/area coverage, delay/cancellation response, entry guidance, and optional ticket/attendance data.
- **Unspecified:** neutral content, screen, delivery, freshness, schedule, event, and recovery metrics.

Subtype changes recommendations and labels only. It does not unlock metrics or multiply commercial allowances.

## External-data dependencies and add-on classification

The following require independent add-on or approved external-data treatment when used beyond manually authored aggregate values:

| Source family | Potential outcomes | Required boundaries |
| --- | --- | --- |
| POS and payment | sales by item/category/time/venue, promotion comparison | source authority, tax/net/gross definitions, refunds/voids, time mapping, privacy, retention |
| Inventory, keg, and tap systems | stock, depletion, tap state, sell-out prediction | mapping, freshness, units, overrides, conflict, disconnect, manual fallback |
| Reservation systems | aggregate reservations, covers, arrivals, no-shows | privacy, audience, identity minimization, venue/time mapping, retention |
| Ticketing, guest-list, identity, and access | aggregate admissions, attendance, entry state | personal data restrictions, authorization, rights, public-display boundaries |
| Sports, event, lineup, and venue feeds | fixtures, results, schedules, performer/event state | licensing, source authority, local time, delay/cancellation, correction |
| Footfall, occupancy, sensor, camera, or Wi-Fi data | aggregate traffic, dwell, occupancy, zone trends | consent/legal review, aggregation, retention, quality, device/source state |
| CRM, loyalty, advertising, or campaign systems | campaign, member, conversion, audience outcomes | privacy, consent, attribution, identity minimization, rights |
| AI or optimization services | summaries, explanations, anomaly detection, recommendations | human review, source disclosure, privacy, quality, requests/tokens/spend limits |

An external connection is not a tier entitlement. Imported metric values are product/domain state; connection access is an add-on; viewing and administration are permissions; data/query/storage volume is a limit.

## Core, tier, add-on, and limit separation

### Core

- current screen health;
- publication and target-level delivery state;
- current content version and effective state;
- basic freshness/outdated awareness;
- current unresolved exceptions;
- correction, retry, supersession, and restoration visibility;
- current service-period and scheduled-transition visibility;
- basic operational history required to understand and recover current operation.

### Tier candidates

- longer native history;
- comparative venue and portfolio analytics;
- advanced dashboards and saved views;
- scheduled reports;
- campaign, event, daypart, content, and workflow analysis;
- template/brand drift and governance reporting;
- anomaly detection using Vennusign data;
- advanced exports and BI-ready native datasets;
- forecasting or optimization based solely on approved native data.

### Independent add-ons

- external POS, inventory, tap, reservation, ticketing, guest-list, access, sports, event, footfall, CRM, loyalty, advertising, payment, BI, weather, or other data;
- premium third-party analytics or benchmarking;
- metered AI and optimization services;
- managed reporting, data transformation, or customer-specific data services.

### Limits

Candidate limits include retained days/months/years, venues, screens, users, reports, dashboards, saved views, schedules, exports, rows, files, queries, refresh frequency, data volume, storage, sources, connections, events, transactions, API requests, tokens, models, support hours, and spend.

Reaching a limit must preserve existing data according to approved retention policy, explain the measured dimension, distinguish it from permission or purchase state, and provide safe archive/export/delete/reduce/upgrade options. It must not hide current operational health or recovery.

## Data quality and metric contract

Every metric must define:

- business meaning;
- grain and dimensions;
- source and source authority;
- venue-local time and operating-day treatment;
- freshness and last update;
- included/excluded states;
- units and aggregation;
- unknown, partial, stale, disconnected, corrected, or estimated behavior;
- permissions and audience;
- retention and export;
- whether it is core, tier, add-on, or a limit dimension.

Metrics must not imply causation, revenue impact, attendance, engagement, compliance, or operational success when the available source only proves content publication or screen delivery.

## Privacy, permissions, and audience

- Public or broadly available analytics must use aggregate, privacy-safe data.
- Personal reservation, guest-list, ticket, payment, identity, access, staff-performance, or behavioral detail requires explicit authorized use and minimum necessary exposure.
- View, configure, export, schedule, share, and administer permissions remain separate.
- Restricted data must not leak into public screens, general dashboards, exports, alerts, or AI prompts.
- Organization-wide analytics must honor venue, region, brand, legal entity, and role scope.
- Employee or venue comparisons must avoid unsupported performance judgments and expose data-quality context.

## Retention, export, and correction

Planning must define:

- minimum core history needed for current-operation diagnosis and recovery;
- advanced retained history and its tier/limit relationship;
- external-source retention and deletion obligations;
- export formats and permission;
- corrected, superseded, late, or deleted source behavior;
- timezone and operating-day stability after corrections;
- downgrade, cancellation, connection removal, and account closure behavior;
- legal hold or extended retention only when separately approved.

Downgrade must not remove current health, delivery confidence, last-known-good recovery, or required audit evidence without an approved safe transition.

## Dashboard and reporting presentation

Analytics surfaces must:

- lead with operator questions and actionable exceptions;
- state scope, time zone, period, source, freshness, and data quality;
- distinguish no activity from no data, delayed data, disconnected source, permission restriction, and zero value;
- support keyboard, assistive technology, 200% zoom, reflow, long labels, localization, non-color status, and reduced motion;
- avoid decorative charts when a number, trend, or table answers the question better;
- keep current operational recovery separate from premium analysis;
- preserve the approved Sky Blue administrative direction without relying on color alone.

Project-local Impeccable `clarify` and `harden` guidance applies to metric names, filters, empty states, warnings, comparison, export, and recovery.

## Owner decisions required later

- exact core-history minimum and advanced retention tiers;
- which native content/event/daypart/campaign metrics belong in each tier;
- which external analytics are sold individually or in data packs;
- whether low-cost native AI explanation is included or independently metered;
- numeric report, export, query, data, and retention limits;
- overage and archive behavior;
- cross-venue benchmarking eligibility;
- employee or venue comparison safeguards;
- BI access and managed reporting packaging;
- privacy, consent, and regional requirements for external behavioral data.

## Boundaries and handoff

Documentation and planning only. No analytics UI, event tracking, API, schema, migration, data warehouse, integration, AI service, pricing, billing, entitlement, or product implementation.

RWP-00.26 owns validation of the complete Bar, Brewery & Nightlife Track 0 profile and the final handoff.