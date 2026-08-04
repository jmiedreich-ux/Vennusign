# Vennusign Session Handoff

## Current State

- Item: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Mode: owner-led planning with independently scheduled native-industry streams; implementation paused
- Active implementation WP/RWP: none
- Phase 14 and later: paused
- RWP-13.06: paused pending the owner-approved Track 0 model
- Restaurant: canonical approved baseline
- Bar, Brewery & Nightlife: RWP-00.16 merged; RWP-00.17 is next
- Café, Bakery & Dessert: RWP-00.28 complete in this proposed merge state; RWP-00.29 is next
- Food Truck & Concession: RWP-00.39 merged; RWP-00.40 is next
- Hospitality: RWP-00.51 merged; RWP-00.52 is next

## Café, Bakery & Dessert Subtype Result

The canonical subtype model is documented in `track0/industries/cafe-bakery-dessert.md` as a delta from Restaurant.

Nine bounded primary subtypes are approved:

- Café
- Coffee Shop
- Tea Shop
- Bakery
- Patisserie
- Bakery-Café
- Dessert Shop
- Frozen Dessert Shop
- Juice & Smoothie Bar

A venue may remain Unspecified / General Café when no subtype is clearly dominant. This is a neutral product-state fallback rather than a commercial package.

Hybrid concepts use one primary subtype plus optional descriptive operating traits. The primary subtype follows the dominant guest journey and daily operating rhythm. Traits tune future terminology and recommendations only; they do not stack entitlements, alter permissions, or increase limits.

The model resolves bubble-tea shops, specialty doughnut/bagel/pretzel/cookie concepts, commercial bakeries with retail counters, meal-heavy bakery-cafés, custom-order cake studios, chocolatiers and confectioners, hotel or entertainment-property outlets, preorder-led businesses, and mobile concepts. Meal-led concepts remain Restaurant, mobile/event-led concepts remain Food Truck & Concession, packaged retail and manufacturing-only operations remain outside the profile, and mixed properties use venue-level subtype configuration.

Each subtype inherits Restaurant capabilities. Differences are limited to defaults, terminology candidates, starter content, screen-purpose suggestions, operational emphasis, and presentation guidance. Subtype-specific screen purposes remain recommendations, not entitlements.

## Classification Result

- Primary venue subtype is product/domain state.
- Neutral subtype state is product/domain state.
- Optional hybrid traits are product/domain state.
- Subtype does not grant capabilities, change plan access, alter permissions, increase limits, or control rollout.
- Counts of venues, screens, users, connections, content, storage, history, or AI consumption remain independent limits.
- Batch, freshness, limited-quantity, expected-return, preorder, pickup, and production values keep their own product-state or later integration classifications independent of subtype.
- A future subtype-change implementation must preserve all customer-authored content and preview changed defaults before explicit confirmation.

## Impeccable Planning Result

The project-local Impeccable skill and `shape` guidance were consulted for future subtype selection and change flows.

Because the run was non-interactive, the brief records explicit assumptions: the user is an owner or authorized manager, selection is venue-local, overlapping concepts are common, and existing content and commercial access must be preserved.

- The surface is an Operate experience for an owner or authorized manager.
- Bounded “best when” definitions, dominant guest journey, example products and screen purposes, and changed defaults outrank legal, manufacturing, or marketing language.
- One primary subtype, a neutral fallback, and optional hybrid traits must be understandable without implying plan differences.
- A change flow must preview effects, preserve content, require confirmation, support safe cancellation and restoration, and cover permission, validation-failure, and success states.
- Phone and desktop layouts must remain scannable, keyboard and assistive-technology usable, plain-language, and independent of color-only distinctions.
- Preserve the approved Sky Blue direction for Vennusign administrative surfaces.

No UI, API, schema, migration, or product implementation was authorized or performed.

## Exact Next Café, Bakery & Dessert Action

After RWP-00.28 is merged, verified on `master`, issue #503 is closed, and the claim is released, execute **RWP-00.29 — Café, Bakery & Dessert Business Terminology** (#504).

RWP-00.29 must:

- define canonical operator and guest terminology for products, sizes, modifiers, batches, freshness, availability, preorders, pickup, and service periods;
- identify terms inherited unchanged from Restaurant;
- define subtype-specific terminology overrides;
- distinguish operator-facing and guest-facing language;
- define neutral wording for organization-wide and ambiguous contexts;
- define hybrid fallback behavior using the approved primary-subtype-plus-traits model;
- keep terminology separate from entitlements and permissions;
- update the Track 0 capability documentation;
- remain documentation-only and hand off to RWP-00.30.

## Parallel-Stream Rule

The owner approved independently scheduled native-industry streams. Each industry remains sequential inside its own approved RWP range. Restaurant is the canonical baseline, only merged documents are authoritative, and no two runs may concurrently modify the same shared controlled file.

## Boundaries

- Do not start product implementation from Track 0 issues.
- Do not resume RWP-13.06 until Track 0 produces an owner-approved capability and packaging model.
- Do not start Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, limits, or rollout controls during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
