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
| Entertainment & Attractions | **RWP-00.71** | Industry model, classification, tier architecture, and first-value onboarding experience are complete in this proposed merge state. | **RWP-00.72 — Default Dashboard (#547)** |

Only merged documents are authoritative. An industry advances only after its current RWP is merged, verified, closed, and released.

## Bar Operating-Characteristics Result

RWP-00.18 defines cross-midnight venue and service periods; separate kitchen, bar, doors, event, last-entry, and locally authored last-call timing; high-frequency tap, item, release, and temporary-offer changes; table, bar, counter, and hybrid service models; bounded age/access and responsible-content presentation; entertainment and sports operations; reservations, guest lists, cover, tickets, and private-event distinctions; and subtype-specific operating rhythms.

Essential manual availability, hours, specials, events, public guidance, explicit targeting, preview, publishing, delivery confirmation, correction, offline/outdated awareness, supersession, and restoration remain core. Operating values are product/domain state. Authority is permission. Advanced workflow is a tier candidate. External synchronization is an add-on candidate. Quantities are limits and temporary delivery controls are rollout flags.

## Hospitality Optional-Capability Result

RWP-00.56 separates optional Hospitality capabilities into advanced native workflow and governance tier candidates, independent external or managed-service add-ons, represented product state, permissions, limits, and rollout controls. Required manual core remains unchanged, and every optional candidate requires manual fallback, source/freshness, privacy, safe failure, cancellation, downgrade, retention, delivery confidence, and recovery.

## Entertainment Onboarding Result

RWP-00.71 defines the aha moment as accurate venue-specific visitor information visibly delivered to the first paired screen with clear update, verification, and recovery confidence. The minimum journey covers venue identity, simple structure, first screen purpose, pairing or selection, subtype-aware starter content, one useful live update, preview/publication, and delivery confirmation.

The flow uses real setup, progressive disclosure, durable save/resume checkpoints, optional guidance, role-aware permissions, mobile and accessibility support, and recovery from pairing, source, save, and publish failures. It does not require an external integration, complete venue modeling, or forced tier comparison. Pricing remains deliberately accessible but is preferably introduced contextually after first-screen activation.

## Track 0 Classification Policy

Every concern has exactly one primary classification: core capability, permission, product/domain state, tier entitlement, independent add-on, usage or quantity limit, or internal rollout flag.

Industry and subtype affect defaults, terminology, starter content, recommendations, and capability presentation. They are not entitlements. Essential daily operation remains core. Permissions do not determine commercial access. Product state is not a feature flag. Limits are not capabilities.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Integration and external-system tests remain skipped under the standing owner instruction.

## Next Actions

- Execute **RWP-00.19 — Bar, Brewery & Nightlife Required Capabilities** (#494).
- After RWP-00.56 is merged, verified, closed, and released, execute **RWP-00.57 — Hospitality Capability Classification** (#532).
- After RWP-00.71 is merged, verified, closed, and released, execute **RWP-00.72 — Entertainment & Attractions Default Dashboard** (#547).

Other owner-approved industry streams may proceed under the queued short-lived shared-file write protocol.

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, migrations, privacy systems, localization, analytics, ticketing, admissions, property-management, event, room-booking, transport, guest-service, access, gaming, mapping, AI, managed hardware, integrations, or later-phase work until the owner approves the completed Track 0 model and implementation packages.
