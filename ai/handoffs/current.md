# Vennusign Session Handoff

## Current State

- Item: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Mode: owner-led planning with independently scheduled native-industry streams; implementation paused
- Active implementation WP/RWP: none
- Phase 14 and later: paused
- RWP-13.06: paused pending the owner-approved Track 0 model
- Restaurant: canonical approved baseline
- Bar, Brewery & Nightlife: RWP-00.16 merged; RWP-00.17 is next
- Café, Bakery & Dessert: RWP-00.28 merged; RWP-00.29 is next
- Food Truck & Concession: RWP-00.40 complete in this proposed merge state; RWP-00.41 is next
- Hospitality: RWP-00.51 merged; RWP-00.52 is next
- Entertainment & Attractions: RWP-00.63 merged; RWP-00.64 is next

## Food Truck & Concession Subtype Result

The canonical subtype model is documented at `track0/industries/food-truck-concession.md` as a delta from Restaurant.

Nine bounded primary subtypes are approved:

- Food Truck
- Food Trailer
- Food Cart
- Kiosk
- Stadium / Arena Concession
- Festival Vendor
- Market Stall
- Pop-Up
- Catering Concession

A local operation may remain **Unspecified / General Mobile or Concession Operation** when no supported subtype clearly controls its daily operating rhythm. This is a neutral product-state fallback rather than a commercial package.

The catalog intentionally includes physical forms and operating contexts. Hybrid concepts use one primary subtype plus optional descriptive physical-form, operating-context, host-relationship, product-focus, recurring-route, seasonal, sponsor, or service-model traits. Selection follows the model that most consistently controls daily setup, service, guest communication, and local defaults. Traits do not stack entitlements, transfer ownership, alter permissions, or increase limits.

The model resolves recurring fixed-pitch trucks, seasonal trailers, food-hall counters, mobile coffee or dessert units, stadium food trucks, festival truck and trailer hybrids, market vendors attending festivals, temporary restaurant or chef residencies, catering trucks, host-operated concessions, mixed-property outlets, and one unit with multiple service windows without creating separate commercial models.

Every subtype inherits Restaurant capabilities. Differences are limited to defaults, terminology candidates, starter content, screen-purpose suggestions, operational emphasis, location or event context, host guidance, and presentation recommendations. Subtype-specific screen purposes remain recommendations, not entitlements.

## Classification Result

- Primary subtype is product/domain state.
- Neutral subtype state is product/domain state.
- Optional physical-form, operating-context, host-relationship, product-focus, and service-model traits are product/domain state.
- Subtype does not grant capabilities, change plan access, transfer authority, alter permissions, increase limits, control rollout, or determine venue counting.
- Current operating location, event, service window, relocation, closure, availability, and related operational values keep their own product-state classifications.
- Manual location, event, closure, relocation, availability, targeting, publishing, delivery confirmation, offline awareness, and recovery remain core.
- Counts of venues, units, stands, service points, screens, users, integrations, storage, retained history, or AI consumption remain independent limits.
- Automatic POS, order, inventory, route, event, host-venue, location, or catering synchronization remains a later integration-packaging question and cannot replace manual core operation.
- A future subtype-change implementation must preserve all customer-authored content, screen assignments, pairing, targeting, publication history, current operational state, host/operator boundaries, and commercial access.

## Impeccable Planning Result

The project-local Impeccable skill and `shape` guidance were consulted for future subtype selection and change flows.

Because the run was non-interactive, the brief records explicit assumptions: the user is an owner or authorized manager; selection is local to a venue, unit, stand, or service point; physical form and operating context frequently overlap; host and operator authority may differ; and existing content and commercial access must be preserved.

- The surface is an **Operate** experience, often used from a phone during setup, relocation, or active service.
- Bounded “best when” definitions, dominant daily rhythm, mobility or permanence, host relationship, event cadence, example screen purposes, and changed defaults outrank legal, permit, vehicle-registration, tax, or marketing language.
- One primary subtype, a neutral fallback, and optional descriptive traits must be understandable without implying plan differences.
- A change flow must preview effects, preserve content and authority boundaries, require explicit confirmation, support safe cancellation and restoration, and cover permission, validation-failure, interrupted-save, and success states.
- Phone and desktop layouts must remain scannable, progressively disclose overlap detail, support keyboard and assistive technology, use plain language, and avoid color-only distinctions.
- Preserve the approved Sky Blue direction for Vennusign administrative surfaces.

No UI, API, schema, migration, limit-counting, host-authority, or product implementation was authorized or performed.

## Exact Next Food Truck & Concession Action

After RWP-00.40 is merged, verified on `master`, issue #515 is closed, and the claim is released, execute **RWP-00.41 — Food Truck & Concession Business Terminology** (#516).

RWP-00.41 must:

- define canonical operator and guest terminology for locations, stops, events, service windows, stands, service points, menus, combos, sell-outs, service periods, pickup, and queues;
- identify terms inherited unchanged from Restaurant;
- define subtype-specific terminology overrides and neutral organization-wide fallbacks;
- resolve language when physical form and operating context differ;
- distinguish operator-facing and guest-facing language;
- keep terminology separate from entitlements, permissions, ownership, host authority, and limits;
- update the Track 0 capability documentation;
- remain documentation-only and hand off to RWP-00.42.

## Parallel-Stream Rule

The owner approved independently scheduled native-industry streams. Each industry remains sequential inside its own approved RWP range. Restaurant is the canonical baseline, only merged documents are authoritative, and no two runs may concurrently modify the same shared controlled file.

## Boundaries

- Do not start product implementation from Track 0 issues.
- Do not resume RWP-13.06 until Track 0 produces an owner-approved capability and packaging model.
- Do not start Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, limits, rollout controls, ordering, payments, inventory, routing, event management, host-venue management, catering management, or integrations during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
