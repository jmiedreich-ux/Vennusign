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
| Café, Bakery & Dessert | **RWP-00.30** | Industry definition, venue subtypes, hybrid rules, business terminology, and operating characteristics are documented. | **RWP-00.31 — Required Capabilities (#506)** |
| Food Truck & Concession | **RWP-00.50** | The complete industry profile is validated and ready for cross-industry consolidation. | **Complete — await the RWP-00.75 consolidation gate** |
| Hospitality | **RWP-00.56** | Industry, subtypes, terminology, operating characteristics, required core, and optional capability candidates are complete in this proposed merge state. | **RWP-00.57 — Capability Classification (#532)** |
| Entertainment & Attractions | **RWP-00.72** | Industry model, packaging, onboarding, and exception-first default dashboard are complete in this proposed merge state. | **RWP-00.73 — KPIs & Analytics (#548)** |

Only merged documents are authoritative. An industry advances only after its current RWP is merged, verified, closed, and released.

## Café Operating-Characteristics Result

RWP-00.30 defines early and cross-midnight business days; independently active service, preorder, pickup, counter, table, and mixed-service contexts; batch-led availability; source-authoritative freshness guidance; rotating and seasonal products; rapid sell-out and return transitions; public preorder and pickup information; screen-purpose and source-conflict behavior; multi-venue safeguards; and subtype-specific operating rhythms.

Essential manual product and content management, rapid availability updates, explicit targeting, preview, immediate publishing, per-target confirmation, correction, supersession, undo, offline/outdated awareness, conflict handling, and restoration remain core. Represented operating facts are product/domain state. Authority is permission. Advanced workflow is a tier candidate. External systems and managed services are add-on candidates. Counts and retention are limits; temporary delivery controls are rollout flags.

## Bar Operating-Characteristics Result

RWP-00.18 defines cross-midnight venue and service periods; separate kitchen, bar, doors, event, last-entry, and locally authored last-call timing; high-frequency tap, item, release, and temporary-offer changes; table, bar, counter, and hybrid service models; bounded age/access and responsible-content presentation; entertainment and sports operations; reservations, guest lists, cover, tickets, and private-event distinctions; and subtype-specific operating rhythms.

Essential manual availability, hours, specials, events, public guidance, explicit targeting, preview, publishing, delivery confirmation, correction, offline/outdated awareness, supersession, and restoration remain core. Operating values are product/domain state. Authority is permission. Advanced workflow is a tier candidate. External synchronization is an add-on candidate. Quantities are limits and temporary delivery controls are rollout flags.

## Hospitality Optional-Capability Result

RWP-00.56 separates optional Hospitality capabilities into advanced native workflow and governance tier candidates, independent external or managed-service add-ons, represented product state, permissions, limits, and rollout controls. Required manual core remains unchanged, and every optional candidate requires manual fallback, source/freshness, privacy, safe failure, cancellation, downgrade, retention, delivery confidence, and recovery.

## Entertainment Dashboard Result

RWP-00.72 defines an exception-first task dashboard. The persistent context shows organization, venue, area or experience, local operating time, current state, and scoped authority. Urgent public-impact and recovery exceptions appear before analytics or promotion.

The planned hierarchy covers quick operational actions; now/today/next; schedule health; queue, wait, capacity, and admission; wayfinding; notices; per-target screen and publication health; source/freshness; upcoming work; and multi-venue oversight. It includes role-aware mobile and desktop presentation, explicit mixed states, safe bulk actions, full empty/permission/tier/add-on/integration/limit/privacy/failure/recovery states, and manual fallback. Healthy aggregate state cannot hide one failed, outdated, excluded, or unknown target.

## Food Truck & Concession Validation Result

RWP-00.50 validates RWP-00.39 through RWP-00.49 as one coherent profile. Restaurant inheritance is preserved; the bounded subtype and terminology models cover mobile, temporary, event, host-venue, compact-service, rapid-availability, service-window, pickup/queue, and intermittent-connectivity deltas without turning subtype into entitlement.

Essential manual menu, availability, current-context, service-state, targeting, preview, publishing, per-target delivery confidence, correction, retry, and restoration remain core. Product state, permission, tier entitlement, independent add-on, limit, source/privacy, and rollout remain separate. Onboarding reaches useful value before pricing or add-on prompts; the default dashboard is exception-first and mobile-first; analytics distinguishes operational evidence from inference and requires source, freshness, coverage, formula, privacy, retention, correction, and export disclosure.

The remaining decisions are owner-level consolidation and implementation choices rather than industry-profile gaps. The Food Truck stream has no further open RWP and must wait for the all-industry gate before RWP-00.75.

## Track 0 Classification Policy

Every concern has exactly one primary classification: core capability, permission, product/domain state, tier entitlement, independent add-on, usage or quantity limit, or internal rollout flag.

Industry and subtype affect defaults, terminology, starter content, recommendations, and capability presentation. They are not entitlements. Essential daily operation remains core. Permissions do not determine commercial access. Product state is not a feature flag. Limits are not capabilities.

## Validation Policy

Documentation-only Track 0 changes use lightweight repository validation. GitHub Actions is authoritative on the exact reviewed PR head. Integration and external-system tests remain skipped under the standing owner instruction.

## Next Actions

- Execute **RWP-00.19 — Bar, Brewery & Nightlife Required Capabilities** (#494).
- Execute **RWP-00.31 — Café, Bakery & Dessert Required Capabilities** (#506).
- After RWP-00.56 is merged, verified, closed, and released, execute **RWP-00.57 — Hospitality Capability Classification** (#532).
- After RWP-00.72 is merged, verified, closed, and released, execute **RWP-00.73 — Entertainment & Attractions KPIs & Analytics** (#548).
- Keep Food Truck & Concession closed through RWP-00.50. Do not begin consolidation until RWP-00.26, RWP-00.38, RWP-00.50, RWP-00.62, and RWP-00.74 are all complete.

Other owner-approved industry streams may proceed under the queued short-lived shared-file write protocol.

Do not implement onboarding, billing, entitlements, feature gates, UI, API, schema, migrations, privacy systems, localization, analytics, ticketing, admissions, property-management, event, room-booking, transport, guest-service, access, gaming, mapping, AI, managed hardware, integrations, or later-phase work until the owner approves the completed Track 0 model and implementation packages.
