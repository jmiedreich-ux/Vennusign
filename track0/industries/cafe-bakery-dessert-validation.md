# Café, Bakery & Dessert Validation, Review & Handoff

## Purpose

This document validates RWP-00.27 through RWP-00.37 as one coherent Café, Bakery & Dessert Track 0 industry profile. It confirms inheritance, terminology, operating characteristics, core protection, classification, packaging, onboarding, dashboard, analytics, recovery, and unresolved owner decisions before cross-industry consolidation.

This is documentation and product planning only. It does not authorize UI, API, schema, migration, billing, entitlement, feature-gate, limit, rollout, analytics, integration, AI, hardware, managed-service, or product implementation.

## Record set reviewed

| RWP | Planning result |
| --- | --- |
| RWP-00.27 | Industry definition and Restaurant inheritance |
| RWP-00.28 | Venue subtypes, neutral fallback, and hybrid rules |
| RWP-00.29 | Business terminology and operator/guest wording |
| RWP-00.30 | Operating characteristics |
| RWP-00.31 | Required capabilities |
| RWP-00.32 | Optional capabilities |
| RWP-00.33 | Canonical capability classification |
| RWP-00.34 | Subscription-tier mapping proposal |
| RWP-00.35 | Onboarding experience |
| RWP-00.36 | Default dashboard |
| RWP-00.37 | KPIs and analytics |

Only merged default-branch records are authoritative.

## Validation summary

**Result: PASS — coherent and ready for cross-industry consolidation.**

The profile preserves the Restaurant baseline while defining only meaningful Café deltas. Essential manual operation remains universally core. Industry and subtype remain non-commercial product state. Permissions, product/domain state, tier entitlement, independent add-on, limit, privacy/source relationships, and rollout flags remain separate. The onboarding, dashboard, and analytics journeys use the same classification and recovery model.

No material internal contradiction or missing industry-planning area was found.

## Industry and inheritance validation

The profile correctly treats Café, Bakery & Dessert as a native industry that inherits shared Restaurant capabilities, including:

- venue and menu/content management;
- ordinary operating information and hours;
- screen pairing and purpose;
- explicit targeting, preview, immediate publishing, and per-target confirmation;
- offline/outdated awareness;
- correction, supersession, retry, undo, and restoration;
- ordinary permissions and accessible operation; and
- core operational visibility.

Café-specific deltas are bounded to early and cross-midnight operation, batch-led products, freshness guidance, rotating and seasonal products, rapid sell-outs and returns, preorder/custom-order and pickup communication, subtype vocabulary, compact service models, and environment-specific presentation priorities.

No inherited Restaurant behavior is duplicated into a new commercial capability merely because Café terminology differs.

## Subtype and terminology validation

The approved subtype model is coherent:

- Café;
- Coffee Shop;
- Tea Shop;
- Bakery;
- Patisserie;
- Bakery-Café;
- Dessert Shop;
- Frozen Dessert Shop;
- Juice & Smoothie Bar; and
- Unspecified / General Café.

One primary subtype plus optional descriptive traits resolves hybrid cases without stacking entitlements. The neutral fallback supports ambiguous or mixed concepts. Subtype changes defaults, terminology, starter content, screen-purpose suggestions, dashboard emphasis, and analytics presentation only.

Terminology consistently distinguishes:

- product, category, collection, size, option, modifier, temperature, base, flavor, topping, add-in, and batch;
- available, unavailable, sold out, limited, next batch, available again, preorder, pickup, and service-period state;
- customer-authored guidance from authoritative source values; and
- public operational wording from private order, customer, payment, production, or fulfillment data.

The model prevents unsupported claims about freshness, quantity, safety, readiness, return time, demand, or customer behavior.

## Operating-characteristics validation

The operating model consistently covers:

- local timezone and business-day boundaries;
- early opening and cross-midnight service;
- independent venue, service-period, preorder, pickup, counter, table, and mixed-service contexts;
- batch, sell-out, limited, expected-return, freshness-guidance, seasonal, and rotating-product rhythms;
- current versus planned state;
- source identity, freshness, conflict, override, and manual fallback;
- explicit screen purpose and target scope;
- partial delivery, outdated screens, retry, correction, and restoration;
- multi-venue inheritance with local preservation; and
- subtype-specific operating emphasis.

Unknown values remain unknown. One object or period does not infer another object’s state.

## Required-core validation

The required capability set protects daily operation in eleven groups:

1. venue and operating information;
2. menu/product/category/size/option management;
3. rapid availability, sell-out, batch, and freshness updates;
4. preorder/custom-order/pickup public presentation;
5. screen pairing, purpose, and explicit targeting;
6. preview and publication;
7. correction, supersession, undo, and restoration;
8. source, freshness, conflict, and manual fallback;
9. roles and permissions;
10. complete operating, delivery, error, and recovery states; and
11. accessibility, responsiveness, and localization readiness.

These remain available without a premium tier or paid integration. The first screen can be configured, updated, published, verified, corrected, and restored through core capability.

## Optional-capability validation

The optional catalog consistently separates:

### Native tier candidates

- advanced schedules, rotations, planned transitions, and exception calendars;
- campaigns, promotions, richer presentation, and brand libraries;
- multi-screen and multi-venue coordination;
- approvals, advanced history, governance, and enterprise administration;
- advanced localization workflow;
- advanced analytics, optimization, and native engagement workflow.

### Independent add-on candidates

- POS, inventory, production, ordering, payment, fulfillment, loyalty, CRM, messaging, supplier, calendar, weather, event, traffic, identity, translation, and other external systems;
- AI and externally metered content or analysis;
- managed players, screens, installation, connectivity, monitoring, operational response, support, content, localization, analytics, and campaign services; and
- custom integrations and customer-specific data services.

### Limits

Counts, storage, retention, frequency, export, transactions, API/sync, messaging, translation, monitoring, support, and AI consumption remain independent limits.

Every optional capability preserves manual fallback, customer-authored data, current safe publication, source/freshness context, delivery confidence, disconnect behavior, downgrade behavior, export/retention expectations, and recovery.

## Classification validation

Every material concern has one primary classification:

- **Core capability:** ordinary manual operation, first-screen use, rapid correction, publication confidence, and recovery.
- **Product/domain state:** industry, subtype, terminology preference, content, products, operating values, sources, targets, publication, delivery, and recovery records.
- **Permission:** authority to view, edit, approve, target, publish, override, bulk-change, undo, restore, administer, or export.
- **Tier entitlement candidate:** advanced native workflow, coordination, governance, presentation, localization, analysis, and scale.
- **Independent add-on candidate:** external systems, metered services, managed services, physical services, and custom data services.
- **Usage or quantity limit:** counts, volume, frequency, storage, retention, exports, transactions, support, and consumption.
- **Internal rollout flag:** temporary exposure, migration, compatibility, experiment, or emergency-disable control.

Key ambiguities are resolved:

- sold out/unavailable/next batch/pickup paused are product states, not feature flags;
- permission denial is not tier denial;
- integration automation does not replace core manual operation;
- advanced workflow is separate from its quantity limit;
- basic recovery history is core while extended history may be tiered/limited;
- external translation and AI are add-ons, while manual language remains core; and
- subtype affects recommendations rather than access.

## Capability-matrix validation

The current cross-industry capability matrix already records the meaningful Café deltas through shared rows for:

- manual availability and sold-out/batch rhythms;
- batch, freshness, expected-return, preorder, and pickup values;
- core manual terminology and operational communication;
- POS-driven availability synchronization as an add-on candidate;
- screen counts and other allowances as limits; and
- industry/subtype/terminology as product state.

RWP-00.33’s detailed classification document supplies the authoritative Café-specific expansion. No duplicate matrix rows are required merely to restate inherited or already normalized behavior. Cross-industry consolidation may merge or refine shared rows after all industries complete.

## Subscription proposal validation

The proposed outcome progression is compatible with Restaurant and Bar planning:

1. **Operate:** complete daily manual core.
2. **Coordinate:** scheduling, rotations, campaigns, richer presentation, approvals, localization, and native analysis.
3. **Portfolio:** multi-venue inheritance, bulk coordination, governance, and comparative analysis.
4. **Enterprise:** identity, administration, audit, risk, complex portfolio, reporting, and service workflow.

These names are planning archetypes, not approved commercial tiers. External/managed services stay independent; quantities stay limits. Upgrade does not change permissions or publish automatically. Downgrade preserves content, current safe delivery, manual core, source/freshness, and recovery, with unresolved owner decisions around grace, read-only, archive, export, and active advanced objects.

## Onboarding validation

The onboarding journey reaches genuine value before pricing or optional setup:

- establish organization/venue context;
- select a subtype or neutral fallback;
- capture a simple service model and optional public hours/preorder/pickup context;
- choose and pair/select the first screen purpose;
- use editable subtype-aware starter content without invented facts;
- complete one real useful core update;
- preview, publish, and confirm delivery per target; and
- enter the actual dashboard with clear next actions.

The journey supports new, existing, invited, returning, mixed-industry, and experienced users; durable save/resume; role-aware authority; pairing and delivery recovery; phone/desktop/accessibility; and contextual optional-capability discovery. Pricing remains directly accessible but does not interrupt first-screen activation. RWP-13.06 remains paused.

## Dashboard validation

The default dashboard is exception-first and task-first:

- persistent venue, subtype, local time, service, authority, and source context;
- urgent public-impact exceptions;
- rapid product, availability, batch, pickup, hours, closure, and recovery actions;
- now/today/next service context;
- product/freshness, preorder/pickup, source, screen, and publication health;
- per-target intended-versus-delivered revision status;
- role and subtype emphasis;
- truthful permission/tier/add-on/limit/integration/rollout/business-state distinctions;
- mobile urgent operation and richer desktop coordination; and
- complete empty, mixed, failure, accessibility, and recovery states.

Healthy aggregate state cannot hide one failed, outdated, excluded, or unknown target.

## KPI and analytics validation

Core current operational evidence includes screen, intended/delivered revision, publication, source, freshness, conflict, exception, and recovery facts needed to operate safely.

Advanced native trends, comparisons, reports, exports, portfolio views, and optimization remain tier candidates. POS/sales, inventory/production, ordering/fulfillment, loyalty/messaging, footfall/queue, weather/event/traffic, premium AI, and managed analysis require authoritative add-ons.

Every metric defines formula, source, freshness, coverage, timezone/business-date, correction, privacy, permission, retention, export, and evidence-versus-inference status. Vennusign state never silently becomes sales, demand, conversion, inventory, readiness, customer behavior, attendance, queue, or attribution evidence.

## User-journey consistency

The profile uses one consistent journey contract:

- identify venue and current operational context;
- make an explicit, authorized change;
- preserve source and state truth;
- select targets;
- preview public impact;
- publish;
- verify each intended target;
- correct, retry, supersede, undo, or restore; and
- retain a clear next action.

Onboarding teaches this contract, the dashboard operationalizes it, and analytics measures only the evidence it produces.

## Accessibility and Impeccable validation

Planning consistently requires:

- task-first Operate-mode hierarchy;
- explicit verb-object actions;
- persistent labels and visible focus;
- keyboard and assistive-technology support;
- 200% zoom and responsive reflow;
- phone and desktop operation;
- non-color status and reduced motion;
- long names and localization expansion;
- low-light, glare, crowding, noise, and interrupted-operation consideration;
- complete first-use, empty, loading, validation, permission, integration, tier, add-on, limit, failure, correction, and recovery states; and
- contextual, outcome-led optional-capability discovery that does not interrupt urgent work.

## Unresolved owner decisions

No unresolved issue blocks industry-profile completion. The remaining decisions belong to cross-industry consolidation and implementation planning:

- final number, names, and prices of tiers;
- exact capability boundaries among Coordinate, Portfolio, and Enterprise outcomes;
- allowances, counting scope, overage, and limit behavior;
- add-on grouping, marketplace/partner/direct delivery, and included low-cost subsets;
- trial access and post-trial behavior;
- downgrade, grace, archive, read-only, export, retention, and deletion policies;
- organization versus venue purchase scope;
- integration source, privacy, authorization, support, and safe-exit policies;
- which analytics are core, tiered, externally sourced, premium, or consumption-backed;
- AI/translation allowance and data-use policy;
- implementation sequence and bounded delivery packages; and
- owner review of the final normalized cross-industry matrix.

## Final handoff

Café, Bakery & Dessert is complete through **RWP-00.38** and is ready for the cross-industry consolidation gate.

Issue #513 originally referenced starting Food Truck RWP-00.39 after Café completion. That instruction is superseded by the verified current default-branch state: Food Truck & Concession is already complete through RWP-00.50. Do not recreate or restart completed Food Truck work.

The next Track 0 action for Café is **no further industry RWP**. Wait until all five completion gates are satisfied:

- Bar, Brewery & Nightlife through RWP-00.26;
- Café, Bakery & Dessert through RWP-00.38;
- Food Truck & Concession through RWP-00.50;
- Hospitality through RWP-00.62; and
- Entertainment & Attractions through RWP-00.74.

Once all are merged and verified, begin the approved consolidation queue at the first open item from RWP-00.75 through RWP-00.81. Do not begin consolidation early and do not implement product behavior from this validation record.