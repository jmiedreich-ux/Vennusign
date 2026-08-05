# Vennusign Session Handoff

## Current State

- Item: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Product implementation: paused
- Phase 14 and later: paused
- RWP-13.06: paused pending the owner-approved Track 0 model
- Restaurant: canonical approved baseline
- Bar, Brewery & Nightlife: RWP-00.18 merged; RWP-00.19 is next
- Café, Bakery & Dessert: complete through RWP-00.38; await the all-industry consolidation gate
- Food Truck & Concession: complete through RWP-00.50; await the all-industry consolidation gate
- Hospitality: complete through RWP-00.62; await the all-industry consolidation gate
- Entertainment & Attractions: RWP-00.72 is the latest documented dashboard item; continue from the first unfinished approved item

## Café Final Result

RWP-00.38 validates RWP-00.27 through RWP-00.37 as one coherent Café, Bakery & Dessert Track 0 profile.

The profile preserves Restaurant inheritance while defining bounded Café subtype, terminology, early/cross-midnight, batch, freshness-guidance, rotating-product, sell-out, expected-return, preorder, pickup, service-context, onboarding, dashboard, and KPI/analytics differences.

Essential manual operation remains core:

- venue, hours, service-period, menu, product, category, price, size, option, image, and public-information management;
- rapid availability, sold-out, limited, next-batch, available-again, pickup, preorder, closure, reopening, and correction updates;
- customer-authored or authoritative freshness and expected-return guidance without invented facts;
- screen pairing, purpose, explicit targeting, preview, immediate publication, and per-target confirmation;
- source/freshness/conflict awareness and safe manual fallback;
- offline/outdated awareness, retry, supersession, undo, and restoration;
- permissions, complete operating states, accessibility, and mobile/desktop operation.

Industry and subtype remain non-commercial product configuration. Product/domain state, permission, tier entitlement, independent add-on, limit, privacy/source relationships, and rollout controls remain separate. Advanced scheduling, campaigns, presentation, approvals, localization, governance, analytics, and portfolio workflow remain tier candidates. POS, inventory, production, ordering, payment, fulfillment, loyalty, messaging, translation, AI, managed hardware, connectivity, monitoring, support, and other external or managed services remain independent add-on candidates. Counts and consumption remain limits.

Café onboarding reaches one real, confirmed first-screen update before prominent pricing or optional-capability prompts. The default dashboard is exception-first and task-first. Analytics keeps current screen, publication, source, freshness, conflict, and recovery evidence core and does not infer sales, demand, inventory, conversion, readiness, customer behavior, queue, attendance, or attribution without authoritative data.

The shared capability matrix already contains the meaningful normalized Café deltas. RWP-00.33 remains the authoritative Café-specific classification expansion; duplicate matrix rows were not added.

## Café Owner Decisions Still Open

These are consolidation-level decisions and do not block the completed industry profile:

- final tier names, prices, and exact capability placement;
- allowances, counting, pooling, overage, and limit behavior;
- add-on grouping and direct, partner, or marketplace delivery;
- trials, downgrade grace, read-only, archive, export, retention, and deletion policy;
- organization-versus-venue purchase scope;
- source, privacy, authorization, support, and safe-exit policy;
- analytics and AI/translation packaging; and
- implementation packages and sequencing after Track 0 approval.

## Hospitality Final Result

RWP-00.62 validates RWP-00.51 through RWP-00.61 as one coherent Hospitality Track 0 profile.

The profile preserves Restaurant inheritance for embedded food-and-beverage venues while defining Hospitality property, subtype, terminology, operating rhythm, required core, optional candidates, primary classifications, proposed tier outcomes, onboarding, default dashboard, and KPI/analytics boundaries.

Essential manual property and guest communication remains core:

- public property information, hours, operating states, notices, amenities, services, outlets, meetings, events, directories, and wayfinding;
- basic customer-authored language variants;
- explicit targeting and preview;
- save, schedule, publish, and confirmed delivery;
- offline, outdated, stale-source, conflict, and override awareness;
- correction, expiry, supersession, retry, undo, and restoration;
- current exceptions and shift-handoff visibility;
- required accessibility, responsive, empty, loading, permission, failure, partial-delivery, success, and recovery states.

Industry and subtype remain non-commercial product configuration. Permissions determine authority. Represented business/system values remain product state. Advanced Vennusign workflow, coordination, governance, localization, analytics, and enterprise administration remain tier candidates. External synchronization, identity, AI, maps, managed hardware, connectivity, monitoring, and related services remain independent add-on candidates. Counts and consumption remain limits. Rollout controls remain internal.

Hospitality onboarding reaches one confirmed active screen before contextual pricing or add-on prompts. The starter menu is task and template navigation, not a tier. The dashboard is exception-first and task-first. Analytics requires source, authority, freshness, coverage, formula, exclusions, privacy, permission, retention, correction, export, and reconciliation and does not infer unsupported guest behavior, occupancy, demand, revenue, attendance, satisfaction, or causation.

## Hospitality Owner Decisions Still Open

- final tier names and exact capability placement;
- pricing, trials, contracts, grandfathering, limits, pooling, and overage;
- add-on prerequisites and administration requirements;
- property-group inheritance and local-override policy;
- downgrade grace, read-only, conversion, export, retention, deletion, and active-screen protection;
- guest personalization purpose and privacy model;
- enterprise identity and managed-service structure;
- external metric definitions and data agreements;
- emergency, safety, legal, security, and compliance obligations;
- implementation packages and sequencing after Track 0 approval.

## Exact Next Actions

- Continue **RWP-00.19 — Bar, Brewery & Nightlife Required Capabilities** (#494).
- Keep Café, Bakery & Dessert closed through **RWP-00.38**.
- Continue Entertainment & Attractions from its first unfinished approved RWP.
- Keep Food Truck & Concession closed through **RWP-00.50**.
- Keep Hospitality closed through **RWP-00.62**.
- Begin RWP-00.75 only after RWP-00.26, RWP-00.38, RWP-00.50, RWP-00.62, and RWP-00.74 are all merged and complete.

Issue #513’s original handoff to begin Food Truck RWP-00.39 is obsolete because Food Truck is already complete through RWP-00.50. Do not recreate completed Food Truck work.

## Parallel-Stream Rule

Each industry remains sequential inside its approved range. Shared living records follow `docs/process/SHARED_FILE_WRITE_PROTOCOL.md` using short transactional writes, semantic reconciliation, retry on concurrent update, and immediate release.

## Boundaries

- Do not start product implementation or consolidation from a completed industry stream.
- Do not resume RWP-13.06 or Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, limits, rollout controls, privacy systems, localization, analytics pipelines, point-of-sale, inventory, production, ordering, payment, fulfillment, loyalty, property-management, event, room-booking, transport, guest-service, access, gaming, emergency, mapping, AI, identity, hardware, managed services, or integrations during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.