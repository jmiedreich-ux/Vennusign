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
- Hospitality: RWP-00.52 merged; RWP-00.53 is next
- Entertainment & Attractions: RWP-00.64 complete in this proposed merge state; RWP-00.65 is next

## Entertainment & Attractions Venue-Subtype Result

The canonical model is documented at `track0/industries/entertainment-attractions.md` as a delta from Restaurant.

Twelve bounded primary subtypes are approved:

- Cinema
- Performing Arts Theater
- Museum
- Gallery / Exhibition Venue
- Zoo / Aquarium
- Theme / Amusement Park
- Family Entertainment Center
- Arcade
- Bowling Center
- Sports Venue
- Live-Event Venue
- Attraction / Tour

A venue may remain **Unspecified / General Entertainment & Attraction Venue** when no supported subtype clearly controls its daily visitor journey and information rhythm. This is a neutral product-state fallback rather than a commercial package.

Hybrid venues use one primary subtype plus optional descriptive traits. Selection follows the dominant visitor journey, program rhythm, physical movement, admissions model, schedule pattern, collection or attraction structure, and operating state. Traits describe outdoor, vehicle-based, water-based, seasonal, weather-sensitive, immersive, heritage, living-collection, education, admission, seating, membership, multi-use, campus, touring, resident-company, team-home, tournament, festival, environmental, or experience characteristics. They do not stack entitlements, transfer authority, alter permissions, or increase limits.

The model resolves drive-ins, water parks, botanical gardens, planetariums, observatories, historical sites, escape rooms, haunted attractions, observation decks, caves, scenic routes, immersive walkthroughs, trampoline parks, miniature golf, multi-activity destinations, mixed bowling/arcade operations, arenas, stadiums, performing-arts houses, flexible concert venues, exhibition spaces, mixed resorts, and casino complexes without creating separate commercial models.

Every subtype inherits Restaurant and Entertainment capabilities. Differences are limited to defaults, terminology candidates, starter recommendations, screen-purpose suggestions, operating guidance, and presentation emphasis. Embedded restaurants, bars, cafés, concessions, lodging, retail, and other local venues may use their own approved business types.

## Classification Result

- Organization primary industry, primary Entertainment subtype, neutral subtype state, and optional descriptive traits are product/domain state.
- Subtype does not grant capabilities, change plan access, transfer authority, alter permissions, increase limits, control rollout, or change commercial access.
- Venue hierarchy and operational values keep independent product/domain-state classifications.
- Manual program, showtime, event, attraction, exhibit, admissions, wayfinding, queue, capacity, delay, closure, relocation, accessibility, safety, targeting, publishing, delivery confirmation, offline awareness, and recovery remain core.
- Counts remain limits.
- Automatic ticketing, admissions, access-control, queue-management, venue-management, cinema, show-control, collection-management, attraction, event, sports, and related synchronization remains a later integration-packaging question and cannot replace manual core operation.
- Visitor-specific, ticket-specific, seat-specific, member-specific, participant-specific, performer-specific, sponsor-specific, security-sensitive, or operationally sensitive information requires later privacy and authorization decisions and is not assumed to be public signage content.
- A future subtype-change implementation must preserve authored content, screens, pairing, targeting, themes, schedules, history, current operational state, custom terminology, authority, privacy, integrations, limits, and commercial access.

## Impeccable Planning Result

The project-local Impeccable skill and `shape` guidance were consulted for future subtype selection and change flows.

- The surface is an **Operate** experience for an owner, administrator, or authorized venue manager.
- Bounded “best when” definitions and the dominant visitor journey outrank legal, statistical, licensing, brand, size, ownership, management, promoter, presenter, or marketing language.
- One primary subtype, a neutral fallback, and optional descriptive traits must be understandable without implying plan differences.
- A change flow must preview effects, preserve content and authority boundaries, require explicit confirmation, support safe cancellation and restoration, and cover permission, validation-failure, interrupted-save, and success states.
- Phone and desktop layouts must remain scannable, progressively disclose overlap detail, support keyboard and assistive technology, allow localization expansion and 200% zoom, use plain language, and avoid color-only distinctions.
- Preserve the approved Sky Blue direction for Vennusign administrative surfaces.

No UI, API, schema, migration, privacy, limit-counting, ticketing, admissions, access-control, queue-management, venue-management, show-control, collection-management, attraction, event, sports, analytics, or product implementation was authorized or performed.

## Exact Next Entertainment & Attractions Action

After RWP-00.64 is merged, verified on `master`, issue #539 is closed, and the claim is released, execute **RWP-00.65 — Entertainment & Attractions Business Terminology** (#540).

RWP-00.65 must:

- define canonical terminology for attraction, experience, show, screening, exhibit, venue, zone, queue, wait time, capacity, admission, ticket, schedule, wayfinding, notices, and subtype overrides;
- identify terms inherited unchanged from Restaurant;
- define subtype-specific terminology preferences and neutral organization-wide fallbacks;
- distinguish operator-facing and visitor-facing language;
- preserve customer-authored names and avoid visitor-specific or privacy-sensitive public wording;
- keep terminology separate from entitlements, permissions, ownership, promoter relationships, privacy scope, and limits;
- update the Track 0 capability documentation;
- remain documentation-only and hand off to RWP-00.66.

## Parallel-Stream Rule

The owner approved independently scheduled native-industry streams. Each industry remains sequential inside its own approved RWP range. Restaurant is the canonical baseline, only merged documents are authoritative, and no two runs may concurrently modify the same shared controlled file.

## Boundaries

- Do not start product implementation from Track 0 issues.
- Do not resume RWP-13.06 until Track 0 produces an owner-approved capability and packaging model.
- Do not start Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, limits, rollout controls, privacy systems, ticketing, admissions, access-control, queue management, venue management, show control, collection management, attractions, events, sports, analytics, localization, or integrations during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
