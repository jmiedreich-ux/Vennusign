# Bar, Brewery & Nightlife Capability Classification

## Authority and scope

This document consolidates the Bar, Brewery & Nightlife inventory from RWP-00.18 through RWP-00.20 and assigns exactly one primary Track 0 classification to every capability or related concern. Secondary relationships are explanatory only. This RWP does not approve packaging or implement product behavior.

## Classification rules

1. A **core capability** is an operation Vennusign must provide for viable manual daily use.
2. A **permission** controls who may perform or view an operation; it is not commercial access.
3. **Product/domain state** is represented business, content, source, delivery, or workflow data; it is not a feature gate.
4. A **tier entitlement candidate** is an advanced Vennusign-operated capability that may be bundled later.
5. An **independent add-on candidate** has separately valuable external, managed, metered, physical, or custom cost.
6. A **usage or quantity limit** measures allowance or consumption; it does not grant a capability.
7. An **internal rollout flag** controls delivery, experiments, compatibility, or emergency disable behavior; customers do not purchase it.

## Consolidated classification table

| Concern | Primary classification | Secondary relationships and rationale |
| --- | --- | --- |
| Industry selection | Product/domain state | Seeds defaults and recommendations; does not grant commercial access. |
| Primary subtype and descriptive traits | Product/domain state | Tunes terminology and presentation; permission controls configuration. |
| Customer-authored terminology and labels | Product/domain state | Manual terminology configuration is core; authored values remain state. |
| Drink menus, tap lists, cocktail lists, wine lists, food menus | Core capability | Content objects and values are product/domain state. |
| Item/category/price/description/image/label editing | Core capability | Permissions control edit scope; item values remain state. |
| Pour size, serve, package format, flight, tap position | Product/domain state | Manual management is part of core content operation. |
| Available, unavailable, sold out | Product/domain state | Quick Update is core; permission controls mutation. |
| Manual Quick Update | Core capability | Cannot be tier gated or replaced by automation. |
| Venue, kitchen, bar, doors, event, last-entry, last-call hours | Product/domain state | Manual hours and one-off changes are core. |
| Cross-midnight service-period support | Core capability | The periods themselves are state; advanced recurrence may be tiered. |
| Happy hour, special, release, tasting, game-day offer | Product/domain state | Manual creation/publication is core; reusable scheduling may be tiered. |
| Effective period, expiration, supersession | Product/domain state | Core publishing and correction operate on these values. |
| Event, lineup, sports fixture, trivia, DJ, live music, tasting event | Product/domain state | Manual event communication is core. |
| Event delay, cancellation, relocation, pause, resumption | Product/domain state | Manual change/publication and recovery are core. |
| Venue area, bar, table, counter, patio, lounge, viewing zone | Product/domain state | Manual area guidance is core; area count may be limited. |
| Table/bar/counter/hybrid service model | Product/domain state | Alters defaults and guidance only. |
| General reservation information | Core capability | Transactional reservation handling is an external add-on candidate. |
| General guest-list, cover, ticket, and entry information | Core capability | Personal eligibility or transaction state requires authorized external capability. |
| Reservation, ticket, guest-list, payment, identity, access transaction | Product/domain state | Only represented after an authorized add-on supplies it; privacy and permissions apply. |
| Locally approved age/access/responsible wording | Core capability | Exact wording is state; permission controls controlled content. |
| Jurisdictional law or policy | Product/domain state | Supplied externally or by the operator; Vennusign does not invent it. |
| Screen-purpose selection | Product/domain state | Screen-purpose recommendations are defaults; screen management is core. |
| Screen pairing and management | Core capability | Screen count is a limit; management authority is permission. |
| Explicit screen targeting | Core capability | Never inferred from tier or subtype. |
| Preview before publication | Core capability | Especially required for high-scope or controlled content. |
| Immediate manual publishing | Core capability | Scheduling is a separate advanced candidate. |
| Publish, delivery, offline, outdated, failed, partial, restored state | Product/domain state | Delivery confirmation and recovery are core. |
| Delivery confirmation and actionable recovery | Core capability | Cannot be replaced by managed monitoring. |
| Correction, retry, supersession, undo, prior-version restoration | Core capability | Retained depth may be limited; advanced audit may be tiered. |
| Basic layout, theme, static content, ordinary rotation | Core capability | Rich presentation is optional. |
| Advanced schedules, dayparts, recurring specials | Tier entitlement candidate | Manual one-off operation remains core. |
| Advanced event series and coordinated event workflow | Tier entitlement candidate | External event feeds remain add-ons. |
| Campaign calendar and orchestration | Tier entitlement candidate | Campaign state is product/domain state. |
| Advanced presentation, video playlists, synchronized displays, video walls | Tier entitlement candidate | Media storage/bandwidth are limits; managed hardware may be an add-on. |
| Shared content libraries and organization templates | Tier entitlement candidate | Local authored content and overrides remain state. |
| Brand governance and controlled distribution | Tier entitlement candidate | Organization-wide authority is permission. |
| Approval chains, acknowledgments, assignment, escalation | Tier entitlement candidate | Individual approval/task values are state; authority is permission. |
| Advanced audit, comparison, and long history | Tier entitlement candidate | History depth/storage are limits. |
| Advanced dashboards, comparative analytics, saved reports | Tier entitlement candidate | Basic operational status remains core. |
| POS synchronization | Independent add-on candidate | Imported values are state; manual operation remains core. |
| Inventory and tap-management synchronization | Independent add-on candidate | Source, freshness, conflict, and disconnect values are state. |
| Reservation-system connection | Independent add-on candidate | Reservation values are state; privacy and permissions apply. |
| Guest-list, ticketing, payment, identity, and access connection | Independent add-on candidate | Transaction values are state; public exposure is restricted. |
| Sports, event, lineup, and venue-data feeds | Independent add-on candidate | Rights, source authority, freshness, and local-time mapping required. |
| AI or metered assisted content services | Independent add-on candidate | Generated content is reviewable product state; requests/tokens are limits. |
| Managed hardware and installation | Independent add-on candidate | Core pairing and ordinary screen management remain available. |
| Managed connectivity, monitoring, replacement, and support | Independent add-on candidate | Online/offline and delivery state remain core product/system state. |
| Custom integrations and data services | Independent add-on candidate | Requires separate mapping, monitoring, ownership, and termination plan. |
| View/edit/approve/publish/restore authority | Permission | Never used to determine purchase or limit. |
| Controlled-wording authority | Permission | Jurisdictional wording remains state. |
| Venue, area, organization, report, integration, and managed-device authority | Permission | Scope is separate from entitlement. |
| Venue, area, screen, device, user, role, approver counts | Usage or quantity limit | Does not grant functionality. |
| Item, tap, list, event, schedule, campaign, template, asset counts | Usage or quantity limit | Reaching a limit preserves core data and safe operation. |
| Media, storage, bandwidth, history, report, export limits | Usage or quantity limit | Separate from advanced capability access. |
| Integration, connection, transaction, request, token, image, language, data, support, spend limits | Usage or quantity limit | Consumption must be visible and explainable. |
| Experiment cohort, staged release, compatibility mode, migration control, emergency disable | Internal rollout flag | Not customer-facing packaging. |

## Duplicate and ambiguous concepts resolved

- **Availability** is a state; **Quick Update** is the core capability that changes it.
- **Happy hour** is a time-bound offer/service label, not a standalone capability.
- **Tap list** is content; **tap position** is state; **tap synchronization** is an add-on candidate.
- **Event** is state/content; manual event communication is core; advanced event workflow is tiered; an external event feed is an add-on.
- **Reservation**, **guest list**, **cover**, and **ticket** are distinct domain concepts. General public guidance is core; transaction-aware integrations are add-ons.
- **Approval** values are state, approval authority is permission, and advanced approval workflow is a tier candidate.
- **Analytics data** is product state; core operational status is included; advanced analysis is a tier candidate; externally sourced data may require an add-on.
- **Managed monitoring** does not replace core offline/outdated and delivery-confidence behavior.
- **Screen count** is a limit; it is never a feature flag or permission.
- **Subtype** affects defaults only and never stacks entitlements.

## Restaurant inheritance

The following remain inherited rather than duplicated as Bar-only capabilities: ordinary content organization, screen pairing/management, explicit targeting, preview, publishing, delivery confirmation, online/offline and outdated awareness, basic layouts/themes, user permissions, restore behavior, and the classification separation itself. Bar-specific documentation adds only beverage volatility, late service periods, event/entry context, responsible wording, and subtype emphasis.

## Owner-review questions carried forward

- Final names and outcome grouping for subscription tiers.
- Which advanced scheduling and event features belong in the same bundle.
- Whether selected low-cost AI assistance is tier bundled or always an add-on.
- Exact allowance dimensions and counting rules.
- Which managed services are offered directly versus through partners.
- Retention and export defaults for history and analytics.

These are packaging decisions, not classification gaps.

## Impeccable planning implications

Future surfaces must distinguish included, not purchased, not permitted, not configured, disconnected, stale, limit reached, unsupported, and rollout-disabled states. Core actions stay visible during optional-capability discovery. Upgrade messaging must not obscure urgent manual paths or confuse permission, state, and limit errors.

## Boundaries and handoff

Documentation only. No live gate, pricing, UI, API, schema, migration, billing, external service, analytics, AI, or hardware implementation.

RWP-00.22 owns the proposed subscription tier mapping using these classifications.