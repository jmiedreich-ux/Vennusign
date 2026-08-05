# Vennusign Project Status

## Current State

- Phase 13 — Customer Identity, Signup, and Onboarding: complete.
- Phase 14 and later: paused pending explicit owner approval.
- Active implementation WP/RWP: none.
- Active planning track: Track 0 — Capability, Packaging, and Entitlement Architecture (#488). Product implementation remains paused.
- The owner approved independent native-industry Track 0 schedules. Each industry remains sequential inside its own RWP range and must avoid shared-file conflicts.
- Restaurant is the canonical approved baseline inherited by later native-industry profiles.
- RWP-13.06 — Trial-First Onboarding (#466) remains paused until Track 0 produces an owner-approved capability and packaging model.

## Native-Industry Track 0 Progress

| Industry | Completed through | Result | Next approved item |
| --- | --- | --- | --- |
| Bar, Brewery & Nightlife | **RWP-00.17** | Industry definition, nine venue subtypes plus neutral fallback, hybrid rules, canonical business terminology, subtype terminology, neutral fallbacks, operator/guest language, analytics labels, and Impeccable clarification guidance are documented. | **RWP-00.18 — Operating Characteristics (#493)** |
| Café, Bakery & Dessert | **RWP-00.28** | Industry definition and nine venue subtypes plus neutral fallback are documented. | **RWP-00.29 — Business Terminology (#504)** |
| Food Truck & Concession | **RWP-00.40** | Industry definition and nine venue subtypes plus neutral fallback, physical-form and operating-context traits, and host boundaries are documented. | **RWP-00.41 — Business Terminology (#516)** |
| Hospitality | **RWP-00.51** | Industry definition is documented. | **RWP-00.52 — Venue Subtypes (#527)** |
| Entertainment & Attractions | **RWP-00.63** | Industry definition is documented. | **RWP-00.64 — Venue Subtypes (#539)** |

Only merged documents are authoritative. An industry may advance only after its current RWP is merged, verified, closed, and released.

## Bar, Brewery & Nightlife Terminology Result

RWP-00.17 establishes a stable language model for onboarding, navigation, content editing, help text, analytics, starter content, and guest-facing displays.

### Neutral cross-industry terms

Organization, venue, content, item, category, screen, area, event, service period, special, availability, publish, and restore remain the preferred neutral terms when a surface spans industries or different venue subtypes.

### Context-specific Bar terms

Venue-scoped surfaces may use drink menu, tap list, cocktail list, wine list, current taps, pour size, flight, bottle, can, release, happy hour, game or match, viewing area, doors, entry information, cover, guest list, reservation, table, room, patio, venue zone, last call, and sold out when the context makes the meaning clear.

### Classification

- Industry, subtype, hybrid traits, and terminology preference are **product/domain state**.
- Terminology changes defaults, labels, help text, starter recommendations, analytics presentation, and guest wording only.
- Terminology does not grant capabilities, alter permissions, increase limits, control rollout, or change commercial access.
- Customer-authored names and custom labels must be preserved through future subtype or profile changes.
- Available, unavailable, sold out, event, reservation, entry, service-period, and area values retain their own product-state classifications.
- Manual editing, availability changes, publishing, delivery confirmation, and restoration remain inherited core capabilities.
- Automatic POS, inventory, tap-management, reservation, ticketing, event, or related synchronization remains a later integration-packaging decision.

### Impeccable planning

The project-local Impeccable `clarify` guidance applies to future UI copy. One noun and verb must represent the same concept across a flow; actions must describe outcomes; labels must persist; state, permission, error, empty, success, and recovery messages must remain distinct; visible and accessible names must align; copy must support localization expansion, long names, pluralization, keyboard access, assistive technology, and 200% zoom; and the Sky Blue administrative direction remains intact.

No UI, API, schema, migration, billing, entitlement, feature-gate, analytics, localization, or integration implementation was performed.

## Track 0 Classification Policy

Every concern has exactly one primary classification:

1. Core capability
2. Permission
3. Product/domain state
4. Tier entitlement
5. Independent add-on
6. Usage or quantity limit
7. Internal rollout flag

Industry and subtype affect defaults, terminology, starter content, recommendations, and capability presentation. They are not entitlements. Essential daily operation remains core. Permissions do not determine commercial access. Product state is not a feature flag. Limits are not capabilities.

## Completed Delivery

Phases 02 through 13 are complete. The retrospective remediation queues through RWP-11.04 are complete. RWP-00.14 installed the project-local Impeccable Codex design skill. Earlier package details, validation evidence, and status snapshots remain under `docs/archive/` for deliberate research.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Integration and external-system tests requiring Azure SQL, external services, credentials, hosted infrastructure, containers, devices, signing/store access, or cross-system integration remain skipped under the standing owner instruction.

## Next Action

After RWP-00.17 is merged, verified on `master`, issue #492 is closed, and the claim is released, continue the Bar, Brewery & Nightlife queue with **RWP-00.18 — Operating Characteristics** (#493).

RWP-00.18 must define late-night hours and business-day behavior, service periods, happy hour, rotating taps, limited releases, last call, bar/table/counter/hybrid service, age and responsible-display considerations, entertainment and event operations, reservations, guest lists, cover and ticketing considerations, inventory volatility, and subtype-specific operating differences. It remains documentation-only.

Other owner-approved native-industry schedules may continue independently inside their own sequential queues. They must use Restaurant as the canonical baseline, treat only merged work as authoritative, and avoid concurrent edits to shared controlled files.

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, migrations, or later-phase work until the owner approves the completed capability matrix and implementation packages.
