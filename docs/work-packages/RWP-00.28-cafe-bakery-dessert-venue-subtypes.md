# RWP-00.28 — Café, Bakery & Dessert Venue Subtypes

## Status

Complete in this proposed merge state.

## Issue

- #503

## Objective

Define the supported venue and business subtypes within the Café, Bakery & Dessert native profile, including bounded selection, change, hybrid, and mixed-organization behavior, without creating separate entitlement models or implementing product behavior.

## Dependency verified

- RWP-00.27 is complete and merged.
- The canonical Restaurant baseline and merged Café, Bakery & Dessert industry definition were used as authority.
- No competing open pull request or active tracker assignment owned this RWP when claimed.

## Delivered

- Expanded `track0/industries/cafe-bakery-dessert.md` with the canonical subtype model.
- Defined nine primary subtypes: Café, Coffee Shop, Tea Shop, Bakery, Patisserie, Bakery-Café, Dessert Shop, Frozen Dessert Shop, and Juice & Smoothie Bar.
- Defined an Unspecified / General Café neutral fallback without creating another commercial package.
- Established inclusion, exclusion, neighboring-profile, and ambiguous-case rules for every subtype.
- Mapped each subtype to inherited Restaurant capabilities and recorded only meaningful operational, content, screen-purpose, and presentation deltas.
- Resolved hybrid concepts through one primary subtype plus optional descriptive traits.
- Resolved bubble-tea shops, doughnut and bagel shops, commercial bakeries with retail counters, meal-heavy bakery-cafés, custom-order cake studios, chocolatiers and confectioners, mobile concepts, and hotel or entertainment-property outlets.
- Defined organization defaults, venue-local selection, subtype change, mixed-organization, multi-venue, and cross-subtype content-copy behavior.
- Updated `track0/CAPABILITY_MATRIX.md` so venue subtype, neutral state, and hybrid traits remain product/domain state.
- Consulted the project-local Impeccable skill and `shape` guidance for future subtype selection and change flows.
- Preserved the approved Sky Blue administrative direction.

## Impeccable planning result

The future subtype selection and change experience is an **Operate** surface for an owner or authorized manager.

Because this is a non-interactive planning run, the following assumptions were made explicitly: subtype selection is venue-local, the user may be uncertain about overlapping concepts, and the safest default is to preserve existing content and commercial access.

The brief requires:

- bounded “best when” definitions based on dominant guest journey and daily operating rhythm rather than legal, tax, manufacturing, or marketing classifications;
- one primary subtype, a neutral fallback, and optional hybrid traits;
- an explicit explanation that subtype changes defaults and recommendations, not plan access;
- a preview of changed defaults before applying a subtype change;
- preservation of all customer-authored content, screen assignments, publication history, and commercial access;
- confirmation, safe cancellation, visible success, permission-restricted, validation-failure, and restoration states;
- scannable phone and desktop behavior, keyboard and assistive-technology support, plain language, and no color-only distinctions.

No UI or implementation contract was created.

## Classification decisions

1. Venue subtype is **product/domain state**.
2. Neutral subtype state is **product/domain state**.
3. Optional hybrid descriptive traits are **product/domain state**.
4. Subtype may affect terminology candidates, starter content, recommendations, screen-purpose suggestions, and guidance only.
5. Subtype does not grant capabilities, increase limits, alter permissions, determine rollout, or act as a subscription entitlement.
6. Subtype-specific screen purposes are recommendations using inherited or later-classified capabilities.
7. Counts of venues, screens, users, connections, content, storage, history, or AI consumption remain independent limits.
8. Batch, freshness, preorder, pickup, production, and availability values remain product/domain state or later integration concerns; subtype does not change their primary classification.

## Validation

Documentation-only review confirmed:

- every issue-listed canonical subtype has a bounded definition;
- Restaurant inheritance is retained and not duplicated as a new commercial model;
- hybrid and ambiguous concepts resolve without stacking entitlements;
- subtype selection and change preserve customer content and commercial access;
- the capability matrix has one primary classification for subtype-related concerns;
- Impeccable `shape` guidance covers job, outcome, hierarchy, states, realistic ranges, interaction, responsiveness, accessibility, feedback, and recovery;
- the next sequential item is RWP-00.29.

GitHub Actions is authoritative for lightweight documentation validation on the exact pull-request head.

## Skipped under standing owner instruction

- Azure SQL and all external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and other integration-type tests.
- Runtime, UI, API, schema, migration, billing, entitlement, permission, feature-gate, limit, rollout, and pricing implementation.

## Exact next action

After this RWP is merged, verified on `master`, issue #503 is closed, and the claim is released, execute **RWP-00.29 — Café, Bakery & Dessert Business Terminology** (#504).

RWP-00.29 must define canonical operator and guest terminology for products, sizes, modifiers, batches, freshness, availability, preorders, pickup, and service periods; identify Restaurant inheritance, subtype overrides, and hybrid fallbacks; keep terminology separate from permissions and entitlements; remain documentation-only; and hand off to RWP-00.30.
