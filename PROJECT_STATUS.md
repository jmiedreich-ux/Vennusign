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
| Bar, Brewery & Nightlife | **RWP-00.17** | Industry definition, venue subtypes, hybrid rules, and business terminology are documented. | **RWP-00.18 — Operating Characteristics (#493)** |
| Café, Bakery & Dessert | **RWP-00.29** | Industry definition, venue subtypes, hybrid rules, and business terminology are documented. | **RWP-00.30 — Operating Characteristics (#505)** |
| Food Truck & Concession | **RWP-00.42** | Industry definition, subtypes, terminology, and operating characteristics are documented. | **RWP-00.43 — Required Capabilities (#518)** |
| Hospitality | **RWP-00.53** | Industry definition, nine property subtypes plus neutral fallback, hybrid rules, and canonical business terminology are complete in this proposed merge state. | **RWP-00.54 — Operating Characteristics (#529)** |
| Entertainment & Attractions | **RWP-00.65** | Industry definition, venue subtypes, hybrid rules, and business terminology are documented. | **RWP-00.66 — Operating Characteristics (#541)** |

Only merged documents are authoritative. An industry advances only after its current RWP is merged, verified, closed, and released.

## Hospitality Terminology Result

RWP-00.53 defines neutral and subtype-aware language for property, property group, guest, visitor, stay, room or accommodation, building hierarchy, venue, outlet, amenity, service, event, meeting and function space, schedules, service and access hours, notices, wayfinding, destinations, screens, publishing, and restoration.

Public state wording now distinguishes available, limited, open, closed, temporarily closed, unavailable, out of service, paused, delayed, canceled, relocated, maintenance-affected, weather-affected, restricted, and unknown conditions. Expected reopening, scheduled reopening, unconfirmed timing, and next-update wording remain separate so the product does not invent certainty.

Manual public terminology, notices, hours, wayfinding, and state communication remain core. Customer-authored and imported labels, hierarchy, schedules, destinations, and operating values remain product/domain state. Permissions control editing, approval, publishing, restoration, and restricted information. Advanced brand, localization, approval, analytics, AI, and integration capabilities remain later tier or add-on candidates. Counts remain limits, and rollout controls remain internal.

Public signage must not expose guest identity, room assignments tied to a person, reservation codes, loyalty or access status, payment state, stay dates, service requests, itineraries, or other guest-specific information by default.

## Track 0 Classification Policy

Every concern has exactly one primary classification: core capability, permission, product/domain state, tier entitlement, independent add-on, usage or quantity limit, or internal rollout flag.

Industry and subtype affect defaults, terminology, starter content, recommendations, and capability presentation. They are not entitlements. Essential daily operation remains core. Permissions do not determine commercial access. Product state is not a feature flag. Limits are not capabilities.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Integration and external-system tests remain skipped under the standing owner instruction.

## Next Action

After RWP-00.53 is merged, verified on `master`, issue #528 is closed, and the claim is released, execute **RWP-00.54 — Hospitality Operating Characteristics** (#529).

Other owner-approved industry streams may proceed only when their owned paths do not conflict with the active assignment.

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, migrations, privacy systems, localization, analytics, property-management, event, room-booking, transport, guest-service, access, gaming, integrations, or later-phase work until the owner approves the completed Track 0 model and implementation packages.
