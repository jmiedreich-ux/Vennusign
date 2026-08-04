# Vennusign Session Handoff

## Current State

- Item: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Mode: owner-led planning with independently scheduled native-industry streams; implementation paused
- Active implementation WP/RWP: none
- Phase 14 and later: paused
- RWP-13.06: paused pending the owner-approved Track 0 model
- Restaurant: canonical approved baseline
- Bar, Brewery & Nightlife: RWP-00.15 industry definition merged; RWP-00.16 is next
- Café, Bakery & Dessert: RWP-00.27 industry definition merged; RWP-00.28 is next
- Food Truck & Concession: RWP-00.39 industry definition complete in this proposed merge state; RWP-00.40 is next

## Why Track 0 Exists

The completed WP/RWP inventory does not yet produce consistent end-to-end customer journeys. Vennusign has also mixed domain state, permissions, commercial entitlements, quantity limits, add-ons, and internal rollout flags under the broad idea of “features.” Track 0 resolves that product architecture before more onboarding or feature implementation.

Every capability must have one primary classification:

1. Core capability
2. Permission
3. Product/domain state
4. Tier entitlement
5. Independent add-on
6. Usage or quantity limit
7. Internal rollout flag

## Food Truck & Concession Definition Result

The canonical profile is documented at `track0/industries/food-truck-concession.md` as a delta from Restaurant.

It covers mobile, temporary, event-based, and concession-led food-service concepts where the defining needs include current operating location or event information, temporary service windows, rapid setup and teardown, compact menus, queue surges, sell-outs, outdoor readability, intermittent connectivity, explicit screen targeting, delivery confidence, and recovery.

Initial native concepts include food trucks, trailers, carts, mobile snack or beverage units, temporary festival and market vendors, kiosks, booths, stalls, temporary concession stands, and host-venue concessions where stand- or unit-level food service is the primary Vennusign context. Exact subtype boundaries are deferred to RWP-00.40.

Current operating location, event, service window, relocation, closure, and related operational values are product/domain state when represented. Manual menu availability, location and closure communication, screen targeting, publishing, delivery confirmation, offline awareness, and recovery remain core capabilities. Organization primary industry and venue, unit, stand, or subtype remain product/domain configuration rather than commercial entitlements. Counts remain limits. Automatic POS, order, inventory, route, event, host-venue, or location synchronization remains a later integration-packaging question.

## Impeccable Planning Result

The project-local Impeccable skill and `shape` guidance were consulted for future UI-facing work.

- Operator surfaces use Operate mode and prioritize location, readiness, menu state, intended targets, delivery state, and recovery.
- Guest-facing operational screens use Read mode and prioritize location or stand identity, open/closed state, current offerings, prices, and collection instructions.
- Later specifications must cover realistic one-unit through multi-unit ranges and first-run, no-location, upcoming, setup, ready, open, paused, limited, sold-out, relocated, canceled, closed, offline, outdated, permission, publish-failure, success, and recovery states.
- Outdoor glare, weather, vibration, crowds, long viewing distances, touch use, small workspaces, and intermittent connectivity are binding conditions.
- Accessibility requires non-color status communication, strong distance legibility, restrained motion, and plain recovery guidance.
- Preserve the Sky Blue direction for Vennusign administrative surfaces.

## Exact Next Food Truck & Concession Action

After RWP-00.39 is merged, verified on `master`, issue #514 is closed, and the claim is released, execute **RWP-00.40 — Food Truck & Concession Venue Subtypes** (#515).

RWP-00.40 must:

- define food truck, trailer, cart, kiosk, stadium or arena concession, festival vendor, market stall, pop-up, catering concession, and hybrid subtypes;
- establish inclusion, exclusion, and ambiguous-boundary rules;
- distinguish operating model from physical form when classifying semi-permanent or long-term units;
- map meaningful subtype differences without duplicating Restaurant inheritance;
- define selection, change, and mixed-concept behavior;
- keep subtypes separate from tiers, entitlements, permissions, and limits;
- consult Impeccable for any UI-facing subtype selection or change-flow planning;
- remain documentation-only and hand off to RWP-00.41.

## Parallel-Stream Rule

The owner approved independently scheduled native-industry streams. Each industry remains sequential inside its own approved RWP range. Restaurant is the canonical baseline, only merged documents are authoritative, and no two runs may concurrently modify the same shared controlled file.

## Boundaries

- Do not start product implementation from Track 0 issues.
- Do not resume RWP-13.06 until Track 0 produces an owner-approved capability and packaging model.
- Do not start Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, or rollout controls during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
