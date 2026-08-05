# Vennusign Session Handoff

## Current State

- Item: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Product implementation: paused
- Phase 14 and later: paused
- RWP-13.06: paused pending the owner-approved Track 0 model
- Restaurant: canonical approved baseline
- Bar, Brewery & Nightlife: RWP-00.18 merged; RWP-00.19 is next
- Café, Bakery & Dessert: the current Café claim remains active; continue sequentially after merge/release verification
- Food Truck & Concession: complete through RWP-00.50; await the all-industry consolidation gate
- Hospitality: complete through RWP-00.62; await the all-industry consolidation gate
- Entertainment & Attractions: complete through RWP-00.74; await the all-industry consolidation gate

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

## Entertainment & Attractions Final Result

RWP-00.74 validates RWP-00.63 through RWP-00.73 as one coherent Entertainment & Attractions Track 0 profile.

Restaurant remains the canonical inherited baseline. Entertainment adds bounded venue, area, experience, attraction, exhibit, event, session, schedule, occurrence, queue, capacity, admission, route, notice, source, screen, delivery, and recovery context without turning subtype, operating state, or external integration into entitlement.

Essential manual visitor communication remains core:

- venue, attraction, exhibit, program, experience, event, session, and schedule information;
- available, limited, full, sold out, delayed, paused, closed, canceled, relocated, restricted, weather-affected, reopening, and unknown states;
- manual queue, wait, capacity, admission, boarding, seating, check-in, last-entry, wayfinding, route, notice, language, and accessibility guidance;
- explicit targeting and preview;
- publish and per-target confirmed delivery;
- offline, outdated, stale-source, conflict, and override awareness;
- correction, supersession, unpublish, retry, undo, and restoration;
- required accessibility, responsive, empty, loading, permission, failure, partial-delivery, success, and recovery states.

Industry and subtype remain non-commercial product configuration. Permissions determine authority. Represented values remain product state. Advanced native coordination, workflow, localization, mapping, analytics, portfolio governance, and enterprise administration remain tier candidates. Ticketing, admissions, access, measured queue/occupancy/footfall, maps, venue/cinema/show-control/collection/attraction/event/sports systems, CRM, POS, translation, AI, identity, hardware, connectivity, monitoring, and managed services remain independent add-on candidates. Counts and consumption remain limits. Rollout controls remain internal.

Entertainment onboarding reaches one confirmed active screen before forced pricing or integrations. The dashboard is exception-first and task-first. Analytics separates publication, delivery, visitor measurement, attendance, conversion, and revenue and requires source, authority, freshness, coverage, uncertainty, privacy, permission, retention, export, and reconciliation.

## Entertainment Owner Decisions Still Open

- final tier names and exact capability placement;
- pricing, trials, contracts, limits, pooling, and overage;
- add-on prerequisites, providers, regions, rights, and administration requirements;
- multi-venue inheritance and local-override policy;
- downgrade grace, read-only, conversion, export, retention, deletion, and active-screen protection;
- privacy, data, camera, biometric, child, accessibility, legal, safety, gambling, alcohol, licensing, and rights obligations;
- player, pairing, full-screen, online-state, theme/content refresh, hardware, connectivity, monitoring, installation, and support behavior;
- external metric definitions, thresholds, alerts, prediction, data agreements, and BI/export;
- implementation packages and sequencing after Track 0 approval.

## Exact Next Actions

- Continue **RWP-00.19 — Bar, Brewery & Nightlife Required Capabilities** (#494).
- Continue the active Café queue strictly sequentially.
- Keep Food Truck & Concession closed through **RWP-00.50**.
- Keep Hospitality closed through **RWP-00.62**.
- Keep Entertainment & Attractions closed through **RWP-00.74**.
- Begin **RWP-00.75 — Cross-Industry Capability Inventory** only after RWP-00.26, RWP-00.38, RWP-00.50, RWP-00.62, and RWP-00.74 are all merged and complete.

## Parallel-Stream Rule

Each industry remains sequential inside its approved range. Shared living records follow `docs/process/SHARED_FILE_WRITE_PROTOCOL.md` using short transactional writes, semantic reconciliation, retry on concurrent update, and immediate release.

## Boundaries

- Do not start product implementation or consolidation before the all-industry gate.
- Do not resume RWP-13.06 or Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, limits, rollout controls, privacy systems, localization, analytics pipelines, ticketing, admissions, property-management, event, room-booking, transport, point-of-sale, guest-service, access, gaming, emergency, mapping, AI, identity, player, hardware, managed services, or integrations during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
