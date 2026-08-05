# Hospitality Subscription Tier Mapping

## Authority and status

This document proposes the Hospitality tier architecture for RWP-00.58. It uses the authoritative RWP-00.57 classification and does not approve final tier names, prices, allowances, trials, contracts, or implementation.

The mapping is customer-outcome based. Industry and subtype remain product configuration and never determine price or access. Core capabilities remain available in the base operating tier. Independent add-ons and limits remain separate from tier entitlement.

## Tier design principles

1. A customer with one Hospitality property and one working screen can perform every essential daily guest-communication task in the base tier.
2. Higher tiers add coordination, governance, scale, and insight—not the ability to communicate accurate daily information.
3. External integrations, managed services, consumption-backed AI or translation, identity-provider connections, and hardware services remain independent add-ons.
4. Quantity and consumption allowances are limits, not capabilities.
5. Permissions determine who may use an included capability but do not determine whether it is commercially included.
6. Industry and subtype tune terminology, defaults, starter content, and presentation only.
7. Downgrade must preserve customer-authored data, current public operation, screen recovery, export, and an understandable path to remove or replace advanced dependencies.
8. Property-group inheritance must never silently override local operational truth.

## Proposed tier archetypes

Working names are descriptive placeholders and require owner approval.

### Tier 1 — Operate

**Customer outcome:** Run accurate, privacy-safe guest communication for a property every day.

Includes the full RWP-00.55 required core:

- property and local-context information;
- guest notices and operating-state communication;
- amenity, service, and outlet hours and availability;
- meetings, events, and directories;
- manual wayfinding and temporary routes;
- basic manually authored language variants and accessibility support;
- explicit screen targeting and preview;
- save, schedule, publish, delivery confirmation, and screen-health visibility;
- offline, outdated, stale-source, conflict, and override awareness;
- correction, expiration, supersession, retry, undo, and restoration;
- current exception and shift-handoff visibility;
- ordinary roles and permissions;
- all required loading, empty, validation, partial-delivery, failure, success, responsive, and accessibility states.

The Operate tier must not require a paid integration to remain useful. It may have limits on properties, screens, users, languages, templates, history, or storage, but those limits are not defined by this RWP.

### Tier 2 — Coordinate

**Customer outcome:** Coordinate richer guest experiences, teams, events, languages, and content workflows within one or several closely managed properties.

Adds candidate Vennusign-authored capabilities:

- advanced shift workflow, acknowledgment, assignment, escalation, and task routing;
- campaign and content-calendar workflow;
- recurring schedules, blackout periods, conflict detection, and orchestration;
- approval chains, separation of duties, delegation, and controlled supersession;
- reusable brand and property libraries with versioning and local override;
- advanced Vennusign wayfinding, map authoring, route variants, and searchable directories;
- advanced localization workflow, terminology libraries, translation assignment, and quality review;
- event, group, and audience coordination workflow without requiring guest-private data;
- expanded operational dashboards and scheduled reports based on Vennusign data.

Coordinate does not include external PMS, event, map, translation, AI, identity, or managed-service connections by default. Those remain add-ons.

### Tier 3 — Portfolio

**Customer outcome:** Govern and coordinate multiple properties, brands, regions, or operating groups while preserving local control.

Adds candidate Vennusign-authored capabilities:

- property groups, regions, brands, operating-company scope, and centralized exception views;
- shared standards, libraries, language portfolios, campaigns, and recovery playbooks;
- centralized preparation with local review, override, opt-out, or exception handling;
- safe cross-property copying and bulk actions with explicit included and excluded targets;
- advanced portfolio analytics, comparisons, exports, and data-quality views;
- delegated administration and property/group role templates;
- broader audit, history, scheduled reporting, and governance workflow;
- portfolio-level screen, source, delivery, outdated, language, and campaign oversight.

Portfolio does not make every connected external system inclusive. Connections remain separately selectable add-ons and retain property-specific source authority and privacy boundaries.

### Tier 4 — Enterprise

**Customer outcome:** Operate Hospitality signage under enterprise identity, governance, audit, risk, support, and service requirements.

Adds candidate Vennusign-authored capabilities:

- enterprise administration, access reviews, conditional policy, delegated governance, and audit export;
- organization-wide role templates and controlled administration boundaries;
- advanced retention, compliance, legal-hold, export, and administrative workflow where approved;
- enterprise support controls, service-management views, and coordinated change windows;
- advanced analytics governance and approved data-sharing controls;
- enterprise setup, migration, portfolio onboarding, and operating-governance workflow.

SSO, SCIM, directory synchronization, identity-provider connections, managed hardware, connectivity, monitoring, and premium service levels remain independent add-ons even when commonly purchased with Enterprise.

## Independent add-on catalog

Add-ons may be attached to eligible customer accounts without changing the customer’s primary industry or subtype.

| Add-on family | Candidate scope | Tier relationship |
| --- | --- | --- |
| Property and lodging systems | PMS, CRS, room status, housekeeping, guest-service, loyalty, package, reservation-derived public data | Independent; may require suitable administration but not a forced higher tier for manual fallback |
| Events and spaces | sales, conference, room-booking, registration, ticketing, event scheduling | Independent |
| Transport and local operations | shuttle, valet, parking, access, package, queue, spa, gaming, restaurant, local services | Independent |
| Maps and positioning | third-party maps, indoor positioning, sensors, live routing | Independent; advanced Vennusign map workflow may be tiered separately |
| Translation and language services | machine translation, language provider, managed localization service | Independent; localization workflow may be tiered separately |
| AI services | drafting, summarization, classification, recommendations, language assistance | Independent consumption-backed add-on; bounded workflow may vary by tier |
| External analytics data | occupancy, transaction, footfall, event, guest, or operational sources | Independent; Vennusign analytics workflow may be tiered separately |
| Enterprise identity | SSO, SCIM, directory, identity-provider connection | Independent; enterprise administration may be tiered separately |
| Managed hardware and service | players, displays, installation, replacement, warranty, monitoring, connectivity, support levels | Independent |

An add-on must state its source, authority, data scope, privacy, freshness, fallback, limits, metering, failure, cancellation, retention, export, and deletion behavior.

## Limits remain separate

Candidate limit families include:

- organizations, brands, property groups, properties, buildings, accommodations, venues, outlets, amenities, services, events, meeting spaces, routes, and destinations;
- screens, players, devices, users, roles, approvers, audiences, languages, integrations, sources, identity providers, and managed-service sites;
- templates, assets, campaigns, schedules, reports, dashboards, exports, variants, and automation rules;
- retained history, storage, transactions, messages, requests, characters, words, tokens, data volume, refresh frequency, support hours, incidents, replacements, and spend.

RWP-00.58 does not choose numeric values. A limit must not hide a capability, change authority, imply source access, or become a feature flag.

## Property-group inheritance model

### Inheritance candidates

A parent organization, brand, region, or property group may offer inherited:

- approved templates, components, design libraries, terminology, language variants, campaigns, schedules, destinations, integration configurations, reporting definitions, and governance policies;
- tier entitlements and purchased add-ons where the commercial contract explicitly scopes them;
- default permissions and role templates without silently assigning users;
- limit allocations where the commercial model supports pooled or property-level allowances.

### Local-control requirements

Every property must be able to identify:

- the source and version of an inherited item;
- whether it is mandatory, recommended, copied, linked, or locally overridden;
- the local names, hours, routes, events, languages, audiences, targets, and source values affected;
- what an update changes before adoption;
- what remains local and what will be restored on rollback;
- mixed states and excluded properties.

Current operational truth, urgent local notices, screen recovery, privacy, authority, and last-known-good content cannot be silently replaced by group inheritance.

## Upgrade behavior

An upgrade should:

- preserve all current content, screens, users, roles, targets, sources, languages, schedules, delivery history, and recovery points;
- clearly identify newly available customer outcomes rather than exposing a feature dump;
- avoid changing active content or target scope automatically;
- support guided setup, preview, sample data, and later completion;
- keep independent add-ons separately selected and priced;
- explain any new limits, pooled allowances, or governance scope.

Industry or subtype change is not an upgrade and must not trigger commercial changes.

## Downgrade behavior and unresolved owner questions

A downgrade must never immediately stop essential guest communication. Before final packaging, the owner must decide:

1. What grace period applies to advanced workflows, campaigns, approvals, portfolio scope, and enterprise administration?
2. Are advanced configurations frozen read-only, converted to simpler core objects, exported, or removed after the grace period?
3. How are active campaigns, recurring schedules, approval chains, inherited libraries, and cross-property targets reduced safely?
4. Which property retains ownership of copied or inherited content?
5. How are pooled limits redistributed across properties?
6. What happens to history, audit, reports, dashboards, translations, and analytics retention?
7. Can customers retain advanced content rendering while losing editing workflow, and for how long?
8. How are independent add-ons handled when their companion workflow tier is downgraded?
9. What notifications, previews, impact reports, and administrator acknowledgments are required?
10. How are active public screens protected from sudden blank, stale, private, or mis-targeted content?

Recommended default: preserve data and current delivery, stop creation of new advanced objects after a clearly communicated grace period, provide export or conversion, and require explicit review before destructive cleanup.

## Owner decisions required before approval

- final tier names and number of tiers;
- which advanced capabilities belong in Coordinate, Portfolio, or Enterprise;
- whether any advanced wayfinding, localization, analytics, or campaign function spans multiple tiers;
- numeric limits, pooling, overage, trial, grandfathering, and contract treatment;
- add-on prerequisites and whether any add-on requires a minimum tier for administration only;
- property-group inheritance and override policy;
- downgrade grace, read-only, conversion, export, retention, and deletion behavior;
- enterprise identity and managed-service commercial structure;
- customer-facing pricing timing and upgrade presentation;
- whether pricing remains hidden until a customer has successfully activated at least one screen, consistent with the accepted onboarding direction.

## Impeccable planning guidance

Future packaging surfaces should present customer outcomes first:

- **Operate:** communicate accurately every day;
- **Coordinate:** coordinate teams, events, languages, and content;
- **Portfolio:** govern multiple properties and brands;
- **Enterprise:** meet enterprise identity, audit, governance, and service needs.

They must show what is included, what remains core, which add-ons are separate, which limits apply, what permission is missing, and what manual fallback remains. Avoid disabled-control grids. Use comparison progressively, keep pricing and upgrade prompts contextual, and preserve the approved Sky Blue administrative direction, keyboard access, assistive technology, 200% zoom, localization expansion, right-to-left readiness, long-name handling, and phone through large desktop.

## Boundaries

This proposal is not final commercial approval. RWP-00.59 owns the Hospitality onboarding experience and must not force tier choice before the customer understands the property setup and has a viable path to a first active screen.