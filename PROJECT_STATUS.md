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
| Hospitality | **RWP-00.56** | Industry, subtypes, terminology, operating characteristics, required core, and optional capability candidates are complete in this proposed merge state. | **RWP-00.57 — Capability Classification (#532)** |
| Entertainment & Attractions | **RWP-00.67** | Industry definition, venue subtypes, terminology, operating characteristics, and the smallest viable required capability set are complete in this proposed merge state. | **RWP-00.68 — Optional Capabilities (#543)** |

Only merged documents are authoritative. An industry advances only after its current RWP is merged, verified, closed, and released.

## Hospitality Optional-Capability Result

RWP-00.56 separates optional Hospitality capabilities into advanced Vennusign workflow and governance tier candidates, independent external or managed-service add-ons, permissions, represented product state, quantity or consumption limits, and internal rollout controls.

Tier candidates include advanced wayfinding, brand libraries, centralized property-group coordination, campaigns, approvals, advanced localization workflow, advanced analytics, enterprise administration, and selected advanced operational workflows. Add-on candidates include property, event, room, transport, guest-service, access, gaming, mapping, positioning, emergency, weather, translation, AI, identity-provider, managed hardware, connectivity, monitoring, and related external connections or services.

The required manual core remains unchanged: property and guest information, hours and states, events and directories, manual wayfinding, basic language variants, explicit targeting and preview, publishing and delivery confidence, offline/outdated/conflict awareness, correction, expiry, supersession, retry, restoration, permissions, privacy-safe audiences, and last-known-good content.

Every optional candidate must define manual fallback, source authority and freshness, privacy and audience, permissions, limits and consumption, failure and disconnect behavior, correction, delivery confidence, downgrade or cancellation, data retention, and recovery before implementation.

## Entertainment Required-Capability Result

RWP-00.67 defines eleven required core groups: venue and visitor context; programs, schedules, shows, screenings, events, sessions, and experiences; closures and disruptions; queue, wait, capacity, and admission communication; manual wayfinding; notices and safety-related communication; basic multilingual and accessible content; targeting, preview, scheduling, and publication; delivery confidence and screen health; source, freshness, conflict, override, and recovery; and permissions, privacy-safe audiences, and authority boundaries.

Essential manual operation remains available without premium tiers or paid integrations. Automated synchronization, live occupancy or ticket inventory, advanced mapping, coordinated workflow, premium analytics, prediction, optimization, translation automation, enterprise identity, AI, managed connectivity, and managed hardware remain outside required core.

## Track 0 Classification Policy

Every concern has exactly one primary classification: core capability, permission, product/domain state, tier entitlement, independent add-on, usage or quantity limit, or internal rollout flag.

Industry and subtype affect defaults, terminology, starter content, recommendations, and capability presentation. They are not entitlements. Essential daily operation remains core. Permissions do not determine commercial access. Product state is not a feature flag. Limits are not capabilities.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Integration and external-system tests remain skipped under the standing owner instruction.

## Next Actions

- After RWP-00.56 is merged, verified, closed, and released, execute **RWP-00.57 — Hospitality Capability Classification** (#532).
- After RWP-00.67 is merged, verified, closed, and released, execute **RWP-00.68 — Entertainment & Attractions Optional Capabilities** (#543).

Other owner-approved industry streams may proceed under the queued short-lived shared-file write protocol.

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, migrations, privacy systems, localization, analytics, ticketing, admissions, property-management, event, room-booking, transport, guest-service, access, gaming, mapping, AI, managed hardware, integrations, or later-phase work until the owner approves the completed Track 0 model and implementation packages.
