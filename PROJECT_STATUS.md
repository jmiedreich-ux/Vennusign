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
| Hospitality | **RWP-00.54** | Industry definition, nine property subtypes plus neutral fallback, canonical terminology, and operating characteristics are complete in this proposed merge state. | **RWP-00.55 — Required Capabilities (#530)** |
| Entertainment & Attractions | **RWP-00.65** | Industry definition, venue subtypes, hybrid rules, and business terminology are documented. | **RWP-00.66 — Operating Characteristics (#541)** |

Only merged documents are authoritative. An industry advances only after its current RWP is merged, verified, closed, and released.

## Hospitality Operating Result

RWP-00.54 defines Hospitality as a continuously operating property environment with independently changing buildings, areas, accommodations, desks, amenities, services, outlets, events, meeting spaces, routes, notices, screens, languages, and sources.

The operating model now covers shift handoffs; arrival, stay, departure, and overnight rhythms; privacy-safe guest notices; amenity, service, and outlet state; meetings and event changes; operational wayfinding; authorized urgent messaging; multilingual publication; property-group coordination; subtype-specific defaults; and safe recovery from stale sources, failed publication, partial delivery, and outdated screens.

Manual property information, notices, hours and states, event and meeting display, wayfinding, language variants, explicit targeting, publishing, confirmation, correction, expiration, supersession, and restoration remain core. Advanced shift workflow, group coordination, approvals, campaigns, interactive mapping, localization workflow, analytics, and managed monitoring remain tier candidates. Property, event, room-booking, transport, point-of-sale, guest-service, access, gaming, translation, AI, weather, emergency-management, map, positioning, and similar synchronization remain independent add-on candidates where integration is required. Quantities remain limits.

Public information must not expose guest identity or private stay data, and no surface may infer room readiness, eligibility, access, capacity, timing, route, translation quality, or source freshness that is not authoritative.

## Track 0 Classification Policy

Every concern has exactly one primary classification: core capability, permission, product/domain state, tier entitlement, independent add-on, usage or quantity limit, or internal rollout flag.

Industry and subtype affect defaults, terminology, starter content, recommendations, and capability presentation. They are not entitlements. Essential daily operation remains core. Permissions do not determine commercial access. Product state is not a feature flag. Limits are not capabilities.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Integration and external-system tests remain skipped under the standing owner instruction.

## Next Action

After RWP-00.54 is merged, verified on `master`, issue #529 is closed, and the claim is released, execute **RWP-00.55 — Hospitality Required Capabilities** (#530).

Other owner-approved industry streams may proceed only when their owned paths do not conflict with the active assignment.

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, migrations, privacy systems, localization, analytics, property-management, event, room-booking, transport, guest-service, access, gaming, integrations, or later-phase work until the owner approves the completed Track 0 model and implementation packages.
