# Vennusign Project Status

## Current State

- Phase 13 — Customer Identity, Signup, and Onboarding: complete.
- Phase 14 and later: paused pending explicit owner approval.
- Active implementation WP/RWP: none.
- Active planning track: Track 0 — Capability, Packaging, and Entitlement Architecture (#488). Product implementation remains paused.
- The owner approved independent native-industry Track 0 schedules. Each industry remains sequential inside its own RWP range and must avoid shared-file conflicts.
- Restaurant is the canonical approved baseline inherited by later native-industry profiles.
- RWP-13.06 — Trial-First Onboarding (#466) remains paused until Track 0 produces an owner-approved capability and packaging model.

## Native-Industry Track 0 Progress

| Industry | Completed through | Result | Next approved item |
| --- | --- | --- | --- |
| Bar, Brewery & Nightlife | **RWP-00.17** | Industry definition, venue subtypes, hybrid rules, and business terminology are documented. | **RWP-00.18 — Operating Characteristics (#493)** |
| Café, Bakery & Dessert | **RWP-00.29** | Industry definition, venue subtypes, hybrid rules, and business terminology are documented. | **RWP-00.30 — Operating Characteristics (#505)** |
| Food Truck & Concession | **RWP-00.41** | Industry definition, venue subtypes, hybrid rules, and business terminology are documented. | **RWP-00.42 — Operating Characteristics (#517)** |
| Hospitality | **RWP-00.52** | Industry definition and nine bounded property subtypes plus neutral fallback are documented. | **RWP-00.53 — Business Terminology (#528)** |
| Entertainment & Attractions | **RWP-00.64** | Industry definition and twelve bounded venue subtypes plus neutral fallback, hybrid traits, ambiguous-boundary rules, subtype-change preservation, classifications, and Impeccable planning guidance are complete in this proposed merge state. | **RWP-00.65 — Business Terminology (#540)** |

Only merged documents are authoritative. An industry may advance only after its current RWP is merged, verified, closed, and released.

## Entertainment & Attractions Venue-Subtype Result

RWP-00.64 establishes twelve primary venue subtypes:

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

A venue may remain **Unspecified / General Entertainment & Attraction Venue** when no supported subtype clearly controls its daily visitor journey and information rhythm. The fallback is neutral product/domain state, not a commercial package.

### Selection and hybrid rules

- Select the subtype from the dominant visitor journey, program rhythm, physical movement, admissions model, schedule pattern, collection or attraction structure, and operating state.
- Do not use legal, statistical, licensing, venue-size, brand, ownership, management, promoter, presenter, or marketing language as the product classification.
- Use one primary subtype plus optional descriptive traits for outdoor, vehicle-based, water-based, seasonal, weather-sensitive, immersive, heritage, living-collection, education, admission, seating, membership, multi-use, campus, touring, resident-company, team-home, tournament, festival, environmental, or experience characteristics.
- Traits tune future defaults and recommendations only. They do not stack entitlements, increase limits, transfer authority, or become hidden feature flags.
- Organization primary industry may seed a suggestion but cannot override the local venue subtype.
- Mixed campuses, resorts, casinos, arena districts, cultural complexes, and destination portfolios may contain multiple locally typed venues and other approved industry types.

### Classification

- Organization primary industry, primary Entertainment subtype, neutral subtype state, and optional descriptive traits are **product/domain state**.
- Subtype changes terminology candidates, starter recommendations, screen-purpose suggestions, operating guidance, and presentation emphasis only.
- Subtype does not grant capabilities, alter permissions, change plan access, increase limits, control rollout, transfer authority, or change commercial access.
- Venue hierarchy and operational values keep independent product/domain-state classifications.
- Manual program, showtime, event, attraction, exhibit, admissions, wayfinding, queue, capacity, delay, closure, relocation, accessibility, safety, targeting, publishing, delivery confirmation, offline awareness, and recovery remain core.
- Counts remain limits. Ticketing, admissions, access-control, queue, venue, cinema, show-control, collection, attraction, event, sports, and related synchronization remain later integration-packaging questions.
- Visitor-specific, ticket-specific, seat-specific, member-specific, participant-specific, performer-specific, sponsor-specific, security-sensitive, and operationally sensitive information remains subject to later privacy and authorization decisions.

### Impeccable planning

The project-local Impeccable `shape` guidance applies to future subtype selection and change flows. The surface is an Operate experience for an owner, administrator, or authorized venue manager. It must compare bounded “best when” definitions; support one primary subtype, a neutral fallback, and optional traits; preview changed defaults; preserve authored content, screens, state, history, authority, privacy, integrations, limits, and commercial access; cover first-run, recommended, no-match, change-preview, permission, failure, success, and restoration states; remain scannable on phone and desktop; support keyboard, assistive technology, localization expansion, and 200% zoom; avoid color-only meaning; and preserve the approved Sky Blue administrative direction.

No UI, API, schema, migration, billing, entitlement, feature-gate, limit-counting, privacy-system, ticketing, admissions, access-control, queue-management, show-control, collection-management, attraction, event, sports, or other product implementation was performed.

## Track 0 Classification Policy

Every concern has exactly one primary classification:

1. Core capability
2. Permission
3. Product/domain state
4. Tier entitlement
5. Independent add-on
6. Usage or quantity limit
7. Internal rollout flag

Industry and subtype affect defaults, terminology, starter content, recommendations, and capability presentation. They are not entitlements. Essential daily operation remains core. Permissions do not determine commercial access. Product state is not a feature flag. Limits are not capabilities.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Integration and external-system tests requiring Azure SQL, external services, credentials, hosted infrastructure, containers, devices, signing/store access, or cross-system integration remain skipped under the standing owner instruction.

## Next Action

After RWP-00.64 is merged, verified on `master`, issue #539 is closed, and the claim is released, continue the Entertainment & Attractions queue with **RWP-00.65 — Business Terminology** (#540).

RWP-00.65 must define canonical terminology for attraction, experience, show, screening, exhibit, venue, zone, queue, wait time, capacity, admission, ticket, schedule, wayfinding, notices, and subtype overrides; identify Restaurant inheritance and neutral organization-wide fallbacks; distinguish operator-facing and visitor-facing language; preserve customer-authored names; keep language separate from permissions and entitlements; remain documentation-only; and hand off to RWP-00.66.

Other owner-approved native-industry schedules may continue independently inside their own sequential queues. They must use Restaurant as the canonical baseline, treat only merged work as authoritative, and avoid concurrent edits to shared controlled files.

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, migrations, privacy systems, ticketing, admissions, access-control, queue-management, venue-management, show-control, collection-management, attraction, event, sports, analytics, integrations, or later-phase work until the owner approves the completed capability matrix and implementation packages.
