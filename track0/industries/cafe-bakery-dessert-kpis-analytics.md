# Café, Bakery & Dessert KPIs & Analytics

## Purpose

This document defines the Café, Bakery & Dessert KPI and analytics planning contract. It distinguishes trustworthy Vennusign operational evidence from metrics that require external POS, ordering, inventory, production, loyalty, traffic, or other authoritative data.

No analytics engine, telemetry, UI, API, schema, retention system, export, billing, entitlement, integration, AI, or product behavior is implemented.

## Evidence principles

1. A metric names its source, freshness, coverage, scope, timezone, formula, and known exclusions.
2. Screen connection, publication acceptance, and confirmed content delivery remain different facts.
3. Vennusign does not infer sales, demand, inventory, conversion, customer behavior, production, readiness, or attribution without authoritative data.
4. Unknown, unavailable, stale, partial, sampled, disconnected, and not-applicable are distinct.
5. Venue, service period, product, screen purpose, target, source, subtype, and organization scope remain visible.
6. Current operational health needed for correction is core; advanced comparison, trend, portfolio, scheduled reporting, and optimization are tier candidates.
7. External data connections and consumption-backed analysis are add-on candidates; quantities and retention are limits.

## Core operational evidence

The following current-state evidence remains available with the Operate core:

### Screen and delivery

- paired screens by venue and purpose;
- online, offline, and unknown connection state;
- intended revision per target;
- latest confirmed delivered revision and time;
- current, outdated, pending, partial, failed, canceled, excluded, and unknown delivery state;
- failed or unresolved targets requiring action; and
- retry, correction, supersession, undo, and restoration status.

### Publication

- publication requests by venue, content, service context, and target;
- accepted, rejected, pending, successful, partial, failed, canceled, superseded, and unknown results;
- age of unresolved publication exceptions;
- latest stable published version; and
- latest available recovery point.

### Content and source freshness

- saved but unpublished changes;
- age of currently intended content;
- source identity, last known freshness, coverage, and disconnect state;
- stale, conflicting, overridden, or unknown values;
- active temporary notices without expiry or supersession; and
- products or messages carrying unknown timing or unsupported claims.

Core evidence supports safe action and recovery. It is not a complete historical analytics product.

## Café operational analytics candidates

### Product and availability analysis

Potential native analytics include:

- frequency and duration of available, unavailable, sold-out, limited, next-batch, available-again, preorder-closed, and pickup-paused states;
- products, categories, flavors, batches, or options most often changed;
- time between a recorded operational change and successful publication;
- time from a failed or stale state to correction;
- proportion of active content with authoritative versus manual source; and
- repeat corrections, overrides, conflicts, and restoration events.

These values describe Vennusign-recorded state changes, not actual stock, production volume, customer demand, or lost sales.

### Service-period and daypart analysis

Potential native analytics include:

- content and availability changes by local service period or business day;
- publication and delivery exceptions by morning, bakery opening, coffee, lunch, dessert, late-night, pickup, or customer-authored period;
- planned-versus-actual Vennusign state transitions when both exist;
- unresolved handoff items across periods; and
- content freshness at period start.

Daypart assignment uses venue-local timezone and the effective business-day model. Cross-midnight activity is not silently reassigned to calendar date.

### Promotion and seasonal-content analysis

Potential native analytics include:

- campaign/content activation and expiry;
- target coverage and delivery completion;
- stale or conflicting promotional content;
- manual replacement, correction, and supersession;
- reuse of templates or content variants; and
- publication comparison across screens or venues.

Engagement, revenue, lift, conversion, or attribution require an authoritative external source and a declared methodology.

### Preorder and pickup communication analysis

Potential native analytics include:

- periods when public preorder or pickup information was active, paused, relocated, closed, or unknown;
- screens carrying pickup information;
- publication and freshness exceptions affecting pickup guidance; and
- timing of manual corrections.

Order count, payment, fulfillment, readiness, collection time, cancellation, and guest behavior require ordering/fulfillment data and privacy controls.

### Subtype analysis

Subtype may be a presentation and comparison dimension when sample size and context are disclosed. It does not determine entitlement and must not be used to claim that one subtype performs better without comparable authoritative data.

## External-data-dependent analytics

### POS and sales

Sales, revenue, units, average transaction, product mix, discount, tax, payment, and transaction attribution require POS or payment data. The report must disclose connection, account, venue mapping, business date, currency, timezone, refund/void treatment, coverage, freshness, and missing periods.

### Inventory and production

On-hand quantity, waste, yield, batch volume, production timing, depletion, forecast accuracy, and ingredient use require authoritative inventory or production systems. Public availability state alone is not inventory evidence.

### Ordering and fulfillment

Order volume, preorder uptake, lead time, readiness, pickup performance, cancellation, abandonment, and fulfillment duration require ordering/fulfillment data. Private guest and transaction data requires minimization, authorization, retention, audience, export, and deletion policy.

### Loyalty, messaging, and engagement

Membership, audience, redemption, message delivery, click, conversion, repeat visit, and attribution require loyalty/CRM/messaging sources and consent, identity, opt-out, mapping, and methodology controls.

### Footfall, queue, weather, event, and traffic

Traffic, queue, wait, attendance, weather impact, event impact, and location behavior require authoritative sensors or external data. Correlation must not be presented as causation.

## Metric contract

Every KPI definition includes:

- business question;
- metric name and plain-language description;
- primary classification and packaging relationship;
- source system and source owner;
- numerator, denominator, formula, units, rounding, and aggregation;
- venue, organization, subtype, screen, product, service-period, and date scope;
- local timezone and business-date handling;
- freshness timestamp and refresh cadence;
- coverage and excluded records;
- unknown, partial, stale, disconnected, and not-applicable behavior;
- correction, restatement, and version policy;
- permission, privacy, retention, export, and audit requirements; and
- evidence versus inference label.

A dashboard card must not conceal material denominator, coverage, freshness, or source changes.

## Venue and organization views

### Venue view

Prioritizes current public-impact exceptions, product and service-period changes, content/source freshness, screen and publication health, recovery, and local trends.

### Organization view

May compare venues only when scope, timezone, subtype, source availability, data definitions, and coverage are comparable. Organization summaries preserve local exceptions and cannot allow healthy aggregate state to hide a failed or unknown venue or screen.

### Mixed-industry view

Uses neutral cross-industry dimensions and retains industry-specific meaning. Café batch or preorder states are not forced into unrelated industry categories merely for comparison.

## Privacy and permissions

- Operational content and delivery evidence is visible only within authorized organization, venue, screen, content, source, and role scope.
- Guest, order, payment, loyalty, identity, contact, or device-level data is not exposed by default.
- Aggregation and suppression rules are required before small or sensitive cohorts are compared.
- Export authority is separate from view authority.
- Connected-source credentials, private identifiers, and raw payloads are restricted.
- AI or externally processed analysis requires explicit data-use and privacy policy.

## Retention, export, and correction

Current operational evidence and a basic recovery point remain core within included limits. Extended history, scheduled reports, comparison, advanced exports, audit, and portfolio analytics are tier candidates. External data, managed analysis, and premium storage may be add-ons.

Retention and export limits preserve:

- source and formula version;
- timezone and business-date context;
- coverage and freshness;
- correction and restatement history;
- privacy classification; and
- deletion, legal-hold, and safe-exit policy where applicable.

A downgrade or disconnected source must not silently rewrite historical meaning.

## Classification summary

| Concern | Primary classification | Treatment |
| --- | --- | --- |
| Current screen, publication, source, freshness, and recovery evidence | Core capability plus product/system state | Included for safe operation |
| KPI definitions, formulas, source, coverage, freshness, and results | Product/domain state | Versioned and reviewable |
| View, compare, export, configure, and administer authority | Permission | Object and scope specific |
| Trends, comparisons, scheduled reporting, advanced exports, benchmarking, portfolio views, optimization | Tier entitlement candidate | Native advanced outcome |
| POS, inventory, production, ordering, loyalty, messaging, footfall, queue, weather, event, traffic, premium AI or managed analysis | Independent add-on candidate | External or separable service |
| Retention, venue, report, export, row, refresh, API, storage, and AI consumption | Limit | Separate from capability |
| Temporary analytics exposure or migration | Rollout flag | Internal only |

## Impeccable analytics brief

Mode is **Operate** for current health and **Analyze** for advanced views.

- Current exceptions and recovery outrank decorative charts.
- Charts never replace exact values, source, freshness, coverage, and accessible tables.
- Empty states distinguish no events, no source, no permission, no tier, disconnected, stale, not applicable, and insufficient coverage.
- Comparisons use appropriate denominators and avoid misleading precision.
- Mobile shows the highest-impact exception and essential current evidence; desktop supports deeper comparison without obscuring operational truth.
- Keyboard, assistive technology, 200% zoom, localization expansion, non-color encodings, reduced motion, export alternatives, and plain-language formulas are required.

## Validation

This plan covers core screen/publish/content-freshness evidence; product, promotion, daypart, preorder, pickup, and subtype analytics; venue and organization views; external POS/order/inventory and other dependencies; privacy, retention, export, correction, and permissions; and core/tier/add-on/limit separation. It introduces no unsupported sales, demand, inventory, conversion, readiness, queue, attendance, or attribution claim.
