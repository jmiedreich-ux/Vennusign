# Vennusign Session Handoff

## Current State

- Item: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Mode: owner-led planning with independently scheduled native-industry streams; implementation paused
- Active implementation WP/RWP: none
- Phase 14 and later: paused
- RWP-13.06: paused pending the owner-approved Track 0 model
- Restaurant: canonical approved baseline
- Bar, Brewery & Nightlife: RWP-00.17 merged; RWP-00.18 is next
- Café, Bakery & Dessert: RWP-00.29 merged; RWP-00.30 is next
- Food Truck & Concession: RWP-00.41 merged; RWP-00.42 is next
- Hospitality: RWP-00.52 complete in this proposed merge state; RWP-00.53 is next
- Entertainment & Attractions: RWP-00.63 merged; RWP-00.64 is next

## Hospitality Venue-Subtype Result

The canonical subtype model is documented at `track0/industries/hospitality.md` as a delta from Restaurant.

Nine bounded primary subtypes are approved:

- Hotel
- Resort
- Motel
- Hostel
- Extended-Stay
- Serviced Apartment
- Conference Property
- Casino Resort
- Boutique Lodging

A property may remain **Unspecified / General Hospitality Property** when no supported subtype clearly controls its daily operating rhythm. This is a neutral product-state fallback rather than a commercial package.

Hybrid properties use one primary subtype plus optional descriptive destination, recreation, wellness, conference, wedding, gaming, heritage, lifestyle, apartment-style, extended-stay, campus, seasonal, or mixed-use traits. Selection follows the model that most consistently controls arrival, circulation, length of stay, shared accommodation, amenity and event breadth, vehicle access, gaming, and daily guest communication. Traits do not stack entitlements, transfer authority, alter permissions, or increase limits.

The model resolves city hotels with conference business, destination hotels, roadside inns and motels, bed-and-breakfasts and lodges, aparthotels, vacation clubs and branded residences, casino hotels, hostels with many private rooms, attached convention centers, mixed resorts, multi-building campuses, and management-company portfolios without creating separate commercial models.

Every subtype inherits Restaurant and Hospitality capabilities. Differences are limited to defaults, terminology candidates, starter content, screen-purpose suggestions, operational emphasis, and presentation guidance. Distinct restaurants, bars, cafés, concessions, attractions, retail, spas, and event venues inside a property may use their own approved local business types.

## Classification Result

- Organization primary industry is product/domain state.
- Primary Hospitality subtype is product/domain state.
- Neutral subtype state is product/domain state.
- Optional descriptive traits are product/domain state.
- Subtype does not grant capabilities, change plan access, transfer authority, alter permissions, increase limits, control rollout, or change commercial access.
- Property, building, tower, wing, floor, area, outlet, room, event, amenity, service window, closure, relocation, and related values keep independent product-state classifications.
- Manual guest information, wayfinding, event, amenity, service, closure, relocation, changed-hours, targeting, publishing, delivery confirmation, offline awareness, and recovery remain core.
- Counts of properties, buildings, rooms, venues, outlets, areas, events, screens, users, integrations, storage, retained history, or AI consumption remain independent limits.
- Automatic property-management, event, room-booking, point-of-sale, transport, guest-service, access, gaming, or related synchronization remains a later integration-packaging question and cannot replace manual core operation.
- Guest-specific, reservation-specific, room-specific, member-specific, or sensitive operational information requires later privacy and authorization decisions and is not assumed to be public signage content.
- A future subtype-change implementation must preserve all customer-authored content, screens, pairing, targeting, themes, schedules, publication history, current property and service state, custom terminology, privacy and authority boundaries, and commercial access.

## Impeccable Planning Result

The project-local Impeccable skill and `shape` guidance were consulted for future subtype selection and change flows.

Because the run was non-interactive, the brief records explicit assumptions: the user is an owner, administrator, or authorized property manager; selection is local to a Hospitality property; organization industry may suggest but cannot override it; subtype overlap and mixed properties are common; and existing content, state, authority, privacy boundaries, and commercial access must be preserved.

- The surface is an **Operate** experience.
- Bounded “best when” definitions and dominant guest journey outrank legal, brand, star-rating, room-count, ownership, management, tax, architecture, or marketing language.
- One primary subtype, a neutral fallback, and optional descriptive traits must be understandable without implying plan differences.
- A change flow must preview effects, preserve content and authority boundaries, require explicit confirmation, support safe cancellation and restoration, and cover permission, validation-failure, interrupted-save, and success states.
- Phone and desktop layouts must remain scannable, progressively disclose overlap detail, support keyboard and assistive technology, allow localization expansion and 200% zoom, use plain language, and avoid color-only distinctions.
- Preserve the approved Sky Blue direction for Vennusign administrative surfaces.

No UI, API, schema, migration, privacy, limit-counting, property-management, event, room-booking, transport, guest-service, gaming, or product implementation was authorized or performed.

## Exact Next Hospitality Action

After RWP-00.52 is merged, verified on `master`, issue #527 is closed, and the claim is released, execute **RWP-00.53 — Hospitality Business Terminology** (#528).

RWP-00.53 must:

- define canonical terminology for property, guest, stay, room, amenity, venue, outlet, event, meeting space, wayfinding, service hours, notices, and closure;
- identify terms inherited unchanged from Restaurant;
- define subtype-specific terminology overrides and neutral organization-wide fallbacks;
- distinguish operator-facing and guest-facing language;
- preserve customer-authored names and avoid guest-specific or privacy-sensitive public wording;
- keep terminology separate from entitlements, permissions, ownership, management, privacy scope, and limits;
- update the Track 0 capability documentation;
- remain documentation-only and hand off to RWP-00.54.

## Parallel-Stream Rule

The owner approved independently scheduled native-industry streams. Each industry remains sequential inside its own approved RWP range. Restaurant is the canonical baseline, only merged documents are authoritative, and no two runs may concurrently modify the same shared controlled file.

## Boundaries

- Do not start product implementation from Track 0 issues.
- Do not resume RWP-13.06 until Track 0 produces an owner-approved capability and packaging model.
- Do not start Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, limits, rollout controls, privacy systems, property-management behavior, event or room-booking behavior, transport or guest-service automation, gaming behavior, analytics, localization, or integrations during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
