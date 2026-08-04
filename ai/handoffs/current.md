# Vennusign Session Handoff

## Current State

- Item: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Mode: owner-led planning with independently scheduled native-industry streams; implementation paused
- Active implementation WP/RWP: none
- Phase 14 and later: paused
- RWP-13.06: paused pending the owner-approved Track 0 model
- Restaurant: canonical approved baseline
- Bar, Brewery & Nightlife: RWP-00.15 industry definition merged
- Café, Bakery & Dessert: RWP-00.27 industry definition complete in this proposed merge state

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

## Café, Bakery & Dessert Definition Result

The canonical profile is documented at `track0/industries/cafe-bakery-dessert.md` as a delta from Restaurant.

It covers guest-facing concepts centered on prepared nonalcoholic beverages, bakery products, desserts, specialty snacks, and closely related counter-service experiences. Initial native concepts include cafés, coffee and tea shops, bakeries and patisseries, baked-specialty shops, dessert and frozen-dessert shops, juice and smoothie bars, and related hybrids.

Meaningful differences include:

- stronger product-size-option presentation;
- batch, freshness, limited-quantity, and sold-out rhythms;
- queue and counter-service scanning needs;
- preorder and pickup communication;
- prepared-service and packaged-retail overlap;
- early, seasonal, and demand-driven service periods;
- menu, display-case, pickup, queue, retail, promotional, and venue-information screen purposes.

Manual sold-out and available-again changes remain a core capability acting on product state. Batch, freshness, limited-quantity, and expected-return values are product/domain state when represented. Industry and venue subtype remain product/domain configuration rather than commercial entitlements. Automatic POS, order, inventory, production, or pickup synchronization remains a future integration-packaging question.

## Impeccable Planning Result

The project-local Impeccable skill and `shape` guidance were consulted for future UI-facing work.

- Operator surfaces use Operate mode with rapid scanning, confident state changes, publishing feedback, and recovery.
- Guest-facing operational screens use Read mode; selective Experience treatment must not obscure ordering facts.
- Later specifications must cover realistic product ranges and first-run, empty, limited, sold-out, available-again, next-batch, seasonal, preorder, offline, outdated, permission, publish-failure, success, and recovery states.
- Accessibility requires non-color status communication, strong distance legibility, restrained motion, and understandable hierarchy.
- Responsive planning must cover mobile counter use, desktop administration, portrait and landscape displays, glare, queues, and crowded service environments.
- Preserve the Sky Blue direction for Vennusign administrative surfaces.

## Exact Next Café Action

After RWP-00.27 is merged, verified on `master`, and issue #502 is closed, execute **RWP-00.28 — Café, Bakery & Dessert Venue Subtypes** (#503).

RWP-00.28 must:

- define the supported café, coffee-shop, tea-shop, bakery, patisserie, dessert-shop, frozen-dessert, juice-or-smoothie, baked-specialty, and hybrid subtype catalog;
- establish inclusion, exclusion, and ambiguous-boundary rules;
- map meaningful subtype differences without duplicating Restaurant inheritance;
- define venue subtype selection, change, and mixed-concept behavior;
- keep subtypes separate from tiers and entitlements;
- consult Impeccable for any UI-facing selection or change-flow planning;
- remain documentation-only and hand off to RWP-00.29.

## Parallel-Stream Rule

The owner approved independently scheduled native-industry streams. Each industry remains sequential inside its own approved RWP range. Restaurant is the canonical baseline, only merged documents are authoritative, and no two runs may concurrently modify the same shared controlled file.

## Boundaries

- Do not start product implementation from Track 0 issues.
- Do not resume RWP-13.06 until Track 0 produces an owner-approved capability and packaging model.
- Do not start Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, or rollout controls during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
