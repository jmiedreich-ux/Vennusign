# Café, Bakery & Dessert Capability Classification

## Purpose

This document assigns every material Café, Bakery & Dessert concern one primary Track 0 classification. It consolidates RWP-00.27 through RWP-00.32 and resolves ambiguity between core capability, permission, product/domain state, tier entitlement, independent add-on, usage or quantity limit, and internal rollout flag.

This is documentation and product planning only. It does not authorize product, UI, API, schema, migration, billing, entitlement, integration, analytics, AI, hardware, or managed-service implementation.

## Classification rules

1. Every concern has exactly one primary classification.
2. Required ordinary manual operation is core.
3. Represented business or system values are product/domain state.
4. Authority is permission, not commercial access.
5. Advanced native Vennusign workflow is a tier candidate.
6. External systems, consumption-backed services, and managed services are independent add-on candidates.
7. Counts, volume, frequency, storage, retention, export, transactions, support, and AI consumption are limits.
8. Temporary internal release, migration, compatibility, and emergency-disable controls are rollout flags.
9. Industry and subtype remain non-commercial product configuration.

## Canonical classification matrix

| Concern | Primary classification | Relationship and boundary |
| --- | --- | --- |
| Industry, primary subtype, neutral fallback, descriptive traits, and terminology preference | Product/domain state | Selects defaults, labels, starter recommendations, screen-purpose suggestions, operating guidance, and analytics presentation; does not grant access. |
| Customer-authored venue, menu, product, category, size, option, batch, pickup, service-period, screen, and content names | Product/domain state | Preserved through profile, package, source, and workflow changes unless intentionally edited by authorized users. |
| Manual terminology configuration and neutral fallback | Core capability | Authorized users can keep local language clear without a higher tier. |
| Venue information, public contact details, hours, changed hours, closure, relocation, and reopening communication | Core capability | Values are state; editing and publishing authority is permission. |
| Business day, timezone, service periods, service contexts, and current operating values | Product/domain state | Recurring scheduling and automated transitions are separate tier candidates. |
| Manual menu, product, category, price, description, image, size, format, temperature, base, flavor, topping, add-in, and customer-authored label management | Core capability | Counts may be limited; advanced templates and governance may be tiered. |
| Available, unavailable, sold out, limited, next batch, available again, preorder closed, pickup paused, and related values | Product/domain state | Authorized core actions mutate state. These values are never feature flags. |
| Manual rapid availability, sell-out, return, batch, freshness-guidance, preorder, pickup, period, closure, and reopening updates | Core capability | Must remain available without POS, inventory, production, ordering, or paid automation. |
| Batch identity, expected return, freshness guidance, source, source freshness, service model, preorder window, and pickup context | Product/domain state | Unknown values remain unknown; public claims require customer-authored or authoritative source data. |
| Public preorder, custom-order, and pickup information | Core capability | Private order, guest, payment, production, and fulfillment data are excluded from public screens by default. |
| Order capture, payment, production tracking, ready state, customer notification, and fulfillment management | Independent add-on candidate | External systems or future separately packaged services; privacy, authorization, and manual fallback are required. |
| Screen pairing, screen purpose, venue/area/service context, and explicit target selection | Core capability | Screen count may be limited; target authority is permission. |
| Screen online, offline, outdated, unknown, intended revision, and latest delivery state | Product/domain state | Online status never proves current intended content. |
| Preview, immediate publication, per-target confirmation, retry, correction, supersession, undo, and restoration | Core capability | Advanced scheduling, approval, and orchestration may be tiered; essential recovery remains core. |
| Publication request, target result, partial success, failure, cancellation, supersession, and restoration point | Product/domain state | Visibility remains core; retained depth may be limited or advanced history tiered. |
| Source identity, mapping, freshness, coverage, conflict, disconnect, override, and manual fallback values | Product/domain state | Integration is an add-on candidate; authority to override is permission. |
| Edit, change-state, target, publish, approve, override, bulk-change, restore, undo, and restricted-detail authority | Permission | Permission neither grants commercial access nor changes represented state. |
| First-use, no-content, no-screen, loading, validation, permission, stale-source, conflict, offline, partial-delivery, failure, concurrent-edit, success, undo, and recovery treatment | Core capability | Complete states are required for every included operation; advanced routing and assignment may be tiered. |
| Basic clear layouts, themes, accessibility, distance readability, phone/desktop use, and customer-authored language variants | Core capability | Advanced presentation and localization workflow may be tiered. |
| Recurring schedules, reusable rotations, planned transitions, conflict detection, exception overlays, and reusable daypart templates | Tier entitlement candidate | Immediate manual current-state operation remains core. |
| Campaigns, recurring promotions, reusable seasonal collections, content variants, and coordinated start/stop/expiration | Tier entitlement candidate | Urgent operational truth remains core and takes precedence. |
| Advanced layouts, brand libraries, reusable components, governed templates, and content adaptation | Tier entitlement candidate | Basic accessible presentation remains core. |
| Multi-screen synchronization, organization templates, local inheritance, safe bulk workflow, and multi-venue coordination | Tier entitlement candidate | Single-venue explicit targeting and per-target confirmation remain core. |
| Configurable approvals, separation of duties, advanced audit, extended history, policy review, and enterprise governance | Tier entitlement candidate | Basic permissions, correction, undo, and restoration remain core. |
| Localization workflow, translation review, terminology libraries, coverage reporting, and multi-language campaign coordination | Tier entitlement candidate | Basic customer-authored language content remains core; external translation service is an add-on. |
| Advanced product, promotion, service-period, content, screen, venue, subtype, freshness, and operational-response analytics | Tier entitlement candidate | Core delivery and current-freshness evidence remains available; external sales or demand data requires add-ons. |
| Loyalty content orchestration and advanced engagement workflow | Tier entitlement candidate | External loyalty, CRM, messaging, identity, and attribution systems are add-ons. |
| POS, inventory, production, ordering, payment, fulfillment, loyalty, CRM, messaging, supplier, calendar, weather, event, traffic, identity, translation, and related system connections | Independent add-on candidate | Each requires authorization, source/freshness, coverage, conflict, privacy, disconnect, manual fallback, and recovery. |
| AI drafting, classification, translation assistance, recommendation, anomaly detection, summarization, and optimization service | Independent add-on candidate | Review, source disclosure, privacy, rejection, and consumption limits are required; AI cannot invent facts. |
| Managed hardware, installation, event deployment, connectivity, monitoring, operational response, managed content, localization, analytics, and premium support | Independent add-on candidate | Customer ownership, export, correction, restoration, and safe exit remain required. |
| Venue, screen, user, role, product, category, menu, option, layout, template, language, schedule, campaign, approval, version, integration, report, and connected-account counts | Usage or quantity limit | A reached limit is distinct from permission, state, entitlement, integration health, unsupported context, or rollout. |
| Publication, API, sync, messaging, translation, transaction, export, storage, support, monitoring, and AI consumption | Usage or quantity limit | Essential public correction and recovery cannot be trapped behind an exhausted optional limit. |
| History and retention duration | Usage or quantity limit | Current correction and a basic recovery point remain core; extended depth may also relate to tiered workflow. |
| Internal staged release, migration cohort, compatibility switch, experiment, emergency disable, and operational kill switch | Internal rollout flag | Never represents sold-out, unavailable, closed, batch, preorder, pickup, permission, tier, or customer-visible limit state. |

## Ambiguities resolved

### Availability versus capability access

`Sold out`, `unavailable`, `limited`, `next batch`, and `pickup paused` are product/domain states. They never mean a capability is commercially unavailable. A locked capability must use explicit entitlement language and may not reuse operational state styling or wording.

### Permission versus tier

A user may have commercial access but lack authority, or have authority for a core action without access to an optional workflow. Interfaces must state which condition applies and preserve the user's current task.

### Integration versus core operation

An external source may automate a value, but the ability to maintain essential public truth manually remains core. Connection state, source state, and public operating state remain distinct.

### Advanced workflow versus limits

Scheduling, campaigns, approvals, orchestration, governance, localization workflow, and analytics are capability candidates. The number of schedules, campaigns, approvers, venues, screens, languages, reports, or retained versions is a separate limit.

### Basic history versus advanced history

A current publication result and a usable correction/restoration point are core. Extended searchable audit history, long retention, comparison, export, governance, and advanced analysis may be tier candidates or limits.

### Translation and AI

Manual authored language is core. Native localization workflow may be tiered. External translation and AI are add-on candidates. Language count and AI consumption are limits. Generated content remains reviewable product state.

### Subtype recommendation versus entitlement

A subtype may alter defaults and recommendations but never decides tier, add-on ownership, permission, or allowance. Subtype changes preserve content, state, history, authority, sources, commercial access, and limits.

## Customer-facing status distinctions

Future product surfaces must distinguish:

- included core capability;
- optional capability not included in the current tier;
- independent add-on not purchased or not connected;
- permission denied;
- setup incomplete;
- source disconnected, stale, conflicting, or unknown;
- usage or quantity limit reached;
- unsupported context;
- temporarily staged or disabled by rollout; and
- represented product state such as sold out, closed, or pickup paused.

Each state needs a clear explanation, available actions, data-preservation behavior, and recovery path.

## Impeccable planning brief

Mode is **Operate**. Classification should appear through clear task behavior rather than exposing internal architecture jargon.

- Core actions remain immediately available.
- Optional capability discovery is contextual and outcome-led.
- Permission, tier, add-on, limit, integration, rollout, and product-state conditions use distinct labels and next actions.
- No locked state may erase work, block essential correction, or impersonate a business state.
- Phone, desktop, keyboard, assistive technology, 200% zoom, localization expansion, long names, non-color status, and complete failure/recovery states are required.

## Matrix update intent

The cross-industry capability matrix must record the Café classifications for manual rapid availability, batch/freshness/preorder/pickup values, required manual operation, optional scheduling and campaigns, external food-service systems, managed services, and limits while preserving all concurrent industry rows.

## Validation

This classification covers every concern in RWP-00.27 through RWP-00.32 and assigns exactly one primary classification. Essential operation remains core; state, permission, tier, add-on, limit, privacy/source relationships, and rollout remain separate. No product behavior or commercial decision is implemented.