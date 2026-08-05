# RWP-00.43 — Food Truck & Concession Required Capabilities

## Status

Complete in this proposed merge state.

## Issue

- #518

## Objective

Define the smallest viable required capability set for Food Truck & Concession operations while preserving the inherited Restaurant baseline and keeping industry, subtype, permissions, product state, commercial entitlement, add-ons, limits, and rollout flags separate.

## Dependency verified

- RWP-00.42 is merged, verified on `master`, closed, and released.
- No competing RWP-00.43 branch or pull request existed when this work began.
- The approved Food Truck & Concession profile and operating-characteristics model remain authoritative.

## Delivered

- Defined required menu and offer management.
- Preserved manual Quick Update, availability, sell-out, pause, reopen, and closure control as core.
- Defined current location, event, host, service-period, and disruption communication.
- Defined required screen pairing, explicit targeting, preview, immediate publish, and cross-location safety.
- Defined per-target delivery confirmation, offline/outdated/unknown states, retry, restore, and conflict recovery.
- Defined required queue, pickup, collection, lane, and service-window guidance as manual core communication.
- Defined minimum permissions and object-level authority boundaries.
- Defined first-use, empty, loading, validation, permission, success, failure, partial-delivery, offline, stale-source, conflict, and recovery states.
- Applied project-local Impeccable `shape` and `harden` guidance as planning requirements.
- Classified advanced scheduling and orchestration as tier candidates, integrations as add-on candidates, quantities as limits, and internal staging as rollout.

## Boundaries

Documentation and planning only. No product, UI, API, schema, migration, billing, entitlement, permission, feature-gate, limit, rollout, analytics, routing, ordering, payment, inventory, event-management, host-system, queue-measurement, hardware, or integration behavior was implemented.

Integration and external-system tests were not applicable and remain skipped under the standing project rule.

## Validation

The durable capability document covers every issue-listed concern and preserves ordinary single-unit operation without premium dependencies. Required core behavior remains separated from permissions, state, tiers, add-ons, limits, and rollout controls.

## Handoff

The next sequential item is **RWP-00.44 — Food Truck & Concession Optional Capabilities** (#519). It must not begin until this RWP is merged, verified on `master`, issue #518 is closed, and the claim is released.
