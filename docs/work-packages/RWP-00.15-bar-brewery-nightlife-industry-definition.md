# RWP-00.15 — Bar, Brewery & Nightlife Industry Definition

## Status

Complete.

## Issue

- #490

## Objective

Define the canonical Bar, Brewery & Nightlife native-industry profile as a delta from the approved Restaurant baseline, without implementing product behavior.

## Delivered

- Added `track0/industries/bar-brewery-nightlife.md`.
- Defined purpose, customer outcomes, Restaurant inheritance, meaningful operating differences, and scope boundaries.
- Defined organization primary-industry, venue business-type, and mixed-organization behavior.
- Recorded initial classification rules for industry configuration, venue subtype configuration, and manual availability.
- Updated `track0/CAPABILITY_MATRIX.md` with bounded Bar, Brewery & Nightlife deltas.
- Consulted the project-local Impeccable skill and recorded UI-facing planning guardrails for hierarchy, states, environmental readability, accessibility, responsiveness, and recovery.
- Preserved the approved Sky Blue administrative direction.

## Validation

Documentation review only:

- Confirmed the profile inherits Restaurant and does not duplicate the full baseline.
- Confirmed industry and venue subtype selection are product/domain state, not entitlements.
- Confirmed higher-frequency beverage availability changes do not convert availability into a commercial feature gate.
- Confirmed food-led restaurants, packaged retail, manufacturing-only operations, and non-beverage-led entertainment remain outside the canonical boundary unless represented through a mixed venue.
- Confirmed no UI, API, schema, migration, billing, entitlement, feature-gate, or rollout implementation was introduced.
- Integration and external-system tests were not applicable and remain skipped under the standing owner instruction.

## Research anchors

- U.S. Census Bureau 2022 NAICS 722410 boundary for drinking places.
- Brewers Association distinction among brewpub and taproom brewery market segments.

References are linked from the industry profile and are used only as boundary evidence, not as Vennusign entitlement or legal classifications.

## Handoff

The next sequential item is **RWP-00.16 — Bar, Brewery & Nightlife Venue Subtypes** (#491).

RWP-00.16 should define the canonical subtype catalog, inclusion and exclusion rules, subtype-specific terminology and operational deltas, venue selection/change behavior, and mixed-concept handling. It must remain documentation-only and must not start until this RWP is merged, verified on `master`, and issue #490 is closed.
