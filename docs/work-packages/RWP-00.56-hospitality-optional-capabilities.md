# RWP-00.56 — Hospitality Optional Capabilities

## Status

Complete in this proposed merge state.

## Issue

- #531

## Objective

Define optional Hospitality capabilities and integration candidates without weakening the RWP-00.55 required manual core. Separate tier candidates, independent add-ons, permissions, product state, limits, and rollout controls. Documentation only.

## Dependency verified

- RWP-00.55 is merged, verified, closed, and released.
- Restaurant remains the canonical baseline.
- The merged Hospitality industry, subtype, terminology, operating, and required-capability records are authoritative.
- RWP-00.57 — Hospitality Capability Classification (#532) is next.

## Delivered

- Added `track0/industries/hospitality-optional-capabilities.md`.
- Defined optional property-management, event, room-booking, transport, parking, access, guest-service, and local-system synchronization.
- Defined advanced wayfinding, mapping, positioning, personalization, brand libraries, property-group coordination, campaigns, approvals, localization, AI, analytics, enterprise identity, managed hardware, connectivity, monitoring, and support candidates.
- Classified each concern as a tier candidate, independent add-on candidate, or a combined boundary where Vennusign workflow and an external connection must remain separate.
- Defined candidate limits without treating them as capabilities.
- Preserved permissions, product state, privacy, source authority, commercial access, and rollout as separate concepts.
- Required manual fallback, source freshness, conflict handling, disconnect behavior, delivery confidence, correction, and restoration for every optional candidate.
- Applied project-local Impeccable `shape`, `clarify`, and `harden` guidance to future packaging and setup surfaces.

## Classification result

**Tier candidates** include advanced wayfinding, brand libraries, centralized property-group coordination, campaigns, approval workflow, advanced localization workflow, advanced analytics, enterprise administration, and selected advanced operational workflows.

**Independent add-on candidates** include external property, event, room, transport, guest-service, access, gaming, mapping, positioning, emergency, weather, translation, AI, identity-provider, managed hardware, connectivity, monitoring, and related connections or services.

**Limits** may apply to properties, groups, buildings, accommodations, venues, outlets, amenities, services, events, meeting spaces, screens, devices, users, roles, languages, integrations, sources, templates, assets, campaigns, reports, history, storage, transactions, messages, requests, tokens, data, or spend.

Permissions control authority. Represented configuration, connection, source, freshness, conflict, content, audience, target, approval, schedule, delivery, consumption, and recovery values remain product/domain state. Rollout controls remain internal.

## Core protection

The following remain core and cannot be removed by optional packaging:

- manual property and guest information;
- manual hours, states, notices, events, directories, and wayfinding;
- basic manually authored language variants;
- explicit targets and previews;
- publishing and delivery confidence;
- offline, outdated, stale-source, and conflict awareness;
- correction, expiration, supersession, retry, and restoration;
- permissions and privacy-safe audiences;
- current operational state and last known good content.

## Impeccable result

Future optional-capability surfaces are **Operate** experiences. They lead with the customer outcome, keep included manual actions visible, and distinguish missing entitlement, missing permission, disconnected source, exceeded limit, unsupported object, and rollout state.

Setup and use must show required data, scope, authority, audience, privacy, source health, limits, consumption, failures, fallback, cancellation, downgrade, export, deletion, correction, and recovery. Required accessibility and responsive states include keyboard, assistive technology, non-color status, 200% zoom, long names, localization expansion, right-to-left readiness, phone through large desktop, and the approved Sky Blue administrative direction.

No UI or product implementation was introduced.

## Validation

Documentation-only review confirmed:

- every issue-listed optional area is addressed;
- the required manual core remains unchanged;
- external connections remain independently selectable add-on candidates;
- Vennusign-authored advanced workflows remain tier candidates where appropriate;
- permissions, state, privacy, entitlement, add-on, limit, and rollout remain separate;
- every optional candidate requires fallback and safe failure behavior;
- RWP-00.57 is next.

GitHub Actions is authoritative for lightweight documentation validation on the exact pull-request head.

## Skipped under standing owner instruction

All integration and external-system testing and all product implementation, including UI, API, schema, migrations, billing, entitlements, permissions, privacy systems, localization, analytics, PMS, event, room-booking, transport, POS, guest-service, access, gaming, mapping, emergency, weather, translation, AI, identity, hardware, connectivity, monitoring, and managed services.

## Exact next action

After this RWP is merged, verified on `master`, issue #531 is closed, and the claim is released, execute **RWP-00.57 — Hospitality Capability Classification** (#532).

RWP-00.57 must consolidate every Hospitality concern into exactly one primary Track 0 classification, resolve duplicates and ambiguous tier/add-on boundaries, remain documentation-only, and hand off to RWP-00.58.