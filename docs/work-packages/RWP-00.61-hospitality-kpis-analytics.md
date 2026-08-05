# RWP-00.61 — Hospitality KPIs & Analytics

## Status

Complete in this proposed merge state.

## Issue

- #536

## Objective

Define the Hospitality KPI and analytics planning contract. Cover required screen, publication, notice, object, event, wayfinding, source, and recovery visibility; optional advanced property and property-group analytics; external-data dependencies; privacy, retention, correction, export, and permission concerns; and clear separation of core, tier, add-on, limits, state, source, privacy, and rollout. Documentation only.

## Dependency verified

- RWP-00.60 is merged, verified, closed, and released.
- The approved Hospitality dashboard, onboarding, tier, classification, capability, operating, terminology, subtype, and industry records are authoritative inputs.
- RWP-00.62 — Hospitality Validation, Review & Handoff (#537) is next.

## Delivered

- Added `track0/industries/hospitality-kpis-analytics.md`.
- Defined required current screen, publication, notice, object, event, wayfinding, source, language, and recovery visibility.
- Defined publication latency, delivery coverage, stale-source, override, correction, expiry, retry, and restoration measures.
- Defined amenity/outlet/service, meeting/event, wayfinding, and property-group analysis candidates.
- Defined PMS/CRS, event, room-booking, occupancy, POS, sensor, access, transport, survey, loyalty, and other external-data dependencies.
- Required source, authority, freshness, latency, coverage, completeness, confidence, effective time, local time zone, operating day, formula, exclusions, correction, and reconciliation for every metric.
- Defined privacy-safe aggregation, minimum necessary data, permission scope, retention, deletion, correction, export, scheduled-report, and disconnected-add-on behavior.
- Classified core operational visibility, tier candidates, independent add-ons, limits, permissions, product/domain state, privacy, source, and rollout separately.
- Applied project-local Impeccable `clarify`, `harden`, `adapt`, and bounded `persuade` guidance.

## Core protection

Current screen, publication, notice, property-object, meeting/event, wayfinding, language, source, and recovery status remain core operational visibility. A connected add-on must expose health and freshness without requiring an additional analytics tier. Advanced trends, benchmarking, forecasting, attribution, recommendations, long retention, scheduled reports, and large exports may be tier candidates or limits.

## Truth and source rules

Unknown, unavailable, not configured, stale, disconnected, restricted, partial, conflicting, and zero are distinct. Manual states may be reported as operator-recorded activity but cannot be presented as verified occupancy, demand, revenue, attendance, wait time, route use, engagement, satisfaction, or guest impact.

Every metric must identify source, authority, effective/recorded/publication/delivery time, freshness, coverage, completeness, exclusions, correction, formula version, permission, and reconciliation.

## Privacy and permission result

Analytics defaults to property, object, screen, content, language, source, event, time period, and aggregate operational outcomes. Guest identity, room/stay/reservation/contact/payment/access/location/loyalty/service detail, employee-level performance, private event/contract/attendee data, raw sensor/access/positioning history, and sensitive safety/security information require approved purpose, minimum necessary scope, access, retention, and aggregation.

View, compare, export, schedule, share, administer, and delete are separate permissions. Property-group visibility does not grant restricted property or guest-data access.

## Classification result

- Current operational and delivery truth: core visibility.
- Advanced trends, portfolio comparison, governance, forecasting, optimization, and scheduled reporting: tier candidates.
- External systems and imported data: independent add-on candidates.
- Properties, rows, refreshes, retention, exports, storage, reports, and consumption: limits.
- Authority: permission.
- Represented values, source, freshness, coverage, and formula version: product/domain state.
- Temporary release control: rollout flag.

## Impeccable result

Analytics remains secondary to urgent operational tasks. Future surfaces lead with plain-language questions, actionable exceptions, scope, source, freshness, coverage, limitations, and recovery rather than decorative KPI grids or false precision. Accessibility, keyboard use, non-color-only status, 200% zoom, localization, right-to-left layouts, long names, responsive presentation, local date/time, accessible exports, and the approved Sky Blue administrative direction are required.

## Validation

Documentation-only review confirmed:

- all issue #536 concerns are covered;
- required operational visibility works without external data or premium analytics;
- external data is not inferred when absent;
- metric specification and reconciliation requirements are explicit;
- privacy, permission, retention, correction, deletion, export, and scheduled-report boundaries are explicit;
- core, tier, add-on, limit, state, source, privacy, permission, and rollout remain separate;
- RWP-00.62 is the exact next Hospitality item.

GitHub Actions is authoritative for lightweight documentation validation on the exact pull-request head.

## Skipped under standing owner instruction

All integration and external-system testing and all product implementation, including telemetry, analytics pipelines, data warehouse, dashboards, reports, charts, alerts, APIs, schemas, migrations, permissions, privacy systems, billing, entitlements, limits, PMS/CRS/event/occupancy/POS/sensor/access/transport/survey/loyalty connections, AI, hardware, connectivity, monitoring, and managed services.

## Shared-record checkpoint

Semantic updates for `tracker/assignments.json`, `PROJECT_STATUS.md`, `ai/handoffs/current.md`, and `track0/CAPABILITY_MATRIX.md` remain queued for a single final RWP-00.62 reconciliation and transactional write window against current `master`.

## Exact next action

After this RWP is merged, verified on `master`, issue #536 is closed, and the claim is released, execute **RWP-00.62 — Hospitality Validation, Review & Handoff** (#537).
