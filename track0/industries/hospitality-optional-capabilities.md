# Hospitality Optional Capabilities

## Authority and scope

This document defines optional Hospitality capabilities for RWP-00.56. It inherits the approved industry, subtype, terminology, operating-characteristics, and required-capability records.

Optional capabilities may improve scale, automation, coordination, personalization, governance, insight, or managed service. They must not remove, hide, or commercially reclassify the required manual core established by RWP-00.55.

This is documentation and planning only. No product behavior, external integration, billing, entitlement, privacy, identity, analytics, AI, hardware, or operational system is implemented.

## Packaging rules

1. A **tier entitlement candidate** is product capability delivered by Vennusign that may be bundled at a higher subscription level.
2. An **independent add-on candidate** depends on a separately purchased connection, managed service, hardware service, or consumption-backed capability that should remain independently selectable.
3. A **usage or quantity limit** controls volume and never grants capability or authority.
4. A **permission** controls who may act and never determines commercial access.
5. A represented value remains **product/domain state** even when an optional capability creates or updates it.
6. An **internal rollout flag** controls safe delivery and is never presented as customer packaging.
7. Every optional feature must preserve manual fallback, source authority, privacy, scope, delivery confidence, correction, and restoration.

## Optional capability catalog

### 1. Property-management and lodging-system synchronization

**Primary classification:** independent add-on candidate.

Potential sources include property-management, central-reservation, room-status, housekeeping, guest-service, loyalty, package, and related lodging systems.

Candidate outcomes:

- synchronize approved property, building, room-range, service, amenity, and operational data;
- import confirmed public arrival, departure, transport, service, or property notices;
- support property-specific source authority, freshness, conflict, override, and disconnect behavior;
- reduce repetitive manual entry while retaining manual correction and fallback.

Boundaries:

- guest identity, reservation, room assignment, payment, loyalty, access, service request, stay, and itinerary data are restricted by default;
- public screens receive only explicitly approved, privacy-safe fields and audience scope;
- imported data does not silently overwrite newer approved manual content;
- a disconnected or stale source must not appear current;
- property-management access does not grant Vennusign permissions or commercial features.

Possible limits: connected properties, source systems, records, polling frequency, event volume, retained history, or consumption.

### 2. Event, sales, conference, and room-booking synchronization

**Primary classification:** independent add-on candidate.

Candidate outcomes:

- import approved events, groups, sessions, rooms, registration points, schedules, and public display names;
- reconcile planned and confirmed room assignments;
- update delays, cancellations, relocations, and directory content;
- preserve planner, host, property, sponsor, outlet, and operator authority.

Private attendee data, internal notes, pricing, contracts, security details, and restricted group information remain excluded unless a later authorized use case explicitly requires them.

Possible limits: event sources, spaces, events, sessions, transactions, polling frequency, retained history, or consumption.

### 3. Transport, parking, access, guest-service, and local-system synchronization

**Primary classification:** independent add-on candidate.

Candidate sources include shuttle, transfer, parking, valet, access-control, queue, guest-service, package, ticketing, attraction, spa, restaurant, gaming, and local operational systems.

Candidate outcomes may include confirmed schedules, pickup points, service state, route changes, closures, and approved public guidance.

Live location, eligibility, access rights, order state, reservation state, queue position, wait prediction, or personalized service status requires authoritative data, privacy review, audience control, and safe failure behavior.

Possible limits: connections, vehicles, routes, service points, events, transactions, messages, or consumption.

### 4. Advanced wayfinding, mapping, and positioning

**Primary classification:** tier-entitlement candidate for Vennusign-authored advanced wayfinding; independent add-on candidate when maps, positioning, sensors, or third-party routing are required.

Candidate capabilities:

- managed property maps and destination libraries;
- route variants by entrance, building, floor, elevator, accessibility requirement, event, closure, or temporary condition;
- interactive directories and searchable destinations;
- map-based preview and route validation;
- indoor positioning, QR handoff, mobile continuation, or kiosk navigation;
- live route adaptation from authoritative building or event sources.

Manual text and static route-change communication remains core. The product must not invent current position, distance, travel time, accessible route, elevator state, or safe route.

Possible limits: properties, floors, maps, destinations, routes, kiosks, positioning sources, scans, sessions, or retained history.

### 5. Guest personalization and audience-aware experiences

**Primary classification:** tier-entitlement candidate for Vennusign personalization workflows; independent add-on candidate when external guest, reservation, loyalty, access, or identity data is required.

Candidate capabilities:

- audience-segment content for approved groups or programs;
- event or group-specific displays;
- language and accessibility preferences;
- mobile handoff or personalized itinerary experiences;
- approved welcome, service, or recommendation content.

Personalization is opt-in, purpose-bound, minimal, time-limited, and privacy-safe. Shared public screens must not display a named guest, room assignment, reservation, payment, loyalty, access, request, or itinerary by default. Manual general information remains core.

Possible limits: segments, campaigns, rules, data sources, personalized sessions, messages, profiles, or consumption.

### 6. Brand libraries and multi-property design governance

**Primary classification:** tier-entitlement candidate.

Candidate capabilities:

- organization and brand libraries for approved templates, typography, color, components, logos, media, terminology, and screen-purpose patterns;
- inheritance from organization or brand to property with explicit local overrides;
- versioning, preview, staged adoption, deprecation, migration, and restoration;
- safe copying across properties with local review of names, hours, routes, languages, audiences, and targets;
- mixed-brand and franchise boundaries.

Brand membership does not imply ownership, management, permission, commercial access, or source authority. Basic customer-authored content and local terminology remain core.

Possible limits: brands, libraries, templates, assets, variants, properties, storage, versions, or history.

### 7. Property-group coordination and centralized operations

**Primary classification:** tier-entitlement candidate.

Candidate capabilities:

- cross-property exception views and operating-center dashboards;
- centralized content preparation with local approval or local opt-out;
- shared notices, campaigns, event programs, language variants, and recovery playbooks;
- property groups, regions, brands, operating companies, and managed portfolios;
- safe bulk actions with explicit selected and excluded targets;
- role-aware delegation and local override protection.

Current property state, local manual editing, delivery confidence, and recovery remain core. Group membership does not silently grant authority or expand targets.

Possible limits: properties, groups, regions, users, roles, bulk targets, campaigns, templates, history, or storage.

### 8. Campaigns, schedules, content orchestration, and approvals

**Primary classification:** tier-entitlement candidate.

Candidate capabilities:

- reusable campaign plans and multi-screen sequences;
- recurring schedules, date windows, blackout periods, and conflict detection;
- content calendars across properties, venues, events, and languages;
- approval chains, separation of duties, escalation, delegation, and expiration;
- legal, brand, sponsor, host, or local-property review states;
- controlled supersession and restoration.

Immediate manual guest notices, basic scheduling, explicit targeting, publication, confirmation, correction, and restoration remain core. Approval status is product state; who may approve is permission; commercial access is entitlement.

Possible limits: campaigns, schedules, approvers, rules, properties, targets, templates, variants, retained history, or storage.

### 9. Advanced localization and translation workflow

**Primary classification:** tier-entitlement candidate for workflow and terminology management; independent add-on candidate for automated translation, language service providers, or AI consumption.

Candidate capabilities:

- translation requests, assignments, review, approval, and publication;
- terminology libraries, translation memory, reusable language variants, and quality status;
- per-property and per-brand language rules;
- missing, stale, source-changed, and untranslated visibility;
- automated translation suggestions with human review;
- large language portfolios and managed localization service.

Manual alternate-language content remains core. Generated or imported translations remain reviewable product state and cannot silently replace approved content.

Possible limits: languages, characters, words, translation units, requests, reviewers, providers, models, or consumption.

### 10. AI-assisted content and operations

**Primary classification:** independent add-on candidate when usage is consumption-backed; some bounded workflow may also be a tier candidate.

Candidate capabilities:

- draft guest notices, summaries, alternate wording, or language variants;
- suggest concise, accessible, privacy-safe copy;
- identify missing target, audience, source, timing, or recovery detail;
- summarize operational exceptions or handoff items;
- recommend templates, destinations, or screen-purpose layouts;
- assist classification and duplicate detection.

AI never publishes, changes state, selects high-impact targets, exposes private data, asserts unsupported facts, or replaces human approval by default. Inputs, outputs, retention, model/provider, source grounding, confidence, review, audit, and cost must be visible where relevant.

Possible limits: requests, tokens, characters, models, users, properties, generated variants, retained history, or spend.

### 11. Analytics, reporting, and optimization

**Primary classification:** tier-entitlement candidate for advanced Vennusign analytics; independent add-on candidate when external occupancy, event, footfall, transaction, guest, or operational data is required.

Candidate capabilities:

- screen online, delivery, outdated, failure, notice, schedule, language, and recovery trends;
- content and campaign performance;
- amenity, outlet, event, meeting-space, wayfinding, and property-group analysis;
- comparison across properties, brands, subtypes, periods, and operating events;
- export, scheduled reports, dashboards, anomaly detection, and recommendations;
- data quality, source coverage, and confidence reporting.

Core delivery health and current failure visibility remain available without premium analytics. Analytics must not imply guest behavior, occupancy, attribution, revenue, conversion, or operational causation without authoritative data.

Possible limits: properties, dashboards, reports, exports, users, metrics, retained history, data volume, or refresh frequency.

### 12. Enterprise identity, access, and administration

**Primary classification:** tier-entitlement candidate for enterprise administration; independent add-on candidate for external identity provider connections.

Candidate capabilities:

- SSO, directory synchronization, SCIM, domain control, and identity-provider integration;
- role templates, property/group assignment, delegated administration, access reviews, and lifecycle automation;
- conditional access, session policy, audit export, and enterprise support controls;
- organization, brand, property, object, and restricted-audience administration.

Basic permissions remain required core. Identity integration does not create commercial capabilities, object authority, source authority, or public audience eligibility.

Possible limits: identity providers, domains, users, groups, roles, assignments, audit history, or API consumption.

### 13. Managed hardware, connectivity, monitoring, and support

**Primary classification:** independent add-on candidate.

Candidate capabilities:

- managed players, displays, mounts, peripherals, spares, installation, replacement, and lifecycle service;
- managed connectivity, cellular backup, network monitoring, remote diagnostics, and proactive support;
- device fleet policy, health, warranty, service level, and dispatch coordination;
- certified device profiles and managed update windows.

Basic pairing, current online/offline state, delivery confirmation, outdated awareness, and recovery remain core. Managed service does not replace customer ownership, local access, safety responsibility, or ordinary product permissions.

Possible limits: devices, sites, data usage, support hours, incidents, replacements, monitoring history, or service level.

## Optional-capability dependencies

Every optional capability must define before implementation:

- customer outcome and manual fallback;
- primary Track 0 classification;
- exact entitlement or add-on boundary;
- permissions and object scope;
- source authority, freshness, conflict, override, and disconnect behavior;
- privacy, audience, consent, retention, export, deletion, and audit requirements;
- limits, metering, overage, downgrade, cancellation, and data-preservation behavior;
- delivery, partial-failure, retry, correction, supersession, and restoration;
- accessibility, localization, phone/desktop, long-name, and time-zone behavior;
- security, compliance, vendor, cost, and operational ownership;
- rollout and compatibility controls kept internal.

## Impeccable planning guidance

Optional-capability surfaces remain **Operate** experiences. They must not turn advanced packaging into a wall of disabled controls.

Presentation should:

- lead with the customer outcome and current operating need;
- keep included core manual actions visible and usable;
- distinguish unavailable entitlement, missing permission, disconnected source, exceeded limit, unsupported object, and rollout state;
- explain why an advanced capability helps, what data or connection it requires, its scope, and its fallback;
- show setup progress, source health, privacy and audience boundaries, limits, consumption, failures, and recovery;
- provide safe trial, preview, connection, approval, cancellation, downgrade, export, and deletion states where applicable;
- preserve keyboard access, assistive technology, 200% zoom, non-color status, localization expansion, right-to-left readiness, and the approved Sky Blue administrative direction.

## Classification summary

- **Tier candidates:** advanced wayfinding, brand libraries, property-group coordination, campaigns, approvals, advanced localization workflow, advanced analytics, enterprise administration, and selected advanced operational workflows.
- **Add-on candidates:** external property, event, room, transport, guest-service, access, gaming, mapping, positioning, emergency, weather, translation, AI, identity-provider, managed hardware, connectivity, monitoring, and related connections or services.
- **Limits:** properties, groups, buildings, accommodations, venues, outlets, amenities, services, events, meeting spaces, screens, devices, users, roles, languages, integrations, sources, templates, assets, campaigns, reports, history, storage, transactions, messages, requests, tokens, data, or spend.
- **Permissions:** view, edit, approve, publish, restore, administer, connect, override, export, manage identity, manage billing, or perform high-scope actions.
- **State:** configuration, connection, source, freshness, conflict, content, audience, target, approval, schedule, delivery, consumption, and recovery values.
- **Rollout flags:** experiments, migrations, staged availability, compatibility controls, and emergency-disable mechanisms remain internal.

## Boundaries

The required RWP-00.55 manual baseline remains core. No optional capability is approved for implementation or packaging by this document. RWP-00.57 owns the consolidated primary classification of all Hospitality concerns.