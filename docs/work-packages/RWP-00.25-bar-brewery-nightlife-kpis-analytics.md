# RWP-00.25 — Bar, Brewery & Nightlife KPIs & Analytics

## Status

Complete in this proposed merge state.

## Issue

#500

## Scope completed

- Defined core operational KPIs for screen health, publication, target-level delivery, content freshness, current service, exceptions, correction, retry, and restoration.
- Defined advanced content, happy-hour, release, event, entertainment, daypart, campaign, workflow, venue, and organization analytics candidates.
- Defined subtype-aware analytics needs for every approved Bar subtype and a neutral fallback.
- Identified POS, inventory, tap, reservation, ticketing, guest-list, access, sports/event, footfall, CRM, loyalty, AI, BI, and related external-data dependencies.
- Separated core operational visibility, tier analytics, independent analytics/data add-ons, product state, permissions, limits, privacy, and rollout controls.
- Defined metric contracts for grain, dimensions, source authority, venue-local time, operating day, freshness, quality, units, unknown/partial/stale behavior, permission, retention, and export.
- Defined privacy, audience, correction, downgrade, cancellation, and data-retention safeguards.
- Defined accessible, actionable dashboard/report presentation and explicit owner-decision points.
- Applied project-local Impeccable `clarify` and `harden` guidance.

## Validation

Reviewed against issue #500, the merged Bar profile through RWP-00.24, the Restaurant baseline, Track 0 classification rules, and the queued shared-file protocol. Core operational visibility is separated from advanced or externally sourced analytics. Data dependencies and classification are explicit. Documentation only; integration and external-system tests remain skipped.

## Handoff

After merge, verification, issue closure, and claim release, execute **RWP-00.26 — Bar, Brewery & Nightlife Validation, Review & Handoff** (#501). Shared-record changes remain queued for that final Bar completion checkpoint.