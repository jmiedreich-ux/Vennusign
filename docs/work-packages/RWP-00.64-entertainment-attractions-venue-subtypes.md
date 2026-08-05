# RWP-00.64 — Entertainment & Attractions Venue Subtypes

## Status

Complete in this proposed merge state.

## Issue

- #539

## Objective

Define the supported Entertainment & Attractions venue subtypes, their bounded inclusion and exclusion rules, hybrid behavior, Restaurant and Entertainment inheritance, subtype selection and change behavior, and meaningful operating and presentation differences without creating separate entitlement models or implementing product behavior.

## Dependency verified

- RWP-00.63 is complete and merged.
- The canonical Restaurant baseline and merged Entertainment & Attractions industry definition were used as authority.
- No competing open pull request, branch, or active tracker assignment owned this RWP when claimed.
- RWP-00.65 — Entertainment & Attractions Business Terminology (#540) is the approved next item.

## Delivered

- Expanded `track0/industries/entertainment-attractions.md` with the canonical venue-subtype model.
- Defined twelve primary subtypes: Cinema, Performing Arts Theater, Museum, Gallery / Exhibition Venue, Zoo / Aquarium, Theme / Amusement Park, Family Entertainment Center, Arcade, Bowling Center, Sports Venue, Live-Event Venue, and Attraction / Tour.
- Defined an Unspecified / General Entertainment & Attraction Venue neutral fallback without creating another commercial package.
- Established inclusion, exclusion, neighboring-profile, hybrid, and ambiguous-case rules for every subtype.
- Resolved drive-ins, water parks, escape rooms, trampoline parks, miniature golf, immersive experiences, haunted attractions, historical sites, botanical gardens, visitor centers, observatories, planetariums, racetracks, seasonal attractions, cultural campuses, arena districts, mixed resorts, and casino complexes.
- Mapped each subtype to inherited Restaurant and Entertainment capabilities and recorded only meaningful operational, content, screen-purpose, and presentation deltas.
- Defined organization defaults, local subtype selection, subtype change, mixed-campus, multi-venue, embedded-venue, and cross-subtype copied-content behavior.
- Kept physical hierarchy, brand, ownership, operator, promoter, presenter, tenant, sponsor, team, performer, distributor, rights-holder, authority, privacy, commercial access, and future quantity-limit counting separate from subtype.
- Updated `track0/CAPABILITY_MATRIX.md` so Entertainment primary subtype, neutral state, and optional descriptive traits remain product/domain state.
- Consulted the project-local Impeccable skill and `shape` guidance for future subtype selection and change flows.
- Preserved the approved Sky Blue administrative direction.

## Canonical subtype result

The approved primary catalog is:

1. Cinema
2. Performing Arts Theater
3. Museum
4. Gallery / Exhibition Venue
5. Zoo / Aquarium
6. Theme / Amusement Park
7. Family Entertainment Center
8. Arcade
9. Bowling Center
10. Sports Venue
11. Live-Event Venue
12. Attraction / Tour

A venue may remain **Unspecified / General Entertainment & Attraction Venue** when no supported subtype clearly controls its daily visitor journey and information rhythm.

Hybrid concepts use one primary subtype plus optional descriptive traits such as outdoor, drive-in or vehicle-based, water-based, seasonal, weather-sensitive, immersive, heritage, living-collection, science or education, general-admission, reserved-seat, timed-entry, membership-led, multi-use, campus, touring, resident-company, team-home, tournament, festival, low-light, high-motion, continuous, self-guided, or guided. Traits do not stack entitlements, transfer authority, or increase limits.

## Key boundary decisions

- A drive-in remains Cinema with outdoor and vehicle-based traits.
- A water park is Theme / Amusement Park with a water-based trait.
- A botanical garden is Zoo / Aquarium when the living collection and visitor path drive the experience.
- A planetarium or observatory is Museum when education, interpretation, and exhibits dominate; a separate showtime-led venue may use Cinema or Performing Arts Theater where that operating rhythm truly controls.
- A historical site is Museum when preservation and interpretation dominate, and Attraction / Tour when the managed route or guided destination experience dominates.
- Escape rooms, haunted attractions, observation decks, caves, scenic routes, and single immersive walkthroughs are Attraction / Tour unless a broader multi-activity destination makes Family Entertainment Center more accurate.
- Trampoline parks and multi-activity miniature-golf, laser-tag, climbing, or party destinations are Family Entertainment Center when no single specialist activity controls the operation.
- Bowling with incidental arcade or food remains Bowling Center; a balanced multi-activity destination may use Family Entertainment Center.
- An arena or stadium is Sports Venue when the competition or home-team calendar dominates and Live-Event Venue when promoter-led concerts and changing productions dominate. A multi-use trait records the secondary behavior.
- A purpose-built live-performance house is Performing Arts Theater; a flexible promoter-led concert, comedy, festival, or event facility is Live-Event Venue.
- An exhibition-led non-retail art space is Gallery / Exhibition Venue; collection stewardship and institutional interpretation make Museum more accurate. Retail art dealers remain outside the native boundary.
- A resort or casino with lodging remains Hospitality at the property level, while its theaters, arenas, attractions, museums, arcades, bowling centers, and event venues may use local Entertainment subtypes.

## Impeccable planning result

The future subtype selection and change experience is an **Operate** surface for an owner, administrator, or authorized venue manager.

Because this is a non-interactive planning run, the following assumptions were made explicit: selection is local to an Entertainment venue; organization industry may suggest but cannot override the local choice; subtype overlap and mixed campuses are common; embedded restaurants, bars, cafés, concessions, lodging, retail, and other venues may use their own approved business types; and existing content, screen assignments, history, authority, privacy boundaries, and commercial access must be preserved.

The brief requires:

- bounded “best when” definitions based on the dominant visitor journey, program rhythm, physical movement, admissions model, schedule pattern, collection or attraction structure, and operational state rather than legal, statistical, licensing, brand, venue-size, ownership, management, promoter, presenter, or marketing classifications;
- one primary subtype, a neutral fallback, and optional descriptive traits for secondary operating characteristics;
- an explicit explanation that subtype changes defaults and recommendations, not plan access, privacy scope, authority, or quantity allowances;
- a preview of changed terminology candidates, starter-content suggestions, screen purposes, operating guidance, and presentation emphasis before applying a subtype change;
- preservation of customer-authored content, screens, pairing, targeting, themes, schedules, publication history, current venue, event, session, queue, capacity, closure, and relocation state, custom terminology, authority boundaries, and commercial access;
- confirmation, safe cancellation, permission-restricted, validation-failure, interrupted-save, success, and restoration states;
- scannable phone and desktop behavior, progressive disclosure for overlap cases, keyboard and assistive-technology support, localization expansion, 200% zoom, plain language, and no color-only distinctions;
- clear handling of a single-screen cinema or small gallery through multi-building museums, zoological campuses, amusement parks, arena districts, sports complexes, and mixed entertainment destinations without implying that subtype selection performs integrations or migrates physical venue structure.

No UI or implementation contract was created.

## Classification decisions

1. Organization primary industry, primary Entertainment subtype, neutral subtype state, and optional descriptive traits are **product/domain state**.
2. Subtype may affect terminology candidates, starter content, recommendations, screen-purpose suggestions, operating guidance, and presentation emphasis only.
3. Subtype does not grant capabilities, change plan access, alter permissions, increase limits, control rollout, transfer authority, or act as a subscription entitlement.
4. Venue, campus, building, entrance, floor, zone, destination, auditorium, screen, stage, gallery, exhibit, attraction, ride, lane, court, field, room, event, performance, screening, session, queue, admission window, capacity state, delay, closure, relocation, and similar values keep their product/domain-state classifications independent of subtype.
5. Brand, ownership, operator, promoter, presenter, tenant, sponsor, team, performer, distributor, rights-holder, legal classification, licensing, and venue architecture do not become hidden feature flags.
6. Subtype-specific screen purposes are recommendations using inherited or later-classified capabilities.
7. Manual program, showtime, event, attraction, exhibit, admissions, wayfinding, queue, capacity, delay, closure, relocation, accessibility, safety, targeting, publishing, delivery confirmation, offline awareness, and recovery remain core.
8. Counts of venues, campuses, buildings, areas, attractions, exhibits, events, performances, screenings, sessions, queues, screens, users, integrations, storage, retained history, or AI consumption remain independent limits.
9. Automatic ticketing, admissions, access-control, queue-management, venue-management, cinema, show-control, collection-management, attraction, event, sports, or related synchronization remains a later integration-packaging question and cannot replace manual core operation.
10. Visitor-specific, ticket-specific, seat-specific, member-specific, participant-specific, performer-specific, sponsor-specific, security-sensitive, or operationally sensitive information remains subject to later privacy and authorization decisions and is not assumed to be public signage content.

## Validation

Documentation-only review confirmed:

- every issue-listed canonical subtype has a bounded definition;
- Restaurant and Entertainment inheritance are retained and not duplicated as new commercial models;
- hybrid and ambiguous venues use one primary subtype plus optional descriptive traits;
- subtype selection and change preserve customer content, screens, state, authority, privacy boundaries, and commercial access;
- mixed-campus, multi-venue, embedded-venue, and multi-use behavior remains explicit;
- venue hierarchy, operating scope, ownership, operator, promoter, presenter, tenant, brand, authority, and future limit counting remain separate;
- the capability matrix has one primary classification for subtype-related concerns;
- Impeccable `shape` guidance covers job, audience, outcome, hierarchy, states, realistic ranges, interaction, responsiveness, accessibility, feedback, and recovery;
- the next sequential item is RWP-00.65.

GitHub Actions is authoritative for lightweight documentation validation on the exact pull-request head.

## Skipped under standing owner instruction

- Azure SQL and all external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and other integration-type tests.
- Runtime, UI, API, schema, migration, billing, entitlement, permission, feature-gate, limit, rollout, privacy-system, ticketing, admissions, access-control, queue-management, show-control, collection-management, attraction, event, sports, and other integration implementation.

## Exact next action

After this RWP is merged, verified on `master`, issue #539 is closed, and the claim is released, execute **RWP-00.65 — Entertainment & Attractions Business Terminology** (#540).

RWP-00.65 must define canonical terminology for attraction, experience, show, screening, exhibit, venue, zone, queue, wait time, capacity, admission, ticket, schedule, wayfinding, notices, and subtype overrides; identify Restaurant inheritance and neutral organization-wide fallbacks; distinguish operator-facing and visitor-facing language; keep language separate from permissions and entitlements; remain documentation-only; and hand off to RWP-00.66.
