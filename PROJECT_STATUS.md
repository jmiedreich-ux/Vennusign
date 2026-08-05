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
| Café, Bakery & Dessert | **RWP-00.29** | Industry definition, nine venue subtypes plus neutral fallback, hybrid rules, canonical product, size, option, batch, freshness, availability, preorder, pickup, and service-period terminology, subtype preferences, neutral fallbacks, operator/guest language, analytics labels, and Impeccable clarification guidance are documented. | **RWP-00.30 — Operating Characteristics (#505)** |
| Food Truck & Concession | **RWP-00.41** | Industry definition, nine venue subtypes plus neutral fallback, physical-form and operating-context traits, host boundaries, canonical operation, unit, service-point, location, stop, pitch, event, service-window, menu, combo, availability, pickup, queue, last-order, relocation, and operating-state terminology, subtype preferences, neutral fallbacks, operator/guest language, analytics labels, and Impeccable clarification guidance are documented. | **RWP-00.42 — Operating Characteristics (#517)** |
| Hospitality | **RWP-00.51** | Industry definition is documented. | **RWP-00.52 — Venue Subtypes (#527)** |
| Entertainment & Attractions | **RWP-00.63** | Industry definition is documented. | **RWP-00.64 — Venue Subtypes (#539)** |

Only merged documents are authoritative. An industry may advance only after its current RWP is merged, verified, closed, and released.

## Food Truck & Concession Terminology Result

RWP-00.41 establishes a stable language model for onboarding, navigation, content editing, Quick Update, location and event communication, help text, analytics, starter content, and guest-facing displays.

### Neutral cross-industry terms

Organization, venue, operation, content, item, category, availability, location, event, service point, service period, screen, publish, and restore remain the preferred neutral terms when a surface spans industries, venue subtypes, hosts, or physical forms.

### Context-specific Food Truck & Concession terms

Operation-scoped surfaces may use truck, trailer, cart, kiosk, concession stand, stand, stall, pop-up, station, current location, stop, next stop, pitch, host location, event, service window, compact menu, event menu, combo, special, pickup, collection, queue, lane, last orders, service paused, canceled, relocating, or serving again when the context makes the meaning clear.

### Important distinctions

- **Operation** is the neutral Food Truck & Concession local context; **venue** remains the cross-industry local business unit.
- **Unit** is a neutral physical or operational instance; it does not decide entitlement or venue counting.
- **Service point** is where guest service occurs; stand, stall, kiosk, station, counter, or window is used only when accurate.
- **Service window** is a physical opening; **service period** is a bounded time interval.
- **Current location**, **stop**, **pitch**, **host location**, and **event** are distinct location and context concepts.
- **Combo** is a named item grouping; it does not imply POS, inventory, discount, or ordering behavior.
- **Pickup** is the neutral guest collection term; collection may be used consistently where established.
- **Queue** is the neutral waiting-line concept; **lane** is a distinct order, express, pickup, or collection path.
- **Available**, **unavailable**, **sold out**, **limited**, **open**, **service paused**, **closed**, **canceled**, and **relocating** are distinct product or operating states.
- **Last orders** communicates an authoritative order cutoff; **service ends at** communicates the end of service.

Unknown location, destination, timing, quantity, queue, pickup, and reopening information remains unknown. Guest copy must not promise a destination, arrival time, remaining quantity, queue length, wait time, pickup readiness, or reopening time without authoritative data.

### Classification

- Industry, subtype, hybrid traits, and terminology preference are **product/domain state**.
- Terminology changes defaults, labels, help text, starter recommendations, analytics presentation, and guest wording only.
- Terminology does not grant capabilities, alter permissions, transfer host or operator authority, increase limits, control rollout, or change commercial access.
- Operation, unit, service-point, location, stop, pitch, host-location, event, service-window, service-period, operating-state, availability, queue-context, pickup-context, combo, and last-order values retain product/domain-state treatment where represented.
- Customer-authored names and custom labels must be preserved through future profile, subtype, host, event, or location changes.
- Manual menu and availability editing, location and event communication, closure and relocation communication, publishing, delivery confirmation, offline awareness, and restoration remain inherited core capabilities.
- Routing, ordering, payments, inventory, queue measurement, event management, host-venue, catering, pickup-source, and related synchronization remain later capability and integration-packaging decisions.

### Impeccable planning

The project-local Impeccable `clarify` guidance applies to future UI copy. One noun and verb must represent the same concept across a flow; actions must name the affected item, combo, service point, stop, location, event, or service period; labels must persist; first-use, empty, filtered, permission, error, sold-out, unavailable, paused, closed, canceled, relocating, unknown-timing, success, and recovery states must remain distinct; visible and accessible names must align; copy must support localization expansion, long names, pluralization, keyboard access, assistive technology, and 200% zoom; and the Sky Blue administrative direction remains intact.

No UI, API, schema, migration, billing, entitlement, feature-gate, routing, ordering, payment, inventory, event-management, host-venue, catering, pickup, analytics, localization, or integration implementation was performed.

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

After RWP-00.41 is merged, verified on `master`, issue #516 is closed, and the claim is released, continue the Food Truck & Concession queue with **RWP-00.42 — Operating Characteristics** (#517).

RWP-00.42 must document operating-day and service-period behavior, setup and teardown, routes and stops, event and host schedules, relocation, weather and cancellation, rapid sell-outs, queue surges, last orders, pickup patterns, intermittent connectivity, multi-window or multi-stand operation, and subtype-specific operating differences. It must tie each difference to defaults, terminology, content, screen purposes, or capability classification; remain documentation-only; and avoid jurisdiction-specific invention.

Other owner-approved native-industry schedules may continue independently inside their own sequential queues. They must use Restaurant as the canonical baseline, treat only merged work as authoritative, and avoid concurrent edits to shared controlled files.

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, migrations, routing, ordering, payments, inventory, event management, host-venue behavior, catering, pickup automation, or later-phase work until the owner approves the completed capability matrix and implementation packages.
