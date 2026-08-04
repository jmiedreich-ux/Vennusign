# RWP-00.27 — Café, Bakery & Dessert Industry Definition

## Status

Complete.

## Issue

- #502

## Objective

Define the canonical Café, Bakery & Dessert native-industry profile as a delta from the approved Restaurant baseline, without implementing product behavior.

## Execution note

The owner explicitly authorized the native-industry schedules to proceed independently. Restaurant remained the authoritative baseline for this bounded definition RWP. The merged Bar, Brewery & Nightlife definition was used only for supplementary cross-industry consistency patterns; no unmerged Bar work was treated as authoritative.

## Delivered

- Added `track0/industries/cafe-bakery-dessert.md`.
- Defined purpose, customer outcomes, Restaurant inheritance, meaningful operating differences, industry boundaries, and mixed-organization behavior.
- Defined initial treatment of product options, batch and freshness state, sold-outs, preorders, pickup communication, counter-service speed, and retail overlap.
- Recorded organization primary industry and venue business type as product/domain state rather than entitlements.
- Confirmed rapid manual sold-out and available-again changes remain core operations acting on product state.
- Updated `track0/CAPABILITY_MATRIX.md` with bounded Café, Bakery & Dessert deltas.
- Consulted the project-local Impeccable skill and its `shape` guidance for future operator and guest-facing surfaces.
- Updated the project status, current handoff, and tracker to identify RWP-00.28 as the next Café, Bakery & Dessert item.

## Impeccable planning record

This RWP defines future UI constraints without designing or implementing screens:

- Operator surfaces use Operate mode with rapid scanning, confident state changes, publishing feedback, and recovery.
- Guest-facing operational screens use Read mode; expressive Experience treatment is limited to content that does not obscure ordering facts.
- Later RWPs must cover hierarchy, realistic content ranges, first-run, empty, sold-out, available-again, next-batch, seasonal, preorder, offline, outdated, permission, publish-failure, success, and recovery states.
- Accessibility requires non-color status communication, strong distance legibility, restrained motion, and understandable hierarchy.
- Responsive planning must cover mobile counter use, desktop administration, portrait and landscape displays, glare, queues, and crowded service environments.
- The approved Sky Blue direction remains authoritative for Vennusign administrative surfaces.

## Validation

Documentation review only:

- Confirmed the profile inherits Restaurant and does not duplicate the full baseline.
- Confirmed Café, Bakery & Dessert selection changes defaults and guidance rather than commercial access.
- Confirmed batch, freshness, limited quantity, and expected-return information are product/domain state when represented.
- Confirmed automatic POS, order, and inventory synchronization remains a future integration-packaging question and does not replace manual core operations.
- Confirmed meal-led restaurants, mobile service, packaged retail, grocery departments, commercial manufacturing, and alcohol-led concepts remain outside the canonical boundary unless represented through a mixed venue.
- Confirmed no product, UI, API, schema, migration, billing, entitlement, feature-gate, or rollout implementation was introduced.
- Integration and external-system tests were not applicable and remain skipped under the standing owner instruction.

## Research anchors

- U.S. Census Bureau 2022 NAICS 722515 boundary for snack and nonalcoholic beverage bars.
- U.S. Census Bureau 2022 NAICS 311811 boundary for retail bakeries.

References are linked from the industry profile and are used only as boundary evidence, not as Vennusign entitlement or legal classifications.

## Handoff

The next Café, Bakery & Dessert item is **RWP-00.28 — Café, Bakery & Dessert Venue Subtypes** (#503).

RWP-00.28 should define the canonical subtype catalog, inclusion and exclusion rules, subtype-specific differences, venue subtype selection and change behavior, and mixed-concept handling. It must remain documentation-only and must not start until this RWP is merged, verified on `master`, and issue #502 is closed.
