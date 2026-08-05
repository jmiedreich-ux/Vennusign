# Food Truck & Concession Capability Classification

## Purpose

This document consolidates the Food Truck & Concession concerns established by RWP-00.39 through RWP-00.44 and assigns each one primary Track 0 classification:

- core capability;
- permission;
- product/domain state;
- tier entitlement;
- independent add-on;
- usage or quantity limit;
- internal rollout flag.

Each concern receives one primary classification. Related secondary controls are noted only to prevent category drift.

## Classification rules

1. **Core capability** means required product behavior for ordinary operation.
2. **Permission** controls who may act on an object or scope; it never grants commercial access.
3. **Product/domain state** describes what is currently true; it is not a commercial gate.
4. **Tier entitlement** grants optional product behavior bundled by customer outcome.
5. **Independent add-on** represents a separately enabled integration, service, data source, hardware offering, or specialized capability.
6. **Usage or quantity limit** constrains count, volume, frequency, retention, or consumption; it is not a capability.
7. **Internal rollout flag** stages delivery internally and must never appear as customer packaging.

Industry, subtype, physical form, host relationship, source authority, approval status, and terminology preference remain descriptive configuration and are not themselves commercial access controls.

## Canonical classification matrix

| Concern | Primary classification | Clarification |
| --- | --- | --- |
| Create and edit menus, categories, items, combos, prices, descriptions, images, options, and dietary labels | Core capability | Required for every subtype; permissions govern who edits |
| Compact versus broader menu presentation | Product/domain state | Layout/content choice, not access |
| Manual Quick Update | Core capability | Required without integrations |
| Item availability | Product/domain state | Available, unavailable, sold out, or limited |
| Combo availability | Product/domain state | Scoped separately from item and operation state |
| Category or menu availability | Product/domain state | Must not silently rewrite item history |
| Whole-operation open, paused, limited, relocating, closed, or canceled state | Product/domain state | Core capability edits and publishes the state |
| Current stop, pitch, host location, stand, gate, section, zone, or service point | Product/domain state | Unknown remains unknown |
| Current event, market, residency, or host engagement | Product/domain state | Does not imply event integration |
| Ordinary hours and bounded manual service periods | Core capability | Schedule values are product state |
| Planned versus current service context | Product/domain state | Must remain distinct |
| Last-order and service-end information | Product/domain state | Published only when authoritative |
| Screen pairing and identity | Core capability | Screen count may be limited |
| Explicit screen targeting | Core capability | Permission controls authorized targets |
| Preview before publish | Core capability | Required for safe publication |
| Immediate publish to selected targets | Core capability | Publication authority is permission-controlled |
| Per-target publish confirmation | Core capability | Delivery result is product state |
| Screen online, offline, outdated, unknown, failed, or delivered status | Product/domain state | Core capability exposes the state |
| Last successfully delivered version and time | Product/domain state | History retention may be limited |
| Retry failed delivery | Core capability | Must preserve target scope |
| Restore prior successful content | Core capability | Included retention may be limited |
| Manual queue, pickup, collection, lane, and service-window guidance | Core capability | The represented guidance is product state/content |
| Manual disruption, weather-affected, delayed, relocated, or early-close notice | Core capability | Notice content and operating state remain operator-controlled |
| View operation, content, screen, and delivery state | Permission | Core capability exists independently |
| Edit menu and content | Permission | Does not grant publish rights automatically |
| Change availability and operating state | Permission | Separate from content editing where needed |
| Manage location, event, service-period, queue, and pickup guidance | Permission | Scoped by operation and object |
| Pair or manage screens | Permission | Does not create screen entitlement |
| Publish to one or more targets | Permission | Entitlement and limits checked separately |
| Restore a prior version | Permission | Restoration capability remains core |
| Local user and authority administration | Permission | Available subject to organization policy |
| Host-required content authority | Permission | Host relationship and mandatory-content state remain separate |
| Sponsor-content authority | Permission | Sponsor access is not a tier by itself |
| Recurring route and stop scheduling | Tier entitlement | Basic manual location updates remain core |
| Reusable event, market, residency, and service calendars | Tier entitlement | External calendar connection is an add-on |
| Schedule conflict detection and approval | Tier entitlement | Optional coordination outcome |
| Public current-location pages and route calendars | Tier entitlement | Map or notification provider may be an add-on |
| Public multi-unit directory | Tier entitlement | Number of public pages/locations may be limited |
| Advanced promotion scheduling and orchestration | Tier entitlement | Manual promotions remain core |
| Promotion approval and performance comparison | Tier entitlement | External ad/loyalty systems are add-ons |
| Organization templates with local overrides | Tier entitlement | Single-unit content management remains core |
| Safe multi-unit bulk actions | Tier entitlement | Target count may be limited |
| Multi-unit exception monitoring | Tier entitlement | Basic per-screen status remains core |
| Delegated publishing and advanced approval workflows | Tier entitlement | Permission assignments remain separate |
| Advanced analytics, benchmarking, forecasts, and scheduled reports | Tier entitlement | Basic operational visibility remains core |
| AI-assisted drafting, layout suggestions, summaries, or recommendations | Tier entitlement | Usage may be metered; external model service may be an add-on |
| POS synchronization | Independent add-on | Manual menu and price editing remain core |
| Ordering and payment integration | Independent add-on | Manual queue and pickup guidance remain core |
| Inventory, production, or kitchen integration | Independent add-on | Manual sell-out control remains core |
| Venue, host, event, gate, section, or sponsor feed | Independent add-on | Manual event and location context remain core |
| Route, mapping, geocoding, or traffic integration | Independent add-on | Does not imply live tracking by default |
| Weather data integration | Independent add-on | Manual weather notice remains core |
| Queue, wait-time, footfall, sensor, or occupancy data | Independent add-on | Unknown values are not inferred |
| Loyalty, coupon, CRM, messaging, notification, or social integration | Independent add-on | Campaign workflow may be tier-entitled |
| Workforce, staffing, operations, delivery-marketplace, or catering integration | Independent add-on | Separate connector and authority |
| Managed hardware, outdoor equipment, rental, installation, or replacement service | Independent add-on | Basic software pairing remains core |
| Managed cellular connectivity, router, or data plan | Independent add-on | Basic offline awareness remains core |
| Remote diagnostics, proactive monitoring, priority support, or managed content service | Independent add-on | Commercial service, not core product state |
| Number of organizations, operations, units, trucks, trailers, carts, stands, kiosks, or service points | Usage or quantity limit | Counting model requires owner approval |
| Number of screens and players | Usage or quantity limit | Pairing remains core |
| Number of users, roles, approvers, or groups | Usage or quantity limit | Permission model remains separate |
| Number of routes, stops, schedules, events, markets, campaigns, or public pages | Usage or quantity limit | Optional capability access remains separate |
| Number of integrations or connected locations | Usage or quantity limit | Connector availability remains add-on access |
| Transactions, messages, notifications, API calls, refreshes, AI generations, or data volume | Usage or quantity limit | Metering must be explicit |
| Publication, audit, analytics, or external-data retention | Usage or quantity limit | Restoration remains core within included retention |
| Export frequency, report rows, or scheduled deliveries | Usage or quantity limit | Export/report capability classification remains separate |
| Internal development, beta, experiment, regional rollout, or staged release | Internal rollout flag | Never displayed as customer tier or add-on |

## Duplicate and ambiguity resolution

### Manual versus integrated availability

- Manual availability change is a **core capability**.
- Available, unavailable, sold out, and limited are **product/domain states**.
- Inventory/POS automation is an **independent add-on**.
- Advanced automation policy may be a **tier entitlement**.
- Transaction, item, or refresh volume is a **limit**.

### Location versus route management

- Current represented location is **product/domain state**.
- Manual location editing and publishing is a **core capability**.
- Recurring route planning is a **tier entitlement**.
- Maps, routing, traffic, or geocoding connections are **independent add-ons**.
- Stops, schedules, pages, or notifications may have **limits**.

### Event and host behavior

- Current event, host, stand, gate, section, or service period is **product/domain state**.
- Manual event/host communication is a **core capability**.
- Advanced event orchestration and approvals are **tier entitlements**.
- Host, event, sponsor, and venue feeds are **independent add-ons**.
- Who may alter host or sponsor content is a **permission**.

### Screen health and managed operations

- Pairing, targeting, publish confirmation, offline/outdated awareness, retry, and basic restoration are **core capabilities**.
- Current delivery and connectivity values are **product/domain states**.
- Advanced fleet monitoring may be a **tier entitlement**.
- Managed hardware, cellular connectivity, diagnostics, and support are **independent add-ons**.
- Screens, devices, data usage, or support terms are **limits or commercial terms**.

### Analytics and external data

- Basic operational visibility for the current operation and screens is a **core capability**.
- Advanced analytics and benchmarking are **tier entitlements**.
- POS, order, queue, footfall, weather, event, and other source connections are **independent add-ons**.
- Retention, rows, refreshes, exports, and transactions are **limits**.

### AI

- AI assistance is not required for ordinary operation.
- Customer access to AI-assisted product behavior is a **tier entitlement** unless an owner decision makes a specialized service an **independent add-on**.
- Generation credits, data volume, or media storage are **limits**.
- Experimental availability is an **internal rollout flag**.
- Permission and human approval govern whether AI output may be accepted or published.

## Customer-facing availability states

Customer-facing surfaces must distinguish:

- included by plan;
- available to purchase as an add-on;
- included but limited;
- unavailable because of permission;
- disconnected, stale, degraded, or failed integration;
- not configured;
- not supported for the represented region, partner, hardware, or host arrangement;
- internally staged or experimental, which must not be represented as purchasable access.

“No access,” “no permission,” “no data,” “not configured,” “offline,” “outdated,” and “limit reached” are different conditions and require different recovery actions.

## Validation

Every concern introduced by the Food Truck & Concession industry definition, subtype model, terminology, operating characteristics, required capabilities, and optional capabilities now has one primary Track 0 classification. Duplicate manual/integrated concepts are resolved, essential operation remains core, and no product behavior or packaging implementation is authorized.
