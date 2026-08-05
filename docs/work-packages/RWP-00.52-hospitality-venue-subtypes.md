# RWP-00.52 — Hospitality Venue Subtypes

## Status

Complete in this proposed merge state.

## Issue

- #527

## Objective

Define the supported Hospitality property subtypes, their bounded inclusion and exclusion rules, hybrid behavior, Restaurant inheritance, subtype selection and change behavior, and meaningful operating and presentation differences without creating separate entitlement models or implementing product behavior.

## Dependency verified

- RWP-00.51 is complete and merged.
- The canonical Restaurant baseline and merged Hospitality industry definition were used as authority.
- No competing open pull request, branch, or active tracker assignment owned this RWP when claimed.
- RWP-00.53 — Hospitality Business Terminology (#528) is the approved next item.

## Delivered

- Expanded `track0/industries/hospitality.md` with the canonical property-subtype model.
- Defined nine primary subtypes: Hotel, Resort, Motel, Hostel, Extended-Stay, Serviced Apartment, Conference Property, Casino Resort, and Boutique Lodging.
- Defined an Unspecified / General Hospitality Property neutral fallback without creating another commercial package.
- Established inclusion, exclusion, neighboring-profile, hybrid, and ambiguous-case rules for every subtype.
- Mapped each subtype to inherited Restaurant and Hospitality capabilities and recorded only meaningful operational, content, screen-purpose, and presentation deltas.
- Resolved city hotels with conference business, destination hotels, inns and lodges, roadside properties, aparthotels, vacation clubs, branded residences, casino hotels, private-room hostels, attached convention centers, mixed resorts, multi-building campuses, and management-company portfolios.
- Defined organization defaults, local subtype selection, subtype change, mixed-property, multi-property, and cross-subtype copied-content behavior.
- Kept property hierarchy, brand, star rating, franchise, ownership, management, authority, privacy, commercial access, and future quantity-limit counting separate from subtype.
- Updated `track0/CAPABILITY_MATRIX.md` so Hospitality primary subtype, neutral state, and optional descriptive traits remain product/domain state.
- Consulted the project-local Impeccable skill and `shape` guidance for future subtype selection and change flows.
- Preserved the approved Sky Blue administrative direction.

## Canonical subtype result

The approved primary catalog is:

1. Hotel
2. Resort
3. Motel
4. Hostel
5. Extended-Stay
6. Serviced Apartment
7. Conference Property
8. Casino Resort
9. Boutique Lodging

A property may remain **Unspecified / General Hospitality Property** when no supported subtype clearly controls its daily operating rhythm.

Hybrid concepts use one primary subtype plus optional descriptive traits such as destination or recreation focus, all-inclusive service, wellness, conference and wedding emphasis, gaming, heritage, independent or lifestyle positioning, apartment-style units, extended-stay service, campus or multi-building form, seasonal operation, and mixed-use outlets. Traits do not stack entitlements, transfer authority, or increase limits.

## Impeccable planning result

The future subtype selection and change experience is an **Operate** surface for an owner, administrator, or authorized property manager.

Because this is a non-interactive planning run, the following assumptions were made explicit: selection is local to a Hospitality property; organization industry may suggest but cannot override the local choice; subtype overlap is common; mixed properties contain local venues with other approved business types; and existing content, screen assignments, history, authority, privacy boundaries, and commercial access must be preserved.

The brief requires:

- bounded “best when” definitions based on dominant guest journey, arrival and circulation pattern, length of stay, shared accommodation, amenity and event breadth, gaming, vehicle access, and daily information rhythm rather than legal, brand, star-rating, room-count, ownership, management, tax, or marketing classifications;
- one primary subtype, a neutral fallback, and optional descriptive traits for secondary operating characteristics;
- an explicit explanation that subtype changes defaults and recommendations, not plan access, privacy scope, authority, or quantity allowances;
- a preview of changed terminology candidates, starter-content suggestions, screen purposes, and operating guidance before applying a subtype change;
- preservation of customer-authored content, screens, pairing, targeting, themes, schedules, publication history, current property and service state, custom terminology, authority boundaries, and commercial access;
- confirmation, safe cancellation, permission-restricted, validation-failure, interrupted-save, success, and restoration states;
- scannable phone and desktop behavior, progressive disclosure for overlap cases, keyboard and assistive-technology support, localization expansion, 200% zoom, plain language, and no color-only distinctions;
- clear handling of small properties through multi-building resorts, conference campuses, casino resorts, and multi-brand management portfolios without implying that subtype selection performs integrations or migrates physical property structure.

No UI or implementation contract was created.

## Classification decisions

1. Organization primary industry, primary Hospitality subtype, neutral subtype state, and optional descriptive traits are **product/domain state**.
2. Subtype may affect terminology candidates, starter content, recommendations, screen-purpose suggestions, and operating guidance only.
3. Subtype does not grant capabilities, change plan access, alter permissions, increase limits, control rollout, transfer authority, or act as a subscription entitlement.
4. Property, building, tower, wing, floor, area, outlet, room, event, amenity, service window, closure, relocation, and similar values keep their product/domain-state classifications independent of subtype.
5. Brand, star rating, franchise, ownership, management, marketing language, and property architecture do not become hidden feature flags.
6. Subtype-specific screen purposes are recommendations using inherited or later-classified capabilities.
7. Manual guest information, wayfinding, event, amenity, service, closure, relocation, changed-hours, targeting, publishing, delivery confirmation, offline awareness, and recovery remain core.
8. Counts of properties, buildings, rooms, venues, outlets, areas, events, screens, users, integrations, storage, retained history, or AI consumption remain independent limits.
9. Automatic property-management, event, room-booking, point-of-sale, transport, guest-service, access, gaming, or related synchronization remains a later integration-packaging question and cannot replace manual core operation.
10. Guest-specific, reservation-specific, room-specific, member-specific, or sensitive operational information remains subject to later privacy and authorization decisions and is not assumed to be public signage content.

## Validation

Documentation-only review confirmed:

- every issue-listed canonical subtype has a bounded definition;
- Restaurant inheritance is retained and not duplicated as a new commercial model;
- hybrid and ambiguous properties use one primary subtype plus optional descriptive traits;
- subtype selection and change preserve customer content, screens, state, authority, privacy boundaries, and commercial access;
- mixed-property and multi-property behavior remains explicit;
- property hierarchy, venue scope, ownership, management, brand, authority, and future limit counting remain separate;
- the capability matrix has one primary classification for subtype-related concerns;
- Impeccable `shape` guidance covers job, audience, outcome, hierarchy, states, realistic ranges, interaction, responsiveness, accessibility, feedback, and recovery;
- the next sequential item is RWP-00.53.

GitHub Actions is authoritative for lightweight documentation validation on the exact pull-request head.

## Skipped under standing owner instruction

- Azure SQL and all external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and other integration-type tests.
- Runtime, UI, API, schema, migration, billing, entitlement, permission, feature-gate, limit, rollout, privacy-system, property-management, event, room-booking, transport, guest-service, gaming, and integration implementation.

## Exact next action

After this RWP is merged, verified on `master`, issue #527 is closed, and the claim is released, execute **RWP-00.53 — Hospitality Business Terminology** (#528).

RWP-00.53 must define canonical terminology for property, guest, stay, room, amenity, venue, outlet, event, meeting space, wayfinding, service hours, notices, and subtype overrides; identify Restaurant inheritance and neutral organization-wide fallbacks; keep language separate from permissions and entitlements; remain documentation-only; and hand off to RWP-00.54.
