# Bar, Brewery & Nightlife Optional Capabilities

## Authority and scope

This document defines optional Bar, Brewery & Nightlife capability candidates for RWP-00.20. It preserves every required manual capability in RWP-00.19 and separates advanced Vennusign workflow from externally supplied or consumption-backed services. It proposes classifications for later owner review; it does not approve commercial packaging or implement behavior.

## Optional-capability rule

A capability may be optional only when a venue can still perform the essential daily job manually and safely without it. Optional capability, permission, represented state, quantity limit, add-on connection, and rollout control remain separate.

## Tier-bundled capability candidates

### Advanced schedules, dayparts, and recurring changes

Candidate value includes reusable weekly schedules, recurring happy hours, daypart-specific lists, cross-midnight templates, event-linked changes, conflict detection, schedule preview, exception calendars, and bulk correction.

- **Primary classification:** tier entitlement candidate.
- **Customer value:** reduces repetitive editing and missed transitions.
- **Dependencies:** required manual hours, effective periods, targeting, publishing, confirmation, and recovery.
- **Cost drivers:** scheduling complexity, retained history, conflict handling, and support.
- **Manual fallback:** operators can continue publishing current content and one-off changes.

### Advanced event and entertainment workflow

Candidate value includes recurring event series, lineup and set management, reusable event templates, sports fixtures, viewing-zone assignments, approvals, coordinated event/offer changes, and event-specific screen sets.

- **Primary classification:** tier entitlement candidate.
- **Customer value:** coordinates high-change event operations.
- **Dependencies:** core manual events, areas, entry guidance, screen targeting, delivery confidence, and restoration.
- **Manual fallback:** one-off event content and schedule changes remain core.

### Campaigns and advanced presentation

Candidate value includes campaign calendars, multi-content rotations, advanced transitions, richer motion controls, video playlists, synchronized multi-screen presentation, menu/event layout libraries, and video-wall orchestration.

- **Primary classification:** tier entitlement candidate.
- **Customer value:** improves branded promotional presentation at scale.
- **Dependencies:** basic layouts/themes, accessible display rules, explicit targets, preview, publishing, and recovery.
- **Cost drivers:** media processing, storage, bandwidth, playback coordination, and support.
- **Manual fallback:** basic static and ordinary rotating content remains available.

### Multi-venue libraries and brand governance

Candidate value includes reusable content blocks, shared drink/event libraries, approved brand assets, organization templates, controlled local copies, regional defaults, inheritance previews, local overrides, and safe bulk distribution.

- **Primary classification:** tier entitlement candidate.
- **Customer value:** improves consistency without removing local operational control.
- **Dependencies:** organization/venue scope, authored-content preservation, permissions, mixed-state preview, and restoration.
- **Manual fallback:** each venue can manage its own content independently.

### Approval, audit, and coordinated operations

Candidate value includes approval chains, separation of author/publisher roles, controlled responsible-content review, scheduled approvals, acknowledgments, assignment, escalation, advanced history, comparison, and exportable audit records.

- **Primary classification:** tier entitlement candidate.
- **Customer value:** supports larger teams and controlled operations.
- **Dependencies:** core permissions, draft/published distinction, content versions, delivery state, and restoration.
- **Manual fallback:** authorized users can edit and publish directly under ordinary permissions.

### Advanced organization dashboards and analytics

Candidate value includes cross-venue exceptions, comparative performance, campaign and event analysis, content engagement, schedule adherence, delivery trends, freshness, advanced exports, saved reports, and longer retention.

- **Primary classification:** tier entitlement candidate.
- **Customer value:** supports optimization and portfolio oversight.
- **Dependencies:** stable dimensions, data-quality indicators, permissions, retention policy, and export controls.
- **Manual fallback:** core screen health, publish status, and current-state visibility remain available.

## Independent add-on candidates

### POS, inventory, and tap-management synchronization

Candidate value includes item/price availability, stock signals, keg/tap state, serving formats, production or purchasing context, and automated source updates.

- **Primary classification:** independent add-on candidate.
- **Why separable:** requires external systems, connection setup, source mapping, monitoring, and support.
- **Dependencies:** source authority, freshness, override rules, conflict handling, manual fallback, disconnect behavior, and safe recovery.
- **Limits:** connections, venues, items, taps, transactions, polling, and retained data.

### Reservation, guest-list, ticketing, payment, identity, and access connections

Candidate value includes approved general availability or entry information from authoritative sources and, where later authorized, transaction-aware operational context.

- **Primary classification:** independent add-on candidate.
- **Why separable:** external commercial systems, privacy, identity, payment, and access risk are independently valuable and costly.
- **Boundaries:** public signage must not expose personal identity or infer eligibility, payment, reservation, ticket, or guest-list state without explicit authorization and audience controls.
- **Limits:** connections, venues, events, reservations, tickets, transactions, requests, and retention.

### Sports, event, lineup, and venue-data connections

Candidate value includes fixtures, scores or event states where licensed, performer/lineup data, venue calendars, and schedule changes.

- **Primary classification:** independent add-on candidate.
- **Dependencies:** rights and source authority, local-time mapping, freshness, cancellation/delay handling, approval, manual override, and safe fallback.
- **Limits:** feeds, leagues, events, requests, venues, and retained history.

### AI and assisted content services

Candidate value includes copy suggestions, menu/event descriptions, image assistance, translation assistance, scheduling suggestions, anomaly detection, and analytics explanation.

- **Primary classification:** independent add-on candidate when externally metered or separately operated; selected included assistance may later be tier bundled.
- **Dependencies:** reviewable generated state, source disclosure, permissions, safe prompts, privacy controls, quality validation, and manual editing.
- **Limits:** requests, tokens, images, languages, data, and spend.

### Managed hardware, connectivity, monitoring, and support

Candidate value includes managed players/screens, cellular or network service, remote monitoring, proactive alerting, replacement, installation, and enhanced support.

- **Primary classification:** independent add-on candidate.
- **Why separable:** physical goods, network consumption, field service, monitoring, replacement, and support create distinct cost and value.
- **Manual fallback:** ordinary pairing, online/offline state, outdated awareness, publishing, and restoration remain core.

### Custom integrations and data services

Candidate value includes customer-specific systems, data transformations, branded feeds, compliance content sources, bespoke exports, and managed connectors.

- **Primary classification:** independent add-on candidate.
- **Dependencies:** contract, source authority, privacy/security review, mapping, support ownership, monitoring, failure behavior, and termination plan.

## Optional product/domain state and permissions

Optional workflows may represent recurrence, campaign, approval, source, freshness, conflict, schedule, feed, analytics, monitor, device, and service state. These values remain product/domain state, not capabilities.

Permissions may govern who creates schedules, campaigns, templates, approvals, integrations, reports, exports, AI requests, managed-device changes, and organization-wide actions. Permission never grants commercial access.

## Usage and quantity limits

Limits must not be modeled as feature flags. Candidate dimensions include venues, areas, screens, devices, users, roles, approvers, lists, items, taps, events, schedules, campaigns, templates, assets, media duration, storage, bandwidth, history, reports, exports, integrations, connections, transactions, requests, tokens, images, languages, monitoring endpoints, data, support incidents, and spend.

Reaching a limit must preserve data and manual core access, explain the measured dimension, identify scope and current usage, show allowed next actions, and distinguish limit reached from permission denied, not purchased, disconnected, stale, unsupported, or internally disabled.

## Failure, downgrade, and cancellation requirements

Before any optional capability is implemented, planning must define:

- manual fallback and last-known-good behavior;
- source authority, freshness, override, and conflict rules;
- privacy, rights, audience, and permission boundaries;
- connection and consumption limits;
- failure, partial failure, disconnection, cancellation, downgrade, and data-retention behavior;
- correction, retry, supersession, restoration, and export where applicable;
- clear distinction among not purchased, not permitted, not configured, disconnected, stale, limit reached, unsupported, and rollout-disabled states.

## Impeccable planning implications

Future optional-capability surfaces must keep the included manual path visible, explain value in operator outcomes, avoid surprise paywalls during urgent tasks, separate permissions from commercial access, provide configuration and failure recovery, and support first-use/empty/loading/permission/not-purchased/not-configured/disconnected/stale/limit/validation/partial/success/undo states. Preserve phone and desktop usability, keyboard and assistive technology, 200% zoom, localization expansion, non-color-only status, restrained motion, and the approved Sky Blue direction.

## Boundaries and handoff

Documentation only. No pricing, tier approval, billing, live gates, UI, API, schema, migration, external connection, AI service, hardware, or analytics implementation.

RWP-00.21 owns the consolidated classification of required and optional Bar concerns.