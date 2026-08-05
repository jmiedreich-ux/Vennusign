# RWP-00.73 — Entertainment & Attractions KPIs & Analytics

## Status

Complete in this proposed merge state.

## Issue

- #548

## Dependency verification

- RWP-00.72 merged through PR #606.
- Issue #547 is closed.
- RWP-00.73 is the first unfinished approved Entertainment & Attractions item.

## Objective

Define the Entertainment & Attractions KPI and analytics model, including core screen/publication/notice/freshness measures, schedule and operating-quality analytics, attendance/queue/capacity/experience/campaign/multi-venue boundaries, external-source dependencies, privacy, retention, export, permissions, tiers, add-ons, and limits.

## Delivered

- Added `track0/industries/entertainment-attractions-kpis-analytics.md`.
- Defined analytics principles that separate operational truth, delivery, audience measurement, conversion, prediction, and external data.
- Defined core screen-state, publication/delivery, public-content freshness, notice/disruption, schedule/occurrence, recovery, and language/accessibility KPIs using Vennusign-owned data.
- Defined tier candidates for trend, workflow, localization, campaign, portfolio, governance, and enterprise analytics.
- Defined independent add-on analytics for ticketing, admissions, access, attendance, queue, wait, occupancy, capacity, footfall, venue/cinema/show-control/collection/attraction/event/sports systems, maps, transport, weather, CRM, membership, donor, advertising, ecommerce, retail, POS, translation, and AI.
- Defined metric-definition contracts, time/scope dimensions, source/freshness/coverage presentation, roles and permissions, privacy, retention, export, deletion, alerts, thresholds, limits, and subtype emphasis.
- Applied project-local Impeccable `clarify`, `shape`, `harden`, and bounded `polish` guidance.

## Validation

- Reviewed against issue #548, RWP-00.63–00.72, `AGENTS.md`, the Track 0 execution packet, and project-local Impeccable guidance.
- Publication acceptance is not delivery; delivery is not visitor view, attendance, conversion, or revenue.
- Core operational metrics remain available without a paid integration.
- External attendance, ticketing, queue, occupancy, footfall, conversion, and revenue measures require authoritative add-on sources.
- Estimates and predictions remain distinct from measured values.
- Unknown, stale, partial, conflicting, overridden, and not-applicable data remain visible.
- Permissions, privacy, tier, add-on, limit, source, and rollout remain separate.
- Documentation-only scope; no product behavior or implementation.
- Azure SQL and all integration/external-system tests remain skipped.

## Completion checkpoint

Shared living-record updates are queued for the RWP-00.74 final Entertainment & Attractions checkpoint. RWP-00.74 is the exact next item.

## Handoff

After merge, issue closure, default-branch verification, and claim release, execute **RWP-00.74 — Entertainment & Attractions Validation, Review & Handoff** (#549).