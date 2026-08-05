# RWP-00.49 — Food Truck & Concession KPIs & Analytics

## Status

Complete in this proposed merge state.

## Issue

- #524

## Objective

Define Food Truck & Concession operational KPIs and analytics boundaries for location/event performance, service windows, promotions, multi-unit comparison, external-data dependencies, privacy, retention, export, and core-versus-premium treatment.

## Dependency verified

- RWP-00.48 is merged, verified on `master`, closed, and released.
- The existing RWP-00.49 branch was resumed; no competing pull request existed when documentation began.

## Delivered

- Defined core operational visibility for content freshness, publication, screen health, rapid availability changes, service state, location/event context, and recovery.
- Defined location, stop, event, host, pitch, service-point, and service-window dimensions without treating them as interchangeable.
- Defined bounded performance families for menu/availability, promotions, service windows, screens/publication, locations/events, and multi-unit operations.
- Distinguished directly observed Vennusign activity from imported POS, ordering, payment, inventory, event, footfall, traffic, weather, queue, loyalty, and campaign data.
- Prohibited inferred sales, demand, wait time, attendance, conversion, inventory, or attribution when an authoritative source is absent.
- Defined source, freshness, coverage, confidence, reconciliation, partial-data, stale-data, conflict, disconnect, and restoration requirements.
- Defined privacy-safe aggregation, role and organization scope, minimum necessary data, retention, deletion, export, and audit expectations.
- Classified essential operational status and recent publication/recovery visibility as core; advanced trends, benchmarking, forecasting, attribution, optimization, cross-unit analysis, extended retention, and scheduled exports as tier candidates; external data connections as add-on candidates; row, site, retention, frequency, and export quantities as limits.
- Applied project-local Impeccable `operate`, `clarify`, `harden`, and bounded `persuade` guidance so metrics remain actionable, honest, accessible, and secondary to urgent operational tasks.

## Boundaries

Documentation and planning only. No analytics UI, telemetry, event tracking, data warehouse, API, schema, migration, billing, entitlement, limit, POS, ordering, payment, inventory, footfall, weather, traffic, queue, loyalty, campaign, AI, or integration behavior was implemented.

Integration and external-system tests were not applicable and remain skipped under the standing project rule.

## Validation

Every issue-listed concern is covered. Core operational visibility remains available without external data or premium packaging. Derived metrics require declared formulas and authoritative inputs. Missing, stale, partial, disconnected, or unsupported data cannot be presented as zero or complete. State, permission, tier, add-on, limit, privacy, source, and rollout remain separate.

## Handoff

The next sequential item is **RWP-00.50 — Food Truck & Concession Validation, Review & Handoff** (#525). It must not begin until this RWP is merged, verified on `master`, issue #524 is closed, and the claim is released.
