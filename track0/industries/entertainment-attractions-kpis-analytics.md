# Entertainment & Attractions KPIs & Analytics

## Authority and scope

This documentation-only model completes RWP-00.73. It defines Entertainment & Attractions metrics, analytics boundaries, data-quality rules, permission and privacy expectations, retention/export behavior, tier/add-on classification, and handoff to final profile validation.

It does not implement analytics, tracking, sensors, reporting, data pipelines, schemas, integrations, billing, pricing, entitlements, permissions, or product behavior.

## Analytics principles

1. Operational truth and public-information quality come before audience, commercial, or optimization analytics.
2. A publication request is not the same as delivery; a delivered screen is not proof that a visitor saw or acted on content.
3. Vennusign-owned operational data may support core metrics without an external integration.
4. Attendance, ticketing, admission, occupancy, footfall, measured queue, conversion, revenue, weather, transport, and other external metrics require an authoritative source and remain add-on dependent.
5. Missing, stale, conflicting, partial, estimated, inferred, and not-applicable data are distinct.
6. Predictions, modeled values, and estimates must never be presented as measured facts.
7. Every metric defines scope, time basis, source, freshness, denominator, exclusions, privacy classification, and permitted audience.
8. Industry and subtype affect terminology and recommended views, not data access or commercial inclusion.
9. Analytics access is permission-controlled and separate from tier entitlement.
10. Limits govern volume, retention, refresh, export, and consumption; they are not capabilities or feature flags.

## Metric classification model

Each metric receives one primary Track 0 classification.

### Core metrics

Core metrics use Vennusign-owned configuration, content, screen, publication, delivery, notice, version, recovery, user-action, and source-health records needed to operate the service safely. They remain available without a paid integration.

### Tier analytics

Tier candidates add native trend, comparison, segmentation, workflow, portfolio, governance, and optimization views using data Vennusign already has permission to process. Higher tiers do not remove the base operational metrics.

### Independent add-on analytics

Metrics requiring ticketing, admissions, access, queue, footfall, occupancy, mapping, venue, cinema, event, sports, CRM, membership, donor, advertising, POS, ecommerce, weather, transport, translation, AI, identity, or other external sources remain add-on dependent.

### Limits

Retention duration, refresh frequency, export volume, rows, venues, screens, events, sessions, sources, reports, schedules, recipients, storage, API calls, messages, AI tokens, and other quantities remain limits.

### Permissions and privacy

Permissions determine who may view, create, export, share, schedule, administer, or delete analytics. Privacy, rights, consent, aggregation, anonymization, and restricted data handling are policy and data-governance concerns, not commercial tiers.

## Core operational KPI set

### 1. Screen status coverage

**Question:** Are the intended visitor screens currently able to receive and display approved content?

Measures:

- intended screen count;
- online, offline, outdated, unknown, incompatible, and failed screen count;
- percentage of intended screens with a known current state;
- screens not seen within the approved operational threshold;
- duration in offline/outdated/unknown state;
- screens without assigned venue, area, purpose, orientation, or current target.

Source: Vennusign player/screen telemetry and configuration.

Rules:

- “Online” means the player met the authoritative heartbeat rule, not that content is visible to a visitor.
- Unknown screens are excluded from healthy counts and shown separately.
- A screen may be online and still display an outdated or incorrect version.

Classification: core.

### 2. Publication and delivery success

**Question:** Did approved content reach every intended target?

Measures:

- publish requests by accepted, pending, delivered, partial, failed, canceled, superseded, or unknown result;
- per-target delivery success rate;
- time from publish acceptance to confirmed delivery;
- failed/partial target count and duration;
- retries, corrections, supersessions, unpublishes, undos, and restorations;
- last successful delivered version and time;
- current intended version versus confirmed displayed version where known.

Source: Vennusign publication, version, target, and delivery records.

Rules:

- Denominator is intended targets at the time of the request, with explicitly excluded targets reported separately.
- Accepted or queued is not delivered.
- Aggregate success cannot hide a failed target.

Classification: core.

### 3. Public-content freshness

**Question:** Is visitor-facing information still within its declared useful period?

Measures:

- active content with source and update time;
- content nearing expiration;
- expired content still targeted or delivered;
- content without an explicit freshness policy where one is required;
- manual overrides older than their review time;
- stale, disconnected, conflicting, partial, or unknown sources affecting public content;
- time from authoritative source change or manual edit to corrected public delivery where known.

Source: Vennusign content, schedule, notice, source, freshness, override, and delivery records.

Rules:

- No universal stale threshold is assumed. Thresholds depend on content type, source, venue, and owner-approved policy.
- Unknown freshness is not fresh.

Classification: core.

### 4. Notice and disruption coverage

**Question:** Are active closures, delays, pauses, cancellations, relocations, restrictions, weather effects, reopening updates, route changes, and other visitor notices complete and delivered?

Measures:

- active notices by type, scope, priority, language, source, effective time, expiration, and delivery state;
- high-priority notices missing target, expiration, next-update time, language fallback, or delivery confirmation;
- notice creation-to-delivery time;
- notice correction, supersession, unpublish, and restore rate;
- notices expired without replacement or explicit resolution;
- venues/areas/experiences with conflicting public states and notices.

Source: Vennusign notice, content, schedule, source, language, target, and delivery records.

Rules:

- Counts do not judge whether a notice was legally or operationally sufficient.
- Safety-related content remains customer-authored and policy-reviewed.

Classification: core.

### 5. Schedule and occurrence quality

**Question:** Are current and upcoming visitor schedules complete, consistent, and publishable?

Measures:

- current/upcoming occurrences with venue, area, experience, local time, state, and target;
- occurrences missing location, public wording, time zone, state, target, or language fallback;
- changed, delayed, canceled, relocated, expired, or superseded occurrences;
- schedule-source freshness and conflict count;
- manual override duration and return-to-source state;
- content scheduled outside venue/experience operating windows where the data is represented;
- overlapping or contradictory occurrences where native conflict detection is available.

Source: Vennusign-owned schedules, occurrences, venue state, content, source, and target records.

Rules:

- Basic completeness and freshness are core.
- Advanced recurring-pattern analysis, conflict detection, forecasting, and portfolio comparison are tier candidates.

Classification: core for operational completeness; tier candidate for advanced analysis.

### 6. Recovery readiness

**Question:** Can the operator restore accurate public communication after a failure or mistake?

Measures:

- screens/content with a known last-successful version;
- failed publications with retry or restoration path;
- time to retry, correct, supersede, or restore;
- unresolved conflicts or stale sources without fallback;
- recovery actions completed and resulting delivery state;
- target groups with no verified fallback content.

Source: Vennusign versions, delivery, source, conflict, retry, and restore records.

Classification: core.

### 7. Language and accessibility coverage

**Question:** Is required visitor content available in the intended languages and accessibility-ready formats?

Measures:

- active content by source language and alternate-language coverage;
- missing-language variants and explicit fallback state;
- screens/contexts with required accessibility information missing or unverified;
- content flagged for long-name, text expansion, contrast, non-color, text alternative, or 200% zoom review where such validation exists;
- alternate-route and accessibility-notice coverage where customer-authored data exists.

Source: Vennusign content, language, validation, and target records.

Rules:

- These metrics measure configured coverage, not legal compliance or real-world accessibility.
- Machine translation quality requires a declared source and review status.

Classification: core for manually authored coverage and validation state; tier candidate for advanced localization workflow; add-on for external translation/AI.

## Operational state summaries

The following are represented states and dimensions, not standalone commercial capabilities:

- venue open, limited, paused, closed, canceled, weather-affected, reopening, or unknown;
- experience available, limited, full, sold out, delayed, paused, closed, canceled, relocated, restricted, or unknown;
- queue open, limited, paused, closed, or unknown;
- admission available, limited, full, sold out, entry paused, restricted, or unknown;
- screen online, offline, outdated, unknown, incompatible, failed, pending, or delivered;
- source manual, imported, integrated, inherited, copied, calculated, stale, disconnected, conflicting, partial, overridden, or unknown;
- content draft, scheduled, active, expired, canceled, superseded, unpublished, or restored.

Dashboards may count and trend these states, but the state itself is product/domain data.

## Tier-candidate analytics

### Coordinate-level candidates

- operational trend views for schedules, notices, delivery, source freshness, queue/capacity state, languages, and recovery;
- event/experience timelines and change history;
- cross-screen and cross-area delivery comparison;
- approval, assignment, acknowledgment, escalation, and shift-handoff measures;
- recurring schedule conflicts, blackout effects, event-phase readiness, and content-calendar analysis;
- native campaign, promotion, membership, sponsor, fundraising, merchandise, or service-content performance using Vennusign-owned publication and interaction data only;
- advanced localization workflow, coverage, review, and terminology analytics;
- native map/directory completeness and route-change history.

### Portfolio-level candidates

- multi-venue comparison and exception trends;
- venue, region, brand, campus, district, park, cinema, museum, arena, stadium, touring, or operating-group benchmarks;
- delivery, freshness, notice, schedule, source, language, queue/capacity, and recovery comparisons;
- shared-template adoption, local override, opt-out, drift, and mixed-state views;
- cross-venue campaign, event, program, and content coordination metrics;
- centralized preparation versus local review and adoption;
- pooled/allocated limit utilization where approved;
- data-quality and governance scorecards with drill-down to raw states.

### Enterprise-level candidates

- access reviews, delegated administration, audit export, policy, retention, rights, and governance reporting;
- enterprise data-sharing, BI administration, service-management, migration, and change-window analytics;
- complex mixed-industry and mixed-operator portfolio views;
- advanced retained history and controlled exports;
- approved custom reporting definitions and scheduled distribution.

Higher-tier analytics use Vennusign-owned data unless an independent add-on is explicitly connected. Tier access never grants permission to restricted venue or visitor data.

## Independent add-on metrics

### Ticketing, admissions, access, and attendance

Possible measures:

- tickets issued, sold, scanned, canceled, refunded, or unused;
- admissions, entries, re-entries, credentials, memberships, reservations, or timed-entry utilization;
- attendance by event, session, attraction, venue, gate, section, or admission method;
- sell-through, no-show, arrival distribution, or access exception.

Dependencies:

- authoritative ticketing/admissions/access source;
- source-specific definitions and reconciliation;
- privacy, rights, retention, and permission approval.

These are not available from screen delivery data alone.

### Queue, wait, occupancy, capacity, and footfall

Possible measures:

- measured wait time and distribution;
- queue length, arrival, abandonment, throughput, and service rate;
- occupancy, utilization, available capacity, entry pause, and crowding state;
- footfall by entrance, area, route, attraction, exhibit, event, or time period.

Dependencies:

- queue platform, sensor, camera, access count, ticketing, occupancy, or other authoritative source;
- clear distinction among measured, estimated, predicted, and manually reported values;
- privacy and accuracy review.

Manual operator-entered public guidance remains core but does not create measured operational analytics.

### Venue, cinema, show-control, collection, attraction, event, and sports systems

Possible measures:

- program/show/session readiness and execution;
- attraction uptime/downtime;
- exhibit or collection availability;
- event phase, production milestone, game/match state, result, or schedule performance;
- rights, sponsor, promoter, team, league, or production data quality.

Each metric depends on the connected source’s authority, rights, timing, vocabulary, and failure behavior.

### Maps, positioning, parking, transit, transport, and weather

Possible measures:

- route usage, destination requests, kiosk/mobile handoff, parking occupancy, transit/service state, travel disruption, and weather impact.

Do not infer real visitor movement from a published route. Measurement requires an approved source and privacy model.

### CRM, membership, donor, campaign, advertising, ecommerce, retail, and POS

Possible measures:

- campaign conversion, membership engagement, donations, merchandise or retail sales, offer redemption, and attributed revenue.

Vennusign publication or impression data alone does not prove conversion. Attribution logic, denominator, source, window, and rights must be explicit.

### Translation and AI

Possible measures:

- characters, words, tokens, requests, latency, cost, review rate, correction rate, language coverage, and quality workflow status.

AI-generated or translated content requires source, model/provider, review, approval, and correction state. Consumption and spend are limits/add-on usage, not core metrics.

## Campaign and content performance boundaries

Vennusign may report operational measures such as:

- content published;
- intended targets;
- delivery confirmation;
- display duration where player telemetry supports it;
- scheduled versus actual active time;
- content version and placement;
- direct interaction on an approved interactive Vennusign surface, when implemented and consented.

It must not label these as visitor views, impressions, attention, conversion, attendance, or revenue without an authoritative measurement source and defined methodology.

## Metric definition contract

Every metric definition includes:

- stable name and business question;
- primary classification;
- numerator and denominator;
- unit and aggregation rule;
- venue-local and reporting time zone;
- event-time versus processing-time basis;
- source and authority;
- refresh time and freshness threshold;
- included and excluded states;
- handling of unknown, stale, partial, conflict, override, and deleted data;
- privacy class and permitted roles;
- retention and export behavior;
- drill-down path to supporting records;
- version and change history.

A ratio with unknown denominator is not reported as zero or 100 percent. Partial source coverage must be visible in the same view as the metric.

## Time and scope dimensions

Supported planning dimensions may include:

- organization, brand, region, group, campus, district, venue, building, floor, zone, area, attraction, exhibit, habitat, event, session, queue, route, gate, section, stage, auditorium, theater, screen, field, court, track, or admission window;
- industry, primary subtype, descriptive trait, and mixed-industry context;
- content, notice, schedule, occurrence, experience, language, source, target, screen purpose, version, delivery result, operating state, and recovery action;
- local date, business day, operating day, event day, occurrence time, effective time, expiration time, publish time, delivery time, source time, and processing time.

Context switchers must not imply access to unauthorized venues or data.

## Roles and permissions

### Front-line operator

May view operational metrics for assigned venues/objects and current screens, notices, schedules, queues, sources, and delivery. Export and broad historical access are not implied.

### Content editor

May view content quality, language, source, schedule, notice, validation, version, and delivery metrics within assigned scope.

### Publisher or duty manager

May view target/delivery results, exceptions, approvals where configured, recovery, and current operational risk within assigned scope.

### Venue administrator

May view venue-level trends, screen/source health, user/workflow configuration, limits, and integration status as permitted.

### Portfolio or enterprise administrator

May view cross-venue analytics only for authorized groups. Tier access does not override venue, brand, operator, promoter, team, tenant, sponsor, rights-holder, contractor, or privacy boundaries.

### Billing or commercial administrator

May view plan, add-on, limit, consumption, and spend information without receiving content, visitor, or operational access automatically.

Analytics permissions should separate view, drill down, export, schedule, share, configure, administer sources, and delete.

## Privacy and restricted data

Default operational analytics should avoid personal visitor data.

Where external systems introduce personal or sensitive data, the model must define:

- purpose and lawful/contractual basis;
- data minimization and permitted fields;
- public, internal, restricted, sensitive, and prohibited classes;
- aggregation and anonymization thresholds;
- consent and opt-out where applicable;
- identity linkage and pseudonymization;
- geographic, biometric, camera, child, membership, credential, accessibility, payment, or protected data treatment;
- access logging and export restrictions;
- retention, correction, deletion, legal hold, and incident behavior;
- provider, region, cross-border, and data-residency requirements.

No camera, biometric, precise location, or visitor profiling capability is implied by this RWP.

## Retention, export, and deletion

Core operational history must retain enough information to explain current content, last-known-good versions, publication results, source freshness, correction, and recovery according to approved policy.

Candidate retained-history and export limits may vary by tier or add-on, but the owner must decide:

- minimum recovery and audit period available to all customers;
- retained history by content, screen, source, notice, schedule, event, queue, and delivery;
- export formats, row limits, frequency, recipients, and secure delivery;
- whether advanced raw data, scheduled exports, external BI, or long-term archives are tier/add-on candidates;
- behavior after downgrade, cancellation, source disconnect, venue removal, or contract termination;
- correction, deletion, legal hold, and backup behavior.

A downgrade must not immediately remove the data needed to keep current public operation safe or restore a recent successful version.

## Data-quality and trust presentation

Analytics views show:

- source and last refresh;
- coverage percentage and missing scope;
- stale, partial, conflict, manual override, estimated, predicted, and unknown indicators;
- definition/version and applicable time zone;
- numerator, denominator, exclusions, and drill-down;
- whether data is Vennusign-owned, customer-entered, inherited, integrated, or modeled;
- current fallback and next corrective action.

Do not use a single green status to hide incomplete coverage.

## Alerts and thresholds

Core alert candidates include:

- intended screen offline/outdated/unknown;
- publication partial/failed;
- high-priority notice not delivered;
- active public content expired or about to expire;
- source stale/disconnected/conflicting;
- schedule or state contradiction;
- no recovery version;
- missing target, language fallback, or required public field.

Advanced user-defined thresholds, escalations, schedules, recipients, portfolio rules, anomaly detection, and predictive alerts are tier candidates. Provider-backed alerts remain add-on dependent.

Thresholds must be explainable, scoped, permission-controlled, and recoverable. Alerts do not change public state automatically unless a separately approved automation exists.

## Subtype emphasis

Subtype changes recommended analytics emphasis only:

- cinema: screening/session delivery, auditorium/screen, format/accessibility, sold-out/delay, ticketing add-on;
- live performance: doors/start, room/stage, event change, promoter/production add-on;
- museum/gallery: exhibit/program availability, gallery routes, collection/event add-on;
- zoo/aquarium/park: attraction/habitat availability, talks/shows, weather/routes, attraction/queue add-ons;
- sports/esports: event, gate/section, transport, game/match, ticketing/access/sports add-ons;
- family entertainment: activity/session/lane, queue/capacity, POS/reservation add-ons;
- tour/landmark/visitor center: departure/entry window, route/language/weather, booking/map add-ons;
- festival/fair/touring: event-phase, temporary venue, route, sponsor/promoter, ticketing/event add-ons;
- mixed campus/district: cross-venue exceptions, inheritance, local control, and portfolio analytics.

Subtype never creates data, permission, entitlement, or commercial access.

## Impeccable planning result

Analytics surfaces should use `clarify` to state the business question, metric definition, source, coverage, and uncertainty; `shape` to keep urgent operational metrics above optional analysis; `harden` for zero, unknown, stale, partial, conflicting, permission, long-name, localization, mobile, keyboard, assistive-technology, and export-failure states; and `polish` to preserve the approved Sky Blue administrative direction without decorative chart noise.

Charts and tables require accessible names, textual summaries, keyboard navigation, non-color-only encoding, local date/time clarity, responsive alternatives, and export parity.

## Validation and handoff

This model covers core screen, publication, delivery, notice, content freshness, schedule quality, recovery, language/accessibility, attendance, queue/wait, capacity, attraction/show/exhibit, campaign, multi-venue, external-source, privacy, retention, export, permission, tier, add-on, and limit requirements.

No implementation is authorized. RWP-00.74 owns final Entertainment & Attractions profile validation, shared-record reconciliation, and handoff to cross-industry consolidation.