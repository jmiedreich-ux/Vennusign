# Track 0 Owner Approval & Implementation Handoff

## Status

RWP-00.81 completes the Track 0 consolidation planning package. The package is ready for owner review but **does not authorize product implementation, pricing, billing changes, new entitlement records, resumed RWP-13.06 work, or Phase 14+ work**.

Track 0 artifacts:

1. `CROSS_INDUSTRY_MODEL.md`
2. `EXISTING_PRODUCT_INVENTORY.md`
3. `RECONCILIATION_GAP_ANALYSIS.md`
4. `TIER_AND_ADDON_ARCHITECTURE.md`
5. `LIMITS_SCOPE_INHERITANCE_POLICY.md`
6. `CUSTOMER_JOURNEY_VALIDATION.md`
7. this final decision and implementation handoff

## Consolidated result

The Restaurant baseline and Bar, Café, Food Truck, Hospitality, and Entertainment profiles form one coherent cross-industry model.

The model requires one primary classification per concern:

- core capability;
- permission;
- product/domain state;
- tier entitlement candidate;
- independent add-on candidate;
- usage/quantity limit;
- internal rollout flag.

The current product has strong foundations to preserve:

- server/provider-authoritative subscription and entitlement confirmation;
- server-validated venue-scoped sessions and context switching;
- separate HaaS contracts;
- explicit screen revision/delivery state;
- reusable accessible locked/upgrade presentation patterns;
- server-side downgrade checks for current screen and venue usage.

The principal architecture gap is a normalized server capability-decision/reason model. Current flat and overloaded keys do not consistently distinguish commercial access, actor permission, product state, add-on configuration/connection, typed limits, privacy/rights restrictions, inheritance/override, support exceptions, or rollout/support availability.

## Owner approvals required

The owner should explicitly approve, reject, or amend each decision group before implementation packages are activated.

### Decision A — Universal core

**Proposed approval:** The complete essential manual operating path is included in every software tier:

- industry-aware content and ordinary public information;
- manual state/availability/hours/schedule/notice/wayfinding updates;
- manual language variants and accessibility-ready content;
- screen pairing/selection, purpose, exact targeting, preview, explicit publish, delivery confidence, correction, expiry/supersession/unpublish, retry, undo, and restore;
- source/freshness/conflict/override awareness and manual fallback;
- core operational evidence and recovery.

Advanced automation, coordination, workflow, localization, analytics, portfolio, and enterprise outcomes may be tiered.

### Decision B — Industry and subtype

**Proposed approval:** Industry, subtype, and descriptive traits are non-commercial product configuration. They affect terminology, defaults, starter content, screen-purpose recommendations, dashboard emphasis, and analytics presentation. They never grant a tier, add-on, permission, limit, or rollout access.

### Decision C — Working software outcome architecture

**Proposed approval for implementation planning, not final naming:**

- Operate — universal essential manual core;
- Coordinate — advanced planning, presentation, workflow, localization, and native analytics;
- Portfolio — multi-site inheritance, local overrides, safe bulk actions, portfolio governance and analytics;
- Enterprise — identity, policy, data, retention, assurance, and enterprise administration.

Owner must approve final names and exact capability placement separately.

### Decision D — Independent add-ons

**Proposed approval:** External systems, separately metered services, hardware/service contracts, HaaS, connectivity, installation, monitoring, managed content/localization/analytics/support, and custom integration remain independently modeled add-ons even when commercially bundled or discounted.

Proposed families:

- commerce and food operations;
- Hospitality systems;
- Entertainment and venue systems;
- data, environmental, mapping, messaging, and communication sources;
- translation, AI, and metered assistance;
- identity and enterprise connections;
- hardware, HaaS, connectivity, installation, monitoring, and managed services.

### Decision E — Permissions and authority

**Proposed approval:** Permission is always independent of commercial entitlement. Future capability decisions evaluate organization access, attachment scope, actor/action permission, product-state compatibility, add-on state, limit state, privacy/rights/safety restrictions, and rollout/support availability independently.

### Decision F — Typed limits

**Proposed approval:** Replace untyped feature limit strings with typed allowances containing unit, source, scope, pool mode, period, usage, enforcement mode, exception/grace, and remediation. Screen/venue tier limits, layout capacity, provider consumption, AI/translation consumption, and HaaS/service contract terms remain distinct domains.

Owner must separately decide numeric values, pooling, warning thresholds, overage, grace, grandfathering, and enforcement.

### Decision G — Inheritance and local overrides

**Proposed approval:** Organization commercial access may inherit to eligible local contexts, while permissions, product state, sources, add-on attachment, privacy/rights, and limits remain independently scoped. Organization defaults seed but do not silently overwrite local content or authority. Local overrides are explicit, reversible, audited, and display inherited versus effective value. Mixed-industry organizations use canonical capability IDs with local terminology.

### Decision H — Downgrade, cancellation and public-output protection

**Proposed approval:** Before downgrade or add-on removal, show lost advanced outcomes, typed usage conflicts, counted objects, inheritance impact, active-screen/public-output risk, scheduled work, history/export/retention effects, add-on prerequisites, and least-destructive remediation. Essential correction, unpublish, active-screen safety, approved export, and recovery remain protected. Automatic destructive deletion is not the default.

Owner must decide grace/read-only periods, retention/export/deletion, active-screen protection duration, and whether downgrade is blocked, scheduled, or allowed with grace.

### Decision I — Locked and unavailable state system

**Proposed approval:** Future UI distinguishes:

- upgrade available;
- add-on required/unconfigured;
- permission restricted;
- usage/quantity limit reached;
- product state unavailable/closed/sold out/etc.;
- source disconnected/stale/conflicted;
- unsupported context;
- privacy/rights/safety restricted;
- rollout/support temporarily unavailable.

Each state has a truthful reason, specific safe action, accessible semantics, responsive behavior, authoritative timing, and recovery. A generic padlock/upgrade message cannot represent all conditions.

### Decision J — Provider and browser authority

**Proposed approval:** Preserve server/provider-authoritative billing. Checkout/Billing Portal returns and browser pending state remain informational. Browser catalogs and lock components never create entitlement or permission. Server decisions and provider-confirmed state remain authoritative.

### Decision K — Legacy keys and migration

**Proposed approval:** Preserve current tier slugs and feature/session capability keys as compatibility aliases during a bounded migration. Introduce stable canonical capability IDs independent of tier name and industry. Do not delete dormant/overloaded keys until all server, data, test, support, and customer consumers are verified.

### Decision L — RWP-13.06 disposition

**Recommended decision: do not resume RWP-13.06 as currently written. Rewrite or replace it after the capability foundation is approved and implemented.**

The future onboarding/trial package should:

- consume canonical industry/subtype state and capability decisions;
- reach one confirmed useful screen before forced pricing or external integration;
- present trials/plans/add-ons only for genuine advanced outcomes;
- distinguish software tier, add-on, permission, limit, source, state, restriction, and rollout;
- preserve provider-authoritative access and safe pending states;
- include downgrade/end-of-trial, active-screen, content, export, and recovery behavior;
- support save/resume, invited/existing/returning customers, accessibility, responsive operation, and mixed-industry organizations.

Until the owner approves this disposition and prerequisite foundation packages, issue #466 remains held.

## Recommended implementation package sequence after approval

These are planning candidates, not active claims or approved RWPs. After owner approval, create bounded issues/RWPs with accepted scope and exact sequencing.

### Foundation 1 — Canonical capability registry and legacy aliases

- define stable capability IDs and outcome metadata;
- map current session/effective feature keys and tier slugs as compatibility aliases;
- define one primary classification per capability;
- no UI placement migration yet.

### Foundation 2 — Server capability decision and reason contract

- independently evaluate commercial access, permission, product-state compatibility, add-on state, typed limits, privacy/rights/safety, inheritance/override, exception, and rollout/support state;
- return stable primary reason, supporting reasons, safe actions, source/version/time;
- preserve existing session/billing response compatibility.

### Foundation 3 — Scoped permission and authority model

- normalize per-action organization/venue/object permissions;
- map existing claims and session capabilities;
- keep route presentation derived and operation authorization server-side;
- add support/admin authority boundaries.

### Foundation 4 — Essential core and overloaded-key migration

- split essential manual core from advanced automation in Quick Update, schedules/meal periods, manual languages/translation, Happy Hour, screen/theme/layout, and multi-location behavior;
- decompose `all_layouts`, `pos_integration`, `multi_location`, `video_wall`, and duplicated entitlement/state fields;
- verify dormant `staff_app`, `white_label`, and `html_editor` consumers before disposition.

### Foundation 5 — Typed allowance and usage service

- introduce typed allowances, counting, pooling, usage, enforcement, grace/exception, downgrade conflict, and remediation;
- migrate `MaxScreens`, `MaxVenues`, and feature `limitValue` compatibility;
- keep layout capacity and contract terms separate.

### Foundation 6 — Add-on instance, attachment and source health model

- catalog/instance/attachment/administrator/configuration/connection/freshness/conflict/override/limit/privacy/support/removal state;
- migrate POS and other current integrations first;
- preserve HaaS separation.

### Foundation 7 — Organization inheritance, local override and exception model

- organization defaults, local effective values, reversible overrides, mixed-industry behavior, safe copy/bulk actions, support exceptions, expiry, audit, and precedence.

### Experience 1 — Unified access-state UI system

- migrate locked navigation, previews, hints, nudges, upgrade sheet, tier decision, limit, permission, source, state, restriction, and rollout presentation to the server reason model;
- preserve Impeccable accessibility/responsive/recovery requirements;
- keep hosted billing and provider authority.

### Industry 1 — Canonical industry configuration and neutral objects

- organization/venue industry/subtype/traits, effect preview, preservation, neutral terminology, and mixed-industry context;
- no feature access change from industry selection.

### Industry 2+ — Bounded native-industry operating slices

Implement complete vertical slices in owner-selected order. Each slice should include domain state, API, permissions, capability decisions, limits, UI, player/display impact, focused non-integration tests, and migration only where required. Do not implement every industry simultaneously.

Suggested prioritization criteria:

- current customer demand and revenue opportunity;
- reuse of existing Restaurant/menu/screen behavior;
- operational risk and legal/privacy complexity;
- required external systems;
- ability to deliver a complete first-value and daily-operation slice.

### Commercial 1 — Tier/add-on catalog and migration

After capability/permission/limit/add-on foundations:

- server-managed outcome metadata and final owner-approved packages;
- legacy tier/key migration;
- customer comparison, upgrade, downgrade, add-on attachment/removal, and allowance presentation;
- no custom payment handling; preserve hosted provider paths.

### Onboarding 1 — Rewritten trial/packaging onboarding

Rewrite/replace RWP-13.06 using the approved foundations and journey validation. This package should be scheduled only after core capability decisions and access-state UI are available.

### Validation 1 — Cross-industry critical journeys and migration closure

- focused non-integration contract/unit/UI tests for all approved journeys;
- migration/compatibility verification;
- exact-head Actions review;
- integration/device/provider tests only when separately authorized and environments exist.

## Package creation rules after approval

- One bounded vertical slice per RWP.
- Strict sequential dependencies for shared foundations.
- Parallel lanes only for independent files/domains with explicit ownership.
- Every UI-changing package consults project-local Impeccable guidance and records complete state/accessibility/responsive/recovery analysis.
- Every package identifies migrations, deployment compatibility, legacy-key behavior, focused validation, skipped integration tests, and rollback/recovery.
- Do not create one mega implementation RWP for the entire model.
- Do not revive obsolete phase numbering merely to preserve history; use the current approved roadmap structure.

## Final unresolved commercial and policy decisions

Even after architecture approval, the owner must separately decide:

- final tier/add-on names and exact capability placement;
- prices, annual rules, trials, promotions, taxes, contracts, grandfathering, discounts, and bundle presentation;
- numeric allowances, pooling, overage, grace, and enforcement;
- provider/region/rights/prerequisite/service-level/support policy;
- privacy, camera, biometric, child, safety, emergency, alcohol, gambling, licensing, advertising, sponsor, and content-rights obligations;
- retention, export, deletion, legal hold, audit, and data-region policy;
- player/device acknowledgement, offline behavior, monitoring, installation, replacement, and support commitments;
- metric definitions, source agreements, reconciliation, uncertainty, alerts, and BI/export;
- industry implementation order and launch market sequencing.

## Owner approval checklist

Record explicit decisions for A through L:

- [ ] A — Universal core
- [ ] B — Industry/subtype non-commercial behavior
- [ ] C — Software outcome archetypes and placement direction
- [ ] D — Independent add-on architecture
- [ ] E — Permission/authority separation
- [ ] F — Typed limits direction
- [ ] G — Inheritance/local override direction
- [ ] H — Downgrade/cancellation/public-output protection
- [ ] I — Locked/unavailable reason system
- [ ] J — Server/provider authority
- [ ] K — Legacy compatibility migration
- [ ] L — Rewrite/replace RWP-13.06 rather than resume unchanged

Approval may be partial. Implementation packages should be created only for approved dependencies and must not silently decide unchecked items.

## Final Track 0 result

Track 0 planning and consolidation are complete through RWP-00.81. The repository now contains a normalized model, current-product inventory, reconciliation/gap analysis, unified tier/add-on architecture, limits/scope/inheritance policy, cross-industry journey validation, and explicit owner decision/implementation handoff.

The next action is owner review and explicit approval. There is no approved implementation RWP, no resumed RWP-13.06, and no Phase 14+ work at Track 0 closure.
