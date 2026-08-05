# Food Truck & Concession KPIs & Analytics

## Purpose

This document defines the Food Truck & Concession KPI and analytics planning model. It separates required operational visibility from optional advanced analytics and identifies the source, freshness, privacy, retention, export, and external-data dependencies for each metric family.

It is documentation only. No analytics pipeline, report, dashboard, API, schema, integration, event tracking, retention policy, or product behavior is authorized.

## Measurement principles

1. Operational truth comes before performance optimization.
2. Core visibility must work without a POS, order, inventory, venue, weather, queue, footfall, route, or event integration.
3. A metric must show its scope, time basis, source, freshness, completeness, and known exclusions.
4. Unknown, unavailable, not configured, stale, disconnected, and zero are different values.
5. Manual states may be measured as operator-recorded activity but must not be presented as verified sales, inventory, wait time, attendance, or footfall.
6. Advanced analytics may be tier-entitled, while external data connections remain independent add-on candidates.
7. Retention, refresh, row count, export frequency, and data volume remain limits.
8. Public or shared reports must not expose customer, employee, ticket holder, order, payment, location-history, or commercially sensitive data without approved authority and aggregation.

## Core operational visibility

The following visibility is required for safe ordinary operation and should not be premium-only.

### Operation and service status

- current operation, unit, stand, service point, location, event, and host context;
- current planned/setup/ready/open/limited/paused/relocating/closed/canceled/unknown state;
- current service period, last-order time, and next known state change when explicitly entered;
- number of operations currently requiring attention;
- recent operator-confirmed state changes and changed-by identity where history exists.

These are product/domain-state and audit facts, not performance KPIs.

### Menu and availability health

- active menu/content set;
- counts of available, sold-out, unavailable, limited, draft, or invalid items/combos/categories;
- time since the last high-impact availability change;
- recent manual Quick Updates;
- unresolved validation or incomplete-content conditions;
- source and freshness when availability is integrated.

A count of sold-out items does not by itself measure lost sales, inventory shortage, or demand.

### Screen and publication health

- intended screen count;
- screens online, offline, outdated, unknown, failed, or successfully delivered;
- latest publication accepted, pending, partial, failed, or delivered;
- time since last successful delivery by target;
- current-version coverage percentage across intended targets;
- retry and restoration outcomes;
- last-known-good version when represented.

Current-version coverage is calculated as successfully delivered intended targets divided by all intended targets for the selected publication. Unknown targets remain in the denominator and are identified separately; they are not silently treated as healthy.

### Integration operational health

For each configured connection:

- connected, disconnected, degraded, stale, conflicting, partially synchronized, or unknown state;
- last successful synchronization time;
- last attempted synchronization time;
- source-reported effective time when available;
- affected objects and unresolved conflicts;
- manual override or fallback state.

This health visibility is core after an add-on is connected. The integration itself remains an add-on.

## Location and event performance candidates

These metrics require reliable location/event/service-period context and may be advanced analytics candidates:

- number of service periods by location, event, host, market, route, stand, gate, or section;
- scheduled versus completed service periods;
- delayed, relocated, canceled, shortened, or extended service periods;
- time open and time limited/paused/closed during expected service;
- menu or promotion mix by location/event;
- sell-out timing by location/event/service period;
- publication and screen-health exceptions by location/event;
- sales, orders, guests, transactions, revenue, units, average order, or conversion where authoritative external data exists;
- footfall, queue, occupancy, or wait metrics where an approved source exists.

A location change recorded after service must not be retroactively assigned to earlier data without explicit effective-time rules.

## Service-window and operations metrics

Candidate metrics include:

- setup-to-ready duration;
- ready-to-open duration;
- planned versus actual opening and closing times;
- service duration;
- time spent limited, paused, relocated, or closed;
- number and duration of service interruptions;
- number of Quick Updates during service;
- time from operator edit to publication acceptance;
- time from publication acceptance to confirmed delivery by target;
- failed/partial publication rate;
- screen outdated duration during expected service;
- recovery time after offline, failed, or outdated state.

These metrics require consistent event timestamps and explicit local time zones. They must identify whether times are operator-entered, system-recorded, player-confirmed, or external-source supplied.

## Menu, item, and sell-out analytics

Optional advanced analytics may include:

- item and combo availability duration;
- sell-out time within a service period;
- repeated sell-out frequency by item, location, event, or unit;
- duration unavailable or limited;
- menu-item appearance and promotion exposure;
- sales/item mix when POS or order data exists;
- estimated missed opportunity only when an owner-approved methodology and sufficient source data exist;
- comparison of manual availability changes with integrated inventory or sales signals;
- restoration/reopen timing after an item becomes available again.

The product must not infer demand, stock level, waste, margin, or lost revenue from a manual sold-out state alone.

## Promotion and content performance

Optional candidates include:

- campaigns scheduled, activated, expired, canceled, or restored;
- target and delivery coverage;
- content exposure time by screen, location, event, service period, or unit;
- promotion-associated sales/order/item mix where source attribution is valid;
- sponsor/host content delivery compliance where contractually authorized;
- variant comparison when an approved experiment design exists;
- public location page views, link actions, or notification engagement where those optional capabilities exist;
- time-to-update and correction frequency.

Screen delivery is not proof that a guest viewed or acted on content. Exposure, impression, engagement, and conversion must be named according to actual evidence.

## Multi-unit analytics

Optional coordinated or advanced outcomes include:

- comparable operational status across units, stands, windows, events, and regions;
- exception rate and recovery time by unit;
- current-version coverage by unit or group;
- sell-out and service-interruption patterns;
- location/event performance comparison;
- menu, price, promotion, and template adoption with local-override visibility;
- source freshness and integration-health comparison;
- schedule adherence and conflict trends;
- performance distributions rather than only averages;
- identification of incomplete or non-comparable data.

Rankings must not compare units with materially different schedules, markets, formats, data sources, or coverage without an explicit normalization and disclosure.

## Core versus premium classification

| Analytics outcome | Classification |
| --- | --- |
| Current operation/service state | Core operational visibility |
| Current menu/availability exceptions | Core operational visibility |
| Current screen and publication health | Core operational visibility |
| Connected integration health and freshness | Core operational visibility after add-on connection |
| Recent operator changes and basic history | Core within included retention limit |
| Service-period trend analysis | Tier-entitlement candidate |
| Location/event comparison | Tier-entitlement candidate |
| Multi-unit benchmarking and exception trends | Tier-entitlement candidate |
| Advanced promotion/content analysis | Tier-entitlement candidate |
| Forecasting, recommendations, anomaly detection, and natural-language summaries | Advanced tier/AI candidate |
| POS/order/inventory/queue/footfall/weather/event/traffic data | Independent add-on candidate |
| Long-term history, scheduled reports, large exports | Tier candidate with separate limits |
| Rows, refreshes, retention, exports, API use, AI generation, and storage | Usage/quantity limits |

## External data dependencies

### POS and transaction data

Potentially supports sales, revenue, item mix, modifiers, discounts, average order, and sales-linked promotion analysis. Required metadata includes transaction time, location/unit, item identity, currency, tax/discount treatment, void/refund state, and source freshness.

### Ordering and payment data

Potentially supports order count, channel, pickup state, fulfillment time, order value, abandonment, and payment status. Payment details and personal data require strict minimization and approved handling.

### Inventory, kitchen, and production data

Potentially supports stock or production status, depletion, preparation, batch readiness, and automated sell-out context. Source conflicts and unit-of-measure mapping must be explicit.

### Venue, event, host, and route data

Potentially supports event attendance context, gates/sections, host schedule, planned stop, event phase, delay, cancellation, and route adherence. It must not imply attendance, arrival, or operating status unless the source proves it.

### Queue, footfall, occupancy, and sensor data

Potentially supports wait, queue length, passage, occupancy, and conversion analysis. Sensor coverage, placement, confidence, downtime, aggregation, and privacy limitations must be shown.

### Weather, traffic, maps, and public conditions

Potentially supports contextual comparison and operational correlation. Correlation must not be presented as causation. Location precision and historical retention require explicit policy.

## Metric specification requirements

Every approved metric must document:

- business name and plain-language purpose;
- numerator and denominator where applicable;
- included and excluded records;
- grain and supported dimensions;
- event/effective/recorded time basis;
- local time-zone and operating-day rules;
- source system and source authority;
- freshness and latency;
- handling of missing, late, corrected, duplicate, voided, refunded, stale, and conflicting data;
- currency, tax, unit, and rounding treatment where applicable;
- privacy classification;
- retention and export rules;
- whether the metric is core, tier-entitled, add-on-dependent, or limited;
- validation method and reconciliation source.

## Privacy and data minimization

Analytics should default to operation, unit, service period, location/event, menu item, screen, and aggregate outcome—not identified people.

Do not expose or retain by default:

- customer name, contact, payment, precise individual location, loyalty identity, or order detail beyond approved purpose;
- employee performance at identified-person level when aggregate operational analysis is sufficient;
- host, sponsor, venue, contract, revenue, or settlement data outside authorized scope;
- raw sensor, device, network, or location history longer than required;
- sensitive operational or safety information in public exports.

Access, export, sharing, deletion, and retention must be permission-controlled and auditable where required. Track 0 does not define legal compliance; owner, privacy, security, and contractual review remain necessary.

## Retention and correction

- Core current-state visibility uses the latest authoritative state and an included bounded history.
- Longer operational, analytics, publication, external-data, and audit histories may depend on tier and explicit limits.
- Corrections must preserve source/effective/recorded time and avoid rewriting prior published reports without a visible recalculation policy.
- Disconnected add-ons must define whether historical data remains available, read-only, exportable, or scheduled for deletion.
- Downgrade must disclose affected history, reports, schedules, exports, and deletion dates before removal.
- Unknown retention must not be implied as permanent.

## Export and reporting

Exports and scheduled reports must show:

- selected scope and time zone;
- generation time;
- source freshness/completeness;
- filters and dimensions;
- metric definitions or version;
- permission and privacy scope;
- row/size/retention limits;
- whether results may change after late data or correction;
- failure, partial, expired, and retry states.

CSV, spreadsheet, PDF, API, email, and external-destination delivery are separate format/delivery decisions. Scheduled delivery and external destinations require explicit authority.

## Dashboard and Impeccable guidance

Core operational status belongs in the default dashboard. Advanced analytics should be discoverable but must not crowd out the immediate Operate task. Future analytics surfaces should support comparison and exploration with clear definitions, filters, source/freshness indicators, empty and incomplete-data states, keyboard access, 200% zoom, assistive technology, long labels, non-color-only series, downloadable definitions, and mobile summaries.

The project-local Impeccable `shape`, `harden`, and `polish` guidance applies. No chart, report, query, or UI is implemented by this document.

## Owner decisions required

- final list of core history views;
- final advanced-analytics tier placement;
- source-specific add-on packaging;
- supported dimensions and time grains;
- operating-day cutoff rules;
- retention and export allowances;
- AI/forecasting inclusion and metering;
- identified-person and location-data policy;
- benchmark normalization and minimum data coverage;
- scheduled-report destinations and sharing controls;
- correction/recalculation/versioning policy;
- regional, contractual, and provider constraints.

## Validation

The model covers core operational visibility, location/event performance, service-window and promotion metrics, multi-unit analytics, external POS/order/event/footfall dependencies, privacy, retention, correction, export, and core-versus-premium classification. It distinguishes evidence from inference and authorizes no implementation.
