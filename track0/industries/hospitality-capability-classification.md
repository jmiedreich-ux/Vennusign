# Hospitality Capability Classification

## Authority and scope

This document is the authoritative Hospitality classification result for RWP-00.57. It consolidates the approved industry, subtype, terminology, operating-characteristics, required-capability, and optional-capability records.

Every concern receives exactly one primary Track 0 classification. Related columns describe packaging, permission, state, or limit relationships but do not replace the primary classification.

This is documentation and planning only. It does not approve subscription packaging or implement product behavior.

## Classification rules

- **Core capability:** essential Vennusign behavior needed for safe, understandable, recoverable daily operation.
- **Permission:** authority to view or perform an action.
- **Product/domain state:** a represented business, content, source, target, delivery, or operating value.
- **Tier entitlement:** advanced Vennusign-authored capability that may be bundled in a subscription tier.
- **Independent add-on:** separately selectable external integration, consumption-backed service, managed service, or hardware service.
- **Usage or quantity limit:** a count, volume, retention, frequency, data, or spend allowance.
- **Internal rollout flag:** experiments, migrations, compatibility controls, staged availability, or emergency disable behavior not sold to customers.

## Consolidated classification matrix

| Hospitality concern | Primary classification | Relationship notes |
| --- | --- | --- |
| Manual property and local-context information | Core capability | Object values are state; edit/publish authority is permission |
| Customer-authored property, venue, amenity, service, outlet, event, destination, and screen names | Product/domain state | Basic manual naming is core; change authority is permission |
| Industry and subtype selection | Product/domain state | Tunes defaults and terminology; never grants commercial access |
| Regular, effective, special, access, and overnight hours | Product/domain state | Manual maintenance and publication are core |
| Open, closed, limited, unavailable, delayed, relocated, restricted, unknown, and related operating values | Product/domain state | Authorized manual change and guest communication are core |
| Guest notices and operational updates | Core capability | Notice text, source, scope, time, audience, priority, and state are product state |
| Manual amenity, service, and outlet communication | Core capability | Availability and hours are product state; outlet-specific behavior inherits local industry |
| Manual meeting, event, directory, room-change, delay, cancellation, and relocation communication | Core capability | Event, session, location, source, time, and status are product state |
| Manual wayfinding and temporary-route communication | Core capability | Destination, route, closure, accessibility, and effective time are product state |
| Basic manually authored language variants | Core capability | Language, coverage, source, freshness, and translation status are product state |
| Explicit screen targeting and preview | Core capability | Target selection and intended content are state; publish authority is permission |
| Save, approve, schedule, publish, and delivery confirmation | Core capability | Each action remains distinct; approval and delivery states are product/system state |
| Offline, outdated, stale-source, disconnected, conflict, and override awareness | Core capability | Screen/source/override values are product/system state |
| Correction, expiry, supersession, retry, undo, and restoration | Core capability | Authority is permission; versions and recovery state are product/system state |
| Shift handoff and current exception visibility | Core capability | Advanced acknowledgment, assignment, escalation, and task routing are tier candidates |
| Privacy-safe public and restricted audiences | Product/domain state | Viewing and publishing authority is permission; privacy policy is not entitlement |
| Property, building, area, venue, object, event, screen, language, and audience scope | Product/domain state | Scope-selection and bulk-action authority are permissions |
| Basic user roles and action authority | Permission | Permissions do not decide commercial access |
| High-impact, urgent, or organization-wide publish authority | Permission | Manual authorized messaging is core; emergency-system connections are add-ons |
| Advanced shift workflow, acknowledgment, assignment, escalation, and task routing | Tier entitlement | Current-state visibility and manual recovery remain core |
| Advanced campaign, content-calendar, recurring schedule, conflict detection, and orchestration workflow | Tier entitlement | Basic scheduling, targeting, publishing, confirmation, and recovery remain core |
| Approval chains, separation of duties, delegation, and enterprise review workflow | Tier entitlement | Who may approve is permission; approval status is state |
| Brand libraries, managed templates, inheritance, migration, and governance | Tier entitlement | Customer-authored content remains core; asset/template counts may be limits |
| Centralized property-group operations and portfolio coordination | Tier entitlement | Local property operation and recovery remain core |
| Advanced Vennusign wayfinding, map authoring, route variants, and searchable directories | Tier entitlement | Manual text/static wayfinding remains core |
| Third-party maps, indoor positioning, sensors, and live routing | Independent add-on | Requires source authority, freshness, privacy, fallback, and connection limits |
| Guest personalization and audience-aware workflow | Tier entitlement | External guest/reservation/loyalty data connection is a separate add-on |
| Property-management, central-reservation, room-status, housekeeping, and lodging-system synchronization | Independent add-on | Manual operation remains core; guest-private fields restricted by default |
| Event, sales, conference, room-booking, registration, or ticketing synchronization | Independent add-on | Manual event directories and changes remain core |
| Transport, parking, valet, access, package, guest-service, spa, gaming, restaurant, and local-system synchronization | Independent add-on | Manual general guidance remains core |
| Advanced localization workflow, terminology libraries, translation memory, and quality review | Tier entitlement | Basic manual alternate-language content remains core |
| Automated translation and external language-service providers | Independent add-on | Generated/imported content remains reviewable state |
| AI-assisted drafting, summarization, classification, and recommendations | Independent add-on | Some UI workflow may be tiered; usage is separately metered; no default auto-publish |
| Advanced analytics, scheduled reports, exports, comparisons, and recommendations | Tier entitlement | Current delivery health and failures remain core |
| External occupancy, transaction, footfall, guest, event, or operational analytics sources | Independent add-on | Cannot imply attribution or causation without authoritative data |
| Enterprise administration, role templates, delegated administration, access reviews, and audit export | Tier entitlement | Basic permissions remain core |
| SSO, directory synchronization, SCIM, and identity-provider connection | Independent add-on | Identity connection does not grant product authority or features |
| Managed players, displays, installation, replacement, warranty, and lifecycle service | Independent add-on | Basic pairing and screen state remain core |
| Managed connectivity, cellular backup, network monitoring, diagnostics, proactive support, and service levels | Independent add-on | Basic online/offline, outdated, confirmation, and recovery remain core |
| Properties, groups, buildings, accommodations, venues, outlets, amenities, services, events, meeting spaces, screens, devices, users, roles, languages, sources, integrations, templates, assets, campaigns, reports, history, storage, transactions, messages, requests, tokens, data, or spend | Usage or quantity limit | Limits never grant capability, permission, authority, source access, or privacy rights |
| Experiments, migrations, staged availability, compatibility controls, emergency disable, and provider failover rollout | Internal rollout flag | Never presented as customer availability or packaging |

## Duplicate and ambiguity resolutions

### Personalization

The Vennusign-authored rules, audience workflow, review, targeting, and presentation layer are a **tier entitlement**. A connection to PMS, reservation, loyalty, identity, access, or guest-profile data is an **independent add-on**. The represented audience and content remain **product state**, and user authority remains **permission**.

### Advanced wayfinding

Vennusign map management, destination libraries, route variants, and interactive directory workflow are **tier entitlements**. External maps, indoor-positioning providers, sensors, and live-routing sources are **independent add-ons**. Destinations, routes, closures, and accessible-route values are **product state**. Manual text and static route communication remain **core**.

### Localization and AI

Translation assignment, review, terminology, and quality workflow are **tier entitlements**. Automated translation, language-provider, and consumption-backed AI services are **independent add-ons**. Language variants and review status are **product state**. Manual language variants remain **core**.

### Analytics

Advanced Vennusign reports, dashboards, exports, comparisons, and recommendations are **tier entitlements**. External data feeds are **independent add-ons**. Current delivery health and failures remain **core product operation**. Retention, rows, exports, refresh, and data volume are **limits**.

### Enterprise identity

Enterprise administration workflow is a **tier entitlement**. SSO, SCIM, directory, and identity-provider connections are **independent add-ons**. User, group, role, session, and connection status are **product/system state**. Authority is always **permission**.

### Managed service

Hardware, installation, connectivity, monitoring, replacement, and service levels are **independent add-ons**. Device count, sites, data, incidents, replacements, and support hours are **limits**. Basic screen pairing, health, delivery confidence, and recovery remain **core**.

## Core protection result

No required RWP-00.55 capability is reclassified as premium. The following remain core:

- manual property and guest information;
- manual hours, states, notices, amenities, services, outlets, events, directories, and wayfinding;
- basic manual language variants;
- explicit targeting and preview;
- publishing and delivery confidence;
- offline, outdated, stale-source, conflict, and override awareness;
- correction, expiry, supersession, retry, undo, and restoration;
- current exception and handoff visibility;
- privacy-safe audiences and ordinary action permissions;
- required first-use, empty, loading, validation, failure, partial-delivery, success, accessibility, responsive, and recovery states.

## Packaging handoff rules

RWP-00.58 may map approved tier-entitlement candidates into subscription tiers, but it must:

- keep all core capabilities available at the base operating tier;
- keep external and managed services independently selectable add-ons;
- treat limits separately from capabilities;
- treat permissions separately from commercial access;
- preserve manual fallback and data access on downgrade or cancellation;
- explain unavailable entitlement, missing permission, disconnected source, exceeded limit, unsupported object, and rollout state distinctly;
- avoid enabling or disabling product-state values through feature flags.

## Impeccable planning guidance

Future classification and packaging surfaces should explain the customer outcome before classification terms. They should use progressive disclosure for commercial, technical, source, privacy, limit, and rollout detail.

Operators must be able to see what remains included, what optional capability is unavailable, what connection is missing, what limit is reached, what permission is absent, and what manual fallback remains. Preserve keyboard access, assistive technology, non-color-only status, 200% zoom, long-name handling, localization expansion, right-to-left readiness, phone through large desktop, and the approved Sky Blue administrative direction.

## Boundaries

Classification does not approve final tiers, prices, add-on prices, limits, trials, grandfathering, contracts, or implementation. RWP-00.58 owns Hospitality Subscription Tier Mapping.