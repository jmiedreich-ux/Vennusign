# RWP-00.16 — Bar, Brewery & Nightlife Venue Subtypes

## Status

Complete in this proposed merge state.

## Issue

- #491

## Objective

Define the supported venue and business subtypes within the Bar, Brewery & Nightlife native profile, including bounded selection, change, hybrid, and mixed-organization behavior, without creating separate entitlement models or implementing product behavior.

## Dependency verified

- RWP-00.15 is complete and merged.
- The canonical Restaurant baseline and merged Bar, Brewery & Nightlife industry definition were used as authority.
- No competing open pull request or active tracker assignment owned this RWP when claimed.

## Delivered

- Expanded `track0/industries/bar-brewery-nightlife.md` with the canonical subtype model.
- Defined nine primary subtypes: Pub, Sports Bar, Cocktail Bar, Wine Bar, Brewery, Brewpub, Taproom, Nightclub, and Lounge.
- Defined an Unspecified / General Bar neutral fallback without creating another commercial package.
- Established inclusion, exclusion, neighboring-profile, and ambiguous-case rules for every subtype.
- Mapped each subtype to inherited Restaurant capabilities and recorded only meaningful operational, content, screen-purpose, and presentation deltas.
- Resolved hybrid concepts through one primary subtype plus optional descriptive traits.
- Resolved gastropubs, sports pubs, brewery taprooms, producer tasting rooms, alcohol-free concepts, private clubs, bottle-shop tasting areas, and hotel/casino/resort bars.
- Defined organization defaults, venue-local selection, subtype change, mixed-organization, multi-venue, and cross-subtype content-copy behavior.
- Updated `track0/CAPABILITY_MATRIX.md` so venue subtype, neutral state, and hybrid traits remain product/domain state.
- Consulted the project-local Impeccable skill and `shape` guidance for future subtype selection and change flows.
- Preserved the approved Sky Blue administrative direction.

## Impeccable planning result

The future subtype selection and change experience is an **Operate** surface for an owner or authorized manager.

The brief requires:

- bounded “best when” definitions rather than legal or marketing classifications;
- one primary subtype, a neutral fallback, and optional hybrid traits;
- an explicit explanation that subtype changes defaults and recommendations, not plan access;
- a preview of changed defaults before applying a subtype change;
- preservation of all customer-authored content and commercial access;
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

## Validation

Documentation-only review confirmed:

- every issue-listed canonical subtype has a bounded definition;
- Restaurant inheritance is retained and not duplicated as a new commercial model;
- food-led, entertainment-led, manufacturing-only, retail-only, and membership-led boundary cases have explicit neighboring-profile rules;
- producer tasting rooms and other hybrids do not create hidden subtype entitlements;
- subtype selection and change preserve customer content and organization entitlement authority;
- terminology detail remains deferred to RWP-00.17;
- no product, UI, API, schema, migration, billing, entitlement, permission, feature-gate, limit, rollout, or pricing implementation was introduced;
- integration and external-system tests were not applicable and remain skipped under the standing owner instruction.

## Handoff

The next sequential item is **RWP-00.17 — Bar, Brewery & Nightlife Business Terminology** (#492).

RWP-00.17 must define the canonical glossary, Restaurant-inherited terms, subtype-specific overrides, operator-facing versus guest-facing language, neutral wording for ambiguous organization-wide surfaces, and hybrid fallback behavior. It must remain documentation-only and must not begin until RWP-00.16 is merged, verified on `master`, issue #491 is closed, and the claim is released.
