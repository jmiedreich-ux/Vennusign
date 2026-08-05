# RWP-00.55 — Hospitality Required Capabilities

## Status

Complete in this proposed merge state.

## Issue

- #530

## Objective

Define the smallest viable core capability set for Hospitality guest information, notices, amenity/outlet/service hours and availability, events and meetings, wayfinding, property context, basic multilingual content, explicit targeting, publishing, delivery confidence, offline/outdated awareness, recovery, permissions, privacy-safe audiences, and required states. Documentation only.

## Dependency verified

- RWP-00.54 is merged, verified, closed, and released.
- Restaurant remains the canonical baseline.
- The merged Hospitality industry, subtype, terminology, and operating records are authoritative.
- RWP-00.56 — Hospitality Optional Capabilities (#531) is next.

## Delivered

- Added `track0/industries/hospitality-required-capabilities.md`.
- Defined eleven required capability groups: property/context information; guest notices and state communication; amenity/service/outlet hours and availability; meetings/events/directories; manual wayfinding; basic multilingual and accessible content; explicit screen targeting and preview; publishing and delivery confidence; offline/outdated/conflict/recovery awareness; permissions and privacy-safe audiences; and required operational states.
- Kept essential manual daily operation available without premium tiers or paid integrations.
- Separated content state, source, freshness, audience, target, delivery, permission, entitlement, add-on, limit, privacy, and rollout.
- Defined safe failure, partial-delivery, correction, expiration, supersession, retry, undo, and restoration behavior.
- Defined what remains outside core, including automated synchronization, guest personalization, live operational data, advanced workflow, premium analytics, optimization, and AI.
- Applied project-local Impeccable `shape`, `clarify`, and `harden` guidance.

## Core classification result

The eleven capability groups are **core capabilities**. Represented property, hierarchy, audience, language, source, freshness, hours, schedule, notice, operating state, target, delivery, and content-version values are **product/domain state**. Authority is a **permission**. Advanced workflow, governance, coordination, analytics, monitoring, localization, personalization, and optimization remain **tier candidates**. External systems and automation remain **add-on candidates**. Counts remain **limits** and temporary delivery controls remain **rollout flags**.

## Impeccable result

Future Hospitality Operate surfaces must present the smallest task-relevant capability subset. Quick communication, hours, event changes, wayfinding changes, screen health, and portfolio views each require explicit scope, state, effective time, public wording, targets, preview, delivery result, and recovery.

Required states include first use, empty, loading, permission, validation, stale source, source conflict, offline, outdated, publish failure, partial delivery, scheduled, active, expired, superseded, restored, success, undo, long names, overnight dates, language expansion, 200% zoom, keyboard, assistive technology, and non-color-only status. Preserve the approved Sky Blue administrative direction.

No UI or implementation contract was created.

## Validation

Documentation-only review confirmed:

- every issue-listed essential capability is classified as core;
- manual daily operation does not require a paid integration;
- permissions do not determine commercial access;
- state, authority, privacy, entitlement, add-on, limit, and rollout remain separate;
- no guest-specific private data is assumed public;
- advanced and automated capabilities are explicitly deferred;
- RWP-00.56 is next.

GitHub Actions is authoritative for lightweight documentation validation on the exact pull-request head.

## Skipped under standing owner instruction

All integration and external-system testing and all product implementation, including UI, API, schema, migrations, billing, entitlements, permissions, privacy systems, localization, analytics, PMS, event, room-booking, transport, POS, guest-service, access, gaming, mapping, emergency, weather, translation, AI, hardware, and integrations.

## Exact next action

After this RWP is merged, verified on `master`, issue #530 is closed, and the claim is released, execute **RWP-00.56 — Hospitality Optional Capabilities** (#531).

RWP-00.56 must define optional advanced capabilities and integration candidates without weakening the required core, remain documentation-only, and hand off to RWP-00.57.