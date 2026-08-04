# RWP-00.40 — Food Truck & Concession Venue Subtypes

## Status

Complete in this proposed merge state.

## Issue

- #515

## Objective

Define the supported venue, unit, stand, and operating-model subtypes within the Food Truck & Concession native profile, including bounded selection, change, hybrid, host-venue, and mixed-organization behavior, without creating separate entitlement models or implementing product behavior.

## Dependency verified

- RWP-00.39 is complete and merged.
- The canonical Restaurant baseline and merged Food Truck & Concession industry definition were used as authority.
- No competing open pull request, branch, or active tracker assignment owned this RWP when claimed.
- RWP-00.41 — Food Truck & Concession Business Terminology (#516) is the approved next item.

## Delivered

- Expanded `track0/industries/food-truck-concession.md` with the canonical subtype model.
- Defined nine primary subtypes: Food Truck, Food Trailer, Food Cart, Kiosk, Stadium / Arena Concession, Festival Vendor, Market Stall, Pop-Up, and Catering Concession.
- Defined an Unspecified / General Mobile or Concession Operation neutral fallback without creating another commercial package.
- Established inclusion, exclusion, neighboring-profile, physical-form, operating-context, and ambiguous-case rules for every subtype.
- Mapped each subtype to inherited Restaurant capabilities and recorded only meaningful operational, content, screen-purpose, and presentation deltas.
- Resolved hybrid concepts through one primary subtype plus optional descriptive physical-form, host-context, product-focus, and service-model traits.
- Resolved recurring fixed-pitch trucks, food-hall stalls, mobile coffee or dessert units, stadium food trucks, trailer and market hybrids, temporary restaurant pop-ups, catering trucks, host-operated concessions, and mixed-property outlets.
- Defined organization defaults, local subtype selection, subtype change, host-venue relationships, mixed organizations, multi-unit operators, and cross-subtype content-copy behavior.
- Kept physical unit form, operating context, host relationship, current location, and event state separate from commercial access and future quantity-limit counting.
- Updated `track0/CAPABILITY_MATRIX.md` so primary subtype, neutral state, and hybrid descriptive traits remain product/domain state.
- Consulted the project-local Impeccable skill and `shape` guidance for future subtype selection and change flows.
- Preserved the approved Sky Blue administrative direction.

## Canonical subtype result

The approved primary catalog is:

1. Food Truck
2. Food Trailer
3. Food Cart
4. Kiosk
5. Stadium / Arena Concession
6. Festival Vendor
7. Market Stall
8. Pop-Up
9. Catering Concession

A local operation may remain **Unspecified / General Mobile or Concession Operation** when no supported subtype clearly controls its daily operating rhythm.

The subtype catalog intentionally mixes physical forms and operating contexts because both can determine setup, guest communication, screen purpose, and defaults. Selection therefore follows the operating model that most consistently controls daily work. Other material characteristics remain optional descriptive traits rather than stacked subtypes or hidden feature flags.

## Impeccable planning result

The future subtype selection and change experience is an **Operate** surface for an owner or authorized manager, commonly working from a phone during setup, relocation, or active service.

Because this is a non-interactive planning run, the following assumptions were made explicit: selection is local to a venue, unit, or stand; physical form and operating context frequently overlap; host-venue authority can differ from operator authority; and existing content, screen assignments, history, and commercial access must be preserved.

The brief requires:

- bounded “best when” definitions based on dominant daily operating rhythm, mobility, permanence, host relationship, and event cadence rather than permit, vehicle-registration, legal, tax, or marketing classifications;
- one primary subtype, a neutral fallback, and optional descriptive traits for secondary physical form, host context, recurring route, product focus, or service model;
- an explicit explanation that subtype changes defaults and recommendations, not plan access or quantity allowances;
- a preview of changed terminology candidates, starter-content suggestions, screen purposes, and operating guidance before applying a subtype change;
- preservation of menus, items, prices, images, options, availability state, locations, events, screens, pairing, targeting, themes, schedules, publication history, and custom terminology;
- confirmation, safe cancellation, permission-restricted, validation-failure, save-success, and restoration states;
- scannable phone and desktop behavior, progressive disclosure for overlap cases, keyboard and assistive-technology support, plain language, and no color-only distinctions;
- clear handling when connectivity is weak, while avoiding any implication that selecting a subtype performs an external synchronization or physical-unit migration.

No UI or implementation contract was created.

## Classification decisions

1. Primary subtype is **product/domain state**.
2. Neutral subtype state is **product/domain state**.
3. Optional physical-form, operating-context, host-relationship, product-focus, and service-model traits are **product/domain state**.
4. Subtype may affect terminology candidates, starter content, recommendations, screen-purpose suggestions, and operating guidance only.
5. Subtype does not grant capabilities, increase limits, alter permissions, determine rollout, change ownership, or act as a subscription entitlement.
6. A physical vehicle, trailer, cart, kiosk, booth, stall, service window, or stand is not automatically an entitlement-counting venue.
7. Current location, event, service window, relocation, closure, and availability values keep their existing product-state classifications independent of subtype.
8. Subtype-specific screen purposes are recommendations using inherited or later-classified capabilities.
9. Counts of venues, units, stands, service points, screens, users, integrations, storage, retained history, or AI consumption remain independent limits.
10. Automatic POS, order, inventory, route, event, host-venue, location, or catering synchronization remains a later integration-packaging question and cannot replace manual core operation.

## Validation

Documentation-only review confirmed:

- every issue-listed canonical subtype has a bounded definition;
- Restaurant inheritance is retained and not duplicated as a new commercial model;
- physical-form and operating-context overlap resolves without stacked entitlements;
- hybrid and ambiguous concepts use one primary subtype plus optional descriptive traits;
- subtype selection and change preserve customer content, screen authority, host boundaries, and commercial access;
- physical units, service points, venue scope, and future limit counting remain separate;
- the capability matrix has one primary classification for subtype-related concerns;
- Impeccable `shape` guidance covers job, audience, outcome, hierarchy, states, realistic ranges, interaction, responsiveness, accessibility, feedback, and recovery;
- the next sequential item is RWP-00.41.

GitHub Actions is authoritative for lightweight documentation validation on the exact pull-request head.

## Skipped under standing owner instruction

- Azure SQL and all external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and other integration-type tests.
- Runtime, UI, API, schema, migration, billing, entitlement, permission, feature-gate, limit, rollout, pricing, ordering, payment, inventory, route, event, host-venue, catering, and integration implementation.

## Exact next action

After this RWP is merged, verified on `master`, issue #515 is closed, and the claim is released, execute **RWP-00.41 — Food Truck & Concession Business Terminology** (#516).

RWP-00.41 must define canonical operator and guest terminology for locations, stops, events, service windows, stands, menus, combos, sell-outs, service periods, pickup, queues, and venue/subtype overrides; identify Restaurant inheritance and neutral organization-wide fallbacks; keep language separate from permissions and entitlements; remain documentation-only; and hand off to RWP-00.42.
