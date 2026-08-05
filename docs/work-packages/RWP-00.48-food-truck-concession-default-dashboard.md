# RWP-00.48 — Food Truck & Concession Default Dashboard

## Status

Complete in this proposed merge state.

## Issue

- #523

## Objective

Define the default role-aware, mobile-first dashboard for current location/event status, rapid menu and sell-out updates, screen and publish health, connectivity/recovery, service-window actions, and multi-unit visibility.

## Dependency verified

- RWP-00.47 is merged, verified on `master`, closed, and released.
- No competing RWP-00.48 branch or pull request existed when this work began.

## Delivered

- Defined an information hierarchy led by current operation context, urgent exceptions, rapid service controls, menu/availability, screen/publication health, guest guidance, upcoming work, and optional multi-unit overview.
- Defined role-aware presentation for operators, editors, publishers/managers, administrators/owners, and limited host/sponsor collaborators.
- Defined phone and desktop priorities without hiding core actions.
- Defined first-use, empty, permission, tier, add-on, integration, limit, service, screen, publication, stale/conflict, partial-success, failure, and recovery states.
- Kept manual core controls visible even when integrations or premium outcomes exist.
- Applied project-local Impeccable Operate, shape, harden, and polish planning guidance.

## Boundaries

Documentation and planning only. No dashboard UI, product behavior, API, schema, migration, analytics, billing, entitlement, integration, screen-player, or hardware implementation was introduced.

Integration and external-system tests were not applicable and remain skipped under the standing project rule.

## Validation

Every issue-listed dashboard concern is covered. The hierarchy is exception-first and mobile-first, targeting and delivery remain explicit, and operational state remains separate from permission, tier, add-on, limit, connection, and rollout conditions.

## Handoff

The next sequential item is **RWP-00.49 — Food Truck & Concession KPIs & Analytics** (#524). It must not begin until this RWP is merged, verified on `master`, issue #523 is closed, and the claim is released.
