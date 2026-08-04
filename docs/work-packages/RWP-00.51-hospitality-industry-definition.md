# RWP-00.51 — Hospitality Industry Definition

## Status

Complete in this proposed merge state.

## Issue and branch

- Issue: #526
- Branch: `rwp/00.51-hospitality-industry-definition`
- Mode: Sequential within the independently approved Hospitality Track 0 stream

## Objective

Define the canonical Hospitality profile as a delta from the approved Restaurant baseline, including customer outcomes, lodging and property boundaries, organization and venue behavior, initial capability classifications, and UI-facing planning guardrails.

## Delivered

- Added `track0/industries/hospitality.md`.
- Defined lodging-led property boundaries and mixed-property behavior.
- Documented inherited Restaurant behavior and meaningful differences only.
- Established property, area, outlet, room, event, amenity, service-window, closure, and relocation values as product/domain state when represented.
- Kept manual guest information, wayfinding, event, amenity, service, targeting, publishing, delivery confirmation, offline awareness, and recovery core.
- Kept permissions, privacy scope, states, entitlements, add-ons, limits, and rollout flags distinct.
- Qualified Restaurant menu semantics for mixed food-and-beverage outlets without making menus the primary Hospitality content model.
- Consulted the project-local Impeccable skill and `shape` guidance for operator and guest-facing planning.
- Updated the Track 0 capability matrix and living status/handoff records.

## Impeccable planning result

- Administrative surfaces use Operate mode and prioritize exact scope, current operational information, delivery state, and recovery.
- Guest-facing information and wayfinding use Read mode and prioritize safety, destination, direction, event or service state, time, and next action.
- Later specifications must cover realistic small-property through multi-property ranges and first-run, empty, active, changed, delayed, relocated, full, unavailable, closed, maintenance, emergency, offline, outdated, permission, privacy, failed-delivery, success, and recovery states.
- Accessibility, localization, restrained motion, distance legibility, mobile property use, mixed display orientations, and unfamiliar-guest navigation are binding conditions.
- Administrative surfaces preserve the approved Sky Blue direction.

## Validation

- Reviewed against `AGENTS.md` and the Track 0 execution packet.
- Restaurant inheritance remains explicit and only meaningful deltas are repeated.
- Each initial concern has one primary Track 0 classification.
- External references are limited to U.S. Census Bureau boundary evidence.
- Documentation-only lightweight review; no runtime build is applicable.
- Azure SQL, external-service, credentialed, hosted-infrastructure, container, device, signing/store, cross-system, and all other integration-type tests were skipped under the standing owner instruction.

## Not performed

- No product, UI, API, schema, migration, billing, entitlement, feature-gate, rollout, privacy-system, or integration implementation.
- No subtype, terminology, detailed operating, packaging, onboarding, dashboard, or analytics decisions beyond the boundary required by this RWP.
- RWP-13.06 and Phase 14+ remain paused.

## Handoff

After this PR is merged, verified on `master`, issue #526 is closed, and the claim is released, the next Hospitality item is **RWP-00.52 — Venue Subtypes** (#527).
