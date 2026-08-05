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
| Café, Bakery & Dessert | **RWP-00.38** | The complete Café profile is validated and ready for cross-industry consolidation. | **Complete — await the RWP-00.75 consolidation gate** |
| Food Truck & Concession | **RWP-00.50** | The complete industry profile is validated and ready for cross-industry consolidation. | **Complete — await the RWP-00.75 consolidation gate** |
| Hospitality | **RWP-00.62** | The complete Hospitality profile is validated and ready for cross-industry consolidation. | **Complete — await the RWP-00.75 consolidation gate** |
| Entertainment & Attractions | **RWP-00.72** | Industry model, packaging, onboarding, and exception-first default dashboard are documented. | **RWP-00.73 — KPIs & Analytics (#548)** |

Only merged documents are authoritative. An industry advances only after its current RWP is merged, verified, closed, and released.

## Café Final Validation Result

RWP-00.38 validates RWP-00.27 through RWP-00.37 as one coherent Café, Bakery & Dessert profile. Restaurant inheritance is preserved while Café adds bounded subtype, terminology, early/cross-midnight operation, batch, freshness-guidance, rotating-product, sell-out, expected-return, preorder, pickup, service-context, dashboard, and measurement differences.

Essential manual venue and menu management, rapid availability and freshness updates, operating information, preorder/pickup presentation, screen pairing and purpose, explicit targeting, preview, immediate publication, per-target confirmation, source/freshness visibility, correction, retry, undo, restoration, permissions, complete operating states, accessibility, responsive operation, and manual fallback remain core.

Industry and subtype remain non-commercial product configuration. Product/domain state, permission, tier entitlement, independent add-on, limits, privacy/source relationships, and rollout controls remain separate. The proposed Operate, Coordinate, Portfolio, and Enterprise outcome archetypes are planning candidates only. POS, inventory, production, ordering, payment, fulfillment, loyalty, messaging, translation, AI, managed hardware, connectivity, monitoring, support, and other external or managed services remain independent add-on candidates.

Café onboarding reaches one confirmed first-screen update before prominent pricing or optional-capability prompts. The default dashboard is exception-first and task-first. Analytics separates Vennusign operational evidence from sales, demand, inventory, conversion, readiness, queue, customer-behavior, and attribution claims that require authoritative external data.

The current cross-industry capability matrix already represents the meaningful normalized Café deltas; the detailed RWP-00.33 record provides the Café-specific classification expansion without duplicating matrix rows.

No further Café industry RWP is open. Café must wait for the all-industry completion gate before RWP-00.75.

## Bar Operating-Characteristics Result

RWP-00.18 defines cross-midnight venue and service periods; separate kitchen, bar, doors, event, last-entry, and locally authored last-call timing; high-frequency tap, item, release, and temporary-offer changes; table, bar, counter, and hybrid service models; bounded age/access and responsible-content presentation; entertainment and sports operations; reservations, guest lists, cover, tickets, and private-event distinctions; and subtype-specific operating rhythms.

## Hospitality Final Validation Result

RWP-00.62 validates RWP-00.51 through RWP-00.61 as one coherent Hospitality profile. Restaurant inheritance is preserved for embedded food-and-beverage venues while Hospitality adds property, accommodation, amenity, service, meeting/event, wayfinding, multilingual, source, delivery, recovery, and multi-property operating context.

Essential manual property information, hours, states, notices, amenities, services, outlets, meetings, events, directories, wayfinding, language variants, explicit targeting, preview, publication, delivery confirmation, offline/outdated awareness, correction, expiry, supersession, retry, undo, restoration, and shift-handoff visibility remain core. Industry and subtype remain non-commercial product configuration. Permissions, product state, tier entitlements, independent add-ons, limits, privacy/source authority, and rollout controls remain separate.

The proposed Operate, Coordinate, Portfolio, and Enterprise outcome archetypes are planning candidates only. External property, event, room-booking, transport, guest-service, access, mapping, translation, AI, identity, analytics-data, hardware, connectivity, monitoring, and managed services remain independent add-on candidates. Numeric limits, prices, trials, contracts, downgrade behavior, inheritance policy, guest personalization, emergency/legal obligations, and implementation sequencing remain owner decisions.

Hospitality onboarding reaches one confirmed active screen before contextual pricing or add-on prompts. The default dashboard is attention-first and task-first. Analytics distinguishes operational evidence from inference and requires source, freshness, coverage, formulas, privacy, retention, correction, export, and reconciliation disclosure.

No further Hospitality industry RWP is open. Hospitality must wait for the all-industry completion gate before RWP-00.75.

## Entertainment Dashboard Result

RWP-00.72 defines an exception-first task dashboard. The persistent context shows organization, venue, area or experience, local operating time, current state, and scoped authority. Urgent public-impact and recovery exceptions appear before analytics or promotion.

## Food Truck & Concession Validation Result

RWP-00.50 validates RWP-00.39 through RWP-00.49 as one coherent profile. Restaurant inheritance is preserved; essential manual menu, availability, current-context, service-state, targeting, preview, publishing, per-target delivery confidence, correction, retry, and restoration remain core. Product state, permission, tier entitlement, independent add-on, limit, source/privacy, and rollout remain separate.

## Track 0 Classification Policy

Every concern has exactly one primary classification: core capability, permission, product/domain state, tier entitlement, independent add-on, usage or quantity limit, or internal rollout flag.

Industry and subtype affect defaults, terminology, starter content, recommendations, and capability presentation. They are not entitlements. Essential daily operation remains core. Permissions do not determine commercial access. Product state is not a feature flag. Limits are not capabilities.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Integration and external-system tests remain skipped under the standing owner instruction.

## Next Actions

- Execute **RWP-00.19 — Bar, Brewery & Nightlife Required Capabilities** (#494).
- Keep Café, Bakery & Dessert complete through **RWP-00.38**.
- Execute **RWP-00.73 — Entertainment & Attractions KPIs & Analytics** (#548) after RWP-00.72 is merged, verified, closed, and released.
- Keep Food Truck & Concession complete through **RWP-00.50**.
- Keep Hospitality complete through **RWP-00.62**.
- Do not begin consolidation until RWP-00.26, RWP-00.38, RWP-00.50, RWP-00.62, and RWP-00.74 are all complete.

Other owner-approved industry streams may proceed under the queued short-lived shared-file write protocol.

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, migrations, privacy systems, localization, analytics, ticketing, admissions, property-management, event, room-booking, transport, guest-service, access, gaming, mapping, AI, managed hardware, integrations, or later-phase work until the owner approves the completed Track 0 model and implementation packages.