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
| Food Truck & Concession | **RWP-00.42** | Industry definition, subtypes, terminology, and operating characteristics are complete in this proposed merge state. Mobility, temporary locations, event and service calendars, setup/teardown, compact menus, sell-outs, queue surges, weather/location changes, host authority, multi-unit scope, and intermittent-connectivity boundaries are documented. | **RWP-00.43 — Required Capabilities (#518)** |
| Hospitality | **RWP-00.52** | Industry definition and nine bounded property subtypes plus neutral fallback are documented. | **RWP-00.53 — Business Terminology (#528)** |
| Entertainment & Attractions | **RWP-00.65** | Industry definition, venue subtypes, hybrid rules, and business terminology are documented. | **RWP-00.66 — Operating Characteristics (#541)** |

Only merged documents are authoritative. An industry advances only after its current RWP is merged, verified, closed, and released.

## Food Truck & Concession Operating Result

RWP-00.42 establishes a bounded operating lifecycle from planned and setup through ready, open, limited, paused, relocating, closed, canceled, teardown, and serving again. It keeps current location, stop, pitch, event, host, service period, service window, availability, queue/pickup context, operating state, and source freshness as product/domain state.

Manual menu and availability updates, current-location and event communication, queue/pickup guidance, explicit targeting, publishing, delivery confirmation, offline/outdated awareness, and restoration remain core. Advanced route/event scheduling, cross-unit coordination, approval workflows, live queue/order information, and managed monitoring remain tier candidates. POS, order, payment, inventory, production, route, event, host, weather, traffic, queue, pickup, and external-calendar synchronization remain add-on candidates where integrations are required. Counts remain limits.

Subtype affects defaults and presentation only. Host relationship, authority, permission, entitlement, state, add-on, limit, and rollout remain separate.

## Track 0 Classification Policy

Every concern has exactly one primary classification: core capability, permission, product/domain state, tier entitlement, independent add-on, usage or quantity limit, or internal rollout flag.

Industry and subtype affect defaults, terminology, starter content, recommendations, and capability presentation. They are not entitlements. Essential daily operation remains core. Permissions do not determine commercial access. Product state is not a feature flag. Limits are not capabilities.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Integration and external-system tests remain skipped under the standing owner instruction.

## Next Action

After RWP-00.42 is merged, verified on `master`, issue #517 is closed, and the claim is released, execute **RWP-00.43 — Food Truck & Concession Required Capabilities** (#518).

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, migrations, routing, ordering, payments, inventory, event management, host-venue management, analytics, localization, integrations, or later-phase work until the owner approves the completed Track 0 model and implementation packages.
