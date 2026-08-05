# RWP-00.53 — Hospitality Business Terminology

## Status

Complete in this proposed merge state.

## Issue

- #528

## Objective

Define canonical Hospitality terminology for property, guest, stay, room or accommodation, amenities, services, outlets, events, meeting spaces, wayfinding, service hours, notices, operating states, subtype preferences, mixed-property fallbacks, and operator-versus-guest wording. Keep language separate from permissions, entitlements, privacy, authority, limits, and rollout. Documentation only.

## Dependency verified

- RWP-00.52 is merged, verified, closed, and released.
- Restaurant remains the canonical baseline.
- The merged Hospitality industry and subtype model is authoritative.
- RWP-00.53 has an issue, visible claim, branch, and pull request.
- RWP-00.54 — Hospitality Operating Characteristics (#529) is the approved next item.

## Delivered

- Added `track0/industries/hospitality-terminology.md` as the durable terminology companion.
- Defined neutral object vocabulary for organization, property, property group, guest, visitor, stay, room, accommodation, hierarchy, venue, outlet, amenity, service, event, meeting space, function space, schedule, service hours, notice, wayfinding, destination, screen, publish, and restore.
- Defined arrival, check-in, stay, departure, and check-out language without implying room readiness, reservation state, eligibility, or private guest information.
- Defined non-overlapping wording for available, limited, open, closed, temporarily closed, unavailable, out of service, paused, delayed, canceled, relocated, maintenance, weather-affected, restricted, and unknown states.
- Defined return-time wording that distinguishes forecast, schedule, unknown time, and next update.
- Separated regular, effective, special, and access hours, plus last service, last entry, last seating, last shuttle, and overnight periods.
- Defined public wayfinding content order and safeguards around current location, accessible routes, distance, and temporary route changes.
- Defined explicit operator action labels while preserving save, approve, publish, schedule, confirm delivery, and restore as distinct actions.
- Separated operator-facing source, freshness, scope, permission, delivery, and recovery detail from concise guest-facing wording.
- Defined terminology preferences for all nine Hospitality subtypes plus neutral and mixed contexts.
- Defined customer-authored, organization-template, imported-source, mixed-property, and analytics language behavior.
- Applied project-local Impeccable `clarify` guidance and preserved the approved Sky Blue administrative direction.

## Core result

Neutral organization-wide terms are **property**, **accommodation**, **area**, **venue**, **outlet**, **amenity**, **service**, **event**, **meeting space**, **notice**, and **destination**. Subtype language may replace these defaults only when precise for the selected property. Customer-authored names remain authoritative unless invalid, unsafe, privacy-sensitive, or superseded by an approved authoritative source.

Public signage must not expose a guest name, a room assignment tied to a person, reservation code, loyalty state, access credential, payment state, stay dates, service request, itinerary, or other guest-specific information by default.

## Classification decisions

1. Industry, subtype, terminology preference, customer-authored labels, imported labels, neutral fallbacks, property hierarchy, hours, schedules, notice type, destination, and current operating state are **product/domain state** where represented.
2. Authorized manual editing of public terminology, notices, hours, wayfinding, and state wording is a **core capability**.
3. Edit, approve, publish, restore, screen-management, restricted-information, and organization-language authority is a **permission**.
4. Advanced brand libraries, coordinated group terminology, approvals, localization workflow, and expanded analytics remain later **tier-entitlement** candidates.
5. PMS, event, room-booking, transport, point-of-sale, guest-service, access, gaming, translation, AI, and automatic synchronization remain later **add-on** or tier candidates.
6. Property, building, room, venue, outlet, area, event, screen, user, language, integration, storage, history, and AI quantities are **limits**.
7. Experiments, migrations, temporary compatibility controls, and emergency disable controls are **internal rollout flags**.
8. Terminology does not grant access, alter privacy, transfer source authority, increase a limit, or change commercial access.

## Impeccable result

Future terminology and notice-management surfaces are **Operate** experiences. They must:

- keep property, object, audience, source, effective time, and screen scope visible;
- use persistent labels and explicit verb-object actions;
- distinguish regular, effective, special, access, availability, and future scheduled states;
- preview guest-facing wording and high-impact screen targets;
- preserve authored content, source relationships, authority, privacy, and the last known good state;
- cover first-use, empty, loading, permission, validation, stale-source, conflict, offline, publish-failure, partial-delivery, success, undo, and restoration states;
- use honest wording without invented availability, access, capacity, room readiness, wait, reopening, or alternatives;
- support keyboard and assistive technology, non-color status cues, 200% zoom, long names, localization expansion, right-to-left readiness, local date/time clarity, and phone through large-desktop layouts.

No UI or implementation contract was created.

## Validation

Documentation-only review confirmed:

- every issue-listed terminology area has a canonical definition or neutral fallback;
- Restaurant terminology remains inherited for food-and-beverage outlets;
- subtype preferences tune defaults rather than packaging;
- mixed-property language remains neutral while preserving local names;
- operator and guest language intentionally expose different detail;
- privacy and source-authority boundaries are explicit;
- action verbs and operating states are non-overlapping and recovery-oriented;
- one primary Track 0 classification is assigned to each concern;
- RWP-00.54 is the next sequential Hospitality item.

GitHub Actions is authoritative for lightweight documentation validation on the exact reviewed pull-request head.

## Skipped under standing owner instruction

- Azure SQL and all external-service, credentialed, hosted-infrastructure, container, physical-device, signing/store, cross-system, and other integration-type tests.
- Runtime, UI, API, schema, migration, billing, entitlement, permission, privacy, localization, translation, AI, analytics, PMS, event, room-booking, transport, point-of-sale, guest-service, access, gaming, and integration implementation.

## Exact next action

After this RWP is merged, verified on `master`, issue #528 is closed, and the claim is released, execute **RWP-00.54 — Hospitality Operating Characteristics** (#529).

RWP-00.54 must document continuous property operation, shifts and handoffs, arrival and departure cycles, guest notices, amenities, outlets, meetings and events, wayfinding, emergency messaging, multilingual needs, property groups, subtype differences, defaults, and capability presentation; remain documentation-only; and hand off to RWP-00.55.