# Food Truck & Concession Subscription Tier Mapping

## Purpose

This document proposes outcome-based subscription bundles for Food Truck & Concession customers without defining final commercial names, prices, contracts, or implementation. The mapping preserves the complete required-core capability set for every customer and keeps industry selection, subtype, permissions, product state, independent add-ons, quantity limits, and rollout controls separate from tiers.

## Packaging principles

1. Every tier includes the required core established by RWP-00.43.
2. A customer must be able to manage a menu, make rapid availability changes, represent current location/event/service state, target screens, publish, confirm delivery, identify offline or outdated screens, retry, and restore without purchasing a higher tier or integration.
3. Tiers bundle optional product outcomes; they do not represent venue subtype or physical form.
4. Integrations, managed hardware, connectivity, installation, external data, and specialized services remain independent add-on candidates unless the owner later chooses an explicit bundle.
5. Counts, volume, frequency, retention, and consumption remain limits and are not described as capabilities.
6. Permissions remain available according to organization policy and object authority; a higher tier does not silently grant a user more authority.
7. Internal rollout flags never appear as customer tiers or purchasable access.

## Working outcome bundles

The names below are planning labels only.

### Core Operations

Customer outcome: run one or a small number of mobile or concession operations confidently during ordinary service.

Includes:

- the complete required-core capability set;
- menu and content authoring;
- manual Quick Update and scoped availability/sell-out states;
- manual current location, event, service-period, queue, pickup, lane, delay, relocation, closure, and reopening communication;
- screen pairing, explicit targeting, preview, immediate publish, per-target confirmation, offline/outdated awareness, retry, and restoration;
- basic operational status and included publication history;
- foundational permissions and local authority;
- starter layouts, subtype recommendations, and accessibility/readability support.

It does not require a POS, route, event, mapping, weather, order, inventory, venue, messaging, AI, or analytics integration.

### Coordinated Operations

Customer outcome: plan recurring service and coordinate several units, stands, events, or operators with fewer manual inconsistencies.

Adds candidate tier entitlements such as:

- recurring route, stop, market, event, residency, and service scheduling;
- reusable schedule and campaign templates;
- organization templates with local overrides;
- safe multi-unit or multi-stand bulk actions;
- groupings by region, event, host, operator, or service model;
- mixed-state and exception-first visibility;
- delegated publishing and bounded approval workflows;
- public current-location pages, route calendars, or multi-unit directories where approved;
- advanced promotion scheduling and expiration;
- broader publication and audit history within the tier's retention allowance.

External maps, notifications, host calendars, POS, order, inventory, or event feeds remain add-ons. Included quantities remain separate limits.

### Advanced Operations

Customer outcome: optimize a larger or operationally complex network with advanced governance, insight, automation, and support.

Adds candidate tier entitlements such as:

- advanced schedule conflict detection and coordinated launch, pause, rollback, or restoration;
- advanced approvals and inheritance policies;
- enterprise exception monitoring and operational summaries;
- advanced analytics, benchmarking, forecasting, scheduled reports, and exports;
- AI-assisted drafting, layout suggestions, summaries, and recommendations with human review;
- promotion, menu, location, event, and service-window performance analysis;
- advanced source-conflict and integration-governance workflows;
- enterprise identity or administration outcomes where approved;
- premium support workflow access where it is a product entitlement rather than a separate service.

External connectors, data providers, managed hardware, cellular service, installation, and specialist services remain independent add-on candidates. Usage is subject to explicit limits.

## Capability-to-tier candidate mapping

| Capability outcome | Core Operations | Coordinated Operations | Advanced Operations |
| --- | --- | --- | --- |
| Required menu, availability, location/event, targeting, publishing, confirmation, offline awareness, retry, and restore | Included | Included | Included |
| Manual queue, pickup, service-window, disruption, and closure communication | Included | Included | Included |
| Basic screen and operation status | Included | Included | Included |
| Recurring route, stop, event, and service scheduling | — | Included candidate | Included |
| Reusable templates and local overrides | — | Included candidate | Included |
| Safe multi-unit bulk actions | — | Included candidate | Included |
| Public location pages and route calendars | — | Included candidate | Included |
| Promotion scheduling and expiration | — | Included candidate | Included |
| Advanced approvals and inheritance policies | — | Bounded candidate | Included candidate |
| Enterprise exception monitoring | — | Bounded candidate | Included candidate |
| Advanced analytics, benchmarking, forecasts, and scheduled reports | — | — | Included candidate |
| AI-assisted content and operational summaries | — | — | Included candidate subject to limits |
| Enterprise governance and advanced source-conflict workflow | — | — | Included candidate |

A dash means the optional outcome is not proposed as tier-included at that level; it does not remove any required manual core behavior.

## Independent add-on candidates

The following remain independently selectable candidates so customers can combine them with the tier that matches their operating scale:

- POS menu, price, sales, and transaction synchronization;
- ordering and payment systems;
- inventory, kitchen, production, and batch-status systems;
- route, map, geocoding, and traffic providers;
- venue, event, host, gate, section, sponsor, and calendar feeds;
- weather and public-condition data;
- queue, wait-time, footfall, occupancy, and sensor data;
- loyalty, coupon, CRM, messaging, notification, and social platforms;
- workforce, staffing, catering, delivery-marketplace, and operations systems;
- managed hardware, outdoor equipment, rentals, installation, warranty, and replacement;
- managed connectivity, cellular plans, routers, remote diagnostics, and proactive monitoring;
- priority or managed support and managed content/campaign services;
- specialized AI, translation, media, or external-data services.

An add-on must expose source, freshness, authority, failure, disconnect, and manual fallback. Add-on availability may depend on region, provider, contract, hardware, or host relationship without changing the customer's industry or tier.

## Limits kept separate

Candidate limits include:

- organizations, operations, units, stands, service points, screens, and players;
- users, roles, approvers, groups, and bulk targets;
- routes, stops, schedules, events, campaigns, public pages, and subscribers;
- integrations and connected locations;
- transactions, API calls, refreshes, messages, notifications, AI generations, storage, and data volume;
- publication, audit, analytics, and external-data retention;
- report rows, exports, scheduled deliveries, and support/service allowances.

When a limit is reached, the product must identify the exact allowance and recovery path. It must not describe the condition as missing permission, missing capability, offline state, or integration failure.

## Organization and multi-unit inheritance

- Organization tier and add-on access may be inherited by child operations only where the commercial account and owner policy allow it.
- Local subtype, location, event, menu, availability, screen targets, host context, permissions, and customer-authored content remain local product/domain concerns.
- A parent organization may share templates or policy without forcing identical content or state across unlike units.
- Local overrides must remain visible and reversible.
- Removing a unit from an organization must not silently transfer content ownership, integration authority, or historical data.
- Mixed-industry organizations must not receive incompatible defaults merely because the parent has a Food Truck & Concession profile.
- Whether a stand, window, truck, trailer, cart, kiosk, event, or public page consumes an allowance remains an owner decision.

## Upgrade experience

An upgrade should:

- explain the operational outcome gained rather than list internal feature flags;
- preserve all existing content, state, permissions, screen assignments, and history;
- identify any newly available setup steps without forcing immediate configuration;
- distinguish tier-included behavior from add-ons and limits;
- avoid implying that external data exists before an integration is connected;
- support preview and clear billing/administrative authority before confirmation.

## Downgrade behavior

A downgrade must be safe, predictable, and non-destructive:

- required core operation remains available;
- customer-authored content, current operating state, screen pairing, and last successful publication are preserved;
- optional schedules, templates, approvals, analytics, AI history, public pages, or advanced governance are not silently deleted;
- affected optional items become read-only, paused, exportable, or require resolution according to a later owner-approved policy;
- over-limit quantities are identified explicitly and are not randomly removed;
- scheduled future actions must not continue invisibly when the entitlement ends;
- integrations remain governed by their own add-on status and disconnect policy;
- retention windows and deletion dates must be disclosed before data is removed;
- users must see the difference between downgrade impact, permission denial, integration failure, and product state.

## Owner decisions required

Final owner decisions are still required for:

- customer-facing tier names and number of tiers;
- final inclusion of each optional outcome;
- whether public location publishing belongs in Coordinated Operations or is a separate add-on;
- whether approvals, enterprise identity, premium support, and AI are tier-included, metered, or separate add-ons;
- exact quantity, usage, retention, export, and support allowances;
- counting rules for units, stands, windows, service points, events, schedules, and screens;
- upgrade proration, trials, grace periods, over-limit handling, and downgrade timing;
- read-only, pause, export, archive, and deletion treatment after downgrade;
- regional/provider/partner availability and contractual constraints;
- organization inheritance and mixed-industry packaging rules.

## Experience and Impeccable guidance

Future packaging surfaces must distinguish included, purchasable add-on, limited, permission-restricted, unconfigured, disconnected, stale, unsupported, and internally staged states. They must show the current plan, the operational outcome, impacted operations, billing authority, effective date, downgrade effects, and recovery path. Mobile and desktop layouts, keyboard access, 200% zoom, assistive technology, long names, and non-color-only status are required planning considerations.

No pricing UI, checkout flow, entitlement engine, billing rule, feature gate, migration, API, schema, or product behavior is authorized by this document.

## Validation

The mapping preserves every required core capability in every proposed tier, separates industry and subtype from commercial access, keeps add-ons and limits independent, documents organization inheritance and downgrade behavior, and records unresolved owner decisions without pretending that final packaging is approved.
