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
| Hospitality | **RWP-00.55** | Industry definition, subtypes, terminology, operating characteristics, and the smallest viable required capability set are complete in this proposed merge state. | **RWP-00.56 — Optional Capabilities (#531)** |
| Entertainment & Attractions | **RWP-00.66** | Industry definition, venue subtypes, terminology, and operating characteristics are complete in this proposed merge state. | **RWP-00.67 — Required Capabilities (#542)** |

Only merged documents are authoritative. An industry advances only after its current RWP is merged, verified, closed, and released.

## Hospitality Required-Capability Result

RWP-00.55 keeps essential daily operation core across eleven groups: property and local-context information; guest notices and operating-state communication; amenity, service, and outlet hours and availability; meetings, events, and directories; manual wayfinding; basic multilingual and accessible content; explicit screen targeting and preview; publishing and delivery confidence; offline, outdated, conflict, and recovery awareness; permissions and privacy-safe audiences; and required operational states.

Manual operation remains available without premium tiers or paid integrations. Product values, source, freshness, audience, targets, delivery state, permissions, commercial packaging, add-ons, limits, privacy, and rollout remain separate. Public operation does not assume guest-specific data, room readiness, eligibility, access, capacity, wait, quantity, route, translation quality, or source freshness.

Automated synchronization, personalization, live operational data, advanced workflow, premium analytics, optimization, prediction, and AI remain outside required core and cannot replace manual editing, targeting, publishing, confirmation, correction, expiration, supersession, retry, and restoration.

## Entertainment Operating-Characteristics Result

RWP-00.66 separates venue operating days, timed occurrences, continuously available experiences, last-entry behavior, queues and wait times, capacity and admissions, attractions and exhibits, closures and recovery, safety and accessibility notices, multilingual content, event surges, source freshness, and subtype-specific operating rhythms.

Essential manual schedule, state, queue, wait, capacity, wayfinding, notice, targeting, publishing, confirmation, offline-awareness, and restoration operation remains core. Operating values are product/domain state. Authority is permission. External synchronization, advanced coordination, analytics, premium localization, identity, AI, and managed hardware remain later tier or add-on candidates. Quantities remain limits and temporary release control remains a rollout flag.

## Track 0 Classification Policy

Every concern has exactly one primary classification: core capability, permission, product/domain state, tier entitlement, independent add-on, usage or quantity limit, or internal rollout flag.

Industry and subtype affect defaults, terminology, starter content, recommendations, and capability presentation. They are not entitlements. Essential daily operation remains core. Permissions do not determine commercial access. Product state is not a feature flag. Limits are not capabilities.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Integration and external-system tests remain skipped under the standing owner instruction.

## Next Actions

- After RWP-00.55 is merged, verified on `master`, issue #530 is closed, and its claim is released, execute **RWP-00.56 — Hospitality Optional Capabilities** (#531).
- After RWP-00.66 is merged, verified on `master`, issue #541 is closed, and its claim is released, execute **RWP-00.67 — Entertainment & Attractions Required Capabilities** (#542).

Other owner-approved industry streams may proceed under the queued short-lived shared-file write protocol.

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, migrations, privacy systems, localization, analytics, property-management, event, room-booking, transport, guest-service, access, gaming, integrations, or later-phase work until the owner approves the completed Track 0 model and implementation packages.
