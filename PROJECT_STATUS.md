# Vennusign Project Status

## Current State

- Phase 13 — Customer Identity, Signup, and Onboarding: complete.
- Phase 14 and later: paused pending explicit owner approval.
- Active implementation WP/RWP: none.
- Active planning track: Track 0 — Capability, Packaging, and Entitlement Architecture (#488). Product implementation remains paused.
- Restaurant is the canonical approved baseline inherited by later native-industry profiles.
- RWP-13.06 — Trial-First Onboarding (#466) remains paused until Track 0 produces an owner-approved capability and packaging model.

## Native-Industry Track 0 Progress

| Industry | Completed through | Result | Next approved item |
| --- | --- | --- | --- |
| Bar, Brewery & Nightlife | **RWP-00.18** | Industry definition, venue subtypes, hybrid rules, terminology, and operating characteristics are documented. | **RWP-00.19 — Required Capabilities (#494)** |
| Café, Bakery & Dessert | **RWP-00.31** | Industry model, terminology, operating characteristics, and the required manual core are documented. | **RWP-00.32 — Optional Capabilities (#507)** |
| Food Truck & Concession | **RWP-00.50** | The complete industry profile is validated and ready for cross-industry consolidation. | **Complete — await the RWP-00.75 consolidation gate** |
| Hospitality | **RWP-00.62** | The complete Hospitality profile is validated and ready for cross-industry consolidation. | **Complete — await the RWP-00.75 consolidation gate** |
| Entertainment & Attractions | **RWP-00.74** | The complete RWP-00.63–00.73 profile is validated and ready for owner review and cross-industry consolidation. | **Complete — await the RWP-00.75 consolidation gate** |

Only merged documents are authoritative. An industry advances only after its current RWP is merged, verified, closed, and released.

## Café Required-Capability Result

RWP-00.31 defines the required manual Café core covering venue information, menu/product management, rapid availability, sell-out, batch, freshness and return updates, preorder/custom-order/pickup presentation, screen purpose, explicit targeting and preview, immediate publication and per-target confirmation, correction, supersession, undo, restoration, source/freshness/conflict awareness, manual fallback, permissions, accessibility, responsiveness, localization readiness, and complete operational feedback states.

## Café Operating-Characteristics Result

RWP-00.30 defines early and cross-midnight business days; independently active service, preorder, pickup, counter, table, and mixed-service contexts; batch-led availability; source-authoritative freshness guidance; rotating and seasonal products; rapid sell-out and return transitions; public preorder and pickup information; screen-purpose and source-conflict behavior; multi-venue safeguards; and subtype-specific operating rhythms.

## Bar Operating-Characteristics Result

RWP-00.18 defines cross-midnight venue and service periods; separate kitchen, bar, doors, event, last-entry, and locally authored last-call timing; high-frequency tap, item, release, and temporary-offer changes; table, bar, counter, and hybrid service models; bounded age/access and responsible-content presentation; entertainment and sports operations; reservations, guest lists, cover, tickets, and private-event distinctions; and subtype-specific operating rhythms.

## Hospitality Final Validation Result

RWP-00.62 validates RWP-00.51 through RWP-00.61 as one coherent Hospitality profile. Restaurant inheritance is preserved for embedded food-and-beverage venues while Hospitality adds property, accommodation, amenity, service, meeting/event, wayfinding, multilingual, source, delivery, recovery, and multi-property operating context.

Essential manual property information, hours, states, notices, amenities, services, outlets, meetings, events, directories, wayfinding, language variants, explicit targeting, preview, publication, delivery confirmation, offline/outdated awareness, correction, expiry, supersession, retry, undo, restoration, and shift-handoff visibility remain core. Industry and subtype remain non-commercial product configuration. Permissions, product state, tier entitlements, independent add-ons, limits, privacy/source authority, and rollout controls remain separate.

The proposed Operate, Coordinate, Portfolio, and Enterprise outcome archetypes are planning candidates only. External property, event, room-booking, transport, guest-service, access, mapping, translation, AI, identity, analytics-data, hardware, connectivity, monitoring, and managed services remain independent add-on candidates. Numeric limits, prices, trials, contracts, downgrade behavior, inheritance policy, guest personalization, emergency/legal obligations, and implementation sequencing remain owner decisions.

Hospitality onboarding reaches one confirmed active screen before contextual pricing or add-on prompts. The default dashboard is attention-first and task-first. Analytics distinguishes operational evidence from inference and requires source, freshness, coverage, formulas, privacy, retention, correction, export, and reconciliation disclosure.

No further Hospitality industry RWP is open. Hospitality must wait for the all-industry completion gate before RWP-00.75.

## Entertainment & Attractions Final Validation Result

RWP-00.74 validates RWP-00.63 through RWP-00.73 as one coherent Entertainment & Attractions profile. Restaurant inheritance is preserved while the profile adds bounded venue, experience, schedule, occurrence, queue, capacity, admission, route, notice, source, screen, delivery, and recovery context.

Essential manual venue and experience information, schedules, operating-state and disruption communication, queue/wait/capacity/admission guidance, wayfinding, notices, language and accessibility variants, explicit targeting, preview, publication, per-target delivery confidence, correction, retry, supersession, unpublish, undo, and restoration remain core. Product state, permission, tier entitlement, independent add-on, limit, privacy/source/rights, and rollout remain separate.

The proposed Operate, Coordinate, Portfolio, and Enterprise outcome archetypes are planning candidates only. Ticketing, admissions, access, queue/occupancy/footfall, maps, venue/cinema/show-control/collection/attraction/event/sports, CRM, POS, translation, AI, identity, hardware, connectivity, monitoring, and managed services remain independent add-on candidates. Numeric limits, prices, trials, contracts, downgrade behavior, inheritance policy, rights/privacy/legal obligations, provider availability, player/pairing behavior, analytics definitions, and implementation sequencing remain owner decisions.

Entertainment onboarding reaches one confirmed active screen before forced pricing or integrations. The default dashboard is exception-first and task-first. Analytics distinguishes publication, delivery, visitor measurement, attendance, conversion, and revenue and requires source, freshness, coverage, privacy, retention, export, and reconciliation disclosure.

No further Entertainment industry RWP is open. Entertainment & Attractions must wait for the all-industry completion gate before RWP-00.75.

## Food Truck & Concession Validation Result

RWP-00.50 validates RWP-00.39 through RWP-00.49 as one coherent profile. Restaurant inheritance is preserved; essential manual menu, availability, current-context, service-state, targeting, preview, publishing, per-target delivery confidence, correction, retry, and restoration remain core. Product state, permission, tier entitlement, independent add-on, limit, source/privacy, and rollout remain separate.

## Track 0 Classification Policy

Every concern has exactly one primary classification: core capability, permission, product/domain state, tier entitlement, independent add-on, usage or quantity limit, or internal rollout flag.

Industry and subtype affect defaults, terminology, starter content, recommendations, and capability presentation. They are not entitlements. Essential daily operation remains core. Permissions do not determine commercial access. Product state is not a feature flag. Limits are not capabilities.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Integration and external-system tests remain skipped under the standing owner instruction.

## Next Actions

- Execute **RWP-00.19 — Bar, Brewery & Nightlife Required Capabilities** (#494).
- Execute the next approved Café item after the current Café claim is merged, verified, closed, and released.
- Keep Food Truck & Concession complete through **RWP-00.50**.
- Keep Hospitality complete through **RWP-00.62**.
- Keep Entertainment & Attractions complete through **RWP-00.74**.
- Do not begin consolidation until RWP-00.26, RWP-00.38, RWP-00.50, RWP-00.62, and RWP-00.74 are all complete. When the gate is satisfied, begin **RWP-00.75 — Cross-Industry Capability Inventory**, not product implementation.

Other owner-approved industry streams may proceed under the queued short-lived shared-file write protocol.

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, migrations, privacy systems, localization, analytics, ticketing, admissions, property-management, event, room-booking, transport, guest-service, access, gaming, mapping, AI, managed hardware, integrations, or later-phase work until the owner approves the completed Track 0 model and implementation packages.
