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
| Food Truck & Concession | **RWP-00.40** | Industry definition and nine venue subtypes plus neutral fallback, physical-form and operating-context traits, and host boundaries are documented. | **RWP-00.41 — Business Terminology (#516)** |
| Hospitality | **RWP-00.51** | Industry definition is documented. | **RWP-00.52 — Venue Subtypes (#527)** |
| Entertainment & Attractions | **RWP-00.63** | Industry definition is documented. | **RWP-00.64 — Venue Subtypes (#539)** |

Only merged documents are authoritative. An industry may advance only after its current RWP is merged, verified, closed, and released.

## Café, Bakery & Dessert Terminology Result

RWP-00.29 establishes a stable language model for onboarding, navigation, content editing, Quick Update, help text, analytics, starter content, and guest-facing displays.

### Neutral cross-industry terms

Organization, venue, content, item, category, option, availability, service period, screen, publish, and restore remain the preferred neutral terms when a surface spans industries or different venue subtypes.

### Context-specific Café terms

Venue-scoped surfaces may use drink menu, coffee menu, tea menu, bakery case, today's selection, dessert menu, current flavors, juice and smoothie menu, size, milk choice, base, temperature, flavor, add-in, topping, batch, next batch, freshness guidance, sold out, limited, preorder, custom order, pickup, seasonal item, and guest-recognizable service-period names when the context makes the meaning clear.

### Important distinctions

- **Item** is the neutral operator object; guest copy uses the product name or a known product noun.
- **Option** is the neutral guest choice; modifier remains operator or integration language.
- **Batch** is a produced group; next-batch timing appears only when authoritative.
- **Freshness guidance** must be venue-authored or source-authoritative and must not be inferred.
- **Available**, **unavailable**, **sold out**, and **limited** are distinct product states or presentation values.
- **Preorder** and **custom order** describe guest-request models but do not authorize ordering, payment, production, or fulfillment implementation.
- **Pickup** is the neutral guest collection term; collection is used only where established and not as a competing term in the same flow.
- **Service period** is operator language; guest copy uses recognizable names such as morning, breakfast, lunch, afternoon, evening, or pickup hours.

### Classification

- Industry, subtype, hybrid traits, and terminology preference are **product/domain state**.
- Terminology changes defaults, labels, help text, starter recommendations, analytics presentation, and guest wording only.
- Terminology does not grant capabilities, alter permissions, increase limits, control rollout, or change commercial access.
- Batch, freshness, limited-quantity, expected-return, availability, preorder-window, pickup-context, service-period, size, and option values retain product/domain-state treatment where represented.
- Customer-authored names and custom labels must be preserved through future profile or subtype changes.
- Manual item editing, manual availability changes, publishing, delivery confirmation, offline awareness, and restoration remain inherited core capabilities.
- Ordering, payment, production management, fulfillment, inventory, POS, pickup-source, and related synchronization remain later capability and integration-packaging decisions.

### Impeccable planning

The project-local Impeccable `clarify` guidance applies to future UI copy. One noun and verb must represent the same concept across a flow; actions must describe outcomes; labels must persist; first-use, empty, filtered, permission, error, availability, timing, success, and recovery states must remain distinct; visible and accessible names must align; copy must support localization expansion, long names, pluralization, keyboard access, assistive technology, and 200% zoom; and the Sky Blue administrative direction remains intact.

No UI, API, schema, migration, billing, entitlement, feature-gate, ordering, payment, production, fulfillment, inventory, analytics, localization, or integration implementation was performed.

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

After RWP-00.29 is merged, verified on `master`, issue #504 is closed, and the claim is released, continue the Café, Bakery & Dessert queue with **RWP-00.30 — Operating Characteristics** (#505).

RWP-00.30 must document early hours, business-day and service-period behavior, batch production, freshness windows, rotating daily products, sell-outs, preorders, pickup, seasonal demand, table and counter service, and subtype-specific operating differences. It must tie each difference to defaults, terminology, content, screen purposes, or capability classification; remain documentation-only; and avoid jurisdiction-specific invention.

Other owner-approved native-industry schedules may continue independently inside their own sequential queues. They must use Restaurant as the canonical baseline, treat only merged work as authoritative, and avoid concurrent edits to shared controlled files.

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, migrations, ordering, payments, production, fulfillment, inventory, or later-phase work until the owner approves the completed capability matrix and implementation packages.
