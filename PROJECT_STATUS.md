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
| Bar, Brewery & Nightlife | **RWP-00.17** | Industry definition, nine venue subtypes plus neutral fallback, hybrid rules, canonical business terminology, subtype terminology, neutral fallbacks, operator/guest language, analytics labels, and Impeccable clarification guidance are documented. | **RWP-00.18 — Operating Characteristics (#493)** |
| Café, Bakery & Dessert | **RWP-00.29** | Industry definition, nine venue subtypes plus neutral fallback, hybrid rules, canonical product, size, option, batch, freshness, availability, preorder, pickup, and service-period terminology, subtype preferences, neutral fallbacks, operator/guest language, analytics labels, and Impeccable clarification guidance are documented. | **RWP-00.30 — Operating Characteristics (#505)** |
| Food Truck & Concession | **RWP-00.41** | Industry definition, nine venue subtypes plus neutral fallback, physical-form and operating-context traits, host boundaries, canonical operation, location, event, menu, combo, availability, pickup, queue, relocation, and operating-state terminology, subtype preferences, neutral fallbacks, operator/guest language, analytics labels, and Impeccable clarification guidance are documented. | **RWP-00.42 — Operating Characteristics (#517)** |
| Hospitality | **RWP-00.52** | Industry definition, nine bounded property subtypes plus neutral fallback, hybrid traits, inclusion/exclusion rules, Restaurant inheritance, mixed-property and multi-property behavior, subtype-change preservation, classification, and Impeccable planning guidance are documented in this proposed merge state. | **RWP-00.53 — Business Terminology (#528)** |
| Entertainment & Attractions | **RWP-00.63** | Industry definition is documented. | **RWP-00.64 — Venue Subtypes (#539)** |

Only merged documents are authoritative. An industry may advance only after its current RWP is merged, verified, closed, and released.

## Hospitality Venue-Subtype Result

RWP-00.52 establishes nine primary Hospitality property subtypes:

1. Hotel
2. Resort
3. Motel
4. Hostel
5. Extended-Stay
6. Serviced Apartment
7. Conference Property
8. Casino Resort
9. Boutique Lodging

A property may remain **Unspecified / General Hospitality Property** when no supported subtype clearly controls its daily operating rhythm. The fallback is neutral product state, not a commercial package.

### Selection and hybrid rules

- Select the subtype that best describes the dominant daily guest journey and information rhythm, not legal status, brand, star rating, room count, ownership, management, tax treatment, architecture, or marketing language.
- When models overlap, use one primary subtype and retain secondary destination, recreation, wellness, conference, wedding, gaming, heritage, lifestyle, apartment-style, extended-stay, campus, seasonal, or mixed-use characteristics as descriptive traits.
- Traits tune defaults and future terminology only. They do not stack entitlements, increase limits, transfer authority, or become hidden feature flags.
- Organization primary industry may seed a suggestion but cannot override the property's local subtype.
- Different properties in one organization may select different subtypes; distinct restaurants, bars, cafés, concessions, attractions, retail, spas, and event venues may use their own approved local business types.

### Classification

- Organization primary industry, primary Hospitality subtype, neutral subtype state, and optional descriptive traits are **product/domain state**.
- Subtype changes terminology candidates, starter recommendations, screen-purpose suggestions, operating emphasis, and presentation guidance only.
- Subtype does not grant capabilities, alter permissions, change plan access, increase limits, control rollout, transfer authority, or change commercial access.
- Property, building, tower, wing, floor, area, outlet, room, event, amenity, service-window, closure, relocation, and similar values keep independent product-state classifications.
- Manual guest information, wayfinding, event, amenity, service, closure, relocation, changed-hours, targeting, publishing, delivery confirmation, offline awareness, and recovery remain core.
- Counts remain limits. Property-management, event, room-booking, transport, guest-service, access, gaming, and related synchronization remain later integration-packaging questions.
- Guest-specific, reservation-specific, room-specific, member-specific, and sensitive operational information remains subject to later privacy and authorization decisions.

### Impeccable planning

The project-local Impeccable `shape` guidance applies to future subtype selection and change flows. The surface is an Operate experience for an owner or authorized property manager. It must compare bounded “best when” definitions; support one primary subtype, a neutral fallback, and optional traits; preview changed defaults; preserve all customer-authored content, screens, state, history, authority, privacy boundaries, and commercial access; cover first-run, neutral, recommended, no-match, change-preview, permission, failure, success, and restoration states; remain scannable on phone and desktop; support keyboard, assistive technology, localization expansion, and 200% zoom; avoid color-only meaning; and preserve the approved Sky Blue administrative direction.

No UI, API, schema, migration, billing, entitlement, feature-gate, limit-counting, privacy-system, property-management, event, room-booking, transport, guest-service, gaming, or integration implementation was performed.

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

## Completed Delivery

Phases 02 through 13 are complete. The retrospective remediation queues through RWP-11.04 are complete. RWP-00.14 installed the project-local Impeccable Codex design skill. Earlier package details, validation evidence, and status snapshots remain under `docs/archive/` for deliberate research.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Integration and external-system tests requiring Azure SQL, external services, credentials, hosted infrastructure, containers, devices, signing/store access, or cross-system integration remain skipped under the standing owner instruction.

## Next Action

After RWP-00.52 is merged, verified on `master`, issue #527 is closed, and the claim is released, continue the Hospitality queue with **RWP-00.53 — Business Terminology** (#528).

RWP-00.53 must define canonical terminology for property, guest, stay, room, amenity, venue, outlet, event, meeting space, wayfinding, service hours, notices, and subtype overrides; identify Restaurant inheritance and neutral organization-wide fallbacks; keep language separate from permissions and entitlements; remain documentation-only; and hand off to RWP-00.54.

Other owner-approved native-industry schedules may continue independently inside their own sequential queues. They must use Restaurant as the canonical baseline, treat only merged work as authoritative, and avoid concurrent edits to shared controlled files.

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, migrations, privacy systems, property-management behavior, event or room-booking behavior, transport or guest-service automation, gaming behavior, or later-phase work until the owner approves the completed capability matrix and implementation packages.
