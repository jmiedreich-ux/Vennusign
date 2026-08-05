# Final Track 0 Industry Planning Validation & Handoff

## Status

The Track 0 industry planning sequence is complete and approved at the planning level. This record consolidates the final owner decisions made after RWP-00.78 and confirms that the complete twelve-subject industry review is coherent and ready for bounded implementation planning.

No product implementation, pricing, billing mutation, entitlement mutation, schema, API, integration, player, device, or Phase 14+ work is authorized by this document.

## Completed subject sequence

1. Industry Definition
2. Venue Subtypes
3. Business Terminology
4. Operating Characteristics
5. Required Capabilities
6. Optional Capabilities
7. Capability Classification
8. Subscription Tier Mapping
9. Onboarding Experience
10. Default Dashboard
11. KPIs and Analytics
12. Validation, Review and Handoff

## Validation result

**PASS — planning complete.**

The industry model is internally consistent across Restaurant, Bar/Brewery/Nightlife, Café/Bakery/Dessert, Food Truck/Concession, Hospitality, and Entertainment/Attractions.

The validation confirms:

- industry and subtype affect terminology, defaults, starter content, recommended dashboard emphasis, onboarding guidance, and suggested add-ons;
- industry and subtype do not independently grant entitlement, permission, limit, add-on access, or rollout access;
- essential manual operation remains a universal product foundation;
- advanced planning, workflow, analytics, portfolio, and enterprise outcomes may be tiered;
- permissions, product/domain state, tier access, independent add-ons, typed limits, privacy/rights/safety restrictions, exceptions, and rollout remain separate concerns;
- mixed-industry organizations remain supported through canonical capability IDs and local terminology;
- active public output, correction, unpublish, delivery confidence, and recovery remain protected during trial expiry, downgrade, limit enforcement, and migration;
- no business-performance KPI is presented without a trustworthy source and explicit definition.

## Approved commercial architecture direction

The working software ladder is:

1. Free
2. Operate
3. Coordinate
4. Portfolio
5. Enterprise

Pricing, final names, numeric allowances, taxes, contracts, and provider commitments remain intentionally undecided.

### Free

Free provides one complete useful outcome:

- one organization;
- one venue;
- one user;
- one active screen;
- one active static image;
- upload, crop/fit, preview, publish, delivery/status visibility, replace, and remove;
- no forced credit card;
- safe ongoing operation under future inactivity policy.

### Paid outcomes

- **Operate** — complete daily manual operation.
- **Coordinate** — recurring planning, campaigns, advanced presentation, workflow, localization, and native analytics.
- **Portfolio** — multi-location inheritance, local overrides, safe bulk actions, delegated administration, cross-site governance, and portfolio analytics.
- **Enterprise** — identity, policy, retention, security/audit assurance, data governance, enterprise brand/localization governance, and contractual administration.

Enterprise does not automatically mean unlimited.

## Screen-capacity decision

Software tier and active screen capacity are separate commercial dimensions.

- Tier determines what the customer can do.
- Screen capacity determines how much the customer can operate.
- Free includes one active screen.
- Paid tiers include a base screen allowance.
- Additional capacity may be offered through packs, pooled allowances, committed-volume bands, or negotiated structures.
- Billable capacity should use active managed endpoints rather than every registered, archived, test, spare, replacement-pending, or temporarily offline device.
- Hardware, HaaS, connectivity, installation, monitoring, replacement, and managed services remain independent add-ons.

## Industry starting-tier recommendations

Industry influences the recommendation, but the customer retains choice and actual operating complexity remains authoritative.

- Restaurant → Operate
- Café, Bakery & Dessert → Operate
- Bar, Brewery & Nightlife → Operate
- Food Truck & Concession → Operate
- Hospitality → Coordinate
- Entertainment & Attractions → Coordinate

Any industry may begin on Free for one static image on one screen. Recommendations move upward based on location count, recurring schedules, team workflow, shared templates, multiple zones/screens, governance, and external-system needs.

## Onboarding experience decision

The approved first-value journey is:

> Sign up → select industry/subtype → create organization and first venue → choose or upload starter content → pair one screen → preview and publish → confirm player heartbeat and expected content acknowledgement → show recommended Free, trial, or paid path.

The planning baseline includes:

- Free, paid-trial, invited-user, existing-customer, and returning-customer entry paths;
- server-side saved/resumable progress;
- industry-aware starter content and terminology;
- one dominant action per step;
- explicit preview and publish confirmation;
- separate pairing, heartbeat, publication, and delivery-acknowledgement states;
- offline, expired/used code, retry, failure, resume, and recovery behavior;
- responsive desktop/mobile operation and accessible focus/live-region behavior;
- no forced pricing or external integration before first useful screen;
- safe trial expiry and fallback to Free without deleting customer-created content or removing the only safe public output.

The existing RWP-13.06 implementation issue must be rewritten or reconciled with this approved Free-plus-trial model before implementation.

## Default dashboard decision

One shared dashboard structure will be used with tier and industry overlays rather than separate dashboard products per industry.

The approved baseline is:

- **Free** — current image, screen status, replace image, one clear next action.
- **Operate** — screen health, current content, quick operational updates, recent publishing, delivery exceptions.
- **Coordinate** — schedules, campaigns, approvals, assignments, coordinated presentation, alerts.
- **Portfolio** — cross-location exceptions, delivery health, inheritance/local overrides, capacity, safe bulk action status.
- **Enterprise** — identity, policy, audit, security, data governance, service status, contractual administration.

Industry overlays change terminology, card emphasis, starter actions, examples, and recommended add-ons without changing the underlying commercial authority.

The first compact dashboard wireframe is the approved planning direction. Final visual design remains an implementation-stage activity.

Required states include loading, empty, offline, stale, partial, error, permission-restricted, limit-reached, trial, upgrade-available, add-on-required, and recovery.

## KPI and analytics decision

Shared native evidence includes:

- screens online/offline;
- delivery success and failure;
- content freshness;
- delayed or failed publication;
- time to first live screen;
- active screen and capacity usage;
- recent operational changes;
- schedule completion where native scheduling exists;
- exceptions requiring attention.

Industry overlays may include:

- Restaurant/Café — menu freshness, availability/sold-out duration, promotion activity;
- Bar/Nightlife — tap/special update freshness, event communication, happy-hour activity;
- Food Truck — service-state, route/location, event-readiness updates;
- Hospitality — event schedule accuracy, guest-information freshness, property exceptions;
- Entertainment — session/program accuracy, closure communication, queue/event status.

A metric is shown only when Vennusign has:

- a defined source;
- a stable metric definition;
- authoritative scope and timestamp;
- freshness and partial-data behavior;
- reconciliation where multiple sources exist;
- privacy, rights, and retention handling.

Sales uplift, revenue impact, occupancy, queue, transaction, or externally sourced performance metrics require the corresponding connected and reconciled add-on/source. They are not inferred from signage activity alone.

## Tier lifecycle and billing continuity validation

The final model also confirms:

- promotions and specials overlay subscriptions or cohorts instead of rewriting sold tier versions;
- a tier version that has ever been sold or assigned cannot be physically deleted or reused;
- sold versions may be hidden, stopped from new sales, retired, and archived while retained for billing, entitlement, audit, refunds, disputes, reporting, and legal retention;
- promoting a tier version changes the default for new sales only;
- existing customers move only through explicit migration campaigns;
- billing uses the assigned tier version, screen capacity, add-ons, promotions, contract overrides, migration effective date, and provider-confirmed state;
- the public catalog is not billing authority;
- upgrades and downgrades require explicit timing, impact preview, conflict evaluation, provider/server confirmation, safe remediation, and non-destructive fallback.

## Outstanding decisions that do not block planning closure

- final tier and add-on names;
- prices, taxes, annual rules, discounts, contracts, and public packaging;
- exact screen and other allowance quantities;
- exact trial duration and eligibility;
- pooling, overage, warning, grace, and inactivity values;
- provider, region, rights, service-level, and support commitments;
- retention, export, deletion, and legal-hold durations;
- final implementation order for native-industry vertical slices.

## Implementation handoff

Implementation must remain split into bounded RWPs with explicit dependencies. The approved foundation order remains:

1. canonical capability registry and legacy aliases;
2. server capability decision and reason contract;
3. scoped permission and authority model;
4. essential-core and overloaded-key migration;
5. typed allowance and usage service;
6. add-on instance, attachment, and source-health model;
7. organization inheritance, local override, and exception model;
8. unified access-state UI;
9. canonical industry configuration and bounded native-industry slices;
10. commercial tier/version, Free/trial, screen-capacity, promotion, billing-continuity, and migration packages;
11. rewritten onboarding implementation;
12. final cross-industry migration and journey validation.

No implementation RWP is activated by this closure record. Each future RWP must be explicitly created, approved, sequenced, claimed, tested, reviewed, merged, verified, and released under the repository process.

## Final closure

The Track 0 industry planning and consolidation work is complete. The repository has sufficient approved direction to create the next bounded implementation roadmap without reopening the industry-planning subjects unless a future implementation finding exposes a material gap.
