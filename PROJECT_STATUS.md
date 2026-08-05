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
| Café, Bakery & Dessert | **RWP-00.29** | Industry definition, venue subtypes, hybrid rules, and business terminology are documented. | **RWP-00.30 — Operating Characteristics (#505)** |
| Food Truck & Concession | **RWP-00.42** | Industry definition, subtypes, terminology, and operating characteristics are documented. | **RWP-00.43 — Required Capabilities (#518)** |
| Hospitality | **RWP-00.56** | Industry, subtypes, terminology, operating characteristics, required core, and optional capability candidates are complete in this proposed merge state. | **RWP-00.57 — Capability Classification (#532)** |
| Entertainment & Attractions | **RWP-00.68** | Industry, subtypes, terminology, operating characteristics, required core, and optional capability candidates are complete in this proposed merge state. | **RWP-00.69 — Capability Classification (#544)** |

Only merged documents are authoritative. An industry advances only after its current RWP is merged, verified, closed, and released.

## Bar Operating-Characteristics Result

RWP-00.18 defines cross-midnight venue and service periods; separate kitchen, bar, doors, event, last-entry, and locally authored last-call timing; high-frequency tap, item, release, and temporary-offer changes; table, bar, counter, and hybrid service models; bounded age/access and responsible-content presentation; entertainment and sports operations; reservations, guest lists, cover, tickets, and private-event distinctions; and subtype-specific operating rhythms.

Essential manual availability, hours, specials, events, public guidance, explicit targeting, preview, publishing, delivery confirmation, correction, offline/outdated awareness, supersession, and restoration remain core. Operating values are product/domain state. Authority is permission. Advanced workflow is a tier candidate. External synchronization is an add-on candidate. Quantities are limits and temporary delivery controls are rollout flags.

## Hospitality Optional-Capability Result

RWP-00.56 separates optional Hospitality capabilities into advanced Vennusign workflow and governance tier candidates, independent external or managed-service add-ons, permissions, represented product state, quantity or consumption limits, and internal rollout controls.

Tier candidates include advanced wayfinding, brand libraries, centralized property-group coordination, campaigns, approvals, advanced localization workflow, advanced analytics, enterprise administration, and selected advanced operational workflows. Add-on candidates include property, event, room, transport, guest-service, access, gaming, mapping, positioning, emergency, weather, translation, AI, identity-provider, managed hardware, connectivity, monitoring, and related external connections or services.

The required manual core remains unchanged. Every optional candidate must define manual fallback, source authority and freshness, privacy and audience, permissions, limits and consumption, failure and disconnect behavior, correction, delivery confidence, downgrade or cancellation, data retention, and recovery before implementation.

## Entertainment Optional-Capability Result

RWP-00.68 defines fourteen optional families: ticketing/admissions/access synchronization; venue, cinema, show-control, collection, attraction, event, and sports synchronization; dynamic queues, wait, occupancy, capacity, and footfall; advanced maps and wayfinding; coordinated screens and event moments; campaigns, membership, sponsorship, and merchandising; multi-venue coordination; brand and template governance; approvals and audit workflow; premium localization; premium analytics and optimization; AI assistance; enterprise identity; and managed hardware, connectivity, deployment, and support.

Native recurring outcomes are tier candidates. Independent integrations and managed services are add-on candidates. Quantities and consumption are limits. Permissions remain authority, imported values remain product/domain state, and temporary release controls remain rollout flags. Required manual visitor communication, targeting, publishing, delivery confirmation, correction, and recovery remain available without premium packaging.

## Track 0 Classification Policy

Every concern has exactly one primary classification: core capability, permission, product/domain state, tier entitlement, independent add-on, usage or quantity limit, or internal rollout flag.

Industry and subtype affect defaults, terminology, starter content, recommendations, and capability presentation. They are not entitlements. Essential daily operation remains core. Permissions do not determine commercial access. Product state is not a feature flag. Limits are not capabilities.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Integration and external-system tests remain skipped under the standing owner instruction.

## Next Actions

- Execute **RWP-00.19 — Bar, Brewery & Nightlife Required Capabilities** (#494).
- After RWP-00.56 is merged, verified, closed, and released, execute **RWP-00.57 — Hospitality Capability Classification** (#532).
- After RWP-00.68 is merged, verified, closed, and released, execute **RWP-00.69 — Entertainment & Attractions Capability Classification** (#544).

Other owner-approved industry streams may proceed under the queued short-lived shared-file write protocol.

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, migrations, privacy systems, localization, analytics, ticketing, admissions, property-management, event, room-booking, transport, guest-service, access, gaming, mapping, AI, managed hardware, integrations, or later-phase work until the owner approves the completed Track 0 model and implementation packages.
