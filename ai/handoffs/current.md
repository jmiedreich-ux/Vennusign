# Vennusign Session Handoff

## Current State

- Item: Track 0 — Capability, Packaging, and Entitlement Architecture (#488)
- Product implementation: paused
- Phase 14 and later: paused
- RWP-13.06: paused pending the owner-approved Track 0 model
- Restaurant: canonical approved baseline
- Bar, Brewery & Nightlife: RWP-00.18 merged; RWP-00.19 is next
- Café, Bakery & Dessert: RWP-00.30 merged; RWP-00.31 is next
- Food Truck & Concession: complete through RWP-00.50; no further industry RWP is open
- Hospitality: RWP-00.56 complete in this proposed merge state; RWP-00.57 is next
- Entertainment & Attractions: RWP-00.72 complete in this proposed merge state; RWP-00.73 is next

## Café Operating-Characteristics Result

RWP-00.30 defines early and cross-midnight business days; independently active service, preorder, pickup, counter, table, and mixed-service contexts; batch-led availability; source-authoritative freshness guidance; rotating and seasonal products; rapid sell-out and return transitions; public preorder and pickup information; screen-purpose and source-conflict behavior; multi-venue safeguards; and subtype-specific operating rhythms.

Essential manual product and content management, rapid availability updates, explicit targeting, preview, immediate publishing, per-target confirmation, correction, supersession, undo, offline/outdated awareness, conflict handling, and restoration remain core. Represented operating facts are product/domain state. Authority is permission. Advanced workflow is a tier candidate. External systems and managed services are add-on candidates. Counts and retention are limits; temporary delivery controls are rollout flags.

## Bar Operating-Characteristics Result

RWP-00.18 defines cross-midnight operating and service periods; separate kitchen, bar, doors, event, last-entry, and locally authored last-call timing; high-frequency tap, item, release, and temporary-offer changes; table, bar, counter, and hybrid service models; bounded responsible-content and age/access presentation; entertainment and sports operations; and distinct reservation, guest-list, cover, ticket, and private-event state.

Essential manual availability, hours, specials, events, public guidance, targeting, preview, publishing, delivery confirmation, correction, offline/outdated awareness, supersession, and restoration remain core. Operating values are product/domain state. Authority is permission. Advanced workflow is a tier candidate. External synchronization is an add-on candidate. Quantities are limits and temporary delivery controls are rollout flags.

## Hospitality Optional-Capability Result

RWP-00.56 preserves the required manual core and defines optional candidates for automation, scale, coordination, personalization, governance, insight, enterprise administration, managed hardware, connectivity, monitoring, and support.

Tier candidates include advanced wayfinding, brand libraries, property-group coordination, campaigns, approvals and advanced workflows, localization workflow, advanced analytics, enterprise administration, and selected advanced operational workflow. Independent add-on candidates include property-management, event, room-booking, transport, parking, access, guest-service, gaming, maps, positioning, weather, emergency, translation, AI, identity providers, managed hardware, connectivity, monitoring, and related external systems or services.

## Entertainment Dashboard Result

RWP-00.72 defines an exception-first, task-first dashboard. Operators see selected organization/venue/area/experience, local operating time, current state, public-impact exceptions, affected targets, source/freshness, safest next action, and recovery before analytics or promotion.

The dashboard hierarchy covers quick updates; now/today/next; schedule health; queues, waits, capacity, admission, boarding, seating, and check-in; wayfinding; notices; screen and publication health; source conflicts; upcoming work; and multi-venue oversight. Healthy aggregate state cannot hide failed, outdated, excluded, or unknown targets.

Role-aware presentation supports front-line operators, editors, publishers, venue administrators, portfolio/enterprise administrators, and limited collaborators. Mobile prioritizes context, the highest-impact exception, Quick Update, retry/restore, and compact now/next/health. Desktop may show more panels without becoming a dense control center. Manual core remains visible, optional prompts remain contextual, and state, permission, tier, add-on, limit, source, privacy, and rollout remain separate.

## Food Truck & Concession Final Result

RWP-00.50 validates the full RWP-00.39–00.49 package. The profile preserves Restaurant inheritance while defining the mobile, temporary, host-venue, event, compact-service, rapid-availability, service-window, pickup/queue, relocation, cancellation, intermittent-connectivity, targeting, delivery, correction, and recovery differences required by Food Truck & Concession operations.

The required manual baseline remains core and usable without integrations or premium access. Product state, permission, tier, add-on, limit, privacy/source authority, and rollout are separate. Onboarding reaches a useful first published screen before optional commercial prompts. The dashboard is role-aware, mobile-first, and exception-first. KPI/analytics planning requires evidence, source, freshness, coverage, formula, privacy, retention, correction, and export disclosure and does not infer unsupported sales, inventory, demand, attendance, wait, or attribution.

No further Food Truck RWP is approved. Remaining questions are owner-level cross-industry packaging and later implementation decisions.

## Exact Next Actions

- Execute **RWP-00.19 — Bar, Brewery & Nightlife Required Capabilities** (#494).
- Execute **RWP-00.31 — Café, Bakery & Dessert Required Capabilities** (#506).
- After RWP-00.56 is merged, verified, closed, and released, execute **RWP-00.57 — Hospitality Capability Classification** (#532).
- After RWP-00.72 is merged, verified, closed, and released, execute **RWP-00.73 — Entertainment & Attractions KPIs & Analytics** (#548).
- Keep Food Truck & Concession complete through **RWP-00.50**. Begin RWP-00.75 only after RWP-00.26, RWP-00.38, RWP-00.50, RWP-00.62, and RWP-00.74 are all complete.

## Parallel-Stream Rule

Each industry remains sequential inside its approved RWP range. Restaurant is the canonical baseline, only merged industry-specific documents are authoritative, and shared living records follow `docs/process/SHARED_FILE_WRITE_PROTOCOL.md` with queued semantic updates and short transactional write windows.

## Boundaries

- Do not start product implementation from Track 0 issues.
- Do not resume RWP-13.06 or Phase 14+.
- Do not implement UI, API, schema, migrations, billing, entitlements, feature gates, limits, rollout controls, privacy systems, localization, analytics, inventory, tap-management, reservation, ticketing, admissions, access, identity, sports, property-management, event, room-booking, transport, point-of-sale, guest-service, gaming, emergency, mapping, AI, hardware, managed services, or integrations during industry planning.
- Integration and external-system tests remain skipped under the standing owner instruction.
