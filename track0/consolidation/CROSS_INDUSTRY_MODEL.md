# Cross-Industry Capability Model

## Status

RWP-00.75 normalizes the Restaurant baseline and the five completed native-industry profiles into one planning model. It does not approve pricing, implement product behavior, resume RWP-13.06, or start Phase 14+.

## Normalization result

The six profiles are compatible after inheritance is applied. Restaurant remains the baseline. Bar, Café, Food Truck, Hospitality, and Entertainment contribute only bounded differences in terminology, represented objects and states, operating rhythm, recommended screen purposes, starter content, dashboard emphasis, and external-system candidates.

Every concern has exactly one primary classification:

1. **Core capability** — essential manual operation, targeting, publication, delivery confidence, correction, recovery, and truthful operational communication.
2. **Permission** — who may view, create, edit, approve, publish, override, restore, administer, or access sensitive information.
3. **Product/domain state** — customer configuration and represented facts such as industry, subtype, hierarchy, availability, schedules, source, freshness, target, delivery, content, and operating state.
4. **Tier entitlement candidate** — advanced native Vennusign workflow, coordination, governance, presentation, localization, analytics, or portfolio administration.
5. **Independent add-on candidate** — an external system, managed service, hardware/service contract, metered AI/translation service, or other separately attachable capability.
6. **Usage or quantity limit** — a count, volume, frequency, storage, retention, export, connection, transaction, support, or consumption allowance.
7. **Internal rollout flag** — a temporary release-control mechanism that is not customer configuration, permission, commercial access, or product state.

A concern may relate to several other classifications, but it receives only one primary classification. For example, POS synchronization is an add-on candidate; its connection count is a limit; its configuration is product state; its administrators require permission; and a rollout flag may control staged release.

## Normalized core

The following capabilities are universally core across supported native industries:

- create and maintain customer-authored public content and local terminology;
- represent ordinary venue, property, operation, area, service, program, event, menu, attraction, schedule, availability, operating, and notice information appropriate to the selected industry;
- perform rapid manual updates for sold-out, unavailable, limited, delayed, paused, closed, canceled, relocated, changed-hours, next-batch, reopening, and comparable bounded states;
- identify and select the intended venue, context, object, audience, screen, and delivery target;
- pair or select a screen, preview the intended result, publish explicitly, and show per-target delivery confidence without claiming unsupported acknowledgement;
- show source identity, freshness, stale/disconnected state, local override, conflict, and last-known-good context when a source exists;
- correct, supersede, expire, unpublish, retry, undo, and restore safely;
- preserve usable manual fallback when an integration, automation, source, or paid add-on is unavailable;
- expose loading, first-use, empty, validation, permission, partial-delivery, failure, success, offline, outdated, conflict, and recovery states;
- provide accessible, responsive, keyboard-operable, assistive-technology-compatible, zoom-safe, localization-ready presentation;
- keep the first-screen path focused on one verified useful outcome before forced pricing, integrations, or complete organization modeling.

Core does not mean every object or unlimited usage is free of limits. It means an allowed organization can perform the essential operation manually and safely within its applicable allowances.

## Normalized product/domain state

Industry and subtype are non-commercial configuration. They change terminology, defaults, starter recommendations, screen-purpose suggestions, dashboard emphasis, analytics labels, and guidance. They do not grant a feature, permission, add-on, quantity, or rollout access.

The following are product/domain state when represented:

- organization, venue/property/operation, hierarchy, subtype, descriptive traits, local terminology, and customer-authored names;
- menu/product, amenity/service, attraction/exhibit/program, event/session, area/destination, route/gate, screen, and source objects;
- schedule, service period, batch, freshness, expected return, preorder/pickup context, queue/wait/capacity/admission context, availability, closure, relocation, delay, cancellation, reopening, and unknown values;
- target, audience, content version, publication, delivery, source, freshness, conflict, override, recovery, and restoration values;
- integration configuration and connection health after an add-on is commercially available.

Unavailable product state must not be presented as a premium lock. A sold-out item, closed amenity, disconnected source, stale value, permission denial, unsupported context, exceeded limit, unconfigured add-on, and rollout restriction require distinct explanations and actions.

## Normalized permissions

Permissions govern authority, not commercial access. The normalized authority model must support object and scope boundaries for:

- organization, venue/property/operation, area, service point, screen, content, event/session, source, integration, billing, and support administration;
- view, create, edit, approve, publish, unpublish, override, restore, delete, export, manage users, manage billing, manage integrations, and access sensitive data;
- organization-wide versus local authority and mixed-industry organizations;
- high-scope, urgent, privacy-sensitive, rights-sensitive, guest-specific, child-related, biometric/camera, alcohol/gambling, safety, sponsor, and regulated content.

A user with commercial access but without authority remains permission-restricted. A user with authority cannot use a capability the organization has not purchased. Neither condition changes represented product state.

## Normalized tier candidates

Advanced native Vennusign outcomes may be bundled into tier archetypes later. Candidate families are:

- planning, recurring schedules, rotations, automation rules, and conflict detection;
- campaigns, promotions, templates, reusable content, advanced presentation, and multi-screen coordination;
- workflow, approval, assignment, acknowledgment, escalation, audit, and retained history;
- advanced localization, terminology libraries, brand governance, and quality review;
- multi-venue/property/operation coordination, inheritance, safe bulk actions, portfolio dashboards, and enterprise administration;
- advanced native analytics, comparisons, alerts, scheduled reporting, and optimization using Vennusign-owned evidence.

The complete normalized core remains available at the base operating tier. Tier access cannot be inferred from industry or subtype.

## Normalized add-on candidates

External or separately delivered capabilities remain independently attachable candidates, including:

- POS, inventory, production, ordering, payment, fulfillment, pickup, tap-management, loyalty, supplier, and CRM systems;
- property-management, room-booking, event/conference, transport, parking, guest-service, access, gaming, and local property systems;
- ticketing, admissions, access control, queue/occupancy/footfall, venue/show control, cinema, collection, attraction, event, sports, mapping, and mobility systems;
- weather, traffic, government, safety, calendar, directory, messaging, translation, identity-provider, data/BI, and other authoritative external sources;
- AI generation, translation, prediction, assistance, or metered processing where separately sold;
- managed hardware, connectivity, installation, deployment, monitoring, content, localization, analytics, support, and custom integration services;
- HaaS contracts, which remain separate from software subscription entitlements.

Every add-on requires source authority, freshness, privacy, disconnect, safe-failure, manual-fallback, downgrade, retention, correction, and recovery rules. An add-on cannot remove the normalized core.

## Normalized limits

Limits are allowances, not capabilities. Candidate dimensions include venues/properties/operations, screens, users, objects, schedules, events, sources, integrations, campaigns, templates, languages, reports, history, storage, exports, transactions, support, hardware, data, translation, and AI consumption.

Limit enforcement must preserve safe correction, unpublish, export, downgrade review, active-screen protection, and recovery. Exact counting units, pooling, overage, grace, and grandfathering remain owner decisions for RWP-00.79.

## Cross-industry defaults and overrides

- Restaurant is the inherited fallback when a native industry does not define a meaningful difference.
- An organization may have a primary industry for defaults, while each venue/property/operation has one primary subtype plus optional descriptive traits.
- Mixed-industry organizations preserve local terminology and starter behavior. Organization templates may offer industry-neutral content or explicit per-industry variants.
- Organization-level configuration may seed local defaults but must not silently overwrite local content, timezones, schedules, authority, sources, state, delivery history, or active screens.
- Local overrides are explicit, visible, reversible, and scoped. Removing an override reveals the inherited value rather than inventing a new value.
- Changing industry or subtype previews effects and preserves authored content, object identity, screens, history, permissions, entitlements, add-ons, limits, privacy settings, and integrations unless an explicit reviewed migration says otherwise.

## Terminology normalization

Use neutral canonical concepts in shared contracts and analytics, with industry-specific display language at the presentation layer. The canonical terms include organization, venue/context, object, content, schedule, state, source, target, publication, delivery, permission, entitlement, add-on, limit, and rollout.

Industry vocabulary is presentation and product state, not a separate entitlement model. Customer-authored names take precedence unless invalid, unsafe, privacy-sensitive, rights-restricted, or superseded by an authoritative source.

## Impeccable planning guidance

Cross-industry administrative surfaces use an Operate-first hierarchy:

1. current scope and public-impact exceptions;
2. the next safe action;
3. intended versus delivered result;
4. source and freshness;
5. recovery and restoration;
6. secondary planning, governance, analytics, and commercial discovery.

Locked, permission-restricted, unavailable, disconnected, stale, unsupported, limited, and rollout-controlled states must be visually and semantically distinct. Persistent labels, specific verb-object actions, accessible names, safe initial focus, responsive/mobile operation, 200% zoom, reduced motion, long-name/localization expansion, and complete recovery states are required. The approved Sky Blue direction remains the visual planning baseline.

## Resolved conflicts

- **Unavailable is not a feature flag.** Availability and operating condition are product/domain state changed by authorized core operations.
- **Subtype is not a package.** It affects defaults and language only.
- **Permission is not entitlement.** Authority and commercial access are evaluated independently.
- **Integration is not core automation.** Manual operation remains core; external synchronization is an add-on candidate.
- **Counts are not features.** Limits govern allowed quantity or consumption.
- **Locked UI is presentation, not authority.** Server-resolved entitlement and permission remain authoritative.
- **Provider return is not entitlement confirmation.** Webhooks and server state remain authoritative.
- **HaaS is not a software tier.** It remains a separate contract and service path.

## Owner decisions carried forward

RWP-00.75 does not decide final tier names, pricing, trials, contracts, numeric allowances, pooling, overage, grandfathering, add-on prerequisites/providers/regions, override policy, downgrade grace, retention/deletion, regulated-content obligations, player/hardware service commitments, metric definitions, or implementation sequencing.

## Handoff

RWP-00.76 must inventory the existing product’s feature keys, capability checks, permissions, overrides, limits, locked surfaces, rollout/configuration controls, authority, scope, and known consumers, then compare that factual inventory to this normalized model in RWP-00.77.
