# RWP-00.39 — Food Truck & Concession Industry Definition

## Status

Complete in this proposed merge state.

## Issue and branch

- Issue: #514
- Branch: `rwp/00.39-food-truck-concession-industry-definition`
- Mode: Sequential within the independently approved Food Truck & Concession Track 0 stream

## Objective

Define the canonical Food Truck & Concession profile as a delta from the approved Restaurant baseline, including customer outcomes, mobile and temporary venue boundaries, mixed-organization behavior, initial capability classifications, and UI-facing planning guardrails.

## Delivered

- Added `track0/industries/food-truck-concession.md`.
- Defined mobile, temporary, event-based, and host-venue concession boundaries.
- Documented inherited Restaurant behavior and meaningful differences only.
- Established current operating location, event, service window, relocation, closure, and related values as product/domain state.
- Kept manual menu availability, location and closure communication, publishing, delivery confirmation, offline awareness, and recovery core.
- Kept permissions, states, entitlements, add-ons, limits, and rollout flags distinct.
- Defined mixed-organization, host-venue, unit, stand, and venue behavior without deciding later limit scopes.
- Consulted the project-local Impeccable skill and `shape` guidance for operator and guest-facing planning.
- Updated the Track 0 capability matrix and living status/handoff records.

## Impeccable planning result

- Operator surfaces use Operate mode and prioritize location, readiness, availability, target confirmation, delivery state, and recovery.
- Guest-facing operational screens use Read mode and prioritize current location or stand identity, open/closed status, current offerings, prices, and collection instructions.
- Outdoor glare, weather, vibration, queues, long viewing distances, touch use, and intermittent connectivity are binding design conditions.
- Later specifications must include first-run, no-location, upcoming, setup, ready, open, paused, limited, sold-out, relocated, canceled, closed, offline, outdated, failed-delivery, permission, success, and recovery states.
- Administrative surfaces preserve the approved Sky Blue direction.

## Validation

- Reviewed against `AGENTS.md` and the Track 0 execution packet.
- Restaurant inheritance remains explicit and only meaningful deltas are repeated.
- Each initial concern has one primary Track 0 classification.
- External references are limited to U.S. Census Bureau boundary evidence.
- Documentation-only lightweight review; no runtime build is applicable.
- Azure SQL, external-service, credentialed, hosted-infrastructure, container, device, signing/store, cross-system, and all other integration-type tests were skipped under the standing owner instruction.

## Not performed

- No product, UI, API, schema, migration, billing, entitlement, feature-gate, rollout, or integration implementation.
- No subtype, terminology, detailed operating, packaging, onboarding, dashboard, or analytics decisions beyond the boundary required by this RWP.
- RWP-13.06 and Phase 14+ remain paused.

## Handoff

After this PR is merged, verified on `master`, issue #514 is closed, and the claim is released, the next Food Truck & Concession item is **RWP-00.40 — Venue Subtypes** (#515).
